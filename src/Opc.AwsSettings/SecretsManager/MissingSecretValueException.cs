namespace Opc.AwsSettings.SecretsManager;

public sealed class MissingSecretValueException(string errorMessage, string secretName, string secretArn, Exception exception)
    : Exception(errorMessage, exception)
{
    public string SecretArn { get; } = secretArn;

    public string SecretName { get; } = secretName;
}
