using Katino.Domain.Models;

namespace Katino.Domain.Services.OrderItemN.SewingQueueService;

public interface ISewingQueueService
{
    Task<List<SewingQueueItem>> GetSewingQueueAsync(CancellationToken ct = default);
}
