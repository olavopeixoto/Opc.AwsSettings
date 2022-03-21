using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Opc.AwsSettings.SecretsManager;

public static class SecretsManagerExtensions
{
    public static IConfigurationBuilder AddSecretsManager(this IConfigurationBuilder configurationBuilder,
        Action<SecretsManagerConfigurationProviderOptions>? configurator = null, ILogger? logger = null)
    {
        var options = new SecretsManagerConfigurationProviderOptions
        {
            AwsOptions = configurationBuilder.Build().GetAWSOptions()
        };

        configurator?.Invoke(options);

        var source = new SecretsManagerConfigurationSource(options, logger);
        
        configurationBuilder.Add(source);

        return configurationBuilder;
    }
}