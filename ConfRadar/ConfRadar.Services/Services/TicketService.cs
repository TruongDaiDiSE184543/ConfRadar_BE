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

        Task<int> CancelTechTickets(CancelTechnicalTickets tickets, string userId);
        Task<int> CancelResearchTickets(CancelResearchTickets tickets, string userId);

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
        public async Task<PagedResultResponseDto<CustomerPaidTicketResponse>> GetTicketsByUserIdAndConferenceId(string conferenceId, string userId, string? keyword, int? pageNumber = 1, int? pageSize = 10, DateTime? sessionStartTime = null, DateTime? sessionEndTime = null)
        {
            int page = pageNumber ?? 1;
            int size = pageSize ?? 10;
            return await _unitOfWork.TicketRepository.GetTicketsByUserIdAndConferenceId(conferenceId, userId, keyword, page, size, sessionStartTime, sessionEndTime);
        }

        public async Task<int> CreateRefundTicketRequest(RefundTicketRequest request, string userId)
        {
            var pendingGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            var acceptedGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription());
            var rejectedGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Rejected.GetDescription());


            var walletPaymentMethod = await _unitOfWork.PaymentMethodRepository.GetPaymentMethodByName(PaymentMethodEnum.Wallet.GetDescription());
            if (acceptedGlobalStatus == null || rejectedGlobalStatus == null || walletPaymentMethod == null || pendingGlobalStatus == null)
            {
                throw new NotFoundException("Không tìm thấy các status trong hệ thống");
            }
            //validation dành riêng cho tìm kiếm entity 
            var ticket = await _unitOfWork.TicketRepository.GetTicketByTicketIdAndUserId(request.TicketId, userId);
            if (ticket == null)
            {
                throw new NotFoundException($"Không tìm thấy vé với mã {request.TicketId} cho bạn");
            }
            if (ticket.IsRefunded == true)
            {
                throw new BadRequestException($"Vé với mã {request.TicketId} đã được hoàn tiền. Không thể yêu cầu hoàn tiền nữa");
            }
            var transactionList = ticket.Transactions;
            if (transactionList.Count > 1)
            {
                throw new BadRequestException($"Bạn đã refund trước đó rồi");
            }
            var transaction = transactionList.FirstOrDefault(t => t.TransactionId == request.TransactionId);
            if (transaction == null)
            {
                throw new NotFoundException($"Không tìm thấy mã giao dịch {request.TransactionId} tương ứng với vé {request.TicketId}");
            }
            if (ticket.PricePhase == null)
            {
                throw new BadRequestException("Không tìm thấy các giai đoạn cho vé này");
            }
            var refundRequestAlreadyExisted = await _unitOfWork.RefundRequestRepository.GetRefundRequestByTicketIdAsync(request.TicketId);
            if (refundRequestAlreadyExisted != null)
            {
                throw new BadRequestException($"Bạn có yêu hoàn tiền vào ngày {refundRequestAlreadyExisted.CreatedAt}! trước đó");
            }

            var dateNow = await _timeProviderService.GetVietnamDate();
            var dateTime = await _timeProviderService.GetVietnamTime();

            string refundRequestReason = string.Empty;

            decimal refundAmount = 0;

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
                throw new BadRequestException("Tất cả các vé hoàn tiền đã quá hạn");
            }
            var isAuthorTicket = ticket.PricePhase.ConferencePrice.IsAuthor;
            if (isAuthorTicket == true)
            {
                var purchasedPaper = await _unitOfWork.PaperRepository.GetPaperByUserAndConference(ticket.PricePhase.ConferencePrice.ConferenceId, userId);
                if (purchasedPaper != null)
                {
                    var abstractPaperDetail = purchasedPaper.Abstract;

                    if (abstractPaperDetail != null && abstractPaperDetail.GlobalStatus != pendingGlobalStatus)
                    {
                        throw new BadRequestException("Abstract đã được xét, duyệt không thể refund");
                    }
                }
            }
            refundRequestReason = $"Hoàn tiền theo refund policy {validPolicy.PercentRefund}% theo refund deadline: {validPolicy.RefundDeadline}";
            refundAmount = (decimal)(transaction.Amount * validPolicy.PercentRefund / 100);
            int result = 0;
            await _unitOfWork.BeginTransactionAsync();
            try
            {


                //logic hoàn tiền về ví


                var userWallet = await _unitOfWork.WalletRepository.GetWalletByUserIdAsync(userId);
                if (userWallet == null)
                {
                    throw new NotFoundException("Không tìm thấy ví");
                }
                userWallet.UpdatedAt = dateTime;
                userWallet.Balance = userWallet.Balance + refundAmount;
                result += await _unitOfWork.WalletRepository.UpdateWalletAsync(userWallet);


                //biến động giao dịch
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
                //logic trả slot
                var pricePhase = await _unitOfWork.PricePhaseRepository.GetPricePhaseByPricePhaseId(ticket.PricePhaseId!);
                if (pricePhase == null)
                {
                    throw new NotFoundException($"Không tìm price phase vớii id {ticket.PricePhaseId}");
                }
                pricePhase.AvailableSlot = pricePhase.AvailableSlot + 1;
                pricePhase.ConferencePrice!.AvailableSlot = pricePhase.ConferencePrice!.AvailableSlot + 1;
                pricePhase.ConferencePrice!.Conference!.AvailableSlot = pricePhase.ConferencePrice!.Conference!.AvailableSlot + 1;
                result += await _unitOfWork.PricePhaseRepository.UpdatePricePhaseAsync(pricePhase);

                //logic ticket refund =true
                ticket.IsRefunded = true;

                //logic thêm transaction dưới dạng refund
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
                throw new NotFoundException("Không tìm thấy các trạng thái trong hệ thống");
            }
            var ticket = await _unitOfWork.TicketRepository.GetTicketByTicketIdAndUserId(ticketId, userId);
            if (ticket == null)
            {
                throw new NotFoundException($"Không tìm bài báo với ticket với id {ticketId}");
            }
            var transactionList = ticket.Transactions;
            if (transactionList.Count > 1)
            {
                throw new BadRequestException($"Vé này đã được refund");
            }
            var transaction = ticket.Transactions.FirstOrDefault(t => t.IsRefunded == false);
            if (transaction == null)
            {
                throw new BadRequestException("Không tìm thấy transaction hợp lệ để hoàn tiền");
            }
            decimal refundAmount = 0;

            var reviewFee = ticket.PricePhase!.ConferencePrice!.Conference!.ResearchConferenceDetail!.ReviewFee;

            refundAmount = (decimal)(transaction.Amount - reviewFee);
            walletTransactionDescription = walletTransactionDescription + $" .Bạn đã được hoàn tiền {refundAmount} và đã bao gồm phí review là: {reviewFee}";


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
            //logic trả slot lại cho conference
            var pricePhase = ticket.PricePhase;
            if (pricePhase == null)
            {
                throw new NotFoundException($"Không tìm price phase ");
            }
            pricePhase.AvailableSlot = pricePhase.AvailableSlot + 1;
            pricePhase.ConferencePrice!.AvailableSlot = pricePhase.ConferencePrice!.AvailableSlot + 1;
            pricePhase.ConferencePrice!.Conference!.AvailableSlot = pricePhase.ConferencePrice!.Conference!.AvailableSlot + 1;
            result += await _unitOfWork.PricePhaseRepository.UpdatePricePhaseAsync(pricePhase);

            //logic chuyển ticket thành true
            ticket.IsRefunded = true;

            //logic thêm transaction vào ví với dạng là refund
            var transactionId = Guid.NewGuid().ToString();
            var transactionObj = new Transaction()
            {
                TransactionId = transactionId,
                UserId = userId,
                Currency = "VND",
                Amount = refundAmount,
                CreatedAt = dateTime,
                IsRefunded = true,
                PaymentMethodId = walletPaymentMethod.PaymentMethodId,
                TicketId = ticketId,
            };

            result += await _unitOfWork.TransactionRepository.CreateTransactionAsync(transactionObj);
            result += await _unitOfWork.TicketRepository.UpdateTicketAsync(ticket);

            return result;
        }

        public async Task<int> CancelTechTickets(CancelTechnicalTickets tickets, string userId)
        {

            var dateTime = await _timeProviderService.GetVietnamTime();
            var dateNow = await _timeProviderService.GetVietnamDate();
            var walletPaymentMethod = await _unitOfWork.PaymentMethodRepository.GetPaymentMethodByName(PaymentMethodEnum.Wallet.GetDescription());

            var ticketList = await _unitOfWork.TicketRepository.GetNotRefundTechnicalTicketListByTicketIdsForCancel(tickets.TicketIds);
            if (ticketList.Count <= 0)
            {
                return 0;
            }

            var ownTechConferenceIds = (await _unitOfWork.ConferenceRepository
                .GetTechnicalConferenceOrResearchConferenceIdsByUserId(userId, isResearchConference: false)).ToHashSet();



            List<WalletTransaction> walletTransactions = new List<WalletTransaction>();
            List<Transaction> transactions = new List<Transaction>();
            foreach (var ticket in ticketList)
            {
                var usertransactionList = ticket.Transactions;
                var validTransaction = usertransactionList.FirstOrDefault(t => t.IsRefunded == false);
                var refundAmount = validTransaction!.Amount;

                bool isValidTicketBelongToConference = ownTechConferenceIds.Contains(ticket.PricePhase.ConferencePrice.ConferenceId);
                if (isValidTicketBelongToConference == false)
                {
                    throw new BadRequestException($"Bạn không thể refund vé này vì vé {ticket.TicketId} không thuộc về hội nghị của bạn");
                }
                //userwallet(update chung ticket)

                var userWallet = ticket.User!.Wallet;
                if (userWallet == null)
                {
                    throw new NotFoundException($"Không tìm thấy ví cho {ticket.User.FullName}");
                }
                userWallet.UpdatedAt = dateTime;
                userWallet.Balance = userWallet.Balance + refundAmount;

                //wallet transac (create)
                var userWalletTransaction = new WalletTransaction()
                {
                    WalletTransactionId = Guid.NewGuid().ToString(),
                    WalletId = userWallet.WalletId,
                    Amount = +refundAmount,
                    TransactionType = WalletTransactionTypeEnum.Refund.GetDescription(),
                    Description = $"Vì hội nghị {ticket.PricePhase.ConferencePrice.Conference.ConferenceName} bị hủy nên bạn được hoàn tiền về tài khoản",
                    CreatedAt = dateTime,
                };
                walletTransactions.Add(userWalletTransaction);

                // pricephase, conf price, conf (update chung với ticket)
                var pricePhase = ticket.PricePhase;
                pricePhase.AvailableSlot = pricePhase.AvailableSlot + 1;
                pricePhase.ConferencePrice!.AvailableSlot = pricePhase.ConferencePrice!.AvailableSlot + 1;
                pricePhase.ConferencePrice!.Conference!.AvailableSlot = pricePhase.ConferencePrice!.Conference!.AvailableSlot + 1;

                //ticket  (update)
                ticket.IsRefunded = true;


                //transaction (create)

                var transactionId = Guid.NewGuid().ToString();
                var transactionObj = new Transaction()
                {
                    TransactionId = transactionId,
                    UserId = ticket.UserId,
                    Currency = "VND",
                    Amount = refundAmount,
                    CreatedAt = dateTime,
                    IsRefunded = true,
                    PaymentMethod = walletPaymentMethod,
                    TicketId = ticket.TicketId,
                };
                transactions.Add(transactionObj);

            }
            await _unitOfWork.BeginTransactionAsync();
            int result = 0;
            try
            {
                result += await _unitOfWork.TicketRepository.UpdateTicketListAsync(ticketList);
                result += await _unitOfWork.WalletTransactionRepository.CreateWalletTransactionListAsync(walletTransactions);
                result += await _unitOfWork.TransactionRepository.CreateTransactionListAsync(transactions);
                await _unitOfWork.CommitAsync();
                return result;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }


        }

        public async Task<int> CancelResearchTickets(CancelResearchTickets tickets, string userId)
        {
            //var abstractPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.Abstract.GetDescription());
            //var fullPaperPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.FullPaper.GetDescription());
            //var revisionPaperPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.Revise.GetDescription());
            //var cameraReadyPhase = await _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync(PaperPhaseEnum.CameraReady.GetDescription());

            var rejectGlobalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Rejected.GetDescription());
            var rejectReviewStatus = await _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync(ReviewStatusEnum.Rejected.GetDescription());


            var dateTime = await _timeProviderService.GetVietnamTime();
            var dateNow = await _timeProviderService.GetVietnamDate();
            var walletPaymentMethod = await _unitOfWork.PaymentMethodRepository.GetPaymentMethodByName(PaymentMethodEnum.Wallet.GetDescription());

            var ticketList = await _unitOfWork.TicketRepository.GetNotRefundResearchTicketListByTicketIdsForCancel(tickets.TicketIds);
            if (ticketList.Count <= 0)
            {
                return 0;
            }

            var ownResearchConferenceIds = (await _unitOfWork.ConferenceRepository
                .GetTechnicalConferenceOrResearchConferenceIdsByUserId(userId, isResearchConference: true)).ToHashSet();
            List<WalletTransaction> walletTransactions = new List<WalletTransaction>();
            List<Transaction> transactions = new List<Transaction>();
            foreach (var ticket in ticketList)
            {
                var usertransactionList = ticket.Transactions;
                var validTransaction = usertransactionList.FirstOrDefault(t => t.IsRefunded == false);
                var refundAmount = validTransaction!.Amount;

                //userwallet (update chung ticket)

                bool isValidTicketBelongToOwnConference = ownResearchConferenceIds.Contains(ticket.PricePhase.ConferencePrice.ConferenceId);
                if (isValidTicketBelongToOwnConference == false)
                {
                    throw new BadRequestException($"Chúng tôi phát hiện vé {ticket.TicketId} không thuộc về bất cứ hội nghị nào của bạn");
                }

                var userWallet = ticket.User!.Wallet;
                if (userWallet == null)
                {
                    throw new NotFoundException($"Không tìm thấy ví cho {ticket.User.FullName}");
                }
                userWallet.UpdatedAt = dateTime;
                userWallet.Balance = userWallet.Balance + refundAmount;

                //wallet transac (create)
                var userWalletTransaction = new WalletTransaction()
                {
                    WalletTransactionId = Guid.NewGuid().ToString(),
                    WalletId = userWallet.WalletId,
                    Amount = +refundAmount,
                    TransactionType = WalletTransactionTypeEnum.Refund.GetDescription(),
                    Description = $"Vì hội nghị {ticket.PricePhase.ConferencePrice.Conference.ConferenceName} bị hủy nên bạn được hoàn tiền về tài khoản",
                    CreatedAt = dateTime,
                };
                walletTransactions.Add(userWalletTransaction);

                // pricephase, conf price, conf (update chung với ticket)
                var pricePhase = ticket.PricePhase;
                pricePhase.AvailableSlot = pricePhase.AvailableSlot + 1;
                pricePhase.ConferencePrice!.AvailableSlot = pricePhase.ConferencePrice!.AvailableSlot + 1;
                pricePhase.ConferencePrice!.Conference!.AvailableSlot = pricePhase.ConferencePrice!.Conference!.AvailableSlot + 1;

                //ticket  (update)
                ticket.IsRefunded = true;


                //transaction (create)

                var transactionId = Guid.NewGuid().ToString();
                var transactionObj = new Transaction()
                {
                    TransactionId = transactionId,
                    UserId = ticket.UserId,
                    Currency = "VND",
                    Amount = refundAmount,
                    CreatedAt = dateTime,
                    IsRefunded = true,
                    PaymentMethod = walletPaymentMethod,
                    TicketId = ticket.TicketId,
                };
                transactions.Add(transactionObj);




                //reject các paper phase
                var paper = ticket.Paper;
                if (ticket.Paper != null)
                {
                    if (paper.Abstract != null)
                    {
                        paper.Abstract.GlobalStatus = rejectGlobalStatus;
                    }

                    if (paper.FullPaper != null)
                    {
                        paper.FullPaper.ReviewStatus = rejectReviewStatus;
                    }

                    if (paper.RevisionPaper != null)
                    {
                        paper.RevisionPaper.GlobalStatus = rejectGlobalStatus;
                    }

                    if (paper.CameraReady != null)
                    {
                        paper.CameraReady.GlobalStatus = rejectGlobalStatus;
                    }

                }



            }
            await _unitOfWork.BeginTransactionAsync();
            int result = 0;
            try
            {
                result += await _unitOfWork.TicketRepository.UpdateTicketListAsync(ticketList);
                result += await _unitOfWork.WalletTransactionRepository.CreateWalletTransactionListAsync(walletTransactions);
                result += await _unitOfWork.TransactionRepository.CreateTransactionListAsync(transactions);
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
