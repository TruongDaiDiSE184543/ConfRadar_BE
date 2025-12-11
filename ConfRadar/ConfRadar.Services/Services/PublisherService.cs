using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.DTOs.ConferenceStep;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Mappers;

namespace ConfRadar.Services.Services
{
    public interface IPublisherService
    {
        Task<string> CreatePublisherAsync(PublisherRequest publisher);
        Task<PublisherResponse> GetPublisherByIdAsync(string publisherId);
        Task<int> UpdatePublisherAsync(string publisherId, PublisherRequest publisher);
        Task<List<PublisherResponse>> GetAllPublishersAsync();
        Task<bool> DeletePublisherAsync(string publisherId);
    }

    public class PublisherService : IPublisherService
    {
        private readonly IUnitOfWork _unitOfWork;
        
        public PublisherService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<string> CreatePublisherAsync(PublisherRequest request) // Đổi tên để rõ ràng hơn
        {
            // VALIDATION 1: Kiểm tra các giá trị đầu vào
            if (request == null)
                throw new BadRequestException("Dữ liệu đầu vào không được để trống.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new BadRequestException("Tên nhà xuất bản là bắt buộc.");

            ValidateUrlFormat(request.WebsiteUrl, "Website URL");
            ValidateUrlFormat(request.LogoUrl, "Logo URL");

            // VALIDATION 2: Kiểm tra tên trùng lặp
            if (await _unitOfWork.PublisherRepository.GetPublisherByNameAsync(request.Name) != null)
            {
                throw new BadRequestException($"Tên nhà xuất bản '{request.Name}' đã tồn tại.");
            }

            var publisherObj = new Publisher
            {
                PublisherId = Guid.NewGuid().ToString(),
                Name = request.Name.Trim(), // Trim() để loại bỏ khoảng trắng thừa
                Description = request.Description,
                WebsiteUrl = request.WebsiteUrl,
                LogoUrl = request.LogoUrl,
            };

            await _unitOfWork.PublisherRepository.CreatePublisher(publisherObj);
            return publisherObj.PublisherId;
        }

        public async Task<int> UpdatePublisherAsync(string publisherId, PublisherRequest request)
        {
            // Lấy đối tượng cần cập nhật
            var publisherFound = await _unitOfWork.PublisherRepository.GetPublisherByIdAsync(publisherId);
            if (publisherFound == null)
            {
                throw new NotFoundException("Nhà xuất bản không tìm thấy.");
            }

            // VALIDATION 1: Kiểm tra các giá trị đầu vào
            if (request == null)
                throw new BadRequestException("Dữ liệu đầu vào không được để trống.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new BadRequestException("Tên nhà xuất bản là bắt buộc.");

            ValidateUrlFormat(request.WebsiteUrl, "Website URL");
            ValidateUrlFormat(request.LogoUrl, "Logo URL");

            // VALIDATION 2: Kiểm tra tên trùng lặp (phức tạp hơn)
            if (!publisherFound.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase))
            {
                var existingPublisher = await _unitOfWork.PublisherRepository.GetPublisherByNameAsync(request.Name);
                if (existingPublisher != null && existingPublisher.PublisherId != publisherId)
                {
                    throw new BadRequestException($"Tên nhà xuất bản '{request.Name}' đã được sử dụng bởi một nhà xuất bản khác.");
                }
            }

            // Cập nhật các trường
            publisherFound.Name = request.Name.Trim();
            publisherFound.Description = request.Description;
            publisherFound.WebsiteUrl = request.WebsiteUrl;
            publisherFound.LogoUrl = request.LogoUrl;

            return await _unitOfWork.PublisherRepository.UpdatePublisherAsync(publisherFound);
        }

        public async Task<bool> DeletePublisherAsync(string publisherId)
        {
            var publisher = await _unitOfWork.PublisherRepository.GetPublisherByIdAsync(publisherId);
            if (publisher == null)
            {
                // Nếu không tìm thấy, có thể coi như đã xóa thành công
                return true;
            }

            // VALIDATION 3: Kiểm tra tham chiếu trước khi xóa
            if (await _unitOfWork.PublisherRepository.IsPublisherBeingUsedAsync(publisherId))
            {
                throw new BadRequestException("Không thể xóa nhà xuất bản này vì đang được sử dụng bởi một hoặc nhiều hội nghị.");
            }

            return await _unitOfWork.PublisherRepository.DeletePublisherAsync(publisher);
        }

        public async Task<List<PublisherResponse>> GetAllPublishersAsync()
        {
            var publishers = await _unitOfWork.PublisherRepository.GetAllPublishersAsync();
            return publishers.Select(p => p.FromModel()).ToList();
        }

        public async Task<PublisherResponse> GetPublisherByIdAsync(string publisherId)
        {
            var result = await _unitOfWork.PublisherRepository.GetPublisherByIdAsync(publisherId);
            if (result == null)
            {
                throw new NotFoundException("Nhà xuất bản không tìm thấy");
            }
            return result.FromModel();
        }

        // --- Helper Method ---
        private void ValidateUrlFormat(string url, string fieldName)
        {
            if (!string.IsNullOrEmpty(url) && !Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                throw new BadRequestException($"Định dạng của '{fieldName}' không phải là một URL hợp lệ.");
            }
        }
    }
}