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
using System.IO;
using XPression;

namespace Xioc.Config
{
   public static class BinderExtensions
   {
      public static IBinder BindFromConfiguration(this IBinder self, string configScriptOrFileName, out object result)
      {
         var script = File.Exists(configScriptOrFileName) ? File.ReadAllText(configScriptOrFileName) : configScriptOrFileName;
         var parser = new ScriptParser<ConfigScriptExtender>(false);
         var context = new ConfigScriptContext(self);
         var fn = parser.CompilePredicate<ConfigScriptContext>(script);
         fn(context);
         result = context.ExitResult;
         return self;
      }

      public static IBinder BindFromConfiguration(this IBinder self, string configScriptOrFileName)
      {
         object result;
         return self.BindFromConfiguration(configScriptOrFileName, out result);
      }
   }
}
