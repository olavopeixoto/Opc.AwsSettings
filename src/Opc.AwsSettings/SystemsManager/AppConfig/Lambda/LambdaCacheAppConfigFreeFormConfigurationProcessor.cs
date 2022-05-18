using System.Net.Mime;
using System.Web;
using Microsoft.Extensions.Logging;
using Opc.AwsSettings.SystemsManager.AppConfig.FreeForm;

namespace Opc.AwsSettings.SystemsManager.AppConfig.Lambda;

public class LambdaCacheAppConfigFreeFormConfigurationProcessor : AppConfigFreeFormConfigurationProcessor
{
    private static readonly HttpClient HttpClient = new();
    private readonly int _port;

    public LambdaCacheAppConfigFreeFormConfigurationProcessor(AppConfigFreeFormConfigurationSource source, ILogger? logger) : base(source, logger)
    {
        if (!int.TryParse(Environment.GetEnvironmentVariable("AWS_APPCONFIG_EXTENSION_HTTP_PORT"), out _port))
        {
            _port = 2772;
        }
    }

    protected override async ValueTask<ConfigurationResponse?> FetchConfigurationAsync(CancellationToken cancellationToken)
    {
        var url = $"http://localhost:{_port}/applications/{HttpUtility.UrlEncode(Source.ApplicationId)}/environments/{HttpUtility.UrlEncode(Source.EnvironmentId)}/configurations/{HttpUtility.UrlEncode(Source.ConfigProfileId)}";

        var response = await HttpClient.GetAsync(url, cancellationToken);

        if (response.Content.Headers.ContentType?.MediaType != MediaTypeNames.Application.Json)
            throw new NotImplementedException("Not implemented AppConfig type: " + response.Content.Headers.ContentType?.MediaType);

        return response.Content.Headers.ContentLength > 0L ? new ConfigurationResponse("LambdaCache", await response.Content.ReadAsStreamAsync(cancellationToken)) : null;
    }
}