namespace Opc.AwsSettings.Settings;

public sealed record SecretsManagerSettings
{
    /// <summary>
    /// </summary>
    public bool LoadAll { get; set; } = false;

    /// <summary>
    /// </summary>
    public List<string>? AcceptedSecretArns { get; set; } = [];

    /// <summary>
    /// </summary>
    public string? Prefix { get; set; }

    public bool Optional { get; set; }
}
