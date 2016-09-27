using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Xml.Linq;

namespace Xioc.Xml.Elements
{
   [XmlConfigElement]
   public class XmlPredicateUser : XmlConfigElementPredicate
   {
      public enum UserType
      {
         Windows,
         Application,
         Thread,
         Http
      }

      public XmlPredicateUser()
         : base("user", "any-of;type")
      {
      }

      protected override Func<bool> CreatePredicate(XElement e)
      {
         var users = new HashSet<string>(e.GetAttributeValue(RequiredAttributes[0]).Split(',').Select(s => s.Trim()).Where(s => s.Length > 0)).ToArray();
         var type = e.GetAttributeValue(OptionalAttributes[0], UserType.Application);
         switch (type)
         {
            case UserType.Windows:
               return () => users.Contains(Environment.UserName) || users.Contains(Environment.UserDomainName + "\\" + Environment.UserName);
            case UserType.Application:
               return () =>
               {
                  var windowsIdentity = WindowsIdentity.GetCurrent();
                  return windowsIdentity != null && (users.Contains(windowsIdentity.Name) || users.Contains(windowsIdentity.Name.Split('\\').Last()));
               };
            case UserType.Thread:
               return () =>
               {
                  var identity = Thread.CurrentPrincipal != null ? Thread.CurrentPrincipal.Identity : null;
                  return identity != null && users.Contains(identity.Name);
               };
            case UserType.Http:
               return () =>
               {
                  var identity = HttpContext.Current != null && HttpContext.Current.User != null ? HttpContext.Current.User.Identity : null;
                  return identity != null && users.Contains(identity.Name);
               };
            default:
               throw new ArgumentOutOfRangeException();
         }
      }

   }
}