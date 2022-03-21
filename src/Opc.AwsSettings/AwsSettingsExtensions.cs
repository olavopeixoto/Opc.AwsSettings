using System.Text.RegularExpressions;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Opc.AwsSettings;
using Opc.AwsSettings.SecretsManager;
using Opc.AwsSettings.Settings;
using Opc.AwsSettings.SystemsManager.AppConfig.FeatureFlags;
using Opc.AwsSettings.SystemsManager.ParameterStore;

namespace Microsoft.Extensions.Configuration;

public static class AwsSettingsExtensions
{
    /// <summary>
    /// Add configuration source from AWS Parameter Store, Secrets Manager, AppConfig Freeform Configuration and AppConfig Feature Flags
    /// </summary>
    /// <param name="builder">HostBuilder</param>
    /// <param name="logger">Optional logger to return information about data being loaded</param>
    /// <returns></returns>
    public static IHostBuilder AddAwsSettings(this IHostBuilder builder, ILogger? logger = null)
    {
        builder.ConfigureAppConfiguration((context, configurationBuilder) =>
        {
            var currentConfiguration = configurationBuilder.Build();
            var awsSettings = currentConfiguration.GetSettings<AwsSettings>();

            var keys = awsSettings.ParameterStore.Keys
                .Union(awsSettings.ParameterStore.Paths
                    .Where(path => awsSettings.ParameterStore.Keys.All(k => k.Path != path))
                    .Select(path =>
                        new ParameterStoreKeySettings
                        {
                            Path = path,
                        }));
            
            foreach (var key in keys)
            {
                configurationBuilder.AddParameterStore(key.Path, key.Alias, awsSettings.ReloadAfter, logger);
            }
            
            
            var secretsManagerPrefix = awsSettings.SecretsManager.Prefix?.TrimStart('/').TrimEnd('/');
            if (secretsManagerPrefix is not null || awsSettings.SecretsManager.AcceptedSecretArns is not null && awsSettings.SecretsManager.AcceptedSecretArns.Count > 0)
            {
                configurationBuilder.AddSecretsManager(configurator: options =>
                {
                    if (awsSettings.SecretsManager.AcceptedSecretArns is not null && awsSettings.SecretsManager.AcceptedSecretArns.Count > 0)
                    {
                        options.AcceptedSecretArns = awsSettings.SecretsManager.AcceptedSecretArns;
                        
                        logger?.AddedSecretsManager(options.AcceptedSecretArns, awsSettings.ReloadAfter);
                    }
                    else
                    {
                        var prefixes = new[]
                        {
                            $"{secretsManagerPrefix}/",
                            $"{context.HostingEnvironment.EnvironmentName.ToLowerInvariant()}/"
                        };
                    
                        logger?.AddedSecretsManager(prefixes, awsSettings.ReloadAfter);
                    
                        options.PollingInterval = awsSettings.ReloadAfter;
                
                        var regexes = prefixes.Select(prefix => new Regex(Regex.Escape(prefix),
                            RegexOptions.IgnoreCase & RegexOptions.Singleline & RegexOptions.Compiled));
                        
                        if (!awsSettings.SecretsManager.LoadAll)
                        {
                            options.ListSecretsFilters = new List<Filter>
                            {
                                new()
                                {
                                    Key = FilterNameStringType.Name,
                                    Values = prefixes.ToList()
                                }
                            };
                
                            options.SecretFilter = secret => prefixes.Any(prefix => secret.Name.StartsWith(prefix)) || options.AcceptedSecretArns.Any();
                        }
                
                        options.KeyGenerator = (_, key) =>
                        {
                            var result = regexes.Aggregate(key, (current, regex) => regex.Replace(current, string.Empty, 1));
                
                            return result.Replace(@"/", ":");
                        };
                    }

                }, logger);                
            }
            

            foreach (var featureSettings in awsSettings.AppConfig.FreeformConfigurations)
            {
                logger?.AddedAppConfigFreeformConfiguration(awsSettings.AppConfig.ApplicationIdentifier, context.HostingEnvironment.EnvironmentName, featureSettings.ConfigurationProfileIdentifier, awsSettings.ReloadAfter);

                configurationBuilder.AddAppConfig(
                    awsSettings.AppConfig.ApplicationIdentifier,
                    context.HostingEnvironment.EnvironmentName, 
                    featureSettings.ConfigurationProfileIdentifier,
                    awsSettings.ReloadAfter);
            }

            
            foreach (var featureSettings in awsSettings.AppConfig.FeatureFlags)
            {
                configurationBuilder.AddFeatureFlags(
                    awsSettings.AppConfig.ApplicationIdentifier, 
                    context.HostingEnvironment.EnvironmentName, 
                    featureSettings.ConfigurationProfileIdentifier, 
                    awsSettings.ReloadAfter, 
                    logger);
            }
        });

        return builder;
    }

    private static void AddParameterStore(this IConfigurationBuilder configurationBuilder, string path, string? alias, TimeSpan? reloadAfter, ILogger? logger)
    {
        configurationBuilder.AddSystemsManager(options =>
        {
            options.Path = path;
            options.Prefix = alias;
                        
            if (reloadAfter.HasValue)
                options.ReloadAfter = reloadAfter;
            
            options.OnLoadException = exceptionContext =>
            {
                logger?.ErrorLoadingFromParameterStore(exceptionContext.Exception, exceptionContext.Provider.GetType().Name);
                exceptionContext.Ignore = true;
            };

            options.ParameterProcessor = new ArraySupportParameterProcessor();

            logger?.AddedParameterStore(path, reloadAfter);
        });
    }
}