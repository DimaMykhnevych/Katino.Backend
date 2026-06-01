using Katino.Domain.Entities;
using Katino.Domain.Repositories.CollectionRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.CollectionN.DeleteCollection;

public class DeleteCollectionCommandHandler : IRequestHandler<DeleteCollectionCommand, bool>
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly ILogger _logger;

    public DeleteCollectionCommandHandler(
        ICollectionRepository collectionRepository,
        ILoggerFactory loggerFactory)
    {
        _collectionRepository = collectionRepository;
        _logger = loggerFactory?.CreateLogger(nameof(DeleteCollectionCommandHandler));
    }

    public async Task<bool> Handle(DeleteCollectionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling delete collection request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            Collection collection = await _collectionRepository.Get(request.Id);
            if (collection == null)
            {
                throw new ArgumentException($"Collection with id {request.Id} doesn't exist");
            }

            _collectionRepository.Delete(collection);
            await _collectionRepository.Save();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during deleting collection");
            return false;
        }
    }
}
