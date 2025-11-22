namespace Katino.Domain.Services.NovaPost.Sync;

public interface INovaPoshtaSyncService
{
    Task<bool> IsSyncCompletedAsync();
    Task SyncAllDataAsync(Guid triggeredBy);
}
