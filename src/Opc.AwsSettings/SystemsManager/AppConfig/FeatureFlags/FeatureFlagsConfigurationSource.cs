using Amazon.Extensions.Configuration.SystemsManager;
using Amazon.Extensions.NETCore.Setup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Opc.AwsSettings.SystemsManager.AppConfig.FeatureFlags
{
    public class FeatureFlagsConfigurationSource : ISystemsManagerConfigurationSource
    {
        private readonly ILogger? _logger;

        public FeatureFlagsConfigurationSource(ILogger? logger = null)
        {
            _logger = logger;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder) => new SystemsManagerConfigurationProvider(this, new FeatureFlagsProcessor(this, _logger));

        /// <summary>
        /// The time that should be waited before refreshing the Feature Flags.
        /// If null, Feature Flags will not be refreshed.
        /// </summary>
        /// <example>
        /// <code>
        /// ReloadAfter = TimeSpan.FromMinutes(15);
        /// </code>
        /// </example>
        public TimeSpan? ReloadAfter { get; set; }
        
        public string EnvironmentIdentifier { get; set; }

        public string? ApplicationIdentifier { get; set; }

        public string ConfigurationProfileIdentifier { get; set; }

        /// <summary>
        /// AwsOptions
        /// </summary>
        public AWSOptions? AwsOptions { get; set; }

        public bool Optional { get; set; }

        public Action<SystemsManagerExceptionContext> OnLoadException { get; set; }
    }
}