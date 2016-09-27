using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Xioc.Core.Internal
{
   internal class ASyncManager : ISynchronizeInvoke
   {
      private Action _disposeCallback;
      private readonly object _syncRoot = new object();
      private int _invocationCount;
      private bool _disposed,_disposeStarted;

      public ASyncManager(Action disposeCallback)
      {
         if (disposeCallback == null) throw new ArgumentNullException("disposeCallback");
         _disposeCallback = disposeCallback;
      }

      /// <summary>
      /// BeginInvoke returns a task
      /// </summary>
      /// <param name="method"></param>
      /// <param name="args"></param>
      /// <returns>a Task that runs the passed method</returns>
      public IAsyncResult BeginInvoke(Delegate method, object[] args)
      {
         lock (_syncRoot)
         {
            if (_disposeStarted)
            {
               throw new ObjectDisposedException(GetType().FullName, "ASyncManager is disposed");
            }
            _invocationCount++;
         }
         return Task.Run(() =>
         {
            try
            {
               return Run(method, args);
            }
            finally
            {
               Action disposeHandler = null;
               lock(_syncRoot)
               {
                  _invocationCount--;
                  if (_disposeStarted && _invocationCount == 0)
                  {
                     disposeHandler = _disposeCallback;
                     _disposeCallback = null;
                  }
               }
               if (disposeHandler != null)
               {
                  disposeHandler();
               }
            }
         });
      }

      public object EndInvoke(IAsyncResult result)
      {
         if (result.IsCompleted || result.CompletedSynchronously || result.AsyncWaitHandle ==null) return result.AsyncState;
         result.AsyncWaitHandle.WaitOne();
         return result.AsyncState;
      }

      public object Invoke(Delegate method, object[] args)
      {
         return Run(method, args);
      }

      public bool InvokeRequired { get { return false; } }

      public void BeginDispose()
      {
         Action disposeHandler = null;
         lock (_syncRoot)
         {
            if (_disposeStarted) return;
            _disposeStarted = true;
            if (_invocationCount == 0)
            {
               _disposed = true;
               disposeHandler = _disposeCallback;
               _disposeCallback = null;
            }
         }
         if (disposeHandler != null)
         {
            disposeHandler();
         }
      }

      public bool IsDisposed
      {
         get
         {
            lock (_syncRoot)
            {
               return _disposed;
            }
         }
      }

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
