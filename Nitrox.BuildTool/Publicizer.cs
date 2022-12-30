using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Mono.Cecil;

namespace Nitrox.BuildTool;

public static class Publicizer
{
    public static event EventHandler<string> LogReceived;

    public static async Task PublicizeAsync(IEnumerable<string> files, string outputSuffix = "", string outputPath = null)
    {
        static int ExecuteSingle(string file, ReaderParameters readerParams, string outputSuffix, string outputPath)
        {
            using AssemblyDefinition assemblyDef = AssemblyDefinition.ReadAssembly(file, readerParams);
            string outputName = $"{Path.GetFileNameWithoutExtension(file)}{outputSuffix}{Path.GetExtension(file)}";
            string outputFile = Path.Combine(outputPath!, outputName);
            PublicizeAssemblyDefinition(assemblyDef);
            assemblyDef.Write(outputFile);
            return assemblyDef.MainModule.Types.Count;
        }

        // Ensure target directory exists.
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            outputPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
        if (!string.IsNullOrWhiteSpace(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        // Create dependency resolve for cecil (needed to write dlls that have other dependencies).
        DefaultAssemblyResolver resolver = new();
        ReaderParameters assemblyReaderParams = new() { AssemblyResolver = resolver };
        // Setup assembly resolver for cecil first, to prevent race condition while another assembly is read.
        string[] fileArray = files as string[] ?? files.ToArray();
        foreach (string file in fileArray)
        {
            if (!File.Exists(file))
            {
                throw new FileNotFoundException("Dll to publicize not found", file);
            }

            resolver.AddSearchDirectory(Path.GetDirectoryName(file));
        }
        // Run publicizer in parallel for each Assembly Definition.
        List<Task> assemblyPublicizeTasks = new();
        foreach (string file in fileArray)
        {
            assemblyPublicizeTasks.Add(Task.Run(() =>
            {
                Stopwatch sw = Stopwatch.StartNew();
                int typeCount;
                try
                {
                    typeCount = ExecuteSingle(file, assemblyReaderParams, outputSuffix, outputPath);
                }
                catch (Exception)
                {
                    sw.Stop();
                    throw;
                }
                OnLogReceived($"Publicized '{file}' with {typeCount} types in {Math.Round(sw.Elapsed.TotalSeconds, 2)}s");
            }));
        }
        await Task.WhenAll(assemblyPublicizeTasks);
    }

    private static void PublicizeAssemblyDefinition(AssemblyDefinition assembly)
    {
        static IEnumerable<TypeDefinition> GetAllNestedTypes(IEnumerable<TypeDefinition> types)
        {
            TypeDefinition[] typesArray = types as TypeDefinition[] ?? types.ToArray();
            foreach (TypeDefinition type in typesArray)
            {
                yield return type;
                foreach (TypeDefinition nestedType in GetAllNestedTypes(type.NestedTypes))
                {
                    yield return nestedType;
                }
            }
        }

        PublicizeTypes(GetAllNestedTypes(assembly.MainModule.Types));
    }

    private static void PublicizeTypes(IEnumerable<TypeDefinition> types)
    {
        foreach (TypeDefinition type in types)
        {
            if (type == null)
            {
                continue;
            }

            // Publicize type and nested types.
            if (type.IsNested)
            {
                type.IsNestedPublic = true;
            }
            else
            {
                type.IsPublic = true;
            }
            // Publicize methods on type.
            foreach (MethodDefinition method in type.Methods)
            {
                method.IsPublic = true;
            }
            // Publicize fields (excludes fields if they would cause name conflicts on a type).
            HashSet<string> eventNames = new(type.Events.Select(eventDefinition => eventDefinition.Name));
            foreach (FieldDefinition field in type.Fields)
            {
                if (eventNames.Contains(field.Name))
                {
                    continue;
                }
                field.IsPublic = true;
            }
        }
    }

    private static void OnLogReceived(string message)
    {
        LogReceived?.Invoke(null, message);
    }
}
