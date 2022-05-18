namespace Opc.AwsSettings.Settings;

public record FeatureFlagsSettings
{
    public string ConfigurationProfileIdentifier { get; init; }
    public bool Optional { get; init; }
}