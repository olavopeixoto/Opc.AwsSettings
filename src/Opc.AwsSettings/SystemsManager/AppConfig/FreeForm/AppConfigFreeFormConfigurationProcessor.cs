using System.Net.Mime;
using Amazon.AppConfig;
using Amazon.AppConfig.Model;
using Amazon.Extensions.Configuration.SystemsManager.Internal;
using Microsoft.Extensions.Logging;

namespace Opc.AwsSettings.SystemsManager.AppConfig.FreeForm;

public class AppConfigFreeFormConfigurationProcessor : ISystemsManagerProcessor
{
    private readonly ILogger? _logger;
    protected readonly AppConfigFreeFormConfigurationSource Source;

    public AppConfigFreeFormConfigurationProcessor(AppConfigFreeFormConfigurationSource source, ILogger? logger)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));

        if (source.ApplicationId is null)
            throw new ArgumentNullException(nameof(source.ApplicationId));

        if (source.ConfigProfileId is null)
            throw new ArgumentNullException(nameof(source.ConfigProfileId));

        if (source.EnvironmentId is null)
            throw new ArgumentNullException(nameof(source.EnvironmentId));

        if (source.ClientId is null)
            throw new ArgumentNullException(nameof(source.ClientId));

        if (source.AwsOptions is null)
            throw new ArgumentNullException(nameof(source.AwsOptions));
        
        _logger = logger;
    }

    private string LastConfigVersion { get; set; }

    private IDictionary<string, string> LastConfig { get; set; } = new Dictionary<string, string>();

    public async Task<IDictionary<string, string>> GetDataAsync()
    {
        var response = await FetchConfigurationAsync(CancellationToken.None);

        if (response is not null)
        {
            LastConfigVersion = response.ConfigurationVersion;
            LastConfig = ParseConfig(response.Configuration);
        }

        return LastConfig;
    }

    protected virtual async ValueTask<ConfigurationResponse?> FetchConfigurationAsync(CancellationToken cancellationToken)
    {
        var configurationRequest = new GetConfigurationRequest
        {
            Application = Source.ApplicationId,
            Environment = Source.EnvironmentId,
            Configuration = Source.ConfigProfileId,
            ClientId = Source.ClientId,
            ClientConfigurationVersion = LastConfigVersion
        };
    
        using var client = Source.AwsOptions.CreateServiceClient<IAmazonAppConfig>();
    
        if (client is AmazonAppConfigClient amazonAppConfigClient)
            amazonAppConfigClient.BeforeRequestEvent += ServiceClientAppender.ServiceClientBeforeRequestEvent;
    
        var response = await client.GetConfigurationAsync(configurationRequest, cancellationToken).ConfigureAwait(false);

        if (response.ContentType == MediaTypeNames.Application.Json)
            throw new NotImplementedException("Not implemented AppConfig type: " + response.ContentType);

        return response.ContentLength > 0L ? new ConfigurationResponse(response.ConfigurationVersion, response.Content) : null;
    }

    private static IDictionary<string, string> ParseConfig(Stream configStream)
    {
        return JsonConfigurationParser.Parse(configStream);
    }

    protected record ConfigurationResponse(string ConfigurationVersion, Stream Configuration);
}