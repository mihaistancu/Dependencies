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
                Console.WriteLine(assembly.FullName);
            }
        }
    }
}
