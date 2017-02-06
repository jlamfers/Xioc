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
         TEnum t;
         foreach (var arg in args)
         {
            if (arg.TryParseEnum(out t))
            {
               enumType = t;
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