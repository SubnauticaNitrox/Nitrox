using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

// Reloader based on http://github.com/pardeike/Reloader
// Modified to remove RimWorld dependency, and cleanups.

namespace NitroxReloader
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ReloadableMethod : Attribute
    {
    }

    public class Reloader
    {
        private static readonly HashSet<string> assemblies = new HashSet<string>() {
            "NitroxModel.dll",
            "NitroxClient.dll"
        };

        private Dictionary<string, MethodInfo> reloadableMethods;

        public Reloader()
        {
            reloadableMethods = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly =>
            {
                string location;
                try
                {
                    // Dynamic assemblies do not have a Location and thus cause an error.
                    location = assembly.Location;
                }
                catch (NotSupportedException)
                {
                    return false;
                }
                return assemblies.Contains(Path.GetFileName(location));
            })
            .SelectMany(assembly =>
            {
                Console.WriteLine("Reloader: reading assembly " + assembly.FullName);
                return assembly.GetTypes();
            })
            .SelectMany(type => type.GetMethods(allBindings))
            .Where(IsMarkedReloadable)
            .ToDictionary(method =>
            {
                var key = QualifiedName(method);
                Console.WriteLine("Reloader: found reloadable method " + key);
                return key;
            }, method => method);

            var managedFolder = @"D:\Users\marij\Documents\code\VSProjects\Nitrox\bin\Debug";
            //Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var watcher = new FileSystemWatcher()
            {
                Path = managedFolder,
                Filter = "*.dll",
                NotifyFilter = NotifyFilters.CreationTime
                | NotifyFilters.LastWrite
                | NotifyFilters.FileName
                | NotifyFilters.DirectoryName
            };
            FileSystemEventHandler handler = (s, e) =>
            {
                var fn = Path.GetFileName(e.Name);
                Console.WriteLine($"Reloader: {fn} {e.ChangeType}");
                if (assemblies.Contains(fn))
                    ReloadAssembly(e.FullPath);
            };
            watcher.Created += handler;
            watcher.Changed += handler;
            watcher.EnableRaisingEvents = true;
        }

        public void ReloadAssembly(string path)
        {
            Console.WriteLine(path);
            Console.WriteLine("before name");
            AssemblyName name = new AssemblyName(path);
            Console.WriteLine("after");
            //name.Version = new Version(1, 0, 0, name.Version.Revision + 1);
            // TODO
            Console.WriteLine("before load");
            var assembly = Assembly.Load(File.ReadAllBytes(path));
            Console.WriteLine(assembly.GetTypes().Length);
            foreach (var method in assembly.GetTypes()
                .SelectMany(type => type.GetMethods(allBindings))
                .Where(IsMarkedReloadable))
            {
                var key = QualifiedName(method);
                Console.WriteLine("Reloader: patching " + key);
                var originalMethod = reloadableMethods[key];
                if (originalMethod != null)
                {
                    var originalCodeStart = GetMethodStart(originalMethod);
                    var newCodeStart = GetMethodStart(method);
                    WriteJump(originalCodeStart, newCodeStart);
                }
                else
                    Console.WriteLine("Reloader: Original method missing (did you add a method with ReloadableMethod attribute?)");
            }
        }

        public static string QualifiedName(MethodBase method)
        {
            return method.DeclaringType.FullName + '.' + method.Name;
        }

        public static bool IsMarkedReloadable(MethodBase method)
        {
            return method.GetAttribute<ReloadableMethod>() != null;
        }

        public static IntPtr GetMethodStart(MethodBase method)
        {
            var handle = GetRuntimeMethodHandle(method);
            RuntimeHelpers.PrepareMethod(handle);
            return handle.GetFunctionPointer();
        }

        public static unsafe void WriteJump(IntPtr original, IntPtr destination)
        {
            if (IntPtr.Size == 8)
            {
                ushort* original_s = (ushort*)original;
                original_s[0] = 0xB848;
                *(ulong*)(original_s + 1) = (ulong)destination;
                original_s[5] = 0xE0FF;
            }
            else
            {
                byte* original_b = (byte*)original;
                original_b[0] = 0x68;
                *(uint*)(original_b + 1) = (uint)destination;
                original_b[5] = 0xC3;

            }
        }

        private static RuntimeMethodHandle GetRuntimeMethodHandle(MethodBase method)
        {
            if (method is DynamicMethod)
            {
                var nonPublicInstance = BindingFlags.NonPublic | BindingFlags.Instance;

                // DynamicMethod actually generates its m_methodHandle on-the-fly and therefore
                // we should call GetMethodDescriptor to force it to be created.

                var m_GetMethodDescriptor = typeof(DynamicMethod).GetMethod("GetMethodDescriptor", nonPublicInstance);
                if (m_GetMethodDescriptor != null)
                    return (RuntimeMethodHandle)m_GetMethodDescriptor.Invoke(method, new object[0]);

                // .Net Core
                var f_m_method = typeof(DynamicMethod).GetField("m_method", nonPublicInstance);
                if (f_m_method != null)
                    return (RuntimeMethodHandle)f_m_method.GetValue(method);

                // Mono
                var f_mhandle = typeof(DynamicMethod).GetField("mhandle", nonPublicInstance);
                return (RuntimeMethodHandle)f_mhandle.GetValue(method);
            }

            return method.MethodHandle;
        }

        public const BindingFlags allBindings =
            BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.Instance
            | BindingFlags.Static
            | BindingFlags.GetField
            | BindingFlags.SetField
            | BindingFlags.GetProperty
            | BindingFlags.SetProperty;
    }

    public static class Helper
    {
        public static T GetAttribute<T>(this MethodBase method)
            where T : Attribute
        {
            return method.GetCustomAttributes(false).Where(a => a is T).Cast<T>().FirstOrDefault();
        }
    }
}
