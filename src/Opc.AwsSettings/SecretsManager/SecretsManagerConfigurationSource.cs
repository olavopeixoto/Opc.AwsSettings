using Amazon.SecretsManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Opc.AwsSettings.SecretsManager;

internal sealed class SecretsManagerConfigurationSource(
    SecretsManagerConfigurationProviderOptions? options = null,
    ILogger? logger = null)
    : IConfigurationSource
{
    private SecretsManagerConfigurationProviderOptions Options { get; } = options ?? new SecretsManagerConfigurationProviderOptions();

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        var client = CreateClient();

        return new SecretsManagerConfigurationProvider(client, Options, logger);
    }

    private IAmazonSecretsManager CreateClient()
    {
        if (Options.AwsOptions is null) throw new ArgumentNullException(nameof(Options.AwsOptions));

        var client = Options.AwsOptions.CreateServiceClient<IAmazonSecretsManager>();

        Options.ConfigureSecretsManagerConfig(client.Config as AmazonSecretsManagerConfig ??
                                              new AmazonSecretsManagerConfig());

        return client;
    }
}
