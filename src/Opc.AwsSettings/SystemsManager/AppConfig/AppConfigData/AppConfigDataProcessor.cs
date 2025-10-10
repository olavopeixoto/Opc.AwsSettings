using System.Text.RegularExpressions;
using Amazon.AppConfigData;
using Amazon.AppConfigData.Model;
using Amazon.Extensions.Configuration.SystemsManager.Internal;
using Microsoft.Extensions.Logging;

namespace Opc.AwsSettings.SystemsManager.AppConfig.AppConfigData;

internal class AppConfigDataProcessor : ISystemsManagerProcessor
{
    private const string FeatureManagementKeyRoot = "FeatureManagement";

    private static readonly Regex FeatureManagementKeyRegex = new("^[^:]+:enabled$|^[^:]+:enabledFor(:.*)?$",
        RegexOptions.Compiled & RegexOptions.IgnoreCase & RegexOptions.Singleline);

    private readonly ILogger? _logger;
    protected readonly AppConfigDataSource Source;
    private string? _configurationToken;

    private IDictionary<string, string> _lastConfiguration = new Dictionary<string, string>();
    private DateTime? _nextPollConfigurationTokenExpirationTime;

    public AppConfigDataProcessor(AppConfigDataSource source, ILogger? logger = null)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));

        if (source.ApplicationIdentifier is null)
            throw new ArgumentNullException(nameof(source.ApplicationIdentifier));

        if (source.ConfigurationProfileIdentifier is null)
            throw new ArgumentNullException(nameof(source.ConfigurationProfileIdentifier));

        if (source.EnvironmentIdentifier is null)
            throw new ArgumentNullException(nameof(source.EnvironmentIdentifier));

        if (source.AwsOptions is null)
            throw new ArgumentNullException(nameof(source.AwsOptions));

        _logger = logger;
    }

    public async Task<IDictionary<string, string>> GetDataAsync()
    {
        var responseStream = await FetchConfigurationAsync(CancellationToken.None);

        if (responseStream == null) return _lastConfiguration;

        var parsedConfiguration = ParseData(responseStream);

        if (parsedConfiguration.Any()) _lastConfiguration = parsedConfiguration;

        return _lastConfiguration;
    }

    protected virtual async ValueTask<Stream?> FetchConfigurationAsync(CancellationToken cancellationToken)
    {
        using var client = Source.AwsOptions?.CreateServiceClient<IAmazonAppConfigData>() ??
                           throw new ArgumentNullException(nameof(Source.AwsOptions));

        if (client is AmazonAppConfigDataClient amazonAppConfigClient)
            amazonAppConfigClient.BeforeRequestEvent += ServiceClientAppender.ServiceClientBeforeRequestEvent;

        if (_configurationToken is null || DateTime.UtcNow >= _nextPollConfigurationTokenExpirationTime)
        {
            var startConfigurationSessionResponse = await client.StartConfigurationSessionAsync(
                new StartConfigurationSessionRequest
                {
                    ApplicationIdentifier = Source.ApplicationIdentifier,
                    EnvironmentIdentifier = Source.EnvironmentIdentifier,
                    ConfigurationProfileIdentifier = Source.ConfigurationProfileIdentifier
                }, cancellationToken).ConfigureAwait(false);

            _configurationToken = startConfigurationSessionResponse.InitialConfigurationToken;
        }

        var getLatestConfigurationResponse = await client.GetLatestConfigurationAsync(new GetLatestConfigurationRequest
        {
            ConfigurationToken = _configurationToken
        }, cancellationToken).ConfigureAwait(false);

        _configurationToken = getLatestConfigurationResponse.NextPollConfigurationToken;

        // Token will expire if not refreshed within 24 hours, so keep track of
        // the expected expiration time minus a bit of padding
        _nextPollConfigurationTokenExpirationTime = DateTime.UtcNow.Add(new TimeSpan(23, 59, 0));

        // 'Configuration' in the response will only be populated the first time we
        // call GetLatestConfiguration or if the config contents have changed since
        // the last time we called. So if it's empty we know we already have the latest
        // config, otherwise we need to update our cache.
        return getLatestConfigurationResponse.ContentLength > 0L ? getLatestConfigurationResponse.Configuration : null;
    }

    private IDictionary<string, string> ParseData(Stream responseStream)
    {
        var tempData = JsonConfigurationParser.Parse(responseStream);

        if (tempData is null) return new Dictionary<string, string>();

        _logger?.ParametersLoaded(tempData.GroupBy(x => x.Key.Split(':')[0]).Count());
        _logger?.ParametersValueLoaded(tempData);

        return tempData
            .Where(x => FeatureManagementKeyRegex.IsMatch(x.Key))
            .Select(x => new KeyValuePair<string, string>($"{FeatureManagementKeyRoot}:{ParseFeatureFlagKey(x.Key)}", x.Value))
            .Union(tempData)
            .ToDictionary(x => new string(KeyToTitleCase(x.Key).ToArray()), x => x.Value);
    }

    private static IEnumerable<char> KeyToTitleCase(string s)
    {
        var newWord = true;
        foreach (var c in s)
        {
            if (newWord)
            {
                yield return char.ToUpper(c);
                newWord = false;
            }
            else
            {
                yield return c;
            }

            if (c is ' ' or ':') newWord = true;
        }
    }

    private static string ParseFeatureFlagKey(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? value
            : value.Replace(":enabled", string.Empty);
    }
}
