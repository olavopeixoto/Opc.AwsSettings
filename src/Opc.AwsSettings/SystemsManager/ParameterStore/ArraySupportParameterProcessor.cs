using System.Text.RegularExpressions;
using Amazon.Extensions.Configuration.SystemsManager;
using Amazon.SimpleSystemsManagement.Model;

namespace Opc.AwsSettings.SystemsManager.ParameterStore;

/// <inheritdoc />
/// <summary>
///     Parameter processor based on Default Parameter processor with support for json array
/// </summary>
internal sealed class ArraySupportParameterProcessor : DefaultParameterProcessor
{
    public override string? GetValue(Parameter parameter, string path)
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
