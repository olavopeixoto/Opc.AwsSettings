using JetBrains.Annotations;

namespace Opc.AwsSettings.Settings;

[PublicAPI]
public sealed record AppConfigSettings
{
    public required string ApplicationIdentifier { get; init; }
    public bool UseLambdaCacheLayer { get; init; } = false;
    public List<AppConfigProfileSettings> ConfigurationProfiles { get; init; } = [];
}
