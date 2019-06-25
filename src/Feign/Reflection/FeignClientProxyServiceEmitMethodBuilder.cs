using Feign.Proxy;
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

        public void BuildMethod(TypeBuilder typeBuilder, Type serviceType, MethodInfo method, FeignClientAttribute feignClientAttribute)
        {
            BuildMethod(typeBuilder, serviceType, method, feignClientAttribute, GetRequestMappingAttribute(method));
        }

        protected virtual void BuildMethod(TypeBuilder typeBuilder, Type serviceType, MethodInfo method, FeignClientAttribute feignClientAttribute, RequestMappingBaseAttribute requestMapping)
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
            EmitRequestContent emitRequestContent = EmitParameter(iLGenerator, method, local_Uri, local_OldValue);
            if (emitRequestContent.RequestContent != null && !SupportRequestContent(invokeMethod, requestMapping))
            {
                throw new NotSupportedException("不支持RequestBody或者RequestForm");
            }
            iLGenerator.Emit(OpCodes.Ldarg_0);  //this
            // baseUrl
            EmitBaseUrl(iLGenerator);
            //mapping uri
            if (requestMapping.Value == null)
            {
                iLGenerator.Emit(OpCodes.Ldnull);
            }
            else
            {
                iLGenerator.Emit(OpCodes.Ldstr, requestMapping.Value);
            }
            iLGenerator.Emit(OpCodes.Ldloc, local_Uri); //uri
            iLGenerator.Emit(OpCodes.Ldstr, requestMapping.GetMethod()); //method
            EmitContentType(iLGenerator, serviceType, requestMapping, emitRequestContent); // contentType
            //content
            if (emitRequestContent.Content != null)
            {
                iLGenerator.Emit(OpCodes.Ldarg_S, emitRequestContent.RequestContentIndex);
            }
            else
            {
                iLGenerator.Emit(OpCodes.Ldnull);
            }
            iLGenerator.Emit(OpCodes.Newobj, typeof(FeignClientRequest).GetConstructors()[0]);
            iLGenerator.Emit(OpCodes.Call, invokeMethod);
            iLGenerator.Emit(OpCodes.Ret);
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
                httpClientMethod = async ? FeignClientProxyService.HTTP_SEND_ASYNC_GENERIC_METHOD : FeignClientProxyService.HTTP_SEND_GENERIC_METHOD;
            }
            else
            {
                httpClientMethod = async ? FeignClientProxyService.HTTP_SEND_ASYNC_METHOD : FeignClientProxyService.HTTP_SEND_METHOD;
            }
            if (isGeneric)
            {
                return httpClientMethod.MakeGenericMethod(returnType);
            }
            return httpClientMethod;
        }

        protected virtual bool SupportRequestContent(MethodInfo method, RequestMappingBaseAttribute requestMappingBaseAttribute)
        {
            return "POST".Equals(requestMappingBaseAttribute.GetMethod(), StringComparison.OrdinalIgnoreCase) || "PUT".Equals(requestMappingBaseAttribute.GetMethod(), StringComparison.OrdinalIgnoreCase);
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
                //methodAttributes = MethodAttributes.Public | MethodAttributes.Virtual;
                methodAttributes = 
                    MethodAttributes.Public
                    | MethodAttributes.HideBySig
                    | MethodAttributes.NewSlot
                    | MethodAttributes.Virtual
                    | MethodAttributes.Final;
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

        protected void EmitBaseUrl(ILGenerator iLGenerator)
        {
            PropertyInfo propertyInfo = typeof(FeignClientProxyService).GetProperty("BaseUrl", BindingFlags.Instance | BindingFlags.NonPublic);
            iLGenerator.Emit(OpCodes.Ldarg_0); //this
            iLGenerator.Emit(OpCodes.Callvirt, propertyInfo.GetMethod);
        }

        protected void EmitContentType(ILGenerator iLGenerator, Type serviceType, RequestMappingBaseAttribute requestMapping, EmitRequestContent requestContent)
        {
            string contentType = requestMapping.ContentType;
            if (string.IsNullOrWhiteSpace(contentType) && serviceType.IsDefined(typeof(RequestMappingAttribute)))
            {
                contentType = serviceType.GetCustomAttribute<RequestMappingAttribute>().ContentType;
            }
            if (contentType == null)
            {
                iLGenerator.Emit(OpCodes.Ldnull);
            }
            else
            {
                iLGenerator.Emit(OpCodes.Ldstr, contentType);
            }
        }

        protected EmitRequestContent EmitParameter(ILGenerator iLGenerator, MethodInfo method, LocalBuilder uri, LocalBuilder value)
        {
            EmitRequestContent emitRequestContent = new EmitRequestContent();
            int index = 1;
            foreach (var parameterInfo in method.GetParameters())
            {
                if (parameterInfo.IsDefined(typeof(RequestBodyAttribute)))
                {
                    if (emitRequestContent.RequestContent != null)
                    {
                        throw new ArgumentException("最多只能有一个RequestBody或者RequestForm", parameterInfo.Name);
                    }
                    emitRequestContent.Content = parameterInfo;
                    emitRequestContent.RequestContent = parameterInfo.GetCustomAttribute<RequestBodyAttribute>();
                    emitRequestContent.RequestContentIndex = index;
                    continue;
                }
                if (parameterInfo.IsDefined(typeof(RequestFormAttribute)))
                {
                    if (emitRequestContent.RequestContent != null)
                    {
                        throw new ArgumentException("最多只能有一个RequestBody或者RequestForm", parameterInfo.Name);
                    }
                    emitRequestContent.Content = parameterInfo;
                    emitRequestContent.RequestContent = parameterInfo.GetCustomAttribute<RequestFormAttribute>();
                    emitRequestContent.RequestContentIndex = index;
                    continue;
                }
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
                index++;
            }
            return emitRequestContent;
        }

    }
}
