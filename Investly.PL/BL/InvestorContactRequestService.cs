using AutoMapper;
using Investly.DAL.Entities;
using Investly.DAL.Repos;
using Investly.DAL.Repos.IRepos;
using Investly.PL.Dtos;
using Investly.PL.General;
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
        public async Task<PaginatedResultDto<InvestorContactRequestDto>> GetContactRequestsAsync(
            int? pageNumber = 1,
            int? pageSize = 10,
            int? investorIdFilter = null,
            int? founderIdFilter = null,
            ContactRequestStatus? statusFilter = null,
            string columnOrderBy = null,
            string orderByDirection = Constants.Ascending,
            string searchTerm = null)
        {
            pageNumber ??= 1;
            pageSize ??= 10;

            int? statusFilterValue = statusFilter.HasValue ? (int)statusFilter.Value : null;

            Expression<Func<InvestorContactRequest, bool>> criteria = request =>
                (request.Status != (int)ContactRequestStatus.Deleted) &&
                (!investorIdFilter.HasValue || request.InvestorId == investorIdFilter) &&
                (!founderIdFilter.HasValue || request.Business.FounderId == founderIdFilter) &&
                (!statusFilterValue.HasValue || request.Status == statusFilterValue) &&
                (string.IsNullOrWhiteSpace(searchTerm) ||
                 request.Business.Title.Contains(searchTerm) ||
                 request.Investor.User.FirstName.Contains(searchTerm) ||
                 request.Investor.User.LastName.Contains(searchTerm) ||
                 request.Investor.User.Email.Contains(searchTerm));

            Expression<Func<InvestorContactRequest, object>> orderBy = null;
            string finalOrderByDirection = orderByDirection;

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
            else
            {
                orderBy = request => request.CreatedAt;
                finalOrderByDirection = Constants.Descending;
            }

            PaginatedResultDto<InvestorContactRequest> tempRes = await _queryService.FindAllAsync(
                take: pageSize,
                skip: (pageNumber - 1) * pageSize,
                criteria: criteria,
                orderBy: orderBy,
                orderByDirection: finalOrderByDirection,
                properties: "Investor.User,Business.Founder.User,Business"
            );

            PaginatedResultDto<InvestorContactRequestDto> res = new PaginatedResultDto<InvestorContactRequestDto>()
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

        public InvestorContactRequestDto GetContactRequestById (int contactId)
        {
            var contact = _unitOfWork.InvestorContactRequestRepo.FirstOrDefault((req => req.Id == contactId), "Investor.User,Business.Founder.User,Business");
            if (contact == null)
                throw new KeyNotFoundException($"Contact With Id {contactId} Not found");

            return _mapper.Map<InvestorContactRequestDto>(contact);
        }

        public void UpdateContactRequestStatus(UpdateContactRequestStatusDto model)
        {
            var contact = _unitOfWork.InvestorContactRequestRepo.GetById(model.ContactRequestId);
            if (contact == null)
                throw new KeyNotFoundException($"Contact with ID {model.ContactRequestId} not found.");

            // Validate current status is Pending
            if (contact.Status != (int)ContactRequestStatus.Pending)
            {
                throw new InvalidOperationException(
                    $"Status can only be changed from Pending. Current status is: {Enum.GetName(typeof(ContactRequestStatus), contact.Status)}"
                );
            }

            // Validate the new status is a valid enum value
            if (!Enum.IsDefined(typeof(ContactRequestStatus), model.NewStatus))
            {
                throw new ArgumentException(
                    $"Invalid status value. Valid values are: {string.Join(", ", Enum.GetNames(typeof(ContactRequestStatus)))}"
                );
            }

            // Special validation: DeclineReason required ONLY when transitioning to Declined
            if (model.NewStatus == ContactRequestStatus.Declined && string.IsNullOrWhiteSpace(model.DeclineReason))
            {
                throw new ArgumentException("DeclineReason is required when status is set to Declined.");
            }

            // Update status and reason (clear DeclineReason if not Declined)
            contact.Status = (int)model.NewStatus;
            contact.DeclineReason = model.NewStatus == ContactRequestStatus.Declined ? model.DeclineReason : null;
            contact.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Save();
        }

        public List<InvestorContactRequestDto> GetContactRequestsByInvestor(int? LoggedInUser)
        {
            try
            {
                var request=_unitOfWork.InvestorContactRequestRepo.GetAll(b=>b.Investor.UserId== LoggedInUser&&b.Status!=(int)ContactRequestStatus.Deleted,
                    "Investor.User,Business.Founder.User,Business.City,Business.Government,Business.Category").OrderByDescending(b=>b.CreatedAt);
                var InvestorRequests = _mapper.Map<List<InvestorContactRequestDto>>(request);
                return InvestorRequests;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public CountContactRequestDto GetContactRequestsCountByInvestor(int? LoggedInUser)
        {
            try
            {
                var count = _unitOfWork.InvestorContactRequestRepo.GetAll(b => b.Investor.UserId == LoggedInUser && b.Status != (int)ContactRequestStatus.Deleted,"Investor").Count();
                CountContactRequestDto res = new CountContactRequestDto { TotalContactRequestCount = count };
            
                return res;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

}
