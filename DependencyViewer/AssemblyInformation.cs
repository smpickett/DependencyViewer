using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DependencyViewer
{
    class AssemblyInformation
    {
        public string Name = string.Empty;
        public string File = string.Empty;
        public string PublicKey = string.Empty;
        public string VersionAsm = string.Empty;
        public string VersionFile = string.Empty;
        public string VersionProduct = string.Empty;
        public string Location = string.Empty;
        public string Arch = string.Empty;
        public bool DotNetAssembly;
        public AssemblyName[] ReferencedAssembliesRaw;
        public Dictionary<AssemblyName, AssemblyResovled> ReferencedAssmeblies = new Dictionary<AssemblyName, AssemblyResovled>();
        public List<AssemblyInformation> ChildAssemblies = new List<AssemblyInformation>();
        public List<AssemblyInformation> ParentAssemblies = new List<AssemblyInformation>();
        public bool AllResolved = false;
        public bool StronglySigned = false;
        public string ResolvedNote = string.Empty;
    }

}
