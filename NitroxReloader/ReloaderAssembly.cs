using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace NitroxReloader
{
    internal class ReloaderAssembly
    {
        internal Dictionary<string, MethodInfo> reloadableMethods;
        internal Assembly assembly;
        internal ReloaderAssembly(Assembly a)
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

        internal void Reload(string newAssemblyLocation)
        {
            Console.WriteLine("Reloader: starting reload for " + assembly.FullName);
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

        internal bool HasReloadableMethods
        {
            get { return reloadableMethods.Count > 0; }
        }

        internal string AssemblyName
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
            return method.GetAttribute<ReloadableMethodAttribute>() != null;
        }

        private static IntPtr GetMethodStart(MethodBase method)
        {
            var handle = GetRuntimeMethodHandle(method);
            RuntimeHelpers.PrepareMethod(handle);
            return handle.GetFunctionPointer();
        }

        internal static unsafe void WriteJump(IntPtr original, IntPtr destination)
        {
            Console.WriteLine("Reloader: Writing jump to {0} at {1}", destination, original);
            if (IntPtr.Size == sizeof(long))
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
