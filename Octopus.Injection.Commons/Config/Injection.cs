using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Octopus.Injection.Commons.Config
{
    public class Injection : ConfigurationElement
    {
        [ConfigurationProperty("class", IsRequired = true)]
        public string Class
        {
            get { return this["class"].ToString(); }
            set { this["class"] = value; }
        }

        [ConfigurationProperty("singleton", IsRequired = false)]
        public bool Singleton
        {
            get { return bool.Parse(this["singleton"].ToString()); }
            set { this["singleton"] = value; }
        }

        [ConfigurationProperty("properties", IsRequired = true)]
        public Properties Properties
        {
            get { return (Properties)this["properties"]; }
        }
    }
}
