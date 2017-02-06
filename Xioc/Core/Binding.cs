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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xioc.Core.Internal;

namespace Xioc.Core
{
   public sealed class Binding : IEnumerable<Binding>
   {
      private ConcurrentDictionary<Type, Binding> _concreteBindings;
      private Func<Context, object> _compiledFactory;
      private Binding _decoratorTarget;

      internal IKernel Kernel;

      internal Binding(IKernel kernel, Type serviceType, Type implementationType, Func<Context, object> factory, Lifestyle lifestyle, IDictionary<string, object> dependencies, Binding parent)
      {
         Kernel = kernel;
         ServiceType = serviceType;
         Dependencies = dependencies != null ? new ReadOnlyDictionary<string, object>(dependencies) : null;
         ImplementationType = implementationType;
         if (factory != null)
         {
            Factory = factory;
         }
         Lifestyle = lifestyle;
         Parent = parent;
      }

      /// <summary>
      /// Gets the registered service type
      /// </summary>
      public Type ServiceType { get; private set; }
      /// <summary>
      /// Gets the dependencies; returns null if no dependencies
      /// </summary>
      public IDictionary<string, object> Dependencies { get; private set; }
      /// <summary>
      /// Gets the implementation type; returns null if no implementation type. In that case Factory is not null.
      /// </summary>
      public Type ImplementationType { get; private set; }
      /// <summary>
      /// Gets the factory; returns null if no factory. In that case ImplementationType not null.
      /// </summary>
      public Func<Context, object> Factory { get; private set; }
      /// <summary>
      /// Gets the LifeStyle for this binding
      /// </summary>
      public Lifestyle Lifestyle { get; private set; }

      /// <summary>
      /// Gets the parent binding, i.e., the binding that that must be considered to be registered before this binding, for the same service type.
      /// Note: If the service type was decorated, than the decorated parent becomes the 'new' parent. The Parent represents the link that is followed
      /// while executing ResolveAll().
      /// </summary>
      public Binding Parent { get; private set; }

      /// <summary>
      /// Gets the decorator target, if any. The decorator target may not necessaryly be the same as the parent, because the service type may
      /// have been decorated. In that case DecoratorTarget refers at the non decorated binding, or de previously decorated binding.
      /// </summary>
      public Binding DecoratorTarget
      {
         get { return _decoratorTarget ?? Parent; }
         internal set { _decoratorTarget = value; }
      }

      /// <summary>
      /// Resolves a new instance for the requested TService.
      /// </summary>
      /// <typeparam name="TService"></typeparam>
      /// <param name="context"></param>
      /// <returns></returns>
      public TService GetInstance<TService>(Context context)
      {
         return (TService)GetInstance(typeof(TService), context);
      }
      public object GetInstance(Type requestedType, Context context)
      {
         return CreateInstance(context.CloneWithRequestedType(requestedType));
      }

      object _cachedSingleton;
      public object GetInstance(Context context)
      {
         if (Lifestyle == Lifestyle.PerContainer && !context.UnManaged && context.RequestedType == ServiceType)
         {
            // caching hook, for better singleton performance
            return _cachedSingleton ?? (_cachedSingleton = CreateInstance(context));
         }
         return CreateInstance(context);
      }

      private object CreateInstance(Context context)
      {
         context = context.Clone(c =>
         {
            c.UnManaged = c.UnManaged || Lifestyle == Lifestyle.UnManaged;
            c.Binding = this;
         });

         if (_compiledFactory != null)
         {
            return _compiledFactory(context);
         }

         if (Factory != null)
         {
            _compiledFactory = new ResolverBuilder(this).WrapResolver(Factory, this);
            return _compiledFactory(context);
         }


         if (ImplementationType.IsGenericTypeDefinition)
         {
            _concreteBindings = _concreteBindings ?? new ConcurrentDictionary<Type, Binding>();
            return _concreteBindings.GetOrAdd(context.RequestedType, t => new Binding(
                Kernel,
                ServiceType.MakeGenericType(context.RequestedType.GetGenericArguments()),
                ImplementationType.MakeGenericType(context.RequestedType.GetGenericArguments()),
                null,
                Lifestyle,
                Dependencies,
                Parent
            ) { DecoratorTarget = _decoratorTarget }).GetInstance(context);
         }

         _compiledFactory = new ResolverBuilder(this).BuildResolver();

         return _compiledFactory(context);

      }

      IEnumerator<Binding> IEnumerable<Binding>.GetEnumerator()
      {
         var b = this;
         while (b != null)
         {
            yield return b;
            b = b.Parent;
         }

      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return ((IEnumerable<Binding>)this).GetEnumerator();
      }

      public override string ToString()
      {
         return string.Format("{0} => {1} ({2})", ServiceType.Name, ImplementationType != null ? ImplementationType.Name : "<factory-method>", Lifestyle);
      }

   }
}