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

        List<Collection> collections = await _katinoDbContext.Collections
            .Include(c => c.ProductCollections)
                .ThenInclude(pc => pc.Product)
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<CollectionDto>>(collections);
    }
}
