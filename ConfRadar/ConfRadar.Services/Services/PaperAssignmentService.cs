using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.DTOs.Paper;
using ConfRadar.Services.Exceptions;

namespace ConfRadar.Services.Services
{
    public interface IPaperAssignmentService
    {
        Task<string> AssignCoAuthorsToPaper(AssignCoAuthorsToPaperRequest request);
        Task<string> AssignReviewersToPaper(AssignReviewerToPaperRequest request);
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
                throw new BadRequestException("Vai trò 'Customer' không tồn tại trong hệ thống.");
            }

            // Lấy trước các thông tin cần thiết để kiểm tra trong vòng lặp
            var paperReviewers = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByConferenceIdAsync(paper.ConferenceId);
            var existingAuthors = paper.PaperAuthors.ToList();
            var rootAuthorId = existingAuthors.FirstOrDefault(pa => pa.IsRootAuthor.Value)?.UserId;

            var newPaperAuthors = new List<PaperAuthor>();
            var newNotifications = new List<Notification>();
            var usersToNotify = new List<User>();
            var distinctUserIds = request.UserIds.Distinct().ToList();

            // --- Bước 2: Lặp qua từng UserId để xác thực ---
            foreach (var userId in distinctUserIds)
            {
                var user = await _unitOfWork.UserRepository.GetUserByUserId(userId);
                if (user == null)
                {
                    throw new BadRequestException($"Không tìm thấy người dùng với ID {userId}.");
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

                newPaperAuthors.Add(new PaperAuthor
                {
                    UserId = userId,
                    PaperId = request.PaperId,
                    IsRootAuthor = false,
                    IsPresenter = false
                });

                usersToNotify.Add(user);
            }

            // --- Bước 3: Thực hiện ghi vào CSDL trong một transaction ---
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Xóa các đồng tác giả cũ (không phải tác giả chính)
                var coAuthorsToRemove = existingAuthors.Where(pa => pa.IsRootAuthor == false).ToList();
                if (coAuthorsToRemove.Any())
                {
                    await _unitOfWork.PaperAuthorRepository.DeleteMutiplePaperAuthorAsync(coAuthorsToRemove);
                }

                if (newPaperAuthors.Any())
                {
                    var timeNow = await _timeProviderService.GetVietnamTime();
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

                    await _unitOfWork.PaperAuthorRepository.CreateMutiplePaperAuthorAsync(newPaperAuthors);
                    await _unitOfWork.NotificationRepository.CreateMutipleNotificationAsync(newNotifications);
                }

                await _unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw; // Ném lại lỗi để cấp cao hơn xử lý
            }

            // --- Bước 4: Gửi push notification sau khi transaction thành công ---
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

            return $"Đã cập nhật thành công danh sách đồng tác giả. Đã gán {newPaperAuthors.Count} đồng tác giả cho bài báo.";
        }

        public async Task<string> AssignReviewersToPaper(AssignReviewerToPaperRequest request)
        {
            // --- BƯỚC 1: XÁC THỰC DỮ LIỆU ĐẦU VÀO ---

            //  Chỉ có một head reviewer trong danh sách**
            if (request.Reviewers.Count(r => r.IsHeadReviewer) != 1)
            {
                throw new BadRequestException("Một bài báo chỉ có thể có một head reviewer.");
            }

            // **Kiểm tra reviewer trùng lặp trong danh sách đầu vào**
            var duplicateUserIds = request.Reviewers.GroupBy(r => r.UserId)
                                                    .Where(g => g.Count() > 1)
                                                    .Select(g => g.Key);
            if (duplicateUserIds.Any())
            {
                throw new BadRequestException($"ID người dùng bị trùng lặp trong danh sách yêu cầu: {string.Join(", ", duplicateUserIds)}");
            }


            // Xác thực bài báo tồn tại
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new BadRequestException($"Bài báo với ID {request.PaperId} không tồn tại.");
            }

            // Xác thực các vai trò Reviewer tồn tại trong hệ thống
            var localReviewerRole = await _unitOfWork.RoleRepository.GetRoleByRoleName("Local Reviewer");
            var externalReviewerRole = await _unitOfWork.RoleRepository.GetRoleByRoleName("External Reviewer");
            if (localReviewerRole == null || externalReviewerRole == null)
            {
                throw new InvalidOperationException("Vai trò 'Local Reviewer' hoặc 'External Reviewer' không tồn tại trong hệ thống.");
            }

            // Chuẩn bị danh sách để thêm vào CSDL và gửi thông báo
            var newPaperReviewers = new List<PaperReviewer>();
            var newNotifications = new List<Notification>();
            var usersToNotify = new List<User>(); // Để gửi push notification sau

            // Lặp qua danh sách reviewer từ request để xác thực từng người
            foreach (var reviewerDto in request.Reviewers)
            {
                var user = await _unitOfWork.UserRepository.GetUserByUserId(reviewerDto.UserId);
                if (user == null)
                {
                    throw new BadRequestException($"Người dùng với ID {reviewerDto.UserId} không tồn tại.");
                }

                var isAuthor = await _unitOfWork.PaperAuthorRepository.GetPaperAuthorByIdAsync(reviewerDto.UserId, request.PaperId);
                if (isAuthor != null)
                {
                    throw new BadRequestException($"Người dùng '{user.FullName}' ({reviewerDto.UserId}) là tác giả của bài báo, không thể được gán làm reviewer.");
                }

                var userRoles = await _unitOfWork.UserRoleRepository.GetMutipleUserRolesByUserId(reviewerDto.UserId);
                var hasLocalReviewerRole = userRoles.Any(ur => ur.RoleId == localReviewerRole.RoleId);
                var hasExternalReviewerRole = userRoles.Any(ur => ur.RoleId == externalReviewerRole.RoleId);

                if (!hasLocalReviewerRole && !hasExternalReviewerRole)
                {
                    throw new BadRequestException($"Người dùng '{user.FullName}' ({reviewerDto.UserId}) không có vai trò 'Local Reviewer' hoặc 'External Reviewer'.");
                }

                if (hasExternalReviewerRole)
                {
                    var contract = await _unitOfWork.ReviewerContractRepository.GetContractByUserAndConferenceAsync(user.UserId, paper.ConferenceId);
                    if (contract == null)
                    {
                        throw new BadRequestException($"Người dùng '{user.FullName}' ({reviewerDto.UserId}) là reviewer ngoài nhưng không có hợp đồng cho hội nghị này.");
                    }

                    if (contract.IsActive != true)
                    {
                        throw new BadRequestException("Hợp đồng của reviewer không còn hoạt động");
                    }
                }

                // Nếu tất cả xác thực đều qua, thêm vào danh sách để xử lý
                newPaperReviewers.Add(new PaperReviewer
                {
                    PaperId = request.PaperId,
                    UserId = reviewerDto.UserId,
                    IsHeadReviewer = reviewerDto.IsHeadReviewer
                });

                usersToNotify.Add(user);
            }


            // --- BƯỚC 2: THỰC THI LOGIC TRONG GIAO DỊCH (TRANSACTION) ---
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Tìm và xóa tất cả các reviewer hiện có của bài báo này
                var existingReviewers = await _unitOfWork.PaperReviewerRepository.GetPaperReviewersByPaperIdAsync(request.PaperId);
                if (existingReviewers.Any())
                {
                    await _unitOfWork.PaperReviewerRepository.DeleteMultiplePaperReviewersAsync(existingReviewers);
                }

                // Thêm danh sách reviewer mới
                if (newPaperReviewers.Any())
                {
                    await _unitOfWork.PaperReviewerRepository.CreateMultiplePaperReviewersAsync(newPaperReviewers);

                    var timeNow = await _timeProviderService.GetVietnamTime();
                    foreach (var reviewer in newPaperReviewers)
                    {
                        string title = "Nhiệm vụ đánh giá bài báo";
                        string message = reviewer.IsHeadReviewer.Value
                            ? $"Bạn đã được gán làm trưởng ban đánh giá (head reviewer) cho bài báo \"{paper.Title}\"."
                            : $"Bạn đã được gán làm người đánh giá (reviewer) cho bài báo \"{paper.Title}\".";

                        newNotifications.Add(new Notification
                        {
                            NotificationId = Guid.NewGuid().ToString(),
                            UserId = reviewer.UserId,
                            Title = title,
                            Message = message,
                            CreatedAt = timeNow,
                            ReadStatus = false
                        });
                    }
                    await _unitOfWork.NotificationRepository.CreateMutipleNotificationAsync(newNotifications);
                }

                // Lưu tất cả thay đổi vào cơ sở dữ liệu
                await _unitOfWork.CommitAsync();

                // --- BƯỚC 3: GỬI THÔNG BÁO PUSH SAU KHI GIAO DỊCH THÀNH CÔNG ---
                foreach (var user in usersToNotify)
                {
                    var assignment = request.Reviewers.First(r => r.UserId == user.UserId);
                    string title = "Nhiệm vụ đánh giá bài báo";
                    string message = assignment.IsHeadReviewer
                        ? $"Bạn đã được gán làm trưởng ban đánh giá (head reviewer) cho bài báo \"{paper.Title}\"."
                        : $"Bạn đã được gán làm người đánh giá (reviewer) cho bài báo \"{paper.Title}\".";

                    if (!string.IsNullOrWhiteSpace(user.FirebaseMobileFcmToken))
                    {
                        await _notificationService.SendMobilePushAsync(user.FirebaseMobileFcmToken, title, message);
                    }
                    if (!string.IsNullOrWhiteSpace(user.FirebaseWebFcmToken))
                    {
                        await _notificationService.SendWebPushAsync(user.FirebaseWebFcmToken, title, message);
                    }
                }

                return $"Đã cập nhật thành công danh sách reviewer. Đã gán {newPaperReviewers.Count} reviewer cho bài báo.";
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

    }
}