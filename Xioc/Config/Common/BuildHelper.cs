

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
