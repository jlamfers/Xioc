using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Xioc.Test.Model
{
    public class RecursionError
    {
        public RecursionError(RecursionError2 error)
        {
            
        }
    }
    public class RecursionError2
    {
        public RecursionError2(RecursionError error)
        {
            
        }
    }


    public interface IFoo1 { }
    public interface IFoo2 { }
    public interface IFoo3 { }

    public class Foo1 : IFoo1, IDisposable {
        public void Dispose()
        {
            Disposed = true;
        }

        public bool Disposed { get; set; }
    }
    [Serializable]
    public class Foo2 : IFoo2, IDisposable
    {
        public void Dispose()
        {
            Disposed = true;
        }

        public bool Disposed { get; set; }
    }
    public class Foo3 : IFoo3, IDisposable
    {
        public IFoo1 Foo1 { get; set; }

        public Foo3(IFoo1 foo1 = null)
        {
            Foo1 = foo1;
        }

        public void Dispose()
        {
            Disposed = true;
        }

        public bool Disposed { get; set; }
    }


    public class Foo1Ex : Foo1 { }
    public class Foo2Ex : Foo2 { }
    public class Foo3Ex : Foo3 { }

    public class CompAll
    {
        public IDictionary<int, string> Dict { get; set; }
        public IEnumerable<IDictionary<int, string>> DictEnumerable { get; set; }
        public CompAll(){}

        public CompAll(
            Foo1 foo1,
            IFoo2 foo2,
            IFoo3 foo3,
            IList<IFoo1> foo1IList,
            ICollection<IFoo1> foo1ICollection,
            List<IFoo1> foo1List,
            Collection<IFoo1> foo1Collection,
            IFoo1[] foo1Array,
            IEnumerable<IFoo1> foo1Enumerable,
            IDictionary<int,string> dict,
            IEnumerable<IDictionary<int,string>> dictEnumerable
            )
            : this(foo1, foo2, foo3, foo1IList, foo1ICollection, foo1List, foo1Collection, foo1Array, foo1Enumerable)
        {
            Dict = dict;
            DictEnumerable = dictEnumerable;
        }

        public CompAll(
            Foo1 foo1,
            IFoo2 foo2,
            IFoo3 foo3,
            IList<IFoo1> foo1IList,
            ICollection<IFoo1> foo1ICollection,
            List<IFoo1> foo1List,
            Collection<IFoo1> foo1Collection,
            IFoo1[] foo1Array,
            IEnumerable<IFoo1> foo1Enumerable,
            int someInt = 10,
            DateTime? someDate = null
            )
        {
            Foo1 = foo1;
            Foo2 = foo2;
            Foo3 = foo3;
            Foo1IList = foo1IList;
            Foo1ICollection = foo1ICollection;
            Foo1List = foo1List;
            Foo1Collection = foo1Collection;
            Foo1Array = foo1Array;
            Foo1Enumerable = foo1Enumerable;
            SomeInt = someInt;
            SomeDate = someDate;
        }

        public Foo1 Foo1 { get; set; }
        public IFoo2 Foo2 { get; set; }
        public IFoo3 Foo3 { get; set; }
        public IList<IFoo1> Foo1IList { get; set; }
        public ICollection<IFoo1> Foo1ICollection { get; set; }
        public List<IFoo1> Foo1List { get; set; }
        public Collection<IFoo1> Foo1Collection { get; set; }
        public IFoo1[] Foo1Array { get; set; }
        public IEnumerable<IFoo1> Foo1Enumerable { get; set; }
        public int SomeInt { get; set; }
        public DateTime? SomeDate { get; set; }

    }
}
