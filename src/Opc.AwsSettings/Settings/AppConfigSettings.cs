namespace Opc.AwsSettings.Settings;

public record AppConfigSettings
{
    public string ApplicationIdentifier { get; init; }
    public bool UseLambdaCacheLayer { get; init; } = false;
    public List<AppConfigProfileSettings> ConfigurationProfiles { get; init; } = new();
}