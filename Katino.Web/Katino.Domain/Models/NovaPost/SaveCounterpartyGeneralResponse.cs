namespace Katino.Domain.Models.NovaPost;

public class SaveCounterpartyGeneralResponse
{
    public string Ref {  get; set; }
    public NpApiResponse<SaveCounterPartyContactPersonResponse> ContactPerson { get; set; }
}


public class SaveCounterPartyContactPersonResponse
{
    public string Ref { get; set; }
}