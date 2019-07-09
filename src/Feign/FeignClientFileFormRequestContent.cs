﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Feign
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class FeignClientFileFormRequestContent : FeignClientRequestContent
    {
        public FeignClientFileFormRequestContent(IRequestFileForm requestFileForm)
        {
            RequestFileForm = requestFileForm;
        }
        public IRequestFileForm RequestFileForm { get; }

        public override HttpContent GetHttpContent(MediaTypeHeaderValue contentType)
        {
            if (RequestFileForm == null)
            {
                return null;
            }
            string boundary = contentType?.Parameters.FirstOrDefault(s => s.Name == "boundary")?.Value;
            if (string.IsNullOrWhiteSpace(boundary))
            {
                boundary = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("N")));
            }
            MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent(boundary);
            if (RequestFileForm.RequestFiles != null)
            {
                foreach (var requestFile in RequestFileForm.RequestFiles)
                {
                    HttpContent httpContent = requestFile?.GetHttpContent();
                    if (httpContent != null)
                    {
                        multipartFormDataContent.Add(httpContent);
                    }
                }
            }

            //other property
            foreach (var property in RequestFileForm.GetType().GetProperties())
            {
                if (property.GetMethod == null)
                {
                    continue;
                }
                if (typeof(IRequestFile).IsAssignableFrom(property.PropertyType) || property.PropertyType.IsGenericType && property.PropertyType.GenericTypeArguments.Any(s => typeof(IRequestFile).IsAssignableFrom(s)))
                {
                    continue;
                }
                object value = property.GetValue(RequestFileForm);
                if (value == null)
                {
                    continue;
                }
                HttpContent httpContent = new StringContent(value.ToString());
                multipartFormDataContent.Add(httpContent, property.Name);
            }
            return multipartFormDataContent;
        }
    }
}
