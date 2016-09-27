using System;
using System.Collections.Generic;
using System.Reflection;

namespace Xioc.Core.Internal
{
   internal interface IKernel : IBinder
   {
      int StackSize { get; }
      Action<IBinder> OnNewScopeBinder { get; set; }
      Binding TryGetBinding(Type serviceType, bool checkCollectionTypes = true);
      ConstructorInfo FindConstructorInfo(Type type, IDictionary<string, object> dependencies);
   }
}