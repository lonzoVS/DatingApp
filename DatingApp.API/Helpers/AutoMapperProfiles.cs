using System.Linq;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;

namespace DatingApp.API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<User, UserForListDto>().ForMember(dest => dest.PhotoUrl, opt => {
                opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url);
            })
            .ForMember(dest => dest.Age, opt => opt.ResolveUsing(d => d.DateOfBirth.CalculateAge()));
            CreateMap<User, UserForDetailedDto>().ForMember(dest => dest.PhotoUrl, opt => {
                opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url);
            })
            .ForMember(dest => dest.Age, opt => opt.ResolveUsing(d => d.DateOfBirth.CalculateAge()));;
            CreateMap<Photo, PhotosForDetiledDto>();
            CreateMap<UserForUpdateDto, User>();
            CreateMap<Photo, PhotoForReturnDto>();
            CreateMap<PhoroForCretionDto, Photo>();
            CreateMap<UserForRegisterDto, User>();

            CreateMap<MessageForCreationsDto, Message>().ReverseMap();

            CreateMap<Message, MessageToReturnDto>()
                .ForMember(m => m.SenderPhotoUrl, opt => opt
                    .MapFrom(u => u.Sender.Photos.FirstOrDefault(p => p.IsMain).Url))
                .ForMember(m => m.RecipientPhotoUrl, opt => opt
                    .MapFrom(u => u.Recipient.Photos.FirstOrDefault(p => p.IsMain).Url));
        }
    }
}