using System.ComponentModel.Composition;
using Xioc.Test.Shared;

namespace Xioc.Test.Plugin
{
   [Export(typeof(IMyPlugin))]
    public class MyPlugin : IMyPlugin
    {
       public void Doit()
       {
          
       }
    }

   [Export(typeof(IMyPlugin))]
   public class MyPlugin2 : IMyPlugin
   {
      public void Doit()
      {

      }
   }
}
