using System;
using System.Threading.Tasks;

namespace Xioc.Core.Internal
{
   internal class ASyncManagerRootImpl : IASyncManager
   {
      public async Task ExecuteAsync(Action action)
      {
         var t = new Task(action);
         t.Start();
         await t;
      }
   }
}