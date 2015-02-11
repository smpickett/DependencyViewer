using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyViewer
{
    public static class Extensions
    {
        public static string PadRight(this string str, int length, string padding)
        {
            while (str.Length < length)
                str += padding;
            return str.Substring(0,length);
        }
    }
}
