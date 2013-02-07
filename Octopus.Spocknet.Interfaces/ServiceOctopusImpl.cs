using Octopus.Injection.Commons;
using Verso.Net.Commons;

namespace Octopus.Spocknet.Interfaces
{
    public class ServiceOctopusImpl : IServiceBlock
    {
        public VersoMsg ExecuteServiceBlock(VersoMsg verso)
        {
            var versoRes = new VersoMsg();

            if(ConfigRepository.ServiceBlocks.ContainsKey(verso.ServiceBlock))
            {
                var serviceBlock = ConfigRepository.ServiceBlocks[verso.ServiceBlock];

                object res;

                if (serviceBlock.IsGeneric && ConfigRepository.ServiceTypes.ContainsKey(verso.TypeVerso.AssemblyName))
                {
                    var tGenService = ConfigRepository.ServiceTypes[verso.TypeVerso.AssemblyName]
                        .GetType(verso.TypeVerso.ClassName);

                    res = verso.Execute(Injector.InjectService(verso.ServiceBlock, tGenService), tGenService);    
                }
                else
                {
                    res = verso.Execute(Injector.InjectService(verso.ServiceBlock), null);
                }

                versoRes.DataVerso = res;
            }

            return versoRes;
        }
    }
}
 