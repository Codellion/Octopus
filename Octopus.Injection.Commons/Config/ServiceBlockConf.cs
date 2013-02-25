using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Octopus.Injection.Commons.Config
{
    public class ServiceBlockConf : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return this["name"].ToString(); }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("assemblyLocation", IsRequired = true)]
        public string AssemblyLocation
        {
            get { return this["assemblyLocation"].ToString(); }
            set { this["assemblyLocation"] = value; }
        }

        [ConfigurationProperty("dependences", IsRequired = false)]
        public Dependences Dependences
        {
            get { return (Dependences)this["dependences"]; }
        }

        [ConfigurationProperty("injectionMap", IsRequired = false)]
        public InjectionConf InjectionMap
        {
            get { return (InjectionConf)this["injectionMap"]; }
        }
    }
}
