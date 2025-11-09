using Katino.Domain.Entities;
using Katino.Domain.Models;
using Katino.Domain.Models.NovaPost;

namespace Katino.Domain.Services.OrderN.AddOrderService;

public interface IAddOrderService
{
    Task<OrderCreationResult> AddAsync(Order order, CreateNovaPostInternetDocument document);
}
