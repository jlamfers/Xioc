using System;
using System.Collections.Generic;
using Xioc.Core;

namespace Xioc
{
   public interface IBinder : IContainerBase
   {
      IBinder Bind(Type serviceType, Type implementationType, Lifestyle lifestyle = Lifestyle.Transient, IDictionary<string, object> dependencies = null);
      IBinder Bind(Type serviceType, Func<Context, object> factory, Lifestyle lifestyle = Lifestyle.Transient);
      IBinder Decorate(Type serviceType, Type decoratorType, Lifestyle lifestyle = Lifestyle.Transient, IDictionary<string, object> dependencies = null);
      IBinder Decorate(Type serviceType, Func<Context, object> factory, Lifestyle lifestyle = Lifestyle.Transient, IDictionary<string, object> dependencies = null);
      IBinder OnNewScope(Action<IBinder> binder, int level = 0);
      IContainer Container { get; }
      IScope Scope { get; }
   }
}