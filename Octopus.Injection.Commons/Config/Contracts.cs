using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Octopus.Injection.Commons.Config
{
    [ConfigurationCollection(typeof(Contract), AddItemName = "contract", CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
    public class Contracts : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new Contract();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            return ((Contract)element).Name;
        }

        public new Contract this[string key]
        {
            get { return (Contract)BaseGet(key); }
        }

        public Contract this[int index]
        {
            get { return (Contract)BaseGet(index); }
        }
    }
}
