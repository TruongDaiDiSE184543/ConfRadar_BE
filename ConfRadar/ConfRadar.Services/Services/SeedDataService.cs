using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;

namespace ConfRadar.Services.Services
{
    public interface ISeedDataService
    {
        Task SeedRolesAsync();
        Task SeedTransactionStatusAsync();
        Task SeedPaymentMethodsAsync();
        Task SeedGlobalStatusesAsync();
        Task SeedTransactionTypeAsync();
        Task SeedMediaTypesAsync();

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
            Func<IEnumerable<T>, Task> createMutipleAsync,
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
        public async Task SeedTransactionStatusAsync()
        {
            var statusNames = Enum.GetValues<TransactionStatusEnum>().Select(s => s.GetDescription()).ToList();
            await SeedEntityAsync<TransactionStatus>(
                statusNames,
                _unitOfWork.TransactionStatusRepository.GetTransactionStatusByName,
                _unitOfWork.TransactionStatusRepository.CreateMutipleTransactionStatusesAsync,
                name => new TransactionStatus
                {
                    TransactionStatusId = Guid.NewGuid().ToString(),
                    StatusName = name,
                });
        }

        public async Task SeedTransactionTypeAsync()
        {
            var typeNames = Enum.GetValues<TransactionTypeEnum>().Select(s => s.GetDescription()).ToList();
            await SeedEntityAsync<TransactionType>(
                typeNames,
                _unitOfWork.TransactionTypeRepository.GetTransactionTypeByName,
                _unitOfWork.TransactionTypeRepository.CreateMutipleTransactionTypesAsync,
                name => new TransactionType
                {
                    TransactionTypeId = Guid.NewGuid().ToString(),
                    TypeName = name,
                });
        }


        public async Task SeedMediaTypesAsync()
        {
            var mediaTypeNames = Enum.GetValues<MediaTypeEnum>().Select(m => m.GetDescription()).ToList();
            await SeedEntityAsync<MediaType>(
                mediaTypeNames,
                _unitOfWork.MediaTypeRepository.GetMediaTypeByNameAsync,
                async entities =>
                {
                    foreach (var entity in entities)
                    {
                        await _unitOfWork.MediaTypeRepository.CreateMediaTypeAsync(entity);
                    }
                },
                name => new MediaType
                {
                    MediaTypeId = Guid.NewGuid().ToString(),
                    MediaTypeName = name

                });
        }
    }


}
