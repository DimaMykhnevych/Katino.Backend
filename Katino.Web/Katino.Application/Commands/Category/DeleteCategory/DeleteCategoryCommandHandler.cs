using Katino.Domain.Entities;
using Katino.Domain.Repositories.CategoryRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.CategoryN.DeleteCategory;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, bool>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger _logger;

    public DeleteCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        ILoggerFactory loggerFactory)
    {
        _categoryRepository = categoryRepository;
        _logger = loggerFactory?.CreateLogger(nameof(DeleteCategoryCommandHandler));
    }

    public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling delete category request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            Category categoryFromDb = await _categoryRepository.Get(request.Id);
            if (categoryFromDb == null)
            {
                throw new ArgumentException($"Category with id {request.Id} doesn't exist");
            }

            _categoryRepository.Delete(categoryFromDb);
            await _categoryRepository.Save();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during deleting category");
            return false;
        }
    }
}
