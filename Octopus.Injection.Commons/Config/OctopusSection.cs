using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Octopus.Injection.Commons.Config
{
    public class OctopusSection : ConfigurationSection
    {
        [ConfigurationProperty("serviceBlocks", IsRequired = false)]
        public ServiceBlocks ServiceBlocks
        {
            get { return (ServiceBlocks)this["serviceBlocks"]; }
        }

        [ConfigurationProperty("injectionMap", IsRequired = false)]
        public InjectionMap InjectionMap
        {
            get { return (InjectionMap)this["injectionMap"]; }
        }
    }
}
