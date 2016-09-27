using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Xioc.Core.Internal
{
   internal sealed class WeakSet<T> : ICollection<T>, ICollection where T : class
   {
      private readonly Action<T> _finalizer;

      private class LinkedListNodeFinalizer
      {
         private readonly WeakSet<T> _parent;
         public LinkedListNode<WeakReference<T>> Node;
         private readonly T _value;

         public LinkedListNodeFinalizer(WeakSet<T> parent, LinkedListNode<WeakReference<T>> node)
         {
            _parent = parent;
            Node = node;
            Node.Value.TryGetTarget(out _value);
         }

         ~LinkedListNodeFinalizer()
         {
            var node = Node;
            if (node == null || node.List == null) return;
            lock (_parent._syncroot)
            {
               if (node.List == null) return;
               node.List.Remove(node);
            }
            if (_parent._finalizer != null)
            {
               _parent._finalizer(_value);
            }
         }
      }

      private readonly LinkedList<WeakReference<T>>
          _weakList = new LinkedList<WeakReference<T>>();

      private readonly ConditionalWeakTable<T, LinkedListNodeFinalizer>
          _weakTable = new ConditionalWeakTable<T, LinkedListNodeFinalizer>();

      private readonly object
          _syncroot = new object();

      public WeakSet(Action<T> finalizer = null)
      {
         _finalizer = finalizer;
      }

      public int WeakReferenceCount
      {
         get
         {
            lock (_syncroot)
            {
               return _weakList.Count;
            }
         }
      }

      public int Count
      {
         get
         {
            lock (_syncroot)
            {
               return GetLiveListEnumerable().Count();
            }
         }
      }

      public object SyncRoot { get { return _syncroot; } }
      public bool IsSynchronized { get { return true; } }
      public void CopyTo(Array array, int index)
      {
         Array.Copy(GetLiveList().ToArray(), 0, array, index, array.Length);
      }

      public bool IsReadOnly
      {
         get { return false; }
      }

      public bool Add(T item)
      {
         lock (_syncroot)
         {
            if (Contains(item)) return false;
            var node = new LinkedListNode<WeakReference<T>>(new WeakReference<T>(item));
            _weakList.AddLast(node);
            _weakTable.Add(item, new LinkedListNodeFinalizer(this, node));
            return true;
         }
      }

      void ICollection<T>.Add(T item)
      {
         Add(item);
      }

      public void AddRange(IEnumerable<T> list)
      {
         lock (_syncroot)
         {
            foreach (var item in list)
            {
               Add(item);
            }
         }
      }

      public bool Remove(T item)
      {
         lock (_syncroot)
         {
            LinkedListNodeFinalizer node;
            if (!_weakTable.TryGetValue(item, out node))
            {
               return false;
            }

            _weakList.Remove(node.Node);
            node.Node = null;
            _weakTable.Remove(item);
            return true;
         }
      }

      public void Clear()
      {
         lock (_syncroot)
         {
            _weakList.Clear();
         }
      }

      public bool Contains(T item)
      {
         lock (_syncroot)
         {
            LinkedListNodeFinalizer value;
            return _weakTable.TryGetValue(item, out value);
         }
      }

      public void CopyTo(T[] array, int arrayIndex)
      {
         GetLiveList().CopyTo(array, arrayIndex);
      }

      public IEnumerator<T> GetEnumerator()
      {
         return GetLiveList().GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }

      private IEnumerable<T> GetLiveListEnumerable()
      {
         var current = _weakList.First;
         while (current != null)
         {
            var wr = current.Value;
            T target;
            if (!wr.TryGetTarget(out target))
            {
               var t = current;
               current = current.Next;
               _weakList.Remove(t);
               continue;
            }
            yield return target;
            current = current.Next;
         }
      }

      private List<T> GetLiveList()
      {
         lock (_syncroot)
         {
            var result = new List<T>(_weakList.Count);
            result.AddRange(GetLiveListEnumerable());
            return result;
         }
      }

   }
}