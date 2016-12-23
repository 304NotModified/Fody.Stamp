using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using Stamp.Fody;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class Configuration
{
    public bool UseProject { get; set; }
    public string ChangeString { get; set; } = "HasChanges";

    /// <summary>
    /// Patch production version (also called assembly information version)?
    /// </summary>
    public bool PatchProductionVersion { get; set; } = true;

    /// <summary>
    /// Patch (assemlby) file version?
    ///
    /// Default: false
    /// </summary>
    public bool PatchFileVersion { get; set; } = true;

    //todo
    public InformationVersionSource PatchInformationVersionSource { get; set; } = InformationVersionSource.Version;

    public Configuration(XElement config)
    {
        if (config == null)
        {
            return;
        }

        UseProject = GetBooleanAttr(config, "UseProject") ?? UseProject;
        ChangeString = GetStringAttr(config, "HasChanges") ?? ChangeString;
        PatchProductionVersion = GetBooleanAttr(config, "PatchProductionVersion") ?? PatchProductionVersion;
        PatchFileVersion = GetBooleanAttr(config, "PatchProductionVersion") ?? PatchFileVersion;
        //todo read source
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
