// Usage: Pdb2Mdb <dll> [<dll>...]
// For each dll, reads the portable PDB next to it and writes <dll>.mdb next to it. The dll itself is never rewritten:
// the .mdb is keyed by the dll's own method tokens, so rewriting the dll with Cecil (which renumbers tokens) would break the mapping.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Mdb;

int failures = 0;
foreach (string arg in args)
{
    string path = Path.GetFullPath(arg);
    try
    {
        DefaultAssemblyResolver resolver = new();
        resolver.AddSearchDirectory(Path.GetDirectoryName(path));
        // Optional extra directories to resolve references from (e.g. the game's Managed folder), separated by the OS path separator.
        foreach (string dir in (Environment.GetEnvironmentVariable("PDB2MDB_EXTRA_DIRS") ?? "").Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            resolver.AddSearchDirectory(dir);
        }

        ReaderParameters readerParameters = new()
        {
            ReadSymbols = true,
            AssemblyResolver = resolver,
            SymbolReaderProvider = new DefaultSymbolReaderProvider(false)
        };
        using AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(path, readerParameters);
        if (!assembly.MainModule.HasSymbols)
        {
            Console.WriteLine($"Pdb2Mdb: no symbols for {path}, skipped");
            continue;
        }

        // Writes <path>.mdb (MonoSymbolWriter names it after the assembly path).
        using (MdbWriter writer = new(assembly.MainModule, path))
        {
            IEnumerable<MethodDebugInformation> infos = assembly.MainModule.GetTypes()
                                                                .SelectMany(type => type.Methods)
                                                                .Select(method => method.DebugInformation)
                                                                .Where(info => info is { HasSequencePoints: true });
            foreach (MethodDebugInformation info in infos)
            {
                writer.Write(info);
            }
        }
        Console.WriteLine($"Pdb2Mdb: wrote {path}.mdb");
    }
    catch (Exception ex)
    {
        failures++;
        Console.Error.WriteLine($"Pdb2Mdb: failed for {path}: {ex.Message}");
    }
}
return failures == 0 ? 0 : 1;
