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
        private Dictionary<string, AssemblyInformation> AsmCollection = new Dictionary<string, AssemblyInformation>();
        private string[] Files = { };

        public DependencyViewer(string root)
        {
            string[] dlls = Directory.GetFiles(root, "*.dll", SearchOption.AllDirectories);
            string[] exes = Directory.GetFiles(root, "*.exe", SearchOption.AllDirectories);

            Files = dlls.Concat(exes).ToArray();

            foreach (var file in Files)
                GatherInformation(file);

            FindRelationships();
        }


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

                        if (refasm.Version.ToString() != found.VersionAsm)
                            asm.ResolvedNote += refasm.Version.ToString() + " -> " + found.VersionAsm;

                        asm.ChildAssemblies.Add(found);
                        found.ParentAssemblies.Add(asm);
                    }
                }
            }
        }

        public void DrawTable()
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

                if (!AsmCollection.Keys.Contains(AssemblyName.GetAssemblyName(file).FullName))
                    AsmCollection.Add(AssemblyName.GetAssemblyName(file).FullName, info);
            }
            catch (FileLoadException)
            {
                info.VersionAsm = AssemblyName.GetAssemblyName(file).Version.ToString();
                info.Name = AssemblyName.GetAssemblyName(file).Name;
                info.StronglySigned = AssemblyName.GetAssemblyName(file).GetPublicKeyToken().Length != 0;
                info.Arch = AssemblyName.GetAssemblyName(file).ProcessorArchitecture.ToString();
                info.ResolvedNote = "Unable to load";

                if (!AsmCollection.Keys.Contains(AssemblyName.GetAssemblyName(file).FullName))
                    AsmCollection.Add(AssemblyName.GetAssemblyName(file).FullName, info);
            }
            catch (BadImageFormatException)
            {
                info.DotNetAssembly = false;
                info.Name = Path.GetFileName(file);
                info.VersionAsm = string.Empty;

                if (!AsmCollection.Keys.Contains(file))
                    AsmCollection.Add(file, info);
            }
        }
    }
}
