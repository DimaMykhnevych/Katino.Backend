using AutoMapper;
using Katino.Application.DTOs.NovaPost;
using Katino.Domain.Services.NovaPost.ContactPerson;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Queries.NovaPost.GetNpSenderContactPersons;

public class GetNpSenderContactPersonsQueryHandler : IRequestHandler<GetNpSenderContactPersonsQuery, IEnumerable<NpContactPersonResponseDto>>
{
    private readonly ILogger _logger;
    private readonly IMapper _mapper;
    private readonly IContactPersonService _contactPersonService;

    public GetNpSenderContactPersonsQueryHandler(
        IContactPersonService contactPersonService,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _logger = loggerFactory?.CreateLogger(nameof(GetNpSenderContactPersonsQueryHandler));
        _mapper = mapper;
        _contactPersonService = contactPersonService;
    }

    public async Task<IEnumerable<NpContactPersonResponseDto>> Handle(GetNpSenderContactPersonsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get NP sender contact persons query");
        ArgumentNullException.ThrowIfNull(request);

        var contactPersons = await _contactPersonService.GetNpSenderContactPersons();
        return _mapper.Map<IEnumerable<NpContactPersonResponseDto>>(contactPersons);
    }
}
