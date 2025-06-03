using Investly.DAL.Helper;
using Investly.PL.Dtos;
using Investly.PL.IBL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Investly.PL.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvestorContactController : ControllerBase
    {
        private readonly IInvestorContactRequestService _investorContactRequestService;

        public InvestorContactController(IInvestorContactRequestService investorContactRequestService)
        {
            _investorContactRequestService = investorContactRequestService;
        }

        [HttpGet]
        public async Task<IActionResult> GetContactRequests(
            int? pageNumber,
            int? pageSize,
            int? investorIdFilter,
            int? founderIdFilter,
            bool? statusFilter,
            string columnOrderBy = null,
            string orderByDirection = OrderBy.Ascending,
            string searchTerm = null)
        {
            var result = await _investorContactRequestService.GetContactRequestsAsync(
                pageNumber,
                pageSize,
                investorIdFilter,
                founderIdFilter,
                statusFilter,
                columnOrderBy,
                orderByDirection,
                searchTerm
            );

            return Ok(result);
        }


        [HttpPut("toggle-activation")]
        public IActionResult ToggleActivationAsync([FromBody] ContactRequestToggleActivationDto model)
        {
            try
            {
                _investorContactRequestService.ToggelActivateContactRequest(model);
                return Ok(new { message = "Contact request activation status updated successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }




    }
}
