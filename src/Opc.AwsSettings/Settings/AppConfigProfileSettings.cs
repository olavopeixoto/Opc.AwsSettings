using JetBrains.Annotations;

namespace Opc.AwsSettings.Settings;

[PublicAPI]
public sealed record AppConfigProfileSettings
{
    public required string Identifier { get; init; }
    public bool Optional { get; init; }
}
