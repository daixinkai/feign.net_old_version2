﻿using Feign.Formatting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Feign
{
    public class FeignOptions : IFeignOptions
    {
        public FeignOptions()
        {
            Assemblies = new List<Assembly>();
            Converters = new ConverterCollection();
            Converters.AddConverter(new ObjectStringConverter());
            MediaTypeFormatters = new MediaTypeFormatterCollection();
            MediaTypeFormatters.AddFormatter(new JsonMediaTypeFormatter());
            MediaTypeFormatters.AddFormatter(new FormUrlEncodedMediaTypeFormatter());
            MediaTypeFormatters.AddFormatter(new MultipartFormDataMediaTypeFormatter());
            MediaTypeFormatters.AddFormatter(new XmlMediaTypeFormatter());
            MediaTypeFormatters.AddFormatter(new XmlMediaTypeFormatter() { MediaType = "text/xml" });
            FeignClientPipeline = new GlobalFeignClientPipeline();
            Lifetime = FeignClientLifetime.Transient;
        }
        public IList<Assembly> Assemblies { get; }
        public ConverterCollection Converters { get; }
        public MediaTypeFormatterCollection MediaTypeFormatters { get; }
        public IGlobalFeignClientPipeline FeignClientPipeline { get; }
        public FeignClientLifetime Lifetime { get; set; }
        public bool IncludeMethodMetadata { get; set; }

    }
}
