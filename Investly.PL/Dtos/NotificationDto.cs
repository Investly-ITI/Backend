using Investly.DAL.Entities;

namespace Investly.PL.Dtos
{
    public class NotificationDto
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? Body { get; set; }

        public int? UserTypeFrom { get; set; }

        public int UserTypeTo { get; set; }

        public int UserIdTo { get; set; }

        public int IsRead { get; set; }

        public int? Status { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime? CreatedAt { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public UserDto? CreatedByNavigation { get; set; }

        public UserDto? UpdatedByNavigation { get; set; }

        public UserDto? UserIdToNavigation { get; set; } = null!;


    }
    public class PaginatedNotificationsDto
    {
        public List<NotificationDto> notifications { get; set; } = new List<NotificationDto>();
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
    }
    public class NotificationSearchDto
    {
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public string? SearchInput { get; set; }
        public int? UserTypeFrom { get; set; }
        public int? UserTypeTo { get; set; }
        public int? isRead { get; set; }
        public int? Status { get; set; }
    }
    public class NotifcationsTotalActiveDeletedDto
    {
        public int TotalActive { get; set; }
        public int TotalDeleted { get; set; }
    }

}
