﻿using AutoMapper;
using Investly.DAL.Entities;
using Investly.DAL.Repos.IRepos;
using Investly.PL.Dtos;
using Investly.PL.General;
using Investly.PL.IBL;

namespace Investly.PL.BL
{
    public class FounderService : IFounderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public FounderService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public int ChangeFounderStatus(int Id, int Status,int ?LoggedInUser)
        {
            try
            {
                if(Id<=0||Status<=0)
                {
                    return -3;
                }
                var founder=_unitOfWork.FounderRepo.FirstOrDefault(x => x.Id == Id,"User");
                if (founder == null)
                {
                    return -2;
                }else
                {
                    founder.User.UpdatedBy = LoggedInUser;
                    founder.User.UpdatedAt = DateTime.Now;
                    founder.User.Status = Status;
                    _unitOfWork.FounderRepo.Update(founder);
                   var res= _unitOfWork.Save();
                    if(res>0)
                    {
                        return 1;
                    }else
                    {
                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        public FoundersPaginatedDto GetAllPaginatedFounders(FounderSearchDto search)
        {
            try
            {
                var FoundersList = _unitOfWork.FounderRepo.GetAll(
             f => (string.IsNullOrEmpty(search.SearchInput) ||
                     f.User.Email.Contains(search.SearchInput) ||
                     f.User.FirstName.Contains(search.SearchInput) ||
                     f.User.LastName.Contains(search.SearchInput) ||
                     f.User.Address.Contains(search.SearchInput) ||
                     f.User.PhoneNumber.Contains(search.SearchInput))&&
                 (search.Gender == null || f.User.Gender == search.Gender) &&
                 (search.GovernmentId == null || search.GovernmentId == 0|| f.User.GovernmentId == search.GovernmentId) &&
                 ( (search.Status == null || search.Status == 0) && f.User.Status != (int)UserStatus.Deleted)||(search.Status != null && search.Status != 0 && f.User.Status == search.Status)

                , "User" ).OrderByDescending(f => f.User.CreatedAt);
                var PaginatedData = FoundersList
                  .Skip(((search.PageNumber > 0 ? search.PageNumber : 1) - 1) * (search.PageSize > 0 ? search.PageSize : 5))
                  .Take(search.PageSize > 0 ? search.PageSize : 10)
                  .ToList();


                var FoundersPaginted = new FoundersPaginatedDto
                {
                    founders=_mapper.Map<List<FounderDto>>(PaginatedData),
                    CurrentPage = (search.PageNumber > 0) ? search.PageNumber : 1,
                    PageSize = (search.PageSize>0)?search.PageSize:5,
                    TotalCount = FoundersList.Count(),

                };
                return FoundersPaginted;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public FounderDto GetFounderById(int Id)
        {
            try
            {
                var founder = _unitOfWork.FounderRepo.FirstOrDefault(x => x.Id == Id, "User");
                if (founder == null)
                {
                    return null;
                }
                var FounderDto = _mapper.Map<FounderDto>(founder);
                return FounderDto; 
               
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public FoundersTotalActiveIactiveDto GetTotalFoundersActiveIactive()
        {
            try
            {
                var founders = _unitOfWork.FounderRepo.GetAll(null,"User").ToList();
                var FounderActive = founders.Count(f => f.User.Status ==(int)UserStatus.Active);
                var FounderInActive= founders.Count(f => f.User.Status == (int)UserStatus.Inactive);
                var ActiveIactiveFounderDto = new FoundersTotalActiveIactiveDto { TotalActive = FounderActive, TotalInactive = FounderInActive };
                return ActiveIactiveFounderDto;

            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
