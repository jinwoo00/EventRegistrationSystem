using AutoMapper;
using EventRegistrationSystem.DTOs;
using EventRegistrationSystem.ViewModel;

namespace EventReistrationSystem.MappingProfiles
{
    public class AuthMappingProfile : Profile
    {
        public AuthMappingProfile()
        {
            CreateMap<LoginViewModel, LoginDTO>().ReverseMap();
            CreateMap<RegisterViewModel, RegisterDTO>().ReverseMap();
            CreateMap<VerifyOTPViewModel, VerifyOTPDTO>();
        }
    }
}