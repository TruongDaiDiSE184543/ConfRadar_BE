using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.Payment;
using ConfRadar.Services.Exceptions;
using Microsoft.Extensions.Options;
using System.Data;
using System.Text;
using System.Text.Json;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.Services.Services
{
    public interface IMomoService
    {
        Task<string> HandleMomoPaymentWithTechConf(CreateTechPaymentRequest request, string userId);
        Task VerifyMomoPaymentDataForTechConference(MomoPaymentCallBackResponse data);
        Task VerifyMomoPaymentDataForResearchConferenceAbstractSubmission(MomoPaymentCallBackResponse data);
        Task<string> CreatePaymentForAbstract(CreatePaperPaymentRequest request, string userId);
    }
    public class MomoService : IMomoService
    {
        private readonly IOptions<MomoSettings> _momoSettings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisService _redisService;
        private readonly ITokenService _tokenService;
        public MomoService(IOptions<MomoSettings> momoSettings, IUnitOfWork unitOfWork, IRedisService redisService, ITokenService tokenService)
        {
            _momoSettings = momoSettings;
            _unitOfWork = unitOfWork;
            _redisService = redisService;
            _tokenService = tokenService;
        }
        public async Task<string> HandleMomoPaymentWithTechConf(CreateTechPaymentRequest request, string userId)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUserId(userId);
            if (user == null)
            {
                throw new ConfRadarAuthenticationException("User không tồn tại");
            }
            var conferencePrice = await _unitOfWork.ConferencePriceRepository.GetConferencePriceByConferencePriceId(request.ConferencePriceId);
            if (conferencePrice == null)
            {
                throw new BadRequestException($"Giá conference với id {request.ConferencePriceId} không tìm thấy");
            }
            if (conferencePrice.Conference?.AvailableSlot == 0)
            {
                throw new BadRequestException($"{conferencePrice.Conference?.ConferenceName} is sold out!");
            }
            var paymentLockKey = ExtensionHelper.GetPaymentLockKeyResult(userId, request.ConferencePriceId);
            bool paymentLockFound = await _redisService.KeyExistsAsync(paymentLockKey);
            if (paymentLockFound == true)
            {
                throw new BadRequestException("Bạn chưa thanh toán vé. Xin vui lòng đợi 120 phút nữa để mua lại vé!");
            }
            var ticketFound = await _unitOfWork.TicketRepository.GetTicketByUserIdAndConferencePriceId(userId, request.ConferencePriceId);
            if (ticketFound != null)
            {
                throw new BadRequestException("Bạn đã mua vé cho sự kiện này rồi!");
            }
            var dateNow = ExtensionHelper.GetVietnamDate();
            var validPhases = conferencePrice.PricePhases.Where(p => p.StartDate <= dateNow && p.EndDate >= dateNow).OrderBy(p => p.StartDate).ToList();
            if (!validPhases.Any())
            {
                throw new BadRequestException("Hiện tại không có phase hợp lệ để thanh toán");
            }
            var currentPhase = validPhases.FirstOrDefault(p => p.AvailableSlot > 0);
            if (currentPhase == null)
            {
                throw new BadRequestException("Tất cả các phase hợp lệ hiện tại đã hết slot");
            }
            var discountPercent = currentPhase.ApplyPercent ?? 0;

            var rawPrice = conferencePrice.TicketPrice;
            var discountedPrice = rawPrice - (rawPrice * discountPercent / 100);
            var finalAmount = (long)discountedPrice;

            var paymentMethod = await _unitOfWork.PaymentMethodRepository.GetPaymentMethodByName(PaymentMethodEnum.MoMo.GetDescription());
            var sessionIds = conferencePrice.Conference!.ConferenceSessions.Select(s => s.ConferenceSessionId).ToList();
            var ticketId = Guid.NewGuid().ToString();

            var lockKey = ExtensionHelper.GetPaymentLockKeyResult(userId, request.ConferencePriceId);
            var transactionData = new TransactionDataHolder
            {
                TicketId = ticketId,
                UserId = user.UserId,
                PaymentMethodId = paymentMethod.PaymentMethodId,
                ConferencePriceId = request.ConferencePriceId,
                ConferenceSessionIds = sessionIds,
                ConferenceId = conferencePrice.ConferenceId,
                PaymentLockKey = lockKey,
                Description = null,
                Title = null,
            };

            var transacJson = JsonSerializer.Serialize(transactionData);
            var result = await CreateMomoPayment(lockKey, finalAmount, $"Trả phí cho {conferencePrice.Conference?.ConferenceName}", _momoSettings.Value.IpnTech, _momoSettings.Value.TechRedirectUrl);
            await _redisService.SetStringAsync(lockKey, "LockKey", TimeSpan.FromMinutes(120));
            await _redisService.SetStringAsync(ticketId, transacJson, TimeSpan.FromMinutes(120));
            return result.payUrl;
        }
        public async Task<string> CreatePaymentForAbstract(CreatePaperPaymentRequest request, string userId)
        {

            var conferencePrice = await _unitOfWork.ConferencePriceRepository.GetConferencePriceByIdAsync(request.ConferencePriceId);
            if (conferencePrice == null)
            {
                throw new BadRequestException($"Giá hội nghị với id {request.ConferencePriceId} không tìm thấy");
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
           
            if (conferencePrice.IsAuthor == false)
            {
                throw new BadRequestException($"Giá vé hiện tại không khả dụng cho việc nộp abstract");

            }
            var researchConferencePhases = conferencePrice.Conference?.ResearchConferencePhases;
            if (researchConferencePhases == null || !researchConferencePhases.Any())
            {
                throw new BadRequestException($"Không tìm thấy các giai đoạn trong hội nghị nghiên cứu này");
            }
           
            var activeFirstResearchConferencePhase = researchConferencePhases.FirstOrDefault(rcp => rcp.IsWaitlist == false && rcp.IsActive == true);
            var activeSecondWaitListResearchConferencePhase = researchConferencePhases.FirstOrDefault(rcp => rcp.IsWaitlist == true && rcp.IsActive == true);
            var pendingWaitListStatus = await _unitOfWork.WaitListStatusRepository.GetWaitListStatusByNameAsync(WaitListStatusEnum.Pending.GetDescription());
            if (pendingWaitListStatus == null)
            {
                throw new NotFoundException("Không tìm thấy trạng thái waitlist trong hệ thống");
            }
            if (activeFirstResearchConferencePhase == null || activeSecondWaitListResearchConferencePhase == null)
            {
                throw new NotFoundException($"Không tìm thấy các giai đoạn trong hội nghị nghiên cứu");
            }
            //check cho 2nd phase
            if (activeSecondWaitListResearchConferencePhase != null)
            {
                var paperWaitListFound = await _unitOfWork.PaperWaitListRepository.GetPaperWaitListByUserIdAndConferenceIdAsync(userId, conferencePrice.ConferenceId);
                if (paperWaitListFound != null && conferencePrice.AvailableSlot >0)
                {
                    await _unitOfWork.PaperWaitListRepository.DeletePaperWaitListAsync(paperWaitListFound);
                }

                if (conferencePrice.AvailableSlot <= 0 && paperWaitListFound ==null)
                {
                    var paperWaitList = new PaperWaitList()
                    {
                        PaperWaitListId = Guid.NewGuid().ToString(),
                        CreatedAt = ExtensionHelper.GetVietnamTime(),
                        NotifiedAt = null,
                        WaitListStatusId = pendingWaitListStatus.WaitListStatusId,
                        UserId = userId,
                        ConferenceId = conferencePrice.ConferenceId,
                    };
                    await _unitOfWork.PaperWaitListRepository.CreatePaperWaitListAsync(paperWaitList);
                    throw new BadRequestException("Hiện tại hội nghị nghiên cứu của của chúng tôi đã hết slot nên bạn sẽ được thêm vào danh sách hàng đợi, xin hãy kiểm tra liên tục thông tin sự kiện này để được cập nhật thêm");
                }
               
            }
            if (conferencePrice.AvailableSlot <= 0)
            {
                throw new BadRequestException($"Hiện tại slot cho hội nghị nghiên cứu này đã hết");
            }


            var paymentMethod = await _unitOfWork.PaymentMethodRepository.GetPaymentMethodByName(PaymentMethodEnum.MoMo.GetDescription());
            if (paymentMethod == null)
            {
                throw new NotFoundException($"Phương thức thanh toán không thể tìm thấy trong hệ thống");
            }


            var paymentLockKey = ExtensionHelper.GetPaymentLockKeyResult(userId, request.ConferencePriceId);
            bool paymentLockFound = await _redisService.KeyExistsAsync(paymentLockKey);
            if (paymentLockFound == true)
            {
                throw new BadRequestException("Bạn chưa thanh toán vé. Xin vui lòng đợi 120 phút nữa để mua lại vé!");
            }
            var ticketFound = await _unitOfWork.TicketRepository.GetTicketByUserIdAndConferencePriceId(userId, conferencePrice.ConferencePriceId);
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
                throw new BadRequestException("Tất cả các phase hợp lệ hiện tại đã hết slot");
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
            var result = await ProcessPaymentForAbstract(request, conferencePrice.ConferenceId, userId, finalPrice, paymentMethod.PaymentMethodId, sessionIdsList, $"Thanh toán abstract");
            return result;
        }
        public async Task<string> ProcessPaymentForAbstract(CreatePaperPaymentRequest request, string conferenceId, string userId, long amount, string paymentMethodId, List<string> conferenceSessionIds, string orderInfo)
        {
            var ticketId = Guid.NewGuid().ToString();
            var lockKey = ExtensionHelper.GetPaymentLockKeyResult(userId, request.ConferencePriceId);
            var transactionData = new TransactionDataHolder()
            {
                TicketId = ticketId,
                UserId = userId,
                PaymentMethodId = paymentMethodId,
                ConferencePriceId = request.ConferencePriceId,
                ConferenceSessionIds = conferenceSessionIds,
                ConferenceId = conferenceId,
                PaymentLockKey = lockKey,
                Title = request.Title,
                Description = request.Description,

            };
            var transacJson = JsonSerializer.Serialize(transactionData);

            var result = await CreateMomoPayment(ticketId, amount, orderInfo, _momoSettings.Value.IpnResearch, _momoSettings.Value.ResearchRedirectUrl);
            await _redisService.SetStringAsync(lockKey, "LockKey", TimeSpan.FromMinutes(120));
            await _redisService.SetStringAsync(ticketId, transacJson, TimeSpan.FromMinutes(120));
            return result.payUrl;
        }
        private async Task<MomoCreatePaymentResponse> CreateMomoPayment(string orderId, long amount, string orderInfo, string ipnUrl, string redirectUrl)
        {
            var rawSignature = "accessKey=" + _momoSettings.Value.AccessKey + "&amount=" + amount +
                "&extraData=" + _momoSettings.Value.ExtraData + "&ipnUrl=" + ipnUrl + "&orderId=" +
                orderId + "&orderInfo=" + orderInfo + "&partnerCode=" + _momoSettings.Value.PartnerCode +
                "&redirectUrl=" + redirectUrl + "&requestId=" + orderId + "&requestType=" + _momoSettings.Value.RequestType;
            var signature = _tokenService.CreateSignature(rawSignature, _momoSettings.Value.SecretKey);
            var requestBody = JsonSerializer.Serialize(new
            {
                partnerCode = _momoSettings.Value.PartnerCode,
                partnerName = "ConfRadar",
                storeId = "MomoTestStore",
                requestId = orderId,
                amount = amount,
                orderId = orderId,
                orderInfo = orderInfo,
                redirectUrl = redirectUrl,
                ipnUrl = ipnUrl,
                lang = _momoSettings.Value.Lang,
                requestType = _momoSettings.Value.RequestType,
                autoCapture = _momoSettings.Value.AutoCapture,
                extraData = _momoSettings.Value.ExtraData,
                signature = signature,
            });
            using var client = new HttpClient();
            using var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://test-payment.momo.vn/v2/gateway/api/create", content);
            var body = await response.Content.ReadAsStringAsync();
            var momoResponse = JsonSerializer.Deserialize<MomoCreatePaymentResponse>(body, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            });
            if (momoResponse.resultCode != 0)
            {
                throw new BadRequestException($"Error {momoResponse.message} with status code ${momoResponse.resultCode}");
            }
            return momoResponse;

        }


        public bool VerifyMomoPaymentData(MomoPaymentCallBackResponse data)
        {
            var raw_Response_Signature = $"accessKey={_momoSettings.Value.AccessKey}&amount={data.amount}&extraData={data.extraData}&message={data.message}&orderId={data.orderId}&orderInfo={data.orderInfo}&orderType={data.orderType}&partnerCode={data.partnerCode}&payType={data.payType}&requestId={data.requestId}&responseTime={data.responseTime}&resultCode={data.resultCode}&transId={data.transId}";
            var signature = _tokenService.CreateSignature(raw_Response_Signature, _momoSettings.Value.SecretKey);
            if (!string.Equals(signature, data.signature, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return true;
        }

        public async Task VerifyMomoPaymentDataForTechConference(MomoPaymentCallBackResponse data)
        {
            var result = VerifyMomoPaymentData(data);
            if (!result)
            {
                throw new BadRequestException("payment data is not valid");
            }
            var transacKey = await _redisService.KeyExistsAsync(data.orderId);
            if (!transacKey)
            {
                throw new BadRequestException("data is not valid");
            }
            var transac = await _redisService.GetStringAsync(data.orderId);
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
                ActualPrice = data.amount,
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
                Amount = data.amount,
                CreatedAt = timeNow,
                TransactionCode = data.transId.ToString(),
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
                    IsPresenter = true,
                    CheckinStatusId = checkInStatus.CheckinStatusId,
                    CheckInTime = null,
                    UserId = transacDataHolder.UserId,
                    TicketId = transacDataHolder.TicketId,
                    ConferenceSessionId = sessionId,

                };
                ticketObj.UserCheckIns.Add(userCheckInObj);
            }
            var pricePhase = await _unitOfWork.PricePhaseRepository.GetPricePhaseByConferencePriceIdAsync(transacDataHolder.ConferencePriceId);
            if (pricePhase.TotalSlot <= 0)
            {
                throw new BadRequestException("Hết slot");
            }
            pricePhase!.TotalSlot -= -1;
            pricePhase!.ConferencePrice!.AvailableSlot -= -1;
            pricePhase!.ConferencePrice!.Conference!.AvailableSlot -= -1;
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.PricePhaseRepository.UpdatePricePhaseAsync(pricePhase);
                await _unitOfWork.TicketRepository.CreateTicketAsync(ticketObj);
                await _redisService.DeleteKeyAsync(transacDataHolder.TicketId);
                await _redisService.DeleteKeyAsync(transacDataHolder.PaymentLockKey);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }


        }
        public async Task VerifyMomoPaymentDataForResearchConferenceAbstractSubmission(MomoPaymentCallBackResponse data)
        {

            var transacKey = await _redisService.KeyExistsAsync(data.orderId);
            if (!transacKey)
            {
                throw new NotFoundException("Dữ liệu không tìm thấy");
            }
            var result = VerifyMomoPaymentData(data);
            if (!result)
            {
                throw new BadRequestException("Dữ liệu payment không khả dụng");
            }
            var dateNow = ExtensionHelper.GetVietnamDate();
            var timeNow = ExtensionHelper.GetVietnamTime();
            var transac = await _redisService.GetStringAsync(data.orderId);
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
                ActualPrice = data.amount,
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
                Amount = data.amount,
                CreatedAt = timeNow,
                TransactionCode = data.transId.ToString(),
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
                    IsPresenter = true,
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
                IsPresenter = true,
                UserId = transacDataHolder.UserId,
                PaperId = paperObj.PaperId,
                IsRootAuthor = true,
            };
            paperObj.PaperAuthors.Add(presenterPaperAuthor);
            var pricePhase = await _unitOfWork.PricePhaseRepository.GetPricePhaseByConferencePriceIdAsync(transacDataHolder.ConferencePriceId);
            if (pricePhase.TotalSlot <= 0)
            {
                throw new BadRequestException("Hết slot");
            }
            pricePhase!.TotalSlot -= -1;
            pricePhase!.ConferencePrice!.AvailableSlot -= -1;
            pricePhase!.ConferencePrice!.Conference!.AvailableSlot -= -1;

            await _unitOfWork.BeginTransactionAsync();
            try
            {

                await _unitOfWork.PaperRepository.CreatePaperAsync(paperObj);
                await _unitOfWork.TicketRepository.CreateTicketAsync(ticketObj);
                await _unitOfWork.PricePhaseRepository.UpdatePricePhaseAsync(pricePhase);
                await _unitOfWork.CommitAsync();
                await _redisService.DeleteKeyAsync(data.orderId);
                await _redisService.DeleteKeyAsync(transacDataHolder.PaymentLockKey);

            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

        }


    }
}
