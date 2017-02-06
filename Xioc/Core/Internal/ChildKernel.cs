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
using System.Collections.Generic;
using System.Linq;

namespace Xioc.Core.Internal
{
   internal class ChildKernel : Kernel
   {
      private readonly IKernel _parent;

      public ChildKernel(IKernel parent, XiocContainer container)
         : base(parent.Container)
      {
         _parent = parent;
         StackSize = _parent.StackSize + 1;
         Scope = (Scope)container;
         OnNewScopeBinder = _parent.OnNewScopeBinder;
      }

      protected override IBinder Bind(Type serviceType, Type implementationType, Func<Context, object> factory, Lifestyle lifestyle, IDictionary<string, object> dependencies)
      {
         EnsureNotSingleton(lifestyle);
         EnsureBindingsCopied(serviceType);
         return base.Bind(serviceType, implementationType, factory, lifestyle, dependencies);
      }

      protected override IBinder Decorate(Type serviceType, Type decoratorType, Func<Context, object> factory, Lifestyle lifestyle, IDictionary<string, object> dependencies)
      {
         EnsureNotSingleton(lifestyle);
         EnsureBindingsCopied(serviceType);
         return base.Decorate(serviceType, decoratorType, factory, lifestyle, dependencies);
      }

      private void EnsureBindingsCopied(Type serviceType)
      {
         var parent = _parent;
         if (!Bindings.ContainsKey(serviceType))
         {
            while (parent != null)
            {
               List<Binding> parentBindings;
               if (((Kernel)parent).Bindings.TryGetValue(serviceType, out parentBindings))
               {
                  Bindings[serviceType] = parentBindings.ToList();
                  return;
               }
               var k = parent as ChildKernel;
               parent = k != null ? k._parent : null;
            }
         }
      }

      public override bool CanResolve(Type type)
      {
         return base.CanResolve(type) || _parent.CanResolve(type);
      }

      public override bool IsRegistered(Type type)
      {
         return base.IsRegistered(type) || _parent.IsRegistered(type);
      }

      public override Binding TryGetBinding(Type serviceType, bool checkCollectionTypes = true)
      {
         return base.TryGetBinding(serviceType, checkCollectionTypes) ?? _parent.TryGetBinding(serviceType, checkCollectionTypes);
      }

      public override IEnumerable<Type> GetServiceTypes()
      {
         return base.GetServiceTypes().Union(_parent.GetServiceTypes());
      }

      private static void EnsureNotSingleton(Lifestyle lifestyle)
      {
         if (lifestyle == Lifestyle.PerContainer)
         {
            throw new XiocException("You cannot use lifestyle PerContainer at scope level. PerContainer instances only can be bound at the root container level.");
         }
      }

   }
}