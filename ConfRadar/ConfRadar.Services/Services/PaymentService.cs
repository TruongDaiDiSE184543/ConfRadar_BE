using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.Payment;
using ConfRadar.Services.DTOs.Transaction;
using ConfRadar.Services.Exceptions;
using ConfRadar.Shared.DTO.Payment;
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
        Task ProcessCallBackForTechConference(string orderId, decimal amountFromIpn, string transactionCodeFromIpn);
        Task ProcessCallBackForResearchConferenceAbstractSubmission(string orderId, decimal amountFromIpn, string transactionCodeFromIpn);
        Task VerifyPayOsDataForConference(Webhook data);

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
        public PaymentService(IUnitOfWork unitOfWork, IOptions<MomoSettings> momoSettings, IRedisService redisService, ITokenService tokenService, IMomoService momoService, IPayOsService payOsService, IOptions<PayOsSettings> payOsSettings)
        {
            _unitOfWork = unitOfWork;
            _momoSettings = momoSettings;
            _redisService = redisService;
            _tokenService = tokenService;
            _momoService = momoService;
            _payOsSettings = payOsSettings;
            _payOsService = payOsService;
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


        #region create payment
        public async Task<GeneralPaymentResultResponse> CreatePaymentForTechConference(CreateTechPaymentRequest request, string userId)
        {

            var paymentMethod = await _unitOfWork.PaymentMethodRepository.GetPaymentMethodById(request.PaymentMethodId);
            if (paymentMethod == null)
            {
                throw new BadRequestException($"Không tìm thấy phương thức thanh toán nào với mã {request.PaymentMethodId}");
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
                    return new GeneralPaymentResultResponse()
                    {
                        PaymentCreateSuccess = false,
                        CheckOutUrl = null,
                        PaymentMessage = $"Bạn hiện đang có 1 thanh toán, và chưa được thực hiện với cổng thanh toán {paymentMethodInPaymentLock!.MethodName}. Xin vui lòng thanh toán bằng cổng {paymentMethodInPaymentLock!.MethodName}. Hoặc hủy thanh toán, hoặc đợi hết hạn 90 phút"
                    };
                }
                return new GeneralPaymentResultResponse()
                {
                    PaymentCreateSuccess = false,
                    CheckOutUrl = paymentLockDataHolder.OldCheckOutUrl,
                    PaymentMessage = $"Chúng tôi phát hiện bạn đang có 1 giao dịch chưa được thực hiện với cổng:{paymentMethodInPaymentLock!.MethodName}. Xin vui lòng thực hiện giao dịch này "
                };

            }
            









            var ticketFound = await _unitOfWork.TicketRepository.GetTicketByUserIdAndConferenceId(userId, conferencePrice.ConferenceId!);
            if (ticketFound != null)
            {
                throw new BadRequestException("Bạn đã mua vé cho sự kiện này rồi!");
            }
            var dateNow = ExtensionHelper.GetVietnamDate();
            if (conferencePrice.Conference?.TicketSaleStart > dateNow)
            {
                throw new BadRequestException($"Chưa đến thời hạn mua vé. Thời hạn mua vé nằm trong khoảng từ {conferencePrice.Conference.TicketSaleStart} đến {conferencePrice.Conference.TicketSaleEnd}");
            }
            if (conferencePrice.Conference?.TicketSaleEnd < dateNow)
            {
                throw new BadRequestException("Đã hết thời hạn mua vé.");
            }

            var validPhases = conferencePrice.PricePhases.Where(p => p.StartDate <= dateNow && p.EndDate >= dateNow).OrderBy(p => p.StartDate).ToList();
            if (!validPhases.Any())
            {
                throw new BadRequestException("Hiện tại không có phase hợp lệ để thanh toán");
            }
            var currentPhase = validPhases.FirstOrDefault(p => p.AvailableSlot > 0);
            if (currentPhase == null)
            {
                throw new BadRequestException("Giai đoạn hiện tại hiện tại đã hết slot");
            }
            //check nhiều người mua trong 1 phase
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
            };


            var transacJson = JsonSerializer.Serialize(transactionData);
            var orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            double expireMinute = 90;
            string paymentDescription = "Thanh toán tech";
            string conferenceName = conferencePrice?.Conference?.ConferenceName ?? "";

            var listPaymentLinkItem = new List<PaymentLinkItem>()
            {
                new PaymentLinkItem()
                {
                    Name = $"Thanh toán vé cho hội nghị: {conferenceName}",
                    Price = finalAmount,
                    Quantity = 1,
                }
            };

            //thêm mutiple phương thức thanh toán:
            string checkOutUrl = string.Empty;
            switch (paymentMethod.MethodName)
            {
                case var s when s == PaymentMethodEnum.PayOs.GetDescription():
                    checkOutUrl = await _payOsService.CreatePayOsPayment(orderCode, finalAmount, paymentDescription, expireMinute, listPaymentLinkItem);
                    break;
                case var s when s == PaymentMethodEnum.MoMo.GetDescription():
                    var momoResult = await _momoService.CreateMomoPayment(orderCode.ToString(), finalAmount, paymentDescription, _momoSettings.Value.IpnTech, _momoSettings.Value.TechRedirectUrl);
                    checkOutUrl = momoResult.payUrl;
                    break;
                case var s when s == PaymentMethodEnum.ZaloPay.GetDescription():
                    throw new BadRequestException("Phương thức thanh toán ZaloPay đang trong trạng thái bảo trì và bị lỏ");

                default:
                    throw new BadRequestException("Phương thức thanh toán không hợp lệ");
            }
            var lockeyData = new PaymentLockKeyDTO()
            {
                OldCheckOutUrl = checkOutUrl,
                PaymentMethodId = request.PaymentMethodId
            };
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
        public async Task<GeneralPaymentResultResponse> CreatePaymentForAbstract(CreatePaperPaymentRequest request, string userId)
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
                    return new GeneralPaymentResultResponse()
                    {
                        PaymentCreateSuccess = false,
                        CheckOutUrl = null,
                        PaymentMessage = $"Bạn hiện đang có 1 thanh toán, và chưa được thực hiện với cổng thanh toán {paymentMethodInPaymentLock!.MethodName}. Xin vui lòng thanh toán bằng cổng {paymentMethodInPaymentLock!.MethodName}. Hoặc hủy thanh toán, hoặc đợi hết hạn 90 phút"
                    };
                }
                return new GeneralPaymentResultResponse()
                {
                    PaymentCreateSuccess = false,
                    CheckOutUrl = paymentLockDataHolder.OldCheckOutUrl,
                    PaymentMessage = $"Chúng tôi phát hiện bạn đang có 1 giao dịch chưa được thực hiện với cổng:{paymentMethodInPaymentLock!.MethodName}. Xin vui lòng thực hiện giao dịch này "
                };

            }
          
            var researchConferencePhases = conferencePrice.Conference?.ResearchConferencePhases;
            if (researchConferencePhases == null || !researchConferencePhases.Any())
            {
                throw new BadRequestException($"Không tìm thấy các giai đoạn trong hội nghị nghiên cứu này");
            }



          


            var ticketFound = await _unitOfWork.TicketRepository.GetTicketByUserIdAndConferenceId(userId, conferencePrice.ConferenceId);
            if (ticketFound != null)
            {
                throw new BadRequestException($"Bạn chỉ có thể mua vé 1 lần cho sự kiện này");
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
            var dateNow = ExtensionHelper.GetVietnamDate();
            if (conferencePrice.Conference?.TicketSaleStart > dateNow)
            {
                throw new BadRequestException($"Chưa đến thời hạn mua vé. Thời hạn mua vé nằm trong khoảng từ {conferencePrice.Conference.TicketSaleStart} đến {conferencePrice.Conference.TicketSaleEnd}");
            }
            if (conferencePrice.Conference?.TicketSaleEnd < dateNow)
            {
                throw new BadRequestException("Đã hết thời hạn mua vé.");
            }

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
            //check nhiều người mua trong 1 phase
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
                PaymentConferenceLockKey = lockKeyConference,
                PaymentPhaseLockKey = lockKeyPhase,
                PricePhaseId = currentPhase.PricePhaseId,
                Title = request.Title,
                Description = request.Description,
                IsResearchConference = true
            };
            var transacJson = JsonSerializer.Serialize(transactionData);

            //logic đa cổng
            var orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            double expireMinute = 90;
            string paymentDescription = "Thanh toán research";
            string conferenceName = conferencePrice?.Conference?.ConferenceName ?? "";

            var listPaymentLinkItem = new List<PaymentLinkItem>()
            {
                new PaymentLinkItem()
                {
                    Name = $"Thanh toán vé cho hội nghị: {conferenceName}",
                    Price = finalPrice,
                    Quantity = 1,
                }
            };

            //thêm mutiple phương thức thanh toán:
            string checkOutUrl = string.Empty;
            switch (paymentMethod.MethodName)
            {
                case var s when s == PaymentMethodEnum.PayOs.GetDescription():
                    checkOutUrl = await _payOsService.CreatePayOsPayment(orderCode, finalPrice, paymentDescription, expireMinute, listPaymentLinkItem);
                    break;
                case var s when s == PaymentMethodEnum.MoMo.GetDescription():
                    var momoResult = await _momoService.CreateMomoPayment(orderCode.ToString(), finalPrice, paymentDescription, _momoSettings.Value.IpnTech, _momoSettings.Value.TechRedirectUrl);
                    checkOutUrl = momoResult.payUrl;
                    break;
                case var s when s == PaymentMethodEnum.ZaloPay.GetDescription():
                    throw new BadRequestException("Phương thức thanh toán ZaloPay đang trong trạng thái bảo trì và bị lỏ");

                default:
                    throw new BadRequestException("Phương thức thanh toán không hợp lệ");
            }

            var lockeyData = new PaymentLockKeyDTO()
            {
                OldCheckOutUrl = checkOutUrl,
                PaymentMethodId = request.PaymentMethodId
            };
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

        #region process callback

        public async Task ProcessCallBackForTechConference(string orderId, decimal amountFromIpn, string transactionCodeFromIpn)
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
            var dateNow = ExtensionHelper.GetVietnamDate();
            var timeNow = ExtensionHelper.GetVietnamTime();
            var checkInStatus = await _unitOfWork.CheckInStatusRepository.GetCheckInStatusByNameAsync(CheckInStatusEnum.Pending.GetDescription());
            var ticketObj = new Ticket()
            {
                TicketId = transacDataHolder.TicketId,
                RegisteredDate = dateNow,
                IsRefunded = false,
                ActualPrice = amountFromIpn,
                UserId = transacDataHolder.UserId,
                ConferencePriceId = transacDataHolder.ConferencePriceId,
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
        public async Task ProcessCallBackForResearchConferenceAbstractSubmission(string orderId, decimal amountFromIpn, string transactionCodeFromIpn)
        {

            //var transacKey = await _redisService.KeyExistsAsync(orderId);
            //if (!transacKey)
            //{
            //    throw new NotFoundException("Dữ liệu không tìm thấy");
            //}
            var dateNow = ExtensionHelper.GetVietnamDate();
            var timeNow = ExtensionHelper.GetVietnamTime();
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
                throw new NotFoundException($"Lỗi không tìm thấy các trạng thái tương ứng trong hệ thống");
            }
            var ticketObj = new Ticket()
            {
                TicketId = transacDataHolder.TicketId,
                RegisteredDate = dateNow,
                IsRefunded = false,
                ActualPrice = amountFromIpn,
                UserId = transacDataHolder.UserId,
                ConferencePriceId = transacDataHolder.ConferencePriceId,
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
                ticketObj.UserCheckIns.Add(userCheckInObj);
            }
            var paperObj = new Paper()
            {
                PaperId = Guid.NewGuid().ToString(),
                ConferenceId = transacDataHolder.ConferenceId,
                CreatedAt = ExtensionHelper.GetVietnamTime(),
                PaperPhaseId = currentPaperPhase.PaperPhaseId,
                Title = transacDataHolder.Title,
                Description = transacDataHolder.Description,
                PaperAuthors = new List<PaperAuthor>()
            };
            var presenterPaperAuthor = new PaperAuthor()
            {
                IsPresenter = false,
                UserId = transacDataHolder.UserId,
                PaperId = paperObj.PaperId,
                IsRootAuthor = true,
            };
            paperObj.PaperAuthors.Add(presenterPaperAuthor);
            var pricePhase = await _unitOfWork.PricePhaseRepository.GetPricePhaseByPricePhaseId(transacDataHolder.PricePhaseId);
            if (pricePhase==null)
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
                await _unitOfWork.PaperRepository.CreatePaperAsync(paperObj);
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





        #endregion



        public async Task VerifyPayOsDataForConference(Webhook data)
        {
            bool payOsCheck = await _payOsService.VerifyPayOs(data);
            if (!payOsCheck)
            {
                throw new BadRequestException("Dữ liệu payos không khả dụng");
            }
            var transacKey = await _redisService.KeyExistsAsync(data.Data.OrderCode.ToString());
            if (!transacKey)
            {
                throw new BadRequestException("Dữ liệu không tìm thấy");
            }
            var transac = await _redisService.GetStringAsync(data.Data.OrderCode.ToString());
            var transacDataHolder = JsonSerializer.Deserialize<TransactionDataHolder>(transac, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            });
            if (transacDataHolder!.IsResearchConference == true)
            {
                await ProcessCallBackForResearchConferenceAbstractSubmission(data.Data.OrderCode.ToString(), (decimal)data.Data.Amount, data.Data.OrderCode.ToString());
            }
            else if (transacDataHolder.IsResearchConference == false)
            {
                await ProcessCallBackForTechConference(data.Data.OrderCode.ToString(), (decimal)data.Data.Amount, data.Data.OrderCode.ToString());
            }
            else
            {
                throw new BadRequestException("Không xác định loại hội nghị trong callback");
            }

        }
    }




}


