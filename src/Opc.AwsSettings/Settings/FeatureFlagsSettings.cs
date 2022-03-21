using Microsoft.Extensions.Configuration;

namespace Opc.AwsSettings.Settings;

public record FeatureFlagsSettings
{
    public string ConfigurationProfileIdentifier { get; init; }
}