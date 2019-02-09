using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class Configuration
{
    public bool UseProject { get; set; }
    public string ChangeString { get; set; } = "HasChanges";

    public Configuration(XElement config)
    {
        if (config == null)
        {
            return;
        }

        UseProject = GetBooleanAttr(config, "UseProject") ?? UseProject;
        ChangeString = GetStringAttr(config, "HasChanges") ?? ChangeString;
    }

    /// <summary>
    /// Get value from attribute as boolean
    /// </summary>
    /// <returns>null = not set</returns>
    private static bool? GetBooleanAttr(XElement config, string name)
    {
        var attr = config?.Attribute(name);
        if (string.IsNullOrWhiteSpace(attr?.Value))
        {
            return null;
        }

        try
        {
            var result = Convert.ToBoolean(attr.Value);
            return result;
        }
        catch
        {
            throw new WeavingException($"Unable to parse '{attr.Value}' as a boolean; please use 'true' or 'false'.");
        }
    }

    /// <summary>
    /// Get value from attribute as string
    /// </summary>
    /// <returns>null = not set</returns>
    private static string GetStringAttr(XElement config, string name)
    {
        var attr = config?.Attribute(name);
        if (string.IsNullOrWhiteSpace(attr?.Value))
        {
            return null;
        }

        return attr.Value;

    }
}
