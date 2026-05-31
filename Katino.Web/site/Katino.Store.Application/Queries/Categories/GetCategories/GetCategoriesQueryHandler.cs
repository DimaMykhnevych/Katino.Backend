using Katino.Domain.Context;
using Katino.Domain.Enums;
using Katino.Store.Application.DTOs.Categories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Katino.Store.Application.Queries.Categories.GetCategories;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, IEnumerable<CategoryListItemDto>>
{
    private readonly IKatinoDbContext _context;
    private readonly ILogger _logger;

    public GetCategoriesQueryHandler(IKatinoDbContext context, ILoggerFactory loggerFactory)
    {
        _context = context;
        _logger = loggerFactory.CreateLogger(nameof(GetCategoriesQueryHandler));
    }

    public async Task<IEnumerable<CategoryListItemDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get categories with product counts");

        return await _context.Categories
            .AsNoTracking()
            .Where(c => c.Products.Any(p => p.Variants.Any(v => v.DeletedAt == null && v.Status != ProductStatus.Discontinued)))
            .Select(c => new CategoryListItemDto
            {
                Id = c.Id,
                Name = c.Name,
                ProductCount = c.Products.Count(p => p.Variants.Any(v => v.DeletedAt == null && v.Status != ProductStatus.Discontinued))
            })
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }
}
