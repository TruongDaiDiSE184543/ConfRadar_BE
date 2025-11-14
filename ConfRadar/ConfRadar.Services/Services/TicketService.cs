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
        Task<List<PaidTicketResponse>> GetTicketListByConferenceId(string conferenceId);
        Task<int> CreateRefundTicketRequest(RefundTicketRequest request, string userId);
        Task<List<RefundRequestResponse>> GetRefundRequestByConferenceId(string conferenceId);
        Task<List<RefundRequestResponse>> GetAllRefundRequests();
        Task<int> RefundAuthorCloneFunction(string userId, string ticketId, string walletTransactionDescription);
    }
    public class TicketService : ITicketService
    {
        private readonly IUnitOfWork _unitOfWork;
        public TicketService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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


        public async Task<int> CreateRefundTicketRequest(RefundTicketRequest request, string userId)
        {
            var pendingGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            var acceptedGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription());
            var rejectedGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Rejected.GetDescription());


            var walletPaymentMethod = await _unitOfWork.PaymentMethodRepository.GetPaymentMethodByName(PaymentMethodEnum.Wallet.GetDescription());
            if (acceptedGlobalStatus == null || rejectedGlobalStatus == null || walletPaymentMethod == null || pendingGlobalStatus == null)
            {
                throw new NotFoundException("Không tìm thấy các trạng thái cho status trong hệ thống");
            }
            //validation dành riêng cho tìm kiếm entity 
            var ticket = await _unitOfWork.TicketRepository.GetTicketByTicketIdAndUserId(request.TicketId, userId);
            if (ticket == null)
            {
                throw new NotFoundException($"Không tìm thấy vé với mã {request.TicketId} cho bạn");
            }
            if (ticket.IsRefunded == true)
            {
                throw new BadRequestException($"Vé với mã {request.TicketId} đã được hoàn tiền rồi, nên bạn không thể yêu cầu hoàn tiền nữa!");
            }
            var transactionList = ticket.Transactions;
            if (transactionList.Count > 1)
            {
                throw new BadRequestException($"Bạn đã refund rồi không được yêu cầu refund nữa");
            }
            var transaction = transactionList.FirstOrDefault(t => t.TransactionId == request.TransactionId);
            if (transaction == null)
            {
                throw new NotFoundException($"Không tìm thấy mã giao dịch {request.TransactionId} tương ứng với vé {request.TicketId}");
            }
            if (ticket.PricePhase == null)
            {
                throw new BadRequestException("Không tìm phấy các giai đoạn vé cho vé này");
            }
            var refundRequestAlreadyExisted = await _unitOfWork.RefundRequestRepository.GetRefundRequestByTicketIdAsync(request.TicketId);
            if (refundRequestAlreadyExisted != null)
            {
                throw new BadRequestException($"Bạn đã gửi yêu cầu hoàn tiền vào ngày {refundRequestAlreadyExisted.CreatedAt}. Chúng tôi đề nghị bạn không được spam!");
            }
            //check để thêm data vô bảng refund request
            var dateNow = ExtensionHelper.GetVietnamDate();
            var dateTime = ExtensionHelper.GetVietnamTime();

            string refundRequestReason = string.Empty;

            decimal refundAmount = 0;
            //cho init ban đầu là accept, qua từng filter , nào ko hợp => lỗi
            
            var refundPolicies = ticket.PricePhase.RefundPolicies;
            if (!refundPolicies.Any())
            {
                throw new BadRequestException("Không có chính sách hoàn tiền cho vé này");
            }
            var validPolicy = refundPolicies
                    .OrderBy(r => r.RefundDeadline)
                    .FirstOrDefault(rp => rp.RefundDeadline >= dateNow);
            if (validPolicy == null)
            {
                throw new BadRequestException("Tất cả chính sách hoàn tiền đã quá hạn");
            }
            var isAuthorTicket = ticket.PricePhase.ConferencePrice.IsAuthor;
            if (isAuthorTicket == true)
            {
                var purchasedPaper = await _unitOfWork.PaperRepository.GetPaperByUserAndConference(ticket.PricePhase.ConferencePrice.ConferenceId, userId);
                if (purchasedPaper != null)
                {
                    var abstractPaperDetail = purchasedPaper.Abstract;
                    // nếu abstract chưa nộp=> hoàn 100%
                    if (abstractPaperDetail != null && abstractPaperDetail.GlobalStatus != pendingGlobalStatus)
                    {
                        throw new BadRequestException("Abstract đã được xét duyệt, không thể refund");
                    }
                }
            }
            refundRequestReason = $"Hoàn tiền theo refund policy {validPolicy.PercentRefund}% trước hạn refund deadline: {validPolicy.RefundDeadline}";
            refundAmount = (decimal)(transaction.Amount * validPolicy.PercentRefund / 100);
            int result = 0;
            await _unitOfWork.BeginTransactionAsync();
            try
            {


                //logic + tiền về lại ví


                var userWallet = await _unitOfWork.WalletRepository.GetWalletByUserIdAsync(userId);
                if (userWallet == null)
                {
                    throw new NotFoundException("Không tìm thấy ví của bạn");
                }
                userWallet.UpdatedAt = dateTime;
                userWallet.Balance = userWallet.Balance + refundAmount;
                result += await _unitOfWork.WalletRepository.UpdateWalletAsync(userWallet);


                //logic thêm biến động giao dịch cho ví
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
                //logic nhả lại slot cho conference
                var pricePhase = await _unitOfWork.PricePhaseRepository.GetPricePhaseByPricePhaseId(ticket.PricePhaseId!);
                if (pricePhase == null)
                {
                    throw new NotFoundException($"Không tìm price phase với id {ticket.PricePhaseId}");
                }
                pricePhase.AvailableSlot = pricePhase.AvailableSlot + 1;
                pricePhase.ConferencePrice!.AvailableSlot = pricePhase.ConferencePrice!.AvailableSlot + 1;
                pricePhase.ConferencePrice!.Conference!.AvailableSlot = pricePhase.ConferencePrice!.Conference!.AvailableSlot + 1;
                result += await _unitOfWork.PricePhaseRepository.UpdatePricePhaseAsync(pricePhase);

                //logic chuyển ticket refund =true
                ticket.IsRefunded = true;

                //logic thêm transaction với dạng là refund
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
            var dateTime = ExtensionHelper.GetVietnamTime();
            var dateNow = ExtensionHelper.GetVietnamDate();
            var walletPaymentMethod = await _unitOfWork.PaymentMethodRepository.GetPaymentMethodByName(PaymentMethodEnum.Wallet.GetDescription());
            var abstractPaperPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.Abstract.GetDescription());
            if (walletPaymentMethod == null || abstractPaperPhase == null)
            {
                throw new NotFoundException("Không tìm thấy các trạng thái  trong hệ thống");
            }
            var ticket = await _unitOfWork.TicketRepository.GetTicketByTicketIdAndUserId(ticketId, userId);
            if (ticket == null)
            {
                throw new NotFoundException($"Không tìm thấy ticket với id {ticketId}");
            }
            var paper = await _unitOfWork.PaperRepository.GetPaperByUserAndConference(ticket.PricePhase.ConferencePrice.ConferenceId, userId);
            if (paper == null)
            {
                throw new NotFoundException($"Không tìm thấy bài báo của user với id {userId}");
            }
            var transaction = ticket.Transactions.FirstOrDefault(t => t.IsRefunded == false);
            if (transaction == null)
            {
                throw new BadRequestException("Không tìm thấy transaction hợp lệ để hoàn tiền");
            }
            decimal refundAmount = 0;

            var reviewFee = ticket.PricePhase!.ConferencePrice!.Conference!.ResearchConferenceDetail!.ReviewFee;

            refundAmount = (decimal)(transaction.Amount - reviewFee);
            walletTransactionDescription = walletTransactionDescription + $" .Bạn được hoàn tiền {refundAmount} và đã bao gồm phí review : {reviewFee}";





            var userWallet = await _unitOfWork.WalletRepository.GetWalletByUserIdAsync(userId);
            if (userWallet == null)
            {
                throw new NotFoundException("Không tìm thấy ví của bạn");
            }
            userWallet.UpdatedAt = dateTime;
            userWallet.Balance = userWallet.Balance + refundAmount;
            result += await _unitOfWork.WalletRepository.UpdateWalletAsync(userWallet);


            //logic thêm biến động giao dịch cho ví
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
            //logic nhả lại slot cho conference
            var pricePhase = await _unitOfWork.PricePhaseRepository.GetPricePhaseByPricePhaseId(ticket.PricePhaseId!);
            if (pricePhase == null)
            {
                throw new NotFoundException($"Không tìm price phase với id {ticket.PricePhaseId}");
            }
            pricePhase.AvailableSlot = pricePhase.AvailableSlot + 1;
            pricePhase.ConferencePrice!.AvailableSlot = pricePhase.ConferencePrice!.AvailableSlot + 1;
            pricePhase.ConferencePrice!.Conference!.AvailableSlot = pricePhase.ConferencePrice!.Conference!.AvailableSlot + 1;
            result += await _unitOfWork.PricePhaseRepository.UpdatePricePhaseAsync(pricePhase);

            //logic chuyển ticket refund =true
            ticket.IsRefunded = true;

            //logic thêm transaction với dạng là refund
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
