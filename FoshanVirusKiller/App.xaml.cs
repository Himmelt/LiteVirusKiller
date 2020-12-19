using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace FoshanVirusKiller
{

    public partial class App : Application
    {
        Mutex mutex;
        const string ProgramDataPath = @"C:\ProgramData\FoshanVirusKiller\";

        [STAThread]
        protected override void OnStartup(StartupEventArgs e)
        {
            mutex = new Mutex(true, "FoshanVirusKiller", out bool ret);
            if (!ret)
            {
                IntPtr hWndPtr = FindWindow(null, "FoshanVirusKiller");
                ShowWindow(hWndPtr, SW_RESTORE);
                SetForegroundWindow(hWndPtr);
                Environment.Exit(0);
            }
            Directory.CreateDirectory(ProgramDataPath);
            base.OnStartup(e);
        }

        public const int SW_RESTORE = 9;

        [DllImport("USER32.DLL", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("USER32.DLL")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("USER32.DLL")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
