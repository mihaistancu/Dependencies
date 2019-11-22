using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Dependencies
{
    class Program
    {
        private static string InputPath;
        private static string InputFilter;
        private static readonly Dictionary<string, Assembly> Assemblies = new Dictionary<string, Assembly>();
        private static readonly Dictionary<string, Assembly> InputAssemblies = new Dictionary<string, Assembly>();
        private static readonly Dictionary<string, Assembly> Dependencies = new Dictionary<string, Assembly>();

        static void Main(string[] args)
        {
            InputPath = args[0];
            InputFilter = args[1];

            LoadAssemblies();

            FilterAssemblies();

            foreach (var inputAssembly in InputAssemblies.Values)
            {
                ComputeDependencies(inputAssembly);
            }

            var output = Dependencies.Except(InputAssemblies);
            Print(output);
        }

        private static void LoadAssemblies()
        {
            foreach (var dll in Directory.GetFiles(InputPath, "*.dll"))
            {
                try
                {
                    var assembly = Assembly.LoadFile(dll);
                    var filename = Path.GetFileName(dll);
                    Assemblies[filename] = assembly;
                }
                catch { }
            }
        }

        private static void FilterAssemblies()
        {
            Regex regex = new Regex(InputFilter, RegexOptions.IgnoreCase);
            foreach (var assemblyPath in Assemblies.Keys)
            {
                if (regex.IsMatch(assemblyPath))
                {
                    InputAssemblies[assemblyPath] = Assemblies[assemblyPath];
                }
            }
        }

        private static void ComputeDependencies(Assembly assembly)
        {
            var dependencies = assembly.GetReferencedAssemblies();
            foreach (var dependency in dependencies)
            {
                var dependencyPathAndAssembly = Assemblies.SingleOrDefault(x => x.Value.FullName == dependency.FullName);

                if (!string.IsNullOrEmpty(dependencyPathAndAssembly.Key))
                {
                    if (!Dependencies.ContainsKey(dependencyPathAndAssembly.Key))
                    {
                        Dependencies[dependencyPathAndAssembly.Key] = dependencyPathAndAssembly.Value;
                        ComputeDependencies(dependencyPathAndAssembly.Value);
                    }
                }
            }
        }

        private static void Print(IEnumerable<KeyValuePair<string, Assembly>> dependencies)
        {
            foreach (var dependency in dependencies)
            {
                Console.WriteLine(dependency.Key);
            }
        }
    }
}
