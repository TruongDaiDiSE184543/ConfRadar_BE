using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.Services
{
    public interface IPaymentService
    {
        Task<List<Transaction>> GetOwnTransactionByUserId(string userId);
    }
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        public PaymentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<List<Transaction>> GetOwnTransactionByUserId(string userId)
        {
            return await _unitOfWork.TransactionRepository.GetOwnTransactionByUserId(userId);
        }
    }
}
