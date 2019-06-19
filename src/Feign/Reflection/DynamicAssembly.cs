using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Feign.Reflection
{
#if DEBUG&&NET45
    public
#endif
    static class DynamicAssembly
    {
        static AssemblyBuilder _assemblyBuilder;
        static ModuleBuilder _moduleBuilder;
        static string _guid = Guid.NewGuid().ToString("N").ToUpper();

#if DEBUG&&NET45
        public static bool DEBUG_MODE = false;
        public static string AssemblyName = "Feign.Debug.dll";
#endif

        public static AssemblyBuilder AssemblyBuilder
        {
            get
            {
                EnsureAssemblyBuilder();
                return _assemblyBuilder;
            }
        }
        public static ModuleBuilder ModuleBuilder
        {
            get
            {
                EnsureModuleBuilder();
                return _moduleBuilder;
            }
        }
        static void EnsureAssemblyBuilder()
        {
            if (_assemblyBuilder == null)
            {
#if DEBUG&&NET45
                if (DEBUG_MODE)
                {
                    _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(_guid), AssemblyBuilderAccess.RunAndSave);
                }
                else
                {
                    _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(_guid), AssemblyBuilderAccess.Run);
                }
#else
                _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(_guid), AssemblyBuilderAccess.Run);
#endif
            }
        }
        static void EnsureModuleBuilder()
        {
            EnsureAssemblyBuilder();
            if (_moduleBuilder == null)
            {
#if DEBUG&&NET45
                if (DEBUG_MODE)
                {
                    _moduleBuilder = _assemblyBuilder.DefineDynamicModule("MainModule", AssemblyName);
                }
                else
                {
                    _moduleBuilder = _assemblyBuilder.DefineDynamicModule("MainModule");
                }
#else
                _moduleBuilder = _assemblyBuilder.DefineDynamicModule("MainModule");
#endif
            }
        }


        //static readonly IDictionary<Type, TypeBuilder> _proxyTypeMap = new Dictionary<Type, TypeBuilder>();


        //public static TypeBuilder GetProxyTypeBuilder(Type type)
        //{
        //    TypeBuilder typeBuilder;
        //    if (_proxyTypeMap.TryGetValue(type, out typeBuilder))
        //    {
        //        return typeBuilder;
        //    }
        //    try
        //    {
        //        typeBuilder = ModuleBuilder.DefineType(type.Name + "_Proxy_" + _guid,
        //          TypeAttributes.Public |
        //          TypeAttributes.Class |
        //          TypeAttributes.AutoClass |
        //          TypeAttributes.AnsiClass |
        //          TypeAttributes.BeforeFieldInit |
        //          TypeAttributes.AutoLayout,
        //          null);
        //        _proxyTypeMap.Add(type, typeBuilder);
        //    }
        //    catch
        //    {
        //        _proxyTypeMap.TryGetValue(type, out typeBuilder);
        //    }
        //    return typeBuilder;
        //}

    }
}
