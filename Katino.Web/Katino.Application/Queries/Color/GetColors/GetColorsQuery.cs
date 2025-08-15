using Katino.Application.DTOs.Color;
using MediatR;

namespace Katino.Application.Queries.ColorN.GetColors;

public class GetColorsQuery : IRequest<GetColorDto>
{
    public string Name { get; set; }
}
