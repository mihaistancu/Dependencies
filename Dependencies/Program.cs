using System;

namespace Dependencies
{
    class Program
    {
        static void Main(string[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                Console.WriteLine(assembly);
            }
        }
    }
}
