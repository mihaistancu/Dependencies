using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CommandLine;

namespace Dependencies
{
    class Program
    {   
        private static readonly Dictionary<string, Assembly> Assemblies = new Dictionary<string, Assembly>();
        private static readonly Dictionary<string, Assembly> InputAssemblies = new Dictionary<string, Assembly>();
        private static readonly Dictionary<string, Assembly> Dependencies = new Dictionary<string, Assembly>();

        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<Options>(args).MapResult(OnParsedArgs, OnNotParsedArgs);
        }

        private static int OnParsedArgs(Options options)
        {
            LoadAssemblies(options.Path);

            FilterAssemblies(options.Filter);

            foreach (var inputAssembly in InputAssemblies.Values)
            {
                ComputeDependencies(inputAssembly, options.Recursive);
            }

            var output = Dependencies.Except(InputAssemblies);
            Print(output);

            return 0;
        }

        private static int OnNotParsedArgs(IEnumerable<Error> errors)
        {
            foreach (var error in errors)
            {
                Console.WriteLine(error.ToString());
            }

            return 1;
        }

        private static void LoadAssemblies(string path)
        {
            foreach (var dll in Directory.GetFiles(path, "*.dll"))
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

        private static void FilterAssemblies(string filter)
        {
            Regex regex = new Regex(filter, RegexOptions.IgnoreCase);
            foreach (var assemblyPath in Assemblies.Keys)
            {
                if (regex.IsMatch(assemblyPath))
                {
                    InputAssemblies[assemblyPath] = Assemblies[assemblyPath];
                }
            }
        }

        private static void ComputeDependencies(Assembly assembly, bool isRecursive = true)
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
                        if (isRecursive)
                        {
                            ComputeDependencies(dependencyPathAndAssembly.Value);
                        }
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
