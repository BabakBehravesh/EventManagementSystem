namespace EventManagementSystem.Application.Profiles;

using AutoMapper;
using EventManagementSystem.Application.DTOs;
using EventManagementSystem.Domain.Models;

public class EventMappingProfile : Profile
{
	public EventMappingProfile()
	{
        // Domain to DTO
        CreateMap<Event, EventResponse>();

        // DTO to Domain
        CreateMap<EventRequest, Event>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());
    }
}
