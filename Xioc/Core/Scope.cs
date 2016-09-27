using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xioc.Core.Internal;

namespace Xioc.Core
{
   /// <summary>
   /// <remarks>
   /// </remarks>
   /// </summary>
   [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly",Justification = "Dispose is fully handled by base class")]
   public sealed class Scope : XiocContainer, IScope
   {

      internal Scope(XiocContainer parent, Action<IBinder> binder)
         : base(parent)
      {
         var p = parent as Scope;
         ScopeLevel = p == null ? 1 : p.ScopeLevel + 1;

         if (binder == null && parent.Kernel.OnNewScopeBinder == null)
         {
            Kernel = parent.Kernel;
         }
         else
         {
            var kernel = new ChildKernel(parent.Kernel, this);
            if (parent.Kernel.OnNewScopeBinder != null)
            {
               parent.Kernel.OnNewScopeBinder(kernel);
            }
            if (binder != null)
            {
               binder(kernel);
            }
            Kernel = kernel.Bindings.Any() ? kernel : parent.Kernel;
         }

      }

      public object Resolve(Type serviceType, bool throwException = true)
      {
         if (serviceType == null) throw new ArgumentNullException("serviceType");
         return Resolve(new Context(this, serviceType), throwException);
      }
      public IEnumerable<object> ResolveAll(Type serviceType)
      {
         if (serviceType == null) throw new ArgumentNullException("serviceType");
         var context = new Context(this, serviceType);
         return ResolveAll(context);
      }

      public int ScopeLevel { get; internal set; }

      internal object Resolve(Context context, bool throwException = true)
      {
         EnsureNotDisposed();
         var binding = Kernel.TryGetBinding(context.RequestedType, false);
         if (binding != null)
         {
            return binding.GetInstance(context);
         }
         if (throwException)
         {
            throw new XiocException("Unable to resolve type " + context.RequestedType);
         }
         return null;
      }
      internal IEnumerable<object> ResolveAll(Context context)
      {
         EnsureNotDisposed();
         return Kernel.GetBindings(context.RequestedType).Select(b => b.GetInstance(context));
      }

      internal object GetOrCreatePerContainer(Context context, Func<Context, object> factory)
      {
         EnsureNotDisposed();
         var binding = context.Binding;

         return context.UnManaged
             ? factory(context)
             : Root.PerScopeInstances.GetOrAdd(binding, b => factory(!context.Singleton ? context.Clone(c => c.Singleton = true) : context));
      }

      internal object GetOrCreatePerScope(Context context, Func<Context, object> factory)
      {
         EnsureNotDisposed();
         var binding = context.Binding;



         if (context.UnManaged)
         {
            return factory(context);
         }

         return context.Singleton
             ? CreateTransient(context, factory)
             : PerScopeInstances.GetOrAdd(binding, b => factory(context));

      }

      internal object CreateTransient(Context context, Func<Context, object> factory)
      {
         EnsureNotDisposed();

         if (context.UnManaged)
         {
            return factory(context);
         }

         var instance = factory(context);
         var disposable = instance as IDisposable;
         if (disposable == null)
         {
            return instance;
         }

         if (context.Singleton)
         {
            Root.TransientDisposableInstances.Add(disposable);
         }
         else
         {
            TransientDisposableInstances.Add(disposable);
         }
         return instance;

      }

      /// <summary>
      /// Convenience method that executes an action, it wraps BeginInvoke.
      /// Applying the 'await' operator on calling is not required, since the scope
      /// never is disposed before the the last async execution is completed
      /// </summary>
      /// <param name="action"></param>
      /// <returns></returns>
      public async Task ExecuteAsync(Action action)
      {
         var task = (Task)BeginInvoke(action, new object[0]);
         await task;
      }

      #region ISynchronizeInvoke
      public IAsyncResult BeginInvoke(Delegate method, object[] args)
      {
         return ASyncManager.BeginInvoke(method, args);
      }

      public object EndInvoke(IAsyncResult result)
      {
         return ASyncManager.EndInvoke(result);
      }

      public object Invoke(Delegate method, object[] args)
      {
         return ASyncManager.Invoke(method,args);
      }

      public bool InvokeRequired { get { return ASyncManager.InvokeRequired; } }
      #endregion
   }
}
