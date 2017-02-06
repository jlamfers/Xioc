
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
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Xioc.Config.Common
{
   internal static class BuildHelper
   {
      private static Assembly _entryAssembly;

      public static bool IsDebug()
      {
         _entryAssembly = _entryAssembly ?? (Assembly.GetEntryAssembly() ?? GetEntryAssemblyFallback());
         var attributes = _entryAssembly.GetCustomAttributes<DebuggableAttribute>().ToArray();
         return attributes.Any() && (attributes.First().IsJITTrackingEnabled);
      }

      private static Assembly GetEntryAssemblyFallback()
      {
         var methodFrames = new StackTrace().GetFrames().Select(t => t.GetMethod()).ToArray();
         MethodBase entryMethod = null;
         var firstInvokeMethod = 0;
         for (var i = 0; i < methodFrames.Length; i++)
         {
            var method = methodFrames[i] as MethodInfo;
            if (method == null)
               continue;
            if (method.Name == "Main" && method.ReturnType == typeof(void))
               entryMethod = method;
            else if (firstInvokeMethod == 0 && method.Name == "InvokeMethod" && method.IsStatic && method.DeclaringType == typeof(RuntimeMethodHandle))
               firstInvokeMethod = i;
         }

         if (entryMethod == null)
         {
            entryMethod = firstInvokeMethod != 0 ? methodFrames[firstInvokeMethod - 1] : methodFrames.Last();
         }

         return entryMethod.Module.Assembly;
      }

   }
}
