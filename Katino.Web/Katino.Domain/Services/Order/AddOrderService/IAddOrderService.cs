using Katino.Domain.Entities;
using Katino.Domain.Models;

namespace Katino.Domain.Services.OrderN.AddOrderService;

public interface IAddOrderService
{
    Task<OrderCreationResult> AddAsync(Order order);
}
