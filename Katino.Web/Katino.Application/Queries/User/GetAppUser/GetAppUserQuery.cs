using Katino.Application.DTOs;
using MediatR;

namespace Katino.Application.Queries.User.GetAppUser;

public class GetAppUserQuery : IRequest<IEnumerable<UserAuthInfoDto>>
{
    public Guid? UserId { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
}