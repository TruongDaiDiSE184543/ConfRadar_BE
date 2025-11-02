using ConfRadar.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRadar.Services.Services
{
    public interface INotificationService
    {
        Task<string> NotifyNextInWaitListInAConferenceAsync(string conferenceId, string pendingWaitListStatusId, string notifiedAtWaitListStatusId, DateTime notifiedAt);
    }
    public class NotificationService :INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        public NotificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<string> NotifyNextInWaitListInAConferenceAsync(string conferenceId, string pendingWaitListStatusId, string notifiedAtWaitListStatusId, DateTime notifiedAt)
        {
            return await _unitOfWork.PaperWaitListRepository.NotifyNextInWaitListInAConferenceAsync(conferenceId, pendingWaitListStatusId, notifiedAtWaitListStatusId, notifiedAt);
        }
    }
}
