using AutoMapper;
using Investly.DAL.Entities;
using Investly.PL.Dtos;

namespace Investly.PL.Mapper
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Dtos.UserDto,User>().ReverseMap();   
            CreateMap<Dtos.InvestorDto, Investor>().ReverseMap();
            CreateMap<GovernmentDto, Government>().ReverseMap();
            CreateMap<CityDto, City>().ReverseMap();


            CreateMap <Dtos.FounderDto,Founder>().ReverseMap();
            CreateMap<InvestorContactRequest, InvestorContactRequestDto>()
                .AfterMap((src, dest) =>
                {
                    dest.InvestorName = $"{src.Investor.User.FirstName} {src.Investor.User.LastName}";
                    dest.BusinessTitle = $"{src.Business.Title}";
                    dest.FounderName = $"{src.Business.Founder.User.FirstName} {src.Business.Founder.User.LastName}";
                    dest.BusinessId = src.Business.Id;
                    dest.InvestorId = src.Investor.Id; // Fixed: was src.Business.Id
                    dest.FounderId = src.Business.Founder.Id;
                });
        }
    }
}
