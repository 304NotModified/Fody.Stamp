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
            try
            {
                UseProject = Convert.ToBoolean(attr.Value);
            }
            catch (Exception)
            {
                throw new WeavingException($"Unable to parse '{attr.Value}' as a boolean, please use true or false.");
            }
        }

        attr = config.Attribute("ChangeString");
        if (!string.IsNullOrWhiteSpace(attr?.Value))
        {
            ChangeString = attr.Value;
        }
    }
}
