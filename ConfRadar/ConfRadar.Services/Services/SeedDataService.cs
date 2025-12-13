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
        Task SeedAuditLogCategoriesAsync();
        //Task SeedPublishersAsync();
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
            var statusNames = Enum.GetValues<ConfRadar.Services.Common.ConferenceStatusEnum>().Select(s => s.ToString()).ToList();
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
            var categoryNames = Enum.GetValues<ConfRadar.Services.Common.RankingCategoriesEnum>().Select(s => s.ToString()).ToList();
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

        public async Task SeedAuditLogCategoriesAsync()
        {
            var categoryNames = Enum.GetValues<AuditLogActionNameEnum>()
                .Select(c => c.GetDescription())
                .ToList();

            await SeedEntityAsync<AuditLogCategory>(
                categoryNames,
                _unitOfWork.AuditLogCategoryRepository.GetAuditLogCategoryByNameAsync,
                _unitOfWork.AuditLogCategoryRepository.CreateMultipleAuditLogCategoriesAsync,
                name => new AuditLogCategory
                {
                    CategoryId = Guid.NewGuid().ToString(),
                    Name = name
                });
        }

        // 1. Data định nghĩa sẵn Publisher
        private readonly List<Publisher> _publisherData = new List<Publisher>
        {
            new Publisher
            {
                Name = "Institute of Electrical and Electronics Engineers",
                Description = "Tổ chức chuyên môn kỹ thuật lớn nhất thế giới.",
                WebsiteUrl = "https://www.ieee.org",
                LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/2/21/IEEE_logo.svg/1200px-IEEE_logo.svg.png",
                PaperFormat = "ieee",
                LinkTemplate = "https://ieeexplore.ieee.org/document/{RANDOM_ID_7}" // IEEE thường dùng ID 7 chữ số
            },
            new Publisher
            {
                Name = "Association for Computing Machinery",
                Description = "Hiệp hội máy tính lớn nhất thế giới.",
                WebsiteUrl = "https://www.acm.org",
                LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/8/8e/Association_for_Computing_Machinery_%28ACM%29_logo.svg/1024px-Association_for_Computing_Machinery_%28ACM%29_logo.svg.png",
                PaperFormat = "acm",
                LinkTemplate = "https://dl.acm.org/doi/10.1145/{RANDOM_ID_7}.{RANDOM_ID_7}"
            },
            new Publisher
            {
                Name = "Springer Nature (General)",
                Description = "Nhà xuất bản học thuật toàn cầu, định dạng Springer chung.",
                WebsiteUrl = "https://www.springer.com",
                LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/6/60/Springer_Nature_Logo.svg/2560px-Springer_Nature_Logo.svg.png",
                PaperFormat = "springer",
                LinkTemplate = "https://link.springer.com/chapter/10.1007/978-3-030-{RANDOM_ID_5}-6_{RANDOM_ID_2}"
            },
            new Publisher
            {
                Name = "Springer (LNCS Series)",
                Description = "Chuỗi Lecture Notes in Computer Science của Springer.",
                WebsiteUrl = "https://www.springer.com/gp/computer-science/lncs",
                LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/6/60/Springer_Nature_Logo.svg/2560px-Springer_Nature_Logo.svg.png",
                PaperFormat = "lncs",
                // LNCS cũng dùng format link tương tự Springer, chỉ khác về nội dung
                LinkTemplate = "https://link.springer.com/chapter/10.1007/978-3-030-{RANDOM_ID_5}-6_{RANDOM_ID_2}"
            },
            new Publisher
            {
                Name = "Elsevier",
                Description = "Doanh nghiệp xuất bản thông tin Hà Lan.",
                WebsiteUrl = "https://www.elsevier.com",
                LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/2/2f/Elsevier_logo.svg/2560px-Elsevier_logo.svg.png",
                PaperFormat = "elsevier",
                LinkTemplate = "https://www.sciencedirect.com/science/article/pii/S{RANDOM_ID_8}X"
            }
        };


        //public async Task SeedPublishersAsync()
        //{
        //    var publisherNames = _publisherData.Select(p => p.Name).ToList();

        //    await SeedEntityAsync<Publisher>(
        //        publisherNames,
        //        _unitOfWork.PublisherRepository.GetPublisherByNameAsync,
        //        _unitOfWork.PublisherRepository.CreateMultiplePublishersAsync,
        //        name =>
        //        {
        //            var data = _publisherData.First(p => p.Name == name);

        //            return new Publisher
        //            {
        //                PublisherId = Guid.NewGuid().ToString(),
        //                Name = data.Name,
        //                PaperFormat = data.PaperFormat,
        //                Description = data.Description,
        //                WebsiteUrl = data.WebsiteUrl,
        //                LinkTemplate = data.LinkTemplate,
        //                LogoUrl = data.LogoUrl
        //            };
        //        }
        //    );
        //}

    }


}
