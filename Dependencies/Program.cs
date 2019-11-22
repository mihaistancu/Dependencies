using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Dependencies
{
    class Program
    {
        static readonly Dictionary<string, AssemblyName> Dictionary = new Dictionary<string, AssemblyName>();

        static void Main(string[] assemblyPaths)
        {
            foreach (var assemblyPath in assemblyPaths)
            {
                var assembly = AssemblyName.GetAssemblyName(assemblyPath);
                Add(assembly);
            }

            var dependencies = DependencyNames();
            var input = FileNames(assemblyPaths);
            var output = dependencies.Except(input);

            Print(output);
        }

        private static void Add(AssemblyName assemblyName)
        {
            if (Dictionary.ContainsKey(assemblyName.FullName))
            {
                return;
            }

            Dictionary[assemblyName.FullName] = assemblyName;

            var dependencies = Dependencies(assemblyName);
            foreach (var dependency in dependencies)
            {
                Add(dependency);
            }
        }

        private static AssemblyName[] Dependencies(AssemblyName assemblyName)
        {
            try
            {
                var assembly = Assembly.Load(assemblyName);
                return assembly.GetReferencedAssemblies();
            }
            catch
            {
                return new AssemblyName[0];
            }
        }

        private static List<string> DependencyNames()
        {
            return Dictionary.Values.Select(a => a.Name).Distinct().ToList();
        }

        private static List<string> FileNames(IEnumerable<string> files)
        {
            return files.Select(Path.GetFileNameWithoutExtension).ToList();
        }

        private static void Print(IEnumerable<string> assemblyPaths)
        {
            foreach (var assemblyPath in assemblyPaths)
            {
                Console.WriteLine(assemblyPath);
            }
        }
    }
}
