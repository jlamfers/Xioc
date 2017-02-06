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
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace Xioc.Proxy.Core
{
    public static class DynamicInterfaceFactory
    {
        private class Key
        {
            private readonly int _hashcode;

            public Key(Type[] types)
            {
                Types = types;
                var hashcode = 103699;
                var factor = 461;
                unchecked
                {
                    foreach (var t in types)
                    {
                        hashcode = hashcode*factor + t.GetHashCode();
                    }
                }
                _hashcode = hashcode;

            }

            public readonly Type[] Types;

            public override int GetHashCode()
            {
                return _hashcode;
            }

            public override bool Equals(object obj)
            {
                var other = obj as Key;
                return other != null && other.Types.SequenceEqual(Types);
            }
        }

        private static readonly ConcurrentDictionary<Key, Type>
            TypeCache = new ConcurrentDictionary<Key, Type>();

        private static int _typeCount;

        public static Type GetAggregateType(this Type[] types)
        {
            return TypeCache.GetOrAdd(new Key(types), x =>
            {
                if (x.Types.Any(t => !t.IsInterface))
                {
                    throw new ArgumentException(string.Format("Type {0} is not an interface", types.First(t => !t.IsInterface)), "types");
                }

                var assemblyName = new AssemblyName(Guid.NewGuid().ToString());

                var dynamicAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(
                    assemblyName,
                    AssemblyBuilderAccess.RunAndCollect);

                var moduleBuilder = dynamicAssembly.DefineDynamicModule(
                    assemblyName.Name,
                    assemblyName + ".dll");
                var typeBuilder = moduleBuilder.DefineType("__dt_" + Interlocked.Increment(ref _typeCount), TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);

                foreach (var t in types)
                {
                    typeBuilder.AddInterfaceImplementation(t);
                }


                return typeBuilder.CreateType();
            });
        }
    }
}
