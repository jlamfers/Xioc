using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xioc.Test
{

    [TestClass]
    public class DisposeTest
    {
        public class Disposable : IDisposable
        {
            public void Dispose()
            {
                Disposed = true;
            }

            public bool Disposed { get; private set; }
        }

        public class Aap : Disposable
        {
            public Noot Noot { get; private set; }
            public Mies Mies { get; private set; }

            public Aap()
            {
            }

            public Aap(Noot noot)
            {
                Noot = noot;
            }

            public Aap(Noot noot, Mies mies)
            {
                Noot = noot;
                Mies = mies;
            }

        }

        public class Noot : Disposable
        {
        }

        public class Mies : Disposable
        {
        }

        public class NootEx : Noot
        {
        }

        public interface IFoo { }
        public class Foo : Disposable, IFoo { }
        public class Foo2 : Disposable, IFoo { }
        public class Foo3 : Disposable, IFoo { }
        public class FooDecorator : Disposable, IFoo
        {
            public IFoo Foo { get; set; }

            public FooDecorator(IFoo foo)
            {
                Foo = foo;
            }
        }

        [TestMethod]
        public void TransientsAreDisposed()
        {
            var c = new XiocContainer(b => b.Bind<Aap>().Bind<Noot>());
            Aap a1, a2;
            using (var s = c.BeginScope())
            {
                a1 = s.Resolve<Aap>();
                a2 = s.Resolve<Aap>();
            }
            Assert.AreNotSame(a1, a2);
            Assert.IsTrue(a1.Disposed);
            Assert.IsTrue(a2.Disposed);

            using (var s = c.BeginScope(b => b.Bind(x => new Aap(x.Scope.Resolve<Noot>()))))
            {
                a1 = s.Resolve<Aap>();
                a2 = s.Resolve<Aap>();
            }
            Assert.AreNotSame(a1, a2);
            Assert.IsTrue(a1.Disposed);
            Assert.IsTrue(a2.Disposed);
        }

        [TestMethod]
        public void PerScopesAreDisposed()
        {
            var c = new XiocContainer(b => b.Bind<Aap>(Lifestyle.PerScope).Bind<Noot>());
            Aap a1, a2;
            using (var s = c.BeginScope())
            {
                a1 = s.Resolve<Aap>();
                a2 = s.Resolve<Aap>();
            }
            Assert.AreSame(a1, a2);
            Assert.IsTrue(a1.Disposed);
            Assert.IsTrue(a1.Noot.Disposed);
        }

        [TestMethod]
        public void SingletonsAreNeverDisposedByScope()
        {
            var c = new XiocContainer(b => b.Bind<Aap>().Bind<Noot>(Lifestyle.PerContainer));
            Aap a1, a2;
            using (var s = c.BeginScope())
            {
                a1 = s.Resolve<Aap>();
                a2 = s.Resolve<Aap>();
            }
            Assert.AreNotSame(a1, a2);
            Assert.AreSame(a1.Noot, a2.Noot);
            Assert.IsTrue(a1.Disposed);
            Assert.IsTrue(a2.Disposed);
            Assert.IsFalse(a2.Noot.Disposed);

            c.Dispose();
            Assert.IsTrue(a2.Noot.Disposed);
        }

        [TestMethod]
        public void SingletonsAreNeverDisposedByScope2()
        {
            var c = new XiocContainer(b => b.Bind<Aap>(Lifestyle.PerContainer).Bind<Noot>());
            Aap a1, a2;
            using (var s = c.BeginScope())
            {
                a1 = s.Resolve<Aap>();
                a2 = s.Resolve<Aap>();
            }
            Assert.AreSame(a1, a2);
            Assert.IsFalse(a1.Disposed);
            Assert.IsFalse(a2.Noot.Disposed);

            c.Dispose();
            Assert.IsTrue(a1.Disposed);
            Assert.IsTrue(a2.Noot.Disposed);
        }

        [TestMethod]
        public void UnmangedLifestyleAlwaysCreatesFreshInstances()
        {
            var c = new XiocContainer(b => b.Bind<Aap>(Lifestyle.UnManaged).Bind<Noot>(Lifestyle.PerContainer));
            Aap a1, a2;
            using (var s = c.BeginScope())
            {
                a1 = s.Resolve<Aap>();
                a2 = s.Resolve<Aap>();
            }
            Assert.AreNotSame(a1, a2);
            Assert.AreNotSame(a1.Noot, a2.Noot);//!
            Assert.IsFalse(a1.Disposed);
            Assert.IsFalse(a1.Noot.Disposed);
            c.Dispose();
            Assert.IsFalse(a1.Disposed);
            Assert.IsFalse(a2.Noot.Disposed);

            c = new XiocContainer(b => b.Bind<Aap>(Lifestyle.PerContainer).Bind<Noot>(Lifestyle.PerContainer));
            using (var s = c.BeginScope(b => b.Bind<Aap>(Lifestyle.UnManaged)))
            {
                a1 = s.Resolve<Aap>();
                a2 = s.Resolve<Aap>();
            }
            Assert.AreNotSame(a1, a2);
            Assert.AreNotSame(a1.Noot, a2.Noot);//! dependencies are handled unmanged as well
            Assert.IsFalse(a1.Disposed);
            Assert.IsFalse(a1.Noot.Disposed);
            c.Dispose();
            Assert.IsFalse(a1.Disposed);
            Assert.IsFalse(a2.Noot.Disposed);

            c = new XiocContainer(b => b.Bind<Aap>(Lifestyle.PerContainer).Bind<Noot>(Lifestyle.UnManaged));
            using (var s = c.BeginScope())
            {
                a1 = s.Resolve<Aap>();
                a2 = s.Resolve<Aap>();
            }
            Assert.AreSame(a1, a2);
            Assert.AreSame(a1.Noot, a2.Noot);
            Assert.IsFalse(a1.Disposed);
            Assert.IsFalse(a1.Noot.Disposed);
            c.Dispose();
            Assert.IsTrue(a1.Disposed);
            Assert.IsFalse(a1.Noot.Disposed);


        }

        [TestMethod]
        public void DecoratorAndTargetAreDisposed()
        {
            var c = new XiocContainer(b => b.Bind<IFoo, Foo>());
            IFoo foo1, foo2;
            using (var s1 = c.BeginScope())
            {
                foo1 = s1.Resolve<IFoo>();
                Assert.AreSame(typeof(Foo),foo1.GetType());
                using (var s2 = s1.BeginScope(b => b.Bind<IFoo,FooDecorator>()))
                {
                    foo2 = s2.Resolve<IFoo>();
                    var f = foo2.CastTo<FooDecorator>();
                    Assert.AreSame(typeof(Foo), f.Foo.GetType());
                }
            }
            var d = foo1.CastTo<Disposable>();
            Assert.IsTrue(d.Disposed);
            var e = foo2.CastTo<FooDecorator>();
            Assert.IsTrue(e.Disposed);
            Assert.IsTrue(e.Foo.CastTo<Disposable>().Disposed);
        }

        [TestMethod]
        public void DecoratorAndTargetAreDisposed2()
        {
            var c = new XiocContainer(b => b.Bind<IFoo, Foo>());
            IFoo foo1, foo2;
            using (var s1 = c.BeginScope())
            {
                foo1 = s1.Resolve<IFoo>();
                Assert.AreSame(typeof(Foo), foo1.GetType());
                using (var s2 = s1.BeginScope(b => b.Bind<IFoo>(x => new FooDecorator(x.Binding.DecoratorTarget.GetInstance<IFoo>(x)))))
                {
                    foo2 = s2.Resolve<IFoo>();
                    var f = foo2.CastTo<FooDecorator>();
                    Assert.AreSame(typeof(Foo), f.Foo.GetType());
                }
            }
            var d = foo1.CastTo<Disposable>();
            Assert.IsTrue(d.Disposed);
            var e = foo2.CastTo<FooDecorator>();
            Assert.IsTrue(e.Disposed);
            Assert.IsTrue(e.Foo.CastTo<Disposable>().Disposed);
        }

        [TestMethod]
        public void DecoratorAndTargetAreDisposed3()
        {
            var c = new XiocContainer(b => b.Bind<IFoo, Foo>().Bind<IFoo, Foo2>().Bind<IFoo, Foo3>().Bind<IFoo, FooDecorator>());
            IFoo foo1, foo2;
            using (var s1 = c.BeginScope())
            {
                foo1 = s1.Resolve<IFoo>();
                using (var s2 = s1.BeginScope(b => b.Bind<IFoo>(x => new FooDecorator(x.Binding.DecoratorTarget.GetInstance<IFoo>(x)))))
                {
                    foo2 = s2.Resolve<IFoo>();
                }
            }
            var d = foo1.CastTo<Disposable>();
            Assert.IsTrue(d.Disposed);
            d = foo2.CastTo<Disposable>();
            Assert.IsTrue(d.Disposed);

            using (var s1 = c.BeginScope())
            {
                foo1 = s1.Resolve<IFoo>();
                using (var s2 = s1.BeginScope(b => b.Decorate<IFoo, FooDecorator>()))
                {
                    foo2 = s2.Resolve<IFoo>();
                    var list = s2.ResolveAll<IFoo>().ToList();
                    Assert.AreEqual(4,list.Count);
                    Assert.IsTrue(list.All(i => i.GetType() == typeof (FooDecorator)));
                }
            }
            d = foo1.CastTo<Disposable>();
            Assert.IsTrue(d.Disposed);
            d = foo2.CastTo<Disposable>();
            Assert.IsTrue(d.Disposed);

        }

    }

}
