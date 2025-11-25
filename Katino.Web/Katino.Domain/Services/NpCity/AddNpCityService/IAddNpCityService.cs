using Katino.Domain.Entities;

namespace Katino.Domain.Services.NpCityN.AddNpCityService;

public interface IAddNpCityService
{
    Task<Guid> UpsertNpCityAsync(NpCity npCity);
}
