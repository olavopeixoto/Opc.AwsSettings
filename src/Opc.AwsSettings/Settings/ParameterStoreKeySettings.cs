namespace Opc.AwsSettings.Settings;

public record ParameterStoreKeySettings
{
    /// <summary>
    /// Use prefix /aws/reference/secretsmanager/{my-key} for Secrets Manager values
    /// </summary>
    public string Path { get; init; }
    
    /// <summary>
    /// Alternative name to map to configuration.
    /// Example:
    /// Path = /aws/reference/secretsmanager/Awe@ful.Key+Name
    /// Alias = BeautifulName
    /// This would map to BeautifulName instead of Awe@ful.Key+Name
    /// </summary>
    public string? Alias { get; init; }
}