using System;

namespace Xioc.Core
{
   /// <summary>
   /// The Context class represents the actual request ccontext
   /// </summary>
   public class Context
   {
      internal bool UnManaged;
      internal bool Singleton;
      internal Scope ScopeInternal;

      internal Context(Scope scope, Type requestedType)
      {
         ScopeInternal = scope;
         RequestedType = requestedType;
      }

      /// <summary>
      /// Get the actual requested type
      /// </summary>
      public Type RequestedType { get; internal set; }

      /// <summary>
      /// Get the actual instance type, i.e., the type in which the dependency is injected
      /// </summary>
      public Type InstanceType { get; internal set; }

      /// <summary>
      /// Gets the actual binding
      /// </summary>
      public Binding Binding { get; internal set; }

      /// <summary>
      /// Gets the original scope from which the request was made
      /// </summary>
      public IScope Scope
      {
         get { return ScopeInternal; }
      }

      internal Context Clone(Action<Context> setter = null)
      {
         var clone = new Context(ScopeInternal, RequestedType) { UnManaged = UnManaged, Singleton = Singleton, Binding = Binding, InstanceType = InstanceType };
         if (setter != null)
         {
            setter(clone);
         }
         return clone;
      }
      internal Context CloneWithRequestedType(Type type)
      {
         return type == RequestedType ? this : Clone(c => c.RequestedType = type);
      }
      internal Context CloneWithInstanceType(Type type)
      {
         return type == InstanceType ? this : Clone(c => c.InstanceType = type);
      }
   }
}
