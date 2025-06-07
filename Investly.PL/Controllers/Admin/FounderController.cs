using Investly.PL.Attributes;
using Investly.PL.BL;
using Investly.PL.Dtos;
using Investly.PL.Extentions;
using Investly.PL.General;
using Investly.PL.IBL;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Investly.PL.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [AuthorizeUserType(((int)UserType.Staff))]
    [ApiController]
    public class FounderController : Controller
    {
       private readonly  IFounderService _founderService;
        public FounderController(IFounderService founderService)
        {
            _founderService = founderService;
        }
        [HttpPost("PaginatedFounders")]
        public IActionResult GetAllFoundersPaginated([FromBody]FounderSearchDto search)
        {

            FoundersPaginatedDto founders = _founderService.GetAllPaginatedFounders(search);
            ResponseDto<FoundersPaginatedDto> res = new ResponseDto<FoundersPaginatedDto>
            { IsSuccess = true ,Data=founders,Message="Founders Retrived Sucssfullyy",StatusCode=StatusCodes.Status200OK};
            return Ok(res);
        }
        [HttpGet("ActiveInactiveFounders")]
       
        public IActionResult GetTotalFoundersActiveIactive()
        {

            FoundersTotalActiveIactiveDto founders = _founderService.GetTotalFoundersActiveIactive();
            ResponseDto<FoundersTotalActiveIactiveDto> res = new ResponseDto<FoundersTotalActiveIactiveDto>
            { IsSuccess = true, Data = founders, Message = "Founders By Status Retrived Sucssfullyy", StatusCode = StatusCodes.Status200OK };
            return Ok(res);
        }
        [HttpPut("ChangeFounderStatus/{id}")]
        public IActionResult ChangeFoundersStatus(int id,[FromQuery]int Status)
        {
          
            int status = _founderService.ChangeFounderStatus(id,Status, User.GetUserId());
            ResponseDto<object> res;
            if (status > 0)
            {
             res = new ResponseDto<object>
                { IsSuccess = true, Data = null, Message = "Founder Status Changed Sucssfullyy", StatusCode = StatusCodes.Status200OK };
                return Ok(res);
            }
            else
            {
                 res = new ResponseDto<object>
                { IsSuccess = false, Data = null, Message = "Founder Status Changed Failed", StatusCode = StatusCodes.Status500InternalServerError };
                return Ok(res);
            }
              
            
        }
        [HttpGet("GetFounderById/{id}")]
        public IActionResult GetFounderByID(int id)
        {

          
            FounderDto founder = _founderService.GetFounderById(id);
            ResponseDto<FounderDto> res = new ResponseDto<FounderDto>
            { IsSuccess = true, Data = founder, Message = "Founder Retrived Sucssfullyy", StatusCode = StatusCodes.Status200OK };
            return Ok(res);
        }
    }
}
