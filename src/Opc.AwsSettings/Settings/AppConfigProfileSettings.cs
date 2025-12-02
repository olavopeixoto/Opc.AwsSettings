namespace Opc.AwsSettings.Settings;

public sealed record AppConfigProfileSettings
{
    public required string Identifier { get; set; }
    public bool Optional { get; set; }
}
