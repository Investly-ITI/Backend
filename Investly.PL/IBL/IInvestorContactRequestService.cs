using Investly.DAL.Entities;
using Investly.DAL.Helper;
using Investly.PL.Dtos;
using System.Linq.Expressions;

namespace Investly.PL.IBL
{
    public interface IInvestorContactRequestService
    {

        public Task<PaginatedResult<InvestorContactRequestDto>> GetContactRequestsAsync(
            int? pageNumber = 1,
            int? pageSize = 10,
            int? investorIdFilter = null,
            int? founderIdFilter = null,
            bool? statusFilter = null,
            string columnOrderBy = null,
            string orderByDirection = OrderBy.Ascending,
            string searchTerm = null);

        public void ToggelActivateContactRequest(ContactRequestToggleActivationDto model);


    }
}
