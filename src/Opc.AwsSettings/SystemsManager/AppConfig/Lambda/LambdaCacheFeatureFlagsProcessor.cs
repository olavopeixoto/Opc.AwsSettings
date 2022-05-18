using System.Net.Mime;
using System.Web;
using Microsoft.Extensions.Logging;
using Opc.AwsSettings.SystemsManager.AppConfig.FeatureFlags;

namespace Opc.AwsSettings.SystemsManager.AppConfig.Lambda;

public class LambdaCacheFeatureFlagsProcessor : FeatureFlagsProcessor
{
    private static readonly HttpClient HttpClient = new();
    private readonly int _port;

    public LambdaCacheFeatureFlagsProcessor(FeatureFlagsConfigurationSource source, ILogger? logger) : base(source, logger)
    {
        if (!int.TryParse(Environment.GetEnvironmentVariable("AWS_APPCONFIG_EXTENSION_HTTP_PORT"), out _port))
        {
            _port = 2772;
        }
    }

    protected override async ValueTask<Stream?> FetchConfigurationAsync(CancellationToken cancellationToken)
    {
        var url = $"http://localhost:{_port}/applications/{HttpUtility.UrlEncode(Source.ApplicationIdentifier)}/environments/{HttpUtility.UrlEncode(Source.EnvironmentIdentifier)}/configurations/{HttpUtility.UrlEncode(Source.ConfigurationProfileIdentifier)}";

        var response = await HttpClient.GetAsync(url, cancellationToken);

        if (response.Content.Headers.ContentType?.MediaType == MediaTypeNames.Application.Json)
            throw new NotImplementedException("Not implemented AppConfig type: " + response.Content.Headers.ContentType?.MediaType);

        return response.Content.Headers.ContentLength > 0L ? await response.Content.ReadAsStreamAsync(cancellationToken) : null;
    }
}