using Investly.DAL.Entities;
using Investly.PL.Attributes;
using Investly.PL.Dtos;
using Investly.PL.Extentions;
using Investly.PL.General;
using Investly.PL.IBL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Investly.PL.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
  //  [AuthorizeUserType(((int)UserType.Staff))]
    public class NotificationController : Controller
    {
        private readonly INotficationService _notificationService;
        public NotificationController(INotficationService notificationService)
        {
            _notificationService = notificationService;
        }
        [HttpPost("PaginatedNotifications")]
        public IActionResult GetAllFoundersPaginated( NotificationSearchDto search)
        {

            PaginatedNotificationsDto notifications = _notificationService.GetallPaginatedNotifications(search);
            ResponseDto<PaginatedNotificationsDto> res = new ResponseDto<PaginatedNotificationsDto>
            { IsSuccess = true, Data = notifications, Message = "Notifications Retrived Sucssfullyy", StatusCode = StatusCodes.Status200OK };
            return Ok(res);
        }
        [HttpPost("SendNotification")]
        public IActionResult SendNotification(NotificationDto notification)
        {
            int res = _notificationService.SendNotification(notification, User.GetUserId(), User.GetUserType());
            ResponseDto<object> Data;
            if (res > 0)
            {
                Data = new ResponseDto<object>
                { IsSuccess = true, Data = null, Message = "Notifaction Sent Sucssfullyy", StatusCode = StatusCodes.Status200OK };
                return Ok(Data);
            }
            else
            {
                Data = new ResponseDto<object>
                { IsSuccess = false, Data = null, Message = "Notifaction Sent Failed", StatusCode = StatusCodes.Status500InternalServerError };
                return Ok(Data);
            }
            return Ok();
        }
        [HttpPut("ChangeStatus/{id}")]
        public IActionResult ChangeStatus(int id, int Status)
        {
            int res = _notificationService.ChnageStatus(id, Status, User.GetUserId());
            ResponseDto<object> Data;
            if (res > 0)
            {
                Data = new ResponseDto<object>
                { IsSuccess = true, Data = null, Message = "Status Change Sucssfullyy", StatusCode = StatusCodes.Status200OK };
                return Ok(Data);
            }
            else if(res==-3)
            {
                Data = new ResponseDto<object>
                { IsSuccess = false, Data = null, Message = "You Don't Have Acces To Change Status", StatusCode = StatusCodes.Status500InternalServerError };
                return Ok(Data);
            }
            else
            {
                Data = new ResponseDto<object>
                { IsSuccess = false, Data = null, Message = "Status Chnage Failed", StatusCode = StatusCodes.Status500InternalServerError };
                return Ok(Data);
            }
           
            return Ok();
        }
        [HttpGet("TotalNotificationsActiveDeleted")]
        public IActionResult GetTotalNotificationsActiveDeleted()
        {
            var data= _notificationService.GetTotalNotificationsActiveDeleted();
            ResponseDto<NotifcationsTotalActiveDeletedDto> res = new ResponseDto<NotifcationsTotalActiveDeletedDto>
            { IsSuccess = true, Data = data, Message = "Notifactions By Status Retrived Sucssfullyy", StatusCode = StatusCodes.Status200OK };
            return Ok(res);
        }
    }
}
