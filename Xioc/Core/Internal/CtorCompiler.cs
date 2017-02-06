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
// ReSharper disable PossibleNullReferenceException

using System.Collections.Concurrent;
using System.Reflection.Emit;
using System;
using System.Linq;
using System.Reflection;

namespace Xioc.Core.Internal
{
   internal static class CtorCompiler
   {
      private static readonly ConcurrentDictionary<ConstructorInfo, Func<object[], object>>
          CompiledConstructors = new ConcurrentDictionary<ConstructorInfo, Func<object[], object>>();

      // compiles into a performant constructor delegate
      public static Func<Context, object> Compile(this ConstructorInfo ctorInfo, Func<Context, object>[] args)
      {
         var ctor = ctorInfo.Compile();

         if (args.Length > 12)
         {
            return ctx =>
            {
               var values = args.Select(f => f(ctx)).ToArray();
               return ctor.Invoke(values);
            };
         }

         var a0 = args.Length > 0 ? args[0] : null;
         var a1 = args.Length > 1 ? args[1] : null;
         var a2 = args.Length > 2 ? args[2] : null;
         var a3 = args.Length > 3 ? args[3] : null;
         var a4 = args.Length > 4 ? args[4] : null;
         var a5 = args.Length > 5 ? args[5] : null;
         var a6 = args.Length > 6 ? args[6] : null;
         var a7 = args.Length > 7 ? args[7] : null;
         var a8 = args.Length > 8 ? args[8] : null;
         var a9 = args.Length > 9 ? args[9] : null;
         var a10 = args.Length > 10 ? args[10] : null;
         var a11 = args.Length > 11 ? args[11] : null;
         switch (args.Length)
         {
            case 0:
               var t = ctorInfo.DeclaringType;
               return ctx => Activator.CreateInstance(t);
            case 1:
               return ctx => ctor.Invoke(new[] { a0(ctx) });
            case 2:
               return ctx => ctor.Invoke(new[] { a0(ctx), a1(ctx) });
            case 3:
               return ctx => ctor.Invoke(new [] { a0(ctx), a1(ctx), a2(ctx) });
            case 4:
               return ctx => ctor.Invoke(new[] { a0(ctx), a1(ctx), a2(ctx), a3(ctx) });
            case 5:
               return ctx => ctor.Invoke(new[] { a0(ctx), a1(ctx), a2(ctx), a3(ctx), a4(ctx) });
            case 6:
               return ctx => ctor.Invoke(new[] { a0(ctx), a1(ctx), a2(ctx), a3(ctx), a4(ctx), a5(ctx) });
            case 7:
               return ctx => ctor.Invoke(new[] { a0(ctx), a1(ctx), a2(ctx), a3(ctx), a4(ctx), a5(ctx), a6(ctx) });
            case 8:
               return ctx => ctor.Invoke(new[] { a0(ctx), a1(ctx), a2(ctx), a3(ctx), a4(ctx), a5(ctx), a6(ctx), a7(ctx) });
            case 9:
               return ctx => ctor.Invoke(new[] { a0(ctx), a1(ctx), a2(ctx), a3(ctx), a4(ctx), a5(ctx), a6(ctx), a7(ctx), a8(ctx) });
            case 10:
               return ctx => ctor.Invoke(new[] { a0(ctx), a1(ctx), a2(ctx), a3(ctx), a4(ctx), a5(ctx), a6(ctx), a7(ctx), a8(ctx), a9(ctx) });
            case 11:
               return ctx => ctor.Invoke(new[] { a0(ctx), a1(ctx), a2(ctx), a3(ctx), a4(ctx), a5(ctx), a6(ctx), a7(ctx), a8(ctx), a9(ctx), a10(ctx) });
            case 12:
               return ctx => ctor.Invoke(new[] { a0(ctx), a1(ctx), a2(ctx), a3(ctx), a4(ctx), a5(ctx), a6(ctx), a7(ctx), a8(ctx), a9(ctx), a10(ctx), a11(ctx) });

            default:
               return ctx => ctor.Invoke(args.Select(f => f(ctx)).ToArray());
         }
      }

      public static Func<object[], object> Compile(this ConstructorInfo ctorInfo)
      {
         return CompiledConstructors.GetOrAdd(ctorInfo, _compile);
      }

      private static Func<object[], object> _compile(this ConstructorInfo ctorInfo)
      {
         var pars = ctorInfo.GetParameters();
         var dm = new DynamicMethod("__dm_" + ctorInfo.Name, ctorInfo.DeclaringType, new[] { typeof(object[]) }, Assembly.GetExecutingAssembly().ManifestModule, true);
         var ilgen = dm.GetILGenerator();

         for (var i = 0; i < pars.Length; i++)
         {
            var type = pars[i].ParameterType;
            ilgen.Emit(OpCodes.Ldarg_0);
            ilgen.Emit(OpCodes.Ldc_I4, i);
            ilgen.Emit(OpCodes.Ldelem_Ref);
            ilgen.Emit(type.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, type);
         }
         ilgen.Emit(OpCodes.Newobj, ctorInfo);
         ilgen.Emit(OpCodes.Ret);
         var factory = (Func<object[], object>)dm.CreateDelegate(typeof(Func<object[], object>));

         return factory;
      }

   }
}
