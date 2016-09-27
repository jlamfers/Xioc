using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Xioc.Core;
using Xioc.Core.Internal;

namespace Xioc
{
   /// <summary>
   /// The NiocContainer holds the root kernel. You cannot resolve instances from the container.
   /// To resolve instances you first need to create a scope using BeginScope(). Then you can
   /// resolve instances from the returned scope object. 
   /// You must dispose a scope on the end of its lifetime.
   /// </summary>
   public class XiocContainer : IContainer
   {
      private interface ILazyResolver
      {
         object Resolve(Context context);
      }
      private class LazyResolver<T> : ILazyResolver
      {
         public object Resolve(Context context)
         {
            return new Lazy<T>(() => context.Scope.Resolve<T>());
         }
      }

      internal readonly ASyncManager ASyncManager;

      internal IKernel Kernel;

      internal ConcurrentDictionary<Binding, object>
          PerScopeInstances = new ConcurrentDictionary<Binding, object>();

      private ConditionalWeakTable<object, IScope>
          _scopedInstancesAtRoot;

      internal IDisposablesBag
          TransientDisposableInstances;

      private readonly object
          _syncRoot = new object();

      ~XiocContainer()
      {
         Dispose(false);
      }

      public XiocContainer(Action<IBinder> binder, ISettings settings = null)
      {
         if (binder == null) throw new ArgumentNullException("binder");
         ASyncManager = new ASyncManager(() => Dispose(true));
         _scopedInstancesAtRoot = new ConditionalWeakTable<object, IScope>();

         Settings = new ReadOnlySettings(settings ?? new Settings());
         TransientDisposableInstances = Settings.EnableWeakDisposableTracking ? (IDisposablesBag)new WeakDisposablesBag() : new DisposablesBag();
         Kernel = new Kernel(this);

         // bind Lazy<> by default
         var lazyResolverLookup = new ConcurrentDictionary<Type, ILazyResolver>();
         Kernel.Bind(typeof(Lazy<>),
             c =>
                 lazyResolverLookup.GetOrAdd(c.RequestedType.GetGenericArguments()[0], t => typeof(LazyResolver<>).MakeGenericType(t).CreateInstance<ILazyResolver>())
                 .Resolve(c));

         Kernel.Bind<IContainer>(c => ((Scope)c.Scope).Root, Lifestyle.UnManaged);
         Kernel.Bind<ISynchronizeInvoke, SynchronizeInvokeRootImpl>();
         Kernel.Bind<IASyncManager, ASyncManagerRootImpl>();

         Kernel.OnNewScope(level: 1, 
            binder: b =>
            {
               b.Bind<ISynchronizeInvoke>(c => c.Scope, Lifestyle.UnManaged);
               b.Bind<IASyncManager>(c => c.Scope, Lifestyle.UnManaged);
               b.Bind<IContainer>(c => c.Scope, Lifestyle.UnManaged);
            }
         );

         binder(Kernel);

         Root = this;

      }

      internal XiocContainer(XiocContainer parent)
      {
         if (parent == null) throw new ArgumentNullException("parent");
         ASyncManager = new ASyncManager(() => Dispose(true));
         Parent = parent;
         Root = parent.Root;
         Settings = parent.Settings;
         TransientDisposableInstances = parent.TransientDisposableInstances.GetType().CreateInstance<IDisposablesBag>();
         // rest of initialization in subclass
      }

      public IScope BeginScope(Action<IBinder> binder = null)
      {
         if (binder != null && !Settings.EnableScopeLevelBindings)
         {
            throw new XiocException("Scope level bindings are disabled by Settings. You are not allowed to make additional scope level bindings. Argument [binder] must be null");
         }
         return new Scope(this, binder);
      }

      public XiocContainer Root { get; private set; }
      public XiocContainer Parent { get; private set; }
      public ISettings Settings { get; private set; }

      public object ResolveScoped(Type serviceType, bool throwException = true)
      {
         EnsureNotDisposed();
         var scope = BeginScope();
         var instance = scope.Resolve(serviceType, throwException);
         if (instance == null)
         {
            // no need to register for release
            return null;
         }
         var lifestyle = scope.GetBinding(serviceType).Lifestyle;
         if (lifestyle == Lifestyle.PerContainer || lifestyle == Lifestyle.UnManaged)
         {
            // no need to register for release
            return instance;
         }
         lock (Root._scopedInstancesAtRoot)
         {
            Root._scopedInstancesAtRoot.Add(instance, scope);
         }
         return instance;
      }
      public bool ReleaseScoped(object instance)
      {
         EnsureNotDisposed();
         IScope scope;
         lock (Root._scopedInstancesAtRoot)
         {
            if (!Root._scopedInstancesAtRoot.TryGetValue(instance, out scope))
            {
               return false;
            }
            Root._scopedInstancesAtRoot.Remove(instance);
         }
         scope.Dispose();
         return true;
      }


      public Binding GetBinding(Type type, bool throwException = true)
      {
         if (type == null) throw new ArgumentNullException("type");
         EnsureNotDisposed();
         return Kernel.GetBinding(type, throwException);
      }

      public IEnumerable<Binding> GetBindings(Type type)
      {
         if (type == null) throw new ArgumentNullException("type");
         EnsureNotDisposed();
         return Kernel.GetBindings(type);
      }

      public IEnumerable<Type> GetServiceTypes()
      {
         return Kernel.GetServiceTypes();
      }

      public bool IsRegistered(Type type)
      {
         if (type == null) throw new ArgumentNullException("type");
         EnsureNotDisposed();
         return Kernel.IsRegistered(type);
      }

      public bool CanResolve(Type type)
      {
         if (type == null) throw new ArgumentNullException("type");
         EnsureNotDisposed();
         return Kernel.CanResolve(type);
      }

      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Dispose(true) is called by disposeCallback from asyncManager")]
      public void Dispose()
      {
         if (ASyncManager.IsDisposed) return;
         ASyncManager.BeginDispose();
      }

      internal int KernelStackSize { get { return Kernel.StackSize; } }

      protected virtual void Dispose(bool disposing)
      {
         if (!disposing) return;

         ConcurrentDictionary<Binding, object> perScopeInstances;
         IDisposablesBag disposables;

         lock (_syncRoot)
         {
            perScopeInstances = PerScopeInstances;
            disposables = TransientDisposableInstances;

            PerScopeInstances = null;
            TransientDisposableInstances = null;
            _scopedInstancesAtRoot = null;
         }

         if (disposables == null && perScopeInstances == null)
         {
            return;
         }

         if (disposables != null)
         {
            disposables.Dispose();
         }

         if (perScopeInstances != null)
         {
            foreach (var d in perScopeInstances.Values.OfType<IDisposable>())
            {
               d.Dispose();
            }
         }
         GC.SuppressFinalize(this);
      }

      protected void EnsureNotDisposed()
      {
         if (ASyncManager.IsDisposed) throw new ObjectDisposedException(ToString(), "object is disposed");
      }


   }
}
