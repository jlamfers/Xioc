using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xioc.Config;
using Xioc.Core;

namespace Sioc.Mvc5
{
   
   public class XiocConfigExtender : IXiocConfigExtender
   {
      public void ExtendSyntax(IList<Tuple<string, MemberInfo>> tuples)
      {
         tuples.Add(Tuple.Create("setup-mvc", Method(Setup1)));
         tuples.Add(Tuple.Create("setup-mvc", Method(Setup2)));
      }

      private static MemberInfo Method(Action<ConfigScriptContext, string[]> setup)
      {
         return setup.Method;
      }

      private static MemberInfo Method(Action<ConfigScriptContext> setup)
      {
         return setup.Method;
      }

      internal static void Setup1(ConfigScriptContext context)
      {
         context.BinderTarget.SetupMvc(AppDomain.CurrentDomain.GetAvailableAssemblies());
      }

      internal static void Setup2(ConfigScriptContext context, string[] assemblyFileNames)
      {
         IList<Assembly> assemblies;
         if (assemblyFileNames != null && !assemblyFileNames.Any())
         {
            assemblies = assemblyFileNames.Select(a => AppDomain.CurrentDomain.EnsureAssemblyIsLoaded(a)).ToList();
         }
         else
         {
            assemblies = AppDomain.CurrentDomain.GetAvailableAssemblies().ToList();
         }
         context.BinderTarget.SetupMvc(assemblies);
      }

   }
}