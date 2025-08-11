using AutoMapper;
using Katino.Application.DTOs.Category;
using Katino.Domain.Context;
using Katino.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Queries.CategoryN.GetCategories;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, GetCategoryDto>
{
    private readonly IKatinoDbContext _katinoDbContext;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public GetCategoriesQueryHandler(
        IKatinoDbContext katinoDbContext,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _katinoDbContext = katinoDbContext;
        _logger = loggerFactory?.CreateLogger(nameof(GetCategoriesQueryHandler));
        _mapper = mapper;
    }

    public async Task<GetCategoryDto> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get categories");
        ArgumentNullException.ThrowIfNull(request);

        IQueryable<Category> categories = _katinoDbContext.Categories.AsNoTracking();
        if (!string.IsNullOrEmpty(request.Name))
        {
            categories = categories.Where(p => p.Name.Contains(request.Name));
        }

        var resultCategories = await categories.ToListAsync();
        List<CategoryDto> categoryDtos =
            _mapper.Map<IEnumerable<CategoryDto>>(resultCategories)
                .ToList();

        return new GetCategoryDto
        {
            Categories = categoryDtos,
            ResultsAmount = categoryDtos.Count
        };
    }
}
