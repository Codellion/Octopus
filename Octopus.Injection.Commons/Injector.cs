using System;

namespace Octopus.Injection.Commons
{
    public static class Injector
    {
        public static object InjectService(string serviceBlock, Type dependency)
        {
            object res = null;

            if(ConfigRepository.ServiceBlocks.ContainsKey(serviceBlock))
            {
                var injType = ConfigRepository.ServiceBlocks[serviceBlock].InjectionType;

                var genParam = new Type[1];
                genParam[0] = dependency;

                injType = injType.MakeGenericType(genParam);
                res = Activator.CreateInstance(injType, null);
            }

            return res;
        }

        public static object InjectService(string serviceBlock)
        {
            object res = null;

            if (ConfigRepository.ServiceBlocks.ContainsKey(serviceBlock))
            {
                var injType = ConfigRepository.ServiceBlocks[serviceBlock].InjectionType;

                res = Activator.CreateInstance(injType, null);
            }

            return res;
        }

        public static object InjectService<T>(string serviceBlock)
        {
            object res = null;

            if (ConfigRepository.ServiceBlocks.ContainsKey(serviceBlock))
            {
                var injType = ConfigRepository.ServiceBlocks[serviceBlock].InjectionType;

                var genParam = new Type[1];
                genParam[0] = typeof(T);

                injType = injType.MakeGenericType(genParam);
                res = Activator.CreateInstance(injType, null);
            }

            return res;
        }
    }
}
