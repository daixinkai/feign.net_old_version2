using Feign.Formatting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Feign
{
    public class FeignOptions : FeignOptionsBase
    {
        public FeignOptions()
        {
            Assemblies = new List<Assembly>();
            Lifetime = ServiceLifetime.Transient;
        }
        public IList<Assembly> Assemblies { get; }
        /// <summary>
        /// default <see cref="ServiceLifetime.Transient"/>
        /// </summary>
        public ServiceLifetime Lifetime { get; set; }
    }
}
