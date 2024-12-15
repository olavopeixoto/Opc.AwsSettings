namespace Opc.AwsSettings.Settings;

public record AppConfigProfileSettings
{
    public required string Identifier { get; init; }
    public bool Optional { get; init; }
}