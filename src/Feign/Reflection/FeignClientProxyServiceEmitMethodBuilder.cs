﻿using Feign.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Feign.Reflection
{
    class FeignClientProxyServiceEmitMethodBuilder : IMethodBuilder
    {
        protected static readonly MethodInfo ReplacePathVariableMethod = typeof(FeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault(o => o.IsGenericMethod && o.Name == "ReplacePathVariable");

        protected static readonly MethodInfo ReplaceRequestParamMethod = typeof(FeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault(o => o.IsGenericMethod && o.Name == "ReplaceRequestParam");

        protected static readonly MethodInfo ReplaceRequestQueryMethod = typeof(FeignClientProxyService).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault(o => o.IsGenericMethod && o.Name == "ReplaceRequestQuery");

        public void BuildMethod(TypeBuilder typeBuilder, Type parentType, MethodInfo method, FeignClientAttribute feignClientAttribute)
        {
            BuildMethod(typeBuilder, parentType, method, feignClientAttribute, GetRequestMappingAttribute(method));
        }

        protected virtual void BuildMethod(TypeBuilder typeBuilder, Type parentType, MethodInfo method, FeignClientAttribute feignClientAttribute, RequestMappingBaseAttribute requestMapping)
        {
            MethodBuilder methodBuilder = CreateMethodBuilder(typeBuilder, method);
            ILGenerator iLGenerator = methodBuilder.GetILGenerator();
            if (requestMapping == null)
            {
                iLGenerator.Emit(OpCodes.Newobj, typeof(NotSupportedException).GetConstructor(Type.EmptyTypes));
                iLGenerator.Emit(OpCodes.Throw);
                return;
            }
            string uri = requestMapping.Value ?? "";
            LocalBuilder local_Uri = iLGenerator.DeclareLocal(typeof(string));
            LocalBuilder local_OldValue = iLGenerator.DeclareLocal(typeof(string));

            iLGenerator.Emit(OpCodes.Ldstr, uri);
            iLGenerator.Emit(OpCodes.Stloc, local_Uri);

            var invokeMethod = GetInvokeMethod(method, requestMapping);
            bool needRequestBody = NeedRequestBody(invokeMethod, requestMapping);

            ParameterInfo requestBodyParameter = null;
            int requestBodyParameterIndex = -1;

            int index = 1;
            foreach (var parameterInfo in method.GetParameters())
            {
                if (parameterInfo.IsDefined(typeof(RequestBodyAttribute)))
                {
                    if (requestBodyParameter != null)
                    {
                        throw new ArgumentException("最多只能有一个RequestBody", parameterInfo.Name);
                    }
                    requestBodyParameter = parameterInfo;
                    requestBodyParameterIndex = index;
                    continue;
                }
                InvokeReplaceValue(iLGenerator, index, parameterInfo, local_Uri, local_OldValue);
                index++;
            }



            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldloc, local_Uri);
            if (needRequestBody)
            {
                if (requestBodyParameter != null)
                {
                    iLGenerator.Emit(OpCodes.Ldarg_S, requestBodyParameterIndex);
                }
                else
                {
                    iLGenerator.Emit(OpCodes.Ldnull);
                }
            }
            iLGenerator.Emit(OpCodes.Call, invokeMethod);
            iLGenerator.Emit(OpCodes.Ret);
        }


        protected void InvokeReplaceValue(ILGenerator iLGenerator, int index, ParameterInfo parameterInfo, LocalBuilder uri, LocalBuilder value)
        {
            MethodInfo replaceValueMethod;
            string name;
            if (parameterInfo.IsDefined(typeof(RequestParamAttribute)))
            {
                name = parameterInfo.GetCustomAttribute<RequestParamAttribute>().Name ?? parameterInfo.Name;
                replaceValueMethod = ReplaceRequestParamMethod;
            }
            else if (parameterInfo.IsDefined(typeof(RequestQueryAttribute)))
            {
                name = parameterInfo.Name;
                replaceValueMethod = ReplaceRequestQueryMethod;
            }
            else
            {
                name = parameterInfo.IsDefined(typeof(PathVariableAttribute)) ? parameterInfo.GetCustomAttribute<PathVariableAttribute>().Name : parameterInfo.Name;
                replaceValueMethod = ReplacePathVariableMethod;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                name = parameterInfo.Name;
            }

            iLGenerator.Emit(OpCodes.Ldstr, name);
            iLGenerator.Emit(OpCodes.Stloc, value);
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldloc, uri);
            iLGenerator.Emit(OpCodes.Ldloc, value);
            iLGenerator.Emit(OpCodes.Ldarg_S, index);

            replaceValueMethod = replaceValueMethod.MakeGenericMethod(parameterInfo.ParameterType);
            iLGenerator.Emit(OpCodes.Call, replaceValueMethod);
            iLGenerator.Emit(OpCodes.Stloc, uri);
        }


        protected virtual MethodInfo GetInvokeMethod(MethodInfo method, RequestMappingBaseAttribute requestMapping)
        {
            if (method.IsTaskMethod())
            {
                if (method.ReturnType.IsGenericType)
                {
                    return GetInvokeMethod(requestMapping, method.ReturnType.GenericTypeArguments[0], true);
                }
                return GetInvokeMethod(requestMapping, method.ReturnType, true);
            }
            return GetInvokeMethod(requestMapping, method.ReturnType, false);
        }

        protected virtual MethodInfo GetInvokeMethod(RequestMappingBaseAttribute requestMapping, Type returnType, bool async)
        {
            MethodInfo httpClientMethod;
            bool isGeneric = !(returnType == null || returnType == typeof(void) || returnType == typeof(Task));

            if (isGeneric)
            {
                switch (requestMapping.GetMethod()?.ToUpper() ?? "")
                {
                    case "GET":
                        httpClientMethod = async ? FeignClientProxyService.HTTP_GET_ASYNC_GENERIC_METHOD : FeignClientProxyService.HTTP_GET_GENERIC_METHOD;
                        break;
                    case "POST":
                        httpClientMethod = async ? FeignClientProxyService.HTTP_POST_ASYNC_GENERIC_METHOD : FeignClientProxyService.HTTP_POST_GENERIC_METHOD;
                        break;
                    case "PUT":
                        httpClientMethod = async ? FeignClientProxyService.HTTP_PUT_ASYNC_GENERIC_METHOD : FeignClientProxyService.HTTP_PUT_GENERIC_METHOD;
                        break;
                    case "DELETE":
                        httpClientMethod = async ? FeignClientProxyService.HTTP_DELETE_ASYNC_GENERIC_METHOD : FeignClientProxyService.HTTP_DELETE_GENERIC_METHOD;
                        break;
                    default:
                        throw new ArgumentException("httpMethod error");
                }
            }
            else
            {
                switch (requestMapping.GetMethod()?.ToUpper() ?? "")
                {
                    case "GET":
                        httpClientMethod = async ? FeignClientProxyService.HTTP_GET_ASYNC_METHOD : FeignClientProxyService.HTTP_GET_METHOD;
                        break;
                    case "POST":
                        httpClientMethod = async ? FeignClientProxyService.HTTP_POST_ASYNC_METHOD : FeignClientProxyService.HTTP_POST_METHOD;
                        break;
                    case "PUT":
                        httpClientMethod = async ? FeignClientProxyService.HTTP_PUT_ASYNC_METHOD : FeignClientProxyService.HTTP_PUT_METHOD;
                        break;
                    case "DELETE":
                        httpClientMethod = async ? FeignClientProxyService.HTTP_DELETE_ASYNC_METHOD : FeignClientProxyService.HTTP_DELETE_METHOD;
                        break;
                    default:
                        throw new ArgumentException("httpMethod error");
                }
            }

            if (isGeneric)
            {
                return httpClientMethod.MakeGenericMethod(returnType);
            }
            return httpClientMethod;
        }

        protected virtual bool NeedRequestBody(MethodInfo method, RequestMappingBaseAttribute requestMappingBaseAttribute)
        {
            return "POST".Equals(requestMappingBaseAttribute.GetMethod(), StringComparison.OrdinalIgnoreCase) || "PUT".Equals(requestMappingBaseAttribute.GetMethod(), StringComparison.OrdinalIgnoreCase);
            //return method.GetParameters().Length == 2;
        }

        protected RequestMappingBaseAttribute GetRequestMappingAttribute(MethodInfo method)
        {
            if (method.IsDefined(typeof(RequestMappingBaseAttribute)))
            {
                RequestMappingBaseAttribute[] requestMappingBaseAttributes = method.GetCustomAttributes<RequestMappingBaseAttribute>().ToArray();
                if (requestMappingBaseAttributes.Length > 1)
                {
                    throw new ArgumentException(nameof(requestMappingBaseAttributes.Length));
                }
                return requestMappingBaseAttributes[0];
            }
            string methodName = method.Name.ToLower();

            if (methodName.StartsWith("get") || methodName.StartsWith("query") || methodName.StartsWith("select"))
            {
                //get
                return new GetMappingAttribute();
            }
            else if (methodName.StartsWith("post") || methodName.StartsWith("create") || methodName.StartsWith("insert"))
            {
                //post
                return new PostMappingAttribute();
            }
            else if (methodName.StartsWith("put") || methodName.StartsWith("update"))
            {
                //put
                return new PutMappingAttribute();
            }
            else if (methodName.StartsWith("delete") || methodName.StartsWith("remove"))
            {
                //delete
                return new DeleteMappingAttribute();
            }
            return null;
        }


        protected MethodBuilder CreateMethodBuilder(TypeBuilder typeBuilder, MethodInfo method)
        {
            MethodAttributes methodAttributes;
            if (method.IsVirtual)
            {
                methodAttributes = MethodAttributes.Public | MethodAttributes.Virtual;
            }
            else
            {
                methodAttributes = MethodAttributes.Public;
            }
            var arguments = method.GetParameters().Select(a => a.ParameterType).ToArray();
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(method.Name, methodAttributes, CallingConventions.Standard, method.ReturnType, arguments);
            typeBuilder.DefineMethodOverride(methodBuilder, method);
            return methodBuilder;
        }

    }
}
