using System;
using System.Collections.Generic;
using System.Reflection;

namespace Octopus.Injection.Commons
{
    public class ServiceBlock
    {
        public string Name { get; set; }

        public List<Assembly> Assemblies { get; set; }

        public Type InjectionType { get; set; }

        public bool IsGeneric { get; set; }

        public ServiceBlock (string name)
        {
            Assemblies = new List<Assembly>();

            Name = name;
        }
    }
}
