using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Linq;

using Octopus.Injection.Commons.Config;

namespace Octopus.Injection.Commons
{
    public static class ConfigRepository
    {
        public static Dictionary<string, ServiceBlock> ServiceBlocks { get; set; }
        public static Dictionary<string, Assembly> ServiceTypes { get; set; }
        private static Dictionary<string, Assembly> ServiceTypesCrossDomain { get; set; }

        public static void LoadConfiguration()
        {
            var octoSec = ConfigurationManager.GetSection("spock/octopus") as OctopusSection;                 

            if (octoSec != null && octoSec.ServiceBlocks != null)
            {
                ServiceBlocks = new Dictionary<string, ServiceBlock>(octoSec.ServiceBlocks.Count);
                ServiceTypes = new Dictionary<string, Assembly>();
                ServiceTypesCrossDomain = new Dictionary<string, Assembly>();


                foreach (ServiceBlockConf svcBlockConf in octoSec.ServiceBlocks)
                {
                    var serviceBlock = new ServiceBlock(svcBlockConf.Name);
                    var asmDir = svcBlockConf.AssemblyLocation;

                    foreach (DependenceConf dependConf in svcBlockConf.Dependencies)
                    {
                        string fileDll = string.Format("{0}/{1}", asmDir, dependConf.Assembly);

                        Assembly assembly = null;
                        
                        if (dependConf.IsCrossDomain)
                        {
                            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                            assembly = AppDomain.CurrentDomain.Load("@#" + fileDll);
                            ServiceTypesCrossDomain[assembly.FullName] = assembly;
                        }
                        else
                        {
                            assembly = Assembly.LoadFrom(Path.GetFullPath(fileDll));
                            ServiceTypes[assembly.FullName] = assembly;
                        }

                        serviceBlock.Assemblies.Add(assembly);

                        //Cargamos el tipo que se injectará en el servicio
                        if (svcBlockConf.GenericParameter != null &&
                            dependConf.Assembly.ToLower().Equals(svcBlockConf.GenericParameter.Assembly.ToLower()))
                        {
                            serviceBlock.InjectionType = assembly.GetType(svcBlockConf.GenericParameter.Class);
                            serviceBlock.IsGeneric = serviceBlock.InjectionType.IsGenericType;
                        }

                        ServiceBlocks[serviceBlock.Name] = serviceBlock;
                    }
                }
            }
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly assembly = null;

            if (args.Name.StartsWith("@#"))
            {
                var fileDll = args.Name.Substring(2);

                assembly = Assembly.LoadFrom(Path.GetFullPath(fileDll));
            }
            else
            {
                if (ServiceTypesCrossDomain != null)
                {
                    if(ServiceTypesCrossDomain.ContainsKey(args.Name))
                    {
                        assembly = ServiceTypesCrossDomain[args.Name];    
                    }
                    else
                    {
                        assembly = ServiceTypesCrossDomain
                                        .Where(asm => asm.Key.Split(',')[0].Equals(args.Name))
                                        .Select(asm => asm.Value).FirstOrDefault();
                    }

                    if (assembly == null && ServiceTypes != null && ServiceTypes.ContainsKey(args.Name))
                    {
                        assembly = ServiceTypes[args.Name];
                    }
                }
            }

            return assembly;
        }
    }
}
