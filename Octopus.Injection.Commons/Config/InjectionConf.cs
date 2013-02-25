using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Octopus.Injection.Commons.Config
{
    public class InjectionConf : ConfigurationElement
    {
        [ConfigurationProperty("class", IsRequired = true)]
        public string Class
        {
            get { return this["class"].ToString(); }
            set { this["class"] = value; }
        }

        [ConfigurationProperty("assembly", IsRequired = true)]
        public string Assembly
        {
            get { return this["assembly"].ToString(); }
            set { this["assembly"] = value; }
        }
    }
}
