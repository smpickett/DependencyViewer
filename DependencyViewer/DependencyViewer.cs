using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DependencyViewer
{
    public class DependencyViewer
    {
        public DependencyViewer(string root)
        {
            string[] dlls = Directory.GetFiles(root, "*.dll", SearchOption.AllDirectories);
            string[] exes = Directory.GetFiles(root, "*.exe", SearchOption.AllDirectories);
            string[] system = { };// Directory.GetFiles(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "*.dll", SearchOption.AllDirectories);

            Files = dlls.Concat(exes).Concat(system).ToArray();

            foreach (var file in Files)
                GatherInformation(file);

            FindRelationships();
        }

        private Dictionary<string, AssemblyInformation> AsmCollection = new Dictionary<string, AssemblyInformation>();
        private string[] Files = { };


        private void FindRelationships()
        {
            foreach (var asm in AsmCollection.Values)
            {
                if (asm.ReferencedAssembliesRaw == null || asm.ReferencedAssembliesRaw.Count() == 0)
                    continue;

                asm.AllResolved = true;
                foreach (var refasm in asm.ReferencedAssembliesRaw)
                {
                    if (refasm.Name == "mscorlib") continue;
                    if (refasm.Name == "WindowsBase") continue;
                    if (refasm.Name == "PresentationCore") continue;
                    if (refasm.Name == "PresentationFramework") continue;
                    if (refasm.Name.StartsWith("System")) continue;
                    if (refasm.Name.StartsWith("Microsoft")) continue;

                    if (refasm.GetPublicKeyToken().Length != 0)
                    {
                        var found = AsmCollection.Values.FirstOrDefault(i => i.Name == refasm.Name && i.VersionAsm == refasm.Version.ToString());
                        if (found == null)
                        {
                            asm.AllResolved = false;
                            asm.ChildAssemblies.Add(new AssemblyInformation() { Name = refasm.Name, VersionAsm = refasm.Version.ToString() });
                            continue;
                        }

                        asm.ChildAssemblies.Add(found);
                        found.ParentAssemblies.Add(asm);
                    }
                    else
                    {
                        var found = AsmCollection.Values.FirstOrDefault(i => i.Name == refasm.Name);
                        if (found == null)
                        {
                            asm.AllResolved = false;
                            asm.ChildAssemblies.Add(new AssemblyInformation() { Name = refasm.Name, VersionAsm = refasm.Version.ToString() });
                            continue;
                        }

                        asm.ResolvedNote = refasm.Version.ToString() + " > " + found.VersionAsm;

                        asm.ChildAssemblies.Add(found);
                        found.ParentAssemblies.Add(asm);
                    }
                }
            }
        }


        public void DrawTable2()
        {
            string[] headings = {
                "Assembly Name",
                "Ver Asm",
                "Ver File",
                "Ver Prod",
                "", // Architecture
                "Signed",
                "Resolved",
                "Possible Issue",
            };

            int[] widths = {
                AsmCollection.Values.Max(info => info.Name.Length) + 8, /* padding for indents */
                AsmCollection.Values.Max(info => info.VersionAsm.Length),
                AsmCollection.Values.Max(info => info.VersionFile.Length),
                AsmCollection.Values.Max(info => info.VersionProduct.Length),
                AsmCollection.Values.Max(info => info.Arch.Length),
                headings[5].Length,
                headings[6].Length,
                Math.Max(headings[7].Length, AsmCollection.Values.Max(info => info.ResolvedNote.Length))
            };

            PrintHorizontal(widths);
            PrintRow(headings, widths);
            PrintHorizontal(widths);

            foreach (var asm in AsmCollection.Values.Where(i => !i.ParentAssemblies.Any() || i.File.EndsWith("exe")))
                PrintAssembly(asm, 0, widths);

            PrintHorizontal(widths);
        }

        private void PrintAssembly(AssemblyInformation asm, int level, int[] widths)
        {
            if (asm.Location.StartsWith(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory()))
                return;

            string name = "".PadRight(level) + asm.Name;
            if (level > 0)
            {
                name = " ".PadRight(level*"|  ".Length-2, "|  ") + "\\-" + asm.Name;
            }

            string[] values = {
                name,
                asm.VersionAsm,
                asm.VersionFile,
                asm.VersionProduct,
                asm.Arch,
                asm.StronglySigned ? "Signed" : string.Empty,
                asm.DotNetAssembly ? (asm.AllResolved ? "Yes" : "No") : string.IsNullOrEmpty(asm.Location) ? string.Empty : "(N/A)",
                AsmCollection.Values.Where(i => i.Name == asm.Name).Count() > 1 ? "Dupe Asm" : string.IsNullOrEmpty(asm.Location) ? "Not Found" : asm.ResolvedNote,
            };

            PrintRow(values, widths);

            foreach (var refasm in asm.ChildAssemblies)
                PrintAssembly(refasm, level + 1, widths);
        }

        public void PrintRow(string[] headings, int[] widths)
        {
            string headers = string.Empty;
            for (int i = 0; i < widths.Count(); i++)
                headers += "| " + headings[i].PadRight(widths[i] + 1).Substring(0, widths[i] + 1);
            headers += "|";
            Console.WriteLine(headers);
        }

        public void PrintHorizontal(int[] widths)
        {
            if (widths == null)
                return;

            string header = string.Empty;
            for (int i = 0; i < widths.Count(); i++)
                header += "+".PadRight(widths[i] + 3, '-');
            header += "+";

            Console.WriteLine(header);
        }



        private void PrintReferences(AssemblyInformation info, int lenName, int lenVerAsm, int lenVerFile, int lenVerProduct, int lenRes, int level)
        {
            foreach (var asmref in info.ReferencedAssmeblies)
            {
                if (asmref.Key.Name == "mscorlib") continue;
                if (asmref.Key.Name == "WindowsBase") continue;
                if (asmref.Key.Name == "PresentationCore") continue;
                if (asmref.Key.Name == "PresentationFramework") continue; 
                if (asmref.Key.Name.StartsWith("System")) continue;
                if (asmref.Key.Name.StartsWith("Microsoft")) continue;

                if (asmref.Value.Resolved)
                {
                    var foundref = AsmCollection.FirstOrDefault(kvp => kvp.Key == asmref.Key.FullName);
                    Console.WriteLine(String.Format("| {5}-{0} | {1} | {2} | {3} | {4} |",
                        foundref.Value.Name.PadRight(lenName).Substring(0, lenName - level),
                        foundref.Value.VersionAsm.PadRight(lenVerAsm),
                        foundref.Value.VersionFile.PadRight(lenVerFile),
                        foundref.Value.VersionProduct.PadRight(lenVerProduct),
                        foundref.Value.AllResolved ? "Yes".PadRight(lenRes) : "No".PadRight(lenRes),
                        "".PadRight(level)
                        ));

                    PrintReferences(foundref.Value as AssemblyInformation, lenName, lenVerAsm, lenVerFile, lenVerProduct, lenRes, level + 2);
                }
                else
                {
                    Console.WriteLine(String.Format("| {5}-{0} | {1} | {2} | {3} | {4} |",
                        asmref.Key.Name.PadRight(lenName).Substring(0, lenName - level),
                        asmref.Key.Version.ToString().PadRight(lenVerAsm),
                        "".PadRight(lenVerFile),
                        "".PadRight(lenVerProduct),
                        "***".PadRight(lenRes),
                        "".PadRight(level)
                        ));
                }
            }
        }


        public void GatherInformation(string file)
        {
            if (!File.Exists(file))
            {
                Console.WriteLine("File does not exist: " + file);
                return;
            }

            var info = new AssemblyInformation();
            info.Location = Path.GetDirectoryName(file) + Path.DirectorySeparatorChar;
            info.File = Path.GetFileName(file);
            info.VersionProduct = FileVersionInfo.GetVersionInfo(file).ProductVersion ?? string.Empty;
            info.VersionFile = FileVersionInfo.GetVersionInfo(file).FileVersion ?? string.Empty;
            
            try
            {
                info.DotNetAssembly = true;
                info.VersionAsm = AssemblyName.GetAssemblyName(file).Version.ToString();
                info.Name = AssemblyName.GetAssemblyName(file).Name;
                info.StronglySigned = AssemblyName.GetAssemblyName(file).GetPublicKeyToken().Length != 0;
                info.Arch = AssemblyName.GetAssemblyName(file).ProcessorArchitecture.ToString();

                var asm = Assembly.LoadFrom(file);
                info.ReferencedAssembliesRaw = asm.GetReferencedAssemblies();
                foreach (var refasm in asm.GetReferencedAssemblies())
                    info.ReferencedAssmeblies.Add(refasm, new AssemblyResovled() { Resolved = false });
                
                if (!AsmCollection.Keys.Contains(AssemblyName.GetAssemblyName(file).FullName))
                    AsmCollection.Add(AssemblyName.GetAssemblyName(file).FullName, info);
            }
            catch (BadImageFormatException)
            {
                info.DotNetAssembly = false;
                info.Name = Path.GetFileName(file);
                info.VersionAsm = string.Empty;
                AsmCollection.Add(file, info);
            }
        }
    }
}
