using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DepView
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine();
                Console.WriteLine("Use: DepView <root path>");
                Console.WriteLine("    where <root Path> is the root directory of the .NET assemblies to look through");
            }

            if (args.Length >= 1)
            {
                var info = new DependencyViewer.DependencyViewer(args[0]);
                info.DrawTable();
            }
        }
    }
}
