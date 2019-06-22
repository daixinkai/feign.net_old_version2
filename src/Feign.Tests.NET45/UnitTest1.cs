using System;
using System.Reflection;
using Autofac;
using Castle.Windsor;
using Feign.Cache;
using Feign.Logging;
using Feign.Reflection;
using Feign.Standalone;
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
            dynamicAssembly.DEBUG_MODE = true;
            FeignClientTypeBuilder feignClientTypeBuilder = new FeignClientTypeBuilder(dynamicAssembly);
            feignClientTypeBuilder.BuildType(typeof(ITestService)); ;
            feignClientTypeBuilder.Save();
        }

        [TestMethod]
        public void TestMethod_Autofac()
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

        [TestMethod]
        public void TestMethod_CastleWindsor()
        {
            IWindsorContainer windsorContainer = new WindsorContainer();
            windsorContainer.AddFeignClients(options =>
            {
                options.Assemblies.Add(typeof(ITestService).Assembly);
                options.FeignClientPipeline.ReceivingQueryResult();
            })
                .AddLoggerFactory<DefaultLoggerFactory>()
            ;
            ITestService testService = windsorContainer.Resolve<ITestService>();
            Assert.IsNotNull(testService);
            var result = testService.GetQueryResultValue("", null);
        }

        [TestMethod]
        public void TestMethod_Standalone()
        {
            FeignClients.AddFeignClients(options =>
            {
                options.Assemblies.Add(typeof(ITestService).Assembly);
                options.FeignClientPipeline.ReceivingQueryResult();
            });
            ITestService testService = FeignClients.Get<ITestService>();
            Assert.IsNotNull(testService);
            var result = testService.GetQueryResultValue("", null);
        }

    }
}
