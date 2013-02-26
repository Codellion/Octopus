using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Octopus.Injection.Commons.Config
{
    public class InjectionMap : ConfigurationElement
    {
        [ConfigurationProperty("contracts", IsRequired = false)]
        public Contracts Contracts
        {
            get { return (Contracts)this["contracts"]; }
        }

        [ConfigurationProperty("injections", IsRequired = false)]
        public Injections Injections
        {
            get { return (Injections)this["injections"]; }
        }
    }
}
