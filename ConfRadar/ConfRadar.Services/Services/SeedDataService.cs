using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;

namespace ConfRadar.Services.Services
{
    public interface ISeedDataService
    {
        Task SeedRolesAsync();
        //Task SeedTransactionStatusAsync();
        Task SeedPaymentMethodsAsync();
        Task SeedGlobalStatusesAsync();
        Task SeedConferenceStatusesAsync();
        Task SeedRankingCategoriesAsync();
        Task SeedReviewStatusesAsync();
        Task SeedPaperPhasesAsync();
        //Task SeedTransactionTypeAsync();
        //Task SeedMediaTypesAsync();
        Task SeedCheckInStatusAsync();
        //Task SeedReviewStatusAsync();
        Task SeedWaitListStatusesAsync();
    }
    public class SeedDataService : ISeedDataService
    {
        private readonly IUnitOfWork _unitOfWork;
        public SeedDataService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        private async Task SeedEntityAsync<T>(
            IEnumerable<string> names,
            Func<string, Task<T?>> findByNameAsync,
            Func<List<T>, Task> createMutipleAsync,
            Func<string, T> createEntityFunc
            )
        {
            List<T> newEntities = new List<T>();
            foreach (var name in names)
            {
                var entityFound = await findByNameAsync(name);
                if (entityFound == null)
                {
                    var newEntity = createEntityFunc(name);
                    newEntities.Add(newEntity);
                }
            }
            if (newEntities.Count > 0)
            {
                await createMutipleAsync(newEntities);
            }

        }

        //public async Task SeedCheckInStatusAsync()
        //{
        //    var statusNames = Enum.GetValues<CheckInStatusEnum>()
        //        .Select(s => s.GetDescription())
        //        .ToList();

        //    await SeedEntityAsync<CheckinStatus>(
        //        statusNames,
        //        _unitOfWork.CheckInStatusRepository.GetCheckInStatusByNameAsync,
        //        _unitOfWork.CheckInStatusRepository.CreateMultipleCheckInStatusesAsync,
        //        name => new CheckinStatus
        //        {
        //            CheckinStatusId = Guid.NewGuid().ToString(),
        //            CheckinStatusName = name
        //        });
        //}

        public async Task SeedRolesAsync()
        {
            var roleNames = Enum.GetValues<SystemRoleEnum>().Select(r => r.GetDescription()).ToList();
            await SeedEntityAsync<Role>(
                roleNames,
                _unitOfWork.RoleRepository.GetRoleByRoleName,
                _unitOfWork.RoleRepository.CreateMutipleRoleAsync,
                name => new Role
                {
                    RoleId = Guid.NewGuid().ToString(),
                    RoleName = name
                });
        }
        public async Task SeedGlobalStatusesAsync()
        {
            var statusNames = Enum.GetValues<GlobalStatusEnum>().Select(s => s.GetDescription()).ToList();
            await SeedEntityAsync<GlobalStatus>(
                statusNames,
                _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName,
                _unitOfWork.GlobalStatusRepository.CreateMutipleGlobalStatusesAsync,
                name => new GlobalStatus
                {
                    GlobalStatusId = Guid.NewGuid().ToString(),
                    Name = name
                });
        }
        public async Task SeedPaymentMethodsAsync()
        {
            var statusNames = Enum.GetValues<PaymentMethodEnum>().Select(s => s.GetDescription()).ToList();
            await SeedEntityAsync<PaymentMethod>(
                statusNames,
                _unitOfWork.PaymentMethodRepository.GetPaymentMethodByName,
                _unitOfWork.PaymentMethodRepository.CreateMutiplePaymentMethodsAsync,
                name => new PaymentMethod
                {
                    PaymentMethodId = Guid.NewGuid().ToString(),
                    MethodName = name,
                });
        }

        public async Task SeedConferenceStatusesAsync()
        {
            var statusNames = Enum.GetValues<ConfRadar.Services.Common.ConferenceStatus>().Select(s => s.ToString()).ToList();
            await SeedEntityAsync<ConfRadar.Repositories.Models.ConferenceStatus>(
                statusNames,
                _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByName,
                _unitOfWork.ConferenceStatusRepository.CreateMultipleConferenceStatusesAsync,
                name => new ConfRadar.Repositories.Models.ConferenceStatus
                {
                    ConferenceStatusId = Guid.NewGuid().ToString(),
                    ConferenceStatusName = name
                });
        }

        public async Task SeedRankingCategoriesAsync()
        {
            var categoryNames = Enum.GetValues<ConfRadar.Services.Common.RankingCategories>().Select(s => s.ToString()).ToList();
            await SeedEntityAsync<ConfRadar.Repositories.Models.RankingCategory>(
                categoryNames,
                _unitOfWork.RankingCategoryRepository.GetRankingCategoryByName,
                _unitOfWork.RankingCategoryRepository.CreateMultipleRankingCategoriesAsync,
                name => new ConfRadar.Repositories.Models.RankingCategory
                {
                    RankingCategoryId = Guid.NewGuid().ToString(),
                    RankName = name
                });
        }

        //public async Task SeedReviewStatusesAsync()
        //{
        //    var statusNames = Enum.GetValues<ConfRadar.Services.Common.ReviewStatus>().Select(s => s.ToString()).ToList();
        //    await SeedEntityAsync<ConfRadar.Repositories.Models.ReviewStatus>(
        //        statusNames,
        //        _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync,
        //        _unitOfWork.ReviewStatusRepository.CreateMultipleReviewStatusesAsync,
        //        name => new ConfRadar.Repositories.Models.ReviewStatus
        //        {
        //            ReviewStatusId = Guid.NewGuid().ToString(),
        //            Name = name
        //        });
        //}

        public async Task SeedPaperPhasesAsync()
        {
            var phaseNames = Enum.GetValues<ConfRadar.Services.Common.PaperPhaseEnum>().Select(s => s.ToString()).ToList();
            await SeedEntityAsync<ConfRadar.Repositories.Models.PaperPhase>(
                phaseNames,
                _unitOfWork.PaperPhaseRepository.GetPaperPhaseByName,
                _unitOfWork.PaperPhaseRepository.CreateMultiplePaperPhasesAsync,
                name => new ConfRadar.Repositories.Models.PaperPhase
                {
                    PaperPhaseId = Guid.NewGuid().ToString(),
                    PhaseName = name
                });
        }

        //public async Task SeedTransactionStatusAsync()
        //{
        //    var statusNames = Enum.GetValues<TransactionStatusEnum>().Select(s => s.GetDescription()).ToList();
        //    await SeedEntityAsync<TransactionStatus>(
        //        statusNames,
        //        _unitOfWork.TransactionStatusRepository.GetTransactionStatusByName,
        //        _unitOfWork.TransactionStatusRepository.CreateMutipleTransactionStatusesAsync,
        //        name => new TransactionStatus
        //        {
        //            TransactionStatusId = Guid.NewGuid().ToString(),
        //            StatusName = name,
        //        });
        //}


        public async Task SeedCheckInStatusAsync()
        {
            var statusNames = Enum.GetValues<CheckInStatusEnum>()
                .Select(s => s.GetDescription())
                .ToList();
            await SeedEntityAsync<CheckinStatus>(
                statusNames,
                _unitOfWork.CheckInStatusRepository.GetCheckInStatusByNameAsync,
                _unitOfWork.CheckInStatusRepository.CreateMultipleCheckInStatusesAsync,
                name => new CheckinStatus
                {
                    CheckinStatusId = Guid.NewGuid().ToString(),
                    CheckinStatusName = name
                });
        }

        public async Task SeedReviewStatusesAsync()
        {
            var statusNames = Enum.GetValues<ReviewStatusEnum>()
                .Select(s => s.GetDescription())
                .ToList();

            await SeedEntityAsync<Repositories.Models.ReviewStatus>(
                statusNames,
                _unitOfWork.ReviewStatusRepository.GetReviewStatusByNameAsync,
                _unitOfWork.ReviewStatusRepository.CreateMultipleReviewStatusesAsync,
                name => new Repositories.Models.ReviewStatus
                {
                    ReviewStatusId = Guid.NewGuid().ToString(),
                    Name = name
                });
        }

        public async Task SeedWaitListStatusesAsync()
        {
            var statusNames = Enum.GetValues<WaitListStatusEnum>()
                .Select(s => s.GetDescription())
                .ToList();

            await SeedEntityAsync<WaitListStatus>(
                statusNames,
                _unitOfWork.WaitListStatusRepository.GetWaitListStatusByNameAsync,
                _unitOfWork.WaitListStatusRepository.CreateMultipleWaitListStatusesAsync,
                name => new WaitListStatus
                {
                    WaitListStatusId = Guid.NewGuid().ToString(),
                    Name = name,
                });
        }
    }


}
