using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Octopus.Injection.Commons.Config
{
    public class Contract : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return this["name"].ToString(); }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("interface", IsRequired = true)]
        public string Interface
        {
            get { return this["interface"].ToString(); }
            set { this["interface"] = value; }
        }

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
    }
}
