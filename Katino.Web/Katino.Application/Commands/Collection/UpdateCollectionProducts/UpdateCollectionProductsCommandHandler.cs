using AutoMapper;
using Katino.Application.DTOs.Collection;
using Katino.Domain.Context;
using Katino.Domain.Entities;
using Katino.Domain.Repositories.CollectionRepository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.CollectionN.UpdateCollectionProducts;

public class UpdateCollectionProductsCommandHandler : IRequestHandler<UpdateCollectionProductsCommand, CollectionDto>
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly IKatinoDbContext _katinoDbContext;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public UpdateCollectionProductsCommandHandler(
        ICollectionRepository collectionRepository,
        IKatinoDbContext katinoDbContext,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _collectionRepository = collectionRepository;
        _katinoDbContext = katinoDbContext;
        _logger = loggerFactory?.CreateLogger(nameof(UpdateCollectionProductsCommandHandler));
        _mapper = mapper;
    }

    public async Task<CollectionDto> Handle(UpdateCollectionProductsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling update collection products request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            Collection collection = await _collectionRepository.Get(request.CollectionId);
            if (collection == null)
            {
                _logger.LogWarning("Collection {CollectionId} not found", request.CollectionId);
                return null;
            }

            await _collectionRepository.DeleteProductsByCollectionId(request.CollectionId);

            var productCollections = request.ProductIds.Select(productId => new ProductCollection
            {
                CollectionId = request.CollectionId,
                ProductId = productId
            });

            await _collectionRepository.InsertProductCollections(productCollections);
            await _collectionRepository.Save();

            var updatedCollection = await _katinoDbContext.Collections
                .Include(c => c.ProductCollections)
                    .ThenInclude(pc => pc.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.CollectionId, cancellationToken);

            return _mapper.Map<CollectionDto>(updatedCollection);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during updating collection products");
            return null;
        }
    }
}
