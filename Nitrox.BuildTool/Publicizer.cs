using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;

namespace Nitrox.BuildTool
{
    public static class Publicizer
    {
        public static IEnumerable<string> Execute(IEnumerable<string> files, string outputSuffix = "", string outputPath = null)
        {
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

            foreach (string file in files)
            {
                if (!File.Exists(file))
                {
                    throw new FileNotFoundException("Dll to publicize not found", file);
                }
                resolver.AddSearchDirectory(Path.GetDirectoryName(file));

                string outputName = $"{Path.GetFileNameWithoutExtension(file)}{outputSuffix}{Path.GetExtension(file)}";
                string outputFile = Path.Combine(outputPath, outputName);
                Publicize(file, resolver).Write(outputFile);
                yield return outputFile;
            }
        }

        public static IEnumerable<FieldDefinition> FilterBackingEventFields(List<TypeDefinition> allTypes)
        {
            List<string> eventNames = allTypes.SelectMany(t => t.Events)
                                              .Select(eventDefinition => eventDefinition.Name)
                                              .ToList();

            return allTypes.SelectMany(x => x.Fields)
                           .Where(fieldDefinition => !eventNames.Contains(fieldDefinition.Name));
        }

        /// <summary>
        ///     Method which returns all Types of the given module, including nested ones (recursively).
        /// </summary>
        /// <param name="moduleDefinition">.NET module to search through for types.</param>
        /// <returns>Types found in module.</returns>
        public static IEnumerable<TypeDefinition> GetAllTypes(ModuleDefinition moduleDefinition) => GetAllNestedTypes(moduleDefinition.Types);

        private static AssemblyDefinition Publicize(string file, BaseAssemblyResolver dllResolver)
        {
            AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(file,
                                                                          new ReaderParameters
                                                                          {
                                                                              AssemblyResolver = dllResolver
                                                                          });
            List<TypeDefinition> allTypes = GetAllTypes(assembly.MainModule).ToList();
            foreach (TypeDefinition type in allTypes)
            {
                if (type == null)
                {
                    continue;
                }
                
                // Publicize type and nested types.
                if (!type.IsPublic || !type.IsNestedPublic)
                {
                    if (type.IsNested)
                    {
                        type.IsNestedPublic = true;
                    }
                    else
                    {
                        type.IsPublic = true;
                    }
                }
                // Publicize methods on type.
                foreach (MethodDefinition method in type.Methods)
                {
                    if (!method?.IsPublic ?? false)
                    {
                        method.IsPublic = true;
                    }
                }
            }
            
            // Publicize all fields (excludes fields if they would cause name conflicts on a type).
            foreach (FieldDefinition field in FilterBackingEventFields(allTypes))
            {
                if (!field?.IsPublic ?? false)
                {
                    field.IsPublic = true;
                }
            }

            return assembly;
        }

        /// <summary>
        ///     Recursive method to get all nested types. Use <see cref="GetAllTypes(ModuleDefinition)" />
        /// </summary>
        /// <param name="typeDefinitions"></param>
        /// <returns></returns>
        private static IEnumerable<TypeDefinition> GetAllNestedTypes(IEnumerable<TypeDefinition> typeDefinitions)
        {
            IEnumerable<TypeDefinition> defs = typeDefinitions as TypeDefinition[] ?? typeDefinitions.ToArray();
            if (!defs.Any())
            {
                return Array.Empty<TypeDefinition>();
            }
            
            return defs.Concat(GetAllNestedTypes(defs.SelectMany(t => t.NestedTypes)));
        }
    }
}
