using System.Reflection;
using Amazon.Extensions.Configuration.SystemsManager.Internal;
using Microsoft.Extensions.Configuration;

namespace Opc.AwsSettings.SystemsManager.AppConfig.FreeForm;

public static class FreeFormAppConfigExtensions
{
    public static IConfigurationBuilder AddFreeFormAppConfig(this IConfigurationBuilder configurationBuilder, AppConfigFreeFormConfigurationSource source)
    {
        source.ApplicationId ??= Assembly.GetCallingAssembly().GetName().Name ?? throw new NullReferenceException("Unable to parse ApplicationIdentifier. Calling Assembly Name is null");
        
        if (string.IsNullOrWhiteSpace(source.ConfigProfileId))
            throw new ArgumentNullException(nameof(source.ConfigProfileId));

        source.AwsOptions ??= AwsOptionsProvider.GetAwsOptions(configurationBuilder);

        configurationBuilder.Add(source);

        return configurationBuilder;
    }
}