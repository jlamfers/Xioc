using System;
using System.Collections.Generic;

namespace Xioc.Core
{
   public interface IContainerBase
   {
      bool CanResolve(Type type);
      bool IsRegistered(Type type);
      Binding GetBinding(Type serviceType, bool throwException = true);
      IEnumerable<Binding> GetBindings(Type serviceType);
      IEnumerable<Type> GetServiceTypes();
   }
}