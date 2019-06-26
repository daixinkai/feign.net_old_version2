using Feign.Cache;
using Feign.Discovery;
using Feign.Proxy;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Feign.Tests
{
    public class TestFeignClientHttpProxy : FeignClientHttpProxy
    {
        public TestFeignClientHttpProxy(IFeignOptions feignOptions, IServiceDiscovery serviceDiscovery, IServiceCacheProvider serviceCacheProvider, Feign.Logging.ILoggerFactory loggerFactory) : base(feignOptions, serviceDiscovery, serviceCacheProvider, loggerFactory)
        {

        }
        public override string ServiceId => throw new NotImplementedException();

        public JObject Test(string s)
        {
            try
            {

                MethodBase method = null;

                MethodInfo methodInfo = (MethodInfo)method;

                //return Send<JObject>(new FeignClientRequest("", "", "", "GET", "", null, null));
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
