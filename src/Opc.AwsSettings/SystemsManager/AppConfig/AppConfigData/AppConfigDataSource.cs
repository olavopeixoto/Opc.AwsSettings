using Amazon.Extensions.Configuration.SystemsManager;
using Amazon.Extensions.Configuration.SystemsManager.Internal;
using Amazon.Extensions.NETCore.Setup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Opc.AwsSettings.SystemsManager.AppConfig.Lambda;

namespace Opc.AwsSettings.SystemsManager.AppConfig.AppConfigData;

internal sealed class AppConfigDataSource(ILogger? logger = null, bool useLambdaCacheLayer = false)
    : ISystemsManagerConfigurationSource
{
    public string? EnvironmentIdentifier { get; set; }

    public string? ApplicationIdentifier { get; set; }

    public string? ConfigurationProfileIdentifier { get; set; }

    /// <summary>
    ///     AwsOptions
    /// </summary>
    public AWSOptions? AwsOptions { get; set; }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new SystemsManagerConfigurationProvider(this, GetProcessor());
    }

    /// <summary>
    ///     The time that should be waited before refreshing the Feature Flags.
    ///     If null, Feature Flags will not be refreshed.
    /// </summary>
    /// <example>
    /// <code>
    /// ReloadAfter = TimeSpan.FromMinutes(15);
    /// </code>
    /// </example>
    public TimeSpan? ReloadAfter { get; set; }

    public bool Optional { get; set; }

    public Action<SystemsManagerExceptionContext>? OnLoadException { get; set; }

    private ISystemsManagerProcessor GetProcessor()
    {
        return useLambdaCacheLayer
            ? new LambdaCacheAppConfigFreeFormConfigurationProcessor(this, logger)
            : new AppConfigDataProcessor(this, logger);
    }
}
