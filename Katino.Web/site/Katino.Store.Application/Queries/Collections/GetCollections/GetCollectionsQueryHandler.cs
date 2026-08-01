using Katino.Domain.Context;
using Katino.Domain.Enums;
using Katino.Store.Application.DTOs.Collections;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Katino.Store.Application.Queries.Collections.GetCollections;

public class GetCollectionsQueryHandler : IRequestHandler<GetCollectionsQuery, IEnumerable<CollectionListItemDto>>
{
    private readonly IKatinoDbContext _context;
    private readonly ILogger _logger;

    public GetCollectionsQueryHandler(IKatinoDbContext context, ILoggerFactory loggerFactory)
    {
        _context = context;
        _logger = loggerFactory.CreateLogger(nameof(GetCollectionsQueryHandler));
    }

    public async Task<IEnumerable<CollectionListItemDto>> Handle(GetCollectionsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get collections with product counts");

        return await _context.Collections
            .AsNoTracking()
            .Where(c => c.ProductCollections.Any(pc => pc.Product.Variants.Any(v => v.DeletedAt == null && v.Status != ProductStatus.Discontinued)))
            .Select(c => new CollectionListItemDto
            {
                Id = c.Id,
                Name = c.Name,
                ProductCount = c.ProductCollections.Count(pc => pc.Product.Variants.Any(v => v.DeletedAt == null && v.Status != ProductStatus.Discontinued))
            })
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }
}
