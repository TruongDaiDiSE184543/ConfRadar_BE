using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.Ticket;
using ConfRadar.Services.Exceptions;
using ConfRadar.Shared.DTO.General;
using ConfRadar.Shared.DTO.Ticket;

namespace ConfRadar.Services.Services
{
    public interface ITicketService
    {
        Task<PagedResultResponseDto<CustomerPaidTicketResponse>> GetTicketsByUserId(string userId, string? keyword, int? pageNumber = 1, int? pageSize = 10, DateTime? sessionStartTime = null, DateTime? sessionEndTime = null);
        Task<List<PaidTicketResponse>> GetTicketListByConferenceId(string conferenceId);
        Task<int> CreateRefundTicketRequest(RefundTicketRequest request, string userId);
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

            var acceptedGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription());
            var rejectedGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Rejected.GetDescription());
            var walletPaymentMethod = await _unitOfWork.PaymentMethodRepository.GetPaymentMethodByName(PaymentMethodEnum.Wallet.GetDescription());
            if (acceptedGlobalStatus == null || rejectedGlobalStatus == null || walletPaymentMethod==null)
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
            GlobalStatus finalGlobalStatus = acceptedGlobalStatus;
            //cho init ban đầu là accept, qua từng filter , nào ko hợp => reject
            var refundPolicies = ticket.PricePhase.RefundPolicies;


            if (refundPolicies.Count <= 0)
            {
                refundRequestReason = "Xin lỗi bạn, hiện tại chính sách này không cho phép được hoàn tiền";
                finalGlobalStatus = rejectedGlobalStatus;
            }

            var validRefundPolicy = refundPolicies.FirstOrDefault(rp => rp.RefundDeadline >= dateNow);
            if (validRefundPolicy == null)
            {
                refundRequestReason = "Xin lỗi bạn, hiện tại bạn đã quá hạn các chính sách hoàn tiền trong hệ thống.";
                finalGlobalStatus = rejectedGlobalStatus;
            }
            else
            {
                refundRequestReason = $"Đã hoàn tiền {validRefundPolicy.PercentRefund}% trước hạn {validRefundPolicy.RefundDeadline}.Vui lòng kiểm tra transaction";
                finalGlobalStatus = acceptedGlobalStatus;
            }
            int result=0;
            await _unitOfWork.BeginTransactionAsync();
            try
            {
               
                if (finalGlobalStatus == acceptedGlobalStatus)
                {
                    //logic + tiền về lại ví

                    var refundAmount = (decimal)(transaction.Amount * validRefundPolicy!.PercentRefund / 100);
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
                        Description = "Bạn đã hoàn tiền cho vé",
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
                }else
                {
                    ticket.IsRefunded = false;
                }
                var refundRequestObj = new RefundRequest()
                {
                    RefundRequestId = Guid.NewGuid().ToString(),
                    TransactionId = request.TransactionId,
                    TicketId = request.TicketId,
                    Reason = refundRequestReason,
                    CreatedAt = dateTime,
                    GlobalStatus = finalGlobalStatus,
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
    }
}
