using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.DTOs.Transaction;

namespace ConfRadar.Services.Services
{
    public interface IPaymentService
    {
        Task<List<TransactionDetailResponse>> GetOwnTransactionByUserId(string userId);
        Task<List<PaymentMethod>> GetListPaymentMethod();

    }
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        public PaymentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
                Amount = x.Amount,
                CreatedAt = x.CreatedAt,
                Currency = x.Currency,
                PaymentMethodId = x.PaymentMethodId,
                PaymentMethodName = x.PaymentMethod?.MethodName,
                //PaymentStatusName = x.TransactionStatus?.StatusName,
                TransactionCode = x.TransactionCode,
                TransactionId = x.TransactionId,
                //TransactionStatusId = x.TransactionStatusId,
                //TransactionTypeId = x.TransactionTypeId,
                UserId = x.UserId,

            }).ToList();
            return transactionDetailResponses;
        }
    }
}
