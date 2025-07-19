using AutoMapper;
using Models;
using Eleganza.DTO;

namespace Eleganza.Profiles
{
    public class ApplicationUserProfiles :Profile
    {
        public ApplicationUserProfiles()
        {
            CreateMap<ApplicationUserDTO, ApplicationUser>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserName, option => option.MapFrom(src => $"{src.FristName}_{src.LastName}"))
            .ForMember(dest => dest.Adderss, option => option.MapFrom(src => src.Address));

        }
    }
}