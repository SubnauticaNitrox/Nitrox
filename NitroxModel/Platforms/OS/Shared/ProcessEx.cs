using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;

public static class ProcessUtility
{
    public static bool IsElevated()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        }
        else
        {
            return geteuid() == 0;
        }
    }

    [DllImport("libc")]
    private static extern uint geteuid();
}




public class ProcessEx : IDisposable
{
    private readonly ProcessExBase _implementation;

    public ProcessEx(int pid)
    {
        _implementation = ProcessExFactory.Create(pid);
    }

    public ProcessEx(System.Diagnostics.Process process)
    {
        _implementation = ProcessExFactory.Create(process.Id);
    }

    public int Id => _implementation.Id;
    public string Name => _implementation.Name;
    public IntPtr Handle => _implementation.Handle;
    public ProcessModuleEx MainModule => _implementation.MainModule;
    public string MainModuleFileName => _implementation.MainModuleFileName;
    public IntPtr MainWindowHandle => _implementation.MainWindowHandle;

    public byte[] ReadMemory(IntPtr address, int size)
    {
        return _implementation.ReadMemory(address, size);
    }

    public int WriteMemory(IntPtr address, byte[] data)
    {
        return _implementation.WriteMemory(address, data);
    }

    public IEnumerable<ProcessModuleEx> GetModules()
    {
        return _implementation.GetModules();
    }

    public void Suspend()
    {
        _implementation.Suspend();
    }

    public void Resume()
    {
        _implementation.Resume();
    }

    public void Terminate()
    {
        _implementation.Terminate();
    }

    public void Dispose()
    {
        _implementation.Dispose();
    }

    public static ProcessEx GetFirstProcess(string procName, Func<ProcessEx, bool> predicate = null, StringComparer comparer = null)
    {
        comparer ??= StringComparer.OrdinalIgnoreCase;
        foreach (string dir in Directory.GetDirectories("/proc"))
        {
            if (int.TryParse(Path.GetFileName(dir), out int pid))
            {
                try
                {
                    ProcessEx processEx = new ProcessEx(pid);
                    if (comparer.Compare(procName, processEx.Name) == 0)
                    {
                        if (predicate == null || predicate(processEx))
                        {
                            return processEx;
                        }
                    }
                }
                catch (ArgumentException)
                {
                    // Process doesn't exist anymore, skip it
                    continue;
                }
                catch (UnauthorizedAccessException)
                {
                    // We don't have permission to access this process, skip it
                    continue;
                }
            }
        }
        return null;
    }

    public static ProcessEx Start(string fileName = null, IEnumerable<(string, string)> environmentVariables = null, string workingDirectory = null, string commandLine = null, bool createWindow = true)
    {
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = fileName,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            CreateNoWindow = !createWindow,
        };

        if (environmentVariables != null)
        {
            foreach (var (key, value) in environmentVariables)
            {
                startInfo.EnvironmentVariables[key] = value;
            }
        }

        if (!string.IsNullOrEmpty(commandLine))
        {
            startInfo.Arguments = commandLine;
        }

        System.Diagnostics.Process process = System.Diagnostics.Process.Start(startInfo);
        return new ProcessEx(process);
    }
}

public abstract class ProcessExBase : IDisposable
{
    public abstract int Id { get; }
    public abstract string Name { get; }
    public abstract IntPtr Handle { get; }
    public abstract ProcessModuleEx MainModule { get; }
    public abstract string MainModuleFileName { get; }
    public abstract byte[] ReadMemory(IntPtr address, int size);
    public abstract int WriteMemory(IntPtr address, byte[] data);
    public abstract IEnumerable<ProcessModuleEx> GetModules();
    public abstract void Suspend();
    public abstract void Resume();
    public abstract void Terminate();
    public abstract void Dispose();
    public abstract IntPtr MainWindowHandle { get; }

}

public class ProcessModuleEx
{
    public IntPtr BaseAddress { get; set; }
    public string ModuleName { get; set; }
    public string FileName { get; set; }
    public int ModuleMemorySize { get; set; }
}

public class WindowsProcessEx : ProcessExBase
{
    private Process _process;
    private IntPtr _handle;
    private bool _disposed = false;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesWritten);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint SuspendThread(IntPtr hThread);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern int ResumeThread(IntPtr hThread);

    public WindowsProcessEx(int id)
    {
        if (!ProcessUtility.IsElevated())
            throw new UnauthorizedAccessException("Elevated privileges required.");

        _process = Process.GetProcessById(id);
        _handle = OpenProcess(0x1F0FFF, false, id);
        if (_handle == IntPtr.Zero)
            throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    public override int Id => _process.Id;
    public override string Name => _process.ProcessName;
    public override IntPtr Handle => _handle;

    public override byte[] ReadMemory(IntPtr address, int size)
    {
        byte[] buffer = new byte[size];
        if (!ReadProcessMemory(_handle, address, buffer, size, out int bytesRead) || bytesRead != size)
            throw new Win32Exception(Marshal.GetLastWin32Error());
        return buffer;
    }

    public override int WriteMemory(IntPtr address, byte[] data)
    {
        if (!WriteProcessMemory(_handle, address, data, data.Length, out int bytesWritten))
            throw new Win32Exception(Marshal.GetLastWin32Error());
        return bytesWritten;
    }

    public override IEnumerable<ProcessModuleEx> GetModules()
    {
        return _process.Modules.Cast<ProcessModule>().Select(m => new ProcessModuleEx
        {
            BaseAddress = m.BaseAddress,
            ModuleName = m.ModuleName,
            FileName = m.FileName,
            ModuleMemorySize = m.ModuleMemorySize
        });
    }

    public override void Suspend()
    {
        foreach (ProcessThread thread in _process.Threads)
        {
            IntPtr threadHandle = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
            if (threadHandle != IntPtr.Zero)
            {
                try
                {
                    if (SuspendThread(threadHandle) == uint.MaxValue)
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                finally
                {
                    CloseHandle(threadHandle);
                }
            }
        }
    }

    public override void Resume()
    {
        foreach (ProcessThread thread in _process.Threads)
        {
            IntPtr threadHandle = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
            if (threadHandle != IntPtr.Zero)
            {
                try
                {
                    if (ResumeThread(threadHandle) == -1)
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                finally
                {
                    CloseHandle(threadHandle);
                }
            }
        }
    }

    public override void Terminate()
    {
        _process.Kill();
    }

    public override void Dispose()
    {
        if (!_disposed)
        {
            if (_handle != IntPtr.Zero)
            {
                CloseHandle(_handle);
                _handle = IntPtr.Zero;
            }
            _process.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
    public override ProcessModuleEx MainModule
    {
        get
        {
            ProcessModule mainModule = _process.MainModule;
            return new ProcessModuleEx
            {
                BaseAddress = mainModule.BaseAddress,
                ModuleName = mainModule.ModuleName,
                FileName = mainModule.FileName,
                ModuleMemorySize = mainModule.ModuleMemorySize
            };
        }
    }

    public override string MainModuleFileName => _process.MainModule.FileName;
    public override IntPtr MainWindowHandle => _process.MainWindowHandle;

}

public class LinuxProcessEx : ProcessExBase
{
    private int _pid;
    private bool _disposed = false;

    [DllImport("libc", SetLastError = true)]
    private static extern int ptrace(PtraceRequest request, int pid, IntPtr addr, IntPtr data);

    [DllImport("libc", SetLastError = true)]
    private static extern int kill(int pid, int sig);

    public LinuxProcessEx(int pid)
    {
        _pid = pid;
        if (!File.Exists($"/proc/{_pid}/status"))
            throw new ArgumentException("Process does not exist.", nameof(pid));
    }

    public override int Id => _pid;
    public override IntPtr Handle => IntPtr.Zero; // Linux doesn't use handles

    public override string Name
    {
        get
        {
            try
            {
                string status = File.ReadAllText($"/proc/{_pid}/status");
                string[] lines = status.Split('\n');
                return lines.FirstOrDefault(l => l.StartsWith("Name:"))?.Substring(5).Trim();
            }
            catch (UnauthorizedAccessException)
            {
                // If we can't read the status file, try to get the name from the command line
                try
                {
                    string cmdline = File.ReadAllText($"/proc/{_pid}/cmdline");
                    return Path.GetFileName(cmdline.Split('\0')[0]);
                }
                catch
                {
                    return null;
                }
            }
        }
    }

    public override byte[] ReadMemory(IntPtr address, int size)
    {
        byte[] buffer = new byte[size];
        try
        {
            using FileStream fs = new FileStream($"/proc/{_pid}/mem", FileMode.Open, FileAccess.Read);
            fs.Seek((long)address, SeekOrigin.Begin);
            if (fs.Read(buffer, 0, size) != size)
                throw new IOException("Failed to read the specified amount of memory.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to read process memory.", ex);
        }
        return buffer;
    }

    public override int WriteMemory(IntPtr address, byte[] data)
    {
        int result = ptrace(PtraceRequest.PTRACE_ATTACH, _pid, IntPtr.Zero, IntPtr.Zero);
        if (result < 0)
            throw new InvalidOperationException("Failed to attach to the process.");

        try
        {
            for (int i = 0; i < data.Length; i += sizeof(long))
            {
                long value = BitConverter.ToInt64(data, i);
                if (ptrace(PtraceRequest.PTRACE_POKEDATA, _pid, address + i, (IntPtr)value) < 0)
                    throw new InvalidOperationException("Failed to write memory.");
            }
        }
        finally
        {
            ptrace(PtraceRequest.PTRACE_DETACH, _pid, IntPtr.Zero, IntPtr.Zero);
        }
        return data.Length;
    }

    public override IEnumerable<ProcessModuleEx> GetModules()
    {
        List<ProcessModuleEx> modules = new List<ProcessModuleEx>();
        string[] lines = File.ReadAllLines($"/proc/{_pid}/maps");
        foreach (string line in lines)
        {
            string[] parts = line.Split(' ');
            if (parts.Length >= 6)
            {
                string[] addresses = parts[0].Split('-');
                modules.Add(new ProcessModuleEx
                {
                    BaseAddress = (IntPtr)long.Parse(addresses[0], System.Globalization.NumberStyles.HexNumber),
                    ModuleName = parts[5],
                    FileName = parts[5],
                    ModuleMemorySize = (int)(long.Parse(addresses[1], System.Globalization.NumberStyles.HexNumber) - long.Parse(addresses[0], System.Globalization.NumberStyles.HexNumber))
                });
            }
        }
        return modules;
    }

    public override void Suspend()
    {
        if (kill(_pid, 19) != 0) // SIGSTOP
            throw new InvalidOperationException("Failed to suspend the process.");
    }

    public override void Resume()
    {
        if (kill(_pid, 18) != 0) // SIGCONT
            throw new InvalidOperationException("Failed to resume the process.");
    }

    public override void Terminate()
    {
        if (kill(_pid, 9) != 0) // SIGKILL
            throw new InvalidOperationException("Failed to terminate the process.");
    }

    public override void Dispose()
    {
        _disposed = true;
        GC.SuppressFinalize(this);
    }
    public override ProcessModuleEx MainModule
    {
        get
        {
            // This is a simplified implementation. You might need to parse /proc/{pid}/maps
            // to get more accurate information about the main module.
            return new ProcessModuleEx
            {
                BaseAddress = IntPtr.Zero,
                ModuleName = Name,
                FileName = MainModuleFileName,
                ModuleMemorySize = 0
            };
        }
    }

    public override string MainModuleFileName
    {
        get
        {
            try
            {
                return ReadSymbolicLink($"/proc/{_pid}/exe");
            }
            catch (UnauthorizedAccessException)
            {
                // If we don't have permission to read the symlink, return null
                return null;
            }
            catch
            {
                return null;
            }
        }
    }

    private string ReadSymbolicLink(string path)
    {
        const int BUFFER_SIZE = 1024;
        byte[] buffer = new byte[BUFFER_SIZE];
        int bytesRead = readlink(path, buffer, BUFFER_SIZE);
        if (bytesRead < 0)
        {
            throw new IOException("Failed to read symbolic link.");
        }
        return System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
    }

    [DllImport("libc")]
    private static extern int readlink(string path, byte[] buf, int bufsiz);

    public override IntPtr MainWindowHandle
    {
        get
        {
            // Linux doesn't have a direct equivalent to Windows' MainWindowHandle.
            // This is a placeholder implementation.
            throw new PlatformNotSupportedException("MainWindowHandle is not supported on Linux.");
        }
    }

}

public class MacOSProcessEx : ProcessExBase
{
    private int _pid;
    private IntPtr _task;
    private bool _disposed = false;

    [DllImport("libSystem.dylib")]
    private static extern int task_for_pid(IntPtr target_tport, int pid, out IntPtr t);

    [DllImport("libSystem.dylib")]
    private static extern IntPtr mach_task_self();

    [DllImport("libSystem.dylib")]
    private static extern int vm_read_overwrite(IntPtr target_task, IntPtr address, IntPtr size, IntPtr data, out IntPtr outsize);

    [DllImport("libSystem.dylib")]
    private static extern int vm_write(IntPtr target_task, IntPtr address, IntPtr data, IntPtr size);

    [DllImport("libSystem.dylib")]
    private static extern int task_suspend(IntPtr task);

    [DllImport("libSystem.dylib")]
    private static extern int task_resume(IntPtr task);

    [DllImport("libSystem.dylib")]
    private static extern int task_terminate(IntPtr task);

    public MacOSProcessEx(int pid)
    {
        if (!ProcessUtility.IsElevated())
            throw new UnauthorizedAccessException("Root privileges required.");

        _pid = pid;
        if (task_for_pid(mach_task_self(), pid, out _task) != 0)
            throw new InvalidOperationException("Failed to get task for pid.");
    }

    public override int Id => _pid;
    public override IntPtr Handle => _task;

    public override string Name
    {
        get
        {
            // This is a simplified implementation. In a real scenario, you'd use sysctl to get the process name.
            throw new NotImplementedException("Getting process name is not implemented for macOS.");
        }
    }

    public override byte[] ReadMemory(IntPtr address, int size)
    {
        byte[] buffer = new byte[size];
        IntPtr outSize;
        GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        try
        {
            if (vm_read_overwrite(_task, address, (IntPtr)size, handle.AddrOfPinnedObject(), out outSize) != 0)
                throw new InvalidOperationException("Failed to read process memory.");
        }
        finally
        {
            handle.Free();
        }
        return buffer;
    }

    public override int WriteMemory(IntPtr address, byte[] data)
    {
        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        try
        {
            if (vm_write(_task, address, handle.AddrOfPinnedObject(), (IntPtr)data.Length) != 0)
                throw new InvalidOperationException("Failed to write process memory.");
        }
        finally
        {
            handle.Free();
        }
        return data.Length;
    }

    public override IEnumerable<ProcessModuleEx> GetModules()
    {
        // This is a simplified implementation. In a real scenario, you'd use dyld APIs to get the loaded modules.
        throw new NotImplementedException("Getting modules is not implemented for macOS.");
    }

    public override void Suspend()
    {
        if (task_suspend(_task) != 0)
            throw new InvalidOperationException("Failed to suspend the process.");
    }

    public override void Resume()
    {
        if (task_resume(_task) != 0)
            throw new InvalidOperationException("Failed to resume the process.");
    }

    public override void Terminate()
    {
        if (task_terminate(_task) != 0)
            throw new InvalidOperationException("Failed to terminate the process.");
    }

    public override void Dispose()
    {
        if (!_disposed)
        {
            // In a real implementation, you'd release the task port here
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
    public override ProcessModuleEx MainModule
    {
        get
        {
            // This is a placeholder implementation. You'll need to use macOS-specific APIs
            // to get accurate information about the main module.
            return new ProcessModuleEx
            {
                BaseAddress = IntPtr.Zero,
                ModuleName = Name,
                FileName = MainModuleFileName,
                ModuleMemorySize = 0
            };
        }
    }

    public override string MainModuleFileName
    {
        get
        {
            // This is a placeholder implementation. You'll need to use macOS-specific APIs
            // to get the main module file name.
            throw new NotImplementedException("Getting main module file name is not implemented for macOS.");
        }
    }

    public override IntPtr MainWindowHandle
    {
        get
        {
            // macOS doesn't have a direct equivalent to Windows' MainWindowHandle.
            // This is a placeholder implementation.
            throw new PlatformNotSupportedException("MainWindowHandle is not supported on macOS.");
        }
    }

}

public static class ProcessExFactory
{
    public static ProcessExBase Create(int pid)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new WindowsProcessEx(pid);
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return new LinuxProcessEx(pid);
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return new MacOSProcessEx(pid);
        else
            throw new PlatformNotSupportedException();
    }
}
