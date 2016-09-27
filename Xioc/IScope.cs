using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xioc.Core;

namespace Xioc
{
   public interface IScope : IContainer, IASyncManager, ISynchronizeInvoke
   {
      object Resolve(Type serviceType, bool throwException = true);
      IEnumerable<object> ResolveAll(Type serviceType);
      int ScopeLevel { get; }
   }
}