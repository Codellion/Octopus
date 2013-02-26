using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Octopus.Injection.Commons.Config
{
    [ConfigurationCollection(typeof(DependenceConf), AddItemName = "dependency", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class Dependencies : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DependenceConf();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            return ((DependenceConf)element).Assembly;
        }

        public new DependenceConf this[string key]
        {
            get { return (DependenceConf)BaseGet(key); }
        }

        public DependenceConf this[int index]
        {
            get { return (DependenceConf)BaseGet(index); }
        }
    }
}
