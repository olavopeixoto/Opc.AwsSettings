namespace Opc.AwsSettings.Settings;

public sealed record ParameterStoreKeySettings
{
    /// <summary>
    ///     Use prefix /aws/reference/secretsmanager/{my-key} for Secrets Manager values
    /// </summary>
    public required string Path { get; set; }

    /// <summary>
    ///     Alternative name to map to configuration.
    /// </summary>
    /// <example>
    ///     Path = /aws/reference/secretsmanager/Awe@ful.Key+Name
    ///     Alias = BeautifulName
    ///     This would map to BeautifulName instead of Awe@ful.Key+Name
    /// </example>
    public string? Alias { get; set; }

    public bool Optional { get; set; }
}
