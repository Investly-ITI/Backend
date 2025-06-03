using AutoMapper;
using Investly.DAL.Entities;
using Investly.DAL.Helper;
using Investly.DAL.Repos;
using Investly.DAL.Repos.IRepos;
using Investly.PL.Dtos;
using Investly.PL.General.Services.IServices;
using Investly.PL.IBL;
using System.Linq.Expressions;

namespace Investly.PL.BL
{
    public class InvestorContactRequestService : IInvestorContactRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IQueryService<InvestorContactRequest> _queryService;

        public InvestorContactRequestService(
            IUnitOfWork unitOfWork, 
            IMapper mapper,
            IQueryService<InvestorContactRequest> queryService
            )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _queryService = queryService;
        }
        public async Task<PaginatedResult<InvestorContactRequestDto>> GetContactRequestsAsync(
            int? pageNumber = 1,
            int? pageSize = 10,
            int? investorIdFilter = null,
            int? founderIdFilter = null,
            bool? statusFilter = null,
            string columnOrderBy = null,
            string orderByDirection = OrderBy.Ascending,
            string searchTerm = null)
        {
            pageNumber ??= 1;
            pageSize ??= 10;

            // Build comprehensive criteria
            Expression<Func<InvestorContactRequest, bool>> criteria = request =>
                (!investorIdFilter.HasValue || request.InvestorId == investorIdFilter) &&
                (!founderIdFilter.HasValue || request.Business.Founder.User.Id == founderIdFilter) &&
                (!statusFilter.HasValue || request.Status == statusFilter) &&
                (string.IsNullOrWhiteSpace(searchTerm) ||
                 request.Business.Title.Contains(searchTerm) ||
                 request.Investor.User.FirstName.Contains(searchTerm) ||
                 request.Investor.User.LastName.Contains(searchTerm) ||
                 request.Investor.User.Email.Contains(searchTerm));

            // Apply ordering
            Expression<Func<InvestorContactRequest, object>> orderBy = null;
            if (!string.IsNullOrEmpty(columnOrderBy))
            {
                if (columnOrderBy.Equals("Investor Name", StringComparison.OrdinalIgnoreCase))
                {
                    orderBy = request => request.Investor.User.FirstName + " " + request.Investor.User.LastName;
                }
                else if (columnOrderBy.Equals("Founder Name", StringComparison.OrdinalIgnoreCase))
                {
                    orderBy = request => request.Business.Founder.User.FirstName;
                }
                else if (columnOrderBy.Equals("createdAt", StringComparison.OrdinalIgnoreCase))
                {
                    orderBy = request => request.CreatedAt;
                }
                else if (columnOrderBy.Equals("Status", StringComparison.OrdinalIgnoreCase))
                {
                    orderBy = request => request.Status;
                }
                else if (columnOrderBy.Equals("Business Title", StringComparison.OrdinalIgnoreCase))
                {
                    orderBy = request => request.Business.Title;
                }
            }

            // Get paginated results from repository
            PaginatedResult<InvestorContactRequest> tempRes = await _queryService.FindAllAsync(
                take: pageSize,
                skip: (pageNumber - 1) * pageSize,
                criteria: criteria,
                orderBy: orderBy,
                orderByDirection: orderByDirection,
                properties: "Investor.User,Business.Founder.User,Business"
            );

            // Map to DTO and return
            PaginatedResult<InvestorContactRequestDto> res = new PaginatedResult<InvestorContactRequestDto>()
            {
                Items = _mapper.Map<List<InvestorContactRequestDto>>(tempRes.Items),
                PageSize = tempRes.PageSize,
                CurrentPage = tempRes.CurrentPage,
                TotalPages = tempRes.TotalPages,
                TotalFilteredItems = tempRes.TotalFilteredItems,
                TotalItemsInTable = tempRes.TotalItemsInTable,
            };

            return res;
        }

        public async Task<InvestorContactRequestDto> GetContactRequestById (int contactId)
        {
            var contact = _unitOfWork.InvestorContactRequestRepo.GetById(contactId);
            if (contact == null)
                throw new Exception($"Contact With Id {contactId} Not found");

            return _mapper.Map<InvestorContactRequestDto>(contact);
        }

        public void ToggelActivateContactRequest(ContactRequestToggleActivationDto model)
        {
            var contact =  _unitOfWork.InvestorContactRequestRepo.GetById(model.ContactRequestId);
            if (contact == null)
                throw new Exception($"Contact With Id {model.ContactRequestId} Not found");

            if (contact.Status && model.DeclineReason == null)
                throw new ArgumentException("DeclineReason is Required");

            contact.Status = !contact.Status;
            if (!contact.Status)
                contact.DeclineReason = model.DeclineReason;
            _unitOfWork.Save();
        }


    }

}
