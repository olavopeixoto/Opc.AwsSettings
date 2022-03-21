namespace Opc.AwsSettings.Settings;

public record AwsSettings
{
    public ParameterStoreSettings ParameterStore { get; init; } = new();   

    public AppConfigSettings AppConfig { get; init; } = new();

    public SecretsManagerSettings SecretsManager { get; init; } = new();
    
    public TimeSpan? ReloadAfter { get; init; } = TimeSpan.FromMinutes(10);
}