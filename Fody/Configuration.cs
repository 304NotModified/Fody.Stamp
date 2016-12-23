using System;
using System.Xml.Linq;

public class Configuration
{
    public bool UseProject;
    public string ChangeString = "HasChanges";

    public Configuration(XElement config)
    {
        if (config == null)
        {
            return;
        }

        var attr = config.Attribute("UseProjectGit");
        if (attr != null)
        {
            UseProject = ConvertAndThrowIfNotBoolean(attr.Value);
        }

        attr = config.Attribute("ChangeString");
        if (HasValue(attr))
        {
            ChangeString = attr.Value;
        }
    }

    private bool HasValue(XAttribute attr)
    {
        return !String.IsNullOrWhiteSpace(attr?.Value);
    }

    private bool ConvertAndThrowIfNotBoolean(string value)
    {
        bool result;
        if (Boolean.TryParse(value, out result))
            return result;
        else
            throw new WeavingException($"Unable to parse '{value}' as a boolean; please use 'true' or 'false'.");
    }
}
