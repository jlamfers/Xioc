using System;
using System.Threading.Tasks;

namespace Xioc.Core
{
   public interface IASyncManager
   {
      Task ExecuteAsync(Action action);
   }
}