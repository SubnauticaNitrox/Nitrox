using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using NitroxModel.Platforms.OS.Windows.Internal;

namespace NitroxModel.Platforms.OS.Shared
{
    /// <summary>
    ///     Lower-level wrapper for an OS process than normal <see cref="Process" /> to support memory access, DLLs discovery and exported functions listing.
    ///
    ///     TODO: Turn this class into abstract that is used by OS specific implementations. Right now it's Windows only.
    /// </summary>
    public sealed class ProcessEx : IDisposable
    {
        private readonly Process optionalInnerProcess;
        private readonly Dictionary<int, ThreadHandle> threadHandles = new();
        private SafeHandle handle;

        private int id;

        private bool? is32Bit;
        private bool isDisposed;
        private Module mainModule;

        private string mainModuleFileName;

        private Dictionary<string, Module> modules;

        private IntPtr pebAddress;

        public SafeHandle Handle
        {
            get
            {
                if (handle != null)
                {
                    return handle;
                }

                try
                {
                    handle = optionalInnerProcess?.SafeHandle;
                }
                catch (Exception ex)
                {
                    if (ex is InvalidOperationException || ex is Win32Exception)
                    {
                        handle = new SafeProcessHandle(IntPtr.Zero, true);
                    }
                    else
                    {
                        throw;
                    }
                }
                return handle;
            }
            init
            {
                handle?.Dispose();
                handle = value;
            }
        }

        public SafeHandle MainThreadHandle { get; }
        public Dictionary<string, Module> Modules => modules ?? GetModules();

        public Module MainModule
        {
            get
            {
                if (mainModule != null)
                {
                    return mainModule;
                }

                GetModules();
                return mainModule;
            }
        }

        public int Id
        {
            get => id > 0 ? id : optionalInnerProcess?.Id ?? 0;
            private init => id = value;
        }

        /// <summary>
        ///     True if targeted process is 32 bit. If it fails it will default to the bitness of the OS.
        /// </summary>
        public bool Is32Bit => is32Bit ??= !Win32Native.IsWow64Process(Handle, out bool isWowProcess) ? !Environment.Is64BitOperatingSystem : isWowProcess;

        public IntPtr PebAddress
        {
            get
            {
                if (pebAddress != IntPtr.Zero)
                {
                    return pebAddress;
                }

                ProcessBasicInformation pbi = default;
                NtStatus queryStatus = Win32Native.NtQueryInformationProcess(Handle, 0, ref pbi, Marshal.SizeOf(typeof(ProcessBasicInformation)), IntPtr.Zero);
                return pebAddress = queryStatus == NtStatus.SUCCESS ? pbi.PebBaseAddress : IntPtr.Zero;
            }
        }

        public IntPtr this[IntPtr address] => new(BitConverter.ToInt32(ReadMemory(address, is32Bit == true ? 4 : 8), 0));

        public string Name => optionalInnerProcess?.ProcessName;

        /// <summary>
        ///     Tries to get the path to main executable of the process. Returns null if insufficient access.
        /// </summary>
        public string MainModuleFileName
        {
            get
            {
                if (mainModuleFileName != null)
                {
                    return mainModuleFileName;
                }
                if (Handle.IsInvalid)
                {
                    return null;
                }

                return mainModuleFileName = Win32Native.QueryFullProcessImageName(Handle);
            }
        }

        public string MainModuleDirectory
        {
            get
            {
                string fileName = MainModuleFileName;
                if (fileName == null)
                {
                    return null;
                }
                return Path.GetDirectoryName(MainModuleFileName);
            }
        }

        public ProcessEx(Process proc)
        {
            optionalInnerProcess = proc;
            MainThreadHandle = new SafeProcessHandle(IntPtr.Zero, true);
        }

        private ProcessEx(IntPtr handle, int processId, IntPtr mainThreadHandle = default)
        {
            Handle = new SafeProcessHandle(handle, true);
            MainThreadHandle = new SafeProcessHandle(mainThreadHandle, true);
            Id = processId;
        }

        /// <summary>
        /// Starts a process.
        /// </summary>
        /// <param name="fileName">Path to the executable file. Without any arguments.</param>
        /// <param name="environmentVariables"></param>
        /// <param name="workingDirectory"></param>
        /// <param name="commandLine">Arguments for the executable.</param>
        /// <returns></returns>
        public static ProcessEx Start(string fileName = null, IEnumerable<(string, string)> environmentVariables = null, string workingDirectory = null, string commandLine = null)
        {
            return StartInternal(fileName, false, environmentVariables, workingDirectory, commandLine);
        }

        public static ProcessEx GetFirstProcess(string procName, Func<ProcessEx, bool> predicate, StringComparer comparer = null)
        {
            comparer ??= StringComparer.OrdinalIgnoreCase;
            ProcessEx found = null;
            foreach (Process proc in Process.GetProcesses())
            {
                // Already found, dispose all other resources to processes.
                if (found != null)
                {
                    proc.Dispose();
                    continue;
                }

                if (comparer.Compare(procName, proc.ProcessName) != 0)
                {
                    proc.Dispose();
                    continue;
                }
                ProcessEx procEx = new(proc);
                if (!predicate(procEx))
                {
                    procEx.Dispose();
                    continue;
                }

                found = procEx;
            }

            return found;
        }

        public IntPtr CreateThread(IntPtr start, IntPtr parameter)
        {
            return Win32Native.CreateRemoteThread(Handle, IntPtr.Zero, 0, start, parameter, 0, out _);
        }

        public bool Suspend()
        {
            if (Handle.IsInvalid)
            {
                return false;
            }
            Win32Native.NtSuspendProcess(Handle);
            return true;
        }

        public bool Resume()
        {
            if (Handle.IsInvalid)
            {
                return false;
            }
            Win32Native.NtResumeProcess(Handle);
            return true;
        }

        public void Kill()
        {
            if (Win32Native.TerminateProcess(Handle, 0))
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }
            isDisposed = true;

            foreach (ThreadHandle threadHandle in threadHandles.Values)
            {
                threadHandle.Dispose();
            }
            threadHandles.Clear();
            optionalInnerProcess?.Dispose();
            if (!MainThreadHandle.IsInvalid && !MainThreadHandle.IsClosed)
            {
                MainThreadHandle.Dispose();
            }
            if (!Handle.IsInvalid && !Handle.IsClosed)
            {
                Handle.Dispose();
            }
        }

        /// <summary>
        ///     Gets the base address of a loaded module or an exported function if found.
        ///     Returns <see cref="IntPtr.Zero" /> if module or exported function name was not found.
        /// </summary>
        /// <param name="moduleName">Name of the loaded module to get the base address from.</param>
        /// <param name="exportedFunctionName">Name of the exported function on the module to get the address from.</param>
        /// <returns></returns>
        public IntPtr GetAddress(string moduleName, string exportedFunctionName = null)
        {
            moduleName = moduleName?.ToLowerInvariant() ?? "";
            if (!Modules.TryGetValue(moduleName, out Module module))
            {
                GetModules();
                if (!Modules.TryGetValue(moduleName, out module))
                {
                    return IntPtr.Zero;
                }
            }
            if (exportedFunctionName == null)
            {
                return module.BaseAddress;
            }

            if (!module.ExportedFunctions.TryGetValue(exportedFunctionName, out ExportedFunction func))
            {
                module.GetExportedFunctions();
                if (!module.ExportedFunctions.TryGetValue(exportedFunctionName, out func))
                {
                    return IntPtr.Zero;
                }
            }
            return func.Address;
        }

        public byte[] ReadMemory(IntPtr address, int size)
        {
            byte[] buffer = new byte[size];
            Win32Native.ReadProcessMemory(Handle, address, buffer, size, out _);
            return buffer;
        }

        public byte[] ReadMemory(IntPtr address, int size, params int[] offsets)
        {
            IntPtr ptr = address;
            if (offsets.Length > 1)
            {
                ptr = ReadPointer(address, offsets.TakeWhile((val, i) => i < offsets.Length - 1));
            }
            int lastOffset = offsets.Length > 0 ? offsets[offsets.Length - 1] : 0;
            return ReadMemory(ptr + lastOffset, size);
        }

        public int WriteMemory(IntPtr address, byte[] data, bool flushInstructionCache = false)
        {
            Win32Native.WriteProcessMemory(Handle, address, data, data.Length, out int written);
            if (flushInstructionCache)
            {
                Win32Native.FlushInstructionCache(Handle, address, (uint)written);
            }
            return written;
        }

        /// <summary>
        ///     Reads the pointer, then reads again after applying an offset each time.
        /// </summary>
        /// <param name="basePointerAddress"></param>
        /// <param name="offsets"></param>
        /// <returns></returns>
        public IntPtr ReadPointer(IntPtr basePointerAddress, params int[] offsets)
        {
            IntPtr address = basePointerAddress;
            address = new IntPtr(BitConverter.ToInt64(ReadMemory(address, 8), 0));
            foreach (int t in offsets)
            {
                address = new IntPtr(BitConverter.ToInt64(ReadMemory(address + t, 8), 0));
            }
            return address;
        }

        public IntPtr ReadPointer(IntPtr basePointerAddress, IEnumerable<int> offsets)
        {
            return ReadPointer(basePointerAddress, offsets.ToArray());
        }

        public string ReadString(UIntPtr address, Encoding encoding = null, int maxBytesRead = 1024)
        {
            return ReadString(new IntPtr((long)address.ToUInt64()), encoding, maxBytesRead);
        }

        public string ReadString(IntPtr address, Encoding encoding = null, int maxBytesRead = 1024)
        {
            encoding ??= Encoding.ASCII;
            int bytesRead = 0;
            List<byte> buffer = new(256);

            bool FillBufferNext()
            {
                int bytesToRead = maxBytesRead - bytesRead > 256 ? 256 : maxBytesRead - bytesRead;
                if (bytesToRead < 1)
                {
                    return false;
                }

                buffer.AddRange(ReadMemory(address + bytesRead, bytesToRead));
                bytesRead += bytesToRead;
                return true;
            }

            // Decide end-of-string pattern based on string encoding in memory.
            string endOfStringPattern = encoding switch
            {
                ASCIIEncoding _ => "\0",
                UnicodeEncoding _ => "\0\0", // UTF-16
                UTF32Encoding _ => throw new NotImplementedException(),
                UTF7Encoding _ => throw new NotImplementedException(),
                UTF8Encoding _ => throw new NotImplementedException(),
                _ => throw new ArgumentOutOfRangeException(nameof(encoding))
            };

            // Loop over string, stop on end character or when max bytes is reached.
            int unbufferedIndex = 0;
            bool reachedEnd = false;
            int encodingCodePointSize = endOfStringPattern.Length; // TODO: Get from encoding instead (2 for UTF-16, 1 for ascii)
            while (!reachedEnd && FillBufferNext())
            {
                for (int i = unbufferedIndex; i < buffer.Count - buffer.Count % encodingCodePointSize; i += encodingCodePointSize)
                {
                    for (int j = 0; j < endOfStringPattern.Length; j++)
                    {
                        if (i + j >= buffer.Count || buffer[i + j] != endOfStringPattern[j])
                        {
                            goto nextChar;
                        }
                    }
                    reachedEnd = true;
                    break;

                    nextChar:
                    unbufferedIndex += encodingCodePointSize;
                }
            }

            return encoding.GetString(buffer.ToArray(), 0, unbufferedIndex);
        }

        private static ProcessEx StartInternal(string fileName, bool withDebugger, IEnumerable<(string, string)> environmentVariables = null, string workingDirectory = null, string commandLine = null)
        {
            RuntimeHelpers.PrepareConstrainedRegions();
            ProcessCreationFlags creationFlags = ProcessCreationFlags.CREATE_UNICODE_ENVIRONMENT;
            if (withDebugger)
            {
                creationFlags |= ProcessCreationFlags.DEBUG_ONLY_THIS_PROCESS;
            }

            StartupInfo startupInfo = new() { cb = Marshal.SizeOf<StartupInfo>() };

            // Inherit environment variables from main process
            IDictionary newProcessEnvironment = Environment.GetEnvironmentVariables();
            if (environmentVariables != null)
            {
                foreach ((string, string) pair in environmentVariables)
                {
                    newProcessEnvironment[pair.Item1] = pair.Item2;
                }
            }
            StringBuilder sb = new();
            foreach (DictionaryEntry entry in newProcessEnvironment)
            {
                sb.Append(entry.Key);
                sb.Append('=');
                sb.Append(entry.Value);
                sb.Append(char.MinValue);
            }
            GCHandle environmentHandle = GCHandle.Alloc(Encoding.Unicode.GetBytes(sb.ToString()), GCHandleType.Pinned);
            IntPtr environment = environmentHandle.AddrOfPinnedObject();

            // Create the process.
            bool created;
            ProcessInfo procInfo;
            try
            {
                created = Win32Native.CreateProcess(fileName, // Use this if pointing to a file
                                                    commandLine, // Use this to run it like a shell command
                                                    IntPtr.Zero,
                                                    IntPtr.Zero,
                                                    true,
                                                    creationFlags,
                                                    environment,
                                                    workingDirectory,
                                                    ref startupInfo,
                                                    out procInfo
                );
            }
            finally
            {
                if (environmentHandle.IsAllocated)
                {
                    environmentHandle.Free();
                }
            }
            if (!created)
            {
                return null;
            }

            return new ProcessEx(procInfo.hProcess, procInfo.ProcessId, procInfo.hThread);
        }

        private SafeAccessTokenHandle OpenThread(int threadId, ThreadAccess requiredAccess)
        {
            if (!threadHandles.TryGetValue(threadId, out ThreadHandle thread) || !thread.Access.HasFlag(requiredAccess))
            {
                if (thread != null)
                {
                    thread.Dispose();
                    requiredAccess |= thread.Access; // Combine access from old handle to (potentially) massively reduce handle creation.
                }
                threadHandles[threadId] = thread = new ThreadHandle(threadId, Win32Native.OpenThread(requiredAccess, false, (uint)threadId), requiredAccess);
            }

            return thread.Handle;
        }

        /// <summary>
        ///     Refreshes the cached <see cref="Modules" /> with the loaded modules in the target process.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, Module> GetModules()
        {
            (modules ??= new Dictionary<string, Module>()).Clear();

            // 0x18 = LDR_DATA
            // 0x20 = IN_MEMORY_ORDER_MODULES_LINKED_LIST
            List<int> offsets = new() { 0x20 };
            Module mod = Module.FromPebRecordPointer64(this, ReadPointer(PebAddress + 0x18, offsets));
            modules[Path.GetFileName(mod.FileName).ToLowerInvariant()] = mod;
            mainModule = mod;

            // Follow the linked list of modules inside the Process Environment Block
            while (true)
            {
                offsets.Add(0); // Next record in linked list (first field, so offset == 0)
                mod = Module.FromPebRecordPointer64(this, ReadPointer(PebAddress + 0x18, offsets));
                if (mod.BaseAddress == IntPtr.Zero)
                {
                    break;
                }
                string key = Path.GetFileName(mod.FileName).ToLowerInvariant();
                if (Modules.ContainsKey(key))
                {
                    break;
                }
                if (mod.BaseAddress == IntPtr.Zero)
                {
                    continue;
                }

                modules[key] = mod;
            }

            return modules;
        }

        private T ReadStruct<T>(IntPtr address, params int[] offsets) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(ReadMemory(address, size, offsets), 0, ptr, size);
                return Marshal.PtrToStructure<T>(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        /// <summary>
        ///     Represents a loaded .exe or .dll in a system process.
        /// </summary>
        public sealed class Module
        {
            private readonly ProcessEx process;

            private ImageExportDirectory? exportDirectory;

            private Dictionary<string, ExportedFunction> exportedFunctions;
            public string FileName { get; }
            public IntPtr BaseAddress { get; }
            public IntPtr EntryPoint { get; }

            private ImageExportDirectory ExportDirectory
            {
                get
                {
                    if (exportDirectory != null)
                    {
                        return exportDirectory.Value;
                    }

                    ImageDosHeader exeHeader = process.ReadStruct<ImageDosHeader>(BaseAddress);
                    ImageNtHeader64 header = process.ReadStruct<ImageNtHeader64>(BaseAddress, exeHeader.e_lfanew);
                    IntPtr exportTablePtr = new(BaseAddress.ToInt64() + header.OptionalHeader.ExportTable.VirtualAddress);
                    return (exportDirectory = process.ReadStruct<ImageExportDirectory>(exportTablePtr)).Value;
                }
            }

            /// <summary>
            ///     <a href="https://revers.engineering/custom-getprocaddress-and-getmodulehandle-implementation-x64/">source</a>
            /// </summary>
            public Dictionary<string, ExportedFunction> ExportedFunctions
            {
                get
                {
                    if (exportedFunctions != null)
                    {
                        return exportedFunctions;
                    }

                    return GetExportedFunctions();
                }
            }

            private Module(ProcessEx process, string fileName, IntPtr baseAddress, IntPtr entryPoint)
            {
                this.process = process;
                FileName = fileName;
                BaseAddress = baseAddress;
                EntryPoint = entryPoint;
            }

            public static Module FromPebRecordPointer64(ProcessEx process, IntPtr pebRecord)
            {
                IntPtr baseAdress = process.ReadPointer(pebRecord + 0x20);
                IntPtr entryPoint = process.ReadPointer(pebRecord + 0x28);
                IntPtr fileNamePtr = process.ReadPointer(pebRecord + 0x40);
                string fileName = process.ReadString(fileNamePtr, Encoding.Unicode);

                return new Module(process, fileName, baseAdress, entryPoint);
            }

            public Dictionary<string, ExportedFunction> GetExportedFunctions()
            {
                Dictionary<string, ExportedFunction> result = new();
                IntPtr functionNamesPtr = IntPtr.Add(BaseAddress, (int)ExportDirectory.AddressOfNames);
                IntPtr functionAddressesPtr = IntPtr.Add(BaseAddress, (int)ExportDirectory.AddressOfFunctions);
                IntPtr functionOrdinalPtr = IntPtr.Add(BaseAddress, (int)ExportDirectory.AddressOfNameOrdinals);
                for (int i = 0; i < ExportDirectory.NumberOfNames; i++)
                {
                    uint funcNameRva = process.ReadStruct<uint>(functionNamesPtr + i * 4);
                    string name = process.ReadString(IntPtr.Add(BaseAddress, (int)funcNameRva), Encoding.ASCII);
                    ushort ordinal = process.ReadStruct<ushort>(functionOrdinalPtr + i * 2);
                    IntPtr functionAddress = new(process.ReadStruct<uint>(functionAddressesPtr, ordinal * 4));
                    result[name] = new ExportedFunction(this, (ushort)(ExportDirectory.Base + ordinal), name, functionAddress);
                }
                exportedFunctions = result;
                return result;
            }

            public override string ToString()
            {
                return $"{nameof(FileName)}: {FileName}, {nameof(BaseAddress)}: 0x{BaseAddress.ToString("X")}, {nameof(EntryPoint)}: 0x{EntryPoint.ToString("X")}";
            }
        }

        public sealed class ExportedFunction
        {
            private readonly Module module;
            public string Name { get; private set; }

            /// <summary>
            ///     Address of the function, relative to base address of the image.
            ///     Use <see cref="Address" /> to get the computed (base + offset) address to function.
            /// </summary>
            public IntPtr Offset { get; private set; }

            public ushort Ordinal { get; private set; }

            public IntPtr Address => IntPtr.Add(module.BaseAddress, Offset.ToInt32());

            internal ExportedFunction(Module module, ushort ordinal, string name, IntPtr offset)
            {
                this.module = module;
                Ordinal = ordinal;
                Name = name;
                Offset = offset;
            }

            public override string ToString()
            {
                return $"{nameof(Ordinal)}: {Ordinal}, {nameof(Name)}: {Name}, {nameof(Offset)}: 0x{Offset.ToInt64():X}, {nameof(Address)}: 0x{Address.ToInt64():X}";
            }
        }

        private sealed class ThreadHandle : IDisposable
        {
            public int Id { get; }
            public SafeAccessTokenHandle Handle { get; }
            public ThreadAccess Access { get; }

            public ThreadHandle(int id, SafeAccessTokenHandle handle, ThreadAccess access)
            {
                Id = id;
                Handle = handle;
                Access = access;
            }

            public void Dispose()
            {
                Handle?.Dispose();
            }
        }
    }
}
