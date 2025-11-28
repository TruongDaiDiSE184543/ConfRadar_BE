using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.Payment;
using ConfRadar.Services.DTOs.Transaction;
using ConfRadar.Services.Exceptions;
using ConfRadar.Shared.DTO.Payment;
using ConfRadar.Shared.DTO.QrCode;
using Microsoft.Extensions.Options;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using System.Text.Json;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.Services.Services
{
    public interface IPaymentService
    {
        Task<List<TransactionDetailResponse>> GetOwnTransactionByUserId(string userId);
        Task<List<PaymentMethod>> GetListPaymentMethod();

        Task<GeneralPaymentResultResponse> CreatePaymentForTechConference(CreateTechPaymentRequest request, string userId);
        Task<GeneralPaymentResultResponse> CreatePaymentForAbstract(CreatePaperPaymentRequest request, string userId);
        Task<GeneralPaymentResultResponse> CreatePaymentForResearchAsAttendee(CreateResearchAttendeePaymentRequest request, string userId);

        //Task ProcessCallBackForTechConference(string orderId, decimal amountFromIpn, string transactionCodeFromIpn);
        //Task ProcessCallBackForResearchConferenceAbstractSubmission(string orderId, decimal amountFromIpn, string transactionCodeFromIpn);



        Task VerifyPayOsDataForConference(Webhook data);
        Task VerifyMomoDataForConference(MomoPaymentCallBackResponse data);
        Task VerifyVnPayDataForConference(VnPayResponse data);
        Task CancelPayment(PaymentMethodEnum paymentMethodEnum, string orderCode);
    }
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptions<MomoSettings> _momoSettings;
        private readonly IOptions<PayOsSettings> _payOsSettings;
        private readonly IRedisService _redisService;
        private readonly ITokenService _tokenService;
        private readonly IMomoService _momoService;
        private readonly IPayOsService _payOsService;
        private readonly IVnPayService _vnPayService;
        private readonly IQRCoderService _qRCoderService;
        private readonly ITimeProviderService _timeProviderService;
        public PaymentService(IUnitOfWork unitOfWork, IOptions<MomoSettings> momoSettings, IRedisService redisService, ITokenService tokenService, IMomoService momoService, IPayOsService payOsService, IOptions<PayOsSettings> payOsSettings, IVnPayService vnPayService, IQRCoderService qRCoderService, ITimeProviderService timeProviderService)
        {
            _unitOfWork = unitOfWork;
            _momoSettings = momoSettings;
            _redisService = redisService;
            _tokenService = tokenService;
            _momoService = momoService;
            _payOsSettings = payOsSettings;
            _payOsService = payOsService;
            _vnPayService = vnPayService;
            _qRCoderService = qRCoderService;
            _timeProviderService = timeProviderService;
        }

        public async Task<List<PaymentMethod>> GetListPaymentMethod()
        {
            return await _unitOfWork.PaymentMethodRepository.GetListPaymentMethods();
        }

        public async Task<List<TransactionDetailResponse>> GetOwnTransactionByUserId(string userId)
        {
            var transactions = await _unitOfWork.TransactionRepository.GetOwnTransactionByUserId(userId);
            var transactionDetailResponses = transactions.Select(x => new TransactionDetailResponse()
            {
                TransactionId = x.TransactionId,
                Currency = x.Currency,
                Amount = x.Amount,
                CreatedAt = x.CreatedAt,
                TransactionCode = x.TransactionCode,
                IsRefunded = x.IsRefunded,
                PaymentMethodId = x.PaymentMethodId,
                PaymentMethodName = x.PaymentMethod?.MethodName,
                TicketId = x.TicketId,
            }).ToList();
            return transactionDetailResponses;
        }


        #region create payment for tech conference
        public async Task<GeneralPaymentResultResponse> CreatePaymentForTechConference(CreateTechPaymentRequest request, string userId)
        {

            var paymentMethod = await _unitOfWork.PaymentMethodRepository.GetPaymentMethodById(request.PaymentMethodId);
            if (paymentMethod == null)
            {
                throw new BadRequestException($"Không tìm thấy phuong thức thanh toán nào với mã {request.PaymentMethodId}");
            }

            var conferencePrice = await _unitOfWork.ConferencePriceRepository.GetConferencePriceByIdAsync(request.ConferencePriceId);
            if (conferencePrice == null)
            {
                throw new BadRequestException($"Giá conference với id {request.ConferencePriceId} không tìm thấy");
            }
            if (conferencePrice.Conference?.AvailableSlot <= 0)
            {
                throw new BadRequestException($"{conferencePrice.Conference?.ConferenceName} đã bán hết vé!");
            }
            //check ko cho mua 1 conf
            var paymentConferenceLockKey = ExtensionHelper.GetPaymentConfereceLockKeyResult(userId, conferencePrice.ConferenceId!);
            bool paymentConferenceLockFound = await _redisService.KeyExistsAsync(paymentConferenceLockKey);
            if (paymentConferenceLockFound == true)
            {
                var paymentLockDataFound = await _redisService.GetStringAsync(paymentConferenceLockKey);
                var paymentLockDataHolder = JsonSerializer.Deserialize<PaymentLockKeyDTO>(paymentLockDataFound, new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true,
                });
                var paymentMethodInPaymentLock = await _unitOfWork.PaymentMethodRepository.GetPaymentMethodById(paymentLockDataHolder.PaymentMethodId);
                if (paymentLockDataHolder.PaymentMethodId != request.PaymentMethodId)
                {
                    throw new BadRequestException($"Bạn hiện dang có 1 thanh toán, và chưa thực hiện với cổng thanh toán {paymentMethodInPaymentLock!.MethodName}. Xin vui lòng thanh toán bằng cổng {paymentMethodInPaymentLock!.MethodName}. Ho?c h?y thanh toán, ho?c d?i h?t h?n 90 phút");
                }
                return new GeneralPaymentResultResponse()
                {
                    PaymentCreateSuccess = false,
                    CheckOutUrl = paymentLockDataHolder.OldCheckOutUrl,
                    PaymentMessage = $"Chúng tôi phát hiện bạn dang có 1 giao dịch chưa thực hiện với cổng:{paymentMethodInPaymentLock!.MethodName}. Xin vui lòng thực hiện giao dịch này "
                };

            }










            var listTicketsFound = await _unitOfWork.TicketRepository.GetAttendeeTicketByUserIdAndConferenceId(userId, conferencePrice.ConferenceId!);
            var boughtTicketFound = listTicketsFound.FirstOrDefault(t => t.IsRefunded == false);
            if (boughtTicketFound != null)
            {
                throw new BadRequestException("Bạn đã mua vé cho sự kiện này rồi!");
            }


            var dateNow = await _timeProviderService.GetVietnamDate();
            if (conferencePrice.Conference?.TicketSaleStart > dateNow)
            {
                throw new BadRequestException($"Chưa đến thời hạn mua vé. Thời hạn mua vé nằm trong khoảng từ {conferencePrice.Conference.TicketSaleStart} đến {conferencePrice.Conference.TicketSaleEnd}");
            }
            if (conferencePrice.Conference?.TicketSaleEnd < dateNow)
            {
                throw new BadRequestException("Ðã hết thời hạn mua vé.");
            }
            if (conferencePrice.IsAuthor == true)
            {
                throw new BadRequestException("Vé này chỉ dành cho nguời tham dự.");
            }

            var validPhases = conferencePrice.PricePhases.Where(p => p.StartDate <= dateNow && p.EndDate >= dateNow).OrderBy(p => p.StartDate).ToList();
            if (!validPhases.Any())
            {
                throw new BadRequestException("Hiện tại không có phase hợp lệ để thanh toán");
            }
            var currentPhase = validPhases.FirstOrDefault(p => p.AvailableSlot > 0);
            if (currentPhase == null)
            {
                throw new BadRequestException("Giai đoạn hiện tại đã hết slot");
            }
            //check nhiều nguời mua trong 1 phase
            var paymentPhaseLockPattern = ExtensionHelper.GetPaymentPhaseLockKeyPattern(currentPhase.PricePhaseId!);
            var paymentPhaseLockList = await _redisService.GetKeysByPatternAsync(paymentPhaseLockPattern);
            int paymentPhaseLockCount = paymentPhaseLockList.Count();
            if (paymentPhaseLockCount >= currentPhase.AvailableSlot)
            {
                return new GeneralPaymentResultResponse()
                {
                    PaymentCreateSuccess = false,
                    CheckOutUrl = null,
                    PaymentMessage = $"Hiện tại đang có {paymentPhaseLockCount} khách hàng đang thực hiện giao dịch trong giai đoạn mua vé từ {currentPhase.StartDate} đến {currentPhase.EndDate} tương ứng {currentPhase.AvailableSlot} số vé "
                };

            }

            var discountPercent = currentPhase.ApplyPercent ?? 0;

            var rawPrice = conferencePrice.TicketPrice;
            var discountedPrice = (long)(rawPrice * ((decimal)discountPercent / (decimal)100.0));
            var finalAmount = (long)discountedPrice;
            if (finalAmount <= 10000)
            {
                throw new BadRequestException($"Giá cho vé hiện tại là {finalAmount} không khả dụng cho cổng thanh toán trong hệ thống xin hãy liên hệ ban tổ chức sự kiện");
            }

            var sessionIds = conferencePrice.Conference!.ConferenceSessions.Select(s => s.ConferenceSessionId).ToList();
            var ticketId = Guid.NewGuid().ToString();

            var conferenceLockKey = ExtensionHelper.GetPaymentConfereceLockKeyResult(userId, conferencePrice.ConferenceId!);
            var phaseLockKey = ExtensionHelper.GetPaymentPhaseLockKeyResult(userId, currentPhase.PricePhaseId);
            var transactionData = new TransactionDataHolder
            {
                TicketId = ticketId,
                UserId = userId,
                PaymentMethodId = paymentMethod.PaymentMethodId,
                ConferencePriceId = request.ConferencePriceId,
                PricePhaseId = currentPhase.PricePhaseId,
                ConferenceSessionIds = sessionIds,
                ConferenceId = conferencePrice.ConferenceId!,
                PaymentConferenceLockKey = conferenceLockKey,
                PaymentPhaseLockKey = phaseLockKey,
                Description = null,
                Title = null,
                IsResearchConference = false,
                IsResearchConferenceAuthor = null
            };


            var transacJson = JsonSerializer.Serialize(transactionData);
            var orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            double expireMinute = 90;
            string paymentDescription = "Thanhtoantech";
            string conferenceName = conferencePrice?.Conference?.ConferenceName ?? "";

            //thêm mutiple phuong thức thanh toán:
            string checkOutUrl = string.Empty;
            var lockeyData = new PaymentLockKeyDTO()
            {
                OldCheckOutUrl = checkOutUrl,
                PaymentMethodId = request.PaymentMethodId
            };
            switch (paymentMethod.MethodName)
            {
                case var s when s == PaymentMethodEnum.PayOs.GetDescription():
                    var listPaymentLinkItem = new List<PaymentLinkItem>()
                    {
                        new PaymentLinkItem()
                        {
                        Name = $"Thanh toán vé cho hội nghị: {conferenceName}",
                        Price = finalAmount,
                        Quantity = 1,
                        }
                    };
                    checkOutUrl = await _payOsService.CreatePayOsPayment(orderCode, finalAmount, paymentDescription, expireMinute, listPaymentLinkItem);
                    break;
                case var s when s == PaymentMethodEnum.MoMo.GetDescription():
                    var momoResult = await _momoService.CreateMomoPayment(orderCode.ToString(), finalAmount, paymentDescription);
                    checkOutUrl = momoResult.payUrl;
                    break;
                case var s when s == PaymentMethodEnum.VnPay.GetDescription():
                    var vnPayResult = _vnPayService.CreateVnPayPayment(orderCode, finalAmount, expireMinute);
                    checkOutUrl = vnPayResult;
                    break;
                case var s when s == PaymentMethodEnum.ZaloPay.GetDescription():
                    throw new BadRequestException("Phuong thức thanh toán ZaloPay đang trong trạng thái bảo trì");
                case var s when s == PaymentMethodEnum.Wallet.GetDescription():
                    var userWallet = await _unitOfWork.WalletRepository.GetWalletByUserIdAsync(userId);
                    if (userWallet == null) throw new NotFoundException("Không tìm thấy ví của bạn trong hệ thống");
                    if (userWallet.Balance < finalAmount) throw new BadRequestException($"Số dư trong ví của bạn {userWallet.Balance} không đủ để mua vé với giá {finalAmount}");



                    var lockeyDataWalletJson = JsonSerializer.Serialize(lockeyData);
                    await _redisService.SetStringAsync(conferenceLockKey, lockeyDataWalletJson, TimeSpan.FromMinutes(expireMinute));
                    await _redisService.SetStringAsync(phaseLockKey, "", TimeSpan.FromMinutes(expireMinute));
                    await _redisService.SetStringAsync(orderCode.ToString(), transacJson, TimeSpan.FromMinutes(expireMinute));
                    await ProcessInsertPaymentData(orderCode.ToString(), finalAmount, Guid.NewGuid().ToString(), useWallet: true);

                    break;
                default:
                    throw new BadRequestException("Phuơng thức thanh toán không hợp lệ");
            }
            if (paymentMethod.MethodName == PaymentMethodEnum.Wallet.GetDescription())
            {
                return new GeneralPaymentResultResponse()
                {
                    PaymentCreateSuccess = true,
                    PaymentMessage = "Đã thanh toán thành công bằng ví",
                    CheckOutUrl = null,
                };
            }
            lockeyData.OldCheckOutUrl = checkOutUrl;
            var lockeyDataJson = JsonSerializer.Serialize(lockeyData);
            await _redisService.SetStringAsync(conferenceLockKey, lockeyDataJson, TimeSpan.FromMinutes(expireMinute));
            await _redisService.SetStringAsync(phaseLockKey, "", TimeSpan.FromMinutes(expireMinute));
            await _redisService.SetStringAsync(orderCode.ToString(), transacJson, TimeSpan.FromMinutes(expireMinute));
            return new GeneralPaymentResultResponse()
            {
                PaymentCreateSuccess = true,
                PaymentMessage = "Tạo liên kết thanh toán thành công. Vui lòng hoàn tất giao dịch tại cổng thanh toán.",
                CheckOutUrl = checkOutUrl,
            };



        }
        #endregion
        #region create payment for abstract
        public async Task<GeneralPaymentResultResponse> CreatePaymentForAbstract(CreatePaperPaymentRequest request, string userId)
        {
            var dateNow = await _timeProviderService.GetVietnamDate();
            var paymentMethod = await _unitOfWork.PaymentMethodRepository.GetPaymentMethodById(request.PaymentMethodId);
            if (paymentMethod == null)
            {
                throw new BadRequestException($"Không tìm thấy phuong thúc thanh toán nào với mã {request.PaymentMethodId}");
            }
            var conferencePrice = await _unitOfWork.ConferencePriceRepository.GetConferencePriceByIdAsync(request.ConferencePriceId);
            if (conferencePrice == null)
            {
                throw new BadRequestException($"Giá hội nghị với id {request.ConferencePriceId} không tìm thấy");
            }
            if (conferencePrice.Conference?.AvailableSlot <= 0)
            {
                throw new BadRequestException($"{conferencePrice.Conference?.ConferenceName} đã bán hết vé!");
            }
            if (conferencePrice.Conference!.IsResearchConference == false)
            {
                throw new BadRequestException($"Bạn chỉ có thể nộp abstract cho research conference");
            }
            if (conferencePrice.IsAuthor == false)
            {
                throw new BadRequestException($"Giá vé hiện tại không dành cho tác giả, xin hãy chọn mức giá khác");
            }
            if (conferencePrice.Conference.IsInternalHosted == false)
            {
                throw new BadRequestException($"Bạn chỉ có thể nộp abstract cho research conference tổ chức bởi confradar");
            }
            var paperCount = await _unitOfWork.PaperRepository.GetPaperCountByConference(conferencePrice.ConferenceId);
            var researchConfDetail = conferencePrice.Conference.ResearchConferenceDetail;
            if (researchConfDetail == null) throw new NotFoundException("Không tìm thấy research conference");
            if (paperCount >= researchConfDetail.NumberPaperAccept)
            {
                throw new BadRequestException($"Hiện đang có {paperCount} trên tổng số bài báo quy định cho hội nghị research {researchConfDetail.NumberPaperAccept}");
            }



            var paymentConferenceLockKey = ExtensionHelper.GetPaymentConfereceLockKeyResult(userId, conferencePrice.ConferenceId!);
            bool paymentConferenceLockFound = await _redisService.KeyExistsAsync(paymentConferenceLockKey);
            if (paymentConferenceLockFound == true)
            {
                var paymentLockDataFound = await _redisService.GetStringAsync(paymentConferenceLockKey);
                var paymentLockDataHolder = JsonSerializer.Deserialize<PaymentLockKeyDTO>(paymentLockDataFound, new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true,
                });
                var paymentMethodInPaymentLock = await _unitOfWork.PaymentMethodRepository.GetPaymentMethodById(paymentLockDataHolder.PaymentMethodId);
                if (paymentLockDataHolder.PaymentMethodId != request.PaymentMethodId)
                {
                    throw new BadRequestException($"Bạn hiện đang có 1 thanh toán, và chưa được thực hiện với cổng thanh toán {paymentMethodInPaymentLock!.MethodName}. Xin vui lòng thanh toán bằng cổng {paymentMethodInPaymentLock!.MethodName}. Hãy thanh toán, hoặc đợi hết hạn 90 phút");

                }
                return new GeneralPaymentResultResponse()
                {
                    PaymentCreateSuccess = false,
                    CheckOutUrl = paymentLockDataHolder.OldCheckOutUrl,
                    PaymentMessage = $"Chúng tôi phát hiện bạn dang có 1 giao dịch chưa được thực hiện với cổng:{paymentMethodInPaymentLock!.MethodName}. Xin vui lòng thực hiện giao dịch này "
                };

            }

            var researchConferencePhases = conferencePrice.Conference?.ResearchConferencePhases;
            if (researchConferencePhases == null || !researchConferencePhases.Any())
            {
                throw new BadRequestException($"Không tìm thấy các giai đoạn trong hội nghị nghiên cứu này");
            }
            var activeResearchConferencePhase = researchConferencePhases.FirstOrDefault(rcp => rcp.IsActive == true);
            if (activeResearchConferencePhase == null)
            {
                throw new BadRequestException($"Giai đoạn hội nghị nghiên cứu không khả dụng. Xin vui lòng liên hệ ban tổ chức");
            }
            if (activeResearchConferencePhase.RegistrationStartDate > dateNow)
            {
                throw new BadRequestException($"Chua đến thời hạn mua vé. Thời hạn nằm trong khoảng từ {activeResearchConferencePhase.RegistrationStartDate} đến {activeResearchConferencePhase.RegistrationEndDate}");
            }
            if (activeResearchConferencePhase.RegistrationEndDate < dateNow)
            {
                throw new BadRequestException("Ðã hết thời hạn mua vé.");
            }

            var listAttendeeTicketsFound = await _unitOfWork.TicketRepository.GetAttendeeTicketByUserIdAndConferenceId(userId, conferencePrice.ConferenceId);
            var validAttendeeTicketFound = listAttendeeTicketsFound.FirstOrDefault(t => t.IsRefunded == false);
            if (validAttendeeTicketFound != null)
            {
                throw new BadRequestException("Chúng tôi phát hiện bạn đang có 1 vé là người tham dự hội nghị");

            }
            var listAuthorTicketsFound = await _unitOfWork.TicketRepository.GetAuthorTicketByUserIdAndConferenceId(userId, conferencePrice.ConferenceId);
            var validAuthorTicktFound = listAuthorTicketsFound.FirstOrDefault(t => t.IsRefunded == false);
            if (validAuthorTicktFound != null)
            {
                throw new BadRequestException("Bạn chỉ có thể mua vé 1 lần cho research paper");
            }



            var reviewerContractFound = await _unitOfWork.ReviewerContractRepository.GetContractByUserAndConferenceAsync(userId, conferencePrice.ConferenceId);
            if (reviewerContractFound != null)
            {
                if (reviewerContractFound.IsActive == true)
                {
                    throw new BadRequestException($"Bạn đang có hợp đồng với sự kiện này nên không thể thực hiện thanh toán");
                }
            }
            var internalReviewRole = await _unitOfWork.RoleRepository.GetRoleByRoleName(SystemRoleEnum.LocalReviewer.GetDescription());
            if (internalReviewRole == null)
            {
                throw new NotFoundException($"Không tìm thấy role trong hệ thống");
            }
            var userRole = await _unitOfWork.UserRoleRepository.GetUserRoleByUserAndRole(userId, internalReviewRole.RoleId);
            if (userRole != null)
            {
                throw new BadRequestException($"Bạn không thể mua vé này vì bạn là reviewer trong hệ thống");
            }
            decimal applyPercent = 0;

            //if (conferencePrice.Conference?.TicketSaleStart > dateNow)
            //{
            //    throw new BadRequestException($"Chua d?n th?i h?n mua vé. Th?i h?n mua vé n?m trong kho?ng t? {conferencePrice.Conference.TicketSaleStart} d?n {conferencePrice.Conference.TicketSaleEnd}");
            //}
            //if (conferencePrice.Conference?.TicketSaleEnd < dateNow)
            //{
            //    throw new BadRequestException("Ðã h?t th?i h?n mua vé.");
            //}

            var validPhases = conferencePrice.PricePhases
            .Where(p => p.StartDate <= dateNow && p.EndDate >= dateNow)
            .OrderBy(p => p.StartDate)
            .ToList();

            if (!validPhases.Any())
            {
                throw new BadRequestException("Hiện tại không có phase hợp lệ để nộp abstract");
            }
            var currentPhase = validPhases.FirstOrDefault(p => p.AvailableSlot > 0);
            if (currentPhase == null)
            {
                throw new BadRequestException("Giai đoạn hiện tại đã hết slot");
            }
            //check nhiều nguời mua trong 1 phase
            var paymentPhaseLockPattern = ExtensionHelper.GetPaymentPhaseLockKeyPattern(currentPhase.PricePhaseId!);
            var paymentPhaseLockList = await _redisService.GetKeysByPatternAsync(paymentPhaseLockPattern);
            int paymentPhaseLockCount = paymentPhaseLockList.Count();
            if (paymentPhaseLockCount >= currentPhase.AvailableSlot)
            {
                return new GeneralPaymentResultResponse()
                {
                    PaymentCreateSuccess = false,
                    CheckOutUrl = null,
                    PaymentMessage = $"Hiện tại dang có {paymentPhaseLockCount} khách hàng đang thực hiện giao dịch trong giai đoạn hiện tại mua vé từ  {currentPhase.StartDate} đến {currentPhase.EndDate} tương ứng với {currentPhase.AvailableSlot} số vé còn lại "
                };

            }



            var sessionIdsList = conferencePrice.Conference.ConferenceSessions.Select(cs => cs.ConferenceSessionId).ToList();
            applyPercent = currentPhase.ApplyPercent ?? 0;
            long finalPrice = 0;
            if (applyPercent < 0)
            {
                throw new BadRequestException($"% giảm giá cho vé hiện tại là {applyPercent} không khả dụng xin hãy liên hệ ban tổ chức sự kiện");
            }
            finalPrice = (long)(conferencePrice.TicketPrice * ((decimal)applyPercent / (decimal)100.0));
            if (finalPrice <= 10000)
            {
                throw new BadRequestException($"Giá cho vé hiện tại là {finalPrice} không khả dụng cho cổng thanh toán trong hệ thống xin hãy liên hệ ban tổ chức sự kiện");
            }
            var paperWaitListFound = await _unitOfWork.PaperWaitListRepository.GetPaperWaitListByUserIdAndConferenceIdAsync(userId, conferencePrice.ConferenceId);

            if (paperWaitListFound != null && conferencePrice.AvailableSlot > 0)
            {
                await _unitOfWork.PaperWaitListRepository.DeletePaperWaitListAsync(paperWaitListFound);
            }
            var ticketId = Guid.NewGuid().ToString();
            var lockKeyConference = ExtensionHelper.GetPaymentConfereceLockKeyResult(userId, conferencePrice!.ConferenceId);
            var lockKeyPhase = ExtensionHelper.GetPaymentPhaseLockKeyResult(userId, currentPhase.PricePhaseId);
            var transactionData = new TransactionDataHolder()
            {
                TicketId = ticketId,
                UserId = userId,
                PaymentMethodId = request.PaymentMethodId,
                ConferencePriceId = request.ConferencePriceId,
                ConferenceSessionIds = sessionIdsList,
                ConferenceId = conferencePrice.ConferenceId,
                ResearchConferencePhaseId = activeResearchConferencePhase.ResearchConferencePhaseId,
                PaymentConferenceLockKey = lockKeyConference,
                PaymentPhaseLockKey = lockKeyPhase,
                PricePhaseId = currentPhase.PricePhaseId,
                Title = request.Title,
                Description = request.Description,
                IsResearchConference = true,
                IsResearchConferenceAuthor = true
            };
            var transacJson = JsonSerializer.Serialize(transactionData);

            //logic đa cổng
            var orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            double expireMinute = 90;
            string paymentDescription = "Thanhtoanresearch";
            string conferenceName = conferencePrice?.Conference?.ConferenceName ?? "";



            //thêm mutiple phuong thức thanh toán:
            string checkOutUrl = string.Empty;
            var lockeyData = new PaymentLockKeyDTO()
            {
                OldCheckOutUrl = checkOutUrl,
                PaymentMethodId = request.PaymentMethodId
            };
            switch (paymentMethod.MethodName)
            {
                case var s when s == PaymentMethodEnum.PayOs.GetDescription():
                    var listPaymentLinkItem = new List<PaymentLinkItem>()
                    {
                        new PaymentLinkItem()
                        {
                            Name = $"Thanh toán vé cho h?i ngh?: {conferenceName}",
                            Price = finalPrice,
                            Quantity = 1,
                        }
                    };
                    checkOutUrl = await _payOsService.CreatePayOsPayment(orderCode, finalPrice, paymentDescription, expireMinute, listPaymentLinkItem);
                    break;
                case var s when s == PaymentMethodEnum.MoMo.GetDescription():
                    var momoResult = await _momoService.CreateMomoPayment(orderCode.ToString(), finalPrice, paymentDescription);
                    checkOutUrl = momoResult.payUrl;
                    break;
                case var s when s == PaymentMethodEnum.VnPay.GetDescription():
                    var vnPayResult = _vnPayService.CreateVnPayPayment(orderCode, finalPrice, expireMinute);
                    checkOutUrl = vnPayResult;
                    break;
                case var s when s == PaymentMethodEnum.ZaloPay.GetDescription():
                    throw new BadRequestException("Phuong thức thanh toán ZaloPay đang trong trong thái bảo trì ");
                case var s when s == PaymentMethodEnum.Wallet.GetDescription():
                    var userWallet = await _unitOfWork.WalletRepository.GetWalletByUserIdAsync(userId);
                    if (userWallet == null) throw new NotFoundException("Không tìm thấy ví của bạn trong hệ thống");
                    if (userWallet.Balance < finalPrice) throw new BadRequestException($"Số dư trong ví của bạn {userWallet.Balance} không đủ để mua vé với giá {finalPrice}");



                    var lockeyDataWalletJson = JsonSerializer.Serialize(lockeyData);
                    await _redisService.SetStringAsync(lockKeyConference, lockeyDataWalletJson, TimeSpan.FromMinutes(expireMinute));
                    await _redisService.SetStringAsync(lockKeyPhase, "", TimeSpan.FromMinutes(expireMinute));
                    await _redisService.SetStringAsync(orderCode.ToString(), transacJson, TimeSpan.FromMinutes(expireMinute));
                    await ProcessInsertPaymentData(orderCode.ToString(), finalPrice, Guid.NewGuid().ToString(), useWallet: true);

                    break;
                default:
                    throw new BadRequestException("Phuong thức thanh toán không hợp lệ");
            }
            if (paymentMethod.MethodName == PaymentMethodEnum.Wallet.GetDescription())
            {
                return new GeneralPaymentResultResponse()
                {
                    PaymentCreateSuccess = true,
                    PaymentMessage = "Đã thanh toán thành công bằng ví",
                    CheckOutUrl = null,
                };
            }
            lockeyData.OldCheckOutUrl = checkOutUrl;
            var lockeyDataJson = JsonSerializer.Serialize(lockeyData);
            await _redisService.SetStringAsync(lockKeyConference, lockeyDataJson, TimeSpan.FromMinutes(expireMinute));
            await _redisService.SetStringAsync(lockKeyPhase, "", TimeSpan.FromMinutes(expireMinute));
            await _redisService.SetStringAsync(orderCode.ToString(), transacJson, TimeSpan.FromMinutes(expireMinute));
            return new GeneralPaymentResultResponse()
            {
                PaymentCreateSuccess = true,
                PaymentMessage = "Tạo liên kết thanh toán thành công. Vui lòng hoàn tất giao dịch tại cổng thanh toán.",
                CheckOutUrl = checkOutUrl,
            };


        }
        #endregion

        #region create payment for research attendee
        public async Task<GeneralPaymentResultResponse> CreatePaymentForResearchAsAttendee(CreateResearchAttendeePaymentRequest request, string userId)
        {
            var paymentMethod = await _unitOfWork.PaymentMethodRepository.GetPaymentMethodById(request.PaymentMethodId);
            if (paymentMethod == null)
            {
                throw new BadRequestException($"Không tìm thấy phương thức thanh toán nào với mã {request.PaymentMethodId}");
            }
            var conferencePrice = await _unitOfWork.ConferencePriceRepository.GetConferencePriceByIdAsync(request.ConferencePriceId);
            if (conferencePrice == null)
            {
                throw new BadRequestException($"Giá hội nghị với id {request.ConferencePriceId} không tìm thấy");
            }
            if (conferencePrice.Conference?.AvailableSlot <= 0)
            {
                throw new BadRequestException($"{conferencePrice.Conference?.ConferenceName} đã bán hết vé!");
            }
            if (conferencePrice.Conference!.IsResearchConference == false)
            {
                throw new BadRequestException($"Bạn chỉ có thể mua vé cho hội nghị nghiên cứu");
            }
            if (conferencePrice.IsAuthor == true)
            {
                throw new BadRequestException($"Giá vé hiện tại chỉ dành cho nguời dự thính trong hội nghị");
            }
            if (conferencePrice.Conference!.ResearchConferenceDetail?.AllowListener != true)
            {
                throw new BadRequestException($"Hội nghị không cho phép mua vé dự thính");
            }
            if (conferencePrice.Conference.IsInternalHosted == false)
            {
                throw new BadRequestException($"Bạn chỉ có thể tham gia hội nghị tổ chức bởi confradar");
            }
            var paymentConferenceLockKey = ExtensionHelper.GetPaymentConfereceLockKeyResult(userId, conferencePrice.ConferenceId!);
            bool paymentConferenceLockFound = await _redisService.KeyExistsAsync(paymentConferenceLockKey);
            if (paymentConferenceLockFound == true)
            {
                var paymentLockDataFound = await _redisService.GetStringAsync(paymentConferenceLockKey);
                var paymentLockDataHolder = JsonSerializer.Deserialize<PaymentLockKeyDTO>(paymentLockDataFound, new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true,
                });
                var paymentMethodInPaymentLock = await _unitOfWork.PaymentMethodRepository.GetPaymentMethodById(paymentLockDataHolder.PaymentMethodId);
                if (paymentLockDataHolder.PaymentMethodId != request.PaymentMethodId)
                {
                    throw new BadRequestException($"Bạn hiện dang có 1 thanh toán, và chưa thực hiện với cổng thanh toán {paymentMethodInPaymentLock!.MethodName}. Xin vui lòng thanh toán bằng {paymentMethodInPaymentLock!.MethodName}. Hãy thanh toán, hoặc đợi hết 90 phút");

                }
                return new GeneralPaymentResultResponse()
                {
                    PaymentCreateSuccess = false,
                    CheckOutUrl = paymentLockDataHolder.OldCheckOutUrl,
                    PaymentMessage = $"Chúng tôi phát hiện bạn dang có 1 giao dịch chưa thực hiện với cổng:{paymentMethodInPaymentLock!.MethodName}. Xin vui lòng thực hiện giao dịch này "
                };

            }

            var researchConferencePhases = conferencePrice.Conference?.ResearchConferencePhases;
            if (researchConferencePhases == null || !researchConferencePhases.Any())
            {
                throw new BadRequestException($"Không tìm thấy các giai đoạn trong hội nghị nghiên cứu này");
            }






            var listAttendeeTicketsFound = await _unitOfWork.TicketRepository.GetAttendeeTicketByUserIdAndConferenceId(userId, conferencePrice.ConferenceId);
            var validAttendeeTicketFound = listAttendeeTicketsFound.FirstOrDefault(t => t.IsRefunded == false);
            if (validAttendeeTicketFound != null)
            {
                throw new BadRequestException("Bạn chỉ có thể mua vé 1 lần cho sự kiện research.");

            }
            var listAuthorTicketsFound = await _unitOfWork.TicketRepository.GetAuthorTicketByUserIdAndConferenceId(userId, conferencePrice.ConferenceId);
            var validAuthorTicktFound = listAuthorTicketsFound.FirstOrDefault(t => t.IsRefunded == false);
            if (validAuthorTicktFound != null)
            {
                throw new BadRequestException("Chúng tôi phát hiện bạn đã có 1 vé là author cho sự kiện research.");
            }



            decimal applyPercent = 0;
            var dateNow = await _timeProviderService.GetVietnamDate();
            if (conferencePrice.Conference?.TicketSaleStart > dateNow)
            {
                throw new BadRequestException($"Chưa đến thời hạn mua vé. Thời hạn mua vé nằm trong khoảng từ {conferencePrice.Conference.TicketSaleStart} đến {conferencePrice.Conference.TicketSaleEnd}");
            }
            if (conferencePrice.Conference?.TicketSaleEnd < dateNow)
            {
                throw new BadRequestException("Ðã hết thời hạn mua vé.");
            }

            var validPhases = conferencePrice.PricePhases
            .Where(p => p.StartDate <= dateNow && p.EndDate >= dateNow)
            .OrderBy(p => p.StartDate)
            .ToList();

            if (!validPhases.Any())
            {
                throw new BadRequestException("Hiện tại không có phase nào hợp lệ");
            }
            var currentPhase = validPhases.FirstOrDefault(p => p.AvailableSlot > 0);
            if (currentPhase == null)
            {
                throw new BadRequestException("Giai đoạn hiện tại đã hết slot");
            }
            //check nhiều nguời mua trong 1 phase
            var paymentPhaseLockPattern = ExtensionHelper.GetPaymentPhaseLockKeyPattern(currentPhase.PricePhaseId!);
            var paymentPhaseLockList = await _redisService.GetKeysByPatternAsync(paymentPhaseLockPattern);
            int paymentPhaseLockCount = paymentPhaseLockList.Count();
            if (paymentPhaseLockCount >= currentPhase.AvailableSlot)
            {
                return new GeneralPaymentResultResponse()
                {
                    PaymentCreateSuccess = false,
                    CheckOutUrl = null,
                    PaymentMessage = $"Hiện tại đang có {paymentPhaseLockCount} khách hàng đang thực hiện giao dịch trong giai đoạn hiện tại mua vé từ {currentPhase.StartDate} đến {currentPhase.EndDate} tương ứng với {currentPhase.AvailableSlot} số vé "
                };

            }



            var sessionIdsList = conferencePrice.Conference.ConferenceSessions.Select(cs => cs.ConferenceSessionId).ToList();
            applyPercent = currentPhase.ApplyPercent ?? 0;
            long finalPrice = 0;
            if (applyPercent < 0)
            {
                throw new BadRequestException($"% giảm giá cho vé hiện tại là {applyPercent} không khả dụng ");
            }
            finalPrice = (long)(conferencePrice.TicketPrice * ((decimal)applyPercent / (decimal)100.0));
            if (finalPrice <= 10000)
            {
                throw new BadRequestException($"Giá cho vé hiện tại là {finalPrice} không khả dụng cho cổng thanh toán trong hệ thống, xin vui lòng liên hệ ban tổ chức sự kiện");
            }
            var paperWaitListFound = await _unitOfWork.PaperWaitListRepository.GetPaperWaitListByUserIdAndConferenceIdAsync(userId, conferencePrice.ConferenceId);

            if (paperWaitListFound != null && conferencePrice.AvailableSlot > 0)
            {
                await _unitOfWork.PaperWaitListRepository.DeletePaperWaitListAsync(paperWaitListFound);
            }
            var ticketId = Guid.NewGuid().ToString();
            var lockKeyConference = ExtensionHelper.GetPaymentConfereceLockKeyResult(userId, conferencePrice!.ConferenceId);
            var lockKeyPhase = ExtensionHelper.GetPaymentPhaseLockKeyResult(userId, currentPhase.PricePhaseId);
            var transactionData = new TransactionDataHolder()
            {
                TicketId = ticketId,
                UserId = userId,
                PaymentMethodId = request.PaymentMethodId,
                ConferencePriceId = request.ConferencePriceId,
                ConferenceSessionIds = sessionIdsList,
                ConferenceId = conferencePrice.ConferenceId,
                PaymentConferenceLockKey = lockKeyConference,
                PaymentPhaseLockKey = lockKeyPhase,
                PricePhaseId = currentPhase.PricePhaseId,
                IsResearchConference = true,
                IsResearchConferenceAuthor = false,
            };
            var transacJson = JsonSerializer.Serialize(transactionData);

            //logic da c?ng
            var orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            double expireMinute = 90;
            string paymentDescription = "Mua vé research";
            string conferenceName = conferencePrice?.Conference?.ConferenceName ?? "";



            //thêm mutiple phuong thức thanh toán:
            string checkOutUrl = string.Empty;
            var lockeyData = new PaymentLockKeyDTO()
            {
                OldCheckOutUrl = checkOutUrl,
                PaymentMethodId = request.PaymentMethodId
            };
            switch (paymentMethod.MethodName)
            {
                case var s when s == PaymentMethodEnum.PayOs.GetDescription():
                    var listPaymentLinkItem = new List<PaymentLinkItem>()
                    {
                        new PaymentLinkItem()
                        {
                            Name = $"Thanh toán vé cho hội nghị: {conferenceName}",
                            Price = finalPrice,
                            Quantity = 1,
                        }
                    };
                    checkOutUrl = await _payOsService.CreatePayOsPayment(orderCode, finalPrice, paymentDescription, expireMinute, listPaymentLinkItem);
                    break;
                case var s when s == PaymentMethodEnum.MoMo.GetDescription():
                    var momoResult = await _momoService.CreateMomoPayment(orderCode.ToString(), finalPrice, paymentDescription);
                    checkOutUrl = momoResult.payUrl;
                    break;
                case var s when s == PaymentMethodEnum.VnPay.GetDescription():
                    var vnPayResult = _vnPayService.CreateVnPayPayment(orderCode, finalPrice, expireMinute);
                    checkOutUrl = vnPayResult;
                    break;
                case var s when s == PaymentMethodEnum.ZaloPay.GetDescription():
                    throw new BadRequestException("Phuong thức thanh toán ZaloPay đang bảo trì");
                case var s when s == PaymentMethodEnum.Wallet.GetDescription():
                    var userWallet = await _unitOfWork.WalletRepository.GetWalletByUserIdAsync(userId);
                    if (userWallet == null) throw new NotFoundException("Không tìm thấy ví của bạn trong hệ thống");
                    if (userWallet.Balance < finalPrice) throw new BadRequestException($"Số dư trong ví của bạn {userWallet.Balance} không đủ để mua vé với giá {finalPrice}");



                    var lockeyDataWalletJson = JsonSerializer.Serialize(lockeyData);
                    await _redisService.SetStringAsync(lockKeyConference, lockeyDataWalletJson, TimeSpan.FromMinutes(expireMinute));
                    await _redisService.SetStringAsync(lockKeyPhase, "", TimeSpan.FromMinutes(expireMinute));
                    await _redisService.SetStringAsync(orderCode.ToString(), transacJson, TimeSpan.FromMinutes(expireMinute));
                    await ProcessInsertPaymentData(orderCode.ToString(), finalPrice, Guid.NewGuid().ToString(), useWallet: true);

                    break;
                default:
                    throw new BadRequestException("Phương thức thanh toán không hợp lệ");
            }
            if (paymentMethod.MethodName == PaymentMethodEnum.Wallet.GetDescription())
            {
                return new GeneralPaymentResultResponse()
                {
                    PaymentCreateSuccess = true,
                    PaymentMessage = "Đã thanh toán thành công bằng ví",
                    CheckOutUrl = null,
                };
            }
            lockeyData.OldCheckOutUrl = checkOutUrl;
            var lockeyDataJson = JsonSerializer.Serialize(lockeyData);
            await _redisService.SetStringAsync(lockKeyConference, lockeyDataJson, TimeSpan.FromMinutes(expireMinute));
            await _redisService.SetStringAsync(lockKeyPhase, "", TimeSpan.FromMinutes(expireMinute));
            await _redisService.SetStringAsync(orderCode.ToString(), transacJson, TimeSpan.FromMinutes(expireMinute));
            return new GeneralPaymentResultResponse()
            {
                PaymentCreateSuccess = true,
                PaymentMessage = "Tạo liên kết thanh toán thành công. Vui lòng hoàn tất giao dịch tại cổng thanh toán.",
                CheckOutUrl = checkOutUrl,
            };
        }
        #endregion


        #region process insert data

        public async Task ProcessCallBackForTechConference(string orderId, decimal amountFromIpn, string transactionCodeFromIpn, bool useWallet)
        {
            //var transacKey = await _redisService.KeyExistsAsync(orderId);
            //if (!transacKey)
            //{
            //    throw new BadRequestException("data is not valid");
            //}
            var transac = await _redisService.GetStringAsync(orderId);
            var transacDataHolder = JsonSerializer.Deserialize<TransactionDataHolder>(transac, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            });
            var dateNow = await _timeProviderService.GetVietnamDate();
            var timeNow = await _timeProviderService.GetVietnamTime();
            var checkInStatus = await _unitOfWork.CheckInStatusRepository.GetCheckInStatusByNameAsync(CheckInStatusEnum.Pending.GetDescription());
            var ticketObj = new Ticket()
            {
                TicketId = transacDataHolder.TicketId,
                RegisteredDate = dateNow,
                IsRefunded = false,
                ActualPrice = amountFromIpn,
                UserId = transacDataHolder.UserId,
                PricePhaseId = transacDataHolder.PricePhaseId,
                Transactions = new List<Transaction>(),
                UserCheckIns = new List<UserCheckIn>()
            };
            var transaction = new Transaction()
            {
                TransactionId = Guid.NewGuid().ToString(),
                UserId = transacDataHolder.UserId,
                Currency = "VND",
                Amount = amountFromIpn,
                CreatedAt = timeNow,
                TransactionCode = transactionCodeFromIpn,
                IsRefunded = false,
                PaymentMethodId = transacDataHolder.PaymentMethodId,
                TicketId = transacDataHolder.TicketId
            };
            ticketObj.Transactions.Add(transaction);
            foreach (var sessionId in transacDataHolder.ConferenceSessionIds)
            {
                var userCheckInObj = new UserCheckIn()
                {
                    UserCheckinId = Guid.NewGuid().ToString(),
                    IsPresenter = false,
                    CheckinStatusId = checkInStatus.CheckinStatusId,
                    CheckInTime = null,
                    UserId = transacDataHolder.UserId,
                    TicketId = transacDataHolder.TicketId,
                    ConferenceSessionId = sessionId,

                };
                var qrData = new QrDataPayload()
                {
                    userCheckinId = userCheckInObj.UserCheckinId,
                    userId = transacDataHolder.UserId,
                    ticketId = transacDataHolder.TicketId,
                    conferenceSessionId = sessionId,
                    createAt = timeNow,

                };
                var finalQrData = _qRCoderService.CreateQrDataPayload(qrData);
                var qrUrl = await _qRCoderService.GenerateQrCode(finalQrData);
                userCheckInObj.QrUrl = qrUrl;
                ticketObj.UserCheckIns.Add(userCheckInObj);
            }
            var pricePhase = await _unitOfWork.PricePhaseRepository.GetPricePhaseByPricePhaseId(transacDataHolder.PricePhaseId);
            if (pricePhase == null)
            {
                throw new BadRequestException("Không tìm thấy phase tương ứng.");
            }
            if (pricePhase.AvailableSlot <= 0)
            {
                throw new BadRequestException("Giai đoạn hiện tại đã hết slot.");
            }
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                pricePhase!.AvailableSlot = pricePhase.AvailableSlot - 1;
                pricePhase!.ConferencePrice!.AvailableSlot = pricePhase!.ConferencePrice!.AvailableSlot - 1;
                pricePhase!.ConferencePrice!.Conference!.AvailableSlot = pricePhase!.ConferencePrice!.Conference!.AvailableSlot - 1;

                if (useWallet == true)
                {
                    var userWallet = await _unitOfWork.WalletRepository.GetWalletByUserIdAsync(transacDataHolder.UserId);
                    if (userWallet == null)
                    {
                        throw new NotFoundException($"Không tìm thấy ví cho user {transacDataHolder.UserId}");
                    }
                    userWallet.Balance = userWallet.Balance - amountFromIpn;
                    var walletTransactionObj = new WalletTransaction()
                    {
                        WalletTransactionId = Guid.NewGuid().ToString(),
                        WalletId = userWallet.WalletId,
                        Amount = -amountFromIpn,
                        TransactionType = WalletTransactionTypeEnum.Purchase.GetDescription(),
                        Description = $"Bạn đã mua đơn hàng #{transacDataHolder.TicketId} thành công vào lúc {timeNow}. Hãy check thông tin vé đã mua",
                        CreatedAt = timeNow,
                    };
                    await _unitOfWork.WalletRepository.UpdateWalletAsync(userWallet);
                    await _unitOfWork.WalletTransactionRepository.CreateWalletTransactionAsync(walletTransactionObj);
                }



                await _unitOfWork.PricePhaseRepository.UpdatePricePhaseAsync(pricePhase);
                await _unitOfWork.TicketRepository.CreateTicketAsync(ticketObj);
                await _unitOfWork.CommitAsync();
                await _redisService.DeleteKeyAsync(orderId);
                await _redisService.DeleteKeyAsync(transacDataHolder.PaymentConferenceLockKey);
                await _redisService.DeleteKeyAsync(transacDataHolder.PaymentPhaseLockKey);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }


        }
        public async Task ProcessCallBackForResearchConferenceAbstractSubmission(string orderId, decimal amountFromIpn, string transactionCodeFromIpn, bool useWallet)
        {

            //var transacKey = await _redisService.KeyExistsAsync(orderId);
            //if (!transacKey)
            //{
            //    throw new NotFoundException("D? li?u không tìm th?y");
            //}
            var dateNow = await _timeProviderService.GetVietnamDate();
            var timeNow = await _timeProviderService.GetVietnamTime();
            var transac = await _redisService.GetStringAsync(orderId);
            var transacDataHolder = JsonSerializer.Deserialize<TransactionDataHolder>(transac, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            });

            var checkInStatus = await _unitOfWork.CheckInStatusRepository.GetCheckInStatusByNameAsync(CheckInStatusEnum.Pending.GetDescription());
            var globalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            var currentPaperPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.Abstract.GetDescription());
            if (checkInStatus == null || globalStatus == null || currentPaperPhase == null)
            {
                throw new NotFoundException($"Lỗi không tìm thấy trạng thái");
            }
            var ticketObj = new Ticket()
            {
                TicketId = transacDataHolder.TicketId,
                RegisteredDate = dateNow,
                IsRefunded = false,
                ActualPrice = amountFromIpn,
                UserId = transacDataHolder.UserId,
                PricePhaseId = transacDataHolder.PricePhaseId,
                Transactions = new List<Transaction>(),
                UserCheckIns = new List<UserCheckIn>()
            };
            var transaction = new Transaction()
            {
                TransactionId = Guid.NewGuid().ToString(),
                UserId = transacDataHolder.UserId,
                Currency = "VND",
                Amount = amountFromIpn,
                CreatedAt = timeNow,
                TransactionCode = transactionCodeFromIpn,
                IsRefunded = false,
                PaymentMethodId = transacDataHolder.PaymentMethodId,
                TicketId = transacDataHolder.TicketId
            };
            ticketObj.Transactions.Add(transaction);
            foreach (var sessionId in transacDataHolder.ConferenceSessionIds)
            {
                var userCheckInObj = new UserCheckIn()
                {
                    UserCheckinId = Guid.NewGuid().ToString(),
                    IsPresenter = false,
                    CheckinStatusId = checkInStatus.CheckinStatusId,
                    CheckInTime = null,
                    UserId = transacDataHolder.UserId,
                    TicketId = transacDataHolder.TicketId,
                    ConferenceSessionId = sessionId
                };
                var qrData = new QrDataPayload()
                {
                    userCheckinId = userCheckInObj.UserCheckinId,
                    userId = transacDataHolder.UserId,
                    ticketId = transacDataHolder.TicketId,
                    conferenceSessionId = sessionId,
                    createAt = timeNow,

                };
                var finalQrData = _qRCoderService.CreateQrDataPayload(qrData);
                var qrUrl = await _qRCoderService.GenerateQrCode(finalQrData);
                userCheckInObj.QrUrl = qrUrl;
                ticketObj.UserCheckIns.Add(userCheckInObj);
            }
            var paperObj = new Paper()
            {
                PaperId = Guid.NewGuid().ToString(),
                ConferenceId = transacDataHolder.ConferenceId,
                ResearchConferencePhaseId = transacDataHolder.ResearchConferencePhaseId,
                CreatedAt = await _timeProviderService.GetVietnamTime(),
                PaperPhaseId = currentPaperPhase.PaperPhaseId,
                Title = transacDataHolder.Title,
                Description = transacDataHolder.Description,
                TicketId = transacDataHolder.TicketId,
                PaperAuthors = new List<PaperAuthor>()
            };
            var presenterPaperAuthor = new PaperAuthor()
            {
                IsPresenter = true,
                UserId = transacDataHolder.UserId,
                PaperId = paperObj.PaperId,
                IsRootAuthor = true,
            };
            paperObj.PaperAuthors.Add(presenterPaperAuthor);
            var pricePhase = await _unitOfWork.PricePhaseRepository.GetPricePhaseByPricePhaseId(transacDataHolder.PricePhaseId);
            if (pricePhase == null)
            {
                throw new BadRequestException("Giai đoạn vé không tìm thấy");
            }
            if (pricePhase.AvailableSlot <= 0)
            {
                throw new BadRequestException("Hết slot");
            }


            await _unitOfWork.BeginTransactionAsync();
            try
            {
                pricePhase!.AvailableSlot = pricePhase.AvailableSlot - 1;
                pricePhase!.ConferencePrice!.AvailableSlot = pricePhase!.ConferencePrice!.AvailableSlot - 1;
                pricePhase!.ConferencePrice!.Conference!.AvailableSlot = pricePhase!.ConferencePrice!.Conference!.AvailableSlot - 1;
                if (useWallet == true)
                {
                    var userWallet = await _unitOfWork.WalletRepository.GetWalletByUserIdAsync(transacDataHolder.UserId);
                    if (userWallet == null)
                    {
                        throw new NotFoundException($"Không tìm thấy ví cho user {transacDataHolder.UserId}");
                    }
                    userWallet.Balance = userWallet.Balance - amountFromIpn;
                    var walletTransactionObj = new WalletTransaction()
                    {
                        WalletTransactionId = Guid.NewGuid().ToString(),
                        WalletId = userWallet.WalletId,
                        Amount = -amountFromIpn,
                        TransactionType = WalletTransactionTypeEnum.Purchase.GetDescription(),
                        Description = $"Bạn đã mua đơn hàng #{transacDataHolder.TicketId} thành công vào lúc {timeNow}. Hãy check thông tin vé đã mua",
                        CreatedAt = timeNow,
                    };
                    await _unitOfWork.WalletRepository.UpdateWalletAsync(userWallet);
                    await _unitOfWork.WalletTransactionRepository.CreateWalletTransactionAsync(walletTransactionObj);
                }
                await _unitOfWork.TicketRepository.CreateTicketAsync(ticketObj);
                await _unitOfWork.PaperRepository.CreatePaperAsync(paperObj);
                await _unitOfWork.PricePhaseRepository.UpdatePricePhaseAsync(pricePhase);
                await _unitOfWork.CommitAsync();
                await _redisService.DeleteKeyAsync(orderId);
                await _redisService.DeleteKeyAsync(transacDataHolder.PaymentConferenceLockKey);
                await _redisService.DeleteKeyAsync(transacDataHolder.PaymentPhaseLockKey);

            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

        }





        #endregion

        public async Task ProcessCallBackForResearchConferenceAttendee(string orderId, decimal amountFromIpn, string transactionCodeFromIpn, bool useWallet)
        {


            var dateNow = await _timeProviderService.GetVietnamDate();
            var timeNow = await _timeProviderService.GetVietnamTime();
            var transac = await _redisService.GetStringAsync(orderId);
            var transacDataHolder = JsonSerializer.Deserialize<TransactionDataHolder>(transac, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            });

            var checkInStatus = await _unitOfWork.CheckInStatusRepository.GetCheckInStatusByNameAsync(CheckInStatusEnum.Pending.GetDescription());
            var globalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            var currentPaperPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.Abstract.GetDescription());
            if (checkInStatus == null || globalStatus == null || currentPaperPhase == null)
            {
                throw new NotFoundException($"Lỗi không tìm thấy trạng thái");
            }
            var ticketObj = new Ticket()
            {
                TicketId = transacDataHolder.TicketId,
                RegisteredDate = dateNow,
                IsRefunded = false,
                ActualPrice = amountFromIpn,
                UserId = transacDataHolder.UserId,
                PricePhaseId = transacDataHolder.PricePhaseId,
                Transactions = new List<Transaction>(),
                UserCheckIns = new List<UserCheckIn>()
            };
            var transaction = new Transaction()
            {
                TransactionId = Guid.NewGuid().ToString(),
                UserId = transacDataHolder.UserId,
                Currency = "VND",
                Amount = amountFromIpn,
                CreatedAt = timeNow,
                TransactionCode = transactionCodeFromIpn,
                IsRefunded = false,
                PaymentMethodId = transacDataHolder.PaymentMethodId,
                TicketId = transacDataHolder.TicketId
            };
            ticketObj.Transactions.Add(transaction);
            foreach (var sessionId in transacDataHolder.ConferenceSessionIds)
            {
                var userCheckInObj = new UserCheckIn()
                {
                    UserCheckinId = Guid.NewGuid().ToString(),
                    IsPresenter = false,
                    CheckinStatusId = checkInStatus.CheckinStatusId,
                    CheckInTime = null,
                    UserId = transacDataHolder.UserId,
                    TicketId = transacDataHolder.TicketId,
                    ConferenceSessionId = sessionId
                };
                var qrData = new QrDataPayload()
                {
                    userCheckinId = userCheckInObj.UserCheckinId,
                    userId = transacDataHolder.UserId,
                    ticketId = transacDataHolder.TicketId,
                    conferenceSessionId = sessionId,
                    createAt = timeNow,

                };
                var finalQrData = _qRCoderService.CreateQrDataPayload(qrData);
                var qrUrl = await _qRCoderService.GenerateQrCode(finalQrData);
                userCheckInObj.QrUrl = qrUrl;
                ticketObj.UserCheckIns.Add(userCheckInObj);
            }
            var pricePhase = await _unitOfWork.PricePhaseRepository.GetPricePhaseByPricePhaseId(transacDataHolder.PricePhaseId);
            if (pricePhase == null)
            {
                throw new BadRequestException("Giai đoạn vé không tìm thấy");
            }
            if (pricePhase.AvailableSlot <= 0)
            {
                throw new BadRequestException("Hết slot");
            }


            await _unitOfWork.BeginTransactionAsync();
            try
            {
                pricePhase!.AvailableSlot = pricePhase.AvailableSlot - 1;
                pricePhase!.ConferencePrice!.AvailableSlot = pricePhase!.ConferencePrice!.AvailableSlot - 1;
                pricePhase!.ConferencePrice!.Conference!.AvailableSlot = pricePhase!.ConferencePrice!.Conference!.AvailableSlot - 1;

                if (useWallet == true)
                {
                    var userWallet = await _unitOfWork.WalletRepository.GetWalletByUserIdAsync(transacDataHolder.UserId);
                    if (userWallet == null)
                    {
                        throw new NotFoundException($"Không tìm thấy ví cho user {transacDataHolder.UserId}");
                    }
                    userWallet.Balance = userWallet.Balance - amountFromIpn;
                    var walletTransactionObj = new WalletTransaction()
                    {
                        WalletTransactionId = Guid.NewGuid().ToString(),
                        WalletId = userWallet.WalletId,
                        Amount = -amountFromIpn,
                        TransactionType = WalletTransactionTypeEnum.Purchase.GetDescription(),
                        Description = $"Bạn đã mua đơn hàng #{transacDataHolder.TicketId} thành công vào lúc {timeNow}. Hãy check thông tin vé đã mua",
                        CreatedAt = timeNow,
                    };
                    await _unitOfWork.WalletRepository.UpdateWalletAsync(userWallet);
                    await _unitOfWork.WalletTransactionRepository.CreateWalletTransactionAsync(walletTransactionObj);
                }



                await _unitOfWork.TicketRepository.CreateTicketAsync(ticketObj);
                await _unitOfWork.PricePhaseRepository.UpdatePricePhaseAsync(pricePhase);
                await _unitOfWork.CommitAsync();
                await _redisService.DeleteKeyAsync(orderId);
                await _redisService.DeleteKeyAsync(transacDataHolder.PaymentConferenceLockKey);
                await _redisService.DeleteKeyAsync(transacDataHolder.PaymentPhaseLockKey);

            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

        }








        #region verify payment 
        public async Task VerifyPayOsDataForConference(Webhook data)
        {
            bool payOsCheck = await _payOsService.VerifyPayOs(data);
            if (!payOsCheck)
            {
                throw new BadRequestException("Dữ liệu payos không khả dụng");
            }
            await ProcessInsertPaymentData(data.Data.OrderCode.ToString(), (decimal)data.Data.Amount, data.Data.OrderCode.ToString(), useWallet: false);

        }

        public async Task VerifyMomoDataForConference(MomoPaymentCallBackResponse data)
        {
            bool momoCheck = _momoService.VerifyMomoPaymentData(data);
            if (!momoCheck)
            {
                throw new BadRequestException("Dữ liệu momo không khả dụng");
            }
            await ProcessInsertPaymentData(data.orderId!, (decimal)data.amount!.Value, data.transId.ToString()!, useWallet: false);
        }
        public async Task VerifyVnPayDataForConference(VnPayResponse data)
        {
            bool vnPayCheck = _vnPayService.VerifyVnPayPayment(data);
            if (!vnPayCheck)
            {
                throw new BadRequestException("Dữ liệu vnpay không khả dụng");
            }
            await ProcessInsertPaymentData(data.Vnp_TxnRef!, (decimal)data.Vnp_Amount!.Value / 100, data.Vnp_TransactionNo!.ToString()!, useWallet: false);
        }
        private async Task ProcessInsertPaymentData(string orderId, decimal amount, string transId, bool useWallet)
        {
            var transacKey = await _redisService.KeyExistsAsync(orderId);
            if (!transacKey)
            {
                throw new BadRequestException("Dữ liệu không tìm thấy");
            }
            var transac = await _redisService.GetStringAsync(orderId);
            var transacDataHolder = JsonSerializer.Deserialize<TransactionDataHolder>(transac, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            });
            if (transacDataHolder == null)
            {
                throw new BadRequestException("Không thấy dữ liệu giao dịch");
            }
            if (transacDataHolder!.IsResearchConference == true && transacDataHolder.IsResearchConferenceAuthor == true)
            {
                await ProcessCallBackForResearchConferenceAbstractSubmission(orderId, amount, transId, useWallet);
            }
            else if (transacDataHolder.IsResearchConference == false)
            {
                await ProcessCallBackForTechConference(orderId, amount, transId, useWallet);
            }
            else if (transacDataHolder!.IsResearchConference == true && transacDataHolder.IsResearchConferenceAuthor == false)
            {
                await ProcessCallBackForResearchConferenceAttendee(orderId, amount, transId, useWallet);
            }
            else
            {
                throw new BadRequestException("Dữ liệu thanh toán không khả dụng");
            }
        }





        public async Task CancelPayment(PaymentMethodEnum paymentMethodEnum, string orderCode)
        {
            var existingOrder = await _redisService.KeyExistsAsync(orderCode);
            if (!existingOrder)
            {
                throw new NotFoundException($"Mã order {orderCode} không tồn tại trong hệ thống");
            }

            switch (paymentMethodEnum)
            {
                case PaymentMethodEnum.PayOs:

                    await _payOsService.CancelPayOs(orderCode);
                    break;
                default:
                    throw new BadRequestException($"Payment method  không hỗ trợ hủy");
            }
            var transac = await _redisService.GetStringAsync(orderCode);
            var transacDataHolder = JsonSerializer.Deserialize<TransactionDataHolder>(transac, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            });
            if (transacDataHolder == null)
            {
                throw new NotFoundException("Thông tin đơn hàng không tồn tại");
            }
            await _redisService.DeleteKeyAsync(orderCode);
            await _redisService.DeleteKeyAsync(transacDataHolder.PaymentConferenceLockKey);
            await _redisService.DeleteKeyAsync(transacDataHolder.PaymentPhaseLockKey);
        }



        #endregion



    }




}


