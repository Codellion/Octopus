using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Octopus.Injection.Commons.Config
{
    [ConfigurationCollection(typeof(ServiceBlockConf), AddItemName = "service", CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
    public class ServiceBlocks : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ServiceBlockConf();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            return ((ServiceBlockConf)element).Name;
        }

        public new ServiceBlockConf this[string key]
        {
            get { return (ServiceBlockConf)BaseGet(key); }
        }

        public ServiceBlockConf this[int index]
        {
            get { return (ServiceBlockConf)BaseGet(index); }
        }
    }
}
