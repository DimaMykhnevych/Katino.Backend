using Katino.Application.DTOs.NovaPost;
using MediatR;

namespace Katino.Application.Queries.NovaPost.GetNpSenderContactPersons;

public class GetNpSenderContactPersonsQuery : IRequest<IEnumerable<NpContactPersonResponseDto>>
{
}
