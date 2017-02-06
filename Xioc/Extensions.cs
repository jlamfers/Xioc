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
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using Xioc.Core;
using Xioc.Proxy;

namespace Xioc
{
   public static class Extension
   {
      #region IBinder
      public static IBinder Bind(this IBinder self, Type implementationType, Lifestyle lifestyle = Lifestyle.Transient,
          IDictionary<string, object> dependencies = null)
      {
         return self.Bind(implementationType, implementationType, lifestyle, dependencies);
      }

      public static IBinder Bind<T>(this IBinder self, Lifestyle lifestyle = Lifestyle.Transient,
          IDictionary<string, object> dependencies = null)
      {
         return self.Bind(typeof(T), typeof(T), lifestyle, dependencies);
      }

      public static IBinder Bind<TService, TImplementation>(this IBinder self, Lifestyle lifestyle = Lifestyle.Transient, IDictionary<string, object> dependencies = null)
          where TImplementation : class, TService
      {
         return self.Bind(typeof(TService), typeof(TImplementation), lifestyle, dependencies);
      }

      public static IBinder Bind<TService>(this IBinder self, Func<Context, TService> factory, Lifestyle lifestyle = Lifestyle.Transient)
      {
         return self.Bind(typeof(TService), c => factory(c), lifestyle);
      }

      public static IBinder Bind<TService>(this IBinder self, Func<TService> factory, Lifestyle lifestyle = Lifestyle.Transient)
      {
         return self.Bind(typeof(TService), c => factory(), lifestyle);
      }

      public static IBinder Decorate<TService, TDecorator>(this IBinder self, Lifestyle lifestyle = Lifestyle.Transient, IDictionary<string, object> dependencies = null)
          where TDecorator : class, TService
      {
         return self.Decorate(typeof(TService), typeof(TDecorator), lifestyle, dependencies);
      }
      public static IBinder Decorate<TService>(this IBinder self, Func<Context, TService> factory, Lifestyle lifestyle = Lifestyle.Transient, IDictionary<string, object> dependencies = null)
      {
         return self.Decorate(typeof(TService), c => factory(c), lifestyle, dependencies);
      }


      public static IBinder BindAllOf<TService>(this IBinder self, IEnumerable<Assembly> assemblies, Action<IBinder, Type> bindAction = null, Lifestyle lifestyle = Lifestyle.Transient, bool ifNotCanBeResolved = true)
          where TService : class
      {
         return self.BindAllOf(typeof (TService), assemblies, bindAction, lifestyle, ifNotCanBeResolved);
      }
      public static IBinder BindAllOf(this IBinder self,  Type type, IEnumerable<Assembly> assemblies, Action<IBinder, Type> bindAction = null, Lifestyle lifestyle = Lifestyle.Transient, bool ifNotCanBeResolved = true)
      {
         if (type == null) throw new ArgumentNullException("type");
         bindAction = bindAction ?? ((b, t) => b.Bind(t));
         assemblies = assemblies ?? AppDomain.CurrentDomain.GetAvailableAssemblies();
         foreach (var t in assemblies.SelectMany(a => a.GetTypes().Where(t => !t.IsAbstract && type.IsAssignableFrom(t))))
         {
            if (ifNotCanBeResolved && self.CanResolve(t)) continue;
            bindAction(self, t);
         }
         return self;
      }

      public static IBinder BindMefExports(this IBinder self, IEnumerable<Assembly> assemblies, Lifestyle defaultLifestyle = Lifestyle.Transient)
      {
         foreach (
             var type in
                 assemblies.SelectMany(a => a.GetTypes())
                     .Where(t => Attribute.IsDefined(t, typeof(ExportAttribute))))
         {
            var exportAtt = type.GetCustomAttribute<ExportAttribute>();
            var partCreationPolicy = type.GetCustomAttribute<PartCreationPolicyAttribute>();
            var lifestyle = defaultLifestyle;
            if (partCreationPolicy != null)
            {
               switch (partCreationPolicy.CreationPolicy)
               {
                  case CreationPolicy.Any:
                     break;
                  case CreationPolicy.Shared:
                     lifestyle = Lifestyle.PerContainer;
                     break;
                  case CreationPolicy.NonShared:
                     lifestyle = Lifestyle.Transient;
                     break;
                  default:
                     throw new ArgumentOutOfRangeException();
               }
            }

            self.Bind(exportAtt.ContractType ?? type.GetInterfaces()[0], type, lifestyle);
         }
         return self;
      }

      public static IBinder BindXiocExports(this IBinder self, IEnumerable<Assembly> assemblies)
      {
         foreach (
             var type in
                 assemblies.SelectMany(a => a.GetTypes())
                     .Where(t => t.IsClass && !t.IsAbstract && typeof(IExports).IsAssignableFrom(t)))
         {
            type.CreateInstance<IExports>().Export(self);
         }
         return self;
      }

      public static IBinder Intercept(this IBinder self, Type serviceType, IInterceptor interceptor)
      {
         if (self == null) throw new ArgumentNullException("self");
         if (serviceType == null) throw new ArgumentNullException("serviceType");
         if (interceptor == null) throw new ArgumentNullException("interceptor");
         return self.Decorate(serviceType, c => ProxyBuilder.CreateProxy(serviceType, c.Binding.DecoratorTarget.GetInstance(c), interceptor));
      }

      public static IBinder Intercept(this IBinder self, Type serviceType, Type interceptorType)
      {
         return self.Intercept(serviceType, interceptorType.CreateInstance<IInterceptor>());
      }

      public static IBinder Intercept<TService, TInterceptor>(this IBinder self)
         where TInterceptor : IInterceptor, new()
      {
         return self.Intercept(typeof(TService), new TInterceptor());
      }

      public static IBinder Intercept(this IBinder self, Type serviceType, Action<IInvocation> interceptor)
      {
         if (interceptor == null) throw new ArgumentNullException("interceptor");
         return self.Intercept(serviceType, new InterceptorAction(interceptor));
      }

      public static IBinder Intercept<TService>(this IBinder self, Action<IInvocation> interceptor)
      {
         return self.Intercept(typeof(TService), interceptor);
      }

      #endregion

      #region IContainer
      public static object TryResolveScoped(this IContainer self, Type serviceType)
      {
         return self.ResolveScoped(serviceType, false);
      }
      public static TService ResolveScoped<TService>(this IContainer self, bool throwException = true)
      {
         return (TService)self.ResolveScoped(typeof(TService), throwException);
      }
      public static TService TryResolveScoped<TService>(this IContainer self)
      {
         return (TService)self.ResolveScoped(typeof(TService), false);
      }
      #endregion

      #region IScope

      public static object TryResolve(this IScope self, Type serviceType)
      {
         return self.Resolve(serviceType, false);
      }

      public static T TryResolve<T>(this IScope self)
      {
         return self.TryResolve(typeof(T)).CastTo<T>();
      }

      public static T Resolve<T>(this IScope self, bool throwException = true)
      {
         return (T)self.Resolve(typeof(T), throwException);
      }

      public static IEnumerable<T> ResolveAll<T>(this IScope self)
      {
         return self.ResolveAll(typeof(T)).Cast<T>();
      }

      public static bool CanResolve<T>(this IScope self)
      {
         return self.CanResolve(typeof(T));
      }

      public static bool IsRegistered<T>(this IScope self)
      {
         return self.IsRegistered(typeof(T));
      }

      public static Binding GetBinding<T>(this IScope self, bool throwException = true)
      {
         return self.GetBinding(typeof(T), throwException);
      }

      public static IEnumerable<Binding> GetBindings<T>(this IScope self)
      {
         return self.GetBindings(typeof(T));
      }

      #endregion

      #region Settings

      public static Settings EnableMefImports(this Settings self)
      {
         return self.AddImportingMemberPredicate((c, m) =>
            Attribute.IsDefined(m, typeof (ImportAttribute)) ||
            Attribute.IsDefined(m, typeof (ImportManyAttribute)));
      }

      public static Settings EnableAutoPropertyInjection(this Settings self, bool publicOnly = true)
      {
         return self.AddImportingMemberPredicate((c, m) =>
         {
            if (m.MemberType != MemberTypes.Property) return false;
            var p = (PropertyInfo)m;
            if (!p.CanWrite) return false;
            if (publicOnly)
            {
              if(p.GetSetMethod(false) == null) return false; 
            } 
            return c.CanResolve(p.PropertyType);
         });
      }

      private static Settings AddImportingMemberPredicate(this Settings self, Func<IContainerBase, MemberInfo, bool> predicate)
      {
         if (self.ImportingMemberPredicate == null)
         {
            self.ImportingMemberPredicate = predicate;

         }
         else
         {
            var currentPredicate = self.ImportingMemberPredicate;
            self.ImportingMemberPredicate = (c, m) => currentPredicate(c, m) || predicate(c, m);
         }
         return self;
      }

      #endregion

      #region Miscellaneous
      internal static T CastTo<T>(this object self)
      {
         return self == null ? default(T) : (T)self;
      }
      internal static object CreateInstance(this Type self, params object[] args)
      {
         return (args == null || args.Length == 0) ? Activator.CreateInstance(self) : Activator.CreateInstance(self, args);
      }
      internal static T CreateInstance<T>(this Type self, params object[] args)
      {
         return self.CreateInstance(args).CastTo<T>();
      }
      internal static bool IsClosedGenericType(this Type type)
      {
         return type.IsGenericType && !type.IsGenericTypeDefinition;
      }

      #endregion

   }
}