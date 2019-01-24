using System;
using System.Xml.Linq;
using Stamp.Fody;

public class Configuration
{
    public bool UseProject { get; set; }

    public bool PatchVersion { get; set; }
    public bool PatchFileVersion { get; set; }
    public bool PatchInformationVersion { get; set; }

    //todo
    public InformationVersionSource PatchInformationVersionSource { get; set; } = InformationVersionSource.Version;

    public string ChangeString { get; set; };

    public Configuration(XElement config)
    {
        UseProject = GetBooleanAttr(config, "UseProject") ?? false;
        PatchVersion = GetBooleanAttr(config, "PatchVersion") ?? true;
        PatchFileVersion = GetBooleanAttr(config, "PatchVersion") ?? true;
        PatchInformationVersion = GetBooleanAttr(config, "PatchInformationVersion") ?? true;
        ChangeString = GetStringAttr(config, "HasChanges");

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

        return ConvertAndThrowIfNotBoolean(attr.Value);

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

    private static bool ConvertAndThrowIfNotBoolean(string value)
    {
        try
        {
            var result = Convert.ToBoolean(value);
            return result;
        }
        catch
        {
            throw new WeavingException($"Unable to parse '{value}' as a boolean; please use 'true' or 'false'.");
        }
    }
}
