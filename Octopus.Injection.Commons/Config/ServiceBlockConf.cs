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

        [ConfigurationProperty("dependencies", IsRequired = false)]
        public Dependencies Dependencies
        {
            get { return (Dependencies)this["dependencies"]; }
        }

        [ConfigurationProperty("genericParameter", IsRequired = false)]
        public GenericParameter GenericParameter
        {
            get { return (GenericParameter)this["genericParameter"]; }
        }
    }
}
