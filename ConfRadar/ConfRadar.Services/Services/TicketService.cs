using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.Ticket;
using ConfRadar.Services.Exceptions;
using ConfRadar.Shared.DTO.General;
using ConfRadar.Shared.DTO.RefundRequest;
using ConfRadar.Shared.DTO.Ticket;
using System.Data;

namespace ConfRadar.Services.Services
{
    public interface ITicketService
    {
        Task<PagedResultResponseDto<CustomerPaidTicketResponse>> GetTicketsByUserId(string userId, string? keyword, int? pageNumber = 1, int? pageSize = 10, DateTime? sessionStartTime = null, DateTime? sessionEndTime = null);
        Task<PagedResultResponseDto<CustomerPaidTicketResponse>> GetTicketsByUserIdAndConferenceId(string conferenceId, string userId, string? keyword, int? pageNumber = 1, int? pageSize = 10, DateTime? sessionStartTime = null, DateTime? sessionEndTime = null);
        Task<List<PaidTicketResponse>> GetTicketListByConferenceId(string conferenceId);
        Task<int> CreateRefundTicketRequest(RefundTicketRequest request, string userId);
        Task<List<RefundRequestResponse>> GetRefundRequestByConferenceId(string conferenceId);
        Task<List<RefundRequestResponse>> GetAllRefundRequests();
        Task<int> RefundAuthorCloneFunction(string userId, string ticketId, string walletTransactionDescription);
    }
    public class TicketService : ITicketService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITimeProviderService _timeProviderService;
        public TicketService(IUnitOfWork unitOfWork, ITimeProviderService timeProviderService)
        {
            _unitOfWork = unitOfWork;
            _timeProviderService = timeProviderService;
        }



        public async Task<List<PaidTicketResponse>> GetTicketListByConferenceId(string conferenceId)
        {
            var tickets = await _unitOfWork.TicketRepository.GetTicketListByConferenceId(conferenceId);
            return tickets.Select(x => new PaidTicketResponse()
            {
                TicketId = x.TicketId,
                UserId = x.UserId,
                IsRefunded = x.IsRefunded,
                UserName = x.User?.FullName ?? null,
                Email = x.User?.Email ?? null,
                AvatarUrl = x.User?.AvatarUrl ?? null,
                RegisteredDate = x.RegisteredDate ?? null,
                ConferenceId = x.PricePhase?.ConferencePrice?.ConferenceId ?? null,
                ConferenceName = x.PricePhase?.ConferencePrice?.Conference?.ConferenceName ?? null,
            }).ToList();
        }

        public async Task<PagedResultResponseDto<CustomerPaidTicketResponse>> GetTicketsByUserId(string userId, string? keyword, int? pageNumber = 1, int? pageSize = 10, DateTime? sessionStartTime = null, DateTime? sessionEndTime = null)
        {
            int page = pageNumber ?? 1;
            int size = pageSize ?? 10;
            return await _unitOfWork.TicketRepository.GetTicketsByUserId(userId, keyword, page, size, sessionStartTime, sessionEndTime);
        }
        public async Task<PagedResultResponseDto<CustomerPaidTicketResponse>> GetTicketsByUserIdAndConferenceId(string conferenceId,string userId, string? keyword, int? pageNumber = 1, int? pageSize = 10, DateTime? sessionStartTime = null, DateTime? sessionEndTime = null)
        {
            int page = pageNumber ?? 1;
            int size = pageSize ?? 10;
            return await _unitOfWork.TicketRepository.GetTicketsByUserIdAndConferenceId(conferenceId,userId, keyword, page, size, sessionStartTime, sessionEndTime);
        }

        public async Task<int> CreateRefundTicketRequest(RefundTicketRequest request, string userId)
        {
            var pendingGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            var acceptedGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription());
            var rejectedGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Rejected.GetDescription());


            var walletPaymentMethod = await _unitOfWork.PaymentMethodRepository.GetPaymentMethodByName(PaymentMethodEnum.Wallet.GetDescription());
            if (acceptedGlobalStatus == null || rejectedGlobalStatus == null || walletPaymentMethod == null || pendingGlobalStatus == null)
            {
                throw new NotFoundException("Không tìm th?y các tr?ng thái cho status trong h? th?ng");
            }
            //validation dành riêng cho tìm ki?m entity 
            var ticket = await _unitOfWork.TicketRepository.GetTicketByTicketIdAndUserId(request.TicketId, userId);
            if (ticket == null)
            {
                throw new NotFoundException($"Không tìm th?y vé v?i mã {request.TicketId} cho b?n");
            }
            if (ticket.IsRefunded == true)
            {
                throw new BadRequestException($"Vé v?i mã {request.TicketId} dã du?c hoàn ti?n r?i, nên b?n không th? yêu c?u hoàn ti?n n?a!");
            }
            var transactionList = ticket.Transactions;
            if (transactionList.Count > 1)
            {
                throw new BadRequestException($"B?n dã refund r?i không du?c yêu c?u refund n?a");
            }
            var transaction = transactionList.FirstOrDefault(t => t.TransactionId == request.TransactionId);
            if (transaction == null)
            {
                throw new NotFoundException($"Không tìm th?y mã giao d?ch {request.TransactionId} tuong ?ng v?i vé {request.TicketId}");
            }
            if (ticket.PricePhase == null)
            {
                throw new BadRequestException("Không tìm ph?y các giai do?n vé cho vé này");
            }
            var refundRequestAlreadyExisted = await _unitOfWork.RefundRequestRepository.GetRefundRequestByTicketIdAsync(request.TicketId);
            if (refundRequestAlreadyExisted != null)
            {
                throw new BadRequestException($"B?n dã g?i yêu c?u hoàn ti?n vào ngày {refundRequestAlreadyExisted.CreatedAt}. Chúng tôi d? ngh? b?n không du?c spam!");
            }
            //check d? thêm data vô b?ng refund request
            var dateNow = await _timeProviderService.GetVietnamDate();
            var dateTime = await _timeProviderService.GetVietnamTime();

            string refundRequestReason = string.Empty;

            decimal refundAmount = 0;
            //cho init ban d?u là accept, qua t?ng filter , nào ko h?p => l?i

            var refundPolicies = ticket.PricePhase.RefundPolicies;
            if (!refundPolicies.Any())
            {
                throw new BadRequestException("Không có chính sách hoàn ti?n cho vé này");
            }
            var validPolicy = refundPolicies
                    .OrderBy(r => r.RefundDeadline)
                    .FirstOrDefault(rp => rp.RefundDeadline >= dateNow);
            if (validPolicy == null)
            {
                throw new BadRequestException("T?t c? chính sách hoàn ti?n dã quá h?n");
            }
            var isAuthorTicket = ticket.PricePhase.ConferencePrice.IsAuthor;
            if (isAuthorTicket == true)
            {
                var purchasedPaper = await _unitOfWork.PaperRepository.GetPaperByUserAndConference(ticket.PricePhase.ConferencePrice.ConferenceId, userId);
                if (purchasedPaper != null)
                {
                    var abstractPaperDetail = purchasedPaper.Abstract;
                    // n?u abstract chua n?p=> hoàn 100%
                    if (abstractPaperDetail != null && abstractPaperDetail.GlobalStatus != pendingGlobalStatus)
                    {
                        throw new BadRequestException("Abstract dã du?c xét duy?t, không th? refund");
                    }
                }
            }
            refundRequestReason = $"Hoàn ti?n theo refund policy {validPolicy.PercentRefund}% tru?c h?n refund deadline: {validPolicy.RefundDeadline}";
            refundAmount = (decimal)(transaction.Amount * validPolicy.PercentRefund / 100);
            int result = 0;
            await _unitOfWork.BeginTransactionAsync();
            try
            {


                //logic + ti?n v? l?i ví


                var userWallet = await _unitOfWork.WalletRepository.GetWalletByUserIdAsync(userId);
                if (userWallet == null)
                {
                    throw new NotFoundException("Không tìm th?y ví c?a b?n");
                }
                userWallet.UpdatedAt = dateTime;
                userWallet.Balance = userWallet.Balance + refundAmount;
                result += await _unitOfWork.WalletRepository.UpdateWalletAsync(userWallet);


                //logic thêm bi?n d?ng giao d?ch cho ví
                var userWalletTransactionObj = new WalletTransaction()
                {
                    WalletTransactionId = Guid.NewGuid().ToString(),
                    WalletId = userWallet.WalletId,
                    Amount = refundAmount,
                    TransactionType = WalletTransactionTypeEnum.Refund.GetDescription(),
                    Description = refundRequestReason,
                    CreatedAt = dateTime,
                };
                result += await _unitOfWork.WalletTransactionRepository.CreateWalletTransactionAsync(userWalletTransactionObj);
                //logic nh? l?i slot cho conference
                var pricePhase = await _unitOfWork.PricePhaseRepository.GetPricePhaseByPricePhaseId(ticket.PricePhaseId!);
                if (pricePhase == null)
                {
                    throw new NotFoundException($"Không tìm price phase v?i id {ticket.PricePhaseId}");
                }
                pricePhase.AvailableSlot = pricePhase.AvailableSlot + 1;
                pricePhase.ConferencePrice!.AvailableSlot = pricePhase.ConferencePrice!.AvailableSlot + 1;
                pricePhase.ConferencePrice!.Conference!.AvailableSlot = pricePhase.ConferencePrice!.Conference!.AvailableSlot + 1;
                result += await _unitOfWork.PricePhaseRepository.UpdatePricePhaseAsync(pricePhase);

                //logic chuy?n ticket refund =true
                ticket.IsRefunded = true;

                //logic thêm transaction v?i d?ng là refund
                var transactionId = Guid.NewGuid().ToString();
                var transactionObj = new Transaction()
                {
                    TransactionId = transactionId,
                    UserId = userId,
                    Currency = "VND",
                    Amount = +refundAmount,
                    CreatedAt = dateTime,
                    IsRefunded = true,
                    PaymentMethodId = walletPaymentMethod.PaymentMethodId,
                    TicketId = ticket.TicketId,
                };

                result += await _unitOfWork.TransactionRepository.CreateTransactionAsync(transactionObj);
                var refundRequestObj = new RefundRequest()
                {
                    RefundRequestId = Guid.NewGuid().ToString(),
                    TransactionId = request.TransactionId,
                    TicketId = request.TicketId,
                    Reason = refundRequestReason,
                    CreatedAt = dateTime,
                    GlobalStatus = acceptedGlobalStatus,
                };
                result += await _unitOfWork.TicketRepository.UpdateTicketAsync(ticket);
                result += await _unitOfWork.RefundRequestRepository.CreateRefundRequestAsync(refundRequestObj);
                await _unitOfWork.CommitAsync();
                return result;


            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<List<RefundRequestResponse>> GetRefundRequestByConferenceId(string conferenceId)
        {
            return await _unitOfWork.RefundRequestRepository.GetRefundRequestByConferenceId(conferenceId);
        }

        public async Task<List<RefundRequestResponse>> GetAllRefundRequests()
        {
            return await _unitOfWork.RefundRequestRepository.GetAllRefundRequest();
        }

        public async Task<int> RefundAuthorCloneFunction(string userId, string ticketId, string walletTransactionDescription)
        {
            int result = 0;
            var dateTime = await _timeProviderService.GetVietnamTime();
            var dateNow = await _timeProviderService.GetVietnamDate();
            var walletPaymentMethod = await _unitOfWork.PaymentMethodRepository.GetPaymentMethodByName(PaymentMethodEnum.Wallet.GetDescription());
            var abstractPaperPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.Abstract.GetDescription());
            if (walletPaymentMethod == null || abstractPaperPhase == null)
            {
                throw new NotFoundException("Không tìm th?y các tr?ng thái  trong h? th?ng");
            }
            var ticket = await _unitOfWork.TicketRepository.GetTicketByTicketIdAndUserId(ticketId, userId);
            if (ticket == null)
            {
                throw new NotFoundException($"Không tìm th?y ticket v?i id {ticketId}");
            }
            var paper = await _unitOfWork.PaperRepository.GetPaperByUserAndConference(ticket.PricePhase.ConferencePrice.ConferenceId, userId);
            if (paper == null)
            {
                throw new NotFoundException($"Không tìm th?y bài báo c?a user v?i id {userId}");
            }
            var transaction = ticket.Transactions.FirstOrDefault(t => t.IsRefunded == false);
            if (transaction == null)
            {
                throw new BadRequestException("Không tìm th?y transaction h?p l? d? hoàn ti?n");
            }
            decimal refundAmount = 0;

            var reviewFee = ticket.PricePhase!.ConferencePrice!.Conference!.ResearchConferenceDetail!.ReviewFee;

            refundAmount = (decimal)(transaction.Amount - reviewFee);
            walletTransactionDescription = walletTransactionDescription + $" .B?n du?c hoàn ti?n {refundAmount} và dã bao g?m phí review : {reviewFee}";





            var userWallet = await _unitOfWork.WalletRepository.GetWalletByUserIdAsync(userId);
            if (userWallet == null)
            {
                throw new NotFoundException("Không tìm th?y ví c?a b?n");
            }
            userWallet.UpdatedAt = dateTime;
            userWallet.Balance = userWallet.Balance + refundAmount;
            result += await _unitOfWork.WalletRepository.UpdateWalletAsync(userWallet);


            //logic thêm bi?n d?ng giao d?ch cho ví
            var userWalletTransactionObj = new WalletTransaction()
            {
                WalletTransactionId = Guid.NewGuid().ToString(),
                WalletId = userWallet.WalletId,
                Amount = refundAmount,
                TransactionType = WalletTransactionTypeEnum.Refund.GetDescription(),
                Description = walletTransactionDescription,
                CreatedAt = dateTime,
            };
            result += await _unitOfWork.WalletTransactionRepository.CreateWalletTransactionAsync(userWalletTransactionObj);
            //logic nh? l?i slot cho conference
            var pricePhase = await _unitOfWork.PricePhaseRepository.GetPricePhaseByPricePhaseId(ticket.PricePhaseId!);
            if (pricePhase == null)
            {
                throw new NotFoundException($"Không tìm price phase v?i id {ticket.PricePhaseId}");
            }
            pricePhase.AvailableSlot = pricePhase.AvailableSlot + 1;
            pricePhase.ConferencePrice!.AvailableSlot = pricePhase.ConferencePrice!.AvailableSlot + 1;
            pricePhase.ConferencePrice!.Conference!.AvailableSlot = pricePhase.ConferencePrice!.Conference!.AvailableSlot + 1;
            result += await _unitOfWork.PricePhaseRepository.UpdatePricePhaseAsync(pricePhase);

            //logic chuy?n ticket refund =true
            ticket.IsRefunded = true;

            //logic thêm transaction v?i d?ng là refund
            var transactionId = Guid.NewGuid().ToString();
            var transactionObj = new Transaction()
            {
                TransactionId = transactionId,
                UserId = userId,
                Currency = "VND",
                Amount = +refundAmount,
                CreatedAt = dateTime,
                IsRefunded = true,
                PaymentMethodId = walletPaymentMethod.PaymentMethodId,
                TicketId = ticket.TicketId,
            };

            result += await _unitOfWork.TransactionRepository.CreateTransactionAsync(transactionObj);
            result += await _unitOfWork.TicketRepository.UpdateTicketAsync(ticket);

            return result;
        }
    }
}
