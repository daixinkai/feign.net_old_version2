using Feign.Standalone;
using Feign.Tests;
using System;

namespace Feign.TestsConsole
{
    class Program
    {



        static void Main(string[] args)
        {
            InitFeignClients();

            Test1();

            Console.WriteLine("Hello World!");
            Console.ReadKey();
        }


        static void Test1()
        {
            ITestService testService = FeignClients.Get<ITestService>();

            Console.WriteLine(testService.GetType().FullName);

        }

        static void InitFeignClients()
        {
            FeignClients.AddFeignClients(options =>
            {
                options.Assemblies.Add(typeof(ITestService).Assembly);
                options.FeignClientPipeline.ReceivingQueryResult();
            });            
        }

    }
}
