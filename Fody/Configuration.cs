using System;
using System.Xml.Linq;

public class Configuration
{
    public bool UseProject = false;
    public bool UseFileVersion = false;
    public bool OverwriteFileVersion = true;
    public string ChangeString = "HasChanges";

    public Configuration(XElement config)
    {
        if (config == null)
        {
            return;
        }

        var attr = config.Attribute("UseProjectGit");
        if (HasValue(attr))
        {
            UseProject = ConvertAndThrowIfNotBoolean(attr.Value);
        }

        attr = config.Attribute("UseFileVersion");
        if (HasValue(attr))
        {
            UseFileVersion = ConvertAndThrowIfNotBoolean(attr.Value);
        }

        attr = config.Attribute("ChangeString");
        if (HasValue(attr))
        {
            ChangeString = attr.Value;
        }

        if (UseFileVersion)
            OverwriteFileVersion = false;
        else
        {
            attr = config.Attribute("OverwriteFileVersion");
            if (HasValue(attr))
            {
                OverwriteFileVersion = ConvertAndThrowIfNotBoolean(attr.Value);
            }
        }
    }

    private bool HasValue(XAttribute attr)
    {
        return !String.IsNullOrWhiteSpace(attr?.Value);
    }

    private bool ConvertAndThrowIfNotBoolean(string value)
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
