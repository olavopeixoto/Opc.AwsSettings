using Amazon.SecretsManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Opc.AwsSettings.SecretsManager
{
    public class SecretsManagerConfigurationSource : IConfigurationSource
    {
        private readonly ILogger? _logger;

        public SecretsManagerConfigurationSource(SecretsManagerConfigurationProviderOptions? options = null, ILogger? logger = null)
        {
            _logger = logger;
            Options = options ?? new SecretsManagerConfigurationProviderOptions();
        }

        public SecretsManagerConfigurationProviderOptions Options { get; }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            var client = CreateClient();
            
            return new SecretsManagerConfigurationProvider(client, Options, _logger);
        }

        private IAmazonSecretsManager CreateClient()
        {
            var client = Options.AwsOptions.CreateServiceClient<IAmazonSecretsManager>();
            
            Options.ConfigureSecretsManagerConfig(client.Config as AmazonSecretsManagerConfig ?? new AmazonSecretsManagerConfig());

            return client;
        }
    }
}