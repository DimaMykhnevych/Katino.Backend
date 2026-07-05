using Katino.Domain.Models;

namespace Katino.Domain.Services.ProductVariantRedistributionN.IncomingReturnQueueService;

public interface IIncomingReturnQueueService
{
    Task<Dictionary<DateTime, List<SewingQueueItem>>> GetIncomingReturnQueueGroupedByDateAsync(CancellationToken ct = default);
}
