#region  License
/*
Copyright 2017 - Jaap Lamfers - jlamfers@xipton.net

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 * */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xioc.Config;
using Xioc.Core;

namespace Xioc.Mvc5
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