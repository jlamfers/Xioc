using System;
using System.Linq;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Xioc.Xml.Core;

namespace Xioc.Xml.Elements
{
   [XmlConfigElement]
   public class XmlPredicateRole : XmlConfigElementPredicate
   {

      public XmlPredicateRole()
         : base("role", ";any-of,all-of,type")
      {
      }

      protected override Func<bool> CreatePredicate(XElement e)
      {
         var usersAnyOf = e.GetAttributeValue(OptionalAttributes[0], "").Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();
         var usersAllOf = e.GetAttributeValue(OptionalAttributes[1], "").Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();
         if (!usersAnyOf.Any() && !usersAllOf.Any())
         {
            throw new XmlException("At least with element <role> one the attributes 'any-of', 'all-of' must be set.");
         }
         var type = e.GetAttributeValue(OptionalAttributes[2], RoleHelper.RoleType.Application);
         switch (type)
         {
            case RoleHelper.RoleType.Windows:
            case RoleHelper.RoleType.Application:
               return () => RoleHelper.IsInWindowsRole(type, usersAnyOf, usersAllOf);
            case RoleHelper.RoleType.Thread:
               return () => Thread.CurrentPrincipal.IsInRole(usersAnyOf, usersAllOf);
            case RoleHelper.RoleType.Http:
               return HttpContext.Current != null
                  ? (Func<bool>) (() => HttpContext.Current.User.IsInRole(usersAnyOf, usersAllOf))
                  : () => false;
            default:
               throw new ArgumentOutOfRangeException();
         }
      }

   }

}