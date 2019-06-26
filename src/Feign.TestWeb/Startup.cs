using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Feign.Fallback;
using Feign.Tests;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Steeltoe.Discovery.Client;

namespace Feign.TestWeb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Type[] types = new Type[5];
            types[0] = null;
            var method = MethodInfo.GetCurrentMethod();
            Type t = typeof(int);
            Configuration = configuration;
        }

        public Type TestType
        {
            get
            {
                return typeof(int);
            }
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            //services.AddDiscoveryClient(Configuration);
            services.AddDiscoveryClient(Configuration);
            services.AddFeignClients()
                .AddTestFeignClients()
                .AddSteeltoeServiceDiscovery()
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
            app.UseDiscoveryClient();
        }
    }
}
