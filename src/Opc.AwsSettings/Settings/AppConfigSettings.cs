namespace Opc.AwsSettings.Settings;

public record AppConfigSettings
{
    public string ApplicationIdentifier { get; init; }
    public List<FeatureFlagsSettings> FeatureFlags { get; init; } = new();
    public List<AppConfigFreeformSettings> FreeformConfigurations { get; init; } = new();
}