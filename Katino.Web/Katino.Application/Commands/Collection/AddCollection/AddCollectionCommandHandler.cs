using AutoMapper;
using Katino.Application.DTOs.Collection;
using Katino.Domain.Entities;
using Katino.Domain.Repositories.CollectionRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.CollectionN.AddCollection;

public class AddCollectionCommandHandler : IRequestHandler<AddCollectionCommand, CollectionDto>
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public AddCollectionCommandHandler(
        ICollectionRepository collectionRepository,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _collectionRepository = collectionRepository;
        _logger = loggerFactory?.CreateLogger(nameof(AddCollectionCommandHandler));
        _mapper = mapper;
    }

    public async Task<CollectionDto> Handle(AddCollectionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling add collection request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            Collection collection = _mapper.Map<Collection>(request);
            var addedCollection = await _collectionRepository.Insert(collection);
            await _collectionRepository.Save();
            return _mapper.Map<CollectionDto>(addedCollection);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during adding collection");
            return null;
        }
    }
}
