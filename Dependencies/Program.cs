using System;
using System.Reflection;

namespace Dependencies
{
    class Program
    {
        static void Main(string[] assemblyPaths)
        {
            foreach (var assemblyPath in assemblyPaths)
            {
                var assembly = Assembly.LoadFile(assemblyPath);

                var dependencies = assembly.GetReferencedAssemblies();
                foreach (var dependency in dependencies)
                {
                    Console.WriteLine($"\t{dependency.FullName}");
                }
                Console.WriteLine(assembly.FullName);
            }
        }
    }
}
