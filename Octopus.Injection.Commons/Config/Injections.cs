using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Octopus.Injection.Commons.Config
{
    [ConfigurationCollection(typeof(Injection), AddItemName = "injection", CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
    public class Injections : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new Injection();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            return ((Injection)element).Class;
        }

        public new Injection this[string key]
        {
            get { return (Injection)BaseGet(key); }
        }

        public Injection this[int index]
        {
            get { return (Injection)BaseGet(index); }
        }
    }
}
