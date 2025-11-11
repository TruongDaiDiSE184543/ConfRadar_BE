using ConfRadar.Repositories;
using ConfRadar.Shared.DTO.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.Services
{
    public interface IWalletService
    {
        Task<OwnWalletDetailResponse?> ViewOwnWallet(string userId);
    }
    public class WalletService :IWalletService
    {
        private readonly IUnitOfWork _unitOfWork;
        public WalletService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<OwnWalletDetailResponse?> ViewOwnWallet(string userId)
        {
            return await _unitOfWork.WalletRepository.ViewOwnWallet(userId);
        }
    }
}
