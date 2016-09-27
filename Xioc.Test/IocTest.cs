using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xioc.Core;
using Xioc.Test.Model;

namespace Xioc.Test
{
    [TestClass]
    [Serializable]
    public class IocTest
    {
        [TestMethod]
        public void ConstructorWorks()
        {
            ExpectException<ArgumentNullException>(() => new XiocContainer(null));
            new XiocContainer(b => { });
        }

        [TestMethod]
        public void ConstructorWithDefaultArgumentWorks()
        {
            var c = new XiocContainer(b => b.Bind<Foo3>()).BeginScope();
            c.Resolve<Foo3>();
        }

        [TestMethod]
        public void ResolveByDefaultThrowsExceptionWhenNoInstanceIsRegistered()
        {
            var c = new XiocContainer(b => { }).BeginScope();
            ExpectException<XiocException>(() => c.Resolve<IFoo1>());
        }
        [TestMethod]
        public void ResolveThrowsNoExceptionWhenNoInstanceIsRegisteredWithTryResolveAndResolveAll()
        {
            var c = new XiocContainer(b => { }).BeginScope();
            c.TryResolve<IFoo1>();
            c.Resolve<IFoo1>(false);
            c.ResolveAll<IFoo1>().ToList();
        }

        [TestMethod]
        public void SimpleINstanceCanBeResolved()
        {
            var c = new XiocContainer(b => b.Bind<IFoo1,Foo1>()).BeginScope();
            Assert.IsNotNull(c.Resolve<IFoo1>());
            c = new XiocContainer(b => b.Bind<Foo1>()).BeginScope();
            Assert.IsNotNull(c.Resolve<Foo1>());
            Assert.IsNotNull(c.ResolveAll<Foo1>());
            Assert.IsNotNull(c.ResolveAll<IFoo1>()); // ! not registered type
            Assert.AreEqual(1, c.ResolveAll<Foo1>().Count());
        }

        [TestMethod]
        public void TransientScopeWorks()
        {
            var c = new XiocContainer(b => b.Bind<Foo1>().Bind<Foo1>().Bind<Foo2>().Bind<Foo3>()).BeginScope();
            var i1 = c.Resolve<Foo1>();
            var i2 = c.Resolve<Foo1>();
            Assert.AreNotEqual(i1,i2);
            Assert.AreEqual(2,c.ResolveAll<Foo1>().Count());
            Foo1 i3; 
            using (var scope = c.BeginScope(b => b.Bind<Foo1,Foo1Ex>()))
            {
                i3 = scope.Resolve<Foo1>();
                var i4 = scope.Resolve<Foo1>();
                Assert.AreNotEqual(i3, i4);
                Assert.AreEqual(typeof(Foo1Ex),i3.GetType());
                Assert.AreEqual(3, scope.ResolveAll<Foo1>().Count());
            }
            Assert.IsTrue(i3.Disposed);
            Assert.AreSame(typeof(Foo1), c.Resolve<Foo1>().GetType());
            Assert.AreEqual(2, c.ResolveAll<Foo1>().Count());
        }

        [TestMethod]
        public void PerScopeWorks()
        {
            var c = new XiocContainer(b => b.Bind<Foo1>(Lifestyle.PerScope).Bind<Foo1>(Lifestyle.PerScope)).BeginScope();
            var i1 = c.Resolve<Foo1>();
            var i2 = c.Resolve<Foo1>();
            Assert.AreEqual(i1, i2); 
            Assert.AreEqual(2, c.ResolveAll<Foo1>().Count());
            Foo2 i6;
            using (var scope = c.BeginScope(b => b.Bind<Foo2, Foo2Ex>(Lifestyle.PerScope)))
            {
                var i3 = scope.Resolve<Foo1>();
                var i4 = scope.Resolve<Foo1>();
                Assert.AreSame(i3, i4);
                var i5 = scope.Resolve<Foo2>();
                i6 = scope.Resolve<Foo2>();
                Assert.AreSame(i5, i6);
            }
            Assert.IsTrue(i6.Disposed);
            Assert.AreEqual(2, c.ResolveAll<Foo1>().Count());
        }

        [TestMethod]
        public void PerContainerWorks()
        {
            var c = new XiocContainer(b => b.Bind<Foo3>(Lifestyle.PerContainer)).BeginScope();
            var i1 = c.Resolve<Foo3>();
            var i2 = c.Resolve<Foo3>();
            Assert.AreSame(i1,i2);
            Assert.IsNull(i1.Foo1);
            Foo3 i3;
            using (var s = c.BeginScope(b => b.Bind<IFoo1, Foo1>()))
            {
                i3 = c.Resolve<Foo3>();
                Assert.AreSame(i2, i3);
                Assert.IsNull(i3.Foo1);
            }
            Assert.IsFalse(i3.Disposed);

            Foo1 f1;
            c = new XiocContainer(b => b.Bind<Foo3>(Lifestyle.PerContainer)).BeginScope();
            using (var s = c.BeginScope(b => b.Bind<IFoo1, Foo1>(Lifestyle.PerScope)))
            {
                i3 = s.Resolve<Foo3>();
                f1 = (Foo1)s.Resolve<IFoo1>();
                Assert.IsNotNull(f1);
                Assert.IsNull(i3.Foo1);
            }
            Assert.AreSame(i3,c.Resolve<Foo3>());
            Assert.IsFalse(i3.Disposed);
            Assert.IsTrue(f1.Disposed);
        }

        [TestMethod]
        public void PerScopeDisposeWorks()
        {
            var c = new XiocContainer(b => b
                .Bind<Foo1>(Lifestyle.PerScope)
                .Bind<Foo1>(Lifestyle.PerScope)
                .Bind<Foo1>(Lifestyle.PerScope)
            ).BeginScope();
            var i1 = c.Resolve<Foo1>();
            var i2 = c.Resolve<Foo1>();
            Assert.AreSame(i1, i2);
            Foo1 i3;
            IEnumerable<Foo1> all;
            using (var scope = c.BeginScope())
            {
                i1 = scope.Resolve<Foo1>();
                i2 = scope.Resolve<Foo1>();
            }
            using (var scope = c.BeginScope())
            {
                i3 = scope.Resolve<Foo1>();
                all = scope.ResolveAll<Foo1>().ToList();
            }
            Assert.AreSame(i1, i2);
            Assert.AreNotSame(i2, i3);
            Assert.IsTrue(i1.Disposed);
            Assert.IsTrue(i2.Disposed);
            Assert.IsTrue(i3.Disposed);

            Assert.AreEqual(3, all.Count());
            Assert.IsTrue(all.All(d => d.Disposed));
            all = c.ResolveAll<Foo1>().ToList();
            Assert.IsTrue(all.All(d => !d.Disposed));
            c.Dispose();
            Assert.IsTrue(all.All(d => d.Disposed)); 
            Assert.IsTrue(i1.Disposed);
            Assert.IsTrue(i2.Disposed);
        }

        [TestMethod]
        public void RecursionErrorWorks()
        {
            var c = new XiocContainer(b => b.Bind<RecursionError>().Bind<RecursionError2>()).BeginScope();
            ExpectException<XiocException>(() => c.Resolve<RecursionError>());
        }

        [TestMethod]
        public void MaxCompositeTest()
        {
            var c = new XiocContainer(b => b
                .Bind<IFoo1, Foo1>()
                .Bind<IFoo1, Foo1>()
                .Bind<IFoo2, Foo2>()
                .Bind<IFoo2, Foo2>()
                .Bind<IFoo3, Foo3>()
                .Bind<IFoo3, Foo3>()
                .Bind<Foo1>()
                .Bind<Foo1>(Lifestyle.PerScope)
                .Bind<Foo2>()
                .Bind<Foo2>()
                .Bind<Foo3>()
                .Bind<Foo3>()
                .Bind<CompAll>()
                .Bind<CompAll>()
                .Bind(typeof(IDictionary<,>), typeof(ConcurrentDictionary<,>))
                ).BeginScope();
            var x = c.Resolve<CompAll>();
            Assert.IsNotNull(x.Foo1);
            Assert.IsNotNull(x.Foo2);
            Assert.IsNotNull(x.Foo3);
            Assert.IsNotNull(x.Foo1Array);
            Assert.IsNotNull(x.Foo1Collection);
            Assert.IsNotNull(x.Foo1List);
            Assert.IsNotNull(x.Foo1Enumerable);
            var z = x.Foo1Enumerable.ToList();
            Assert.IsNotNull(x.Dict);
            Assert.IsNotNull(x.DictEnumerable);

            var s = c.BeginScope(b => b.Bind(typeof(IDictionary<,>), typeof(Dictionary<,>))
                );

            x = s.Resolve<CompAll>();
            Assert.IsNotNull(x.Dict);
            Assert.IsNotNull(x.DictEnumerable);
            Assert.AreEqual(2,x.DictEnumerable.Count());

            s.Dispose();

            Assert.IsTrue(x.Foo1.Disposed);

            var wr = new WeakReference<IContainer>(s);
            s = null;
            x = null;// !! => because of enumerable, else scope is not garbage collected
            GC.Collect();
            IContainer y;
            Assert.IsFalse(wr.TryGetTarget(out y));
            
        }

        public class Dict
        {
            public IEnumerable<IDictionary<int, string>> List { get; private set; }

            public Dict(IEnumerable<IDictionary<int, string>> list)
            {
                List = list;
            }
        }

        [TestMethod]
        public void EnumerableGenericsAreResolved()
        {
            var c = new XiocContainer(b => b.Bind(typeof (IDictionary<,>), typeof (ConcurrentDictionary<,>)).Bind<Dict>());
            Dict d;
            using (var s = c.BeginScope())
            {
                d = s.Resolve<Dict>();
            }
            Assert.AreEqual(1,d.List.Count());

        }

        [TestMethod]
        public void SingletonCannotBeRegisteredAtScope()
        {
            ExpectException<XiocException>( () => new XiocContainer(b => b.Bind<Foo1>()).BeginScope(b => b.Bind<Foo1>(Lifestyle.PerContainer)));
        }

        [TestMethod]
        public void LoadTest()
        {
            var c = new XiocContainer(b =>
            {
                for (var i = 0; i < 10000; i++)
                {
                    b.Bind<IFoo1, Foo1>();
                }
                for (var i = 0; i < 10000; i++)
                {
                    b.Bind<IFoo2, Foo2>();
                }
                for (var i = 0; i < 10000; i++)
                {
                    b.Bind<IFoo3, Foo3>();
                }
                for (var i = 0; i < 10000; i++)
                {
                    b.Bind<Foo1>();
                }
                for (var i = 0; i < 10000; i++)
                {
                    b.Bind<CompAll>(Lifestyle.PerScope);
                }
            });
            using (var s = c.BeginScope())
            {
                var list = s.ResolveAll<Foo1>().ToList();
                s.ResolveAll<IFoo1>().ToList();
                s.ResolveAll<IFoo2>().ToList();
                s.ResolveAll<IFoo3>().ToList();
                var list2 = s.ResolveAll<CompAll>();
            }

        }

        [TestMethod]
        public void MultiThreadTest()
        {
            var c = new XiocContainer(b =>
            {
                for (var i = 0; i < 10000; i++)
                {
                    b.Bind<IFoo1, Foo1>();
                }
                for (var i = 0; i < 10000; i++)
                {
                    b.Bind<IFoo2, Foo2>();
                }
                for (var i = 0; i < 10000; i++)
                {
                    b.Bind<IFoo3, Foo3>(Lifestyle.PerScope);
                }
                for (var i = 0; i < 10000; i++)
                {
                    b.Bind<Foo1>();
                }
                for (var i = 0; i < 10000; i++)
                {
                    b.Bind<CompAll>(Lifestyle.PerScope);
                }
            });

            var exceptions = new List<Exception>();

            Action job = () =>
            {
                try
                {
                    using (var s = c.BeginScope(b => b.Bind<Foo1, Foo1Ex>()))
                    {
                        s.ResolveAll<Foo1>().ToList();
                        s.ResolveAll<IFoo1>().ToList();
                        s.ResolveAll<IFoo2>().ToList();
                        s.ResolveAll<IFoo3>().ToList();
                        s.ResolveAll<CompAll>().First();
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            };

            int count = 40;
            Thread[] threads = new Thread[count];

            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(() => job());
            }

            foreach (Thread thread in threads)
            {
                thread.Start();
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            Assert.AreEqual(0, exceptions.Count);


        }

        public interface IFoo { }

        public class Foo : IFoo { }
        public class FooByConstructor : IFoo { }

        public class InjectTarget
        {
            [Import]
            private IFoo _foo;
            [ImportMany]
            private IEnumerable<IFoo> _fooEnumerable;
            [Import]
            private IList<IFoo> _fooList;
            [Import]
            private IFoo[] _fooArray;

            [Import]
            public IFoo Foo { get; set; }
            [Import]
            public IEnumerable<IFoo> FooEnumerable { get; set; }
            [Import]
            public IList<IFoo> FooList { get;  protected set; }
            [Import]
            public IFoo[] FooArray { get; private set; }

            public InjectTarget(IFoo foo)
            {
                FooByConstructor = foo;
            }

            public IFoo FooByConstructor { get; set; }

            public void EnsureAllInitialized()
            {
                var fields =
                    this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                var props =
                    this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (fields.Any(f => f.GetValue(this) == null))
                {
                    throw new Exception("Any field is null");
                }
                if (props.Any(f => f.GetValue(this) == null))
                {
                    throw new Exception("Any property is null");
                }
            }
        }
        [TestMethod]
        public void MemberInjectionWorks()
        {
            var c = new XiocContainer(b => b.Bind<IFoo, Foo>().Bind<InjectTarget>(),new Settings().EnableMefImports());

            using (var s = c.BeginScope())
            {
                var obj = s.Resolve<InjectTarget>();
                obj.EnsureAllInitialized();
            }
            using (var s = c.BeginScope())
            {
                var sw = new Stopwatch();
                sw.Start();
                for (var i = 0; i < 10000; i++)
                {
                    var obj = s.Resolve<InjectTarget>();
                }
                sw.Stop();
                Debug.WriteLine(sw.ElapsedMilliseconds);
            }
        }
        [TestMethod]
        public void AutoPropertyInjectionWorks()
        {
           var c = new XiocContainer(b => b.Bind<IFoo, Foo>().Bind<InjectTarget>(), new Settings().EnableAutoPropertyInjection());

           using (var s = c.BeginScope())
           {
              var obj = s.Resolve<InjectTarget>();
              Assert.IsNotNull(obj.Foo);
              Assert.IsNotNull(obj.FooEnumerable);
              Assert.IsNull(obj.FooList);
              Assert.IsNull(obj.FooArray);
           }
           c = new XiocContainer(b => b.Bind<IFoo, Foo>().Bind<InjectTarget>(), new Settings().EnableAutoPropertyInjection(publicOnly:false));

           using (var s = c.BeginScope())
           {
              var obj = s.Resolve<InjectTarget>();
              Assert.IsNotNull(obj.Foo);
              Assert.IsNotNull(obj.FooEnumerable);
              Assert.IsNotNull(obj.FooList);
              Assert.IsNotNull(obj.FooArray);
           }
           using (var s = c.BeginScope())
           {
              var sw = new Stopwatch();
              sw.Start();
              for (var i = 0; i < 10000; i++)
              {
                 var obj = s.Resolve<InjectTarget>();
              }
              sw.Stop();
              Debug.WriteLine(sw.ElapsedMilliseconds);
           }
           c = new XiocContainer(b => b.Bind<IFoo, Foo>().Bind<InjectTarget>());
           using (var s = c.BeginScope())
           {
              s.Resolve<InjectTarget>();
              var sw = new Stopwatch();
              sw.Start();
              for (var i = 0; i < 10000; i++)
              {
                 var obj = s.Resolve<InjectTarget>();
              }
              sw.Stop();
              Debug.WriteLine(sw.ElapsedMilliseconds);
           }

        }

        [TestMethod]
        public void InstanceTypeIsPassed()
        {
            var cr = new XiocContainer(b => b
                .Bind<InjectTarget>()
                .Bind(c => c.InstanceType == typeof (InjectTarget) ? (IFoo) new FooByConstructor() : new Foo()));
            using (var s = cr.BeginScope())
            {
                var r = s.Resolve<IFoo>();
                Assert.AreSame(typeof(Foo), r.GetType());
                var r2 = s.Resolve<InjectTarget>();
                Assert.AreSame(typeof(FooByConstructor), r2.FooByConstructor.GetType());
            };

        }

        [TestMethod]
        public void RequestedTypeIsPassed()
        {
           Type requestedType = null;

           var cr = new XiocContainer(b => b
               .Bind<InjectTarget>()
               .Bind<IFoo>(c =>
               {
                  requestedType = c.RequestedType;
                  return new Foo();
               }));
           using (var s = cr.BeginScope())
           {
              var r = s.Resolve<IFoo>();
              Assert.AreSame(typeof(IFoo), requestedType);
              var r2 = s.Resolve<InjectTarget>();
              Assert.AreSame(typeof(IFoo), requestedType);
           };

        }

        private static void ExpectException<TException>(Action action)
            where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException)
            {
                return;
            }
            throw new  Exception("Exception expected: " + typeof(TException));
            
        }


    }
}
