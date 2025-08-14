using AutoMapper;
using Katino.Domain.Entities;
using Katino.Domain.Repositories.CategoryRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.CategoryN.UpdateCategory;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, bool>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public UpdateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _logger = loggerFactory?.CreateLogger(nameof(UpdateCategoryCommandHandler));
    }

    public async Task<bool> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling update category request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var category = _mapper.Map<Category>(request.Category);

            await _categoryRepository.Update(category);
            await _categoryRepository.Save();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during updating category");
            return false;
        }
    }
}
