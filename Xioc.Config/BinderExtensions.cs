using System.IO;
using XPression;

namespace Xioc.Config
{
   public static class BinderExtensions
   {
      public static IBinder BindFromConfiguration(this IBinder self, string configScriptOrFileName, out object result)
      {
         var script = File.Exists(configScriptOrFileName) ? File.ReadAllText(configScriptOrFileName) : configScriptOrFileName;
         var parser = new ScriptParser<ConfigScriptContext>(false);
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
