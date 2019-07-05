﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
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
            bool b = IsPrime(10111);
            Assert.IsTrue(b);
            FeignClients.AddFeignClients(options =>
            {
                options.Assemblies.Add(typeof(ITestService).Assembly);
                options.FeignClientPipeline.ReceivingQueryResult();
            });
            ITestService testService = FeignClients.Get<ITestService>();

            Assert.IsNotNull(testService);
            var result = testService.GetQueryResultValue("", null);
        }


        [TestMethod]
        public void TestCustomOrderBy()
        {
            List<QueryResult> queryResults = new List<QueryResult>();

            queryResults.Add(new QueryResult()
            {
                StatusCode = System.Net.HttpStatusCode.Accepted
            });
            queryResults.Add(new QueryResult()
            {
                StatusCode = System.Net.HttpStatusCode.Ambiguous
            });
            queryResults.Add(new QueryResult()
            {
                StatusCode = System.Net.HttpStatusCode.BadGateway
            });

            queryResults.Add(new QueryResult()
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest
            });
            queryResults.Add(new QueryResult()
            {
                StatusCode = System.Net.HttpStatusCode.Conflict
            });
            queryResults.Add(new QueryResult()
            {
                StatusCode = System.Net.HttpStatusCode.Continue
            });
            var query = queryResults.AsQueryable();

            var rr = query.OrderBy(s => s.StatusCode).OrderBy("StatusCode", true, true).ToList();

        }


        public static bool IsPrime(int number)
        {
            int times = 0;
            for (int i = 2; Math.Pow(i, 2) < number; ++i)
            {
                times++;
                if (number % i == 0)
                {
                    return false;
                }
            }
            Console.WriteLine(times);
            return true;
        }

    }


    public static class TestExtensions
    {
        public static IOrderedQueryable<TEntity> OrderBy<TEntity>(this IOrderedQueryable<TEntity> source, string orderByProperty, bool desc, bool then)
        {
            var command = (then ? "Then" : "Order") + (desc ? "ByDescending" : "By");

            var entityType = typeof(TEntity);
            var entityParameter = Expression.Parameter(entityType, "x");

            var property = entityType.GetProperty(orderByProperty);

            var propertyAccess = Expression.MakeMemberAccess(entityParameter, property);
            var orderByExpression = Expression.Lambda(propertyAccess, entityParameter);

            var resultExpression =
                Expression.Call(typeof(Queryable), command, new Type[] { entityType, property.PropertyType }, source.Expression, Expression.Quote(orderByExpression));

            return (IOrderedQueryable<TEntity>)source.Provider.CreateQuery<TEntity>(resultExpression);
        }
    }


}
