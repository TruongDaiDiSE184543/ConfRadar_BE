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
        Task ProcessCallBackForTechConference(string orderId, decimal amountFromIpn, string transactionCodeFromIpn);
        Task ProcessCallBackForResearchConferenceAbstractSubmission(string orderId, decimal amountFromIpn, string transactionCodeFromIpn);



        Task VerifyPayOsDataForConference(Webhook data);
        Task VerifyMomoDataForConference(MomoPaymentCallBackResponse data);
        Task VerifyVnPayDataForConference(VnPayResponse data);

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
                throw new BadRequestException($"Không tìm th?y phuong th?c thanh toán nào v?i mã {request.PaymentMethodId}");
            }

            var conferencePrice = await _unitOfWork.ConferencePriceRepository.GetConferencePriceByIdAsync(request.ConferencePriceId);
            if (conferencePrice == null)
            {
                throw new BadRequestException($"Giá conference v?i id {request.ConferencePriceId} không tìm th?y");
            }
            if (conferencePrice.Conference?.AvailableSlot <= 0)
            {
                throw new BadRequestException($"{conferencePrice.Conference?.ConferenceName} dã bán h?t vé!");
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
                    throw new BadRequestException($"B?n hi?n dang có 1 thanh toán, và chua du?c th?c hi?n v?i c?ng thanh toán {paymentMethodInPaymentLock!.MethodName}. Xin vui lòng thanh toán b?ng c?ng {paymentMethodInPaymentLock!.MethodName}. Ho?c h?y thanh toán, ho?c d?i h?t h?n 90 phút");
                }
                return new GeneralPaymentResultResponse()
                {
                    PaymentCreateSuccess = false,
                    CheckOutUrl = paymentLockDataHolder.OldCheckOutUrl,
                    PaymentMessage = $"Chúng tôi phát hi?n b?n dang có 1 giao d?ch chua du?c th?c hi?n v?i c?ng:{paymentMethodInPaymentLock!.MethodName}. Xin vui lòng th?c hi?n giao d?ch này "
                };

            }










            var ticketFound = await _unitOfWork.TicketRepository.GetAttendeeTicketByUserIdAndConferenceId(userId, conferencePrice.ConferenceId!);
            if (ticketFound != null)
            {
                throw new BadRequestException("B?n dã mua vé cho s? ki?n này r?i!");
            }
            var dateNow = await _timeProviderService.GetVietnamDate();
            if (conferencePrice.Conference?.TicketSaleStart > dateNow)
            {
                throw new BadRequestException($"Chua d?n th?i h?n mua vé. Th?i h?n mua vé n?m trong kho?ng t? {conferencePrice.Conference.TicketSaleStart} d?n {conferencePrice.Conference.TicketSaleEnd}");
            }
            if (conferencePrice.Conference?.TicketSaleEnd < dateNow)
            {
                throw new BadRequestException("Ðã h?t th?i h?n mua vé.");
            }
            if (conferencePrice.IsAuthor == true)
            {
                throw new BadRequestException("Vé này ch? dành cho ngu?i tham d?.");
            }

            var validPhases = conferencePrice.PricePhases.Where(p => p.StartDate <= dateNow && p.EndDate >= dateNow).OrderBy(p => p.StartDate).ToList();
            if (!validPhases.Any())
            {
                throw new BadRequestException("Hi?n t?i không có phase h?p l? d? thanh toán");
            }
            var currentPhase = validPhases.FirstOrDefault(p => p.AvailableSlot > 0);
            if (currentPhase == null)
            {
                throw new BadRequestException("Giai do?n hi?n t?i hi?n t?i dã h?t slot");
            }
            //check nhi?u ngu?i mua trong 1 phase
            var paymentPhaseLockPattern = ExtensionHelper.GetPaymentPhaseLockKeyPattern(currentPhase.PricePhaseId!);
            var paymentPhaseLockList = await _redisService.GetKeysByPatternAsync(paymentPhaseLockPattern);
            int paymentPhaseLockCount = paymentPhaseLockList.Count();
            if (paymentPhaseLockCount >= currentPhase.AvailableSlot)
            {
                return new GeneralPaymentResultResponse()
                {
                    PaymentCreateSuccess = false,
                    CheckOutUrl = null,
                    PaymentMessage = $"Hi?n t?i dang có {paymentPhaseLockCount} khách hàng dang th?c hi?n giao d?ch trong giai do?n hi?n t?i mua vé t? {currentPhase.StartDate} d?n {currentPhase.EndDate} tuong ?ng v?i {currentPhase.AvailableSlot} s? vé "
                };

            }

            var discountPercent = currentPhase.ApplyPercent ?? 0;

            var rawPrice = conferencePrice.TicketPrice;
            var discountedPrice = (long)(rawPrice * ((decimal)discountPercent / (decimal)100.0));
            var finalAmount = (long)discountedPrice;
            if (finalAmount <= 10000)
            {
                throw new BadRequestException($"Giá cho vé hi?n t?i là {finalAmount} không kh? d?ng cho c?ng thanh toán trong h? th?ng xin hãy liên h? ban t? ch?c s? ki?n");
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
            string paymentDescription = "Thanh toan tech";
            string conferenceName = conferencePrice?.Conference?.ConferenceName ?? "";

            var listPaymentLinkItem = new List<PaymentLinkItem>()
            {
                new PaymentLinkItem()
                {
                    Name = $"Thanh toán vé cho h?i ngh?: {conferenceName}",
                    Price = finalAmount,
                    Quantity = 1,
                }
            };

            //thêm mutiple phuong th?c thanh toán:
            string checkOutUrl = string.Empty;
            switch (paymentMethod.MethodName)
            {
                case var s when s == PaymentMethodEnum.PayOs.GetDescription():
                    checkOutUrl = await _payOsService.CreatePayOsPayment(orderCode, finalAmount, paymentDescription, expireMinute, listPaymentLinkItem);
                    break;
                case var s when s == PaymentMethodEnum.MoMo.GetDescription():
                    var momoResult = await _momoService.CreateMomoPayment(orderCode.ToString(), finalAmount, paymentDescription);
                    checkOutUrl = momoResult.payUrl;
                    break;
                case var s when s == PaymentMethodEnum.VnPay.GetDescription():
                    var vnPayResult = await _vnPayService.CreateVnPayPayment(orderCode, finalAmount, expireMinute);
                    checkOutUrl = vnPayResult;
                    break;
                case var s when s == PaymentMethodEnum.ZaloPay.GetDescription():
                    throw new BadRequestException("Phuong th?c thanh toán ZaloPay dang trong tr?ng thái b?o trì và b? l?");

                default:
                    throw new BadRequestException("Phuong th?c thanh toán không h?p l?");
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
                PaymentMessage = "T?o liên k?t thanh toán thành công. Vui lòng hoàn t?t giao d?ch t?i c?ng thanh toán.",
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
                throw new BadRequestException($"Không tìm th?y phuong th?c thanh toán nào v?i mã {request.PaymentMethodId}");
            }
            var conferencePrice = await _unitOfWork.ConferencePriceRepository.GetConferencePriceByIdAsync(request.ConferencePriceId);
            if (conferencePrice == null)
            {
                throw new BadRequestException($"Giá h?i ngh? v?i id {request.ConferencePriceId} không tìm th?y");
            }
            if (conferencePrice.Conference?.AvailableSlot <= 0)
            {
                throw new BadRequestException($"{conferencePrice.Conference?.ConferenceName} dã bán h?t vé!");
            }
            if (conferencePrice.Conference!.IsResearchConference == false)
            {
                throw new BadRequestException($"B?n ch? có th? n?p abstract cho research conference");
            }
            if (conferencePrice.IsAuthor == false)
            {
                throw new BadRequestException($"Giá vé hi?n t?i không dành cho tác gi?, xin hãy ch?n m?c giá khác");
            }
            if (conferencePrice.Conference.IsInternalHosted == false)
            {
                throw new BadRequestException($"B?n ch? có th? n?p abstract cho research conference t? ch?c b?i confradar");
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
                    throw new BadRequestException($"B?n hi?n dang có 1 thanh toán, và chua du?c th?c hi?n v?i c?ng thanh toán {paymentMethodInPaymentLock!.MethodName}. Xin vui lòng thanh toán b?ng c?ng {paymentMethodInPaymentLock!.MethodName}. Ho?c h?y thanh toán, ho?c d?i h?t h?n 90 phút");

                }
                return new GeneralPaymentResultResponse()
                {
                    PaymentCreateSuccess = false,
                    CheckOutUrl = paymentLockDataHolder.OldCheckOutUrl,
                    PaymentMessage = $"Chúng tôi phát hi?n b?n dang có 1 giao d?ch chua du?c th?c hi?n v?i c?ng:{paymentMethodInPaymentLock!.MethodName}. Xin vui lòng th?c hi?n giao d?ch này "
                };

            }

            var researchConferencePhases = conferencePrice.Conference?.ResearchConferencePhases;
            if (researchConferencePhases == null || !researchConferencePhases.Any())
            {
                throw new BadRequestException($"Không tìm th?y các giai do?n trong h?i ngh? nghiên c?u này");
            }
            var activeResearchConferencePhase = researchConferencePhases.FirstOrDefault(rcp => rcp.IsActive == true);
            if (activeResearchConferencePhase == null)
            {
                throw new BadRequestException($"Giai do?n h?i ngh? nghiên c?u hi?n t?i dã b? dóng. Xin vui lòng liên h? ban t? ch?c s? ki?n");
            }
            if (activeResearchConferencePhase.RegistrationStartDate > dateNow)
            {
                throw new BadRequestException($"Chua d?n th?i h?n mua vé. Th?i h?n mua vé n?m trong kho?ng t? {activeResearchConferencePhase.RegistrationStartDate} d?n {activeResearchConferencePhase.RegistrationEndDate}");
            }
            if (activeResearchConferencePhase.RegistrationEndDate < dateNow)
            {
                throw new BadRequestException("Ðã h?t th?i h?n mua vé.");
            }



            var ticketFound = await _unitOfWork.TicketRepository.GetAuthorTicketByUserIdAndConferenceId(userId, conferencePrice.ConferenceId);
            if (ticketFound != null)
            {
                throw new BadRequestException($"B?n ch? có th? mua vé 1 l?n cho s? ki?n này");
            }
            var reviewerContractFound = await _unitOfWork.ReviewerContractRepository.GetContractByUserAndConferenceAsync(userId, conferencePrice.ConferenceId);
            if (reviewerContractFound != null)
            {
                if (reviewerContractFound.IsActive == true)
                {
                    throw new BadRequestException($"B?n dang có h?p d?ng v?i s? ki?n này nên không th? th?c hi?n thanh toán");
                }
            }
            var internalReviewRole = await _unitOfWork.RoleRepository.GetRoleByRoleName(SystemRoleEnum.LocalReviewer.GetDescription());
            if (internalReviewRole == null)
            {
                throw new NotFoundException($"Không tìm th?y role trong h? th?ng");
            }
            var userRole = await _unitOfWork.UserRoleRepository.GetUserRoleByUserAndRole(userId, internalReviewRole.RoleId);
            if (userRole != null)
            {
                throw new BadRequestException($"B?n không th? mua vé này vì b?n là reviewer trong h? th?ng");
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
                throw new BadRequestException("Hi?n t?i không có phase h?p l? d? n?p abstract");
            }
            var currentPhase = validPhases.FirstOrDefault(p => p.AvailableSlot > 0);
            if (currentPhase == null)
            {
                throw new BadRequestException("Giai do?n hi?n t?i dã h?t slot");
            }
            //check nhi?u ngu?i mua trong 1 phase
            var paymentPhaseLockPattern = ExtensionHelper.GetPaymentPhaseLockKeyPattern(currentPhase.PricePhaseId!);
            var paymentPhaseLockList = await _redisService.GetKeysByPatternAsync(paymentPhaseLockPattern);
            int paymentPhaseLockCount = paymentPhaseLockList.Count();
            if (paymentPhaseLockCount >= currentPhase.AvailableSlot)
            {
                return new GeneralPaymentResultResponse()
                {
                    PaymentCreateSuccess = false,
                    CheckOutUrl = null,
                    PaymentMessage = $"Hi?n t?i dang có {paymentPhaseLockCount} khách hàng dang th?c hi?n giao d?ch trong giai do?n hi?n t?i mua vé t? {currentPhase.StartDate} d?n {currentPhase.EndDate} tuong ?ng v?i {currentPhase.AvailableSlot} s? vé "
                };

            }



            var sessionIdsList = conferencePrice.Conference.ConferenceSessions.Select(cs => cs.ConferenceSessionId).ToList();
            applyPercent = currentPhase.ApplyPercent ?? 0;
            long finalPrice = 0;
            if (applyPercent < 0)
            {
                throw new BadRequestException($"% gi?m giá cho vé hi?n t?i là {applyPercent} không kh? d?ng xin hãy liên h? ban t? ch?c s? ki?n");
            }
            finalPrice = (long)(conferencePrice.TicketPrice * ((decimal)applyPercent / (decimal)100.0));
            if (finalPrice <= 10000)
            {
                throw new BadRequestException($"Giá cho vé hi?n t?i là {finalPrice} không kh? d?ng cho c?ng thanh toán trong h? th?ng xin hãy liên h? ban t? ch?c s? ki?n");
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

            //logic da c?ng
            var orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            double expireMinute = 90;
            string paymentDescription = "Thanhtoanresearch";
            string conferenceName = conferencePrice?.Conference?.ConferenceName ?? "";

            var listPaymentLinkItem = new List<PaymentLinkItem>()
            {
                new PaymentLinkItem()
                {
                    Name = $"Thanh toán vé cho h?i ngh?: {conferenceName}",
                    Price = finalPrice,
                    Quantity = 1,
                }
            };

            //thêm mutiple phuong th?c thanh toán:
            string checkOutUrl = string.Empty;
            switch (paymentMethod.MethodName)
            {
                case var s when s == PaymentMethodEnum.PayOs.GetDescription():
                    checkOutUrl = await _payOsService.CreatePayOsPayment(orderCode, finalPrice, paymentDescription, expireMinute, listPaymentLinkItem);
                    break;
                case var s when s == PaymentMethodEnum.MoMo.GetDescription():
                    var momoResult = await _momoService.CreateMomoPayment(orderCode.ToString(), finalPrice, paymentDescription);
                    checkOutUrl = momoResult.payUrl;
                    break;
                case var s when s == PaymentMethodEnum.VnPay.GetDescription():
                    var vnPayResult = await _vnPayService.CreateVnPayPayment(orderCode, finalPrice, expireMinute);
                    checkOutUrl = vnPayResult;
                    break;
                case var s when s == PaymentMethodEnum.ZaloPay.GetDescription():
                    throw new BadRequestException("Phuong th?c thanh toán ZaloPay dang trong tr?ng thái b?o trì và b? l?");

                default:
                    throw new BadRequestException("Phuong th?c thanh toán không h?p l?");
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
                PaymentMessage = "T?o liên k?t thanh toán thành công. Vui lòng hoàn t?t giao d?ch t?i c?ng thanh toán.",
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
                throw new BadRequestException($"Không tìm th?y phuong th?c thanh toán nào v?i mã {request.PaymentMethodId}");
            }
            var conferencePrice = await _unitOfWork.ConferencePriceRepository.GetConferencePriceByIdAsync(request.ConferencePriceId);
            if (conferencePrice == null)
            {
                throw new BadRequestException($"Giá h?i ngh? v?i id {request.ConferencePriceId} không tìm th?y");
            }
            if (conferencePrice.Conference?.AvailableSlot <= 0)
            {
                throw new BadRequestException($"{conferencePrice.Conference?.ConferenceName} dã bán h?t vé!");
            }
            if (conferencePrice.Conference!.IsResearchConference == false)
            {
                throw new BadRequestException($"B?n ch? có th? mua vé cho h?i ngh? nghiên c?u");
            }
            if (conferencePrice.IsAuthor == true)
            {
                throw new BadRequestException($"Giá vé hi?n t?i ch? dành cho ngu?i d? thính trong h?i ngh? nghiên c?u");
            }
            if (conferencePrice.Conference.IsInternalHosted == false)
            {
                throw new BadRequestException($"B?n ch? có th? tham gia h?i ngh? t? ch?c b?i confradar");
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
                    throw new BadRequestException($"B?n hi?n dang có 1 thanh toán, và chua du?c th?c hi?n v?i c?ng thanh toán {paymentMethodInPaymentLock!.MethodName}. Xin vui lòng thanh toán b?ng c?ng {paymentMethodInPaymentLock!.MethodName}. Ho?c h?y thanh toán, ho?c d?i h?t h?n 90 phút");

                }
                return new GeneralPaymentResultResponse()
                {
                    PaymentCreateSuccess = false,
                    CheckOutUrl = paymentLockDataHolder.OldCheckOutUrl,
                    PaymentMessage = $"Chúng tôi phát hi?n b?n dang có 1 giao d?ch chua du?c th?c hi?n v?i c?ng:{paymentMethodInPaymentLock!.MethodName}. Xin vui lòng th?c hi?n giao d?ch này "
                };

            }

            var researchConferencePhases = conferencePrice.Conference?.ResearchConferencePhases;
            if (researchConferencePhases == null || !researchConferencePhases.Any())
            {
                throw new BadRequestException($"Không tìm th?y các giai do?n trong h?i ngh? nghiên c?u này");
            }






            var ticketFound = await _unitOfWork.TicketRepository.GetAttendeeTicketByUserIdAndConferenceId(userId, conferencePrice.ConferenceId);
            if (ticketFound != null)
            {
                throw new BadRequestException($"B?n ch? có th? mua vé 1 l?n cho s? ki?n này");
            }
            decimal applyPercent = 0;
            var dateNow = await _timeProviderService.GetVietnamDate();
            if (conferencePrice.Conference?.TicketSaleStart > dateNow)
            {
                throw new BadRequestException($"Chua d?n th?i h?n mua vé. Th?i h?n mua vé n?m trong kho?ng t? {conferencePrice.Conference.TicketSaleStart} d?n {conferencePrice.Conference.TicketSaleEnd}");
            }
            if (conferencePrice.Conference?.TicketSaleEnd < dateNow)
            {
                throw new BadRequestException("Ðã h?t th?i h?n mua vé.");
            }

            var validPhases = conferencePrice.PricePhases
            .Where(p => p.StartDate <= dateNow && p.EndDate >= dateNow)
            .OrderBy(p => p.StartDate)
            .ToList();

            if (!validPhases.Any())
            {
                throw new BadRequestException("Hi?n t?i không có phase h?p l? d? n?p abstract");
            }
            var currentPhase = validPhases.FirstOrDefault(p => p.AvailableSlot > 0);
            if (currentPhase == null)
            {
                throw new BadRequestException("Giai do?n hi?n t?i dã h?t slot");
            }
            //check nhi?u ngu?i mua trong 1 phase
            var paymentPhaseLockPattern = ExtensionHelper.GetPaymentPhaseLockKeyPattern(currentPhase.PricePhaseId!);
            var paymentPhaseLockList = await _redisService.GetKeysByPatternAsync(paymentPhaseLockPattern);
            int paymentPhaseLockCount = paymentPhaseLockList.Count();
            if (paymentPhaseLockCount >= currentPhase.AvailableSlot)
            {
                return new GeneralPaymentResultResponse()
                {
                    PaymentCreateSuccess = false,
                    CheckOutUrl = null,
                    PaymentMessage = $"Hi?n t?i dang có {paymentPhaseLockCount} khách hàng dang th?c hi?n giao d?ch trong giai do?n hi?n t?i mua vé t? {currentPhase.StartDate} d?n {currentPhase.EndDate} tuong ?ng v?i {currentPhase.AvailableSlot} s? vé "
                };

            }



            var sessionIdsList = conferencePrice.Conference.ConferenceSessions.Select(cs => cs.ConferenceSessionId).ToList();
            applyPercent = currentPhase.ApplyPercent ?? 0;
            long finalPrice = 0;
            if (applyPercent < 0)
            {
                throw new BadRequestException($"% gi?m giá cho vé hi?n t?i là {applyPercent} không kh? d?ng xin hãy liên h? ban t? ch?c s? ki?n");
            }
            finalPrice = (long)(conferencePrice.TicketPrice * ((decimal)applyPercent / (decimal)100.0));
            if (finalPrice <= 10000)
            {
                throw new BadRequestException($"Giá cho vé hi?n t?i là {finalPrice} không kh? d?ng cho c?ng thanh toán trong h? th?ng xin hãy liên h? ban t? ch?c s? ki?n");
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
            string paymentDescription = "Thanh toán research";
            string conferenceName = conferencePrice?.Conference?.ConferenceName ?? "";

            var listPaymentLinkItem = new List<PaymentLinkItem>()
            {
                new PaymentLinkItem()
                {
                    Name = $"Thanh toán vé cho h?i ngh?: {conferenceName}",
                    Price = finalPrice,
                    Quantity = 1,
                }
            };

            //thêm mutiple phuong th?c thanh toán:
            string checkOutUrl = string.Empty;
            switch (paymentMethod.MethodName)
            {
                case var s when s == PaymentMethodEnum.PayOs.GetDescription():
                    checkOutUrl = await _payOsService.CreatePayOsPayment(orderCode, finalPrice, paymentDescription, expireMinute, listPaymentLinkItem);
                    break;
                case var s when s == PaymentMethodEnum.MoMo.GetDescription():
                    var momoResult = await _momoService.CreateMomoPayment(orderCode.ToString(), finalPrice, paymentDescription);
                    checkOutUrl = momoResult.payUrl;
                    break;
                case var s when s == PaymentMethodEnum.VnPay.GetDescription():
                    var vnPayResult = _vnPayService.CreateVnPayPayment(orderCode, finalPrice, expireMinute);
                    checkOutUrl = await vnPayResult;
                    break;
                case var s when s == PaymentMethodEnum.ZaloPay.GetDescription():
                    throw new BadRequestException("Phuong th?c thanh toán ZaloPay dang trong tr?ng thái b?o trì và b? l?");

                default:
                    throw new BadRequestException("Phuong th?c thanh toán không h?p l?");
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
                PaymentMessage = "T?o liên k?t thanh toán thành công. Vui lòng hoàn t?t giao d?ch t?i c?ng thanh toán.",
                CheckOutUrl = checkOutUrl,
            };
        }
        #endregion


        #region process insert data

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
                throw new BadRequestException("Không tìm th?y phase tuong ?ng.");
            }
            if (pricePhase.AvailableSlot <= 0)
            {
                throw new BadRequestException("Giai do?n hi?n t?i dã h?t slot.");
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
                throw new NotFoundException($"L?i không tìm th?y các tr?ng thái tuong ?ng trong h? th?ng");
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
            if (pricePhase == null)
            {
                throw new BadRequestException("Giai do?n vé không tìm th?y");
            }
            if (pricePhase.AvailableSlot <= 0)
            {
                throw new BadRequestException("H?t slot");
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

        public async Task ProcessCallBackForResearchConferenceAttendee(string orderId, decimal amountFromIpn, string transactionCodeFromIpn)
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
                throw new NotFoundException($"L?i không tìm th?y các tr?ng thái tuong ?ng trong h? th?ng");
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
                throw new BadRequestException("Giai do?n vé không tìm th?y");
            }
            if (pricePhase.AvailableSlot <= 0)
            {
                throw new BadRequestException("H?t slot");
            }


            await _unitOfWork.BeginTransactionAsync();
            try
            {
                pricePhase!.AvailableSlot = pricePhase.AvailableSlot - 1;
                pricePhase!.ConferencePrice!.AvailableSlot = pricePhase!.ConferencePrice!.AvailableSlot - 1;
                pricePhase!.ConferencePrice!.Conference!.AvailableSlot = pricePhase!.ConferencePrice!.Conference!.AvailableSlot - 1;

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
                throw new BadRequestException("D? li?u payos không kh? d?ng");
            }
            await ProcessInsertPaymentData(data.Data.OrderCode.ToString(), (decimal)data.Data.Amount, data.Data.OrderCode.ToString());

        }

        public async Task VerifyMomoDataForConference(MomoPaymentCallBackResponse data)
        {
            bool momoCheck = _momoService.VerifyMomoPaymentData(data);
            if (!momoCheck)
            {
                throw new BadRequestException("D? li?u momo không kh? d?ng");
            }
            await ProcessInsertPaymentData(data.orderId!, (decimal)data.amount!.Value, data.transId.ToString()!);
        }
        public async Task VerifyVnPayDataForConference(VnPayResponse data)
        {
            bool vnPayCheck = _vnPayService.VerifyVnPayPayment(data);
            if (!vnPayCheck)
            {
                throw new BadRequestException("D? li?u vnpay không kh? d?ng");
            }
            await ProcessInsertPaymentData(data.Vnp_TxnRef!, (decimal)data.Vnp_Amount!.Value / 100, data.Vnp_TransactionNo!.ToString()!);
        }
        private async Task ProcessInsertPaymentData(string orderId, decimal amount, string transId)
        {
            var transacKey = await _redisService.KeyExistsAsync(orderId);
            if (!transacKey)
            {
                throw new BadRequestException("D? li?u không tìm th?y");
            }
            var transac = await _redisService.GetStringAsync(orderId);
            var transacDataHolder = JsonSerializer.Deserialize<TransactionDataHolder>(transac, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            });
            if (transacDataHolder == null)
            {
                throw new BadRequestException("Không th? d?c d? li?u giao d?ch");
            }
            if (transacDataHolder!.IsResearchConference == true && transacDataHolder.IsResearchConferenceAuthor == true)
            {
                await ProcessCallBackForResearchConferenceAbstractSubmission(orderId, amount, transId);
            }
            else if (transacDataHolder.IsResearchConference == false)
            {
                await ProcessCallBackForTechConference(orderId, amount, transId);
            }
            else if (transacDataHolder!.IsResearchConference == true && transacDataHolder.IsResearchConferenceAuthor == false)
            {
                await ProcessCallBackForResearchConferenceAttendee(orderId, amount, transId);
            }
            else
            {
                throw new BadRequestException("D? li?u thanh toán không kh? d?ng");
            }
        }
        #endregion



    }




}


