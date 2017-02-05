using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xioc.Config.Common;
using Xioc.Core;
using Xioc.Wcf;
using XPression.Core;
using XPression.Core.Functions;
using XPression.Language.Syntax;
using XPression.Core.Tokens;
using XPression.Language.Syntax.DSL.Helpers;

namespace Xioc.Config
{
   class ConfigScriptContext : DefaultSyntaxExtender
   {

      private class OnNewScopeBinder : IBinder
      {
         private readonly IBinder _innerBinder;

         public List<Action<IBinder>> OnNewScopeActions = new List<Action<IBinder>>();

         public OnNewScopeBinder(IBinder inner)
         {
            _innerBinder = inner;
         }

         public bool CanResolve(Type type)
         {
            return _innerBinder.CanResolve(type);
         }

         public bool IsRegistered(Type type)
         {
            return _innerBinder.IsRegistered(type);
         }

         public Binding GetBinding(Type serviceType, bool throwException = true)
         {
            return _innerBinder.GetBinding(serviceType, throwException);
         }

         public IEnumerable<Binding> GetBindings(Type serviceType)
         {
            return _innerBinder.GetBindings(serviceType);
         }

         public IEnumerable<Type> GetServiceTypes()
         {
            return _innerBinder.GetServiceTypes();
         }

         public IBinder Bind(Type serviceType, Type implementationType, Lifestyle lifestyle = Lifestyle.Transient, IDictionary<string, object> dependencies = null)
         {
            OnNewScopeActions.Add(b => b.Bind(serviceType, implementationType, lifestyle, dependencies));
            return this;
         }

         public IBinder Bind(Type serviceType, Func<Context, object> factory, Lifestyle lifestyle = Lifestyle.Transient)
         {
            OnNewScopeActions.Add(b => b.Bind(serviceType, factory, lifestyle));
            return this;
         }

         public IBinder Decorate(Type serviceType, Type decoratorType, Lifestyle lifestyle = Lifestyle.Transient, IDictionary<string, object> dependencies = null)
         {
            OnNewScopeActions.Add(b => b.Decorate(serviceType, decoratorType, lifestyle, dependencies));
            return this;
         }

         public IBinder Decorate(Type serviceType, Func<Context, object> factory, Lifestyle lifestyle = Lifestyle.Transient, IDictionary<string, object> dependencies = null)
         {
            OnNewScopeActions.Add(b => b.Decorate(serviceType, factory, lifestyle, dependencies));
            return this;
         }

         public IBinder OnNewScope(Action<IBinder> binder, int level = 0)
         {
            throw new NotImplementedException();
         }

         public IContainer Container
         {
            get { return _innerBinder.Container; }
         }

         public IScope Scope
         {
            get { return _innerBinder.Scope; }
         }
      }

      public object ExitResult { get; set; }

      private IBinder 
         _binder;


      private readonly List<string> 
         _namespaces = new List<string>();

      private IDictionary<string, Type> 
         _types;

      public ConfigScriptContext() { }


      public ConfigScriptContext(IBinder binder)
      {
         _binder = binder;
      }

      public bool Using(string @namespace)
      {
         _namespaces.Add(@namespace);
         return true;
      }
      [ScriptMethod("bind")]
      public bool Bind(string serviceType, string implementationType, string lifestyle)
      {
         if (lifestyle.ToLower() == "wcf-client")
         {
            _binder.BindWcfService(GetType(serviceType), GetType(implementationType));
         }
         else
         {
            _binder.Bind(GetType(serviceType), GetType(implementationType), lifestyle.ParseEnum<Lifestyle>());
         }
         return true;
      }
      [ScriptMethod("bind")]
      public bool Bind(string serviceType, string implementationType)
      {
         _binder.Bind(GetType(serviceType), GetType(implementationType));
         return true;
      }

      internal bool _Bind(string serviceType, Tuple<Type, string> tuple)
      {
         if (tuple.Item2.ToLower() == "wcf-client")
         {
            _binder.BindWcfService(GetType(serviceType), tuple.Item1);
         }
         else
         {
            _binder.Bind(GetType(serviceType), tuple.Item1, tuple.Item2.ParseEnum<Lifestyle>());
         }
         
         return true;
      }
      internal bool _Bind(string serviceType, Tuple<Type, Tuple<string, IDictionary<string, object>>> tuple)
      {
         _binder.Bind(GetType(serviceType), tuple.Item1, tuple.Item2.Item1.ParseEnum<Lifestyle>(),tuple.Item2.Item2);
         return true;
      }
      internal bool _Bind(string serviceType, Tuple<string, IDictionary<string, object>> tuple)
      {
         _binder.Bind(GetType(serviceType), GetType(tuple.Item1), dependencies:tuple.Item2);
         return true;
      }

      internal Tuple<Type, string> _As(string implementationType, string lifestyle)
      {
         return Tuple.Create(GetType(implementationType), lifestyle);
      }
      internal Tuple<Type, Tuple<string, IDictionary<string, object>>> _As(string implementationType, Tuple<string, IDictionary<string, object>> tuple)
      {
         return Tuple.Create(GetType(implementationType), tuple);
      }
      internal Tuple<string, IDictionary<string,object>> _WithDependencies(string lifestyleOrImplementationType, IDictionary<string,object> dependencies)
      {
         return Tuple.Create(lifestyleOrImplementationType, dependencies);
      }

      [ScriptMethod("intercept")]
      public bool Intercept(string serviceType, string interceptorType)
      {
         _binder.Intercept(GetType(serviceType), GetType(interceptorType));
         return true;
      }
      [ScriptMethod("decorate")]
      public bool Decorate(string serviceType, string interceptorType)
      {
         _binder.Decorate(GetType(serviceType), GetType(interceptorType));
         return true;
      }
      [ScriptMethod("decorate")]
      public bool Decorate(string serviceType, string interceptorType, string lifestyle)
      {
         _binder.Decorate(GetType(serviceType), GetType(interceptorType), lifestyle.ParseEnum<Lifestyle>());
         return true;
      }
      internal bool _Decorate(string serviceType, Tuple<string, string> tuple)
      {
         return Decorate(serviceType, tuple.Item1, tuple.Item2);
      }

      [ScriptMethod("return")]
      public static bool Return()
      {
         return false;
      }
      [ScriptMethod("return")]
      public bool Return(object result)
      {
         ExitResult = result;
         return false;
      }

      [ScriptMethod("debug")]
      public static bool Debug()
      {
         return BuildHelper.IsDebug();
      }
      [ScriptMethod("has-any-role")]
      public static bool HasAnyRole(params string[] args)
      {
         AccountHelper.AccountType type;
         args = args.GetArgs(out type);
         return args.Any() && AccountHelper.IsInRole(args, null,type);
      }
      [ScriptMethod("has-roles")]
      public static bool HasRoles(params string[] args)
      {
         AccountHelper.AccountType type;
         args = args.GetArgs(out type);
         return args.Any() && AccountHelper.IsInRole(null, args, type);
      }
      [ScriptMethod("is-user")]
      public static bool User(params string[] args)
      {
         AccountHelper.AccountType type;
         args = args.GetArgs(out type);
         return args.Any() && AccountHelper.IsUser(null);
      }


      [ScriptMethod("on-new-scope")]
      public bool OnNewScope(Delegate condition, Delegate bindings, int level)
      {
         var predicate = (Func<ConfigScriptContext, bool>) condition;
         var previousBinder = _binder;
         var onNewScopeBinder = new OnNewScopeBinder(previousBinder);
         _binder = onNewScopeBinder;
         ((Func<ConfigScriptContext, bool>)bindings)(this);
         var collectedActions = onNewScopeBinder.OnNewScopeActions.ToArray();
         _binder = previousBinder;

         _binder.OnNewScope(b =>
         {
            if (predicate != null && !predicate(null)) return;
            foreach (var a in collectedActions)
            {
               a(b);
            }
         },level);
         return true;
      }

      [ScriptMethod("on-new-scope")]
      public bool OnNewScope(Delegate condition,Delegate bindings)
      {
         return OnNewScope(condition, bindings, 0);
      }
      [ScriptMethod("on-new-scope")]
      public bool OnNewScope(Delegate bindings, int level)
      {
         return OnNewScope(null, bindings, level);
      }

      [ScriptMethod("on-new-scope")]
      public bool OnNewScope(Delegate bindings)
      {
         return OnNewScope(bindings,0);
      }

      [ScriptMethod("debug-write")]
      public static bool DebugWrite(string text)
      {
         System.Diagnostics.Debug.WriteLine(text);
         return true;
      }
      [ScriptMethod("console-write")]
      public static bool ConsoleWrite(string text)
      {
         Console.WriteLine(text);
         return true;
      }

      [ScriptMethod("bind-mef-exports")]
      public bool BindMefExports(params string[] assemblyPathNames)
      {
         if (assemblyPathNames == null || !assemblyPathNames.Any())
         {
            _binder.BindMefExports(AppDomain.CurrentDomain.GetAvailableAssemblies());
            return true;
         }
         Lifestyle defaultLifestyle;
         assemblyPathNames = assemblyPathNames.GetArgs(out defaultLifestyle);
         _binder.BindMefExports(assemblyPathNames.SelectMany(LoadAssembly), defaultLifestyle);
         return true;
      }
      [ScriptMethod("bind-xioc-exports")]
      public bool BindXiocExports(params string[] assemblyPathNames)
      {
         if (assemblyPathNames == null || !assemblyPathNames.Any())
         {
            _binder.BindXiocExports(AppDomain.CurrentDomain.GetAvailableAssemblies());
            return true;
         }
         _binder.BindXiocExports(assemblyPathNames.SelectMany(LoadAssembly));
         return true;
      }

      [ScriptMethod("dependencies")]
      public IDictionary<string, object> Dependencies(params object[] args)
      {
         var dependencies = new Dictionary<string, object>(Syntax.StringEqualityComparer);
         string argname = null;
         foreach (var arg in args)
         {
            if (argname == null)
            {
               argname = (string) arg;
               continue;
            }
            dependencies.Add(argname, arg);
            argname = null;
         }
         if (argname != null)
         {
            throw new ApplicationException("no value specified for argument name: " + argname);
         }
         return dependencies;
      }

      [ScriptMethod("$")]
      public Delegate AsDelegate(Delegate d)
      {
         return new Func<object>(() => d.DynamicInvoke(new object[]{null}));
      }

      private static IEnumerable<Assembly> LoadAssembly(string from)
      {
         if (File.Exists(from))
         {
            return new[] { AppDomain.CurrentDomain.EnsureAssemblyIsLoaded(from) };
         }
         if (Directory.Exists(from))
         {
            return AppDomain.CurrentDomain.GetAssembliesFromDirectory(@from);
         }
         throw new ApplicationException("Argument 'from' is not an assembly filename nor a directory name");
      }

      [ScriptMethod("env")]
      public static string Env(string variable)
      {
         return Environment.GetEnvironmentVariable(variable);
      }
      [ScriptMethod("expand")]
      public static string Expand(string name)
      {
         return Environment.ExpandEnvironmentVariables(name);
      }
      [ScriptMethod("windows-roles")]
      public static string GetWindowsRoles(string type)
      {
         return string.Join(",",AccountHelper.GetWindowsRoles(type.ParseEnum<AccountHelper.WindowsAccountType>()).ToArray());
      }
      [ScriptMethod("user-name")]
      public static string GetUserName(string type)
      {
         return AccountHelper.UserName(type.ParseEnum<AccountHelper.AccountType>());
      }

      #region ISyntaxExtender
      public override void ExtendSyntax(ISyntax syntax)
      {
         base.ExtendSyntax(syntax);

         var symbols = syntax.Symbols;

         //syntax.NonBreakingIdentifierChars.Clear();
         syntax.NonBreakingIdentifierChars.Add('-');
         syntax.NonBreakingIdentifierChars.Add('|');

         syntax.SyntaxChars.Remove('-');
         syntax.SyntaxChars.Remove('|');
         syntax.SyntaxChars.Add('-', TokenType.UnKnown); // because it is contained in '->'

         symbols.Add("sub", TokenType.Sub); // replace substract '-' by operator named 'sub' (would we need it?)
         symbols.Add("bw-or", TokenType.BitwiseOr); // replace bitwise or '|' by operator named 'bw-or' (again would we need it?)

         symbols.Add("->", TokenType.FunctionalBinaryOperator); // bind operator
         symbols.Add("<+", TokenType.FunctionalBinaryOperator); // decorate operator
         symbols.Add("<!", TokenType.FunctionalBinaryOperator); // intercept operator
         symbols.Add("as", TokenType.FunctionalBinaryOperator);
         symbols.Add("with", TokenType.FunctionalBinaryOperator);
         symbols.Add("using", TokenType.FunctionalUnaryOperator);


         var functions = syntax.Functions;
         functions.Add(new FunctionMap("->", GetMethodInfo(x => x.Bind(null, null))));
         functions.Add(new FunctionMap("->", GetMethodInfo(x => x._Bind(null, (Tuple<Type, string>)null))));// tuple overload
         functions.Add(new FunctionMap("->", GetMethodInfo(x => x._Bind(null, (Tuple<Type, Tuple<string, IDictionary<string, object>>>)null))));// tuple overload
         functions.Add(new FunctionMap("->", GetMethodInfo(x => x._Bind(null, (Tuple<string, IDictionary<string, object>>)null))));// tuple overload
         functions.Add(new FunctionMap("<+", GetMethodInfo(x => x.Decorate(null, null))));
         functions.Add(new FunctionMap("<+", GetMethodInfo(x => x._Decorate(null,null))));
         functions.Add(new FunctionMap("<!", GetMethodInfo(x => x.Intercept(null, null))));
         functions.Add(new FunctionMap("as", GetMethodInfo(x => x._As(null, (string)null))));
         functions.Add(new FunctionMap("as", GetMethodInfo(x => x._As(null, (Tuple<string, IDictionary<string, object>>)null))));
         functions.Add(new FunctionMap("with", GetMethodInfo(x => x._WithDependencies(null, null))));
         functions.Add(new FunctionMap("using", GetMethodInfo(x => x.Using(null))));
      }
      #endregion

      private MethodInfo GetMethodInfo(Expression<Func<ConfigScriptContext, object>> expression)
      {
         return expression.GetMethodInfo();
      }

      #region Type resolving
      private Type GetType(string name)
      {
         var type = _namespaces
            .Select(ns => TryGetType(ns + "." + name))
            .FirstOrDefault(t => t != null)
            ?? TryGetType(name);
         if (type == null)
         {
            throw new XPressionException("Could not load type: " + name);
         }
         return type;
      }

      private Type TryGetType(string typename)
      {
         Type type;
         Types.TryGetValue(typename, out type);
         return type;
      }

      private IDictionary<string, Type> Types
      {
         get
         {
            var types = _types;
            if (types != null) return types;
            types = new Dictionary<string, Type>(Syntax.StringEqualityComparer);
            foreach (var type in AppDomain.CurrentDomain.GetExportedTypes())
            {
               types[type.FullName] = type;
            }
            return _types ?? (_types = types);
         }
      }
      #endregion



   }
}
