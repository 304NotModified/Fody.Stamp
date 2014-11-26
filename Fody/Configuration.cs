using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

public class Configuration {
    public bool UseProject { get; set; }
    public string ChangeString { get; set; }

    public Configuration(XElement config) {
        UseProject = false;

        if ( config == null )
            return;

        var attr = config.Attribute("UseProjectGit");
        if ( attr != null && !String.IsNullOrWhiteSpace(attr.Value) ) {
            try {
                this.UseProject = Convert.ToBoolean(attr.Value);
            } catch {
                this.UseProject = false;
            }
        }

        attr = config.Attribute("ChangeString");
        if ( attr != null && !String.IsNullOrWhiteSpace(attr.Value) ) {
            this.ChangeString = config.Attribute("ChangeString").Value;
        } else {
            this.ChangeString = "HasChanges";
        }
    }
}
