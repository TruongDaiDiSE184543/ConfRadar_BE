using ConfRadar.Repositories.Base;
using ConfRadar.Repositories.Data;
using ConfRadar.Repositories.Models;
using ConfRadar.Shared.DTO.General;
using ConfRadar.Shared.DTO.Ticket;
using Microsoft.EntityFrameworkCore;

namespace ConfRadar.Repositories.Repositories
{
    public interface ITicketRepository
    {
        Task<PagedResultResponseDto<CustomerPaidTicketResponse>> GetTicketsByUserId(string userId, string? keyword, int pageNumber = 1, int pageSize = 10, DateTime? sessionStartTime = null, DateTime? sessionEndTime = null);
        Task<PagedResultResponseDto<CustomerPaidTicketResponse>> GetTicketsByUserIdAndConferenceId(string conferenceId, string userId, string? keyword, int pageNumber = 1, int pageSize = 10, DateTime? sessionStartTime = null, DateTime? sessionEndTime = null);
        Task<Ticket?> GetTicketByUserIdAndConferencePriceId(string userId, string conferencePriceId);
        Task<List<Ticket>> GetTicketListByConferenceId(string conferenceId);
        Task<int> CreateTicketAsync(Ticket ticket);
        Task<int> GetTicketCountByConferencePriceIdAsync(string conferencePriceId);
        Task<Ticket?> GetTicketByUserIdAndConferenceId(string userId, string conferenceId);

        Task<Ticket?> GetNotRefundAuthorTicketByUserIdAndConferenceId(string userId, string conferenceId);
        Task<List<Ticket>?> GetNotRefundedTicketsByConferenceIdAsync(string conferenceId);


        Task<List<Ticket>> GetAuthorTicketByUserIdAndConferenceId(string userId, string conferenceId);
        Task<List<Ticket>> GetAttendeeTicketByUserIdAndConferenceId(string userId, string conferenceId);


        Task<Ticket> GetTicketById(string ticketId);
        Task<Ticket?> GetTicketByTicketIdAndUserId(string ticketId, string userId);
        Task<int> UpdateTicketAsync(Ticket ticket);
        Task<List<Ticket>> GetPaidTicketsByConferenceIdAsync(string conferenceId);
        Task<List<Ticket>> GetTicketsWithDetailsByConferenceIdAsync(string conferenceId);

        Task<List<Ticket>> GetRefundedNonAuthorTicketsByConferenceIdAsync(string conferenceId);



        Task<List<Ticket>> GetNotRefundTechnicalTicketListByTicketIdsForCancel(List<string> ticketIds);
        Task<List<Ticket>> GetNotRefundResearchTicketListByTicketIdsForCancel(List<string> ticketIds);
        Task<int> UpdateTicketListAsync(List<Ticket> tickets);
        IQueryable<Ticket> GetIncludedQueryable();
    }
    public class TicketRepository : GenericRepository<Ticket>, ITicketRepository
    {
        public TicketRepository(ConfRadarDbContext context) : base(context)
        {
        }

        public async Task<PagedResultResponseDto<CustomerPaidTicketResponse>> GetTicketsByUserId(string userId, string? keyword, int pageNumber = 1, int pageSize = 10, DateTime? sessionStartTime = null, DateTime? sessionEndTime = null)
        {
            var query = _context.Tickets.AsNoTracking().Where(t => t.UserId == userId);
            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.ToLower();

                query = query.Where(t =>
                    t.UserCheckIns.Any(uci =>
                        (uci.ConferenceSession != null && (
                            (uci.ConferenceSession.Title != null && uci.ConferenceSession.Title.ToLower().Contains(keyword)) ||
                            (uci.ConferenceSession.Conference != null && uci.ConferenceSession.Conference.ConferenceName != null &&
                                uci.ConferenceSession.Conference.ConferenceName.ToLower().Contains(keyword)) ||
                            (uci.ConferenceSession.Room != null && (
                                (uci.ConferenceSession.Room.Number != null && uci.ConferenceSession.Room.Number.ToLower().Contains(keyword)) ||
                                (uci.ConferenceSession.Room.DisplayName != null && uci.ConferenceSession.Room.DisplayName.ToLower().Contains(keyword)) ||
                                (uci.ConferenceSession.Room.Destination != null && (
                                    (uci.ConferenceSession.Room.Destination.Name != null && uci.ConferenceSession.Room.Destination.Name.ToLower().Contains(keyword)) ||
                                    (uci.ConferenceSession.Room.Destination.District != null && uci.ConferenceSession.Room.Destination.District.ToLower().Contains(keyword)) ||
                                    (uci.ConferenceSession.Room.Destination.Street != null && uci.ConferenceSession.Room.Destination.Street.ToLower().Contains(keyword)) ||
                                    (uci.ConferenceSession.Room.Destination.City != null && uci.ConferenceSession.Room.Destination.City.CityName != null && uci.ConferenceSession.Room.Destination.City.CityName.ToLower().Contains(keyword))
                                ))
                            ))
                        ))
                    )
                    || t.Transactions.Any(tr =>
                        (tr.TransactionId != null && tr.TransactionId.ToLower().Contains(keyword)) ||
                        (tr.TransactionCode != null && tr.TransactionCode.ToLower().Contains(keyword)) ||
                        (tr.PaymentMethod != null && tr.PaymentMethod.MethodName != null &&
                            tr.PaymentMethod.MethodName.ToLower().Contains(keyword))
                    )
                );
            }


            if (sessionStartTime.HasValue)
            {
                query = query.Where(t =>
                    t.UserCheckIns.Any(uci => uci.ConferenceSession!.StartTime >= sessionStartTime.Value)
                );
            }
            if (sessionEndTime.HasValue)
            {
                query = query.Where(t =>
                    t.UserCheckIns.Any(uci => uci.ConferenceSession!.EndTime <= sessionEndTime.Value)
                );
            }


            var totalCount = await query.CountAsync();

            var listTicketDetail = await query
            .OrderByDescending(t => t.RegisteredDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
        .Select(t => new CustomerPaidTicketResponse
        {
            TicketId = t.TicketId,
            RegisteredDate = t.RegisteredDate,
            IsRefunded = t.IsRefunded,
            ActualPrice = t.ActualPrice,

            ConferenceId = t.PricePhase != null && t.PricePhase.ConferencePrice != null
    ? t.PricePhase.ConferencePrice.ConferenceId
    : null,

            ConferenceName = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null
    ? t.PricePhase.ConferencePrice.Conference.ConferenceName
    : null,

            ConferenceDescription = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null
    ? t.PricePhase.ConferencePrice.Conference.Description
    : null,

            ConferenceStartDate = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null
    ? t.PricePhase.ConferencePrice.Conference.StartDate
    : null,

            ConferenceEndDate = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null
    ? t.PricePhase.ConferencePrice.Conference.EndDate
    : null,

            ConferenceTotalSlot = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null
    ? t.PricePhase.ConferencePrice.Conference.TotalSlot
    : null,

            ConferenceAvailableSlot = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null
    ? t.PricePhase.ConferencePrice.Conference.AvailableSlot
    : null,

            ConferenceAddress = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null
    ? t.PricePhase.ConferencePrice.Conference.Address
    : null,

            BannerImageUrl = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null
    ? t.PricePhase.ConferencePrice.Conference.BannerImageUrl
    : null,

            ConferenceCreatedAt = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null
    ? t.PricePhase.ConferencePrice.Conference.CreatedAt
    : null,

            ConferenceTicketSaleStart = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null
    ? t.PricePhase.ConferencePrice.Conference.TicketSaleStart
    : null,

            ConferenceTicketSaleEnd = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null
    ? t.PricePhase.ConferencePrice.Conference.TicketSaleEnd
    : null,

            IsInternalHosted = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null
    ? t.PricePhase.ConferencePrice.Conference.IsInternalHosted
    : null,

            IsResearchConference = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null
    ? t.PricePhase.ConferencePrice.Conference.IsResearchConference
    : null,

            CityId = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null
    ? t.PricePhase.ConferencePrice.Conference.CityId
    : null,

            CityName = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null && t.PricePhase.ConferencePrice.Conference.City != null
    ? t.PricePhase.ConferencePrice.Conference.City.CityName
    : null,

            ConferenceCategoryId = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null
    ? t.PricePhase.ConferencePrice.Conference.ConferenceCategoryId
    : null,

            ConferenceCategoryName = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null && t.PricePhase.ConferencePrice.Conference.ConferenceCategory != null
    ? t.PricePhase.ConferencePrice.Conference.ConferenceCategory.ConferenceCategoryName
    : null,

            ConferenceStatusId = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null
    ? t.PricePhase.ConferencePrice.Conference.ConferenceStatusId
    : null,

            ConferenceStatusName = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null && t.PricePhase.ConferencePrice.Conference.ConferenceStatus != null
    ? t.PricePhase.ConferencePrice.Conference.ConferenceStatus.ConferenceStatusName
    : null,





            HasRefundPolicy = t.PricePhase != null && t.PricePhase.RefundPolicies.Any() ? true : false,
            TicketPricePhase = t.PricePhase != null ? new CustomerTicketPricePhaseDetailResponse()
            {
                PricePhaseId = t.PricePhaseId,
                PhaseName = t.PricePhase.PhaseName,
                StartDate = t.PricePhase.StartDate,
                EndDate = t.PricePhase.EndDate,
                ApplyPercent = t.PricePhase.ApplyPercent,
                TotalSlot = t.PricePhase.TotalSlot,
                AvailableSlot = t.PricePhase.AvailableSlot,
                ConferencePriceId = t.PricePhase.ConferencePriceId,
                RefundPolicies = t.PricePhase.RefundPolicies.Select(rp => new CustomerTicketRefundPoliciesDetailResponse()
                {
                    RefundPolicyId = rp.RefundPolicyId,
                    ConferenceId = rp.ConferenceId,
                    PricePhaseId = rp.PricePhaseId,
                    PercentRefund = rp.PercentRefund,
                    PricePhaseStartDate = t.PricePhase.StartDate,
                    RefundDeadline = rp.RefundDeadline,
                    RefundOrder = rp.RefundOrder,

                }).ToList(),
                ConferencePrice = new CustomerTicketConferencePriceDetailResponse()
                {
                    ConferencePriceId = t.PricePhase.ConferencePriceId,
                    TicketPrice = t.PricePhase != null && t.PricePhase.ConferencePrice != null ? t.PricePhase.ConferencePrice.TicketPrice : null,
                    TicketName = t.PricePhase != null && t.PricePhase.ConferencePrice != null ? t.PricePhase.ConferencePrice.TicketName : null,
                    TicketDescription = t.PricePhase != null && t.PricePhase.ConferencePrice != null ? t.PricePhase.ConferencePrice.TicketDescription : null,
                    TotalSlot = t.PricePhase != null && t.PricePhase.ConferencePrice != null ? t.PricePhase.ConferencePrice.TotalSlot : null,
                    AvailableSlot = t.PricePhase != null && t.PricePhase.ConferencePrice != null ? t.PricePhase.ConferencePrice.AvailableSlot : null,
                    ConferenceId = t.PricePhase != null && t.PricePhase.ConferencePrice != null ? t.PricePhase.ConferencePrice.ConferenceId : null,
                    IsAuthor = t.PricePhase != null && t.PricePhase.ConferencePrice != null ? t.PricePhase.ConferencePrice.IsAuthor : null,

                    PaperId = (t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.IsAuthor == true) ?
                    _context.Papers
                    .Where(p => p.PaperAuthors.Any(pa => pa.UserId == userId)
                    && t.PricePhase != null
                    && t.PricePhase.ConferencePrice != null
                    && p.ConferenceId == t.PricePhase.ConferencePrice.ConferenceId)
                    .Select(p => p.PaperId)
                    .FirstOrDefault() : null,

                    RegistrationStartDate = (t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.IsAuthor == true) ?
                    _context.Papers
                    .Where(p => p.PaperAuthors.Any(pa => pa.UserId == userId)
                    && p.ResearchConferencePhase != null && t.PricePhase != null
                    && t.PricePhase.ConferencePrice != null && p.ConferenceId == t.PricePhase.ConferencePrice.ConferenceId)
                    .Select(p => p.ResearchConferencePhase!.RegistrationStartDate)
                    .FirstOrDefault() : null,

                    RegistrationEndDate = (t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.IsAuthor == true) ?
                    _context.Papers
                    .Where(p => p.PaperAuthors.Any(pa => pa.UserId == userId)
                    && p.ResearchConferencePhase != null && t.PricePhase != null
                    && t.PricePhase.ConferencePrice != null && p.ConferenceId == t.PricePhase.ConferencePrice.ConferenceId)
                    .Select(p => p.ResearchConferencePhase!.RegistrationEndDate)
                    .FirstOrDefault() : null,




                }
            } : new CustomerTicketPricePhaseDetailResponse(),



            Transactions = t.Transactions.Select(transac => new CustomerTransactionDetailRespone
            {
                TransactionId = transac.TransactionId,
                Currency = transac.Currency,
                Amount = transac.Amount,
                CreatedAt = transac.CreatedAt,
                TransactionCode = transac.TransactionCode,
                IsRefunded = transac.IsRefunded,
                PaymentMethodId = transac.PaymentMethodId,
                PaymentMethodName = transac.PaymentMethod != null ? transac.PaymentMethod.MethodName : null,

            }).ToList(),

            UserCheckIns = t.UserCheckIns.Select(uci => new CustomerCheckInDetailResponse
            {
                UserCheckinId = uci.UserCheckinId,
                QrUrl = uci.QrUrl,
                IsPresenter = uci.IsPresenter,
                CheckinStatusId = uci.CheckinStatusId,
                CheckinStatusName = uci.CheckinStatus != null ? uci.CheckinStatus.CheckinStatusName : null,
                CheckInTime = uci.CheckInTime,
                ConferenceSessionId = uci.ConferenceSessionId,
                TicketId = uci.TicketId,
                ConferenceSessionDetail = uci.ConferenceSession != null ? new CustomerSessionDetailResponse
                {
                    ConferenceSessionId = uci.ConferenceSessionId,
                    Title = uci.ConferenceSession.Title,
                    Description = uci.ConferenceSession.Description,
                    StartTime = uci.ConferenceSession.StartTime,
                    EndTime = uci.ConferenceSession.EndTime,
                    SessionDate = uci.ConferenceSession.SessionDate,
                    ConferenceId = uci.ConferenceSession.ConferenceId,
                    ConferenceName = uci.ConferenceSession != null && uci.ConferenceSession.Conference != null ? uci.ConferenceSession.Conference.ConferenceName : null,
                    RoomId = uci.ConferenceSession != null ? uci.ConferenceSession.RoomId : null,
                    RoomNumber = uci.ConferenceSession != null && uci.ConferenceSession.Room != null ? uci.ConferenceSession.Room.Number : null,
                    RoomDisplayName = uci.ConferenceSession != null && uci.ConferenceSession.Room != null ? uci.ConferenceSession.Room.DisplayName : null,
                    DestinationId = uci.ConferenceSession != null && uci.ConferenceSession.Room != null ? uci.ConferenceSession.Room.DestinationId : null,
                    DestinationName = uci.ConferenceSession != null && uci.ConferenceSession.Room != null && uci.ConferenceSession.Room.Destination != null ? uci.ConferenceSession.Room.Destination.Name : null,
                    CityId = uci.ConferenceSession != null && uci.ConferenceSession.Room != null && uci.ConferenceSession.Room.Destination != null ? uci.ConferenceSession.Room.Destination.CityId : null,
                    CityName = uci.ConferenceSession != null && uci.ConferenceSession.Room != null && uci.ConferenceSession.Room.Destination != null && uci.ConferenceSession.Room.Destination.City != null ? uci.ConferenceSession.Room.Destination.City.CityName : null,
                    District = uci.ConferenceSession != null && uci.ConferenceSession.Room != null && uci.ConferenceSession.Room.Destination != null ? uci.ConferenceSession.Room.Destination.District : null,
                    Street = uci.ConferenceSession != null && uci.ConferenceSession.Room != null && uci.ConferenceSession.Room.Destination != null ? uci.ConferenceSession.Room.Destination.Street : null,
                } : new CustomerSessionDetailResponse()
            }).ToList()
        }).ToListAsync();

            return new PagedResultResponseDto<CustomerPaidTicketResponse>()
            {
                Items = listTicketDetail,
                Page = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
            };
        }
        public async Task<PagedResultResponseDto<CustomerPaidTicketResponse>> GetTicketsByUserIdAndConferenceId(string conferenceId, string userId, string? keyword, int pageNumber = 1, int pageSize = 10, DateTime? sessionStartTime = null, DateTime? sessionEndTime = null)
        {
            var query = _context.Tickets.AsNoTracking()
                .Where(t => t.UserId == userId
                && t.PricePhase != null && t.PricePhase.ConferencePrice != null
                && t.PricePhase.ConferencePrice.ConferenceId == conferenceId);


            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.ToLower();

                query = query.Where(t =>
                    t.UserCheckIns.Any(uci =>
                        (uci.ConferenceSession != null && (
                            (uci.ConferenceSession.Title != null && uci.ConferenceSession.Title.ToLower().Contains(keyword)) ||
                            (uci.ConferenceSession.Conference != null && uci.ConferenceSession.Conference.ConferenceName != null &&
                                uci.ConferenceSession.Conference.ConferenceName.ToLower().Contains(keyword)) ||
                            (uci.ConferenceSession.Room != null && (
                                (uci.ConferenceSession.Room.Number != null && uci.ConferenceSession.Room.Number.ToLower().Contains(keyword)) ||
                                (uci.ConferenceSession.Room.DisplayName != null && uci.ConferenceSession.Room.DisplayName.ToLower().Contains(keyword)) ||
                                (uci.ConferenceSession.Room.Destination != null && (
                                    (uci.ConferenceSession.Room.Destination.Name != null && uci.ConferenceSession.Room.Destination.Name.ToLower().Contains(keyword)) ||
                                    (uci.ConferenceSession.Room.Destination.District != null && uci.ConferenceSession.Room.Destination.District.ToLower().Contains(keyword)) ||
                                    (uci.ConferenceSession.Room.Destination.Street != null && uci.ConferenceSession.Room.Destination.Street.ToLower().Contains(keyword)) ||
                                    (uci.ConferenceSession.Room.Destination.City != null && uci.ConferenceSession.Room.Destination.City.CityName != null && uci.ConferenceSession.Room.Destination.City.CityName.ToLower().Contains(keyword))
                                ))
                            ))
                        ))
                    )
                    || t.Transactions.Any(tr =>
                        (tr.TransactionId != null && tr.TransactionId.ToLower().Contains(keyword)) ||
                        (tr.TransactionCode != null && tr.TransactionCode.ToLower().Contains(keyword)) ||
                        (tr.PaymentMethod != null && tr.PaymentMethod.MethodName != null &&
                            tr.PaymentMethod.MethodName.ToLower().Contains(keyword))
                    )
                );
            }


            if (sessionStartTime.HasValue)
            {
                query = query.Where(t =>
                    t.UserCheckIns.Any(uci => uci.ConferenceSession!.StartTime >= sessionStartTime.Value)
                );
            }
            if (sessionEndTime.HasValue)
            {
                query = query.Where(t =>
                    t.UserCheckIns.Any(uci => uci.ConferenceSession!.EndTime <= sessionEndTime.Value)
                );
            }


            var totalCount = await query.CountAsync();

            var listTicketDetail = await query
            .OrderByDescending(t => t.RegisteredDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
        .Select(t => new CustomerPaidTicketResponse
        {
            TicketId = t.TicketId,
            RegisteredDate = t.RegisteredDate,
            IsRefunded = t.IsRefunded,
            ActualPrice = t.ActualPrice,

            ConferenceId = t.PricePhase != null && t.PricePhase.ConferencePrice != null
    ? t.PricePhase.ConferencePrice.ConferenceId
    : null,

            ConferenceName = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null
    ? t.PricePhase.ConferencePrice.Conference.ConferenceName
    : null,

            ConferenceDescription = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null
    ? t.PricePhase.ConferencePrice.Conference.Description
    : null,

            ConferenceStartDate = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null
    ? t.PricePhase.ConferencePrice.Conference.StartDate
    : null,

            ConferenceEndDate = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null
    ? t.PricePhase.ConferencePrice.Conference.EndDate
    : null,

            ConferenceTotalSlot = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null
    ? t.PricePhase.ConferencePrice.Conference.TotalSlot
    : null,

            ConferenceAvailableSlot = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null
    ? t.PricePhase.ConferencePrice.Conference.AvailableSlot
    : null,

            ConferenceAddress = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null
    ? t.PricePhase.ConferencePrice.Conference.Address
    : null,

            BannerImageUrl = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null
    ? t.PricePhase.ConferencePrice.Conference.BannerImageUrl
    : null,

            ConferenceCreatedAt = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null
    ? t.PricePhase.ConferencePrice.Conference.CreatedAt
    : null,

            ConferenceTicketSaleStart = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null
    ? t.PricePhase.ConferencePrice.Conference.TicketSaleStart
    : null,

            ConferenceTicketSaleEnd = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null
    ? t.PricePhase.ConferencePrice.Conference.TicketSaleEnd
    : null,

            IsInternalHosted = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null
    ? t.PricePhase.ConferencePrice.Conference.IsInternalHosted
    : null,

            IsResearchConference = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null
    ? t.PricePhase.ConferencePrice.Conference.IsResearchConference
    : null,

            CityId = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null
    ? t.PricePhase.ConferencePrice.Conference.CityId
    : null,

            CityName = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null && t.PricePhase.ConferencePrice.Conference.City != null
    ? t.PricePhase.ConferencePrice.Conference.City.CityName
    : null,

            ConferenceCategoryId = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null
    ? t.PricePhase.ConferencePrice.Conference.ConferenceCategoryId
    : null,

            ConferenceCategoryName = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null && t.PricePhase.ConferencePrice.Conference.ConferenceCategory != null
    ? t.PricePhase.ConferencePrice.Conference.ConferenceCategory.ConferenceCategoryName
    : null,

            ConferenceStatusId = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null
    ? t.PricePhase.ConferencePrice.Conference.ConferenceStatusId
    : null,

            ConferenceStatusName = t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.Conference != null && t.PricePhase.ConferencePrice.Conference.ConferenceStatus != null
    ? t.PricePhase.ConferencePrice.Conference.ConferenceStatus.ConferenceStatusName
    : null,





            HasRefundPolicy = t.PricePhase != null && t.PricePhase.RefundPolicies.Any() ? true : false,
            TicketPricePhase = t.PricePhase != null ? new CustomerTicketPricePhaseDetailResponse()
            {
                PricePhaseId = t.PricePhaseId,
                PhaseName = t.PricePhase.PhaseName,
                StartDate = t.PricePhase.StartDate,
                EndDate = t.PricePhase.EndDate,
                ApplyPercent = t.PricePhase.ApplyPercent,
                TotalSlot = t.PricePhase.TotalSlot,
                AvailableSlot = t.PricePhase.AvailableSlot,
                ConferencePriceId = t.PricePhase.ConferencePriceId,
                RefundPolicies = t.PricePhase.RefundPolicies.Select(rp => new CustomerTicketRefundPoliciesDetailResponse()
                {
                    RefundPolicyId = rp.RefundPolicyId,
                    ConferenceId = rp.ConferenceId,
                    PricePhaseId = rp.PricePhaseId,
                    PercentRefund = rp.PercentRefund,
                    PricePhaseStartDate = t.PricePhase.StartDate,
                    RefundDeadline = rp.RefundDeadline,
                    RefundOrder = rp.RefundOrder,

                }).ToList(),
                ConferencePrice = new CustomerTicketConferencePriceDetailResponse()
                {
                    ConferencePriceId = t.PricePhase.ConferencePriceId,
                    TicketPrice = t.PricePhase != null && t.PricePhase.ConferencePrice != null ? t.PricePhase.ConferencePrice.TicketPrice : null,
                    TicketName = t.PricePhase != null && t.PricePhase.ConferencePrice != null ? t.PricePhase.ConferencePrice.TicketName : null,
                    TicketDescription = t.PricePhase != null && t.PricePhase.ConferencePrice != null ? t.PricePhase.ConferencePrice.TicketDescription : null,
                    TotalSlot = t.PricePhase != null && t.PricePhase.ConferencePrice != null ? t.PricePhase.ConferencePrice.TotalSlot : null,
                    AvailableSlot = t.PricePhase != null && t.PricePhase.ConferencePrice != null ? t.PricePhase.ConferencePrice.AvailableSlot : null,
                    ConferenceId = t.PricePhase != null && t.PricePhase.ConferencePrice != null ? t.PricePhase.ConferencePrice.ConferenceId : null,
                    IsAuthor = t.PricePhase != null && t.PricePhase.ConferencePrice != null ? t.PricePhase.ConferencePrice.IsAuthor : null,

                    PaperId = (t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.IsAuthor == true) ?
                    _context.Papers
                    .Where(p => p.PaperAuthors.Any(pa => pa.UserId == userId)
                    && t.PricePhase != null
                    && t.PricePhase.ConferencePrice != null
                    && p.ConferenceId == t.PricePhase.ConferencePrice.ConferenceId)
                    .Select(p => p.PaperId)
                    .FirstOrDefault() : null,

                    RegistrationStartDate = (t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.IsAuthor == true) ?
                    _context.Papers
                    .Where(p => p.PaperAuthors.Any(pa => pa.UserId == userId)
                    && p.ResearchConferencePhase != null && t.PricePhase != null
                    && t.PricePhase.ConferencePrice != null && p.ConferenceId == t.PricePhase.ConferencePrice.ConferenceId)
                    .Select(p => p.ResearchConferencePhase!.RegistrationStartDate)
                    .FirstOrDefault() : null,

                    RegistrationEndDate = (t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.IsAuthor == true) ?
                    _context.Papers
                    .Where(p => p.PaperAuthors.Any(pa => pa.UserId == userId)
                    && p.ResearchConferencePhase != null && t.PricePhase != null
                    && t.PricePhase.ConferencePrice != null && p.ConferenceId == t.PricePhase.ConferencePrice.ConferenceId)
                    .Select(p => p.ResearchConferencePhase!.RegistrationEndDate)
                    .FirstOrDefault() : null,




                }
            } : new CustomerTicketPricePhaseDetailResponse(),



            Transactions = t.Transactions.Select(transac => new CustomerTransactionDetailRespone
            {
                TransactionId = transac.TransactionId,
                Currency = transac.Currency,
                Amount = transac.Amount,
                CreatedAt = transac.CreatedAt,
                TransactionCode = transac.TransactionCode,
                IsRefunded = transac.IsRefunded,
                PaymentMethodId = transac.PaymentMethodId,
                PaymentMethodName = transac.PaymentMethod != null ? transac.PaymentMethod.MethodName : null,

            }).ToList(),

            UserCheckIns = t.UserCheckIns.Select(uci => new CustomerCheckInDetailResponse
            {
                UserCheckinId = uci.UserCheckinId,
                QrUrl = uci.QrUrl,
                IsPresenter = uci.IsPresenter,
                CheckinStatusId = uci.CheckinStatusId,
                CheckinStatusName = uci.CheckinStatus != null ? uci.CheckinStatus.CheckinStatusName : null,
                CheckInTime = uci.CheckInTime,
                ConferenceSessionId = uci.ConferenceSessionId,
                TicketId = uci.TicketId,
                ConferenceSessionDetail = uci.ConferenceSession != null ? new CustomerSessionDetailResponse
                {
                    ConferenceSessionId = uci.ConferenceSessionId,
                    Title = uci.ConferenceSession.Title,
                    Description = uci.ConferenceSession.Description,
                    StartTime = uci.ConferenceSession.StartTime,
                    EndTime = uci.ConferenceSession.EndTime,
                    SessionDate = uci.ConferenceSession.SessionDate,
                    ConferenceId = uci.ConferenceSession.ConferenceId,
                    ConferenceName = uci.ConferenceSession != null && uci.ConferenceSession.Conference != null ? uci.ConferenceSession.Conference.ConferenceName : null,
                    RoomId = uci.ConferenceSession != null ? uci.ConferenceSession.RoomId : null,
                    RoomNumber = uci.ConferenceSession != null && uci.ConferenceSession.Room != null ? uci.ConferenceSession.Room.Number : null,
                    RoomDisplayName = uci.ConferenceSession != null && uci.ConferenceSession.Room != null ? uci.ConferenceSession.Room.DisplayName : null,
                    DestinationId = uci.ConferenceSession != null && uci.ConferenceSession.Room != null ? uci.ConferenceSession.Room.DestinationId : null,
                    DestinationName = uci.ConferenceSession != null && uci.ConferenceSession.Room != null && uci.ConferenceSession.Room.Destination != null ? uci.ConferenceSession.Room.Destination.Name : null,
                    CityId = uci.ConferenceSession != null && uci.ConferenceSession.Room != null && uci.ConferenceSession.Room.Destination != null ? uci.ConferenceSession.Room.Destination.CityId : null,
                    CityName = uci.ConferenceSession != null && uci.ConferenceSession.Room != null && uci.ConferenceSession.Room.Destination != null && uci.ConferenceSession.Room.Destination.City != null ? uci.ConferenceSession.Room.Destination.City.CityName : null,
                    District = uci.ConferenceSession != null && uci.ConferenceSession.Room != null && uci.ConferenceSession.Room.Destination != null ? uci.ConferenceSession.Room.Destination.District : null,
                    Street = uci.ConferenceSession != null && uci.ConferenceSession.Room != null && uci.ConferenceSession.Room.Destination != null ? uci.ConferenceSession.Room.Destination.Street : null,
                } : new CustomerSessionDetailResponse()
            }).ToList()
        }).ToListAsync();

            return new PagedResultResponseDto<CustomerPaidTicketResponse>()
            {
                Items = listTicketDetail,
                Page = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
            };
        }























        public async Task<Ticket?> GetTicketByUserIdAndConferencePriceId(string userId, string conferencePriceId)
        {
            return await _context.Tickets.FirstOrDefaultAsync(x => x.UserId == userId && x.PricePhase != null && x.PricePhase.ConferencePriceId == conferencePriceId);
        }
        public async Task<List<Ticket>> GetTicketListByConferenceId(string conferenceId)
        {
            var query = from t in _context.Tickets
                        join pp in _context.PricePhases on t.PricePhaseId equals pp.PricePhaseId
                        join cp in _context.ConferencePrices on pp.ConferencePriceId equals cp.ConferencePriceId
                        join c in _context.Conferences on cp.ConferenceId equals c.ConferenceId
                        join u in _context.Users on t.UserId equals u.UserId
                        where /*t.IsRefunded == false */  c.ConferenceId == conferenceId
                        select new Ticket()
                        {
                            TicketId = t.TicketId,
                            UserId = u.UserId,
                            RegisteredDate = t.RegisteredDate,
                            IsRefunded = t.IsRefunded,
                            User = new User()
                            {
                                UserId = u.UserId,
                                FullName = u.FullName,
                                Email = u.Email,
                                AvatarUrl = u.AvatarUrl,
                            },
                            PricePhase = new PricePhase
                            {
                                PricePhaseId = pp.PricePhaseId,
                                ApplyPercent = pp.ApplyPercent,
                                AvailableSlot = pp.AvailableSlot,
                                TotalSlot = pp.TotalSlot,
                                StartDate = pp.StartDate,
                                EndDate = pp.EndDate,
                                ConferencePrice = new ConferencePrice
                                {
                                    ConferencePriceId = cp.ConferencePriceId,
                                    ConferenceId = cp.ConferenceId,
                                    Conference = new Conference
                                    {
                                        ConferenceId = c.ConferenceId,
                                        ConferenceName = c.ConferenceName
                                    }
                                },
                            },

                        };
            return await query.ToListAsync();
        }

        public async Task<int> GetTicketCountByConferencePriceIdAsync(string conferencePriceId)
        {
            return await _context.Tickets
                .Where(t => t.PricePhase.ConferencePriceId == conferencePriceId && !t.IsRefunded.Value)
                .CountAsync();
        }

        public async Task<int> CreateTicketAsync(Ticket ticket)
        {
            return await CreateAsync(ticket);
        }

        public async Task<Ticket?> GetTicketByUserIdAndConferenceId(string userId, string conferenceId)
        {
            return await _context.Tickets
                .Include(t => t.PricePhase)
                    .ThenInclude(t => t.ConferencePrice)
                .Where(t => t.UserId == userId && t.PricePhase != null && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.ConferenceId == conferenceId)
                .FirstOrDefaultAsync();
        }

        public async Task<Ticket> GetTicketById(string ticketId)
        {
            return await _context.Tickets.FirstOrDefaultAsync(t => t.TicketId == ticketId);
        }

        public async Task<Ticket?> GetTicketByTicketIdAndUserId(string ticketId, string userId)
        {
            return await _context.Tickets
                .Include(t => t.Transactions)

                .Include(t => t.PricePhase)
                    .ThenInclude(pp => pp.RefundPolicies)
                .Include(t => t.PricePhase)
                    .ThenInclude(pp => pp.ConferencePrice)
                        .ThenInclude(cp => cp.Conference)
                            .ThenInclude(c => c.ResearchConferenceDetail)
                .AsSplitQuery()
                .FirstOrDefaultAsync(t => t.TicketId == ticketId && t.UserId == userId);
        }

        public async Task<int> UpdateTicketAsync(Ticket ticket)
        {
            return await UpdateAsync(ticket);
        }
        public async Task<int> UpdateTicketListAsync(List<Ticket> tickets)
        {
            _context.UpdateRange(tickets);
            return await _context.SaveChangesAsync();
        }

        public async Task<List<Ticket>> GetAuthorTicketByUserIdAndConferenceId(string userId, string conferenceId)
        {
            return await _context.Tickets
               .Include(t => t.PricePhase)
                   .ThenInclude(t => t.ConferencePrice)
               .Where(t => t.UserId == userId
               && t.PricePhase != null
               && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.IsAuthor == true
               && t.PricePhase.ConferencePrice.ConferenceId == conferenceId).ToListAsync();
        }

        public async Task<List<Ticket>> GetAttendeeTicketByUserIdAndConferenceId(string userId, string conferenceId)
        {
            return await _context.Tickets
               .Include(t => t.PricePhase)
                   .ThenInclude(t => t.ConferencePrice)
               .Where(t => t.UserId == userId
               && t.PricePhase != null
               && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.IsAuthor == false
               && t.PricePhase.ConferencePrice.ConferenceId == conferenceId).ToListAsync();
        }

        public async Task<Ticket?> GetNotRefundAuthorTicketByUserIdAndConferenceId(string userId, string conferenceId)
        {
            return await _context.Tickets
             .Include(t => t.PricePhase)
                 .ThenInclude(t => t.ConferencePrice)
             .FirstOrDefaultAsync(t => t.UserId == userId
             && t.PricePhase != null
             && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.IsAuthor == true
             && t.IsRefunded == false
             && t.PricePhase.ConferencePrice.ConferenceId == conferenceId);
        }

        public async Task<List<Ticket>> GetPaidTicketsByConferenceIdAsync(string conferenceId)
        {
            return await _context.Tickets
       .AsNoTracking() // Thêm AsNoTracking vì đây là query chỉ đọc, giúp tăng hiệu năng
       .Include(t => t.PricePhase) // Tải kèm bảng PricePhase
           .ThenInclude(pp => pp.ConferencePrice) // Từ PricePhase, tải tiếp bảng ConferencePrice
       .Where(t =>
           t.IsRefunded == false &&
           t.PricePhase != null && // Thêm kiểm tra null để an toàn
           t.PricePhase.ConferencePrice != null && // Thêm kiểm tra null để an toàn
           t.PricePhase.ConferencePrice.ConferenceId == conferenceId)
       .ToListAsync();
        }

        public async Task<List<Ticket>> GetTicketsWithDetailsByConferenceIdAsync(string conferenceId)
        {
            return await _context.Tickets
                .Include(t => t.User)
                .Include(t => t.PricePhase)
                .ThenInclude(pp => pp.ConferencePrice)
                .ThenInclude(cp => cp.Conference)
                .Where(t => t.PricePhase != null &&
                           t.PricePhase.ConferencePrice != null &&
                           t.PricePhase.ConferencePrice.ConferenceId == conferenceId)
                .ToListAsync();
        }




        public async Task<List<Ticket>> GetNotRefundTechnicalTicketListByTicketIdsForCancel(List<string> ticketIds)
        {
            return await _context.Tickets
                .Include(t => t.Transactions)


                .Include(t => t.User)
                    .ThenInclude(t => t.Wallet)


                .Include(t => t.PricePhase)
                   .ThenInclude(pp => pp.ConferencePrice)
                   .ThenInclude(cp => cp.Conference)
                    .ThenInclude(c => c.TechnicalConferenceDetail)
               .Where(t =>
               t.PricePhase != null
               && t.PricePhase.ConferencePrice != null && t.PricePhase.ConferencePrice.IsAuthor == false
               && t.PricePhase.ConferencePrice.Conference != null
               && t.PricePhase.ConferencePrice.Conference.TechnicalConferenceDetail != null
               && t.IsRefunded == false
               && ticketIds.Contains(t.TicketId))
               .AsSplitQuery()
               .ToListAsync();
        }
        public async Task<List<Ticket>> GetNotRefundResearchTicketListByTicketIdsForCancel(List<string> ticketIds)
        {
            return await _context.Tickets
                .Include(t => t.Transactions)


                .Include(t => t.User)
                    .ThenInclude(t => t.Wallet)


                .Include(t => t.PricePhase)
                   .ThenInclude(pp => pp.ConferencePrice)
                   .ThenInclude(cp => cp.Conference)
                    .ThenInclude(c => c.ResearchConferenceDetail)

                .Include(t => t.Paper)
                    .ThenInclude(p => p.PaperPhase)

                 .Include(t => t.Paper)
                    .ThenInclude(p => p.Abstract)

                .Include(t => t.Paper)
                    .ThenInclude(p => p.FullPaper)

                .Include(t => t.Paper)
                    .ThenInclude(p => p.RevisionPaper)

                .Include(t => t.Paper)
                    .ThenInclude(p => p.CameraReady)
               .Where(t =>
               t.PricePhase != null
               && t.PricePhase.ConferencePrice != null
               && t.PricePhase.ConferencePrice.Conference != null
               && t.PricePhase.ConferencePrice.Conference.ResearchConferenceDetail != null
               && t.IsRefunded == false
               && ticketIds.Contains(t.TicketId))
               .AsSplitQuery()
               .ToListAsync();
        }


        public async Task<List<Ticket>?> GetNotRefundedTicketsByConferenceIdAsync(string conferenceId)
        {
            return await _context.Tickets.AsNoTracking()
             .Include(t => t.PricePhase)
                 .ThenInclude(pp => pp.ConferencePrice)
             .Where(t => t.PricePhase != null && t.PricePhase.ConferencePrice != null &&
             t.IsRefunded == false &&
             t.PricePhase.ConferencePrice.ConferenceId == conferenceId).ToListAsync();
        }

        public async Task<List<Ticket>> GetRefundedNonAuthorTicketsByConferenceIdAsync(string conferenceId)
        {
            return await _context.Tickets.AsNoTracking()
               .Include(t => t.PricePhase)
                   .ThenInclude(pp => pp.ConferencePrice)
               .Where(t => t.PricePhase != null && t.PricePhase.ConferencePrice != null &&
               t.PricePhase.ConferencePrice.IsAuthor == false &&
               t.IsRefunded == true &&
               t.PricePhase.ConferencePrice.ConferenceId == conferenceId).ToListAsync();

        }

        public IQueryable<Ticket> GetIncludedQueryable()
        {
            return _context.Tickets.AsNoTracking().Include(t => t.PricePhase).ThenInclude(pp => pp.ConferencePrice).ThenInclude(cp => cp.Conference).AsQueryable();
        }

        //public async Task<List<Ticket>> GetRefundedNonAuthorTicketsByConferenceIdAsync(string conferenceId)
        //{
        //    return await _context.Tickets.AsNoTracking()
        //      .Include(t => t.PricePhase)
        //          .ThenInclude(pp => pp.ConferencePrice)
        //      .Where(t => t.PricePhase != null && t.PricePhase.ConferencePrice != null &&
        //      t.PricePhase.ConferencePrice.IsAuthor == false &&
        //      t.IsRefunded == true &&
        //      t.PricePhase.ConferencePrice.ConferenceId == conferenceId).ToListAsync();
        //}

    }
}
