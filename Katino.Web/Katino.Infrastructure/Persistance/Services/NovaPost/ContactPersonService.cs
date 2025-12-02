using Katino.Domain.Models.NovaPost;
using Katino.Domain.Options;
using Katino.Domain.Services.NovaPost.ContactPerson;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Katino.Infrastructure.Persistance.Services.NovaPost;

public class ContactPersonService : BaseNpApiService, IContactPersonService
{
    private readonly HttpClient _httpClient;
    private readonly NovaPostOptions _novaPostOptions;
    private readonly ILogger _logger;

    public ContactPersonService(
        HttpClient httpClient,
        IOptions<NovaPostOptions> options,
        ILoggerFactory loggerFactory) : base(httpClient, options, loggerFactory)
    {
        _httpClient = httpClient;
        _novaPostOptions = options.Value;
        _logger = loggerFactory?.CreateLogger(nameof(ContactPersonService));
    }

    public async Task<IEnumerable<NpContactPersonResponse>> GetNpSenderContactPersons()
    {
        var senderCounterpartyRef = await GetSenderCounterpartyRef();

        NpApiRequest<object> getSenderContactPersonsRequest = new()
        {
            ApiKey = _novaPostOptions.ApiKey,
            ModelName = "CounterpartyGeneral",
            CalledMethod = "getCounterpartyContactPersons",
            MethodProperties = new
            {
                Ref = senderCounterpartyRef,
                Page = "1"
            }
        };

        var responseString = await GetProcessedStringResponse(getSenderContactPersonsRequest);
        var senderContactResponse = JsonConvert.DeserializeObject<NpApiResponse<ContactPersonsResponse>>(responseString);

        _logger.LogDebug("GetSenderContactPerson response: {Response}", responseString);

        CheckApiResponse(senderContactResponse);

        return senderContactResponse.Data.Select(s => new NpContactPersonResponse()
        {
            Ref = s.Ref,
            CounterpartyRef = senderCounterpartyRef,
            LastName = s.LastName,
            FirstName = s.FirstName,
            MiddleName = s.MiddleName,
            Phones = s.Phones,
        });
    }

    public async Task<SaveCounterpartyGeneralResponse> SaveRecipientCounterparty(
        string firstName,
        string middleName,
        string lastName,
        string phone)
    {
        NpApiRequest<object> saveRecipientCounterpartyRequest = new()
        {
            ApiKey = _novaPostOptions.ApiKey,
            ModelName = "CounterpartyGeneral",
            CalledMethod = "save",
            MethodProperties = new
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                Phone = phone,
                Email = string.Empty,
                CounterpartyType = "PrivatePerson",
                CounterpartyProperty = "Recipient",
            }
        };

        var responseString = await GetProcessedStringResponse(saveRecipientCounterpartyRequest);

        var saveRecipientCounterpartyResponse = JsonConvert.DeserializeObject<NpApiResponse<SaveCounterpartyGeneralResponse>>(responseString);

        _logger.LogDebug("SaveRecipientCounterparty response: {Response}", responseString);

        CheckApiResponse(saveRecipientCounterpartyResponse);

        return saveRecipientCounterpartyResponse.Data.FirstOrDefault();
    }

    private async Task<string> GetSenderCounterpartyRef()
    {
        NpApiRequest<object> getSenderCounterpartyRefRequest = new()
        {
            ApiKey = _novaPostOptions.ApiKey,
            ModelName = "CounterpartyGeneral",
            CalledMethod = "getCounterparties",
            MethodProperties = new
            {
                CounterpartyProperty = "Sender",
                Page = "1"
            }
        };

        var responseString = await GetProcessedStringResponse(getSenderCounterpartyRefRequest);
        var senderResponse = JsonConvert.DeserializeObject<NpApiResponse<SenderCounterpartyResponse>>(responseString);

        _logger.LogDebug("GetSenderCounterpartyRef response: {Response}", responseString);

        CheckApiResponse(senderResponse);

        var result = senderResponse.Data.FirstOrDefault().Ref;

        return result;
    }
}
