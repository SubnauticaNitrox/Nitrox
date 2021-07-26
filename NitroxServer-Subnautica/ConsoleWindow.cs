using System;
using System.Runtime.InteropServices;

namespace NitroxServer_Subnautica
{
    /// <summary>
    ///     Helper methods to manage the console window used by this process.
    /// </summary>
    public class ConsoleWindow
    {
        /// <summary>
        ///     This flag enables the user to use the mouse to select and edit text. To enable
        ///     this option, you must also set the ExtendedFlags flag.
        /// </summary>
        private const int QUICK_EDIT_MODE = 64;

        // ExtendedFlags must be combined with
        // InsertMode and QuickEditMode when setting
        /// <summary>
        ///     ExtendedFlags must be enabled in order to enable InsertMode or QuickEditMode.
        /// </summary>
        private const int EXTENDED_FLAGS = 128;

        public const int STD_INPUT_HANDLE = -10;

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetConsoleMode(IntPtr hConsoleHandle, out int lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleMode(IntPtr hConsoleHandle, int ioMode);

        public static void QuickEdit(bool enable)
        {
            IntPtr conHandle = GetStdHandle(STD_INPUT_HANDLE);
            int mode;
            if (!GetConsoleMode(conHandle, out mode))
            {
                return;
            }

            if (enable)
            {
                mode = mode | QUICK_EDIT_MODE | EXTENDED_FLAGS;
            }
            else
            {
                mode = mode & ~(QUICK_EDIT_MODE | EXTENDED_FLAGS);
            }
            SetConsoleMode(conHandle, mode);
        }
    }
}
