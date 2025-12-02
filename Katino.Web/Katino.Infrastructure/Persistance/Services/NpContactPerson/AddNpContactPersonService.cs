using Katino.Domain.Entities;
using Katino.Domain.Repositories.NpContactPersonRepository;
using Katino.Domain.Services.NpContactPersonN.AddNpContactPersonService;
using Microsoft.Extensions.Logging;

namespace Katino.Infrastructure.Persistance.Services.NpContactPersonN;

public class AddNpContactPersonService : IAddNpContactPersonService
{
    private readonly INpContactPersonRepository _npContactPersonRepository;
    private readonly ILogger _logger;

    public AddNpContactPersonService(
        INpContactPersonRepository npContactPersonRepository,
        ILoggerFactory loggerFactory)
    {
        _npContactPersonRepository = npContactPersonRepository;
        _logger = loggerFactory?.CreateLogger(nameof(AddNpContactPersonService));
    }

    public async Task<Guid> UpsertNpContactPersonAsync(NpContactPerson npContactPerson)
    {
        var existingNpContactPerson = await _npContactPersonRepository
            .GetNpContactPersonByRefsAsync(npContactPerson.CounterpartyRef, npContactPerson.Ref);
        if (existingNpContactPerson == null)
        {
            _logger.LogDebug($"NP contact person {npContactPerson.Ref} (counterparty {npContactPerson.CounterpartyRef}) doesn't exist in database, adding...");
            var addedNpContactPerson = await _npContactPersonRepository.Insert(npContactPerson);
            await _npContactPersonRepository.Save();
            return addedNpContactPerson.Id;
        }

        _logger.LogDebug($"NP contact person {npContactPerson.Ref} (counterparty {npContactPerson.CounterpartyRef})) exists in database, updating...");

        existingNpContactPerson.LastName = npContactPerson.LastName;
        existingNpContactPerson.FirstName = npContactPerson.FirstName;
        existingNpContactPerson.MiddleName = npContactPerson.MiddleName;
        existingNpContactPerson.Phones = npContactPerson.Phones;

        await _npContactPersonRepository.Update(existingNpContactPerson);
        await _npContactPersonRepository.Save();
        return existingNpContactPerson.Id;
    }
}
