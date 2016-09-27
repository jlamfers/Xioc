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