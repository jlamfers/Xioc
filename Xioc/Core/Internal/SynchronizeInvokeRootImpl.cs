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
using System.ComponentModel;
using System.Threading.Tasks;

namespace Xioc.Core.Internal
{
   internal class SynchronizeInvokeRootImpl : ISynchronizeInvoke
   {
      public IAsyncResult BeginInvoke(Delegate method, object[] args)
      {
         return Task.Run(() => Run(method, args));
      }

      public object EndInvoke(IAsyncResult result)
      {
         if (result.IsCompleted || result.CompletedSynchronously || result.AsyncWaitHandle == null) return result.AsyncState;
         result.AsyncWaitHandle.WaitOne();
         return result.AsyncState;
      }

      public object Invoke(Delegate method, object[] args)
      {
         return method.DynamicInvoke(args);
      }

      public bool InvokeRequired { get { return false; }}

      private static object Run(Delegate method, object[] args)
      {
         if (args != null && args.Length != 0) return method.DynamicInvoke(args);
         var action = method as Action;
         if (action != null)
         {
            action();
            return null;
         }
         var func = method as Func<object>;
         return func != null ? func() : method.DynamicInvoke();
      }
   }
}