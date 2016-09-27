using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Principal;

namespace Xioc.Xml.Core
{
   internal static class RoleHelper
   {
      public enum RoleType
      {
         Windows,
         Application,
         Thread,
         Http
      }

      public class AutoRefreshCache<TKey, TValue>
      {
         private readonly TimeSpan _expiration;
         private readonly Func<TKey, TValue> _refreshCallback;
         
         private readonly ConcurrentDictionary<TKey, Tuple<TValue, DateTime>>
             _innerCache = new ConcurrentDictionary<TKey, Tuple<TValue, DateTime>>();

         public AutoRefreshCache(TimeSpan expiration, Func<TKey, TValue> refreshCallbackExpression)
         {
            if (refreshCallbackExpression == null)
            {
               throw new ArgumentNullException("refreshCallbackExpression");
            }

            _expiration = expiration;
            _refreshCallback = refreshCallbackExpression;
         }

         public TValue Get(TKey key)
         {
            return _innerCache.AddOrUpdate(key,
                         k => Tuple.Create(Refresh(k), DateTime.UtcNow),
                (k, v) => (v.Item2 + _expiration < DateTime.UtcNow)
                              ? Tuple.Create(Refresh(k), DateTime.UtcNow)
                              : v)
                              .Item1;
         }

         private TValue Refresh(TKey key)
         {
            try
            {
               return _refreshCallback(key);
            }
            catch 
            {
               Tuple<TValue, DateTime> value;
               if (_innerCache.TryGetValue(key, out value))
               {
                  // continue to work with current value
                  return value.Item1;
               }

               // if not any item has been loaded for the given key, then retrow the exception
               throw;
            }
         }
      }

      private static readonly AutoRefreshCache<string,ICollection<string>>
         UserGroups = new AutoRefreshCache<string, ICollection<string>>(TimeSpan.FromMinutes(10), s =>
         {
            var domain = s.Split('\\').First();
            var context = new PrincipalContext(String.Equals(domain, Environment.MachineName, StringComparison.InvariantCultureIgnoreCase) ? ContextType.Machine : ContextType.Domain, domain);
            var user = new UserPrincipal(context) { SamAccountName = s };
            var searcher = new PrincipalSearcher(user);
            user = searcher.FindOne() as UserPrincipal;
            return new HashSet<string>(user.GetGroups().Select(p => p.Name));
         });

      public static bool IsInRole(this IPrincipal principal, IList<string> any, IList<string> all)
      {
         if (principal == null) return false;
         if (any != null && any.Count > 0)
         {
            if (!any.Any(principal.IsInRole)) return false;
         }
         if (all != null && all.Count > 0)
         {
            return all.All(principal.IsInRole);
         }
         return false;
      }

      public static bool IsInWindowsRole(RoleType type, IList<string> any, IList<string> all)
      {
         var fullUserName = type == RoleType.Windows
            ? Environment.UserDomainName + "\\" + Environment.UserName
            : WindowsIdentity.GetCurrent().Name;
         var userGroups = UserGroups.Get(fullUserName);
         var anyGroupsDefined = any != null && any.Count > 0;
         if (anyGroupsDefined)
         {
            if (!any.Any(userGroups.Contains))
            {
               return false;
            }
         }
         return all != null && all.Count > 0 ? all.All(userGroups.Contains) : anyGroupsDefined;
      }

   }
}
