using System;
using System.Collections.Generic;

namespace Xioc.Core.Internal
{
   internal class DisposablesBag : IDisposablesBag
   {
      private List<IDisposable>
          _instancesList = new List<IDisposable>();

      private readonly object _syncroot = new object();

      public void Add(IDisposable disposable)
      {
         lock (_syncroot)
         {
            EnsureNotDisposed();
            _instancesList.Add(disposable);
         }
      }

      public void Dispose()
      {
         List<IDisposable> storage;
         lock (_syncroot)
         {
            storage = _instancesList;
            _instancesList = null;
         }
         if (storage == null) return;
         foreach (var d in storage)
         {
            d.Dispose();
         }
      }

      public void EnsureNotDisposed()
      {
         if (_instancesList == null)
         {
            throw new ObjectDisposedException(GetType().FullName);
         }
      }
   }
}