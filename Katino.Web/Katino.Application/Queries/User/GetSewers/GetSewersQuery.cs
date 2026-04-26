using Katino.Application.DTOs.User;
using MediatR;

namespace Katino.Application.Queries.User.GetSewers;

public class GetSewersQuery : IRequest<List<SewerDto>>
{
}
