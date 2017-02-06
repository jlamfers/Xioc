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
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Web;

namespace Xioc.Config.Common
{
   public static class AccountHelper
   {
      [Flags]
      public enum AccountType
      {
         Windows = 1,
         Application = 2,
         Thread = 4,
         Web = 8
      }

      public enum WindowsAccountType
      {
         Windows,
         Application
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

      private static readonly AutoRefreshCache<string, ICollection<string>>
         UserGroups = new AutoRefreshCache<string, ICollection<string>>(TimeSpan.FromMinutes(10), s =>
         {
            var domain = s.Split('\\').First();
            var context = new PrincipalContext(String.Equals(domain, Environment.MachineName, StringComparison.InvariantCultureIgnoreCase) ? ContextType.Machine : ContextType.Domain, domain);
            var user = new UserPrincipal(context) { SamAccountName = s };
            var searcher = new PrincipalSearcher(user);
            user = searcher.FindOne() as UserPrincipal;
            return new HashSet<string>(user.GetGroups().Select(p => p.Name));
         });

      public static bool IsInRole(IList<string> any, IList<string> all, AccountType type = AccountType.Thread | AccountType.Web)
      {
         return type.HasFlag(AccountType.Windows) && IsInWindowsRole(WindowsAccountType.Windows, any, all) ||
                type.HasFlag(AccountType.Application) && IsInWindowsRole(WindowsAccountType.Application, any, all) ||
                type.HasFlag(AccountType.Thread) && IsInRole(Thread.CurrentPrincipal, any, all) ||
                type.HasFlag(AccountType.Web) && HttpContext.Current != null && IsInRole(HttpContext.Current.User, any, all);
      }
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
      public static bool IsInWindowsRole(WindowsAccountType type, IList<string> any, IList<string> all)
      {
         var fullUserName = GetWindowsUserName(type);
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
      public static string GetWindowsUserName(WindowsAccountType type)
      {
         var fullUserName = type == WindowsAccountType.Windows
            ? Environment.UserDomainName + "\\" + Environment.UserName
            : WindowsIdentity.GetCurrent().Name;
         return fullUserName;
      }
      public static ICollection<string> GetWindowsRoles(WindowsAccountType type)
      {
         return UserGroups.Get(GetWindowsUserName(type));
      }

      public static bool IsUser(IList<string> any, AccountType type = AccountType.Thread | AccountType.Web)
      {
         return type.HasFlag(AccountType.Windows) && any.Contains(GetWindowsUserName(WindowsAccountType.Windows)) ||
                type.HasFlag(AccountType.Application) && any.Contains(GetWindowsUserName(WindowsAccountType.Application)) ||
                type.HasFlag(AccountType.Thread) && Thread.CurrentPrincipal.Identity != null && Thread.CurrentPrincipal.Identity.Name != null && any.Contains(Thread.CurrentPrincipal.Identity.Name) ||
                type.HasFlag(AccountType.Web) && HttpContext.Current != null && HttpContext.Current.User != null && HttpContext.Current.User.Identity != null && any.Contains(HttpContext.Current.User.Identity.Name);
      }

      public static string UserName(AccountType type)
      {
         switch (type)
         {
            case AccountType.Windows:
               return GetWindowsUserName(WindowsAccountType.Windows);
            case AccountType.Application:
               return GetWindowsUserName(WindowsAccountType.Application);
            case AccountType.Thread:
               return Thread.CurrentPrincipal.Identity != null ? Thread.CurrentPrincipal.Identity.Name : null;
            case AccountType.Web:
               return (HttpContext.Current != null && HttpContext.Current.User != null && HttpContext.Current.User.Identity != null) ? HttpContext.Current.User.Identity.Name : null;
            default:
               throw new ArgumentOutOfRangeException("type");
         }
      }


   }
}
