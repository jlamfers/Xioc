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
