using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Amazon.AppConfigData;
using Amazon.AppConfigData.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Opc.AwsSettings.Parsers;

namespace Opc.AwsSettings.SystemsManager.AppConfig.FeatureFlags;

public class FeatureFlagsConfigurationProvider : ConfigurationProvider, IDisposable
{
    private readonly ILogger? _logger;
    private const string FeatureManagementKeyRoot = "FeatureManagement";
    private string? _configurationToken;
    private DateTime? _nextPollConfigurationTokenExpirationTime;
    private string? _configurationJson;
        
    public IAmazonAppConfigData Client { get; }
    public FeatureFlagsConfigurationProviderOptions Options { get; }

    private Task? _pollingTask;
    private CancellationTokenSource? _cancellationToken;

    public FeatureFlagsConfigurationProvider(IAmazonAppConfigData client,
        FeatureFlagsConfigurationProviderOptions options, ILogger? logger = null)
    {
        _logger = logger;
        Client = client ?? throw new ArgumentNullException(nameof(client));
        Options = options;
    }

    public override void Load()
    {
        _cancellationToken ??= new CancellationTokenSource();

        LoadAsync(_cancellationToken.Token).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        var values = await FetchConfigurationAsync(cancellationToken).ConfigureAwait(false);

        SetData(values, triggerReload: false);

        if (Options.PollingInterval.HasValue)
        {
            _cancellationToken?.Cancel();
            _cancellationToken?.Dispose();
            _cancellationToken = new CancellationTokenSource();
            _pollingTask = PollForChangesAsync(Options.PollingInterval.Value, _cancellationToken.Token);
        }
    }

    private async Task PollForChangesAsync(TimeSpan interval, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
            try
            {
                await ReloadAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger?.ErrorPollingForChanges(ex);
            }
        }
    }

    private async Task ReloadAsync(CancellationToken cancellationToken)
    {
        var oldValues = _configurationJson;
        var newValues = await FetchConfigurationAsync(cancellationToken).ConfigureAwait(false);

        if (oldValues != newValues)
        {
            SetData(newValues, triggerReload: true);
        }
    }
        
    private void SetData(string? jsonString, bool triggerReload)
    {
        if (jsonString is null)
        {
            return;
        }

        try
        {
             var tempData = JsonConfigurationFileParser.Parse(new MemoryStream(Encoding.UTF8.GetBytes(jsonString)));

             _logger?.FeatureFlagsLoaded(tempData.GroupBy(x => x.Key.Split(':')[0]).Count());
             _logger?.FeatureFlagsValueLoaded(tempData);

             Data = tempData
                 .Where(x => FeatureManagementKeyRegex.IsMatch(x.Key))
                 .Select(x => KeyValuePair.Create($"{FeatureManagementKeyRoot}:{ParseFeatureFlagKey(x.Key)}", x.Value))
                 .Union(tempData)
                 .ToDictionary(x => new string(KeyToTitleCase(x.Key).ToArray()), x => x.Value);
        }
        catch (JsonException e)
        {
            throw new FormatException("Could not parse the JSON file.", e);
        }
        
        if (triggerReload)
        {
            OnReload();
        }
    }

    private static readonly Regex FeatureManagementKeyRegex = new Regex("^[^:]+:enabled$|^[^:]+:enabledFor(:.*)?$", RegexOptions.Compiled & RegexOptions.IgnoreCase & RegexOptions.Singleline);
    
    private IEnumerable<char> KeyToTitleCase(string s)
    {
        var newWord = true;
        foreach(var c in s)
        {
            if (newWord)
            {
                yield return char.ToUpper(c); newWord = false;
            }
            else
            {
                yield return c;
            }
            if(c is ' ' or ':') newWord = true;
        }
    }
    
    private static string ParseFeatureFlagKey(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? value : value.Replace(":enabled", string.Empty, StringComparison.InvariantCultureIgnoreCase);
    }

    private async Task<string?> FetchConfigurationAsync(CancellationToken cancellationToken)
    {
        if (_configurationToken is null || DateTime.UtcNow >= _nextPollConfigurationTokenExpirationTime)
        {
            var startConfigurationSessionResponse = await Client.StartConfigurationSessionAsync(
                new StartConfigurationSessionRequest
                {
                    ApplicationIdentifier = Options.ApplicationIdentifier,
                    EnvironmentIdentifier = Options.EnvironmentIdentifier,
                    ConfigurationProfileIdentifier = Options.ConfigurationProfileIdentifier
                }, cancellationToken).ConfigureAwait(false);

            _configurationToken = startConfigurationSessionResponse.InitialConfigurationToken;
        }

        var getLatestConfigurationResponse = await Client.GetLatestConfigurationAsync(new GetLatestConfigurationRequest
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
        using var reader = new StreamReader(getLatestConfigurationResponse.Configuration);
        var json = await reader.ReadToEndAsync().ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(json))
        {
            _configurationJson = json;
        }

        return _configurationJson;
    }

    public void Dispose()
    {
        _cancellationToken?.Cancel();
        _cancellationToken?.Dispose();
        _cancellationToken = null;

        try
        {
            _pollingTask?.ConfigureAwait(false).GetAwaiter().GetResult();
        }
        catch (TaskCanceledException)
        {}
            
        _pollingTask = null;

        _logger?.FeatureFlagsConfigurationProviderDisposed();
    }
}