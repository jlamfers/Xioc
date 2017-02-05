using System;
using System.Collections.Generic;
using System.Reflection;
using Xioc.Config;

namespace Xioc.WebApi2
{
   public class XiocConfigExtender : IXiocConfigExtender
   {
      public void ExtendSyntax(IList<Tuple<string, MemberInfo>> tuples)
      {
         tuples.Add(Tuple.Create("setup-webapi", Method(Setup)));
      }

      private static MemberInfo Method(Action<ConfigScriptContext> setup)
      {
         return setup.Method;
      }

      internal static void Setup(ConfigScriptContext context)
      {
         context.BinderTarget.SetupWebApi(null, null);
      }

   }
}