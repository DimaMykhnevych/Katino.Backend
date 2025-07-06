using Katino.Domain.Constants;
using Microsoft.Extensions.Configuration;

namespace Katino.Domain.Extensions;

public static class ConfigurationExtensions
{
    public static bool EmailConfirmationEnabled(this IConfiguration configuration)
    {
        return bool.TryParse(configuration[ConfigurationKeys.EmailConfirmationEnabled], out bool result) && result;
    }
}
