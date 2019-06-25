using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Feign.TestWeb
{
    [FeignClient("test-service", Url = "http://www.baidu.com")]
    public interface ITestService1
    {
        Task<string> GetHtml();
    }
}
