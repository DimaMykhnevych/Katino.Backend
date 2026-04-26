using Katino.Domain.Builders;
using Katino.Domain.Entities;
using Katino.Infrastructure.Persistance.Context;

namespace Katino.Infrastructure.Persistance.Builders;

public class AppUserQueryBuilder : IAppUserQueryBuilder
{
    private readonly KatinoDbContext _dbContext;
    private IQueryable<AppUser> _query;

    public AppUserQueryBuilder(KatinoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<AppUser> Build()
    {
        IQueryable<AppUser> result = _query;
        _query = null;
        return result;
    }


    public IAppUserQueryBuilder SetBaseUserInfo()
    {
        _query = _dbContext.AppUsers;
        return this;
    }

    public IAppUserQueryBuilder SetUserEmail(string userEmail)
    {
        if (!string.IsNullOrEmpty(userEmail))
        {
            _query = _query.Where(u => u.Email == userEmail);
        }
        return this;
    }

    public IAppUserQueryBuilder SetUserId(Guid? userId)
    {
        if (userId is not null)
        {
            _query = _query.Where(u => u.Id == userId);
        }
        return this;
    }

    public IAppUserQueryBuilder SetUserName(string userName)
    {
        if (!string.IsNullOrEmpty(userName))
        {
            _query = _query.Where(u => u.UserName == userName);
        }
        return this;
    }

    public IAppUserQueryBuilder SetRole(string role)
    {
        if (!string.IsNullOrEmpty(role))
        {
            _query = _query.Where(u => u.Role == role);
        }
        return this;
    }
}