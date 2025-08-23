using System;
using System.Runtime.InteropServices;
using System.Text;

namespace NitroxModel.Platforms.OS.Windows.Internal;

internal static class Win32Native
{
    /// <summary>
    /// https://learn.microsoft.com/en-us/windows/win32/shell/assocf_str
    /// </summary>
    [Flags]
    public enum AssocF : uint
    {
        None = 0x0,
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
        InitForFile = 0x2000,
        IsFullUri = 0x4000,
        PerMachineOnly = 0x8000,
        AppToApp = 0x10000
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

        StringBuilder sb = new((int)length);
        ret = AssocQueryString(AssocF.None, str, extension, null, sb, ref length);
        if (ret != S_OK)
        {
            return null;
        }
        return sb.ToString();
    }

    [DllImport("Shlwapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern uint AssocQueryString(AssocF flags, AssocStr str, string extension, string pszExtra, [Out] StringBuilder pszOut, ref uint pcchOut);

    [DllImport("Wintrust.dll", PreserveSig = true, SetLastError = false)]
    private static extern uint WinVerifyTrust(IntPtr hWnd, IntPtr pgActionID, IntPtr pWinTrustData);

    public static bool IsTrusted(string fileName)
    {
        return WinVerifyTrust(fileName) == 0;
    }

    private static uint WinVerifyTrust(string fileName)
    {
        Guid wintrust_action_generic_verify_v2 = new("{00AAC56B-CD44-11d0-8CC2-00C04FC295EE}");
        uint result = 0;
        using (WINTRUST_FILE_INFO fileInfo = new(fileName,
                                                 Guid.Empty))
        using (UnmanagedPointer guidPtr = new(Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Guid))),
                                              AllocMethod.HGlobal))
        using (UnmanagedPointer wvtDataPtr = new(Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WINTRUST_DATA))),
                                                 AllocMethod.HGlobal))
        {
            WINTRUST_DATA data = new(fileInfo);
            IntPtr pGuid = guidPtr;
            IntPtr pData = wvtDataPtr;
            Marshal.StructureToPtr(wintrust_action_generic_verify_v2,
                                   pGuid,
                                   true);
            Marshal.StructureToPtr(data,
                                   pData,
                                   true);
            result = WinVerifyTrust(IntPtr.Zero,
                                    pGuid,
                                    pData);
        }
        return result;
    }

    internal struct WINTRUST_FILE_INFO : IDisposable
    {
        public WINTRUST_FILE_INFO(string fileName, Guid subject)
        {
            cbStruct = (uint)Marshal.SizeOf(typeof(WINTRUST_FILE_INFO));

            pcwszFilePath = fileName;

            if (subject != Guid.Empty)
            {
                pgKnownSubject = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Guid)));

                Marshal.StructureToPtr(subject, pgKnownSubject, true);
            }

            else
            {
                pgKnownSubject = IntPtr.Zero;
            }

            hFile = IntPtr.Zero;
        }

        public uint cbStruct;

        [MarshalAs(UnmanagedType.LPTStr)] public string pcwszFilePath;

        public IntPtr hFile;

        public IntPtr pgKnownSubject;

        public void Dispose() => Dispose(true);

        private void Dispose(bool disposing)
        {
            if (pgKnownSubject != IntPtr.Zero)
            {
                Marshal.DestroyStructure(pgKnownSubject, typeof(Guid));

                Marshal.FreeHGlobal(pgKnownSubject);
            }
        }
    }

    private enum AllocMethod
    {
        HGlobal,
        CoTaskMem
    }

    private enum UnionChoice
    {
        File = 1,
        Catalog,
        Blob,
        Signer,
        Cert
    }

    private enum UiChoice
    {
        All = 1,
        NoUI,
        NoBad,
        NoGood
    }

    private enum RevocationCheckFlags
    {
        None = 0,
        WholeChain
    }

    private enum StateAction
    {
        Ignore = 0,
        Verify,
        Close,
        AutoCache,
        AutoCacheFlush
    }

    private enum TrustProviderFlags
    {
        UseIE4Trust = 1,
        NoIE4Chain = 2,
        NoPolicyUsage = 4,
        RevocationCheckNone = 16,
        RevocationCheckEndCert = 32,
        RevocationCheckChain = 64,
        RecovationCheckChainExcludeRoot = 128,
        Safer = 256,
        HashOnly = 512,
        UseDefaultOSVerCheck = 1024,
        LifetimeSigning = 2048
    }

    private enum UIContext
    {
        Execute = 0,
        Install
    }

    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    internal static extern int SetWindowLong32(HandleRef hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    internal static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, long dwNewLong);

    [Flags]
    public enum WS : long
    {
        WS_BORDER = 0x00800000L,
        WS_CAPTION = 0x00C00000L,
        WS_CHILD = 0x40000000L,
        WS_CHILDWINDOW = 0x40000000L,
        WS_CLIPCHILDREN = 0x02000000L,
        WS_CLIPSIBLINGS = 0x04000000L,
        WS_DISABLED = 0x08000000L,
        WS_DLGFRAME = 0x00400000L,
        WS_GROUP = 0x00020000L,
        WS_HSCROLL = 0x00100000L,
        WS_ICONIC = 0x20000000L,
        WS_MAXIMIZE = 0x01000000L,
        WS_MAXIMIZEBOX = 0x00010000L,
        WS_MINIMIZE = 0x20000000L,
        WS_MINIMIZEBOX = 0x00020000L,
        WS_OVERLAPPED = 0x00000000L,
        WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
        WS_POPUP = 0x80000000L,
        WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
        WS_SIZEBOX = 0x00040000L,
        WS_SYSMENU = 0x00080000L,
        WS_TABSTOP = 0x00010000L,
        WS_THICKFRAME = 0x00040000L,
        WS_TILED = 0x00000000L,
        WS_TILEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
        WS_VISIBLE = 0x10000000L,
        WS_VSCROLL = 0x00200000L
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WINTRUST_DATA : IDisposable
    {
        public WINTRUST_DATA(WINTRUST_FILE_INFO fileInfo)
        {
            cbStruct = (uint)Marshal.SizeOf(typeof(WINTRUST_DATA));

            pInfoStruct = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WINTRUST_FILE_INFO)));

            Marshal.StructureToPtr(fileInfo, pInfoStruct, false);

            dwUnionChoice = UnionChoice.File;

            pPolicyCallbackData = IntPtr.Zero;

            pSIPCallbackData = IntPtr.Zero;

            dwUIChoice = UiChoice.NoUI;

            fdwRevocationChecks = RevocationCheckFlags.None;

            dwStateAction = StateAction.Ignore;

            hWVTStateData = IntPtr.Zero;

            pwszURLReference = IntPtr.Zero;

            dwProvFlags = TrustProviderFlags.Safer;

            dwUIContext = UIContext.Execute;
        }

        public uint cbStruct;

        public IntPtr pPolicyCallbackData;

        public IntPtr pSIPCallbackData;

        public UiChoice dwUIChoice;

        public RevocationCheckFlags fdwRevocationChecks;

        public UnionChoice dwUnionChoice;

        public IntPtr pInfoStruct;

        public StateAction dwStateAction;

        public IntPtr hWVTStateData;

        private readonly IntPtr pwszURLReference;

        public TrustProviderFlags dwProvFlags;

        public UIContext dwUIContext;

        public void Dispose() => Dispose(true);

        private void Dispose(bool disposing)
        {
            if (dwUnionChoice == UnionChoice.File)
            {
                WINTRUST_FILE_INFO info = new();

                Marshal.PtrToStructure(pInfoStruct, info);

                info.Dispose();

                Marshal.DestroyStructure(pInfoStruct, typeof(WINTRUST_FILE_INFO));
            }

            Marshal.FreeHGlobal(pInfoStruct);
        }
    }

    private sealed class UnmanagedPointer : IDisposable
    {
        private readonly AllocMethod m_meth;

        private IntPtr m_ptr;

        public UnmanagedPointer(IntPtr ptr, AllocMethod method)
        {
            m_meth = method;

            m_ptr = ptr;
        }

        public static implicit operator IntPtr(UnmanagedPointer ptr) => ptr.m_ptr;

        ~UnmanagedPointer()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (m_ptr != IntPtr.Zero)
            {
                if (m_meth == AllocMethod.HGlobal)
                {
                    Marshal.FreeHGlobal(m_ptr);
                }

                else if (m_meth == AllocMethod.CoTaskMem)
                {
                    Marshal.FreeCoTaskMem(m_ptr);
                }

                m_ptr = IntPtr.Zero;
            }

            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
        }

        public void Dispose() => Dispose(true);
    }
}
