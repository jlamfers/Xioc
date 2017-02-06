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
