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
using System.Reflection;

namespace Xioc.Core.Internal
{
   internal static class Extensions
   {
      #region Types
      private interface ISetter
      {
         ISetter Initialize(PropertyInfo property);
         Action<object, object> Setter { get; }
      }
      [Serializable]
      private class SetterDelegate<TTarget, TValue> : ISetter
      {
         private Action<TTarget, TValue> _setter;

         public Action<object, object> Setter
         {
            get { return (t, v) => _setter((TTarget)t, (TValue)v); }
         }

         public ISetter Initialize(PropertyInfo property)
         {
            _setter = (Action<TTarget, TValue>)Delegate.CreateDelegate(typeof(Action<TTarget, TValue>), property.GetSetMethod(true));
            return this;
         }
      }
      #endregion

      public static Action<object, object> CreateSetterDelegate(this PropertyInfo self)
      {
         return typeof(SetterDelegate<,>)
             .MakeGenericType(self.DeclaringType, self.PropertyType)
             .CreateInstance<ISetter>()
             .Initialize(self)
             .Setter;

      }

   }
}
