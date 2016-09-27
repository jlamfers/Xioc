// ReSharper disable InconsistentNaming

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Xioc.Core.Internal
{
   internal static class CollectionTypeHelper
   {
      public interface ICollectionTypeConverter
      {
         object ToList(IEnumerable<object> source);
         object ToEnumerable(IEnumerable<object> source);
         object ToCollection(IEnumerable<object> source);
         object ToArray(IEnumerable<object> source);
         object ToReadOnlyCollection(IEnumerable<object> source);
      }

      private class CollectionTypeConverter<T> : ICollectionTypeConverter
      {
         public object ToList(IEnumerable<object> source)
         {
            return source.Cast<T>().ToList();
         }

         public object ToEnumerable(IEnumerable<object> source)
         {
            return source.Cast<T>();
         }

         public object ToCollection(IEnumerable<object> source)
         {
            return new Collection<T>(source.Cast<T>().ToArray());
         }

         public object ToArray(IEnumerable<object> source)
         {
            return source.Cast<T>().ToArray();
         }

         public object ToReadOnlyCollection(IEnumerable<object> source)
         {
            return source.Cast<T>().ToList().AsReadOnly();
         }

      }

      private static readonly Dictionary<Type, Func<ICollectionTypeConverter, IEnumerable<object>, object>>
          _converterDelegates = new Dictionary<Type, Func<ICollectionTypeConverter, IEnumerable<object>, object>>
            {
                {typeof (IEnumerable<>), (c,e) => c.ToEnumerable(e)},
                {typeof (ICollection<>), (c,e) => c.ToCollection(e)},
                {typeof (Collection<>), (c,e) => c.ToCollection(e)},
                {typeof (IList<>),(c,e) => c.ToList(e)},
                {typeof (List<>),(c,e) => c.ToList(e)}, 
                {typeof (IReadOnlyCollection<>), (c,e) => c.ToReadOnlyCollection(e)},
                {typeof (ReadOnlyCollection<>),(c,e) => c.ToReadOnlyCollection(e)},
            };

      private static readonly ConcurrentDictionary<Type, ICollectionTypeConverter>
          _converters = new ConcurrentDictionary<Type, ICollectionTypeConverter>();

      public static bool IsCollectionType(this Type type)
      {
         return type.IsArray || (type.IsGenericType && _converterDelegates.ContainsKey(type.GetGenericTypeDefinition()));
      }
      public static Type GetCollectionElementType(this Type type)
      {
         return type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];
      }
      public static ICollectionTypeConverter GetCollectionTypeConverter(this Type type)
      {
         return _converters.GetOrAdd(type, t => (ICollectionTypeConverter)Activator.CreateInstance(typeof(CollectionTypeConverter<>).MakeGenericType(type)));
      }

      public static Func<ICollectionTypeConverter, IEnumerable<object>, object> GetCollectionTypeConverterDelegate(this Type type)
      {
         return type.IsArray
             ? ((cv, col) => cv.ToArray(col))
             : _converterDelegates[type.GetGenericTypeDefinition()];
      }
   }
}