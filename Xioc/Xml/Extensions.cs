using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Xioc.Xml
{
   public static class Extensions
   {
      public static string GetAttributeValue(this XElement self, string name, string @default)
      {
         return self != null && self.Attributes(name).Any() ? self.Attributes(name).Select(a => a.Value).Single() : @default;
      }
      public static string GetAttributeValue(this XElement self, string name)
      {
         if (self == null) throw new ArgumentNullException("self");
         if (!self.Attributes(name).Any())
         {
            throw new XmlException(String.Format("attribute '{0}/@{1}' not found.", self.Name, name));
         }
         return self.Attributes(name).Select(a => a.Value).Single();
      }
      public static T GetAttributeValue<T>(this XElement self, string name, T @default)
      {
         return !self.Attributes(name).Any() ? @default : self.GetAttributeValue<T>(name);
      }
      public static T GetAttributeValue<T>(this XElement self, string name)
      {
         return (T)self.GetAttributeValue(name, typeof(T));
      }
      public static object GetAttributeValue(this XElement self, string name, Type type)
      {
         if (!self.Attributes(name).Any())
         {
            throw new XmlException(String.Format("attribute '{0}/@{1}' not found.", self.Name, name));
         }
         var attributeValue = self.GetAttributeValue(name);
         try
         {
            if (type == typeof(Type))
            {
               return Type.GetType(attributeValue, true);
            }
            return TypeDescriptor.GetConverter(type).ConvertFromInvariantString(attributeValue);
         }
         catch (Exception ex)
         {
            var msg = "";
            if (type.IsEnum)
            {
               msg = ". Value must be any of: " + string.Join(", ",Enum.GetNames(type));
            }
            throw new XmlException(String.Format("Unable to read, or convert, attribute '{0}/@{1}': {2}" + msg, self.Name, name, ex.Message), ex);
         }
      }
      internal static Dictionary<string, object> GetDependencies(this XElement self, Type type)
      {
         var dependencies = new Dictionary<string, object>();
         var dependencyNodes = self.Descendants("dependencies").ToArray();
         if (dependencyNodes.Any())
         {
            var childs = dependencyNodes.Descendants("add");
            if (childs != null)
            {
               var args = new Dictionary<string, Type>();
               foreach (var arg in type.GetConstructors().SelectMany(c => c.GetParameters()))
               {
                  if (arg.ParameterType.IsSimpleType())
                  {
                     args[arg.Name] = arg.ParameterType;
                  }
               }
               foreach (var childNode in childs)
               {
                  var name = childNode.GetAttributeValue("name");
                  var value = childNode.GetAttributeValue("value", args[name]);
                  dependencies.Add(name, value);
               }
            }
            return dependencies;
         }
         return null;
      }

      internal static List<Action<IBinder>> GetBinders(this IEnumerable<XElement> self)
      {
         var binders = new List<Action<IBinder>>();
         foreach (var e in self)
         {
            IXmlConfigElement xmlBinder;
            if (!XmlConfig.Nested.BinderLookup.TryGetValue(e.Name.LocalName, out xmlBinder) || xmlBinder.IsPredicate)
            {
               throw new XmlException("No binder found for element: " + e.Name.LocalName);
            }
            binders.Add(xmlBinder.CreateBinder(e));
         }
         return binders;
      }

      internal static List<Func<bool>> GetPredicates(this IEnumerable<XElement> self)
      {
         var predicates = new List<Func<bool>>();
         foreach (var e in self)
         {
            IXmlConfigElement xmlBinder;
            if (!XmlConfig.Nested.BinderLookup.TryGetValue(e.Name.LocalName, out xmlBinder) || !xmlBinder.IsPredicate)
            {
               throw new XmlException("No predicate found for element: " + e.Name.LocalName);
            }
            predicates.Add(xmlBinder.CreatePredicate(e));
         }
         return predicates;
      }


      private static bool IsSimpleType(this Type type)
      {
         type = Nullable.GetUnderlyingType(type) ?? type;
         return type.IsPrimitive
                || type.IsEnum
                || type == typeof(string)
                || type == typeof(decimal);
      }
   }
}