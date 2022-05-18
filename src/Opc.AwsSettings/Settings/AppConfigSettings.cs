namespace Opc.AwsSettings.Settings;

public record AppConfigSettings
{
    public string ApplicationIdentifier { get; init; }
    public bool UseLambdaCacheLayer { get; init; } = false;
    public List<FeatureFlagsSettings> FeatureFlags { get; init; } = new();
    public List<AppConfigFreeformSettings> FreeFormConfigurations { get; init; } = new();
}