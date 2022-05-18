namespace Opc.AwsSettings.Settings;

public record AppConfigProfileSettings
{
    public string Identifier { get; init; }
    public bool Optional { get; init; }
}