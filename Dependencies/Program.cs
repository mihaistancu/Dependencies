using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CommandLine;
using NuGet;

namespace Dependencies
{
    class Program
    {   
        private static readonly Dictionary<string, Assembly> Assemblies = new Dictionary<string, Assembly>();
        private static readonly Dictionary<string, Assembly> InputAssemblies = new Dictionary<string, Assembly>();
        private static readonly Dictionary<string, Assembly> Dependencies = new Dictionary<string, Assembly>();
        private static readonly Dictionary<string, IPackage> Packages = new Dictionary<string, IPackage>();

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

            ComputePackages(options.NuGetPath);

            var output = Dependencies.Except(InputAssemblies);
            Print(output, options.IncludeMicrosoftAssemblies);

            return 0;
        }

        private static int OnNotParsedArgs(IEnumerable<Error> errors)
        {
            foreach (var error in errors)
            {
                Console.WriteLine(error.Tag);
            }

            return 1;
        }

        private static void LoadAssemblies(string path)
        {
            new List<string>{"*.dll", "*.exe"}.ForEach(searchPattern =>
            {
                foreach (var file in Directory.GetFiles(path, searchPattern))
                {
                    try
                    {
                        var assembly = Assembly.LoadFrom(file);
                        var filename = Path.GetFileName(file);
                        Assemblies[filename] = assembly;
                    }
                    catch { }
                }
            });
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

        private static void ComputeDependencies(Assembly assembly, bool isRecursive)
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
                            ComputeDependencies(dependencyPathAndAssembly.Value, true);
                        }
                    }
                }
            }
        }

        private static void ComputePackages(string nugetPath)
        {
            var repository = new LocalPackageRepository(nugetPath);
            foreach (var dependency in Dependencies)
            {
                foreach (IPackage package in repository.GetPackages())
                {
                    var files = package.GetFiles();
                    if (files.Any(f => f.Path.Contains(dependency.Key)))
                    {
                        if (!Packages.ContainsKey(dependency.Key))
                        {
                            Packages[dependency.Key] = package;
                        }
                    }
                }
            }
        }

        private static string Copyright(Assembly assembly)
        {
            try
            {
                var attribute = (AssemblyCopyrightAttribute) assembly.GetCustomAttribute(typeof(AssemblyCopyrightAttribute));
                return attribute?.Copyright;
            }
            catch
            {
                return null;
            }
        }

        private static void Print(IEnumerable<KeyValuePair<string, Assembly>> dependencies, bool includeMicrosoft)
        {
            foreach (var dependency in dependencies)
            {
                var copyright = Copyright(dependency.Value);
                if (!string.IsNullOrEmpty(copyright) && copyright.Contains("Microsoft") && !includeMicrosoft)
                {
                    continue;
                }

                Console.WriteLine("├─ " + dependency.Key);

                if (Packages.ContainsKey(dependency.Key))
                {
                    Console.WriteLine("│  ├─ NuGet Package: " + Packages[dependency.Key].Id);
                    Console.WriteLine("│  ├─ Version: " + Packages[dependency.Key].Version);
                    Console.WriteLine("│  ├─ Project URL: " + Packages[dependency.Key].ProjectUrl);
                    Console.WriteLine("│  ├─ License URL: " + Packages[dependency.Key].LicenseUrl);
                }

                Console.WriteLine("│  ├─ Copyright: " + Copyright(dependency.Value));
            }
        }
    }
}
