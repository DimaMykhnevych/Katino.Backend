using AutoMapper;
using Katino.Application.DTOs.NpContactPerson;
using Katino.Domain.Context;
using Katino.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Queries.NovaPost.GetNpContactPerson;

public class GetNpContactPersonQueryHandler : IRequestHandler<GetNpContactPersonQuery, IEnumerable<NpContactPersonDto>>
{
    private const int DefaultResultSize = 20;
    private readonly IKatinoDbContext _katinoDbContext;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public GetNpContactPersonQueryHandler(
        IKatinoDbContext katinoDbContext,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _katinoDbContext = katinoDbContext;
        _logger = loggerFactory?.CreateLogger(nameof(GetNpContactPersonQueryHandler));
        _mapper = mapper;
    }

    public async Task<IEnumerable<NpContactPersonDto>> Handle(GetNpContactPersonQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get sizes");
        ArgumentNullException.ThrowIfNull(request);

        IQueryable<NpContactPerson> persons = _katinoDbContext.NpContactPersons.AsNoTracking().Take(DefaultResultSize);
        if (!string.IsNullOrEmpty(request.Phone))
        {
            persons = persons.Where(p => p.Phones.Contains(request.Phone));
        }

        var resultPersons = await persons.OrderBy(s => s.Id).ToListAsync(cancellationToken);
        List<NpContactPersonDto> personDtos =
            _mapper.Map<IEnumerable<NpContactPersonDto>>(resultPersons)
                .ToList();

        return personDtos;
    }
}
