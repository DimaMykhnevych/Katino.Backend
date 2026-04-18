using Katino.Domain.Entities;

namespace Katino.Domain.Services.OrderN.UrgentOrderRedistributionService;

public interface IUrgentOrderRedistributionService
{
    Task RedistributeForUrgentOrderAsync(Order urgentOrder);
}
