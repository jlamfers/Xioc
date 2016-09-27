using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Xioc.Xml
{
   public static class BinderExtension
   {
      [Serializable]
      internal class XmlReturnException : Exception
      {
      }

      private static readonly ConcurrentDictionary<string, List<Action<IBinder>>>
         Binders = new ConcurrentDictionary<string, List<Action<IBinder>>>();


      public static IBinder BindXml(this IBinder binder, string uriOrXml)
      {
         if (uriOrXml == null) throw new ArgumentNullException("uriOrXml");
         try
         {

            var bindersList = Binders.GetOrAdd(uriOrXml, u =>
            {
               var xdoc = uriOrXml.TrimStart().StartsWith("<")
                  ? XDocument.Load(new StringReader(uriOrXml))
                  : XDocument.Load(uriOrXml);

               return xdoc.Root != null ? xdoc.Root.Elements().GetBinders() : null;
            });

            bindersList.Bind(binder);
         }
         catch (XmlReturnException)
         {
            // any binder evaluated a return element
         }

         return binder;
      }

      internal static void Bind(this IEnumerable<Action<IBinder>> self, IBinder binder)
      {
         if (self == null) return;

         foreach (var b in self)
         {
            b(binder);
         }
      }
   }
}