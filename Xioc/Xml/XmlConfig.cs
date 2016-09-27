using System;
using System.Collections.Generic;
using System.Linq;
using Xioc.Core;

namespace Xioc.Xml
{
   public static class XmlConfig
   {
      internal static class Nested
      {
         internal static readonly Dictionary<string, IXmlConfigElement>
            BinderLookup;

         static Nested()
         {
            BinderLookup = new Dictionary<string, IXmlConfigElement>();
            Initialize();

         }

         private static void Initialize()
         {
            foreach (var instance in AppDomain.CurrentDomain.GetExportedTypes()
               .Where(t => Attribute.IsDefined(t, typeof(XmlConfigElementAttribute)))
               .Select(t => t.CreateInstance<IXmlConfigElement>()))
            {
               try
               {
                  BinderLookup.Add(instance.ElementName, instance);
               }
               catch (Exception ex)
               {
                  throw new XiocException(string.Format("Config element <{0}> could not be auto-registered: ", instance.ElementName) + ex.Message + " Still you manually can re-register (replace) xml config elements using XmlConfig.RegisterBinder or XmlConfig.RegisterPredicate.", ex);
               }
            }
         }
      }

      public static void RegisterBinder(XmlConfigElementBinder binder)
      {
         if (binder == null) throw new ArgumentNullException("binder");
         Nested.BinderLookup[binder.ElementName] = binder;
      }

      public static void RegisterPredicate(XmlConfigElementPredicate predicate)
      {
         if (predicate == null) throw new ArgumentNullException("predicate");
         Nested.BinderLookup[predicate.ElementName] = predicate;
      }

      public static IDictionary<string, IXmlConfigElement> Binders
      {
         get { return Nested.BinderLookup.ToDictionary(e => e.Key, e => e.Value.GetType().CreateInstance<IXmlConfigElement>()); }
      }
   }
}