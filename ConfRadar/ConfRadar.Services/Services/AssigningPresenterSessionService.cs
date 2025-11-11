using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.Paper;
using ConfRadar.Services.DTOs.PresenterSession;
using ConfRadar.Services.Exceptions;

namespace ConfRadar.Services.Services
{
    public interface IAssigningPresenterSessionService
    {
        Task<PresenterSessionResponse> AssignPresenterToSession(string paperId, string sessionId); //paperid and session need to exist, if there is already a record for the paper then throw exception, if paperId is from the paper whose cameraready is not complete then throw exception also, if passes all then insert a record into presentauthor then change the usercheckin of the user who is the root author of this paper check the paperauthor table turn that record where has the userid and session and make the ispresenter to true
        Task<PresenterSessionResponse> GetPresentSessionbySessionAndPaperid(string sessionId, string paperId);
        Task<List<PaperDetailResponseDtoDetail>> GetAllAcceptedPaper();
        Task<ConfRadar.Services.DTOs.PresenterSession.PresenterChangeRequest> ChangePresenterSession(string currentRootAuthorId, CreatePresenterChangeRequest request); //check if paper and user exist, user is author of paper in the paperauthor is the user whose record in paper author ispresenter is true and the same as the request.newuserid? can't change to the same user, check if paper is complete throw exception if not, check if this new userId already bought a conferenceprice of this conference (just check to see the conferenceprice) and have a conferenceprice of type isauthor = true so they are eligible to be nominated as the new presenter of paper
        Task<string> ApprovePresenterChangeRequest(ApprovePresenterChangeRequest request, string approvedById);
        Task<List<ConfRadar.Services.DTOs.PresenterSession.PresenterChangeRequest>> GetPendingPresenterChangeRequests();

        //Task<string> CreateSessionChangeRequest(CreateSessionChangeRequest request, string requestedById);
        //Task<List<ConfRadar.Services.DTOs.PresenterSession.SessionChangeRequestResponse>> GetPendingSessionChangeRequests();
        //Task<string> ApproveSessionChangeRequest(ApproveSessionChangeRequest request, string approvedById);
    }

    public class AssigningPresenterSessionService : IAssigningPresenterSessionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;

        public AssigningPresenterSessionService(IUnitOfWork unitOfWork, ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
        }
        public async Task<List<PaperDetailResponseDtoDetail>> GetAllAcceptedPaper()
        {
            var acceptedStatus = _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription()).Result;
            var list = await _unitOfWork.PaperRepository.GetAllAcceptedPaper(acceptedStatus);
            List<PaperDetailResponseDtoDetail> paperDetailResponseDTOs = list.Select(paper => new PaperDetailResponseDtoDetail
            {
                PaperId = paper.PaperId,
                Title = paper.Title,
                Description = paper.Description,
            }).ToList();
            return paperDetailResponseDTOs;
        }


        // make a helper class to check if a paper is in complete form aka camera ready of it is in accepted status, use  the globalstatusenum to get accepted string then use that to get paper whose camera global status id = acceptedid
        private async Task<bool> IsPaperCameraReadyAndAccepted(string paperId)
        {
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(paperId);
            if (paper == null)
            {
                return false;
            }

            if (paper.CameraReadyId == null)
            {
                return false;
            }

            var cameraReady = await _unitOfWork.CameraReadyRepository.GetCameraReadyByIdAsync(paper.CameraReadyId);
            if (cameraReady == null)
            {
                return false;
            }

            var acceptedStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription());

            return cameraReady.GlobalStatusId == acceptedStatus.GlobalStatusId;
        }

        public async Task<PresenterSessionResponse> AssignPresenterToSession(string paperId, string sessionId)
        {
            // Check if paper exists
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(paperId);
            if (paper == null)
            {
                throw new BadRequestException($"Paper với ID {paperId} không tồn tại.");
            }

            // Check if session exists
            var session = await _unitOfWork.ConferenceSessionRepository.GetConferenceSessionByIdAsync(sessionId);
            if (session == null)
            {
                throw new BadRequestException($"Session với ID {sessionId} không tồn tại.");
            }

            // Check if there is already a record for the paper
            var existingPresentAuthor = await _unitOfWork.PresentAuthorRepository.GetPresentAuthorByPaperIdAsync(paperId);
            if (existingPresentAuthor != null)
            {
                throw new BadRequestException($"Paper với ID {paperId} đã có presenter ở session {existingPresentAuthor.ConferenceSessionId} rồi không thể gán ở session {sessionId} nữa.");
            }

            // Check if paper is in complete form (camera ready and accepted)
            bool isPaperComplete = await IsPaperCameraReadyAndAccepted(paperId);
            if (!isPaperComplete)
            {
                throw new BadRequestException($"Paper với ID {paperId} cameready chưa có hoặc chưa được chấp nhập.");
            }

            // Get the root author of the paper from PaperAuthor table
            var paperAuthors = await _unitOfWork.PaperAuthorRepository.GetPaperAuthorsByPaperIdAsync(paperId);
            var rootAuthor = paperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true && pa.IsPresenter == true);
            if (rootAuthor == null)
            {
                throw new BadRequestException($"Không tìm thấy người author nộp cũng là ngưởi presenter cho paper ID {paperId}.");
            }
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Create PresentAuthor record
                var presentAuthor = new PresentAuthor
                {
                    ConferenceSessionId = sessionId,
                    PaperId = paperId,
                    AssignedAt = ExtensionHelper.GetVietnamTime()
                };

                await _unitOfWork.PresentAuthorRepository.CreatePresentAuthorAsync(presentAuthor);

                // Update UserCheckIn record: find the record with the user and session, and make the ispresenter to true
                var userCheckIn = await _unitOfWork.UserCheckInRepository.GetUserCheckInByUserAndSessionAsync(rootAuthor.UserId, sessionId);
                if (userCheckIn != null)
                {
                    userCheckIn.IsPresenter = true;
                    await _unitOfWork.UserCheckInRepository.UpdateUserCheckInAsync(userCheckIn);
                }
                else
                {
                    // If no check-in record exists, we might need to create one, but typically the user should already be checked in
                    // For now, just log this case - the user might not be checked in yet
                }

                // Update the IsPresenter field in PaperAuthor table
                //var paperAuthor = paperAuthors.FirstOrDefault(pa => pa.UserId == rootAuthor.UserId);
                //if (paperAuthor != null)
                //{
                //    paperAuthor.IsPresenter = true;
                //    await _unitOfWork.PaperAuthorRepository.UpdatePaperAuthorAsync(paperAuthor);
                //}
                await _unitOfWork.CommitAsync();

                // Return the response
                return new PresenterSessionResponse
                {
                    ConferenceSessionId = sessionId,
                    PaperId = paperId,
                    AssignedAt = DateTime.UtcNow,
                    PaperTitle = paper.Title,
                    PresenterName = rootAuthor.User?.FullName, // Assuming User has FullName
                    UserId = rootAuthor.UserId
                };

            }
            catch (Exception e)
            {
                await _unitOfWork.RollbackAsync();
                throw e;
            }

        }

        public async Task<List<PresenterSessionResponse>> GetAllPresenterResponse()
        {
            var presentAuthors = await _unitOfWork.PresentAuthorRepository.GetAllPresentAuthorsAsync();
            var responses = new List<PresenterSessionResponse>();

            foreach (var presentAuthor in presentAuthors)
            {
                var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(presentAuthor.PaperId);
                var session = await _unitOfWork.ConferenceSessionRepository.GetConferenceSessionByIdAsync(presentAuthor.ConferenceSessionId);

                // Get the presenter for this paper
                var paperAuthors = await _unitOfWork.PaperAuthorRepository.GetPaperAuthorsByPaperIdAsync(presentAuthor.PaperId);
                var presenterAuthor = paperAuthors.FirstOrDefault(pa => pa.IsPresenter == true);

                responses.Add(new PresenterSessionResponse
                {
                    ConferenceSessionId = presentAuthor.ConferenceSessionId,
                    PaperId = presentAuthor.PaperId,
                    AssignedAt = presentAuthor.AssignedAt,
                    PaperTitle = paper?.Title,
                    PresenterName = presenterAuthor?.User?.FullName,
                    UserId = presenterAuthor?.UserId
                });
            }

            return responses;
        }

        public async Task<PresenterSessionResponse> GetPresentSessionbySessionAndPaperid(string sessionId, string paperId)
        {

            var presentAuthor = await _unitOfWork.PresentAuthorRepository.GetPresentAuthorByIdAsync(sessionId, paperId);
            if (presentAuthor == null)
            {
                throw new BadRequestException($"PresentAuthor với Session ID {sessionId} với Paper ID {paperId} Không tìm thấy.");
            }

            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(presentAuthor.PaperId);
            var paperAuthors = await _unitOfWork.PaperAuthorRepository.GetPaperAuthorsByPaperIdAsync(presentAuthor.PaperId);
            var presenterAuthor = paperAuthors.FirstOrDefault(pa => pa.IsPresenter == true);

            return new PresenterSessionResponse
            {
                ConferenceSessionId = presentAuthor.ConferenceSessionId,
                PaperId = presentAuthor.PaperId,
                AssignedAt = presentAuthor.AssignedAt,
                PaperTitle = paper?.Title,
                PresenterName = presenterAuthor?.User?.FullName,
                UserId = presenterAuthor?.UserId
            };
        }

        public async Task<ConfRadar.Services.DTOs.PresenterSession.PresenterChangeRequest> ChangePresenterSession(string currentRootAuthorId, CreatePresenterChangeRequest request)
        {


            // Check if paper exists
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
            if (paper == null)
            {
                throw new BadRequestException($"Paper với ID {request.PaperId} không tồn tại.");
            }

            // Check if new user exists
            var newUser = await _unitOfWork.UserRepository.GetUserByUserId(request.NewUserId);
            if (newUser == null)
            {
                throw new BadRequestException($"User với ID {request.NewUserId} không tồn tại.");
            }



            // Check if user is author of paper in the PaperAuthor table
            var paperAuthors = await _unitOfWork.PaperAuthorRepository.GetPaperAuthorsByPaperIdAsync(request.PaperId);
            var existingPaperAuthor = paperAuthors.FirstOrDefault(pa => pa.UserId == request.NewUserId);
            if (existingPaperAuthor == null)
            {
                throw new BadRequestException($"User với ID {request.NewUserId} không là author của paper với ID {request.PaperId}.");
            }

            //check if the the request user is actually the rootauthor aka the default presenter of the paper
            var currentRootAuthor = paperAuthors.FirstOrDefault(pa => pa.IsRootAuthor == true && pa.UserId == currentRootAuthorId);
            if (currentRootAuthor == null)
            {
                throw new BadRequestException("Bạn không phải là rootauthor của paper bạn không thể nhượng quyền");
            }


            // Check if current presenter is different from new user (can't change to the same user)
            var currentPresenter = paperAuthors.FirstOrDefault(pa => pa.IsPresenter == true);
            if (currentPresenter != null && currentPresenter.UserId == request.NewUserId)
            {
                throw new BadRequestException("Không thể đổi presenter nếu như userId trùng với presenter hiện tại.");
            }


            // Check if paper is complete (camera ready and accepted) 
            bool isPaperComplete = await IsPaperCameraReadyAndAccepted(request.PaperId);
            if (!isPaperComplete)
            {
                throw new BadRequestException($"Paper với ID {request.PaperId} chưa có camera ready hoặc chưa được chấp nhập.");
            }

            // Check if the new user already bought a conference price of this conference and have a conference price of type isauthor = true
            var paperConferenceId = paper.ConferenceId;
            if (paperConferenceId != null)
            {
                var userTicket = await _unitOfWork.TicketRepository.GetTicketByUserIdAndConferenceId(request.NewUserId, paperConferenceId);


                if (userTicket != null && userTicket.PricePhase!.ConferencePrice != null && userTicket.PricePhase.ConferencePrice.IsAuthor == true)
                {
                    throw new BadRequestException($"User ID {request.NewUserId} Không có vé author cho hội nghị nghiên cứu với ID {paperConferenceId}.");
                }
            }

            // Determine who is making the request - for this implementation, we'll use the current presenter if they exist
            string requesterId = currentRootAuthorId; //currentPresenter?.UserId; // The current presenter is requesting the change
            var presentAuthor = await _unitOfWork.PresentAuthorRepository.GetPresentAuthorByPaperIdAsync(request.PaperId);

            // Fetch ticket for the new presenter for this conference
            ConfRadar.Repositories.Models.Ticket? changeRequestTicket = null;
            if (paperConferenceId != null)
            {
                changeRequestTicket = await _unitOfWork.TicketRepository.GetTicketByUserIdAndConferenceId(request.NewUserId, paperConferenceId);
            }
            var pendingStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var changeRequest = new ConfRadar.Repositories.Models.PresenterChangeRequest
                {
                    PresenterChangeRequestId = Guid.NewGuid().ToString(),
                    TicketId = changeRequestTicket?.TicketId, // Use the found ticket
                    RequestedById = requesterId,
                    NewPresenterId = request.NewUserId,
                    Reason = request.Reason,
                    RequestAt = ExtensionHelper.GetVietnamTime(),
                    PaperId = request.PaperId,
                    GlobalStatusId = pendingStatus.GlobalStatusId,

                };


                await _unitOfWork.PresenterChangeRequestRepository.CreatePresenterChangeRequestAsync(changeRequest);

                // Update PaperAuthor to set the new user as presenter and the old presenter as not presenter
                if (currentPresenter != null)
                {
                    currentPresenter.IsPresenter = false;
                    await _unitOfWork.PaperAuthorRepository.UpdatePaperAuthorAsync(currentPresenter);
                }

                existingPaperAuthor.IsPresenter = true;
                await _unitOfWork.PaperAuthorRepository.UpdatePaperAuthorAsync(existingPaperAuthor);

                //// Update the corresponding UserCheckIn records
                //if (currentPresenter != null)
                //{
                //    var oldUserCheckIn = await _unitOfWork.UserCheckInRepository.GetUserCheckInByUserAndSessionAsync(currentPresenter.UserId, request.SessionId);
                //    if (oldUserCheckIn != null)
                //    {
                //        oldUserCheckIn.IsPresenter = false;
                //        await _unitOfWork.UserCheckInRepository.UpdateUserCheckInAsync(oldUserCheckIn);
                //    }
                //}

                //var newUserCheckIn = await _unitOfWork.UserCheckInRepository.GetUserCheckInByUserAndSessionAsync(request.NewUserId, request.SessionId);
                //if (newUserCheckIn != null)
                //{
                //    newUserCheckIn.IsPresenter = true;
                //    await _unitOfWork.UserCheckInRepository.UpdateUserCheckInAsync(newUserCheckIn);
                //}

                await _unitOfWork.CommitAsync();

                // Return DTO instead of model
                return new ConfRadar.Services.DTOs.PresenterSession.PresenterChangeRequest
                {
                    PresenterChangeRequestId = changeRequest.PresenterChangeRequestId,
                    TicketId = changeRequest.TicketId,
                    RequestedById = changeRequest.RequestedById,
                    NewPresenterId = changeRequest.NewPresenterId,
                    Reason = changeRequest.Reason,
                    RequestAt = changeRequest.RequestAt,
                    PaperId = request.PaperId,
                    SessionId = presentAuthor.ConferenceSessionId,
                    GlobalStatusName = changeRequest.GlobalStatus?.Name,
                    NewPresenterName = newUser.FullName,
                    RequestedByName = currentRootAuthor.User.FullName
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw ex;
            }

        }

        public async Task<string> ApprovePresenterChangeRequest(ApprovePresenterChangeRequest request, string approvedById)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var changeRequest = await _unitOfWork.PresenterChangeRequestRepository.GetPresenterChangeRequestByIdAsync(request.PresenterChangeRequestId);
                if (changeRequest == null)
                {
                    throw new BadRequestException($"Không tìm thấy yêu cầu thay đổi người trình bày với ID {request.PresenterChangeRequestId}.");
                }

                ConfRadar.Repositories.Models.GlobalStatus targetStatus;
                string resultMessage;

                if (request.IsApproved)
                {

                    // Get the accepted global status
                    targetStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription());
                    if (targetStatus == null)
                    {
                        throw new BadRequestException("Không tìm thấy trạng thái chấp nhận.");
                    }

                    //approve the request
                    changeRequest.ReviewedAt = ExtensionHelper.GetVietnamTime();
                    changeRequest.GlobalStatusId = targetStatus.GlobalStatusId;
                    await _unitOfWork.PresenterChangeRequestRepository.UpdatePresenterChangeRequestAsync(changeRequest);


                    resultMessage = "Yêu cầu thay đổi người trình bày đã được chấp thuận thành công.";

                    // Find the paper associated with the new presenter to identify the session
                    // We need to find which paper this presenter is associated with
                    var newPresenterPaperAuthors = await _unitOfWork.PaperAuthorRepository.GetPaperAuthorsByUserIdAsync(changeRequest.NewPresenterId);
                    var newPresenterPaperAuthor = newPresenterPaperAuthors.FirstOrDefault(pa => pa.IsPresenter == true);

                    if (newPresenterPaperAuthor != null)
                    {
                        // Find the session information from PresentAuthor table
                        var targetPresentAuthor = await _unitOfWork.PresentAuthorRepository.GetPresentAuthorByPaperIdAsync(newPresenterPaperAuthor.PaperId);


                        if (targetPresentAuthor != null)
                        {
                            // Update UserCheckIn records to reflect the presenter change
                            // Find the old presenter who was presenting this paper
                            var paperAuthors = await _unitOfWork.PaperAuthorRepository.GetPaperAuthorsByPaperIdAsync(targetPresentAuthor.PaperId);
                            var oldPresenter = paperAuthors.FirstOrDefault(pa => pa.IsPresenter == true && pa.UserId != changeRequest.NewPresenterId);

                            if (oldPresenter != null)
                            {
                                // Update old presenter's UserCheckIn
                                var oldUserCheckIn = await _unitOfWork.UserCheckInRepository.GetUserCheckInByUserAndSessionAsync(oldPresenter.UserId, targetPresentAuthor.ConferenceSessionId);
                                if (oldUserCheckIn != null)
                                {
                                    oldUserCheckIn.IsPresenter = false;
                                    await _unitOfWork.UserCheckInRepository.UpdateUserCheckInAsync(oldUserCheckIn);
                                }
                            }

                            // Update new presenter's UserCheckIn
                            var newUserCheckIn = await _unitOfWork.UserCheckInRepository.GetUserCheckInByUserAndSessionAsync(changeRequest.NewPresenterId, targetPresentAuthor.ConferenceSessionId);
                            if (newUserCheckIn != null)
                            {
                                newUserCheckIn.IsPresenter = true;
                                await _unitOfWork.UserCheckInRepository.UpdateUserCheckInAsync(newUserCheckIn);
                            }
                        }
                    }
                }
                else
                {
                    // Get the rejected global status
                    targetStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Rejected.GetDescription());
                    if (targetStatus == null)
                    {
                        throw new BadRequestException("Không tìm thấy trạng thái từ chối.");
                    }

                    resultMessage = "Yêu cầu thay đổi người trình bày đã bị từ chối.";
                }

                // Update the request status
                changeRequest.GlobalStatusId = targetStatus.GlobalStatusId;
                changeRequest.ReviewedAt = ExtensionHelper.GetVietnamTime();

                await _unitOfWork.PresenterChangeRequestRepository.UpdatePresenterChangeRequestAsync(changeRequest);

                await _unitOfWork.CommitAsync();

                return resultMessage;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw ex;
            }
        }

        public async Task<List<ConfRadar.Services.DTOs.PresenterSession.PresenterChangeRequest>> GetPendingPresenterChangeRequests()
        {
            var pendingStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            if (pendingStatus == null)
            {
                return new List<ConfRadar.Services.DTOs.PresenterSession.PresenterChangeRequest>();
            }

            var allChangeRequests = await _unitOfWork.PresenterChangeRequestRepository.GetAllPresenterChangeRequestsAsync();
            var pendingRequests = allChangeRequests.Where(pcr => pcr.GlobalStatusId == pendingStatus.GlobalStatusId).ToList();

            var responseList = new List<ConfRadar.Services.DTOs.PresenterSession.PresenterChangeRequest>();
            foreach (var request in pendingRequests)
            {
                var requestedUser = await _unitOfWork.UserRepository.GetUserByUserId(request.RequestedById);
                var newPresenterUser = await _unitOfWork.UserRepository.GetUserByUserId(request.NewPresenterId);
                var globalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByIdAsync(request.GlobalStatusId);

                responseList.Add(new ConfRadar.Services.DTOs.PresenterSession.PresenterChangeRequest
                {
                    PresenterChangeRequestId = request.PresenterChangeRequestId,
                    TicketId = request.TicketId,
                    RequestedById = request.RequestedById,
                    RequestedByName = requestedUser?.FullName,
                    NewPresenterId = request.NewPresenterId,
                    NewPresenterName = newPresenterUser?.FullName,
                    GlobalStatusId = request.GlobalStatusId,
                    GlobalStatusName = globalStatus?.Name,
                    Reason = request.Reason,
                    RequestAt = request.RequestAt,
                    ReviewedAt = request.ReviewedAt
                });
            }

            return responseList;
        }

        //public async Task<string> CreateSessionChangeRequest(CreateSessionChangeRequest request, string requestedById)
        //{
        //    var paper =await  _unitOfWork.PaperRepository.GetAllIncludeById(request.PaperId);
        //    if (paper == null) throw new BadRequestException($"Không tìm thấy paper với ID {request.PaperId}");

        //    //check if the request user is the presenter of the paper
        //    var newSession = await _unitOfWork.ConferenceSessionRepository.GetSessionWithDetailsAsync(request.NewSessionId);
        //    if (newSession == null) throw new BadRequestException($"Session {request.NewSessionId} không tồn tại");

        //    var alltAuthors =  await _unitOfWork.PaperAuthorRepository.GetPaperAuthorsByPaperIdAsync(request.PaperId);
        //    var rootAuthor = alltAuthors.FirstOrDefault(a => a.IsPresenter == true);
        //    if (requestedById != rootAuthor.UserId) throw new Exception($"Bạn không là presenter của paper với ID {request.PaperId} để đổi session");

        //    //get current presentauthor
        //    var currentPresentAuthor = await _unitOfWork.PresentAuthorRepository.GetPresentAuthorByPaperIdAsync(request.PaperId);


        //    //get current UserCheckin whose isPresenter is true for this paper 
        //    var currentUserCheckin = await _unitOfWork.UserCheckInRepository.GetUserCheckInByUserAndSessionAsync(requestedById, currentPresentAuthor.ConferenceSessionId);




        //    //get ticket
        //    var ticket = await _unitOfWork.TicketRepository.GetTicketById(currentUserCheckin.TicketId);
        //    //get pending status
        //    var pendingStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());

        //    //check if there alreay is a request for this paper
        //    var existingPendingRequest = await _unitOfWork.SessionChangeRequestRepository.GetSessionChangeRequestByPaperIdAndSessionId(request.PaperId, request.NewSessionId);
        //    if (existingPendingRequest.Any(scr => scr.GlobalStatusId == pendingStatus.GlobalStatusId)) throw new BadRequestException("Đã có yêu cầu request đổi session cho paper này rồi");

        //    //get usercheckin whose isPresenter needs to changed to true
        //    var toNewUserCheckin = await _unitOfWork.UserCheckInRepository.GetUserCheckInByUserAndSessionAsync(requestedById,request.NewSessionId);
        //    if (toNewUserCheckin == null) throw new BadRequestException($"Không tim thấy usercheckin với userId: {requestedById} và session {request.NewSessionId} để cập nhật presenter");

        //    await _unitOfWork.BeginTransactionAsync();
        //    try
        //    {
        //        currentUserCheckin.IsPresenter = false; 
        //        toNewUserCheckin.IsPresenter = true;
        //        await _unitOfWork.UserCheckInRepository.UpdateUserCheckInAsync(currentUserCheckin);
        //        await _unitOfWork.UserCheckInRepository.UpdateUserCheckInAsync(toNewUserCheckin);

        //        SessionChangeRequest sessionChangeRequest = new SessionChangeRequest
        //        {
        //            SessionChangeRequestId = Guid.NewGuid().ToString(),
        //            TicketId = request.TicketId,
        //            NewConferenceSessionId = request.NewSessionId,
        //            Reason = request.Reason,
        //            RequestAt = ExtensionHelper.GetVietnamTime(),
        //            CustomerId = requestedById,
        //            GlobalStatusId = pendingStatus.GlobalStatusId,
        //            PaperId = request.PaperId

        //        };
        //        await _unitOfWork.SessionChangeRequestRepository.CreateSessionChangeRequestAsync(sessionChangeRequest);
        //    }
        //    catch(Exception ex)
        //    {
        //        await _unitOfWork.RollbackAsync();
        //        throw ex;
        //    }
        //}

        public Task<List<SessionChangeRequestResponse>> GetPendingSessionChangeRequests()
        {
            throw new NotImplementedException();
        }

        public Task<string> ApproveSessionChangeRequest(ApproveSessionChangeRequest request, string approvedById)
        {
            throw new NotImplementedException();
        }




        //public async Task<string> CreateSessionChangeRequest(CreateSessionChangeRequest request, string requestedById)
        //{
        //    await _unitOfWork.BeginTransactionAsync();
        //    try
        //    {
        //        // Validate that the requested user exists
        //        var user = await _unitOfWork.UserRepository.GetUserByUserId(requestedById);
        //        if (user == null)
        //        {
        //            throw new BadRequestException("Người yêu cầu không tồn tại.");
        //        }

        //        //validate ticket exists
        //        var ticket = await _unitOfWork.TicketRepository.GetTicketById(request.TicketId);
        //        if (ticket == null) throw new Exception($"Ticket {request.TicketId} không tồn tại");
        //        // Validate that the current session exists
        //        var usercheckin = await _unitOfWork.UserCheckInRepository.GetPresenterByTicket(request.TicketId);
        //        var currentSession = await _unitOfWork.ConferenceSessionRepository.GetConferenceSessionByIdAsync(usercheckin.id);
        //        if (currentSession == null)
        //        {
        //            throw new BadRequestException("Session không tồn tại.");
        //        }

        //        // Validate that the new session exists
        //        var newSession = await _unitOfWork.ConferenceSessionRepository.GetConferenceSessionByIdAsync(request.NewSessionId);
        //        if (newSession == null)
        //        {
        //            throw new BadRequestException("Phiên mới không tồn tại.");
        //        }

        //        // Validate that the paper exists
        //        var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(request.PaperId);
        //        if (paper == null)
        //        {
        //            throw new BadRequestException("Bài báo không tồn tại.");
        //        }

        //        // Check if the user is the presenter of the paper
        //        var paperAuthors = await _unitOfWork.PaperAuthorRepository.GetPaperAuthorsByPaperIdAsync(request.PaperId);
        //        var presenter = paperAuthors.FirstOrDefault(pa => pa.UserId == requestedById && pa.IsPresenter == true);
        //        if (presenter == null)
        //        {
        //            throw new BadRequestException("Người yêu cầu không phải là người trình bày của bài báo này.");
        //        }

        //        // Check if there's already a present author record for this paper
        //        var presentAuthor = await _unitOfWork.PresentAuthorRepository.GetPresentAuthorByPaperIdAsync(request.PaperId);
        //        if (presentAuthor == null)
        //        {
        //            throw new BadRequestException("Bài báo chưa được gán phiên trình bày.");
        //        }

        //        // Check if the current session matches the one in the present author record
        //        if (presentAuthor.ConferenceSessionId != currentSession.ConferenceSessionId)
        //        {
        //            throw new BadRequestException("Phiên hiện tại không khớp với phiên được gán cho bài báo.");
        //        }

        //        // Check if there's already a session change request for this paper that is pending
        //        var existingRequests = await _unitOfWork.SessionChangeRequestRepository.GetAllSessionChangeRequestsAsync();
        //        var pendingRequest = existingRequests.FirstOrDefault(r => 
        //            r.PaperId == request.PaperId && 
        //            r.GlobalStatusId == (await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription())).GlobalStatusId
        //        );

        //        if (pendingRequest != null)
        //        {
        //            throw new BadRequestException("Đã có yêu cầu thay đổi phiên đang chờ xử lý cho bài báo này.");
        //        }

        //        // Create the session change request
        //        var sessionChangeRequest = new ConfRadar.Repositories.Models.SessionChangeRequest
        //        {
        //            SessionChangeRequestId = Guid.NewGuid().ToString(),
        //            CurrentSessionId = currentSession.ConferenceSessionId,
        //            NewSessionId = request.NewSessionId,
        //            PaperId = request.PaperId,
        //            RequestedById = requestedById,
        //            Reason = request.Reason,
        //            RequestAt = ExtensionHelper.GetVietnamTime()
        //        };

        //        // Set the status to pending
        //        var pendingStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
        //        if (pendingStatus == null)
        //        {
        //            throw new BadRequestException("Không tìm thấy trạng thái đang chờ.");
        //        }

        //        sessionChangeRequest.GlobalStatusId = pendingStatus.GlobalStatusId;

        //        await _unitOfWork.SessionChangeRequestRepository.CreateSessionChangeRequestAsync(sessionChangeRequest);
        //        await _unitOfWork.CommitAsync();

        //        return "Yêu cầu thay đổi phiên đã được tạo thành công.";
        //    }
        //    catch (Exception ex)
        //    {
        //        await _unitOfWork.RollbackAsync();
        //        throw ex;
        //    }
        //}

        //public async Task<List<ConfRadar.Services.DTOs.PresenterSession.SessionChangeRequestResponse>> GetPendingSessionChangeRequests()
        //{
        //    var pendingStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
        //    if (pendingStatus == null)
        //    {
        //        return new List<ConfRadar.Services.DTOs.PresenterSession.SessionChangeRequestResponse>();
        //    }

        //    var allChangeRequests = await _unitOfWork.SessionChangeRequestRepository.GetAllSessionChangeRequestsAsync();
        //    var pendingRequests = allChangeRequests.Where(scr => scr.GlobalStatusId == pendingStatus.GlobalStatusId).ToList();

        //    var responseList = new List<ConfRadar.Services.DTOs.PresenterSession.SessionChangeRequestResponse>();
        //    foreach (var request in pendingRequests)
        //    {
        //        var requestedUser = await _unitOfWork.UserRepository.GetByIdAsync(request.RequestedById);
        //        var globalStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByIdAsync(request.GlobalStatusId);

        //        responseList.Add(new ConfRadar.Services.DTOs.PresenterSession.SessionChangeRequestResponse
        //        {
        //            SessionChangeRequestId = request.SessionChangeRequestId,
        //            CurrentSessionId = request.CurrentSessionId,
        //            NewSessionId = request.NewSessionId,
        //            PaperId = request.PaperId,
        //            RequestedById = request.RequestedById,
        //            RequestedByName = requestedUser?.FullName,
        //            GlobalStatusId = request.GlobalStatusId,
        //            GlobalStatusName = globalStatus?.Name,
        //            Reason = request.Reason,
        //            RequestAt = request.RequestAt,
        //            ReviewedAt = request.ReviewedAt
        //        });
        //    }

        //    return responseList;
        //}

        //public async Task<string> ApproveSessionChangeRequest(ApproveSessionChangeRequest request, string approvedById)
        //{
        //    await _unitOfWork.BeginTransactionAsync();
        //    try
        //    {
        //        var changeRequest = await _unitOfWork.SessionChangeRequestRepository.GetSessionChangeRequestByIdAsync(request.SessionChangeRequestId);
        //        if (changeRequest == null)
        //        {
        //            throw new BadRequestException($"Không tìm thấy yêu cầu thay đổi phiên với ID {request.SessionChangeRequestId}.");
        //        }

        //        ConfRadar.Repositories.Models.GlobalStatus targetStatus;
        //        string resultMessage;

        //        if (request.IsApproved)
        //        {
        //            // Get the accepted global status
        //            targetStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription());
        //            if (targetStatus == null)
        //            {
        //                throw new BadRequestException("Không tìm thấy trạng thái chấp nhận.");
        //            }

        //            resultMessage = "Yêu cầu thay đổi phiên đã được chấp thuận thành công.";

        //            // Update the PresentAuthor table to reflect the new session
        //            var presentAuthor = await _unitOfWork.PresentAuthorRepository.GetPresentAuthorByPaperIdAsync(changeRequest.PaperId);
        //            if (presentAuthor != null)
        //            {
        //                presentAuthor.ConferenceSessionId = changeRequest.NewSessionId;
        //                await _unitOfWork.PresentAuthorRepository.UpdatePresentAuthorAsync(presentAuthor);

        //                // Update UserCheckIn records to reflect the session change
        //                // Find all presenters for the paper
        //                var paperAuthors = await _unitOfWork.PaperAuthorRepository.GetPaperAuthorsByPaperIdAsync(changeRequest.PaperId);
        //                var presenters = paperAuthors.Where(pa => pa.IsPresenter == true).ToList();

        //                foreach (var presenter in presenters)
        //                {
        //                    // Update old session check-in (current session)
        //                    var oldUserCheckIn = await _unitOfWork.UserCheckInRepository.GetUserCheckInByUserAndSessionAsync(presenter.UserId, changeRequest.CurrentSessionId);
        //                    if (oldUserCheckIn != null)
        //                    {
        //                        oldUserCheckIn.IsPresenter = false;
        //                        await _unitOfWork.UserCheckInRepository.UpdateUserCheckInAsync(oldUserCheckIn);
        //                    }

        //                    // Update new session check-in (new session)
        //                    var newUserCheckIn = await _unitOfWork.UserCheckInRepository.GetUserCheckInByUserAndSessionAsync(presenter.UserId, changeRequest.NewSessionId);
        //                    if (newUserCheckIn != null)
        //                    {
        //                        newUserCheckIn.IsPresenter = true;
        //                        await _unitOfWork.UserCheckInRepository.UpdateUserCheckInAsync(newUserCheckIn);
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            // Get the rejected global status
        //            targetStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Rejected.GetDescription());
        //            if (targetStatus == null)
        //            {
        //                throw new BadRequestException("Không tìm thấy trạng thái từ chối.");
        //            }

        //            resultMessage = "Yêu cầu thay đổi phiên đã bị từ chối.";
        //        }

        //        // Update the request status
        //        changeRequest.GlobalStatusId = targetStatus.GlobalStatusId;
        //        changeRequest.ReviewedAt = ExtensionHelper.GetVietnamTime();

        //        await _unitOfWork.SessionChangeRequestRepository.UpdateSessionChangeRequestAsync(changeRequest);

        //        await _unitOfWork.CommitAsync();

        //        return resultMessage;
        //    }
        //    catch (Exception ex)
        //    {
        //        await _unitOfWork.RollbackAsync();
        //        throw ex;
        //    }
        //}
    }
}
