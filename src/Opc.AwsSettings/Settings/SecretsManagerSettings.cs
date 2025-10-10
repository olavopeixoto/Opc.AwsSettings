using JetBrains.Annotations;

namespace Opc.AwsSettings.Settings;

[PublicAPI]
public sealed record SecretsManagerSettings
{
    /// <summary>
    /// </summary>
    public bool LoadAll { get; init; } = false;

    /// <summary>
    /// </summary>
    public List<string>? AcceptedSecretArns { get; init; } = new();

    /// <summary>
    /// </summary>
    public string? Prefix { get; init; }

    public bool Optional { get; init; }
}
