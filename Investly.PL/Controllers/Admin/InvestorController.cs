﻿using Investly.PL.Attributes;
using Investly.PL.Dtos;
using Investly.PL.Extentions;
using Investly.PL.General;
using Investly.PL.General.Services.IServices;
using Investly.PL.IBL;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Investly.PL.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [AuthorizeUserType(((int)UserType.Staff))]
    [ApiController]

    public class InvestorController : ControllerBase
    {
        private readonly IInvestorService _investorService;
        private readonly IHelper _helper;
        public InvestorController(IInvestorService investorService, IHelper helper)
        {
            _investorService = investorService;
            _helper = helper;
        }
       
        [HttpPost("paginated")]
        public ResponseDto<InvestorDtoWithPagination> GetPaginted(InvestorSearchDto dataSearch)
         {
            InvestorDtoWithPagination paginatedDto = _investorService.GetPaginatedData(dataSearch);
            return new ResponseDto<InvestorDtoWithPagination>
            {
                IsSuccess = true,
                Message = "Investors retrieved successfully.",
                Data = paginatedDto,
                StatusCode = StatusCodes.Status200OK
            };

        }

        [AuthorizeUserType((int)UserType.Staff)]

        [HttpGet("total-active-inactive")]
        public ResponseDto<InvestorTotalActiveIactiveDto> GetTotalActiveInactive()
        {
            var total = _investorService.GetTotalActiveInactiveInvestors();
            return new ResponseDto<InvestorTotalActiveIactiveDto>
            {
                IsSuccess = true,
                Message = "Total active and inactive investors retrieved successfully.",
                Data = total,
                StatusCode = StatusCodes.Status200OK
            };
        }


        [HttpPost]
        public ResponseDto<InvestorDto> Post([FromForm] InvestorDto data)
        {
          
            var picpath = _helper.UploadFile(data.User.PicFile, "investor","profilePic");
            var frontIdPath=_helper.UploadFile(data.User.FrontIdPicFile, "investor", "nationalIdPic");
            var backIdPath = _helper.UploadFile(data.User.BackIdPicFile, "investor", "nationalIdPic");
            data.User.ProfilePicPath = picpath;
            data.User.FrontIdPicPath = frontIdPath;
            data.User.BackIdPicPath = backIdPath;
            var result = _investorService.Add(data,User.GetUserId());
            ResponseDto<InvestorDto> response;
            if (result > 0)
            {
                response = new ResponseDto<InvestorDto>
                {
                    IsSuccess = true,
                    Message = "Investor added successfully.",
                    Data = _investorService.GetById(result),
                    StatusCode = StatusCodes.Status201Created
                };
             
            }
            else
            {
                response = new ResponseDto<InvestorDto> {
                
                    IsSuccess = false,
                    Message = "Failed to add investor.",
                    Data = null,
                    StatusCode = StatusCodes.Status500InternalServerError
                };
           
            }
            return response;
        }


        [HttpPut]
        public ResponseDto<InvestorDto> Put([FromBody] InvestorDto data)
        {
            var result = _investorService.Update(data,User.GetUserId());
            ResponseDto<InvestorDto> response;
            if (result > 0)
            {
                response = new ResponseDto<InvestorDto>
                {
                    IsSuccess = true,
                    Message = "Investor updated successfully.",
                    Data = _investorService.GetById(data.Id??0),
                    StatusCode = StatusCodes.Status200OK
                };

            }
            else
            {
                response = new ResponseDto<InvestorDto>
                {
                    IsSuccess = false,
                    Message = "Failed to update investor.",
                    Data = null,
                    StatusCode = StatusCodes.Status500InternalServerError
                };


            }
            return response;
        }

        [HttpPut("change-status/{id}")]
        public ResponseDto<object> ChangeStatus(int id, [FromQuery] int status)
        {
            var result = _investorService.ChangeStatus(id, status, User.GetUserId());
            ResponseDto<object> response;
            if (result > 0)
            {
                response = new ResponseDto<object>
                {
                    IsSuccess = true,
                    Message = "Investor status changed successfully.",
                    Data = null,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            else
            {
                response = new ResponseDto<object>
                {
                    IsSuccess = false,
                    Message = "Failed to change investor status.",
                    Data = null,
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
            return response;
        }

    }
}
