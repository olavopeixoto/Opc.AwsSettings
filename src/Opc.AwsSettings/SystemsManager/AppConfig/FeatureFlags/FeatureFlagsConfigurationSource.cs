using Amazon.AppConfigData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Opc.AwsSettings.SystemsManager.AppConfig.FeatureFlags
{
    public class FeatureFlagsConfigurationSource : IConfigurationSource
    {
        private readonly ILogger? _logger;

        public FeatureFlagsConfigurationSource(FeatureFlagsConfigurationProviderOptions? options = null,
            ILogger? logger = null)
        {
            _logger = logger;
            Options = options ?? new FeatureFlagsConfigurationProviderOptions();
        }

        public FeatureFlagsConfigurationProviderOptions Options { get; }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            var client = CreateClient();
            
            return new FeatureFlagsConfigurationProvider(client, Options, _logger);
        }

        private IAmazonAppConfigData CreateClient()
        {
            return Options.AwsOptions.CreateServiceClient<IAmazonAppConfigData>();
        }
    }
}