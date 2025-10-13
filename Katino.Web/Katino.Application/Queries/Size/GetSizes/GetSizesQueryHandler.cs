using AutoMapper;
using Katino.Application.DTOs.Size;
using Katino.Domain.Context;
using Katino.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Queries.SizeN.GetSizes;

public class GetSizesQueryHandler : IRequestHandler<GetSizesQuery, GetSizeDto>
{
    private readonly IKatinoDbContext _katinoDbContext;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public GetSizesQueryHandler(
        IKatinoDbContext katinoDbContext,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _katinoDbContext = katinoDbContext;
        _logger = loggerFactory?.CreateLogger(nameof(GetSizesQueryHandler));
        _mapper = mapper;
    }

    public async Task<GetSizeDto> Handle(GetSizesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get sizes");
        ArgumentNullException.ThrowIfNull(request);

        IQueryable<Size> sizes = _katinoDbContext.Sizes.AsNoTracking();
        if (!string.IsNullOrEmpty(request.Name))
        {
            sizes = sizes.Where(p => p.Name.Contains(request.Name));
        }

        var resultSizes = await sizes.OrderBy(s => s.Name).ToListAsync(cancellationToken);
        List<SizeDto> sizeDtos =
            _mapper.Map<IEnumerable<SizeDto>>(resultSizes)
                .ToList();

        return new GetSizeDto
        {
            Sizes = sizeDtos,
            ResultsAmount = sizeDtos.Count
        };
    }
}
