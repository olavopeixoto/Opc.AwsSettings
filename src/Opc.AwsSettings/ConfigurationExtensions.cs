using Microsoft.Extensions.Configuration;

public static class ConfigurationExtensions
{
    /// <summary>
    ///     Attempts to bind the configuration instance to a new instance of type T using the configuration section with the
    ///     type name.
    /// </summary>
    /// <param name="configuration">The configuration instance to bind.</param>
    /// <typeparam name="T">The type of the new instance to bind.</typeparam>
    /// <returns>The new instance of T if successful, default(T) otherwise.</returns>
    public static T GetSettings<T>(this IConfiguration configuration)
    {
        return configuration.GetSection(typeof(T).Name).Get<T>();
    }
}