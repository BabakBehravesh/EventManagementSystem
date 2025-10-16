using AutoMapper;
using EventManagementSystem.Application.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.UnitTests.Common;

public abstract class TestBase
{
    protected readonly IMapper Mapper;

    protected TestBase()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ParticipationMappingProfile>();
            cfg.AddProfile<EventMappingProfile>();
        });
        Mapper = config.CreateMapper();
    }
}
