using Sioc.Mvc5;
using System;
using System.Web;
using Xioc;
using Xioc.Core;

[assembly: PreApplicationStartMethod(typeof(XiocHttpModule), "RegisterHttpModule")]

namespace Sioc.Mvc5
{
   public sealed class XiocHttpModule : IHttpModule
   {
      private static readonly Guid ScopeKey = Guid.NewGuid();
      private static IContainer _container;

      public static void RegisterHttpModule()
      {
         HttpApplication.RegisterModule(typeof (XiocHttpModule));
      }

      void IHttpModule.Init(HttpApplication context)
      {
         context.BeginRequest += OnBeginRequest;
         context.EndRequest += OnEndRequest;
      }

      void IHttpModule.Dispose()
      {

      }


      private static void OnBeginRequest(object sender, EventArgs e)
      {
         if (!Initialized)
            return;

         SetRequestScope(_container.BeginScope());
      }

      private static void OnEndRequest(object sender, EventArgs e)
      {
         if (!Initialized)
         {
            return;
         }
         var scope = GetRequestScope();
         SetRequestScope(null); 
         if (scope != null)
         {
            scope.Dispose();
         }
      }

      public static void SetContainer(IContainer container)
      {
         if (container == null) throw new ArgumentNullException("container");
         _container = container;
      }

      public static IScope GetRequestScope()
      {
         var currentScope = HttpContext.Current.Items[ScopeKey] as IScope;
         if (currentScope == null)
         {
            throw new XiocException(
               "Request scope is not avialable (yet). Did you set the container with method XiocScopeHttpModule.SetContainer(IContainer)?");
         }
         return currentScope;
      }


      private static void SetRequestScope(IScope value)
      {
         if (!Initialized)
            return;

         if (value == null)
         {
            HttpContext.Current.Items.Remove(ScopeKey);
         }
         else
         {
            HttpContext.Current.Items[ScopeKey] = value;
         }
      }


      public static bool Initialized
      {
         get { return _container != null; }
      }

   }
}