namespace Opc.AwsSettings.Settings;

public record AppConfigFreeformSettings
{
    public string ConfigurationProfileIdentifier { get; init; }
    public bool Optional { get; init; }
}