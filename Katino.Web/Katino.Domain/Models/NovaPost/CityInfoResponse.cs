namespace Katino.Domain.Models.NovaPost;

public class CityInfoResponse
{
    public int TotalCount { get; set; }
    public List<Address> Addresses { get; set; }
}

public class Address
{
    public string DeliveryCity { get; set; }
}