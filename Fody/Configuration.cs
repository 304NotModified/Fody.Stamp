using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

public class Configuration {
    public bool UseProject { get; set; }
    public string ChangeString { get; set; }

    public Configuration(XElement config) {
        UseProject = false;
        ChangeString = "HasChanges";

        if ( config == null )
            return;

        var attr = config.Attribute("UseProjectGit");
        if ( attr != null ) {
            try {
                this.UseProject = Convert.ToBoolean(attr.Value);
            } catch (Exception ex) {
                throw new WeavingException( String.Format("Unable to parse '{0}' as a boolean, please use true or false.") );
            }
            
        }

        attr = config.Attribute("ChangeString");
        if ( attr != null && !String.IsNullOrWhiteSpace(attr.Value) ) {
            this.ChangeString = config.Attribute("ChangeString").Value;
        }
    }
}
