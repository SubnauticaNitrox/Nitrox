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
        private static readonly HashSet<string> assemblyWhitelist = new HashSet<string>() {
            "NitroxModel.dll",
            "NitroxClient.dll",
            "NitroxPatcher.dll",
        };

        // TODO: Reloadable attribute for classes? (To define all methods in there to be reloadable)
        // TODO: See if method-size is an easy thing. If so, code could be copied but is slower.
        // TODO: Figure out assembly unloading.
        // TODO: Add reloader to server as well (only thing to reload so far is NitroxModel though)

        private Dictionary<string, ReloadableAssembly> reloadableAssemblies;

        public Reloader()
        {
            reloadableAssemblies = AppDomain.CurrentDomain.GetAssemblies()
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
                return assemblyWhitelist.Contains(Path.GetFileName(location));
            })
            .Select(assembly =>
            {
                Console.WriteLine("Reloader: reading assembly " + assembly.FullName);
                return new ReloadableAssembly(assembly);
            }).Where(ra => ra.HasReloadableMethods)
            .ToDictionary(ra => ra.AssemblyName, null);

            // TODO: Change this. Probably by another BeforeBuild task.
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
                ReloadableAssembly ra;
                if (reloadableAssemblies.TryGetValue(fn, out ra))
                    ra.Reload(e.FullPath);
            };
            watcher.Created += handler;
            watcher.Changed += handler;
            watcher.EnableRaisingEvents = true;
            Console.WriteLine("Reloader: set up to watch " + managedFolder);
        }

        private class ReloadableAssembly
        {
            public Dictionary<string, MethodInfo> reloadableMethods;
            public Assembly assembly;
            public ReloadableAssembly(Assembly a)
            {
                assembly = a;
                reloadableMethods = GetReloadableMethods(assembly)
                .ToDictionary(method =>
                {
                    var key = QualifiedName(method);
                    Console.WriteLine("Reloader: found reloadable method " + key);
                    return key;
                }, null);
            }

            public void Reload(string newAssemblyLocation)
            {
                var newAssembly = Assembly.Load(File.ReadAllBytes(newAssemblyLocation));
                foreach (var method in GetReloadableMethods(newAssembly))
                {
                    var key = QualifiedName(method);
                    Console.WriteLine("Reloader: patching " + key);
                    var originalMethod = reloadableMethods[key];
                    if (originalMethod != null)
                    {
                        var originalCodeStart = GetMethodStart(originalMethod);
                        var newCodeStart = GetMethodStart(method);
                        if (originalCodeStart == newCodeStart)
                            Console.WriteLine("Reloader: Methods are identical! (Not emitting jump, that causes an infinite loop)");
                        else
                            WriteJump(originalCodeStart, newCodeStart);
                    }
                    else
                        Console.WriteLine("Reloader: Original method missing (did you add a method with ReloadableMethod attribute?)");
                }
            }

            public bool HasReloadableMethods
            {
                get { return reloadableMethods.Count > 0; }
            }

            public string AssemblyName
            {
                get { return Path.GetFileName(assembly.Location); }
            }

            #region Utils
            private static IEnumerable<MethodInfo> GetReloadableMethods(Assembly a)
            {
                return a.GetTypes()
                    .SelectMany(type => type.GetMethods(allBindings))
                    .Where(IsMarkedReloadable);
            }
            private static string QualifiedName(MethodBase method)
            {
                return method.DeclaringType.FullName + '.' + method.Name;
            }

            private static bool IsMarkedReloadable(MethodBase method)
            {
                return method.GetAttribute<ReloadableMethod>() != null;
            }

            private static IntPtr GetMethodStart(MethodBase method)
            {
                var handle = GetRuntimeMethodHandle(method);
                RuntimeHelpers.PrepareMethod(handle);
                return handle.GetFunctionPointer();
            }

            public static unsafe void WriteJump(IntPtr original, IntPtr destination)
            {
                Console.WriteLine("Reloader: Writing jump to {0} at {1}", destination, original);
                bool x64 = IntPtr.Size == sizeof(long);
#if true
                if (x64)
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
#else
            // For some reason, this doesn't work...
            using (UnmanagedMemoryStream ums = new UnmanagedMemoryStream((byte*)original, x64 ? (4 + sizeof(long)) : (2 + sizeof(int))))
            using (BinaryWriter bw = new BinaryWriter(ums))
                if (x64)
                {
                    byte[] bla = new byte[] { 0x48, 0xB8, 0xFF, 0xE0 };
                    // MOVABS RAX, 64:
                    bw.Write(bla, 0, 2);
                    // operand:
                    bw.Write(destination.ToInt64());
                    // JMP RAX:
                    bw.Write(bla, 2, 2);
                }
                else
                {
                    // PUSH imm16/32:
                    bw.Write((byte)0x68);
                    // operand:
                    bw.Write(destination.ToInt32());
                    // RET:
                    bw.Write((byte)0xC3);
                }
#endif
            }
            private static RuntimeMethodHandle GetRuntimeMethodHandle(MethodBase method)
            {
                if (method is DynamicMethod)
                {
                    Console.WriteLine($"Reloader: {method} Is a dynamic method!");
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

            private const BindingFlags allBindings =
                BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.Instance
                | BindingFlags.Static
                | BindingFlags.GetField
                | BindingFlags.SetField
                | BindingFlags.GetProperty
                | BindingFlags.SetProperty;
            #endregion
        }
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
