using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Repositories.Repositories
{
    public interface IPaymentMethodRepository
    {
        Task<PaymentMethod?> GetPaymentMethodByName(string paymentMethodName);
        Task<int> CreateMutiplePaymentMethodsAsync(IEnumerable<PaymentMethod> paymentMethods);
    }
    public class PaymentMethodRepository : GenericRepository<PaymentMethod>, IPaymentMethodRepository
    {
        public PaymentMethodRepository(ConfRadarDbContext context) : base(context)
        {
        }

        public async Task<PaymentMethod?> GetPaymentMethodByName(string paymentMethodName)
        {
            return await _context.PaymentMethods.FirstOrDefaultAsync(x => x.MethodName == paymentMethodName);
        }
        public async Task<int> CreateMutiplePaymentMethodsAsync(IEnumerable<PaymentMethod> paymentMethods)
        {
            await _context.PaymentMethods.AddRangeAsync(paymentMethods);
            return await _context.SaveChangesAsync();
        }
    }
    
}
