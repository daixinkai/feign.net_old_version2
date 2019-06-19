using System;
using System.Reflection;
using Feign.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Feign.Tests.NET45
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            DynamicAssembly.DEBUG_MODE = true;
            FeignClientTypeBuilder feignClientTypeBuilder = new FeignClientTypeBuilder();
            feignClientTypeBuilder.BuildType(typeof(ITestService)); ;
            feignClientTypeBuilder.Save();

        }
    }
}
