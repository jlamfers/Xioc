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
using System.ComponentModel.Composition;
using System.Reflection;
using Xioc.Core;

namespace Xioc
{
   public interface ISettings
   {
      bool EnableWeakDisposableTracking { get; }
      bool EnableScopeLevelBindings { get; }
      Func<IContainerBase, ConstructorInfo, bool> ImportingConstructorPredicate { get; }
      Func<IContainerBase, MemberInfo, bool> ImportingMemberPredicate { get; }
   }

   public sealed class Settings : ISettings
   {
      private bool _enableWeakDisposableTracking;
      private bool _enableEnableScopeLevelBindings = true;

      bool ISettings.EnableWeakDisposableTracking { get { return _enableWeakDisposableTracking; } }
      bool ISettings.EnableScopeLevelBindings { get { return _enableEnableScopeLevelBindings; } }

      public Func<IContainerBase, ConstructorInfo, bool> ImportingConstructorPredicate { get; set; }
      public Func<IContainerBase, MemberInfo, bool> ImportingMemberPredicate { get; set; }

      public Settings EnableWeakDisposableTracking(bool setting = true)
      {
         _enableWeakDisposableTracking = setting;
         return this;
      }
      public Settings EnableScopeLevelBindings(bool setting = true)
      {
         _enableEnableScopeLevelBindings = setting;
         return this;
      }

      public Settings EnableMef()
      {
         ImportingConstructorPredicate = (ctx, ctor) => Attribute.IsDefined(ctor, typeof (ImportingConstructorAttribute));
         var p = ImportingMemberPredicate;
         ImportingMemberPredicate = (ctx, m) => (p != null && p(ctx,m)) || (Attribute.IsDefined(m, typeof (ImportAttribute)) || Attribute.IsDefined(m, typeof (ImportManyAttribute)));
         return this;
      }

      public Settings EnablePropertyInjection()
      {
         var p = ImportingMemberPredicate;
         ImportingMemberPredicate = (ctx, m) =>
         {
            if (p != null && p(ctx, m)) return true;
            var pi = m as PropertyInfo;
            return pi != null && pi.GetSetMethod() != null && ctx.CanResolve(pi.PropertyType);
         };
         return this;
      }

   }

   internal class ReadOnlySettings : ISettings
   {
      private readonly ISettings _settings;

      public ReadOnlySettings(ISettings settings)
      {
         if (settings == null) throw new ArgumentNullException("settings");
         _settings = settings;
      }

      public bool EnableWeakDisposableTracking { get { return _settings.EnableWeakDisposableTracking; } }
      public bool EnableScopeLevelBindings { get { return _settings.EnableScopeLevelBindings; } }
      public Func<IContainerBase, ConstructorInfo, bool> ImportingConstructorPredicate { get { return _settings.ImportingConstructorPredicate; } }
      public Func<IContainerBase, MemberInfo, bool> ImportingMemberPredicate { get { return _settings.ImportingMemberPredicate; } }
   }
}
