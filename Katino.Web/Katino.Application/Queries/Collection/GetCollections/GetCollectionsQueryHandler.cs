using AutoMapper;
using Katino.Application.DTOs.Collection;
using Katino.Domain.Context;
using Katino.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Queries.CollectionN.GetCollections;

public class GetCollectionsQueryHandler : IRequestHandler<GetCollectionsQuery, List<CollectionDto>>
{
    private readonly IKatinoDbContext _katinoDbContext;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public GetCollectionsQueryHandler(
        IKatinoDbContext katinoDbContext,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _katinoDbContext = katinoDbContext;
        _logger = loggerFactory?.CreateLogger(nameof(GetCollectionsQueryHandler));
        _mapper = mapper;
    }

    public async Task<List<CollectionDto>> Handle(GetCollectionsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get collections");
        ArgumentNullException.ThrowIfNull(request);

        IQueryable<Collection> collections = _katinoDbContext.Collections
            .Include(c => c.ProductCollections)
                .ThenInclude(pc => pc.Product)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(request.Name))
        {
            collections = collections.Where(p => p.Name.Contains(request.Name));
        }

        var resultCollections = await collections
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<CollectionDto>>(resultCollections);
    }
}
