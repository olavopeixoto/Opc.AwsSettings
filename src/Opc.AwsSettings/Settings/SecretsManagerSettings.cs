namespace Opc.AwsSettings.Settings;

public record SecretsManagerSettings
{
    public bool LoadAll { get; init; } = false;

    public List<string>? AcceptedSecretArns { get; init; } = new();
    
    public string? Prefix { get; init; }
}