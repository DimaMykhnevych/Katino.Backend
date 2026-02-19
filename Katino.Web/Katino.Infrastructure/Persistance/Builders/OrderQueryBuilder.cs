using Katino.Domain.Builders;
using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Infrastructure.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Katino.Infrastructure.Persistance.Builders;

public class OrderQueryBuilder : IOrderQueryBuilder
{
    private const int DefaultPageSize = 20;
    private readonly KatinoDbContext _dbContext;
    private IQueryable<Order> _query;

    public OrderQueryBuilder(KatinoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<Order> Build()
    {
        IQueryable<Order> result = _query;
        _query = null;
        return result;
    }

    public IOrderQueryBuilder SetBaseOrderInfo()
    {
        _query = _dbContext.Orders
            .AsNoTracking()
            .AsSplitQuery()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                        .ThenInclude(p => p.Category)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                    .ThenInclude(pv => pv.Color)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                    .ThenInclude(pv => pv.Size)
            .Include(o => o.OrderNpOptionsSeats)
                .ThenInclude(o => o.NpOptionsSeat)
            .Include(o => o.SenderNpWarehouse)
            .Include(o => o.RecipientNpWarehouse)
            .Include(o => o.SenderNpCity)
            .Include(o => o.RecipientNpCity)
            .Include(o => o.SenderContactPerson)
            .Include(o => o.OrderRecipient)
                .ThenInclude(r => r.NpContactPerson)
            .Include(o => o.AddressInfo)
            .IgnoreQueryFilters()
            .OrderByDescending(o => o.CreationDateTime);

        return this;
    }

    public IOrderQueryBuilder SetBaseOrderInfoForToatalCount()
    {
        _query = _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.OrderRecipient)
                .ThenInclude(r => r.NpContactPerson);

        return this;
    }

    public IOrderQueryBuilder ApplyPaging(int page, int pageSize)
    {
        EnsureQuery();

        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? DefaultPageSize : pageSize;

        _query = _query!
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return this;
    }

    public IOrderQueryBuilder ApplySearch(string search)
    {
        EnsureQuery();

        if (string.IsNullOrWhiteSpace(search))
        {
            return this;
        }

        var term = search.Trim();
        var like = $"%{term.ToLower()}%";

        _query = _query!.Where(o =>
            // 1) InternetDocumentIntDocNumber
            (o.InternetDocumentIntDocNumber != null &&
             EF.Functions.Like(o.InternetDocumentIntDocNumber.ToLower(), like))

            ||

            // 2) OrderRecipient.InstUrl
            (o.OrderRecipient != null &&
             o.OrderRecipient.InstUrl != null &&
             EF.Functions.Like(o.OrderRecipient.InstUrl.ToLower(), like))

            ||

            // 3) OrderRecipient.NpContactPerson fields
            (o.OrderRecipient != null &&
             o.OrderRecipient.NpContactPerson != null &&
             (
                 (o.OrderRecipient.NpContactPerson.LastName != null &&
                  EF.Functions.Like(o.OrderRecipient.NpContactPerson.LastName.ToLower(), like))
                 ||
                 (o.OrderRecipient.NpContactPerson.FirstName != null &&
                  EF.Functions.Like(o.OrderRecipient.NpContactPerson.FirstName.ToLower(), like))
                 ||
                 (o.OrderRecipient.NpContactPerson.MiddleName != null &&
                  EF.Functions.Like(o.OrderRecipient.NpContactPerson.MiddleName.ToLower(), like))
                 ||
                 (o.OrderRecipient.NpContactPerson.Phones != null &&
                  EF.Functions.Like(o.OrderRecipient.NpContactPerson.Phones.ToLower(), like))
             ))
        );

        return this;
    }

    public IOrderQueryBuilder ApplyOrderStatusFilter(IList<OrderStatus> orderStatuses)
    {
        if (orderStatuses == null || !orderStatuses.Any())
        {
            return this;
        }

        _query = _query.Where(o => orderStatuses.Contains(o.OrderStatus));
        return this;
    }

    private void EnsureQuery()
    {
        _query ??= _dbContext.Orders.AsQueryable();
    }
}
