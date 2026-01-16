using AutoMapper;
using Investly.DAL.Entities;
using Investly.DAL.Repos;
using Investly.DAL.Repos.IRepos;
using Investly.PL.Dtos;
using Investly.PL.General;
using Investly.PL.General.Services.IServices;
using Investly.PL.IBL;
using iText.Forms.Xfdf;
using System.Diagnostics.Metrics;
using System.Linq.Expressions;

namespace Investly.PL.BL
{
    public class InvestorContactRequestService : IInvestorContactRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IQueryService<InvestorContactRequest> _queryService;
        private readonly INotficationService _notficationService;

        public InvestorContactRequestService(
            IUnitOfWork unitOfWork, 
            IMapper mapper,
            IQueryService<InvestorContactRequest> queryService,
            INotficationService notficationService
            )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _queryService = queryService;
            _notficationService = notficationService;
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

        public void UpdateContactRequestStatus(UpdateContactRequestStatusDto model,int? LoggedInUser, string? loggedInEmail = null)
        {
            var contact = _unitOfWork.InvestorContactRequestRepo.FirstOrDefault(c=>c.Id==model.ContactRequestId, "Investor.User,Business.Founder");
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

         var res=   _unitOfWork.Save();

            if (res > 0&&loggedInEmail== "SuperAdmin@gmail.com")
            {
                var investorId = contact.Investor.UserId;
                var founderId = contact.Business.Founder.UserId;
                if (model.NewStatus == ContactRequestStatus.Accepted)
                {
                    NotificationDto FounderNotification = new NotificationDto
                    {
                        Title = "Contact Request Status.",
                        Body = $"You have received a new contact request. The investor will reach out to you shortly.",
                        UserTypeTo = (int)UserType.Founder,
                        UserIdTo = founderId,

                    };
                    _notficationService.SendNotification(FounderNotification, LoggedInUser, (int)UserType.Staff);
                }
                NotificationDto InvestorNotification = new NotificationDto
                {
                    Title = "Contact Request Status.",
                    Body = $"Your Contact Request has been {(ContactRequestStatus)model.NewStatus}.",
                    UserTypeTo = (int)UserType.Investor,
                    UserIdTo = investorId,

                };
                _notficationService.SendNotification(InvestorNotification, LoggedInUser, (int)UserType.Staff);
            }
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

        public int CreateContactRequest(int businessId, int? loggedInUser)
        {
            if (!loggedInUser.HasValue)
            {
                throw new UnauthorizedAccessException("User not authenticated.");
            }

            // Get investor details
            var investor = _unitOfWork.InvestorRepo.FirstOrDefault(i => i.UserId == loggedInUser.Value);
            if (investor == null)
            {
                throw new InvalidOperationException("Only investors can create contact requests.");
            }

            // Get business details
            var business = _unitOfWork.BusinessRepo.FirstOrDefault(b => b.Id == businessId, "Founder");
            if (business == null)
            {
                throw new KeyNotFoundException($"Business with ID {businessId} not found.");
            }

            // Check if business is active
            if (business.Status != (int)BusinessIdeaStatus.Active)
            {
                throw new InvalidOperationException("Business is not active and cannot receive contact requests.");
            }

            // Check if contact request already exists
            var existingRequest = _unitOfWork.InvestorContactRequestRepo.FirstOrDefault(
                icr => icr.InvestorId == investor.Id &&
                       icr.BusinessId == businessId &&
                       icr.Status != (int)ContactRequestStatus.Deleted);

            if (existingRequest != null)
            {
                throw new InvalidOperationException("A contact request for this business already exists.");
            }

            // Check if investor has more than 4 pending requests
            var pendingRequestsCount = _unitOfWork.InvestorContactRequestRepo.GetAll(
                icr => icr.InvestorId == investor.Id &&
                        icr.Status == (int)ContactRequestStatus.Pending).Count();

            if (pendingRequestsCount >= Constants.ContactRequestsLimit)
            {
                throw new InvalidOperationException("You cannot have more than 4 pending contact requests.");
            }

            // Create new contact request
            var contactRequest = new InvestorContactRequest
            {
                InvestorId = investor.Id,
                BusinessId = businessId,
                Status = (int)ContactRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = loggedInUser.Value
            };

            _unitOfWork.InvestorContactRequestRepo.Insert(contactRequest);
            var result = _unitOfWork.Save();

            if (result <= 0)
            {
                throw new InvalidOperationException("Failed to save contact request to database.");
            }

            try
            {
                NotificationDto adminNotification = new NotificationDto
                {
                    Title = "New Contact Request - Pending Approval",
                    Body = $"A new contact request has been submitted by an investor for the business idea: {business.Title}",
                    UserTypeTo = (int)UserType.Staff,
                    UserIdTo = _unitOfWork.UserRepo.FirstOrDefault(u => u.UserType == (int)UserType.Staff && u.Status == (int)UserStatus.Active).Id,
                };
                _notficationService.SendNotification(adminNotification, loggedInUser, (int)UserType.Investor);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send notification: {ex.Message}"); // i don't want to stop the whole operation if notification failed to be sent
            }

            return contactRequest.Id;
        }

    }

}
