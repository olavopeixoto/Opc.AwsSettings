using System.Text.RegularExpressions;
using Amazon.Extensions.Configuration.SystemsManager;
using Amazon.Extensions.Configuration.SystemsManager.Utils;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Opc.AwsSettings.Settings;

namespace Opc.AwsSettings.SystemsManager.ParameterStore;

/// <inheritdoc />
/// <summary>
///     Parameter processor based on Default Parameter processor with support for json array
/// </summary>
internal sealed class AwsSettingsParameterProcessor(ILogger? logger, ParameterStoreKeySettings settings) : IParameterProcessor
{
    private string GetKey(Parameter parameter, string path)
    {
        return settings.Alias ?? (parameter.Name.StartsWith(path, StringComparison.OrdinalIgnoreCase) ? parameter.Name.Substring(path.Length) : parameter.Name).TrimStart('/').Replace("/", ConfigurationPath.KeyDelimiter);
    }

    public IDictionary<string, string> ProcessParameters(
        IEnumerable<Parameter> parameters,
        string path)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var parameter in parameters)
        {
            var key = GetKey(parameter, path);
            var str = GetValue(parameter);
            if (parameter.Type == ParameterType.StringList)
                ParameterProcessorUtil.ParseStringListParameter(key, str, result);
            else if (parameter.Type == ParameterType.String)
                ParameterProcessorUtil.ParseStringParameter(key, str, result);
            else
                try
                {
                    ParameterProcessorUtil.ParseJsonParameter(key, str, result);
                }
                catch(Exception ex)
                {
                    logger?.ErrorLoadingParameter(ex, parameter.Name, parameter.Value);
                    ParameterProcessorUtil.ParseStringParameter(key, str, result);
                }
        }
        return result;
    }

    private static string? GetValue(Parameter parameter)
    {
        if (parameter.Value is null) return parameter.Value;

        var matches = Regex.Matches(parameter.Value, "(\"[^\"]+\":)", RegexOptions.Multiline);

        var result = parameter.Value;

        foreach (Match match in matches)
        {
            var newValue = match.Value.Replace("/", ":");
            result = result.Remove(match.Index, match.Length).Insert(match.Index, newValue);
        }

        return result;
    }
}
