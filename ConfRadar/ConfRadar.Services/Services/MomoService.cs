using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.Payment;
using ConfRadar.Services.Exceptions;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using static ConfRadar.Services.Common.AppSettingConfig;

namespace ConfRadar.Services.Services
{
    public interface IMomoService
    {
        Task<string> HandleMomoPaymentWithTechConf(CreateTechPaymentRequest request, string userId);
        Task VerifyMomoPaymentDataWithTechConf(MomoPaymentCallBackResponse data);
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
                throw new ConfRadarAuthenticationException("User not found");
            }
            var conferencePrice = await _unitOfWork.ConferencePriceRepository.GetConferencePriceByConferencePriceId(request.ConferencePriceId);
            if (conferencePrice == null)
            {
                throw new BadRequestException("Conference price not found");
            }
            if (conferencePrice.Conference?.Capacity == 0)
            {
                throw new BadRequestException($"{conferencePrice.Conference?.ConferenceName} is sold out!");
            }
            var ticket = await _unitOfWork.TicketRepository.GetTicketByUserIdAndConferencePriceId(userId, request.ConferencePriceId);
            if (ticket != null)
            {
                throw new BadRequestException("You have already purchase ticket!");
            }
            var dateNow = DateOnly.FromDateTime(DateTime.UtcNow);
            int discountPercent = 0;
            var phase = conferencePrice.PricePhase;
            if (phase == null)
            {
                throw new BadRequestException("Phase is not available");
            }
            if (dateNow <= phase.EarlierBirdEndInterval)
            {
                discountPercent = phase.PercentForEarly ?? 0;
            }
            else if (dateNow <= phase.StandardEndInterval)
            {
                discountPercent = 0;
            }
            else if (dateNow <= phase.LateEndInterval)
            {
                discountPercent = phase.PercentForEnd ?? 0;
            }
            else
            {
                throw new BadRequestException("Price phase is not available!");
            }

            var moneyAmount = conferencePrice.ActualPrice - (conferencePrice.ActualPrice * discountPercent / 100);
            if (moneyAmount < 10000 || moneyAmount > 50000000)
            {
                throw new BadRequestException("Money amount must between 10000 - 50000000");
            }
            var finalAmount = (long)Math.Round(moneyAmount ?? 0);
            var transactionStatus = await _unitOfWork.TransactionStatusRepository.GetTransactionStatusByName(TransactionStatusEnum.Pending.GetDescription());
            var transactionType = await _unitOfWork.TransactionTypeRepository.GetTransactionTypeByName(TransactionTypeEnum.Payment.GetDescription());
            var paymentMethod = await _unitOfWork.PaymentMethodRepository.GetPaymentMethodByName(PaymentMethodEnum.MoMo.GetDescription());
            List<string> sessionIds = new List<string>();
            foreach (var sessionId in conferencePrice.Conference!.ConferenceSessions)
            {
                sessionIds.Add(sessionId.ConferenceSessionId);
            }
            string transactionId = Guid.NewGuid().ToString();
            var transactionData = new TransactionDataHolder()
            {
                PaymentMethodId = paymentMethod!.PaymentMethodId,
                TransactionId = transactionId,
                TransactionStatusId = transactionStatus!.TransactionStatusId,
                TransactionTypeId = transactionType!.TransactionTypeId,
                UserId = user.UserId,
                ConferencePriceId = request.ConferencePriceId,
                ConferenceSessionIds = sessionIds,
            };
            var transacJson = JsonSerializer.Serialize(transactionData);
            await _redisService.SetStringAsync(transactionId, transacJson, TimeSpan.FromMinutes(120));
            var result = await CreateMomoPayment(transactionId, finalAmount, "Payment for tech conf");
            return result.payUrl;
        }
        private async Task<MomoCreatePaymentResponse> CreateMomoPayment(string orderId, long amount, string orderInfo)
        {

            var rawSignature = "accessKey=" + _momoSettings.Value.AccessKey + "&amount=" + amount +
                "&extraData=" + _momoSettings.Value.ExtraData + "&ipnUrl=" + _momoSettings.Value.IpnUrl + "&orderId=" +
                orderId + "&orderInfo=" + orderInfo + "&partnerCode=" + _momoSettings.Value.PartnerCode +
                "&redirectUrl=" + _momoSettings.Value.RedirectUrl + "&requestId=" + orderId + "&requestType=" + _momoSettings.Value.RequestType;
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
                redirectUrl = _momoSettings.Value.RedirectUrl,
                ipnUrl = _momoSettings.Value.IpnUrl,
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

        public async Task VerifyMomoPaymentDataWithTechConf(MomoPaymentCallBackResponse data)
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
            var successTransactionStatus = await _unitOfWork.TransactionStatusRepository.GetTransactionStatusByName(TransactionStatusEnum.Success.GetDescription());
            var timeNow = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            var listUserCheckIn = new List<UserCheckIn>();
            var ticketId = Guid.NewGuid().ToString();
            foreach (var sessionId in transacDataHolder.ConferenceSessionIds)
            {
                var userCheckIn = new UserCheckIn()
                {
                    UserCheckInId = Guid.NewGuid().ToString(),
                    IsPresenter = false,
                    HasCheckIn = false,
                    CheckInTime = null,
                    ConferenceSessionId = sessionId,
                    UserId = transacDataHolder.UserId,
                    TicketId = ticketId
                };
                listUserCheckIn.Add(userCheckIn);
            }
            var transactionObj = new Transaction()
            {
                TransactionId = transacDataHolder.TransactionId,
                UserId = transacDataHolder.UserId,
                Currency = "VND",
                Amount = data.amount,
                TransactionCode = data.transId.ToString(),
                CreatedAt = timeNow,
                TransactionStatusId = successTransactionStatus?.TransactionStatusId,
                TransactionTypeId = transacDataHolder.TransactionTypeId,
                PaymentMethodId = transacDataHolder.PaymentMethodId,
                Ticket = new Ticket()
                {
                    TicketId = ticketId,
                    UserId = transacDataHolder.UserId,
                    ConferencePriceId = transacDataHolder.ConferencePriceId,
                    TransactionId = transacDataHolder.TransactionId,
                    RegisteredDate = timeNow,
                    IsRefunded = false,
                    ActualPrice = data.amount,
                    UserCheckIns = listUserCheckIn
                },
            };
            await _unitOfWork.TransactionRepository.CreateTransactionAsync(transactionObj);
            await _redisService.DeleteKeyAsync(data.orderId);

        }
    }
}
