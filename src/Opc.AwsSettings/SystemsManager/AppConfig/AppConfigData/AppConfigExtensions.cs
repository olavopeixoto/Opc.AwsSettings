using System.Data;
using System.Reflection;
using Amazon.Extensions.Configuration.SystemsManager.Internal;
using Microsoft.Extensions.Configuration;

namespace Opc.AwsSettings.SystemsManager.AppConfig.AppConfigData;

internal static class AppConfigExtensions
{
    public static IConfigurationBuilder AddAppConfig(this IConfigurationBuilder configurationBuilder,
        AppConfigDataSource source)
    {
        source.ApplicationIdentifier ??= Assembly.GetCallingAssembly().GetName().Name ??
                                         throw new NoNullAllowedException(
                                             "Unable to parse ApplicationIdentifier. Calling Assembly Name is null");

        if (string.IsNullOrWhiteSpace(source.ConfigurationProfileIdentifier))
            throw new ArgumentNullException(nameof(source.ConfigurationProfileIdentifier));

        source.AwsOptions ??= AwsOptionsProvider.GetAwsOptions(configurationBuilder);

        configurationBuilder.Add(source);

        return configurationBuilder;
    }
}
