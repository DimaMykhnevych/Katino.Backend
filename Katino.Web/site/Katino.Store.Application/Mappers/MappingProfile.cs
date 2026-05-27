using AutoMapper;
using Katino.Domain.Enums;
using Katino.Store.Application.DTOs;

namespace Katino.Store.Application.Mappers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<GetLogsModeDto, GetLogsMode>();
    }
}
