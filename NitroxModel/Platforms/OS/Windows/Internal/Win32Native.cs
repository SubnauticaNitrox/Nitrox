using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace NitroxModel.Platforms.OS.Windows.Internal
{
    internal static class Win32Native
    {
        [Flags]
        public enum AssocF : uint
        {
            None = 0,
            Init_NoRemapCLSID = 0x1,
            Init_ByExeName = 0x2,
            Open_ByExeName = 0x2,
            Init_DefaultToStar = 0x4,
            Init_DefaultToFolder = 0x8,
            NoUserSettings = 0x10,
            NoTruncate = 0x20,
            Verify = 0x40,
            RemapRunDll = 0x80,
            NoFixUps = 0x100,
            IgnoreBaseClass = 0x200,
            Init_IgnoreUnknown = 0x400,
            Init_FixedProgId = 0x800,
            IsProtocol = 0x1000,
            InitForFile = 0x2000
        }

        public enum AssocStr
        {
            Command = 1,
            Executable,
            FriendlyDocName,
            FriendlyAppName,
            NoOpen,
            ShellNewValue,
            DDECommand,
            DDEIfExec,
            DDEApplication,
            DDETopic,
            InfoTip,
            QuickTip,
            TileInfo,
            ContentType,
            DefaultIcon,
            ShellExtension,
            DropTarget,
            DelegateExecute,
            SupportedUriProtocols,

            // The values below ('Max' excluded) have been introduced in W10 1511
            ProgID,
            AppID,
            AppPublisher,
            AppIconReference,
            Max
        }

        /// <summary>
        ///     Gets the associated program for the file extension.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string AssocQueryString(AssocStr str, string extension)
        {
            const int S_OK = 0;
            const int S_FALSE = 1;

            uint length = 0;
            uint ret = AssocQueryString(AssocF.None, str, extension, null, null, ref length);
            if (ret != S_FALSE)
            {
                return null;
            }

            StringBuilder sb = new StringBuilder((int)length);
            ret = AssocQueryString(AssocF.None, str, extension, null, sb, ref length);
            if (ret != S_OK)
            {
                return null;
            }
            return sb.ToString();
        }

        [DllImport("Shlwapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern uint AssocQueryString(AssocF flags, AssocStr str, string extension, string pszExtra, [Out] StringBuilder pszOut, ref uint pcchOut);
        
        [DllImport("kernel32.dll", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        /// <summary>
        ///     If true, kills the debugged process when debugger detaches.
        /// </summary>
        /// <param name="killOnExit"></param>
        [DllImport("kernel32.dll")]
        public static extern void DebugSetProcessKillOnExit(bool killOnExit);

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process(
            [In] SafeHandle hProcess,
            [Out] [MarshalAs(UnmanagedType.Bool)] out bool wow64Process
        );

        public static string QueryFullProcessImageName(SafeHandle process, uint flags = 0)
        {
            try
            {
                StringBuilder fileNameBuilder = new StringBuilder(1024);
                int size = fileNameBuilder.Capacity;
                return QueryFullProcessImageName(process, flags, fileNameBuilder, ref size) ? fileNameBuilder.ToString() : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        [DllImport("kernel32.dll")]
        public static extern bool TerminateProcess(SafeHandle hProcess, int exitCode);

        [DllImport("kernel32.dll")]
        public static extern uint SuspendThread(SafeHandle hThread);

        [DllImport("kernel32.dll")]
        public static extern uint ResumeThread(SafeHandle hThread);

        [DllImport("ntdll.dll", PreserveSig = false)]
        public static extern void NtSuspendProcess(SafeHandle processHandle);

        [DllImport("ntdll.dll", PreserveSig = false, SetLastError = true)]
        public static extern void NtResumeProcess(SafeHandle processHandle);

        [DllImport("kernel32.dll")]
        public static extern bool CreateProcess(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            ProcessCreationFlags dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref StartupInfo lpStartupInfo,
            out ProcessInfo lpProcessInformation
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeProcessHandle OpenProcess(
            ProcessAccessFlags processAccess,
            bool bInheritHandle,
            int processId
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(
            SafeHandle hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(SafeHandle hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesWritten);

        [DllImport("ntdll.dll")]
        public static extern NtStatus NtQueryInformationProcess(SafeHandle processHandle, int processInformationClass, ref ProcessBasicInformation processInformation, int processInformationLength, IntPtr returnLength);

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateRemoteThread(SafeHandle hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress,
                                                       IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

        [DllImport("kernel32.dll")]
        internal static extern bool FlushInstructionCache(SafeHandle hProcess, IntPtr lpBaseAddress, uint dwSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern SafeAccessTokenHandle OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        [DllImport("kernel32.dll")]
        private static extern bool QueryFullProcessImageName([In] SafeHandle hProcess, [In] uint dwFlags, [Out] StringBuilder lpExeName, [In] [Out] ref int lpdwSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool QueryFullProcessImageName([In] SafeHandle hProcess, [In] int dwFlags, [Out] StringBuilder lpExeName, ref int lpdwSize);
    }
}
