//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using ConfRadar.Repositories.Models;
//using ConfRadar.Services.DTOs.PresenterSession;
//using ConfRadar.Services.Common;
//using ConfRadar.Services.Exceptions;
//using ConfRadar.Repositories;

//namespace ConfRadar.Services.Services
//{
//    public interface IAssigningPresenterSessionService
//    {
//        Task<PresenterSessionResponse> AssignPresenterToSession(string paperId, string sessionId); //paperid and session need to exist, if there is already a record for the paper then throw exception, if paperId is from the paper whose cameraready is not complete then throw exception also, if passes all then insert a record into presentauthor then change the usercheckin of the user who is the root author of this paper check the paperauthor table turn that record where has the userid and session and make the ispresenter to true
//        Task<List<PresenterSessionResponse>> GetAllPresenterResponse();
//        Task<PresenterSessionResponse> Getbyid(string id);
//        Task<ConfRadar.Repositories.Models.PresenterChangeRequest> ChangePresenterSession(string paperId, CreatePresenterChangeRequest request); //check if paper and user exist, user is author of paper in the paperauthor is the user whose record in paper author ispresenter is true and the same as the request.newuserid? can't change to the same user, check if paper is complete throw exception if not, check if this new userId already bought a conferenceprice of this conference (just check to see the conferenceprice) and have a conferenceprice of type isauthor = true so they are eligible to be nominated as the new presenter of paper
//    }

//    public class AssigningPresenterSessionService : IAssigningPresenterSessionService
//    {
//        private readonly IUnitOfWork _unitOfWork;
//        private readonly ITokenService _tokenService;

//        public AssigningPresenterSessionService(IUnitOfWork unitOfWork, ITokenService tokenService)
//        {
//            _unitOfWork = unitOfWork;
//            _tokenService = tokenService;
//        }

//        //make a helper class to check if a paper is in complete form aka camera ready of it is in accepted status, use  the globalstatusenum to get accepted string then use that to get paper whose camera global status id = acceptedid
//        private async Task<bool> IsPaperCameraReadyAndAccepted(string paperId)
//        {
//            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(paperId);
//            if (paper == null)
//            {
//                return false;
//            }

//            if (paper.CameraReadyId == null)
//            {
//                return false;
//            }

//            var cameraReady = await _unitOfWork.CameraReadyRepository.GetCameraReadyByIdAsync(paper.CameraReadyId);
//            if (cameraReady == null || cameraReady.GlobalStatus == null)
//            {
//                return false;
//            }

//            var acceptedStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription());
//            if (acceptedStatus == null)
//            {
//                return false;
//            }

//            return cameraReady.GlobalStatusId == acceptedStatus.GlobalStatusId;
//        }

//        public async Task<PresenterSessionResponse> AssignPresenterToSession(string paperId, string sessionId)
//        {
//            // Check if paper exists
//            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(paperId);
//            if (paper == null)
//            {
//                throw new BadRequestException($"Paper với ID {paperId} không tồn tại.");
//            }

//            // Check if session exists
//            var session = await _unitOfWork.ConferenceSessionRepository.GetConferenceSessionByIdAsync(sessionId);
//            if (session == null)
//            {
//                throw new BadRequestException($"Session với ID {sessionId} không tồn tại.");
//            }

//            // Check if there is already a record for the paper
//            var existingPresentAuthor = await _unitOfWork.PresentAuthorRepository.GetPresentAuthorByPaperIdAsync(paperId);
//            if (existingPresentAuthor != null)
//            {
//                throw new BadRequestException($"Paper với ID {paperId} đã có presenter ở session cụ thể rồi.");
//            }

//            // Check if paper is in complete form (camera ready and accepted)
//            bool isPaperComplete = await IsPaperCameraReadyAndAccepted(paperId);
//            if (!isPaperComplete)
//            {
//                throw new BadRequestException($"Paper với ID {paperId} cameready chưa có hoặc chưa được chấp nhập.");
//            }

//            // Get the root author of the paper from PaperAuthor table
//            var paperAuthors = await _unitOfWork.PaperAuthorRepository.GetPaperAuthorsByPaperIdAsync(paperId);
//            var rootAuthor = paperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true && pa.IsPresenter ==true);
//            if (rootAuthor == null)
//            {
//                throw new BadRequestException($"Không tìm thấy người author nộp cũng là ngưởi presenter cho paper ID {paperId}.");
//            }

//            // Create PresentAuthor record
//            try
//            {
//                var presentAuthor = new PresentAuthor
//                {
//                    ConferenceSessionId = sessionId,
//                    PaperId = paperId,
//                    AssignedAt = ExtensionHelper.GetVietnamTime()
//                };

//                await _unitOfWork.PresentAuthorRepository.CreatePresentAuthorAsync(presentAuthor);

//                // Update UserCheckIn record: find the record with the user and session, and make the ispresenter to true
//                var userCheckIn = await _unitOfWork.UserCheckInRepository.GetUserCheckInByUserAndSessionAsync(rootAuthor.UserId, sessionId);
//                if (userCheckIn != null)
//                {
//                    userCheckIn.IsPresenter = true;
//                    await _unitOfWork.UserCheckInRepository.UpdateUserCheckInAsync(userCheckIn);
//                }
//                else
//                {
//                    // If no check-in record exists, we might need to create one, but typically the user should already be checked in
//                    // For now, just log this case - the user might not be checked in yet
//                }

//                // Update the IsPresenter field in PaperAuthor table
//                var paperAuthor = paperAuthors.FirstOrDefault(pa => pa.UserId == rootAuthor.UserId);
//                if (paperAuthor != null)
//                {
//                    paperAuthor.IsPresenter = true;
//                    await _unitOfWork.PaperAuthorRepository.UpdatePaperAuthorAsync(paperAuthor);
//                }
//                await _unitOfWork.CommitAsync();

//                // Return the response
//                return new PresenterSessionResponse
//                {
//                    PresentAuthorId = $"{sessionId}_{paperId}",
//                    ConferenceSessionId = sessionId,
//                    PaperId = paperId,
//                    AssignedAt = DateTime.UtcNow,
//                    PaperTitle = paper.Title,
//                    SessionName = session.SessionName,
//                    PresenterName = rootAuthor.User?.FullName, // Assuming User has FullName
//                    UserId = rootAuthor.UserId
//                };

//            }catch(Exception e)
//            {
//                await _unitOfWork.RollbackAsync();
//                throw e;
//            }
          
//        }

//        public async Task<List<PresenterSessionResponse>> GetAllPresenterResponse()
//        {
//            var presentAuthors = await _unitOfWork.PresentAuthorRepository.GetAllPresentAuthorsAsync();
//            var responses = new List<PresenterSessionResponse>();

//            foreach (var presentAuthor in presentAuthors)
//            {
//                var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(presentAuthor.PaperId);
//                var session = await _unitOfWork.ConferenceSessionRepository.GetConferenceSessionByIdAsync(presentAuthor.ConferenceSessionId);
                
//                // Get the presenter for this paper
//                var paperAuthors = await _unitOfWork.PaperAuthorRepository.GetPaperAuthorsByPaperIdAsync(presentAuthor.PaperId);
//                var presenterAuthor = paperAuthors.FirstOrDefault(pa => pa.IsPresenter == true);

//                responses.Add(new PresenterSessionResponse
//                {
//                    PresentAuthorId = $"{presentAuthor.ConferenceSessionId}_{presentAuthor.PaperId}",
//                    ConferenceSessionId = presentAuthor.ConferenceSessionId,
//                    PaperId = presentAuthor.PaperId,
//                    AssignedAt = presentAuthor.AssignedAt,
//                    PaperTitle = paper?.Title,
//                    SessionName = session?.SessionName,
//                    PresenterName = presenterAuthor?.User?.FullName,
//                    UserId = presenterAuthor?.UserId
//                });
//            }

//            return responses;
//        }

//        public async Task<PresenterSessionResponse> Getbyid(string id)
//        {
//            // Since PresentAuthor uses a composite key, we need to parse the id parameter
//            // Assuming the ID format is "sessionId_paperId" based on how it's constructed in other methods
//            var parts = id.Split('_');
//            if (parts.Length != 2)
//            {
//                throw new BadRequestException("Invalid ID format. Expected: sessionId_paperId");
//            }

//            var sessionId = parts[0];
//            var paperId = parts[1];

//            var presentAuthor = await _unitOfWork.PresentAuthorRepository.GetPresentAuthorByIdAsync(sessionId, paperId);
//            if (presentAuthor == null)
//            {
//                throw new BadRequestException($"PresentAuthor with Session ID {sessionId} and Paper ID {paperId} not found.");
//            }

//            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(presentAuthor.PaperId);
//            var session = await _unitOfWork.ConferenceSessionRepository.GetConferenceSessionByIdAsync(presentAuthor.ConferenceSessionId);
//            var paperAuthors = await _unitOfWork.PaperAuthorRepository.GetPaperAuthorsByPaperIdAsync(presentAuthor.PaperId);
//            var presenterAuthor = paperAuthors.FirstOrDefault(pa => pa.IsPresenter == true);

//            return new PresenterSessionResponse
//            {
//                PresentAuthorId = id,
//                ConferenceSessionId = presentAuthor.ConferenceSessionId,
//                PaperId = presentAuthor.PaperId,
//                AssignedAt = presentAuthor.AssignedAt,
//                PaperTitle = paper?.Title,
//                SessionName = session?.SessionName,
//                PresenterName = presenterAuthor?.User?.FullName,
//                UserId = presenterAuthor?.UserId
//            };
//        }

//        public async Task<ConfRadar.Repositories.Models.PresenterChangeRequest> ChangePresenterSession(string paperId, CreatePresenterChangeRequest request)
//        {
//            // Check if paper exists
//            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(paperId);
//            if (paper == null)
//            {
//                throw new BadRequestException($"Paper with ID {paperId} does not exist.");
//            }

//            // Check if new user exists
//            var newUser = await _unitOfWork.UserRepository.GetByIdAsync(request.NewUserId);
//            if (newUser == null)
//            {
//                throw new BadRequestException($"User with ID {request.NewUserId} does not exist.");
//            }

//            // Check if user is author of paper in the PaperAuthor table
//            var paperAuthors = await _unitOfWork.PaperAuthorRepository.GetPaperAuthorsByPaperIdAsync(paperId);
//            var existingPaperAuthor = paperAuthors.FirstOrDefault(pa => pa.UserId == request.NewUserId);
//            if (existingPaperAuthor == null)
//            {
//                throw new BadRequestException($"User with ID {request.NewUserId} is not an author of paper with ID {paperId}.");
//            }

//            // Check if current presenter is different from new user (can't change to the same user)
//            var currentPresenter = paperAuthors.FirstOrDefault(pa => pa.IsPresenter == true);
//            if (currentPresenter != null && currentPresenter.UserId == request.NewUserId)
//            {
//                throw new BadRequestException("Cannot change presenter to the same user.");
//            }

//            // Check if paper is complete (camera ready and accepted) 
//            bool isPaperComplete = await IsPaperCameraReadyAndAccepted(paperId);
//            if (!isPaperComplete)
//            {
//                throw new BadRequestException($"Paper with ID {paperId} is not in complete form or not accepted.");
//            }

//            // Check if the new user already bought a conference price of this conference and have a conference price of type isauthor = true
//            var paperConferenceId = paper.ConferenceId;
//            if (paperConferenceId != null)
//            {
//                var userTicket = await _unitOfWork.TicketRepository.GetTicketByUserIdAndConferenceIdAsync(request.NewUserId, paperConferenceId);
//                bool hasAuthorTicket = false;

//                if (userTicket != null && userTicket.ConferencePrice != null && userTicket.ConferencePrice.IsAuthor == true)
//                {
//                    hasAuthorTicket = true;
//                }

//                if (!hasAuthorTicket)
//                {
//                    throw new BadRequestException($"New user with ID {request.NewUserId} does not have an author ticket for this conference.");
//                }
//            }

//            // Determine who is making the request - for this implementation, we'll use the current presenter if they exist
//            string requesterId = currentPresenter?.UserId; // The current presenter is requesting the change

//            // Fetch ticket for the new presenter for this conference
//            var paperConferenceId = paper.ConferenceId;
//            ConfRadar.Repositories.Models.Ticket? changeRequestTicket = null;
//            if (paperConferenceId != null)
//            {
//                changeRequestTicket = await _unitOfWork.TicketRepository.GetTicketByUserIdAndConferenceIdAsync(request.NewUserId, paperConferenceId);
//            }

//            // Create a presenter change request record
//            var changeRequest = new ConfRadar.Repositories.Models.PresenterChangeRequest
//            {
//                PresenterChangeRequestId = Guid.NewGuid().ToString(),
//                TicketId = changeRequestTicket?.TicketId, // Use the found ticket
//                RequestedById = requesterId, 
//                NewPresenterId = request.NewUserId,
//                Reason = request.Reason,
//                RequestAt = DateTime.UtcNow
//            };

//            var pendingStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
//            if (pendingStatus != null)
//            {
//                changeRequest.GlobalStatusId = pendingStatus.GlobalStatusId;
//            }

//            await _unitOfWork.PresenterChangeRequestRepository.CreatePresenterChangeRequestAsync(changeRequest);

//            // Update PaperAuthor to set the new user as presenter and the old presenter as not presenter
//            if (currentPresenter != null)
//            {
//                currentPresenter.IsPresenter = false;
//                await _unitOfWork.PaperAuthorRepository.UpdatePaperAuthorAsync(currentPresenter);
//            }

//            existingPaperAuthor.IsPresenter = true;
//            await _unitOfWork.PaperAuthorRepository.UpdatePaperAuthorAsync(existingPaperAuthor);

//            // Update the corresponding UserCheckIn records
//            if (currentPresenter != null)
//            {
//                var oldUserCheckIn = await _unitOfWork.UserCheckInRepository.GetUserCheckInByUserAndSessionAsync(currentPresenter.UserId, request.SessionId);
//                if (oldUserCheckIn != null)
//                {
//                    oldUserCheckIn.IsPresenter = false;
//                    await _unitOfWork.UserCheckInRepository.UpdateUserCheckInAsync(oldUserCheckIn);
//                }
//            }

//            var newUserCheckIn = await _unitOfWork.UserCheckInRepository.GetUserCheckInByUserAndSessionAsync(request.NewUserId, request.SessionId);
//            if (newUserCheckIn != null)
//            {
//                newUserCheckIn.IsPresenter = true;
//                await _unitOfWork.UserCheckInRepository.UpdateUserCheckInAsync(newUserCheckIn);
//            }
//            // Note: If no check-in record exists for the new presenter, they may need to check in first before being able to present

//            return changeRequest; // Return the repository model
//        }
//    }
//}
