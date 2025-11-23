using ConfRadar.Repositories.Models;
using ConfRadar.Services.DTOs.Conference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.Mappers
{
    public static class ConferenceMapper
    {
        public static ConferenceResponse toConferenceResponse(this Conference model)
        {
            return new ConferenceResponse
            {
                Address = model.Address,
                AvailableSlot = model.AvailableSlot,
                BannerImageUrl = model.BannerImageUrl,
                CityId = model.CityId,
                ConferenceCategoryId = model.ConferenceCategoryId,
                ConferenceName = model.ConferenceName,
                ConferenceId = model.ConferenceId,
                ConferenceStatusId = model.ConferenceStatusId,
                CreatedAt = model.CreatedAt,
                CreatedBy = model.CreatedBy,
                Description = model.Description,
                EndDate = model.EndDate,
                IsInternalHosted = model.IsInternalHosted,
                IsResearchConference = model.IsResearchConference,
                StartDate = model.StartDate,
                TicketSaleEnd = model.TicketSaleEnd,
                TicketSaleStart = model.TicketSaleStart,
                TotalSlot = model.TotalSlot,
            };
        }
    }
}
