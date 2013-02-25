using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Octopus.Injection.Commons.Config
{
    public class DependenceConf : ConfigurationElement
    {       
        [ConfigurationProperty("assembly", IsRequired = true)]
        public string Assembly
        {
            get { return this["assembly"].ToString(); }
            set { this["assembly"] = value; }
        }

        [ConfigurationProperty("isCrossDomain", IsRequired = false, DefaultValue = false)]
        public bool IsCrossDomain
        {
            get { return bool.Parse(this["isCrossDomain"].ToString()); }
            set { this["isCrossDomain"] = value; }
        }
    }
}
