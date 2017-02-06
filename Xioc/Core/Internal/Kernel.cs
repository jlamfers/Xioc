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
using System.Reflection;

namespace Xioc.Core.Internal
{
   internal class Kernel : IKernel
   {
      public Kernel(IContainer baseContainer)
      {
         Container = baseContainer;
      }

      // we do not need a thread safe Dictionary here
      // a "normal" dictionary is thread safe for readings, writings are done at setup only
      internal readonly Dictionary<Type, List<Binding>>
          Bindings = new Dictionary<Type, List<Binding>>();

      public Action<IBinder> OnNewScopeBinder { get; set; }

      public IBinder Bind(Type serviceType, Type implementationType, Lifestyle lifestyle, IDictionary<string, object> dependencies)
      {
         return Bind(serviceType, implementationType, null, lifestyle, dependencies);
      }
      public IBinder Bind(Type serviceType, Func<Context, object> factory, Lifestyle lifestyle)
      {
         return Bind(serviceType, null, factory, lifestyle, null);
      }

      protected virtual IBinder Bind(Type serviceType, Type implementationType, Func<Context, object> factory, Lifestyle lifestyle, IDictionary<string, object> dependencies)
      {
         var bindings = Bindings.GetOrAdd(serviceType, t => new List<Binding>());
         var binding = new Binding(this, serviceType, implementationType, factory, lifestyle, dependencies, bindings.LastOrDefault());
         bindings.Add(binding);
         return this;
      }

      public IBinder Decorate(Type serviceType, Type decoratorType, Lifestyle lifestyle, IDictionary<string, object> dependencies)
      {
         return Decorate(serviceType, decoratorType, null, lifestyle, dependencies);
      }
      public IBinder Decorate(Type serviceType, Func<Context, object> factory, Lifestyle lifestyle, IDictionary<string, object> dependencies)
      {
         return Decorate(serviceType, null, factory, lifestyle, dependencies);
      }

      public IBinder OnNewScope(Action<IBinder> binder, int level = 0)
      {
         if (level > 0)
         {
            var levelBinder = binder;
            binder = b =>
            {
               if (b.Scope.ScopeLevel == level)
               {
                  levelBinder(b);
               }

            };
         }
         if (OnNewScopeBinder != null)
         {
            var b1 = OnNewScopeBinder;
            var b2 = binder;
            binder = b =>
            {
               b1(b);
               b2(b);
            };
         }
         OnNewScopeBinder = binder;
         return this;
      }

      protected virtual IBinder Decorate(Type serviceType, Type decoratorType, Func<Context, object> factory, Lifestyle lifestyle, IDictionary<string, object> dependencies)
      {
         List<Binding> targetBindings;
         if (!Bindings.TryRemove(serviceType, out targetBindings) || targetBindings.First().ServiceType != serviceType)
         {
            throw new XiocException("Target type for decorator " + serviceType + " has not been bound, so it cannot be decorated");
         }
         var decorators = new List<Binding>();
         foreach (var target in targetBindings)
         {
            var binding = new Binding(this, serviceType, decoratorType, factory, lifestyle, dependencies, decorators.LastOrDefault())
            {
               DecoratorTarget = target
            };
            decorators.Add(binding);
         }
         Bindings[serviceType] = decorators;
         return this;
      }

      public IEnumerable<Binding> GetBindings(Type serviceType)
      {
         var binding = TryGetBinding(serviceType, false);
         return binding == null ? Enumerable.Empty<Binding>() : binding.Select(b => b);
      }

      public virtual IEnumerable<Type> GetServiceTypes()
      {
         return Bindings.Keys;
      }

      public IContainer Container { get; private set; }
      public IScope Scope { get; protected set; }

      public Binding GetBinding(Type type, bool throwException = true)
      {
         if (type == null) throw new ArgumentNullException("type");
         var binding = TryGetBinding(type, false);
         if (binding != null) return binding;
         if (throwException)
         {
            throw new XiocException("No binding found for type " + type);
         }
         return null;

      }

      public int StackSize { get; set; }

      public virtual Binding TryGetBinding(Type serviceType, bool checkCollectionTypes = true)
      {
         List<Binding> list;
         if (Bindings.TryGetValue(serviceType, out list))
         {
            return list.Last();
         }
         if (serviceType.IsClosedGenericType() && Bindings.TryGetValue(serviceType.GetGenericTypeDefinition(), out list))
         {
            return list.Last();
         }
         if (!checkCollectionTypes)
         {
            return null;
         }
         Binding binding;
         if (serviceType.IsCollectionType() && (binding = TryGetBinding(serviceType.GetCollectionElementType(), false)) != null)
         {
            return binding;
         }
         return null;
      }

      public virtual bool CanResolve(Type type)
      {
         return IsRegistered(type)
             || (type.IsClosedGenericType() && CanResolve(type.GetGenericTypeDefinition()))
             || (type.IsCollectionType() && CanResolve(type.GetCollectionElementType()));
      }

      public virtual bool IsRegistered(Type type)
      {
         return Bindings.ContainsKey(type);
      }

      public ConstructorInfo FindConstructorInfo(Type type, IDictionary<string, object> dependencies)
      {
         var ctors = type.GetConstructors().ToArray();

         var predicate = Container.Settings.ImportingConstructorPredicate;

         ConstructorInfo ctor = null;
         var @this = this;
         if (predicate != null)
         {
            ctor = ctors.FirstOrDefault(c => predicate(@this, c));
         }
         ctor = ctor
             ??
             type.GetConstructors()
             .Where(c => c.GetParameters()
                 .All(p => CanResolve(p.ParameterType) || p.HasDefaultValue || (dependencies != null && dependencies.ContainsKey(p.Name))))
             .OrderByDescending(c => c.GetParameters().Count(p => CanResolve(p.ParameterType)))
             .FirstOrDefault(c => dependencies == null || dependencies.Keys.All(k => c.GetParameters().Any(p => p.Name == k)));

         if (ctor == null && (dependencies == null || dependencies.Count == 0))
         {
            ctor = type.GetConstructor(Type.EmptyTypes);
         }
         return ctor;

      }
   }

   static class DictionaryExtensions
   {
      public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> self, TKey key, Func<TKey, TValue> factory)
      {
         TValue current;
         if (self.TryGetValue(key, out current))
         {
            return current;
         }
         return self[key] = factory(key);
      }

      public static bool TryRemove<TKey, TValue>(this Dictionary<TKey, TValue> self, TKey key, out TValue result)
      {
         self.TryGetValue(key, out result);
         return self.Remove(key);
      }
   }
}
