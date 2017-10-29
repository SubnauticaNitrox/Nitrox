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
        private static readonly List<ReloaderAssembly> reloadableAssemblies = new List<ReloaderAssembly>();

        private Assembly assembly;
        internal ReloaderAssembly(Assembly a)
        {
            assembly = a;
            reloadableAssemblies.Add(this);
        }

        internal void Reload(string newAssemblyLocation)
        {
            Console.WriteLine("Reloader: Starting reload for " + assembly.FullName);
            Assembly newAssembly = Assembly.Load(File.ReadAllBytes(newAssemblyLocation));

            if (newAssembly.GlobalAssemblyCache)
            {
                Console.WriteLine("*** Reloader WARNING: New assembly was loaded from cache! ***\nYour changes will probably not be loaded in! Make sure the version of the new assembly has changed.");
            }

            IEnumerable<MethodInfo> reloadableMethods = GetReloadableMethods(newAssembly);
            Console.WriteLine("Reloader: New assembly {0} has {1} reloadable methods.", newAssembly.FullName, reloadableMethods.Count());
            foreach (MethodInfo method in reloadableMethods)
            {
                string key = QualifiedName(method);
                Console.WriteLine("Reloader: Patching " + key);

                Type definingType = assembly.GetTypes().FirstOrDefault(t => t.FullName == method.DeclaringType.FullName);
                if (definingType == null)
                {
                    Console.WriteLine("Reloader: Type {0} not found in original assembly", method.DeclaringType.FullName);
                    continue;
                }

                // Get the original type, because that's what was used for the original method that has been overwritten.
                Type[] paramTypes = method.GetParameters().Select(pi => ResolveOriginalType(pi.ParameterType)).ToArray();

                MethodInfo originalMethod = definingType.GetMethod(method.Name, ALL_BINDINGS, null, paramTypes, null);

                if (originalMethod == null)
                {
                    Console.WriteLine("Reloader: Original method not found with parameters {0}", string.Join(", ", paramTypes.Select(typ => typ.ToString()).ToArray()));
                    continue;
                }

                IntPtr originalCodeStart = GetMethodStart(originalMethod);
                IntPtr newCodeStart = GetMethodStart(method);
                if (originalCodeStart == newCodeStart)
                {
                    Console.WriteLine("Reloader: Methods are identical! (Not emitting jump, that causes an infinite loop)");
                }
                else
                {
                    WriteJump(originalCodeStart, newCodeStart);
                }
            }
        }

        internal string AssemblyName
        {
            get { return GetAssemblyName(assembly); }
        }

        #region Utils
        private static string GetAssemblyName(Assembly a)
        {
            string name = Path.GetFileName(a.Location);

            if (string.IsNullOrEmpty(name))
            {
                return a.FullName.Split(',')[0] + ".dll";
            }

            return name;
        }

        private static IEnumerable<MethodInfo> GetReloadableMethods(Assembly a)
        {
            return a.GetTypes()
                .SelectMany(type => type.GetMethods(ALL_BINDINGS))
                .Where(IsMarkedReloadable);
        }
        private static string QualifiedName(MethodBase method)
        {
            return method.DeclaringType.FullName + '.' + method.Name;
        }

        private static bool IsMarkedReloadable(MethodBase method)
        {
            return Attribute.IsDefined(method, typeof(ReloadableMethodAttribute));
        }

        private static Type ResolveOriginalType(Type newType)
        {
            // Reloader objects contain references to the initial assemblies that defined all the methods and types.
            // Checking assemblies through CurrentDomain means all types in reloaded assemblies are included as well.
            string assemblyName = GetAssemblyName(newType.Assembly);
            ReloaderAssembly definingAssembly = reloadableAssemblies
                .FirstOrDefault(ra => ra.AssemblyName == assemblyName);

            if (definingAssembly == null)
            {
                // This type comes from a non-reloadable assembly, so it's always the same.
                return newType;
            }

            Type originalType = definingAssembly.assembly.GetTypes().FirstOrDefault(t => t.FullName == newType.FullName);

            if (originalType == null)
            {
                // It's impossible to patch a method with a type that didn't even exist in the original assemblies!
                throw new ArgumentException($"Reloader: No original type found for {newType}! If you added it in the new (reloaded) assembly, the application needs to be restarted.");
            }

            return originalType;
        }

        private static IntPtr GetMethodStart(MethodBase method)
        {
            RuntimeMethodHandle handle = GetRuntimeMethodHandle(method);
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
                BindingFlags nonPublicInstance = BindingFlags.NonPublic | BindingFlags.Instance;

                // DynamicMethod actually generates its m_methodHandle on-the-fly and therefore
                // we should call GetMethodDescriptor to force it to be created.

                MethodInfo m_GetMethodDescriptor = typeof(DynamicMethod).GetMethod("GetMethodDescriptor", nonPublicInstance);
                if (m_GetMethodDescriptor != null)
                {
                    return (RuntimeMethodHandle)m_GetMethodDescriptor.Invoke(method, new object[0]);
                }

                // .Net Core
                FieldInfo f_m_method = typeof(DynamicMethod).GetField("m_method", nonPublicInstance);
                if (f_m_method != null)
                {
                    return (RuntimeMethodHandle)f_m_method.GetValue(method);
                }

                // Mono
                FieldInfo f_mhandle = typeof(DynamicMethod).GetField("mhandle", nonPublicInstance);
                return (RuntimeMethodHandle)f_mhandle.GetValue(method);
            }

            return method.MethodHandle;
        }

        private const BindingFlags ALL_BINDINGS =
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
