using AutoMapper;
using EventManagementSystem.Application.DTOs;
using EventManagementSystem.Domain.Interfaces;
using EventManagementSystem.Domain.Models;

namespace EventManagementSystem.Application.Profiles;

public class ParticipationMappingProfile : Profile
{
    public ParticipationMappingProfile()
    {
        // DTO to Model
        CreateMap<ParticipationRequest, Participation>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.EventId, opt => opt.Ignore())
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber ?? string.Empty));

        // Model to DTO
        CreateMap<Participation, ParticipationResponse>()
            .ForMember(dest => dest.EventName, src => src.MapFrom(x => x.Event != null ? x.Event.Name : string.Empty))
            .ForMember(dest => dest.EventId, src => src.MapFrom(x => x.EventId))
            .ForMember(dest => dest.Name, src => src.MapFrom(x => x.Name)) 
            .ForMember(dest => dest.Email, src => src.MapFrom(x => x.Email)) 
            .ForMember(dest => dest.PhoneNumber, src => src.MapFrom(x => x.PhoneNumber)) 
            .ForMember(dest => dest.Id, src => src.MapFrom(x => x.Id));
    }
}
