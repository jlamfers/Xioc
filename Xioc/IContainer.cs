using System;
using Xioc.Core;

namespace Xioc
{
   public interface IContainer : IContainerBase, IDisposable
   {
      IScope BeginScope(Action<IBinder> binder = null);
      ISettings Settings { get;}
      object ResolveScoped(Type serviceType, bool throwException = true);
      bool ReleaseScoped(object instance);
   }
}