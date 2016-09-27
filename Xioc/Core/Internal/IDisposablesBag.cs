using System;

namespace Xioc.Core.Internal
{
   internal interface IDisposablesBag : IDisposable
   {
      void Add(IDisposable disposable);
   }
}
