using System;
using System.Collections.Generic;
using Xioc.Core;

namespace Xioc
{
   public static class WithDependencies
   {
      public static IDictionary<string, object> AddParam(string name, object argument)
      {
         return new Dictionary<string, object> { { name, argument } };
      }
      public static IDictionary<string, object> AddParam(string name, Func<object> argument)
      {
         return AddParam(name, (object)argument);
      }
      public static IDictionary<string, object> AddParam(string name, Func<Context,object> argument)
      {
         return AddParam(name, (object)argument);
      }

      public static IDictionary<string, object> AddParam(this IDictionary<string, object> self, string name, object argument)
      {
         self = self ?? new Dictionary<string, object>();
         self.Add(name, argument);
         return self;
      }
      public static IDictionary<string, object> AddParam(this IDictionary<string, object> self, string name,Func<object> argument)
      {
         return self.AddParam(name, (object)argument);
      }
      public static IDictionary<string, object> AddParam(this IDictionary<string, object> self, string name, Func<Context,object> argument)
      {
         return self.AddParam(name, (object)argument);
      }
   }
}
