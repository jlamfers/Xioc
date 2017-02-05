using System;
using System.Collections.Generic;
using System.Linq;

namespace Xioc.Config
{
   internal static partial class Extensions
   {
      public static string[] GetArgs<TEnum>(this string[] args, out TEnum enumType) where TEnum : struct
      {
         enumType = default(TEnum);
         var result = new List<string>();
         foreach (var arg in args)
         {
            if (arg.TryParseEnum<TEnum>(out enumType))
            {
               continue;
            }
            result.Add(arg);
         }
         return result.ToArray();
      }
      public static bool TryParseEnum<T>(this string self, out T enumResult) where T : struct
      {
         if (self.Contains("|"))
         {
            var result = self.Split('|').Select(x => x.Trim()).Aggregate(0, (current, t) => (current | (int)(object)t.ParseEnum<T>()));
            enumResult = (T)Enum.ToObject(typeof(T), result);
            return true;
         }
         return Enum.TryParse(self, true, out enumResult);
      }
      public static T ParseEnum<T>(this string self) where T : struct
      {
         T result;
         if (!self.TryParseEnum(out result))
         {
            throw new ApplicationException("Unable to parse enumvalue for type for type " + typeof(T).Name+": " + self);
         }
         return result;
      }
   
   }
}