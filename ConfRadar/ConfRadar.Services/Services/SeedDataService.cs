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
        //Task SeedTransactionTypeAsync();
        //Task SeedMediaTypesAsync();
        Task SeedConferenceStatusesAsync();
        Task SeedPaperPhasesAsync();
        Task SeedCheckInStatusAsync();

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
            var statusNames = Enum.GetValues<ConferenceStatusEnum>()
                .Select(s => s.GetDescription())
                .ToList();

            await SeedEntityAsync<ConferenceStatus>(
                statusNames,
                _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync,
                _unitOfWork.ConferenceStatusRepository.CreateMultipleConferenceStatusAsync,
                name => new ConferenceStatus
                {
                    ConferenceStatusId = Guid.NewGuid().ToString(),
                    ConferenceStatusName = name
                });
        }

        public async Task SeedPaperPhasesAsync()
        {
            var phaseNames = Enum.GetValues<PaperPhaseEnum>()
                .Select(s => s.GetDescription())
                .ToList();

            await SeedEntityAsync<PaperPhase>(
                phaseNames,
                _unitOfWork.PaperPhaseRepository.GetPaperPhaseByNameAsync,
                _unitOfWork.PaperPhaseRepository.CreateMultiplePaperPhasesAsync,
                name => new PaperPhase
                {
                    PaperPhaseId = Guid.NewGuid().ToString(),
                    PhaseName = name
                });
        }
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


    }


}
