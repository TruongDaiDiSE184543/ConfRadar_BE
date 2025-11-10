using ConfRadar.Repositories;
using ConfRadar.Services.DTOs.Paper;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Mappers;

namespace ConfRadar.Services.Services
{
    public interface IPaperAssignmentService
    {
        Task<string> AssignAuthorToPaper(AssignAuthorToPaperRequest request);
        Task<string> AssignReviewerToPaper(AssignReviewerToPaperRequest request);
    }
    public class PaperAssignmentService : IPaperAssignmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PaperAssignmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<string> AssignAuthorToPaper(AssignAuthorToPaperRequest request)
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

            // Validate that the user has the 'Customer' role
            var customerRole = await _unitOfWork.RoleRepository.GetRoleByRoleName("Customer");
            if (customerRole == null)
            {
                throw new BadRequestException("Customer role does not exist in the system.");
            }

            var userRoles = await _unitOfWork.UserRoleRepository
                .GetMutipleUserRolesByUserId(request.UserId);

            var hasCustomerRole = userRoles.Any(ur => ur.RoleId == customerRole.RoleId);
            if (!hasCustomerRole)
            {
                throw new BadRequestException($"User with ID {request.UserId} does not have the Customer role.");
            }

            // Check if the user is already assigned to this paper
            var existingPaperAuthor = await _unitOfWork.PaperAuthorRepository
                .GetPaperAuthorByIdAsync(request.UserId, request.PaperId);

            if (existingPaperAuthor != null)
            {
                throw new BadRequestException($"User with ID {request.UserId} is already assigned as an author to paper with ID {request.PaperId}.");
            }

            // Create the paper author assignment
            var paperAuthor = request.ToModel();
            await _unitOfWork.PaperAuthorRepository.CreatePaperAuthorAsync(paperAuthor);

            await _unitOfWork.SaveChangesAsync();

            return $"User with ID {request.UserId} has been successfully assigned as an author to paper with ID {request.PaperId}.";
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
            var paperReviewer = request.ToModel();
            await _unitOfWork.PaperReviewerRepository.CreatePaperReviewerAsync(paperReviewer);

            await _unitOfWork.SaveChangesAsync();

            var reviewerType = hasLocalReviewerRole ? "Local Reviewer" : "External Reviewer";
            var headReviewerStatus = request.IsHeadReviewer ? " as a head reviewer" : "";

            return $"User with ID {request.UserId} ({reviewerType}) has been successfully assigned to paper with ID {request.PaperId}{headReviewerStatus}.";
        }
    }
}