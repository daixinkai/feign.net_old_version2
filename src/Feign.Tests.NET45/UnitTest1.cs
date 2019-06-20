using System;
using System.Reflection;
using Autofac;
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
            DynamicAssembly dynamicAssembly = new DynamicAssembly();
            FeignClientTypeBuilder feignClientTypeBuilder = new FeignClientTypeBuilder(dynamicAssembly);
            feignClientTypeBuilder.BuildType(typeof(ITestService)); ;
            feignClientTypeBuilder.Save();

        }

        [TestMethod]
        public void TestMethod2()
        {
            ContainerBuilder containerBuilder = new ContainerBuilder();

            IFeignBuilder feignBuilder = containerBuilder.AddFeignClients(options =>
              {
                  options.Assemblies.Add(typeof(ITestService).Assembly);
                  options.FeignClientPipeline.ReceivingQueryResult();
              });

            IContainer container = containerBuilder.Build();

            using (ILifetimeScope lifetimeScope = container.BeginLifetimeScope())
            {
                ITestService testService = lifetimeScope.Resolve<ITestService>();
                var result = testService.GetQueryResultValue("1", null);
            }



        }

    }
}
