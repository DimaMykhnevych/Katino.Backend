using Katino.Domain.Models.NovaPost;

namespace Katino.Domain.Services.NovaPost.ContactPerson;

public interface IContactPersonService
{
    Task<IEnumerable<NpContactPersonResponse>> GetNpSenderContactPersons();
}
