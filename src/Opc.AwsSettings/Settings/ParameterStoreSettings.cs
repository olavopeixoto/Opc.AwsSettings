namespace Opc.AwsSettings.Settings;

public record ParameterStoreSettings
{
    public string[] Paths { get; init; } = Array.Empty<string>();
    public ParameterStoreKeySettings[] Keys { get; init; } = Array.Empty<ParameterStoreKeySettings>();
}