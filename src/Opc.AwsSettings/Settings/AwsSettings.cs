namespace Opc.AwsSettings.Settings;

/// <summary>
///     Controls from where to read the configuration and how to map it to your options objects
/// </summary>
public sealed record AwsSettings
{
    /// <summary>
    ///     Determines how to read configuration from AWS Systems Manager / Parameter Store.<br />
    ///     It can also read configuration from Secrets Manager using individual keys "/aws/reference/secretsmanager/{my-key}"
    /// </summary>
    public ParameterStoreSettings ParameterStore { get; set; } = new();

    /// <summary>
    ///     Determines how to read configuration from AWS Systems Manager / AppConfig
    /// </summary>
    public AppConfigSettings AppConfig { get; set; } = new()
    {
        ApplicationIdentifier = string.Empty
    };

    /// <summary>
    ///     Determines how to read configuration from Secrets Manager in a more flexible way than using
    ///     <see cref="ParameterStore" />
    /// </summary>
    public SecretsManagerSettings SecretsManager { get; set; } = new();

    /// <summary>
    ///     Configures if and in what frequency it should reload the settings from the source.<br />
    ///     Null tells it to not reload values.<br />
    ///     The same value is used across all sources.
    /// </summary>
    public TimeSpan? ReloadAfter { get; set; }
}
