using Amazon.Extensions.Configuration.SystemsManager;
using Amazon.Extensions.Configuration.SystemsManager.Internal;
using Amazon.Extensions.NETCore.Setup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Opc.AwsSettings.SystemsManager.AppConfig.Lambda;

namespace Opc.AwsSettings.SystemsManager.AppConfig.FreeForm;

public class AppConfigFreeFormConfigurationSource : ISystemsManagerConfigurationSource
{
    private readonly ILogger? _logger;
    private readonly bool _useLambdaCacheLayer;

    public AppConfigFreeFormConfigurationSource(ILogger? logger, bool useLambdaCacheLayer = false)
    {
        _logger = logger;
        _useLambdaCacheLayer = useLambdaCacheLayer;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder) => new SystemsManagerConfigurationProvider(this, GetProcessor());

    private ISystemsManagerProcessor GetProcessor()
    {
        return _useLambdaCacheLayer
            ? new LambdaCacheAppConfigFreeFormConfigurationProcessor(this, _logger)
            : new AppConfigFreeFormConfigurationProcessor(this, _logger);
    }

    public string? EnvironmentId { get; set; }

    public string? ApplicationId { get; set; }

    public string? ConfigProfileId { get; set; }

    public string? ClientId { get; set; }

    public AWSOptions? AwsOptions { get; set; }

    public bool Optional { get; set; }
    
    public TimeSpan? ReloadAfter { get; set; }

    public Action<SystemsManagerExceptionContext>? OnLoadException { get; set; }
}