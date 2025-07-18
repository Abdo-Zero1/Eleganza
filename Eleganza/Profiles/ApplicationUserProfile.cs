using AutoMapper;
using Models;
using Eleganza.DTOs;

namespace Eleganza.Profiles
{
    public class ApplicationUserProfile :Profile
    {
        public ApplicationUserProfile()
        {
            CreateMap<ApplicationUserDTO, ApplicationUser>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserName, option => option.MapFrom(src => $"{src.FristName}_{src.LastName}"))
            .ForMember(dest => dest.Adderss, option => option.MapFrom(src => src.Address));

        }
    }
}