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
        Task<PagedResultResponseDto<CustomerPaidTicketResponse>> GetTicketsByUserId(string userId, string? keyword, int pageNumber = 1, int pageSize = 10);
        Task<Ticket?> GetTicketByUserIdAndConferencePriceId(string userId, string conferencePriceId);
        Task<List<Ticket>> GetTicketListByConferenceId(string conferenceId);
        Task<int> CreateTicketAsync(Ticket ticket);
        Task<int> GetTicketCountByConferencePriceIdAsync(string conferencePriceId);
        Task<Ticket?> GetTicketByUserIdAndConferenceId(string userId, string conferenceId);
    }
    public class TicketRepository : GenericRepository<Ticket>, ITicketRepository
    {
        public TicketRepository(ConfRadarDbContext context) : base(context)
        {
        }

        public async Task<PagedResultResponseDto<CustomerPaidTicketResponse>> GetTicketsByUserId(string userId, string? keyword, int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.Tickets.AsNoTracking().Where(t => t.UserId == userId);
            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.ToLower();

                query = query.Where(t =>
                    t.UserCheckIns.Any(uci =>
                        uci.ConferenceSession.Title.ToLower().Contains(keyword) ||
                        uci.ConferenceSession.Conference.ConferenceName.ToLower().Contains(keyword) ||
                        uci.ConferenceSession.Room.Number.ToLower().Contains(keyword) ||
                        uci.ConferenceSession.Room.DisplayName.ToLower().Contains(keyword) ||
                        uci.ConferenceSession.Room.Destination.Name.ToLower().Contains(keyword) ||
                        uci.ConferenceSession.Room.Destination.District.ToLower().Contains(keyword) ||
                        uci.ConferenceSession.Room.Destination.Street.ToLower().Contains(keyword) ||
                        // Search theo City
                        uci.ConferenceSession.Room.Destination.City.CityName.ToLower().Contains(keyword)
                    )||t.Transactions.Any(tr =>
                        tr.TransactionCode.ToLower().Contains(keyword) ||
                        tr.PaymentMethod.MethodName.ToLower().Contains(keyword)
                    )
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
                PaymentMethodName = transac.PaymentMethod.MethodName,

            }).ToList(),

            UserCheckIns = t.UserCheckIns.Select(uci => new CustomerCheckInDetailResponse
            {
                UserCheckinId = uci.UserCheckinId,
                IsPresenter = uci.IsPresenter,
                CheckinStatusId = uci.CheckinStatusId,
                CheckinStatusName = uci.CheckinStatus.CheckinStatusName,
                CheckInTime = uci.CheckInTime,
                ConferenceSessionId = uci.ConferenceSessionId,
                TicketId = uci.TicketId,
                ConferenceSessionDetail = new CustomerSessionDetailResponse
                {
                    ConferenceSessionId = uci.ConferenceSessionId,
                    Title = uci.ConferenceSession.Title,
                    Description = uci.ConferenceSession.Description,
                    StartTime = uci.ConferenceSession.StartTime,
                    EndTime = uci.ConferenceSession.EndTime,
                    SessionDate = uci.ConferenceSession.SessionDate,
                    ConferenceId = uci.ConferenceSession.ConferenceId,
                    ConferenceName = uci.ConferenceSession.Conference.ConferenceName,
                    RoomId = uci.ConferenceSession.RoomId,
                    RoomNumber = uci.ConferenceSession.Room.Number,
                    RoomDisplayName = uci.ConferenceSession.Room.DisplayName,
                    DestinationId = uci.ConferenceSession.Room.DestinationId,
                    DestinationName = uci.ConferenceSession.Room.Destination.Name,
                    CityId = uci.ConferenceSession.Room.Destination.CityId,
                    CityName = uci.ConferenceSession.Room.Destination.City.CityName,
                    District = uci.ConferenceSession.Room.Destination.District,
                    Street = uci.ConferenceSession.Room.Destination.Street,
                }
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
            return await _context.Tickets.FirstOrDefaultAsync(x => x.UserId == userId && x.ConferencePriceId == conferencePriceId);
        }
        public async Task<List<Ticket>> GetTicketListByConferenceId(string conferenceId)
        {
            var query = from t in _context.Tickets
                        join cp in _context.ConferencePrices on t.ConferencePriceId equals cp.ConferencePriceId
                        join c in _context.Conferences on cp.ConferenceId equals c.ConferenceId
                        join u in _context.Users on t.UserId equals u.UserId
                        where t.IsRefunded == false && c.ConferenceId == conferenceId
                        select new Ticket()
                        {
                            TicketId = t.TicketId,
                            UserId = u.UserId,
                            User = new User()
                            {
                                UserId = u.UserId,
                                FullName = u.FullName,
                                Email = u.Email,
                                AvatarUrl = u.AvatarUrl,
                            },
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
                            RegisteredDate = t.RegisteredDate,

                        };
            return await query.ToListAsync();


        }

        public async Task<int> GetTicketCountByConferencePriceIdAsync(string conferencePriceId)
        {
            return await _context.Tickets
                .Where(t => t.ConferencePriceId == conferencePriceId && !t.IsRefunded.Value)
                .CountAsync();
        }

        public async Task<int> CreateTicketAsync(Ticket ticket)
        {
            return await CreateAsync(ticket);
        }

        public async Task<Ticket> GetTicketByUserIdAndConferenceId(string userId, string conferenceId)
        {
            return await _context.Tickets
                .Include(t => t.ConferencePrice)
                .Where(t => t.UserId == userId && t.ConferencePrice != null && t.ConferencePrice.ConferenceId == conferenceId)
                .FirstOrDefaultAsync();
        }
    }
}
