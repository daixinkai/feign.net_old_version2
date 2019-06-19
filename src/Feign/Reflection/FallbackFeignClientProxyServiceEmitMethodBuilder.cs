using Feign.Internal;
using Feign.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Feign.Reflection
{
    class FallbackFeignClientProxyServiceEmitMethodBuilder : FeignClientProxyServiceEmitMethodBuilder
    {


        protected override MethodInfo GetInvokeMethod(RequestMappingBaseAttribute requestMapping, Type returnType, bool async)
        {
            MethodInfo httpClientMethod;
            bool isGeneric = !(returnType == null || returnType == typeof(void) || returnType == typeof(Task));

            if (isGeneric)
            {
                switch (requestMapping.GetMethod()?.ToUpper() ?? "")
                {
                    case "GET":
                        httpClientMethod = async ? FallbackFeignClientProxyService.HTTP_GET_ASYNC_GENERIC_METHOD_FALLBACK : FallbackFeignClientProxyService.HTTP_GET_GENERIC_METHOD_FALLBACK;
                        break;
                    case "POST":
                        httpClientMethod = async ? FallbackFeignClientProxyService.HTTP_POST_ASYNC_GENERIC_METHOD_FALLBACK : FallbackFeignClientProxyService.HTTP_POST_GENERIC_METHOD_FALLBACK;
                        break;
                    case "PUT":
                        httpClientMethod = async ? FallbackFeignClientProxyService.HTTP_PUT_ASYNC_GENERIC_METHOD_FALLBACK : FallbackFeignClientProxyService.HTTP_PUT_GENERIC_METHOD_FALLBACK;
                        break;
                    case "DELETE":
                        httpClientMethod = async ? FallbackFeignClientProxyService.HTTP_DELETE_ASYNC_GENERIC_METHOD_FALLBACK : FallbackFeignClientProxyService.HTTP_DELETE_GENERIC_METHOD_FALLBACK;
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
                        httpClientMethod = async ? FallbackFeignClientProxyService.HTTP_GET_ASYNC_METHOD_FALLBACK : FallbackFeignClientProxyService.HTTP_GET_METHOD_FALLBACK;
                        break;
                    case "POST":
                        httpClientMethod = async ? FallbackFeignClientProxyService.HTTP_POST_ASYNC_METHOD_FALLBACK : FallbackFeignClientProxyService.HTTP_POST_METHOD_FALLBACK;
                        break;
                    case "PUT":
                        httpClientMethod = async ? FallbackFeignClientProxyService.HTTP_PUT_ASYNC_METHOD_FALLBACK : FallbackFeignClientProxyService.HTTP_PUT_METHOD_FALLBACK;
                        break;
                    case "DELETE":
                        httpClientMethod = async ? FallbackFeignClientProxyService.HTTP_DELETE_ASYNC_METHOD_FALLBACK : FallbackFeignClientProxyService.HTTP_DELETE_METHOD_FALLBACK;
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

        //protected override bool NeedRequestBody(MethodInfo method, RequestMappingBaseAttribute requestMappingBaseAttribute)
        //{
        //    return method.GetParameters().Length == 3;
        //}

        protected override void BuildMethod(TypeBuilder typeBuilder, Type parentType, MethodInfo method, FeignClientAttribute feignClientAttribute, RequestMappingBaseAttribute requestMapping)
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

            // fallback
            LocalBuilder fallbackDelegate = DefineFallbackDelegate(typeBuilder, methodBuilder, iLGenerator, parentType, method);
            iLGenerator.Emit(OpCodes.Ldloc, fallbackDelegate);
            iLGenerator.Emit(OpCodes.Call, invokeMethod);
            iLGenerator.Emit(OpCodes.Ret);
        }


        LocalBuilder DefineFallbackDelegate(TypeBuilder typeBuilder, MethodBuilder methodBuilder, ILGenerator iLGenerator, Type parentType, MethodInfo method)
        {
            Type delegateType;
            if (method.ReturnType == null || method.ReturnType == typeof(void))
            {
                delegateType = typeof(Action);
            }
            else
            {
                delegateType = typeof(Func<>).MakeGenericType(method.ReturnType);
            }

            int bindingFlagsValue = 0;
            foreach (BindingFlags item in Enum.GetValues(typeof(BindingFlags)))
            {
                bindingFlagsValue += item.GetHashCode();
            }
            var delegateConstructor = delegateType.GetConstructors((BindingFlags)bindingFlagsValue)[0];
            LocalBuilder invokeDelegate = iLGenerator.DeclareLocal(delegateType);
            // if has parameters
            if (method.GetParameters().Length > 0)
            {
                //var anonymousMethodClassTypeBuild = AnonymousMethodClassBuilder.BuildType(typeBuilder, methodBuilder, method.GetParameters());
                var anonymousMethodClassTypeBuild = AnonymousMethodClassBuilder.BuildType(typeBuilder, method);
                // new anonymousMethodClass
                LocalBuilder anonymousMethodClass = iLGenerator.DeclareLocal(anonymousMethodClassTypeBuild.Item1);
                //field
                iLGenerator.Emit(OpCodes.Ldarg_0); //this
                iLGenerator.Emit(OpCodes.Call, parentType.GetProperty("Fallback").GetMethod); //.Fallback
                for (int i = 1; i <= method.GetParameters().Length; i++)
                {
                    iLGenerator.Emit(OpCodes.Ldarg_S, i);
                }
                iLGenerator.Emit(OpCodes.Newobj, anonymousMethodClassTypeBuild.Item2);
                iLGenerator.Emit(OpCodes.Stloc, anonymousMethodClass);
                iLGenerator.Emit(OpCodes.Ldloc, anonymousMethodClass);
                iLGenerator.Emit(OpCodes.Ldftn, anonymousMethodClassTypeBuild.Item3);
            }
            else
            {
                iLGenerator.Emit(OpCodes.Ldarg_0); //this
                iLGenerator.Emit(OpCodes.Call, parentType.GetProperty("Fallback").GetMethod); //.Fallback
                iLGenerator.Emit(OpCodes.Ldftn, method);
            }

            iLGenerator.Emit(OpCodes.Newobj, delegateConstructor);
            iLGenerator.Emit(OpCodes.Stloc, invokeDelegate);

            return invokeDelegate;
        }


    }

}