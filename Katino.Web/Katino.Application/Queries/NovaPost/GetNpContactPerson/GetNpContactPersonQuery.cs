using Katino.Application.DTOs.NpContactPerson;
using MediatR;

namespace Katino.Application.Queries.NovaPost.GetNpContactPerson;

public class GetNpContactPersonQuery : IRequest<IEnumerable<NpContactPersonDto>>
{
    public string Phone { get; set; }
}
