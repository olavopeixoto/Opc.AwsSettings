namespace Opc.AwsSettings.Settings;

public sealed record AppConfigSettings
{
    public required string ApplicationIdentifier { get; set; }
    public bool UseLambdaCacheLayer { get; set; } = false;
    public List<AppConfigProfileSettings> ConfigurationProfiles { get; set; } = [];
}
