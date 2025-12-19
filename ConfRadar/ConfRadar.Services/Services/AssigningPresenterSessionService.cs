using ConfRadar.Repositories;
using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.Paper;
using ConfRadar.Services.DTOs.PresenterSession;
using ConfRadar.Services.Exceptions;
using ConfRadar.Services.Mappers;
using System.Drawing.Printing;

namespace ConfRadar.Services.Services
{
    public interface IAssigningPresenterSessionService
    {
        Task<PresenterSessionResponse> AssignPresenterToSession(string paperId, string sessionId); //paperid and session need to exist, if there is already a record for the paper then throw exception, if paperId is from the paper whose cameraready is not complete then throw exception also, if passes all then insert a record into presentauthor then change the usercheckin of the user who is the root author of this paper check the paperauthor table turn that record where has the userid and session and make the ispresenter to true
        Task<PresenterSessionResponse> GetPresentSessionbySessionAndPaperid(string sessionId, string paperId);
        Task<List<PresenterSessionResponse>> GetAllPresenterResponse(string confId);
        Task<List<PaperDetailResponseDtoDetail>> GetAllAcceptedPaper(string confId);
        //Task<List<PaperDetailResponseDtoDetail>> GetAllAcceptedPaperInSession(string sessionId);
        Task<ConfRadar.Services.DTOs.PresenterSession.PresenterChangeRequest> ChangePresenterSession(string currentRootAuthorId, CreatePresenterChangeRequest request); //check if paper and user exist, user is author of paper in the paperauthor is the user whose record in paper author ispresenter is true and the same as the request.newuserid? can't change to the same user, check if paper is complete throw exception if not, check if this new userId already bought a conferenceprice of this conference (just check to see the conferenceprice) and have a conferenceprice of type isauthor = true so they are eligible to be nominated as the new presenter of paper
        Task<bool> ApprovePresenterChangeRequest(ApprovePresenterChangeRequest request, string approvedById);
        Task<List<ConfRadar.Services.DTOs.PresenterSession.PresenterChangeRequest>> GetPendingPresenterChangeRequests(string confId);

        Task<SessionChangeRequestResponse> CreateSessionChangeRequest(CreateSessionChangeRequest request, string requestedById);
        Task<List<ConfRadar.Services.DTOs.PresenterSession.SessionChangeRequestResponse>> GetPendingSessionChangeRequests(string confId);
        Task<bool> ApproveSessionChangeRequest(ApproveSessionChangeRequest request, string approvedById);
        Task<bool> Unassign(string paperId, string sessionId);
    }

    public class AssigningPresenterSessionService : IAssigningPresenterSessionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly ITimeProviderService _timeProviderService;

        public AssigningPresenterSessionService(IUnitOfWork unitOfWork, ITokenService tokenService, ITimeProviderService timeProviderService)
        {
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _timeProviderService = timeProviderService;
        }
        #region: helper
        private async Task checkConference(Conference conf)
        {
            var today = await _timeProviderService.GetVietnamDate();
            var statusComplete = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Completed.GetDescription());
            var statusCancelled = await _unitOfWork.ConferenceStatusRepository.GetConferenceStatusByNameAsync(ConferenceStatusEnum.Cancelled.GetDescription());
            if (conf.IsResearchConference != true)
                throw new Exception("Thao tác chỉ dành cho hội nghị nghiên cứu");
            if (today > conf.EndDate)
            {
                throw new BadRequestException("Không thể gán do đã kết thúc hội nghị rồi");
            }

            if (conf.ConferenceStatusId == statusComplete.ConferenceStatusId || conf.ConferenceStatusId == statusCancelled.ConferenceStatusId)
                throw new BadRequestException("Không thể gán do hội nghị đã bị cancelled hoặc complete rồi");
            return;
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

            if (paper.TicketId == null)
            {
                return false;
            }

            var cameraReady = await _unitOfWork.CameraReadyRepository.GetCameraReadyByIdAsync(paper.CameraReadyId);
            if (cameraReady == null)
            {
                return false;
            }

            var acceptedStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription());

            return cameraReady != null;
        }

        #endregion

        public async Task<List<PaperDetailResponseDtoDetail>> GetAllAcceptedPaper(string confId)
        {
            var acceptedStatus = _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription()).Result;
            var list = await _unitOfWork.PaperRepository.GetAllAcceptedPaper(acceptedStatus, confId);
            List<PaperDetailResponseDtoDetail> response= new List<PaperDetailResponseDtoDetail>();
            foreach (var paper in list)
            {
                //get all authors
                var allAuthor = await _unitOfWork.PaperAuthorRepository.GetPaperAuthorsByPaperIdAsync(paper.PaperId);
                //get rootauthor
                var paperRootAuthor = allAuthor.FirstOrDefault(pa => pa.IsRootAuthor == true);
                var RootAuthor = await _unitOfWork.UserRepository.GetUserByUserId(paperRootAuthor.UserId);
                var coAuthorIds = allAuthor.Where(pa => pa.UserId != RootAuthor.UserId).Select(paper => paper.UserId).ToList();
                List<User> coAuthors = new List<User>();
                if (coAuthorIds.Count() > 0)
                {
                    foreach (var authorId in coAuthorIds)
                    {
                        User CoAuthor = await _unitOfWork.UserRepository.GetUserByUserId(authorId);
                        if (CoAuthor != null)
                        {
                            coAuthors.Add(CoAuthor);
                        }
                    }
                }

                var paperDetail = await _unitOfWork.PaperRepository.GetSubmittedPaperWith4PhaseStatusByConferenceId(confId);
                PaperDetailResponseDtoDetail paperDetailResponseDtoDetail = new PaperDetailResponseDtoDetail()
                {
                    PaperId = paperDetail.PaperId,
                    Title = paperDetail.Title,
                    Description = paperDetail.Description,
                    Abstract = paperDetail.Abstract != null ? new AbstractDtoDetail()
                    {
                        AbstractId = paperDetail.AbstractId,
                        Title = paperDetail.Abstract.Title,
                        Description = paperDetail.Abstract.Description,
                        FileUrl = paperDetail.Abstract.AbstractUrl,
                    } : null,
                    CameraReady = paperDetail.CameraReady != null ? new CameraReadyDtoDetail()
                    {
                        CameraReadyId = paperDetail.CameraReadyId,
                        Title = paperDetail.CameraReady.Title,
                        Description = paperDetail.CameraReady.Description,
                        FileUrl= paperDetail.CameraReady.CameraReadyUrl
                        
                    }: null,
                    RootAuthor = RootAuthor != null ? new Author { userId = RootAuthor.UserId, fullName = RootAuthor.FullName, avatarUrl = RootAuthor.AvatarUrl } : null,
                    CoAuthors = coAuthors?.Select(user => new Author
                    {
                        userId = user.UserId,
                        fullName = user.FullName,
                        avatarUrl = user.AvatarUrl
                    }).ToList(),
                    IsAssignedToSession = paperDetail.PresentAuthors.Any()
                };
                response.Add(paperDetailResponseDtoDetail);
            }
            return response;
        }

        //public async Task<List<PaperDetailResponseDtoDetail>> GetAllAcceptedPaperInSession(string sessionId)
        //{
        //    //check session and conf
        //    var session = await _unitOfWork.ConferenceSessionRepository.GetConferenceSessionByIdAsync(sessionId);
        //    if (session == null)
        //        throw new Exception("Không tìm thấy session ");

        //    //conf is of type research
        //    var conf = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(session.ConferenceId);
        //    await checkConference(conf);
        //    var list = await _unitOfWork.PaperRepository.GetAllAcceptedPaperFromResearchSession(sessionId);
        //    List<PaperDetailResponseDtoDetail> paperDetailResponseDTOs = list.Select(paper => new PaperDetailResponseDtoDetail
        //    {
        //        PaperId = paper.PaperId,
        //        Title = paper.Title,
        //        Description = paper.Description,
        //    }).ToList();
        //    return paperDetailResponseDTOs;
        //}
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

            //check time need to be before end date
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(session.ConferenceId);
            if (conference == null)
            {
                throw new BadRequestException("Không tìm thấy hội nghị nghiên cứu");
            }

            await checkConference(conference);

            //check if session has room
            if (session.RoomId == null)
                throw new Exception("Không thể gán do session chưa có phòng");


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
                    AssignedAt = await _timeProviderService.GetVietnamTime()
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
                    // Người dùng phải có vé và check-in vào session này trước khi được gán làm presenter.
                    throw new BadRequestException($"Không tìm thấy thông tin check-in cho tác giả {rootAuthor.User?.FullName} tại session {sessionId}.");
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
                    AssignedAt = await _timeProviderService.GetVietnamTime(),
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


        public async Task<bool> Unassign(string paperId, string sessionId)
        {
            // 1. Kiểm tra Paper và Session có tồn tại không
            var paper = await _unitOfWork.PaperRepository.GetPaperByIdAsync(paperId);
            if (paper == null) throw new NotFoundException($"Paper với ID {paperId} không tồn tại.");

            var session = await _unitOfWork.ConferenceSessionRepository.GetConferenceSessionByIdAsync(sessionId);
            if (session == null) throw new NotFoundException($"Session với ID {sessionId} không tồn tại.");

            // 2. Kiểm tra thời gian và trạng thái hội nghị 
            var conference = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(session.ConferenceId);
            if (conference == null) throw new NotFoundException("Không tìm thấy hội nghị.");
            await checkConference(conference);

            // 3. Kiểm tra xem Paper có đang được gán vào Session này không
            var presentAuthor = await _unitOfWork.PresentAuthorRepository.GetPresentAuthorByIdAsync(sessionId, paperId);
            if (presentAuthor == null)
            {
                throw new BadRequestException($"Paper ID {paperId} không được gán cho session {sessionId}.");
            }

            // 4. Tìm Presenter hiện tại của Paper này
            var paperAuthors = await _unitOfWork.PaperAuthorRepository.GetPaperAuthorsByPaperIdAsync(paperId);
            var presenterAuthor = paperAuthors.FirstOrDefault(pa => pa.IsPresenter == true);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 5. Cập nhật UserCheckIn: IsPresenter = false
                if (presenterAuthor != null)
                {
                    var userCheckIn = await _unitOfWork.UserCheckInRepository.GetUserCheckInByUserAndSessionAsync(presenterAuthor.UserId, sessionId);
                    if (userCheckIn != null)
                    {
                        userCheckIn.IsPresenter = false;
                        await _unitOfWork.UserCheckInRepository.UpdateUserCheckInAsync(userCheckIn);
                    }

                }

                // 6. Xóa bản ghi trong PresentAuthor
                await _unitOfWork.PresentAuthorRepository.DeletePresentAuthorAsync(presentAuthor);


                await _unitOfWork.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw ex;
            }
        }

        public async Task<List<PresenterSessionResponse>> GetAllPresenterResponse(string confId)
        {
            var presentAuthors = await _unitOfWork.PresentAuthorRepository.GetAllPresentAuthorsByConfIdAsync(confId);
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

            var conf = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(paper.ConferenceId);
            if (conf == null)
                throw new Exception("Hội nghị không tồn tại");

            await checkConference(conf);


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

                if (userTicket == null)
                    throw new BadRequestException($"Người dùng {newUser.FullName} không có vé của hội nghị:");

                if (userTicket.IsRefunded == true)
                {
                    throw new BadRequestException($"Người dùng với ID {newUser.FullName} phải có vé của hội nghị và chưa refund để được chỉ định làm người trình bày.");
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
                    TicketId = changeRequestTicket?.TicketId,
                    RequestedById = requesterId,
                    NewPresenterId = request.NewUserId,
                    Reason = request.Reason,
                    RequestAt = await _timeProviderService.GetVietnamTime(),
                    PaperId = request.PaperId,
                    GlobalStatusId = pendingStatus.GlobalStatusId,

                };


                await _unitOfWork.PresenterChangeRequestRepository.CreatePresenterChangeRequestAsync(changeRequest);



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

        public async Task<bool> ApprovePresenterChangeRequest(ApprovePresenterChangeRequest request, string approvedById)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var changeRequest = await _unitOfWork.PresenterChangeRequestRepository.GetPresenterChangeRequestByIdAsync(request.PresenterChangeRequestId);
                if (changeRequest == null)
                {
                    throw new BadRequestException($"Không tìm thấy yêu cầu thay đổi người trình bày với ID {request.PresenterChangeRequestId}.");
                }

                var acceptedStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription());
                var RejectedStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Rejected.GetDescription());
                string resultMessage;

                if (request.IsApproved)
                {



                    //approve the request
                    changeRequest.ReviewedAt = await _timeProviderService.GetVietnamTime();
                    changeRequest.GlobalStatusId = acceptedStatus.GlobalStatusId;
                    await _unitOfWork.PresenterChangeRequestRepository.UpdatePresenterChangeRequestAsync(changeRequest);



                    // get all author
                    var PaperAuthors = await _unitOfWork.PaperAuthorRepository.GetPaperAuthorsByPaperIdAsync(changeRequest.PaperId);
                    var currentPresenter = PaperAuthors.FirstOrDefault(pa => pa.IsPresenter == true);
                    var NewPresenter = PaperAuthors.FirstOrDefault(pa => pa.UserId == changeRequest.NewPresenterId);

                    //get present author and session
                    var presentAuthor = await _unitOfWork.PresentAuthorRepository.GetPresentAuthorByPaperIdAsync(changeRequest.PaperId);

                    if (currentPresenter == null) throw new Exception($"Không tìm thấy session đã gán cho paper với ID {changeRequest.PaperId}");

                    if (currentPresenter == null) throw new Exception($"Không tìm thấy presenter hiện tại của paper với thông tin sau: presenterId{changeRequest.RequestedById} paperId {changeRequest.PaperId}");
                    if (NewPresenter == null) throw new Exception($"Không tìm thấy usser với ID {changeRequest.NewPresenterId} để gán cho  paperId {changeRequest.PaperId}");
                    var currentUserCheckin = await _unitOfWork.UserCheckInRepository.GetUserCheckInByUserAndSessionAsync(currentPresenter.UserId, presentAuthor.ConferenceSessionId);

                    //update currentPresenter
                    currentPresenter.IsPresenter = false;
                    currentUserCheckin.IsPresenter = false;

                    await _unitOfWork.PaperAuthorRepository.UpdatePaperAuthorAsync(currentPresenter);
                    await _unitOfWork.UserCheckInRepository.UpdateUserCheckInAsync(currentUserCheckin);

                    //get newpresenter usercheckin

                    var newUserCheckin = await _unitOfWork.UserCheckInRepository.GetUserCheckInByUserAndSessionAsync(NewPresenter.UserId, presentAuthor.ConferenceSessionId);

                    if (newUserCheckin == null)
                    {
                        throw new BadRequestException($"Không tìm thấy thông tin check-in cho người trình bày mới (ID: {NewPresenter.UserId}) tại session này. Yêu cầu không thể được duyệt.");
                    }
                    NewPresenter.IsPresenter = true;
                    newUserCheckin.IsPresenter = true;

                    await _unitOfWork.PaperAuthorRepository.UpdatePaperAuthorAsync(NewPresenter);
                    await _unitOfWork.UserCheckInRepository.UpdateUserCheckInAsync(newUserCheckin);

                    await _unitOfWork.CommitAsync();
                    return true;



                    //if (currentPresent != null)
                    //{
                    //    // Find the session information from PresentAuthor table
                    //    var targetPresentAuthor = await _unitOfWork.PresentAuthorRepository.GetPresentAuthorByPaperIdAsync(currentPresent.PaperId);


                    //    if (targetPresentAuthor != null)
                    //    {
                    //        // Update UserCheckIn records to reflect the presenter change
                    //        // Find the old presenter who was presenting this paper
                    //        var paperAuthors = await _unitOfWork.PaperAuthorRepository.GetPaperAuthorsByPaperIdAsync(targetPresentAuthor.PaperId);
                    //        var oldPresenter = paperAuthors.FirstOrDefault(pa => pa.IsPresenter == true && pa.UserId != changeRequest.NewPresenterId);

                    //        if (oldPresenter != null)
                    //        {
                    //            // Update old presenter's UserCheckIn
                    //            var oldUserCheckIn = await _unitOfWork.UserCheckInRepository.GetUserCheckInByUserAndSessionAsync(oldPresenter.UserId, targetPresentAuthor.ConferenceSessionId);
                    //            if (oldUserCheckIn != null)
                    //            {
                    //                oldUserCheckIn.IsPresenter = false;
                    //                await _unitOfWork.UserCheckInRepository.UpdateUserCheckInAsync(oldUserCheckIn);
                    //            }
                    //        }

                    //        // Update new presenter's UserCheckIn
                    //        var newUserCheckIn = await _unitOfWork.UserCheckInRepository.GetUserCheckInByUserAndSessionAsync(changeRequest.NewPresenterId, targetPresentAuthor.ConferenceSessionId);
                    //        if (newUserCheckIn != null)
                    //        {
                    //            newUserCheckIn.IsPresenter = true;
                    //            await _unitOfWork.UserCheckInRepository.UpdateUserCheckInAsync(newUserCheckIn);
                    //        }
                    //    }
                    //}
                }
                else
                {
                    changeRequest.ReviewedAt = await _timeProviderService.GetVietnamTime();
                    changeRequest.GlobalStatusId = RejectedStatus.GlobalStatusId;
                    await _unitOfWork.PresenterChangeRequestRepository.UpdatePresenterChangeRequestAsync(changeRequest);
                    await _unitOfWork.CommitAsync();
                    return true;

                }


            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw ex;
            }
        }

        public async Task<List<ConfRadar.Services.DTOs.PresenterSession.PresenterChangeRequest>> GetPendingPresenterChangeRequests(string confId)
        {
            var pendingStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            if (pendingStatus == null)
            {
                return new List<ConfRadar.Services.DTOs.PresenterSession.PresenterChangeRequest>();
            }

            var conf = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(confId);
            if (conf == null)
            {
                throw new Exception("Hội nghị không tồn tại");
            }

            var allChangeRequests = await _unitOfWork.PresenterChangeRequestRepository.GetAllPresenterChangeRequestsByConfIdAndStatusIdAsync(pendingStatus.GlobalStatusId, confId);

            var responseList = new List<ConfRadar.Services.DTOs.PresenterSession.PresenterChangeRequest>();
            foreach (var request in allChangeRequests)
            {

                responseList.Add(new ConfRadar.Services.DTOs.PresenterSession.PresenterChangeRequest
                {
                    PresenterChangeRequestId = request.PresenterChangeRequestId,
                    TicketId = request.TicketId,
                    RequestedById = request.RequestedById,
                    RequestedByName = request.RequestedBy?.FullName,
                    NewPresenterId = request.NewPresenterId,
                    NewPresenterName = request.NewPresenter?.FullName,
                    GlobalStatusId = request.GlobalStatusId,
                    GlobalStatusName = request.GlobalStatus?.Name,
                    Reason = request.Reason,
                    RequestAt = request.RequestAt,
                    ReviewedAt = request.ReviewedAt,
                    ConferenceName = conf.ConferenceName,
                    ConferenceDescription = conf.Description,
                    PaparTile = request.Paper?.Title,
                    PaperDescription = request.Paper?.Description,
                    PaperId = request.Paper?.PaperId,
                    ConferenceId = conf.ConferenceId,
                    SessionId = request.Paper?.PresentAuthors.FirstOrDefault(pa => pa.PaperId == request.PaperId)?.ConferenceSession.ConferenceSessionId,
                    SessionTitle = request.Paper?.PresentAuthors.FirstOrDefault(pa => pa.PaperId == request.PaperId)?.ConferenceSession?.Title,
                    SessionDate = request.Paper?.PresentAuthors.FirstOrDefault(pa => pa.PaperId == request.PaperId)?.ConferenceSession?.SessionDate,

                });
            }

            return responseList;
        }

        public async Task<SessionChangeRequestResponse> CreateSessionChangeRequest(CreateSessionChangeRequest request, string requestedById)
        {
            // --- Các bước kiểm tra ban đầu của bạn đã rất tốt ---
            var paper = await _unitOfWork.PaperRepository.GetAllIncludeById(request.PaperId);
            if (paper == null) throw new BadRequestException($"Không tìm thấy paper với ID {request.PaperId}");


            var conf = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(paper.ConferenceId);
            if (conf == null)
                throw new Exception("Hội nghị không tồn tại");

            await checkConference(conf);


            var newSession = await _unitOfWork.ConferenceSessionRepository.GetSessionWithDetailsAsync(request.NewSessionId);
            if (newSession == null) throw new BadRequestException($"Session mới với ID {request.NewSessionId} không tồn tại");

            var allAuthors = await _unitOfWork.PaperAuthorRepository.GetPaperAuthorsByPaperIdAsync(request.PaperId);
            var currentPresenter = allAuthors.FirstOrDefault(a => a.IsPresenter == true);
            if (currentPresenter == null || requestedById != currentPresenter.UserId)
            {
                throw new BadRequestException($"Bạn không phải là người trình bày của paper với ID {request.PaperId} để yêu cầu đổi session.");
            }

            var currentPresentAuthor = await _unitOfWork.PresentAuthorRepository.GetPresentAuthorByPaperIdAsync(request.PaperId);
            if (currentPresentAuthor == null) throw new BadRequestException("Paper này chưa được gán vào session nào.");

            // Không thể yêu cầu đổi đến chính session hiện tại
            if (currentPresentAuthor.ConferenceSessionId == request.NewSessionId)
            {
                throw new BadRequestException("Bạn không thể yêu cầu đổi đến chính session hiện tại.");
            }

            var pendingStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());
            if (pendingStatus == null) throw new BadRequestException("Không tìm thấy trạng thái 'Pending'.");

            var existingPendingRequest = await _unitOfWork.SessionChangeRequestRepository.GetSessionChangeRequestByPaperIdAndSessionId(request.PaperId, request.NewSessionId);
            if (existingPendingRequest.Any(scr => scr.GlobalStatusId == pendingStatus.GlobalStatusId))
            {
                throw new BadRequestException("Đã có một yêu cầu đổi đến session này đang chờ xử lý.");
            }

            // Người dùng phải có check-in ở session mới thì mới được yêu cầu đổi
            UserCheckIn toNewUserCheckin = await _unitOfWork.UserCheckInRepository.GetUserCheckInByUserAndSessionAsync(requestedById, request.NewSessionId);
            if (toNewUserCheckin == null)
            {
                throw new BadRequestException($"Bạn phải có vé và thông tin check-in tại session mới (ID: {request.NewSessionId}) thì mới có thể yêu cầu đổi.");
            }

            // --- Logic tạo request không thay đổi trạng thái ---
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                string sessionChangeRequestId = Guid.NewGuid().ToString();

                // BỎ HOÀN TOÀN VIỆC CẬP NHẬT UserCheckIn Ở ĐÂY

                SessionChangeRequest sessionChangeRequest = new SessionChangeRequest
                {
                    SessionChangeRequestId = sessionChangeRequestId,
                    TicketId = toNewUserCheckin.TicketId, // Lấy ticket từ bản ghi check-in mới
                    NewConferenceSessionId = request.NewSessionId,
                    Reason = request.Reason,
                    RequestAt = await _timeProviderService.GetVietnamTime(),
                    CustomerId = requestedById,
                    GlobalStatusId = pendingStatus.GlobalStatusId,
                    PaperId = request.PaperId
                };
                await _unitOfWork.SessionChangeRequestRepository.CreateSessionChangeRequestAsync(sessionChangeRequest);

                await _unitOfWork.CommitAsync(); // Commit ngay sau khi tạo request

                return new SessionChangeRequestResponse
                {
                    CurrentSessionId = currentPresentAuthor.ConferenceSessionId, // Lấy session ID hiện tại
                    GlobalStatusId = pendingStatus.GlobalStatusId,
                    GlobalStatusName = pendingStatus.Name,
                    NewSessionId = request.NewSessionId,
                    PaperId = request.PaperId,
                    Reason = request.Reason,
                    RequestAt = sessionChangeRequest.RequestAt,
                    RequestedById = requestedById,
                    SessionChangeRequestId = sessionChangeRequestId
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw ex;
            }
        }
        public async Task<List<SessionChangeRequestResponse>> GetPendingSessionChangeRequests(string confId)
        {
            //response dto list
            List<SessionChangeRequestResponse> sessionChangeRequestResponses = new List<SessionChangeRequestResponse>();

            var conf = await _unitOfWork.ConferenceRepository.GetConferenceByIdAsync(confId);
            if (conf == null)
                throw new Exception("Không tìm thấy hội nghị");

            //get pending status
            var pendingStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Pending.GetDescription());

            //get pending request
            var PendingRequests = await _unitOfWork.SessionChangeRequestRepository.GetAllSessionChangeRequestsByStatusIdAndConfIdAsync(pendingStatus.GlobalStatusId, confId);


            foreach (var pendingRequest in PendingRequests)
            {
                var presentAuthor = await _unitOfWork.PresentAuthorRepository.GetPresentAuthorByPaperIdAsync(pendingRequest.PaperId);
                SessionChangeRequestResponse sessionChangeRequestResponse = new SessionChangeRequestResponse
                {
                    SessionChangeRequestId = pendingRequest.SessionChangeRequestId,

                    GlobalStatusId = pendingRequest.GlobalStatusId,
                    GlobalStatusName = pendingRequest.GlobalStatus.Name,

                    CurrentSession = presentAuthor.ConferenceSession.ToResearchSessionWithMediaResponse(),

                    NewSession = pendingRequest.NewConferenceSession.ToResearchSessionWithMediaResponse(),

                    ConferenceId = conf.ConferenceId,

                    ConferenceName = conf.ConferenceName,
                    ConferencDescription = conf.Description,

                    PaperId = presentAuthor.Paper?.PaperId,
                    PaparTile = presentAuthor.Paper?.Title,
                    PaperDescription = presentAuthor.Paper?.Description,

                    Reason = pendingRequest.Reason,
                    RequestAt = pendingRequest.RequestAt,
                    ReviewedAt = pendingRequest.ReviewedAt,
                    RequestedById = pendingRequest.CustomerId,
                    RequestedByName = pendingRequest.Customer?.FullName,
                };
                sessionChangeRequestResponses.Add(sessionChangeRequestResponse);
            }
            return sessionChangeRequestResponses;
        }

        public async Task<bool> ApproveSessionChangeRequest(ApproveSessionChangeRequest request, string approvedById)
        {
            var sessionChangeRequest = await _unitOfWork.SessionChangeRequestRepository.GetSessionChangeRequestByIdAsync(request.SessionChangeRequestId);
            if (sessionChangeRequest == null) throw new BadRequestException($"Không tìm thấy yêu cầu đổi session với ID {request.SessionChangeRequestId}.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (request.IsApproved)
                {
                    var acceptedStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Accepted.GetDescription());
                    if (acceptedStatus == null) throw new BadRequestException("Không tìm thấy trạng thái 'Accepted'.");

                    // 1. Lấy thông tin session cũ từ PresentAuthor
                    var presentAuthor = await _unitOfWork.PresentAuthorRepository.GetPresentAuthorByPaperIdAsync(sessionChangeRequest.PaperId);
                    if (presentAuthor == null) throw new BadRequestException($"Paper ID {sessionChangeRequest.PaperId} không còn được gán cho session nào.");

                    string oldSessionId = presentAuthor.ConferenceSessionId;
                    string newSessionId = sessionChangeRequest.NewConferenceSessionId;
                    string userId = sessionChangeRequest.CustomerId;

                    // 2. Lấy các bản ghi UserCheckIn liên quan
                    var oldUserCheckin = await _unitOfWork.UserCheckInRepository.GetUserCheckInByUserAndSessionAsync(userId, oldSessionId);
                    var newUserCheckin = await _unitOfWork.UserCheckInRepository.GetUserCheckInByUserAndSessionAsync(userId, newSessionId);

                    if (oldUserCheckin == null) throw new BadRequestException("Không tìm thấy thông tin check-in của presenter tại session cũ.");
                    if (newUserCheckin == null) throw new BadRequestException("Không tìm thấy thông tin check-in c    ủa presenter tại session mới.");

                    // 3. Cập nhật trạng thái IsPresenter
                    oldUserCheckin.IsPresenter = false;
                    newUserCheckin.IsPresenter = true;
                    await _unitOfWork.UserCheckInRepository.UpdateUserCheckInAsync(oldUserCheckin);
                    await _unitOfWork.UserCheckInRepository.UpdateUserCheckInAsync(newUserCheckin);

                    // 4. Cập nhật bản ghi PresentAuthor (Delete cái cũ, Create cái mới)
                    await _unitOfWork.PresentAuthorRepository.DeletePresentAuthorAsync(presentAuthor);
                    await _unitOfWork.PresentAuthorRepository.CreatePresentAuthorAsync(new PresentAuthor
                    {
                        ConferenceSessionId = newSessionId,
                        PaperId = sessionChangeRequest.PaperId,
                        AssignedAt = await _timeProviderService.GetVietnamTime()
                    });

                    // 5. Cập nhật trạng thái của yêu cầu
                    sessionChangeRequest.GlobalStatusId = acceptedStatus.GlobalStatusId;
                    sessionChangeRequest.ReviewedAt = await _timeProviderService.GetVietnamTime();
                    await _unitOfWork.SessionChangeRequestRepository.UpdateSessionChangeRequestAsync(sessionChangeRequest);

                    await _unitOfWork.CommitAsync();
                    return true;
                }
                else // Trường hợp từ chối
                {
                    var rejectStatus = await _unitOfWork.GlobalStatusRepository.GetGlobalStatusByName(GlobalStatusEnum.Rejected.GetDescription());
                    if (rejectStatus == null) throw new BadRequestException("Không tìm thấy trạng thái 'Rejected'.");

                    sessionChangeRequest.GlobalStatusId = rejectStatus.GlobalStatusId;
                    sessionChangeRequest.ReviewedAt = await _timeProviderService.GetVietnamTime();
                    // PHẢI GỌI UPDATE
                    await _unitOfWork.SessionChangeRequestRepository.UpdateSessionChangeRequestAsync(sessionChangeRequest);

                    await _unitOfWork.CommitAsync();
                    return false; // Trả về false khi từ chối
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw ex;
            }
        }




    }
}
