using Microsoft.Extensions.Configuration;

namespace Katino.Web.Options;

public class MySqlConfigOptions
{
    [ConfigurationKeyName("Default")]
    public string DefaultConnectionString { get; set; }
}