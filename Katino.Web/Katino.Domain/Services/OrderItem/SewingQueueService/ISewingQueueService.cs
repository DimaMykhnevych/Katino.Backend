using Katino.Domain.Models;

namespace Katino.Domain.Services.OrderItemN.SewingQueueService;

public interface ISewingQueueService
{
    Task<List<SewingQueueItem>> GetSewingQueueAsync(Guid? sewerId = null, CancellationToken ct = default);
    Task<Dictionary<DateTime, List<SewingQueueItem>>> GetSewingQueueGroupedByDateAsync(Guid? sewerId = null, CancellationToken ct = default);
}
