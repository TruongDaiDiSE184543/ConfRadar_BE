using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.DTOs.Paper;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Mappers;

namespace ConfRadar.Services.Services
{
    public interface IPaperAssignmentService
    {
        Task<string> AssignCoAuthorsToPaper(AssignCoAuthorsToPaperRequest request);
        Task<string> AssignReviewerToPaper(AssignReviewerToPaperRequest request);
    }
    public class PaperAssignmentService : IPaperAssignmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITimeProviderService _timeProviderService;
        private readonly INotificationService _notificationService;

        public PaperAssignmentService(IUnitOfWork unitOfWork, ITimeProviderService timeProviderService, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _timeProviderService = timeProviderService;
            _notificationService = notificationService;
        }

        public async Task<string> AssignCoAuthorsToPaper(AssignCoAuthorsToPaperRequest request)
        {
            // --- Bước 1: Lấy và xác thực thông tin chung một lần ---
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new NotFoundException($"Không tìm thấy bài báo với ID {request.PaperId}.");
            }
            if (paper.Conference == null)
            {
                throw new NotFoundException($"Không tìm thấy thông tin hội nghị cho bài báo ID: {paper.PaperId}");
            }

            var customerRole = await _unitOfWork.RoleRepository.GetRoleByRoleName("Customer");
            if (customerRole == null)
            {
                throw new InvalidOperationException("Vai trò 'Customer' không tồn tại trong hệ thống.");
            }

            // Lấy trước các thông tin cần thiết để kiểm tra trong vòng lặp
            var paperReviewers = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByConferenceIdAsync(paper.ConferenceId);
            var existingAuthorIds = paper.PaperAuthors.Select(pa => pa.UserId).ToHashSet(); // Dùng HashSet để kiểm tra nhanh hơn
            var rootAuthorId = paper.PaperAuthors.FirstOrDefault(pa => pa.IsRootAuthor.Value)?.UserId;

            var newPaperAuthors = new List<PaperAuthor>();
            var newNotifications = new List<Notification>();
            var usersToNotify = new List<User>();

            // --- Bước 2: Lặp qua từng UserId để xác thực ---
            foreach (var userId in request.UserIds.Distinct()) // Dùng Distinct để tránh xử lý trùng lặp
            {
                var user = await _unitOfWork.UserRepository.GetUserByUserId(userId);
                if (user == null)
                {
                    throw new BadRequestException($"Không tìm thấy người dùng với ID {userId}.");
                }

                if (existingAuthorIds.Contains(userId))
                {
                    throw new BadRequestException($"Người dùng {user.FullName} ({userId}) đã là tác giả của bài báo này.");
                }

                if (userId == rootAuthorId)
                {
                    throw new BadRequestException($"Không thể thêm tác giả chính ({user.FullName}) làm đồng tác giả.");
                }

                var userRoles = await _unitOfWork.UserRoleRepository.GetMutipleUserRolesByUserId(userId);
                if (!userRoles.Any(ur => ur.RoleId == customerRole.RoleId))
                {
                    throw new BadRequestException($"Người dùng {user.FullName} ({userId}) không có vai trò 'Customer'.");
                }

                if (paperReviewers.Any(pr => pr.UserId == userId))
                {
                    throw new BadRequestException($"Người dùng {user.FullName} ({userId}) đang là reviewer của hội nghị, không thể thêm.");
                }

                var reviewerContract = await _unitOfWork.ReviewerContractRepository.GetContractByUserAndConferenceAsync(userId, paper.ConferenceId);
                if (reviewerContract?.IsActive == true)
                {
                    throw new BadRequestException($"Người dùng {user.FullName} ({userId}) đang có hợp đồng review đang hoạt động với hội nghị.");
                }

                // Nếu tất cả kiểm tra đều qua, thêm vào danh sách để xử lý sau
                newPaperAuthors.Add(new PaperAuthor
                {
                    UserId = userId,
                    PaperId = request.PaperId,
                    IsRootAuthor = false,
                    IsPresenter = false
                });

                usersToNotify.Add(user);
            }

            if (!newPaperAuthors.Any())
            {
                return "Không có tác giả hợp lệ nào để thêm.";
            }

            // --- Bước 3: Thực hiện ghi vào CSDL trong một transaction ---
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var timeNow = DateTime.UtcNow; // Hoặc dùng time provider
                string notiTitle = $"Được thêm làm đồng tác giả cho bài báo: {paper.Title}";
                string notiMessageBase = $"Bạn đã được thêm làm đồng tác giả cho bài báo \"{paper.Title}\" của hội nghị {paper.Conference.ConferenceName}.";

                foreach (var user in usersToNotify)
                {
                    newNotifications.Add(new Notification
                    {
                        NotificationId = Guid.NewGuid().ToString(),
                        UserId = user.UserId,
                        Title = notiTitle,
                        Message = notiMessageBase,
                        CreatedAt = timeNow,
                        ReadStatus = false
                    });
                }

                // Giả sử bạn có các phương thức để thêm nhiều bản ghi cùng lúc
                await _unitOfWork.PaperAuthorRepository.CreateMutiplePaperAuthorAsync(newPaperAuthors);
                await _unitOfWork.NotificationRepository.CreateMutipleNotificationAsync(newNotifications);

                await _unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw; // Ném lại lỗi để cấp cao hơn xử lý
            }

            // --- Bước 4: Gửi push notification sau khi transaction thành công ---
            // Việc này nên nằm ngoài transaction để tránh trường hợp gửi thông báo rồi nhưng CSDL bị rollback
            foreach (var user in usersToNotify)
            {
                string title = $"Được thêm làm đồng tác giả cho bài báo: {paper.Title}";
                string message = $"Bạn đã được thêm làm đồng tác giả cho bài báo \"{paper.Title}\" của hội nghị {paper.Conference.ConferenceName}.";

                if (!string.IsNullOrEmpty(user.FirebaseMobileFcmToken))
                {
                    await _notificationService.SendMobilePushAsync(user.FirebaseMobileFcmToken, title, message);
                }
                if (!string.IsNullOrEmpty(user.FirebaseWebFcmToken))
                {
                    await _notificationService.SendWebPushAsync(user.FirebaseWebFcmToken, title, message);
                }
            }

            return $"Đã gán thành công {newPaperAuthors.Count} đồng tác giả cho bài báo.";
        }

        public async Task<string> AssignReviewerToPaper(AssignReviewerToPaperRequest request)
        {
            // Validate that the user exists
            var user = await _unitOfWork.UserRepository.GetUserByUserId(request.UserId);
            if (user == null)
            {
                throw new BadRequestException($"User with ID {request.UserId} does not exist.");
            }

            // Validate that the paper exists
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new BadRequestException($"Paper with ID {request.PaperId} does not exist.");
            }

            // Validate that the user has either 'Local Reviewer' or 'External Reviewer' role
            var localReviewerRole = await _unitOfWork.RoleRepository.GetRoleByRoleName("Local Reviewer");
            var externalReviewerRole = await _unitOfWork.RoleRepository.GetRoleByRoleName("External Reviewer");

            if (localReviewerRole == null || externalReviewerRole == null)
            {
                throw new BadRequestException("Local Reviewer or External Reviewer role does not exist in the system.");
            }

            // Check if the user is already assigned to this paper
            var existingPaperAuthor = await _unitOfWork.PaperAuthorRepository
                .GetPaperAuthorByIdAsync(request.UserId, request.PaperId);

            if (existingPaperAuthor != null)
            {
                throw new BadRequestException($"User with ID {request.UserId} is already assigned as an author to paper with ID, they cannot be a reviewer and author for the same paper {request.PaperId}.");
            }


            var userRoles = await _unitOfWork.UserRoleRepository
                .GetMutipleUserRolesByUserId(request.UserId);

            var hasLocalReviewerRole = userRoles.Any(ur => ur.RoleId == localReviewerRole.RoleId);
            var hasExternalReviewerRole = userRoles.Any(ur => ur.RoleId == externalReviewerRole.RoleId);

            if (!hasLocalReviewerRole && !hasExternalReviewerRole)
            {
                throw new BadRequestException($"User with ID {request.UserId} does not have Local Reviewer or External Reviewer role.");
            }

            // Check if the user is already assigned to this paper as a reviewer
            var existingPaperReviewer = await _unitOfWork.PaperReviewerRepository
                .GetPaperReviewersByPaperIdAndUserIdAsync(request.UserId, request.PaperId);

            if (existingPaperReviewer != null)
            {
                throw new BadRequestException($"User with ID {request.UserId} is already assigned as a reviewer to paper with ID {request.PaperId}.");
            }

            // If this is a head reviewer assignment, check if there's already a head reviewer for this paper
            if (request.IsHeadReviewer)
            {
                var existingHeadReviewers = await _unitOfWork.PaperReviewerRepository
                    .GetHeadReviewersByPaperIdAsync(request.PaperId);

                if (existingHeadReviewers.Any())
                {
                    throw new BadRequestException($"Paper with ID {request.PaperId} already has a head reviewer assigned.");
                }
            }

            // Create the paper reviewer assignment
            int result = 0;
            var paperReviewer = request.ToModel();
            result += await _unitOfWork.PaperReviewerRepository.CreatePaperReviewerAsync(paperReviewer);


            string title = "Nhiệm vụ làm reviewer";
            string message = request.IsHeadReviewer ? $"Bạn có nhiệm vụ là head reviewer cho bài báo {paper.Title}" : $"Bạn có nhiệm vụ là reviewer thường cho bài báo {paper.Title}";
            if (result > 0)
            {
                var timeNow = await _timeProviderService.GetVietnamTime();

                var notification = new ConfRadar.Repositories.Models.Notification()
                {
                    NotificationId = Guid.NewGuid().ToString(),
                    UserId = user.UserId,
                    Title = title,
                    Message = message,
                    Type = null,
                    CreatedAt = timeNow,
                    ReadStatus = false,
                };
                await _unitOfWork.NotificationRepository.CreateNotificationAsync(notification);


            }




            await _unitOfWork.SaveChangesAsync();
            if (result > 0)
            {
                if (!string.IsNullOrWhiteSpace(user.FirebaseMobileFcmToken))
                {
                    await _notificationService.SendMobilePushAsync(user.FirebaseMobileFcmToken, title, message);
                }

                if (!string.IsNullOrWhiteSpace(user.FirebaseWebFcmToken))
                {
                    await _notificationService.SendWebPushAsync(user.FirebaseWebFcmToken, title, message);
                }
            }
            var reviewerType = hasLocalReviewerRole ? "Local Reviewer" : "External Reviewer";
            var headReviewerStatus = request.IsHeadReviewer ? " as a head reviewer" : "";

            return $"User with ID {request.UserId} ({reviewerType}) has been successfully assigned to paper with ID {request.PaperId}{headReviewerStatus}.";
        }
    }
}