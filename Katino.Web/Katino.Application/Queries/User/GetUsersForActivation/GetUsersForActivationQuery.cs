using Katino.Application.DTOs.User;
using MediatR;

namespace Katino.Application.Queries.User.GetUsersForActivation;

public class GetUsersForActivationQuery : IRequest<IEnumerable<ManageableUserDto>>
{
    public string CallerRole { get; set; }
}
