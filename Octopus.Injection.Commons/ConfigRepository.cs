using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace Octopus.Injection.Commons
{
    public static class ConfigRepository
    {
        public static Dictionary<string, ServiceBlock> ServiceBlocks { get; set; }
        public static Dictionary<string, Assembly> ServiceTypes { get; set; }

        public static void LoadConfiguration()
        {
            var secSvcBlock = ConfigurationManager.GetSection("octopus/serviceBlocks") as NameValueCollection;
            var secAsmDep = ConfigurationManager.GetSection("octopus/assembliesDependencies") as NameValueCollection;
            var secInjMap = ConfigurationManager.GetSection("octopus/injectionMap") as NameValueCollection;

            if (secSvcBlock != null && secSvcBlock.HasKeys())
            {
                ServiceBlocks = new Dictionary<string, ServiceBlock>(secSvcBlock.AllKeys.Length);
                ServiceTypes = new Dictionary<string, Assembly>();

                foreach (var asmLoc in secSvcBlock.AllKeys)
                {
                    var serviceBlock = new ServiceBlock(asmLoc);
                    var asmDir = secSvcBlock[asmLoc];

                    var sInjType = new string[2];

                    if (secInjMap != null && secInjMap.HasKeys() && secInjMap[asmLoc].Contains(","))
                        sInjType = secInjMap[asmLoc].Split(',');

                    if (secAsmDep != null && secAsmDep.HasKeys())
                    {
                        //Cargamos las librerias asociadas al servicio
                        foreach (var asmDep in secAsmDep.AllKeys)
                        {
                            string fileDll = string.Format("{0}/{1}", asmDir, asmDep);

                            var assembly = Assembly.LoadFrom(Path.GetFullPath(fileDll));
                            
                            serviceBlock.Assemblies.Add(assembly);
                            ServiceTypes[assembly.FullName] = assembly;

                            //Cargamos el tipo que se injectará en el servicio
                            if(asmDep.ToLower().Equals(sInjType[1].Trim().ToLower()))
                            {
                                serviceBlock.InjectionType = assembly.GetType(sInjType[0]);
                                serviceBlock.IsGeneric = serviceBlock.InjectionType.IsGenericType;
                            }
                        }   
                    }

                    ServiceBlocks[asmLoc] = serviceBlock;
                }
            }
        }
    }
}
