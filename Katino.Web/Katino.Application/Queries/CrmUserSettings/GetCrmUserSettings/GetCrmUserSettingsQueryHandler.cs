using AutoMapper;
using Katino.Application.DTOs.CrmUserSettings;
using Katino.Application.DTOs.NpCity;
using Katino.Application.DTOs.NpWarehouse;
using Katino.Domain.Repositories.CrmUserSettingsRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Queries.CrmUserSettings.GetCrmUserSettings;

public class GetCrmUserSettingsQueryHandler : IRequestHandler<GetCrmUserSettingsQuery, GetCrmUserSettingsDto>
{
    private readonly ICrmUserSettingsRepository _crmUserSettingsRepository;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public GetCrmUserSettingsQueryHandler(
        ICrmUserSettingsRepository crmUserSettingsRepository,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _crmUserSettingsRepository = crmUserSettingsRepository;
        _logger = loggerFactory?.CreateLogger(nameof(GetCrmUserSettingsQueryHandler));
        _mapper = mapper;
    }

    public async Task<GetCrmUserSettingsDto> Handle(GetCrmUserSettingsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get crm user settings");
        ArgumentNullException.ThrowIfNull(request);

        var settings = await _crmUserSettingsRepository.GetAppUserSettingsWithFullInfo(request.AppUserId);
        if (settings == null)
        {
            return new();
        }

        var cityInfo = settings.NpCity == null ? null : _mapper.Map<GetNpCityDto>(settings.NpCity);
        var warehouse = settings.NpWarehouse == null ? null : _mapper.Map<NpWarehouseDto>(settings.NpWarehouse);

        return new() { Id = settings.Id, NpCity = cityInfo, NpWarehouse = warehouse };
    }
}
