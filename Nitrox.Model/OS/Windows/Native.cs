using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Nitrox.Model.OS.Windows
{
    internal class Native
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
    }
}
