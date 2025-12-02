using Katino.Domain.Models.NovaPost;

namespace Katino.Domain.Services.NovaPost.ContactPerson;

public interface IContactPersonService
{
    Task<IEnumerable<NpContactPersonResponse>> GetNpSenderContactPersons();

    Task<SaveCounterpartyGeneralResponse> SaveRecipientCounterparty(
        string firstName,
        string middleName,
        string lastName,
        string phone);
}
