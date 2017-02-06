#region  License
/*
Copyright 2017 - Jaap Lamfers - jlamfers@xipton.net

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 * */
#endregion
namespace Xioc
{
   public enum Lifestyle
   {
      /// <summary>
      /// Each resolve request for the same type creates a new instance, disposables are tracked and disposed
      /// when the scope ends (when the corresponding scope is disposed), only if no singleton 
      /// instance depends on such an instance.
      /// </summary>
      Transient,

      /// <summary>
      /// Each resolve request for the same type returns the same instance per scope, disposables are 
      /// disposed when scope ends (when the corresponding scope is disposed) unless any singleton 
      /// instance depends on the corresponding instance.
      /// </summary>
      PerScope,

      /// <summary>
      /// Each resolve request for the same type returns the same instance per container (so it can 
      /// be regarded as a singleton), disposable singletons only are disposed when the container is 
      /// disposed, any disposable dependency lives as long as the singleton (parent) instance lives, 
      /// so all dependencies only are disposed when the container is disposed as well.
      /// <remarks>
      /// Singleton instances only can be bound (registered) at the container (root) level, not at any scope level.
      /// </remarks>
      /// </summary>
      PerContainer,

      /// <summary>
      /// Each resolve request creates a new instance, disposables are NOT tracked nor disposed when 
      /// the scope ends (when the corresponding scope is disposed). Dependencies ARE tracked and 
      /// disposed, when necessary.
      /// </summary>
      TransientNoDispose,

      /// <summary>
      /// Each resolve request creates a new instance, the whole resulting graph is unmanaged, and all
      /// dependent instances are fresh unique instances. So not any disposable nor dependency is 
      /// tracked or disposed when the scope ends. All dependencies are newly created, even if those 
      /// types were bound as singletons.
      /// </summary>
      UnManaged
   }

}