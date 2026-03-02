using AutoMapper;
using Katino.Application.DTOs.CrmUserSettings;
using Katino.Application.DTOs.NpCity;
using Katino.Application.DTOs.NpWarehouse;
using Katino.Domain.Constants;
using Katino.Domain.Context;
using Katino.Domain.Repositories.CrmUserSettingsRepository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Queries.CrmUserSettings.GetCrmUserSettings;

public class GetCrmUserSettingsQueryHandler : IRequestHandler<GetCrmUserSettingsQuery, GetCrmUserSettingsDto>
{
    private readonly ICrmUserSettingsRepository _crmUserSettingsRepository;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;
    private readonly IKatinoDbContext _katinoDbContext;

    public GetCrmUserSettingsQueryHandler(
        ICrmUserSettingsRepository crmUserSettingsRepository,
        ILoggerFactory loggerFactory,
        IMapper mapper,
        IKatinoDbContext katinoDbContext)
    {
        _katinoDbContext = katinoDbContext;
        _crmUserSettingsRepository = crmUserSettingsRepository;
        _logger = loggerFactory?.CreateLogger(nameof(GetCrmUserSettingsQueryHandler));
        _mapper = mapper;
    }

    public async Task<GetCrmUserSettingsDto> Handle(GetCrmUserSettingsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get crm user settings");
        ArgumentNullException.ThrowIfNull(request);

        var userInfo = await _katinoDbContext.AppUsers.FirstOrDefaultAsync(u => u.Id == request.AppUserId);
        if (userInfo == null)
        {
            throw new ArgumentException($"User with id {request.AppUserId} was not found");
        }

        var settings = await _crmUserSettingsRepository.GetAppUserSettingsWithFullInfo(request.AppUserId);
        if (settings == null && Role.IsAdminRole(userInfo.Role))
        {
            return new();
        }
        else if (settings == null && userInfo.Role == Role.DirectManager)
        {
            var ownerSettings = await _crmUserSettingsRepository.GetOwnerAppUserSettingsWithFullInfo();
            var cityInfoOwner = ownerSettings.NpCity == null ? null : _mapper.Map<GetNpCityDto>(ownerSettings.NpCity);
            var warehouseOwner = ownerSettings.NpWarehouse == null ? null : _mapper.Map<NpWarehouseDto>(ownerSettings.NpWarehouse);
            return new() { Id = ownerSettings.Id, NpCity = cityInfoOwner, NpWarehouse = warehouseOwner };
        }

        var cityInfo = settings.NpCity == null ? null : _mapper.Map<GetNpCityDto>(settings.NpCity);
        var warehouse = settings.NpWarehouse == null ? null : _mapper.Map<NpWarehouseDto>(settings.NpWarehouse);

        return new() { Id = settings.Id, NpCity = cityInfo, NpWarehouse = warehouse };
    }
}
