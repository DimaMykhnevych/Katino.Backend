using Katino.Application.DTOs.Size;
using MediatR;

namespace Katino.Application.Queries.SizeN.GetSizes;

public class GetSizesQuery : IRequest<GetSizeDto>
{
    public string Name { get; set; }
}
