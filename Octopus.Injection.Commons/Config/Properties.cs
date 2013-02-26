using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Octopus.Injection.Commons.Config
{
    [ConfigurationCollection(typeof(Property), AddItemName = "property", CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
    public class Properties : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new Property();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            return ((Property)element).Name;
        }

        public new Property this[string key]
        {
            get { return (Property)BaseGet(key); }
        }

        public Property this[int index]
        {
            get { return (Property)BaseGet(index); }
        }
    }
}
