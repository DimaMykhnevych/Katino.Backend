using Katino.Domain.Entities;

namespace Katino.Domain.Builders;

public interface IAppUserQueryBuilder : IQueryBuilder<AppUser>
{
    IAppUserQueryBuilder SetBaseUserInfo();
    IAppUserQueryBuilder SetUserName(string userName);
    IAppUserQueryBuilder SetUserId(Guid? userId);
    IAppUserQueryBuilder SetUserEmail(string userEmail);
}
