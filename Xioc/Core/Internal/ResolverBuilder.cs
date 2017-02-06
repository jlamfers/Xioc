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
using System.Threading;

namespace Xioc.Core.Internal
{
   internal class ResolverBuilder
   {
      private readonly Binding _binding;

      public static readonly IDictionary<Lifestyle, Func<Scope, Context, Func<Context, object>, object>>
          ScopeHooks = new Dictionary<Lifestyle, Func<Scope, Context, Func<Context, object>, object>>
            {
                {Lifestyle.PerContainer, (s,c,f) => s.GetOrCreatePerContainer(c,f)},
                {Lifestyle.PerScope, (s,c,f) => s.GetOrCreatePerScope(c,f)},
                {Lifestyle.Transient, (s,c,f) => s.CreateTransient(c,f)},
                {Lifestyle.TransientNoDispose, (s,c,f) => f(c)},
                {Lifestyle.UnManaged, (s,c,f) => f(c)}
            };

      public ResolverBuilder(Binding binding)
      {
         _binding = binding;
      }

      public bool NoNeedToHook()
      {
         var t = _binding.ImplementationType;
         return _binding.Lifestyle == Lifestyle.UnManaged || (t != null && !typeof(IDisposable).IsAssignableFrom(t) && _binding.Lifestyle == Lifestyle.Transient);
      }

      public Func<Context, object> BuildResolver()
      {
         var t = _binding.ImplementationType;
         var d = _binding.Dependencies;

         var ctor = _binding.Kernel.FindConstructorInfo(t, d);

         if (ctor == null)
         {
            throw new XiocException("Type " + t + " cannot be created. No appropriate constructor found.");
         }
         var memberInjector = BuildMemberInjector();
         Func<Context, object> compiledCtor;

         if (ctor.GetParameters().Length == 0)
         {
            compiledCtor = c => Activator.CreateInstance(t);
            if (memberInjector != null)
            {
               var cc = compiledCtor;
               compiledCtor = ctx => memberInjector(ctx, cc(ctx));
            }
            // use default ctor; no need to resolve anything, no risk of recursion error
            return NoNeedToHook() ? compiledCtor : WrapResolver(compiledCtor, _binding);
         }

         var args = new Func<Context, object>[ctor.GetParameters().Length];
         var parameters = ctor.GetParameters();
         for (var i = 0; i < parameters.Length; i++)
         {
            args[i] = BuildParameterResolver(parameters[i]);
         }

         var tempCtor = ctor.Compile(args);
         compiledCtor = ctx => tempCtor(ctx.CloneWithInstanceType(t));
         if (memberInjector != null)
         {
            var cc = compiledCtor;
            compiledCtor = ctx => memberInjector(ctx, cc(ctx));
         }

         return WrapResolver(compiledCtor, _binding);
      }

      public Func<Context, object> BuildParameterResolver(ParameterInfo parameter)
      {

         var parameterBinding = _binding.Kernel.TryGetBinding(parameter.ParameterType);

         if (parameterBinding == null)
         {
            // parameter type cannot be resolved by kernel
            object value;
            if (_binding.Dependencies == null || !_binding.Dependencies.TryGetValue(parameter.Name, out value))
            {
               if (!parameter.HasDefaultValue)
               {
                  throw new XiocException("Parameter " + parameter + " cannot be resolved.");
               }
               value = parameter.DefaultValue;
            }
            var f1 = value as Func<object>;
            if (f1 != null)
            {
               return c => f1();
            }
            var f2 = value as Func<Context,object>;
            if (f2 != null)
            {
               return f2;
            }
            return c => value;
         }

         if (parameterBinding.ServiceType != _binding.ServiceType && !(_binding.ServiceType.IsGenericType && parameterBinding.ServiceType.IsGenericTypeDefinition && parameterBinding.ServiceType == _binding.ServiceType.GetGenericTypeDefinition()))
         {
            return BuildTypeResolver(parameter.ParameterType, parameterBinding);
         }

         if (_binding.DecoratorTarget == null)
         {
            throw new XiocException("Recursion error with binding " + _binding);
         }

         // decorated binding
         parameterBinding = _binding.DecoratorTarget;
         return BuildTypeResolver(parameter.ParameterType, parameterBinding, true);
      }

      public Func<Context, object> BuildMemberResolver(Type memberType)
      {

         var parameterBinding = _binding.Kernel.TryGetBinding(memberType);

         if (parameterBinding == null)
         {
            return null;
         }

         if (parameterBinding.ServiceType != _binding.ServiceType && !(_binding.ServiceType.IsGenericType && parameterBinding.ServiceType.IsGenericTypeDefinition && parameterBinding.ServiceType == _binding.ServiceType.GetGenericTypeDefinition()))
         {
            return BuildTypeResolver(memberType, parameterBinding);
         }

         if (_binding.DecoratorTarget == null)
         {
            throw new XiocException("Recursion error with binding " + _binding);
         }

         // decorated binding
         parameterBinding = _binding.DecoratorTarget;
         return BuildTypeResolver(memberType, parameterBinding, true);
      }

      private Func<Context, object> BuildTypeResolver(Type type, Binding binding, bool decorated = false)
      {

         var b = binding;
         if (type.IsAssignableFrom(binding.ImplementationType))
         {
            if (!decorated)
            {
               return ctx =>
                   ctx.ScopeInternal.KernelStackSize <= b.Kernel.StackSize
                      // get instance directly from binding, no need to do scope traversal
                       ? b.GetInstance(ctx.CloneWithRequestedType(type))
                      // resolve by scope traversal
                       : ctx.ScopeInternal.Resolve(ctx.CloneWithRequestedType(type));
            }

            return b.GetInstance;
         }

         if (type.IsCollectionType())
         {
            var et = type.GetCollectionElementType();
            var c = et.GetCollectionTypeConverter();
            var f = type.GetCollectionTypeConverterDelegate();
            if (!decorated)
            {
               // bound service types chain
               return ctx =>
                   ctx.ScopeInternal.KernelStackSize <= b.Kernel.StackSize
                       ? f(c, b.Select(x => x.GetInstance(ctx.CloneWithRequestedType(et))))
                       : f(c, ctx.ScopeInternal.ResolveAll(ctx.CloneWithRequestedType(et)));
            }
            // binding targets chain
            return ctx => f(c, b.Select(x => x.GetInstance(ctx.CloneWithRequestedType(et))));

         }
         if (!decorated)
         {
            return ctx => ctx.ScopeInternal.KernelStackSize <= b.Kernel.StackSize
                ? b.GetInstance(ctx.CloneWithRequestedType(type))
                : ctx.ScopeInternal.Resolve(ctx.CloneWithRequestedType(type));
         }
         return b.GetInstance;
      }

      public Func<Context, object> WrapResolver(Func<Context, object> resolver, Binding binding)
      {
         var recursionFlag = new ThreadLocal<bool>();
         var hook = ScopeHooks[binding.Lifestyle];
         var f = resolver;

         var factory = NoNeedToHook() ? f : ctx => hook(ctx.ScopeInternal, ctx, f);

         return c =>
         {
            if (recursionFlag.Value)
            {
               throw new XiocException("Recursion error with binding " + _binding);
            }
            recursionFlag.Value = true;
            try
            {
               return factory(c);
            }
            finally
            {
               recursionFlag.Value = false;
            }
         };

      }

      private Func<Context, object, object> BuildMemberInjector()
      {
         var predicate = _binding.Kernel.Container.Settings.ImportingMemberPredicate;
         if (predicate == null) return null;
         var k = _binding.Kernel;
         var members = _binding.ImplementationType.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                 .Where(m => predicate(k, m))
                 .ToArray();

         if (members.Length == 0) return null;


         Func<Context, object, object> setterDelegate = null;

         var fields = members.Where(m => m.MemberType == MemberTypes.Field).Cast<FieldInfo>().ToArray();
         var properties = members.Where(m => m.MemberType == MemberTypes.Property).Cast<PropertyInfo>().ToArray();

         var setters = new Action<Context, object>[fields.Length + properties.Length];

         int index = 0;
         foreach (var f in fields)
         {
            var r = BuildMemberResolver(f.FieldType);
            var f1 = f;
            setters[index++] = (ctx, obj) => f1.SetValue(obj, r(ctx));
         }
         foreach (var p in properties)
         {
            var r = BuildMemberResolver(p.PropertyType);
            var p1 = p.CreateSetterDelegate();
            setters[index++] = (ctx, obj) => p1.Invoke(obj, r(ctx));
         }

         var t = _binding.ImplementationType;
         setterDelegate = (ctx, obj) =>
         {
            ctx = ctx.CloneWithInstanceType(t);
            for (var i = 0; i < setters.Length; i++)
            {
               setters[i](ctx, obj);
            }
            return obj;
         };

         return setterDelegate;
      }
   }
}
