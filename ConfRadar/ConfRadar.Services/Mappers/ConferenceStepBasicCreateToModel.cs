using ConfRadar.Repositories.Models;
using ConfRadar.Services.Common;
using ConfRadar.Services.DTOs.ConferenceStep;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.Mappers
{
    public static class ConferenceStepBasicCreateToModel
    {
        public static Conference creatBasicConference(CreateTechnicalConferenceBasicRequest request,Repositories.Models.ConferenceStatus status, DateOnly now)
        {
            var conferenceObject = new Conference
            {
                ConferenceId = Guid.NewGuid().ToString(),
                ConferenceName = request.ConferenceName,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TotalSlot = request.TotalSlot,
                AvailableSlot = request.TotalSlot,
                Address = request.Address,
                BannerImageUrl = request.bannerImageFileUrl,
                TicketSaleStart = request.TicketSaleStart,
                TicketSaleEnd = request.TicketSaleEnd,
                IsInternalHosted = request.IsInternalHosted,
                IsResearchConference = request.IsResearchConference,
                CityId = request.CityId,
                CreatedBy = request.createdby,
                ConferenceCategoryId = request.ConferenceCategoryId,
                ConferenceStatusId = status.ConferenceStatusId,
                CreatedAt = now
            };
            return conferenceObject;
        }
    }
}
