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
        Task<Ticket?> GetTicketByUserIdAndConferencePriceId(string userId, string conferencePriceId);
        Task<List<Ticket>> GetTicketListByConferenceId(string conferenceId);
        Task<int> CreateTicketAsync(Ticket ticket);
        Task<int> GetTicketCountByConferencePriceIdAsync(string conferencePriceId);
        Task<Ticket?> GetTicketByUserIdAndConferenceId(string userId, string conferenceId);
        Task<Ticket> GetTicketById(string ticketId);
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
    }
}
