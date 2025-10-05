using AutoMapper;
using Katino.Application.DTOs.Category;
using Katino.Domain.Entities;
using Katino.Domain.Repositories.CategoryRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.CategoryN.AddCategory;

public class AddCategoryCommandHandler : IRequestHandler<AddCategoryCommand, CategoryDto>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public AddCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    ILoggerFactory loggerFactory,
    IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _logger = loggerFactory?.CreateLogger(nameof(AddCategoryCommandHandler));
        _mapper = mapper;
    }

    public async Task<CategoryDto> Handle(AddCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling add product category request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            Category category = _mapper.Map<Category>(request);
            var addedCategory = await _categoryRepository.Insert(category);
            await _categoryRepository.Save();
            return _mapper.Map<CategoryDto>(addedCategory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during adding product category");
            return null;
        }
    }
}
