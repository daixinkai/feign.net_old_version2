using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Feign.Fallback;
using Feign.Tests;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Feign.TestWeb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            //services.AddDiscoveryClient(Configuration);
            services.AddFeignClients(options =>
            {
                options.Assemblies.Add(typeof(ITestService).Assembly);
                options.Lifetime = ServiceLifetime.Singleton;
                options.Lifetime = ServiceLifetime.Scoped;
                options.Lifetime = ServiceLifetime.Transient;
                options.FeignClientPipeline.Authorization(proxy =>
                {
                    return ("global", "asdasd");
                });
                //options.FeignClientPipeline.BuildingRequest += FeignClientPipeline_BuildingRequest;
                options.FeignClientPipeline.Service("yun-platform-service-provider").BuildingRequest += (sender, e) =>
                {

                    IFallbackFeignClient<object> fallbackFeignClient = e.FeignClient as IFallbackFeignClient<object>;

                    var fallback = fallbackFeignClient?.Fallback;

                    if (!e.Headers.ContainsKey("Authorization"))
                    {
                        e.Headers["Authorization"] = "service asdasd";
                    }
                    e.Headers["Accept-Encoding"] = "gzip, deflate, br";

                    //add session
                    e.Headers.Add("cookie", "csrftoken=EGxYkyZeT3DxEsvYsdR5ncmzpi9pmnQx; _bl_uid=nLjRstOyqOejLv2s0xtzqs74Xsmg; courseId=1; versionId=522; textbookId=2598; Hm_lvt_f0984c42ef98965e03c60661581cd219=1559783251,1559818390,1560213044,1560396804; uuid=6a30ff68-2b7c-4cde-a355-2e332b74e31d##1; Hm_lpvt_f0984c42ef98965e03c60661581cd219=1560413345; SESSION=5ee4854d-34b7-423a-9cca-76ddc8a0f111; sid=5ee4854d-34b7-423a-9cca-76ddc8a0f111");

                };

                options.FeignClientPipeline.Service("yun-platform-service-provider").Authorization(proxy =>
                {
                    return ("service", "asdasd");
                });

                options.FeignClientPipeline.SendingRequest += FeignClientPipeline_SendingRequest;


                options.FeignClientPipeline.Service("yun-platform-service-provider").ReceivingResponse += (sender, e) =>
                {

                };

                options.FeignClientPipeline.ReceivingResponse += (sender, e) =>  
                {
                    if (!typeof(QueryResult).IsAssignableFrom(e.ResultType))
                    {
                        return;
                    }
                    if (e.ResultType == typeof(QueryResult))
                    {
                        e.Result = new QueryResult()
                        {
                            StatusCode = e.ResponseMessage.StatusCode
                        };
                        return;
                    }

                    if (e.ResultType.IsGenericType && e.ResultType.GetGenericTypeDefinition() == typeof(QueryResult<>))
                    {
                        QueryResult queryResult;
                        if (e.ResponseMessage.IsSuccessStatusCode)
                        {
                            string json = e.ResponseMessage.Content.ReadAsStringAsync().Result;
                            object data = Newtonsoft.Json.JsonConvert.DeserializeObject(json, e.ResultType.GetGenericArguments()[0]);
                            var constructor = e.ResultType.GetConstructor(new Type[] { typeof(object) });
                            queryResult = (QueryResult)constructor.Invoke(new object[] { data });
                        }
                        else
                        {
                            queryResult = (QueryResult)e.ResultType.GetConstructor(Type.EmptyTypes).Invoke(new object[0]);
                        }

                        queryResult.StatusCode = e.ResponseMessage.StatusCode;

                        e.Result = queryResult;

                    }

                };

                options.FeignClientPipeline.CancelRequest += (sender, e) =>
                {
                    e.CancellationToken.Register((state) =>
                    {

                    }, sender);
                };
                options.FeignClientPipeline.ErrorRequest += (sender, e) =>
                {
                    Exception exception = e.Exception;
                    //e.ExceptionHandled = true;
                };
            })
            //.AddDiscoveryClient();
            ;
        }

        private void FeignClientPipeline_SendingRequest(object sender, SendingRequestEventArgs e)
        {

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
