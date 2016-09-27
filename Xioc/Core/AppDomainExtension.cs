using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Xioc.Core
{
   public static class AppDomainExtension
   {

      private static readonly ConcurrentDictionary<AppDomain, AppDomain>
          ProcessedAppDomains = new ConcurrentDictionary<AppDomain, AppDomain>();


      public static Type[] GetExportedTypes(this Assembly self)
      {
         try
         {
            return self.GetTypes().ToArray();
         }
         catch (ReflectionTypeLoadException e)
         {
            // we still have access to all load attempts
            return e.Types.Where(t => t != null).ToArray();
         }
      }
      public static IEnumerable<Type> GetExportedTypes(this AppDomain self)
      {
         return self.GetAvailableAssemblies().SelectMany(a => a.GetExportedTypes());
      }
      public static IEnumerable<Assembly> GetAvailableAssemblies(this AppDomain self)
      {
         return self.EnsureAvailableAssembliesLoaded().GetAssemblies();
      }
      public static IList<Assembly> GetAssembliesFromDirectory(this AppDomain self, string path)
      {
         if (!Directory.Exists(path))
         {
            var binFolder = !string.IsNullOrEmpty(self.RelativeSearchPath)
                ? Path.Combine(self.BaseDirectory, self.RelativeSearchPath)
                : self.BaseDirectory;
            path = Path.Combine(binFolder, path);
            if (!Directory.Exists(path))
            {
               throw new XiocException("Path not found: " + path);
            }
         }

         return Directory.GetFiles(path, "*.dll")
            .Union(Directory.GetFiles(path, "*.exe"))
            .Select(s => AppDomain.CurrentDomain.EnsureAssemblyIsLoaded(s))
            .Where(a => a != null)
            .ToList();
      }

      private static AppDomain EnsureAvailableAssembliesLoaded(this AppDomain self)
      {
         ProcessedAppDomains.GetOrAdd(self, ad =>
         {

            var binFolder = !string.IsNullOrEmpty(self.RelativeSearchPath)
               ? Path.Combine(self.BaseDirectory, self.RelativeSearchPath)
               : self.BaseDirectory;

            GetAssembliesFromDirectory(self, binFolder);

            return ad;
         });

         return self;
      }
      private static Assembly EnsureAssemblyIsLoaded(this AppDomain self, string assemblyFileName)
      {
         try
         {
            var assemblyName = AssemblyName.GetAssemblyName(assemblyFileName);
            var assembly = self.GetAssemblies().FirstOrDefault(a => AssemblyName.ReferenceMatchesDefinition(assemblyName, a.GetName())) ??
                           self.Load(assemblyName);
            return assembly;
         }
         catch (BadImageFormatException)
         {
            // thrown by GetAssemblyName
            // ignore this assembly since it is an unmanaged assembly
         }
         return null;
      }

   }
}