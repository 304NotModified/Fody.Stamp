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
                throw new WeavingException(String.Format("Unable to parse '{0}' as a boolean, please use true or false.", attr.Value));
            }
        }

        attr = config.Attribute("ChangeString");
        if (attr != null && !String.IsNullOrWhiteSpace(attr.Value))
        {
            ChangeString = config.Attribute("ChangeString").Value;
        }
    }
}
