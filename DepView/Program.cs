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
            if (args.Length > 0)
            {
                var info = new DependencyViewer.DependencyViewer(args[0]);
                info.DrawTable2();
            }
            else
            {
                var info = new DependencyViewer.DependencyViewer(@"C:\Program Files (x86)\Fiddler2\");
                //info.DrawTable();
                info.DrawTable2();

                Console.WriteLine("Press <enter> to exit");
                Console.ReadKey();
            }
        }
    }
}
