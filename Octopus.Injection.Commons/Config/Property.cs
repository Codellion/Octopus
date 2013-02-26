using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Octopus.Injection.Commons.Config
{
    public class Property : ConfigurationElement
    {       
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return this["name"].ToString(); }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("contract", IsRequired = false)]
        public string Contract
        {
            get { return this["contract"].ToString(); }
            set { this["contract"] = value; }
        }

        [ConfigurationProperty("class", IsRequired = false)]
        public string Class
        {
            get { return this["class"].ToString(); }
            set { this["class"] = value; }
        }

        [ConfigurationProperty("singleton", IsRequired = false, DefaultValue = false)]
        public bool Singleton
        {
            get { return bool.Parse(this["singleton"].ToString()); }
            set { this["singleton"] = value; }
        }
    }
}
