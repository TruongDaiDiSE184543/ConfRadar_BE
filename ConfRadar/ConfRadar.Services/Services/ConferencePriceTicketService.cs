using ConfRadar.Repositories;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.ConferencePriceTicket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ConfRadar.Services.Services
{
    public interface IConferencePriceTicketService
    {
        Task<List<ConferencePriceTicketListResponse>> GetConferencePriceTicketListAsync(ConferencePriceTicketSearchRequest request);
        Task<ConferencePriceTicketDetailResponse> GetConferencePriceTicketDetailAsync(string priceId);
        Task<int> GetTotalConferencePriceTicketCountAsync(ConferencePriceTicketSearchRequest request);
    }

    public class ConferencePriceTicketService : IConferencePriceTicketService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppSettingConfig.ObjectStorageSettings _objectStorageSettings;

        public ConferencePriceTicketService(IUnitOfWork unitOfWork, IOptions<AppSettingConfig.ObjectStorageSettings> objectStorageSettings)
        {
            _unitOfWork = unitOfWork;
            _objectStorageSettings = objectStorageSettings.Value;
        }

        public async Task<List<ConferencePriceTicketListResponse>> GetConferencePriceTicketListAsync(ConferencePriceTicketSearchRequest request)
        {
            // Build the query with filtering, sorting, and pagination
            var query = _unitOfWork.ConferencePriceRepository.GetConferencePricesWithIncludes();

            // Apply search filters
            if (!string.IsNullOrEmpty(request.TicketName))
            {
                query = query.Where(cp => cp.TicketName != null && cp.TicketName.Contains(request.TicketName));
            }

            if (request.MinPrice.HasValue)
            {
                query = query.Where(cp => cp.TicketPrice >= request.MinPrice.Value);
            }

            if (request.MaxPrice.HasValue)
            {
                query = query.Where(cp => cp.TicketPrice <= request.MaxPrice.Value);
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "ticketname" => request.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(cp => cp.TicketName)
                    : query.OrderBy(cp => cp.TicketName),
                "ticketprice" => request.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(cp => cp.TicketPrice)
                    : query.OrderBy(cp => cp.TicketPrice),
                "actualprice" => request.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(cp => cp.TicketPrice)
                    : query.OrderBy(cp => cp.TicketPrice),
                _ => request.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(cp => cp.TicketPrice)
                    : query.OrderBy(cp => cp.TicketPrice) // Default sort by TicketPrice
            };

            // Apply pagination
            var tickets = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            // Map to response DTO
            var responses = new List<ConferencePriceTicketListResponse>();
            var now = DateOnly.FromDateTime(DateTime.UtcNow);

            foreach (var ticket in tickets)
            {
                string currentPhase = "Unknown";
                decimal? actualPrice = ticket.TicketPrice ?? ticket.TicketPrice; // Default to stored actual price or ticket price

                if (ticket.PricePhases != null)
                {
                    //if (ticket.PricePhase.EarlierBirdEndInterval != null && now <= ticket.PricePhase.EarlierBirdEndInterval)
                    //{
                    //    currentPhase = "Early Bird";
                    //    if (ticket.PricePhase.PercentForEarly.HasValue && ticket.TicketPrice.HasValue)
                    //    {
                    //        actualPrice = ticket.TicketPrice * (ticket.PricePhase.PercentForEarly.Value / 100.0m);
                    //    }
                    //}
                    //else if (ticket.PricePhase.StandardEndInterval != null && now <= ticket.PricePhase.StandardEndInterval)
                    //{
                    //    currentPhase = "Standard";
                    //    actualPrice = ticket.TicketPrice; // Full price during standard phase
                    //}
                    //else if (ticket.PricePhase.LateEndInterval != null && now <= ticket.PricePhase.LateEndInterval)
                    //{
                    //    currentPhase = "Late";
                    //    if (ticket.PricePhase.PercentForEnd.HasValue && ticket.TicketPrice.HasValue)
                    //    {
                    //        actualPrice = ticket.TicketPrice * (ticket.PricePhase.PercentForEnd.Value / 100.0m);
                    //    }
                    //}
                    //else
                    //{
                    //    currentPhase = "Expired"; // After all phases ended
                    //}
                }

                responses.Add(new ConferencePriceTicketListResponse
                {
                    PriceId = ticket.ConferencePriceId,
                    TicketName = ticket.TicketName,
                    TicketDescription = ticket.TicketDescription,
                    TicketPrice = ticket.TicketPrice,
                    ActualPrice = actualPrice,
                    CurrentPhase = currentPhase,
                    ConferenceName = ticket.Conference?.ConferenceName,
                    ConferenceBannerUrl = AddBaseUrlToUrl(ticket.Conference?.BannerImageUrl),
                    ConferenceStartDate = ticket.Conference?.StartDate,
                    ConferenceEndDate = ticket.Conference?.EndDate
                });
            }

            return responses;
        }

        public async Task<ConferencePriceTicketDetailResponse> GetConferencePriceTicketDetailAsync(string priceId)
        {
            var ticket = await _unitOfWork.ConferencePriceRepository.GetConferencePriceWithIncludesAsync(priceId);

            if (ticket == null)
            {
                return null; // Or throw an exception depending on your error handling strategy
            }

            var now = DateOnly.FromDateTime(DateTime.UtcNow);

            string currentPhase = "Unknown";
            decimal? actualPrice = ticket.TicketPrice ?? ticket.TicketPrice; // Default to stored actual price or ticket price

            //if (ticket.PricePhases != null)
            //{
            //    if (ticket.PricePhase.EarlierBirdEndInterval != null && now <= ticket.PricePhase.EarlierBirdEndInterval)
            //    {
            //        currentPhase = "Early Bird";
            //        if (ticket.PricePhase.PercentForEarly.HasValue && ticket.TicketPrice.HasValue)
            //        {
            //            actualPrice = ticket.TicketPrice * (ticket.PricePhase.PercentForEarly.Value / 100.0m);
            //        }
            //    }
            //    else if (ticket.PricePhase.StandardEndInterval != null && now <= ticket.PricePhase.StandardEndInterval)
            //    {
            //        currentPhase = "Standard";
            //        actualPrice = ticket.TicketPrice; // Full price during standard phase
            //    }
            //    else if (ticket.PricePhase.LateEndInterval != null && now <= ticket.PricePhase.LateEndInterval)
            //    {
            //        currentPhase = "Late";
            //        if (ticket.PricePhase.PercentForEnd.HasValue && ticket.TicketPrice.HasValue)
            //        {
            //            actualPrice = ticket.TicketPrice * (ticket.PricePhase.PercentForEnd.Value / 100.0m);
            //        }
            //    }
            //    else
            //    {
            //        currentPhase = "Expired"; // After all phases ended
            //    }
            //}

            return new ConferencePriceTicketDetailResponse
            {
                PriceId = ticket.ConferencePriceId,
                TicketName = ticket.TicketName,
                TicketDescription = ticket.TicketDescription,
                TicketPrice = ticket.TicketPrice,
                ActualPrice = actualPrice,
                CurrentPhase = currentPhase,
                ConferenceName = ticket.Conference?.ConferenceName,
                ConferenceDescription = ticket.Conference?.Description,
                ConferenceBannerUrl = AddBaseUrlToUrl(ticket.Conference?.BannerImageUrl),
                ConferenceStartDate = ticket.Conference?.StartDate,
                ConferenceEndDate = ticket.Conference?.EndDate,
                PricePhase = ticket.PricePhases != null ? new PricePhaseInfoResponse
                {
                    //PricePhaseId = ticket.PricePhase.PricePhaseId,
                    //Name = ticket.PricePhase.Name,
                    //EarlierBirdEndInterval = ticket.PricePhase.EarlierBirdEndInterval,
                    //PercentForEarly = ticket.PricePhase.PercentForEarly,
                    //StandardEndInterval = ticket.PricePhase.StandardEndInterval,
                    //LateEndInterval = ticket.PricePhase.LateEndInterval,
                    //PercentForEnd = ticket.PricePhase.PercentForEnd
                } : null
            };
        }

        public async Task<int> GetTotalConferencePriceTicketCountAsync(ConferencePriceTicketSearchRequest request)
        {
            // Get all prices and filter in memory - this is less efficient but simpler
            var allPrices = await _unitOfWork.ConferencePriceRepository.GetAllConferencePricesAsync();
            var query = allPrices.AsQueryable();

            // Apply search filters
            if (!string.IsNullOrEmpty(request.TicketName))
            {
                query = query.Where(cp => cp.TicketName != null && cp.TicketName.Contains(request.TicketName));
            }

            if (request.MinPrice.HasValue)
            {
                query = query.Where(cp => cp.TicketPrice >= request.MinPrice.Value);
            }

            if (request.MaxPrice.HasValue)
            {
                query = query.Where(cp => cp.TicketPrice <= request.MaxPrice.Value);
            }

            return query.Count();
        }

        /// <summary>
        /// Adds the base MinIO URL to a file URL if it's not already a full URL
        /// </summary>
        private string? AddBaseUrlToUrl(string? url)
        {
            if (string.IsNullOrEmpty(url))
                return url;

            // If the URL already starts with http/https, return as is
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                return url;

            // Prepend the base URL from configuration
            return _objectStorageSettings.EndPoint?.TrimEnd('/') + "/" + url.TrimStart('/');
        }
    }
}