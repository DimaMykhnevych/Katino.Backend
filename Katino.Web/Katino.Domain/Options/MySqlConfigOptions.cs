using Microsoft.Extensions.Configuration;

namespace Katino.Domain.Options;

public class MySqlConfigOptions
{
    [ConfigurationKeyName("Default")]
    public string DefaultConnectionString { get; set; }
}