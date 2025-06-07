using Investly.DAL.Repos.IRepos;
using Investly.PL.Dtos;

namespace Investly.PL.IBL
{
    public interface INotficationService
    {
        public PaginatedNotificationsDto GetallPaginatedNotifications(NotificationSearchDto search);
        public int SendNotification(NotificationDto notification, int? LoggedInUser, int LoggedInUserType);
        public int ChnageStatus(int NotificationId,int Status, int? LoggedInUser);
        public NotifcationsTotalActiveDeletedDto GetTotalNotificationsActiveDeleted();
    }
}
