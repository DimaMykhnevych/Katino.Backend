using Katino.Domain.Entities;
using Katino.Domain.Repositories.NpOptionsSeatRepository;
using Katino.Domain.Services.NpOptionsSeatN.AddNpOptionsSeatService;
using Microsoft.Extensions.Logging;

namespace Katino.Infrastructure.Persistance.Services.NpOptionsSeatN;

public class AddNpOptionsSeatService : IAddNpOptionsSeatService
{
    private readonly INpOptionsSeatRepository _npOptionsSeatRepository;
    private readonly ILogger _logger;

    public AddNpOptionsSeatService(
        INpOptionsSeatRepository npOptionsSeatRepository,
        ILoggerFactory loggerFactory)
    {
        _npOptionsSeatRepository = npOptionsSeatRepository;
        _logger = loggerFactory?.CreateLogger(nameof(AddNpOptionsSeatService));
    }

    public async Task<Guid> GetOrCreateNpOptionsSeat(NpOptionsSeat npOptionsSeat)
    {
        var existingNpOptionsSeat = await _npOptionsSeatRepository
            .GetBySeatParamsAsync(npOptionsSeat);
        if (existingNpOptionsSeat != null)
        {
            _logger.LogDebug($"NpOptionsSeat with passed params already exists, id: {existingNpOptionsSeat.Id}");
            return existingNpOptionsSeat.Id;
        }

        _logger.LogDebug($"NpOptionsSeat ({npOptionsSeat.VolumetricHeight}, {npOptionsSeat.VolumetricWidth}, {npOptionsSeat.VolumetricHeight}) doesn't exist, adding...");

        var addedNpOptionsSeat = await _npOptionsSeatRepository.Insert(npOptionsSeat);
        await _npOptionsSeatRepository.Save();
        return addedNpOptionsSeat.Id;
    }
}
