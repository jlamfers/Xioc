using System;

namespace Xioc.Core.Internal
{
   internal class WeakDisposablesBag : IDisposablesBag
   {
      private WeakSet<IDisposable>
         _weakInstances = new WeakSet<IDisposable>();

      public void Add(IDisposable disposable)
      {
         EnsureNotDisposed();
         _weakInstances.Add(disposable);
      }

      public void Dispose()
      {
         var instances = _weakInstances;
         _weakInstances = null;
         if (instances == null) return;
         foreach (var d in instances)
         {
            d.Dispose();
         }
      }

      public void EnsureNotDisposed()
      {
         if (_weakInstances == null)
         {
            throw new ObjectDisposedException(GetType().FullName);
         }
      }
   }
}