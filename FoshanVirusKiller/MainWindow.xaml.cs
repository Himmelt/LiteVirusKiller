using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace FoshanVirusKiller
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private static long count = 0;
        private static bool entire = false;

        private static string KEYFOLDER = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        private static List<long> SIZES = new List<long> {
            80235,//winmgr.exe
            //65879,//autorun.inf
            //2898,//DeviceConfigManager.vbs
            237568,  // DeviceConfigManager
            //469,     // DeviceConfigManager.vbs
            //100,     // autorun.inf
            //347,     // disk.lnk
            480768,  // svhost.exe
            376832,  // fold.exe
            680511, //rundll32.exe
            32768
        };
        private static List<string> VHASH = new List<string>{
            "E6-DB-74-2A-81-9E-B5-CC-87-4A-80-6E-AF-A0-EF-06-DB-A7-02-12",//winmgr.exe
            //"08-F8-65-30-3F-00-AC-37-41-D4-5A-77-C7-7D-D2-7B-B3-BE-B1-11",//autorun.inf
            //"6D-9D-DB-EA-CE-40-6F-E3-C5-34-59-9C-FC-85-1C-75-07-A6-FA-02",//DeviceConfigManager.vbs
            //"F0-EA-23-8D-9C-6C-C8-67-F1-6C-26-48-8F-D2-2E-FC-40-00-71-D0",// DeviceConfigManager.exe
            //"C7-AD-86-7B-3B-79-E0-20-0C-72-5D-E9-83-DA-79-89-CE-BC-50-E5",// DeviceConfigManager.vbs
            //"82-20-57-F5-57-CD-E0-DB-19-BC-22-06-71-54-1B-91-65-83-A1-E8",// autorun.inf
            //"89-B4-3C-C9-B0-D9-93-3C-4E-F7-50-BC-6D-E7-F7-28-8A-8F-7F-45",// disk.lnk
            "ED-02-7B-13-7F-D7-31-04-B3-AD-DD-EF-9E-46-13-0F-03-FB-B6-36",// svhost.exe
            "1F-6F-E8-49-39-DA-40-51-CE-47-B8-11-30-82-36-65-29-8A-2B-60",// fold.exe
            "06-29-3D-EA-80-E3-9C-7E-B7-EE-2B-DB-00-D6-0B-58-D9-32-FA-8A" // rundll32.exe
        };

        private static List<Task> TASKS = new List<Task>();

        private static SHA1CryptoServiceProvider SHA1 = new SHA1CryptoServiceProvider();
        private static Action<TextBox> ClearText = new Action<TextBox>((textBox) => textBox.Clear());
        private static Action<TextBox, string> Print = new Action<TextBox, string>((textBox, text) =>
        {
            textBox.AppendText(text);
            textBox.ScrollToEnd();
        });
        private static Action<TextBox, string> Println = new Action<TextBox, string>((textBox, text) =>
        {
            textBox.AppendText(text + "\n");
            textBox.ScrollToEnd();
        });
        private static Action<TextBlock, string> ShowStatus = new Action<TextBlock, string>((status, text) =>
        {
            status.Text = "正在检查：" + text;
        });
        private static Action<TextBlock> ClearStatus = new Action<TextBlock>((status) =>
        {
            status.Text = "";
        });
        private static Action<Control, bool> EnableControl = new Action<Control, bool>((control, enable) => control.IsEnabled = enable);

        private void DeleteFile(FileInfo target, bool top)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                target.Attributes = FileAttributes.Normal;
                try
                {
                    target.Delete();
                }
                catch (Exception)
                {
                    DeleteFile(target, false);
                }
                if (top) Dispatcher.BeginInvoke(Println, console, "已删除病毒文件：" + target.FullName);
            });
            TASKS.Add(task);
        }

        public MainWindow()
        {
            InitializeComponent();
            ShowExtension();
        }

        private void OneKeyKill(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(KillTask);
        }

        private void KillTask()
        {
            // 初始化启动
            count = 0;
            TASKS.Clear();
            Dispatcher.BeginInvoke(EnableControl, killer, false);
            Dispatcher.BeginInvoke(EnableControl, checkQuick, false);
            Dispatcher.BeginInvoke(EnableControl, checkEntire, false);
            Dispatcher.BeginInvoke(ClearText, console);

            // 检查系统进程
            Dispatcher.BeginInvoke(Println, console, "正在检查系统进程...");
            KillProcess();

            // 检查可移动磁盘
            KillDisks();

            Task.WaitAll(TASKS.ToArray());

            // 查杀完毕
            Dispatcher.BeginInvoke(ClearStatus, status);
            Dispatcher.BeginInvoke(Println, console, "查杀完毕！");
            Dispatcher.BeginInvoke(EnableControl, killer, true);
            Dispatcher.BeginInvoke(EnableControl, checkQuick, true);
            Dispatcher.BeginInvoke(EnableControl, checkEntire, true);
        }

        private void KillProcess()
        {
            //遍历电脑中的进程
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    FileInfo info = new FileInfo(process.MainModule.FileName);
                    Dispatcher.BeginInvoke(ShowStatus, status, info.FullName);
                    if (info.Exists && SIZES.Contains(info.Length))
                    {
                        FileStream file = info.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        string hash = BitConverter.ToString(SHA1.ComputeHash(file));
                        file.Close();
                        if (VHASH.Contains(hash))
                        {
                            try
                            {
                                process.Kill();
                                Dispatcher.BeginInvoke(Println, console, "结束病毒进程：" + info.Name);
                                DeleteFile(info, true);
                            }
                            catch (Exception e)
                            {
                                Dispatcher.BeginInvoke(Println, console, "病毒进程处理失败：" + info.FullName);
                                Dispatcher.BeginInvoke(Println, console, e.Message);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    //Console.WriteLine(e.Message);
                    //Dispatcher.BeginInvoke(Println, console, e.Message);
                }
            }
        }

        private void KillDisks()
        {
            if (entire) Dispatcher.BeginInvoke(Println, console, "正在全盘检查，文件数量巨大，请耐心等待！");
            foreach (DriveInfo info in DriveInfo.GetDrives())
            {
                if (entire || info.DriveType == DriveType.Removable)
                {
                    Dispatcher.BeginInvoke(Println, console, "正在检查磁盘：" + info.VolumeLabel);
                    CheckDirectory(info.RootDirectory, info.DriveType == DriveType.Removable);
                }
            }
            if (!entire)
            {
                Dispatcher.BeginInvoke(Println, console, "正在检查用户文件夹，文件数量较大，请耐心等待！");
                CheckDirectory(new DirectoryInfo(@"C:\Users\"), false);
            }
        }

        private void CheckDirectory(DirectoryInfo directory, bool usb)
        {
            foreach (var dir in directory.GetDirectories())
            {
                try
                {
                    if (usb) dir.Attributes = FileAttributes.Normal;
                    CheckDirectory(dir, usb);
                }
                catch (Exception)
                {
                    //Console.WriteLine(e.Message);
                    //Dispatcher.BeginInvoke(Println, console, e.Message);
                }
            }
            foreach (var file in directory.GetFiles())
            {
                if (count++ > 100)
                {
                    Dispatcher.BeginInvoke(ShowStatus, status, file.FullName);
                    count = 0;
                }
                try
                {
                    if (usb) file.Attributes = FileAttributes.Normal;
                    if (usb && file.Extension.Contains("lnk"))
                        DeleteFile(file, true);
                    else
                        TryKillVirus(file);
                }
                catch (Exception)
                {
                    //Console.WriteLine(e.Message);
                    //Dispatcher.BeginInvoke(Println, console, e.Message);
                }
            }
        }

        private void TryKillVirus(FileInfo info)
        {
            if (info.Exists && SIZES.Contains(info.Length))
            {
                FileStream file = info.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                string hash = BitConverter.ToString(SHA1.ComputeHash(file));
                file.Close();
                if (VHASH.Contains(hash)) DeleteFile(info, true);
            }
        }

        private void ShowHidden()
        {
            Registry.CurrentUser.OpenSubKey(KEYFOLDER, true).SetValue("Hidden", 1, RegistryValueKind.DWord);
            Registry.CurrentUser.Close();
        }

        private void HideHidden()
        {
            Registry.CurrentUser.OpenSubKey(KEYFOLDER, true).SetValue("Hidden", 2, RegistryValueKind.DWord);
            Registry.CurrentUser.Close();
        }

        private void ShowSystem()
        {
            Registry.CurrentUser.OpenSubKey(KEYFOLDER, true).SetValue("ShowSuperHidden", 1, RegistryValueKind.DWord);
            Registry.CurrentUser.Close();
        }

        private void HideSystem()
        {
            Registry.CurrentUser.OpenSubKey(KEYFOLDER, true).SetValue("ShowSuperHidden", 0, RegistryValueKind.DWord);
            Registry.CurrentUser.Close();
        }

        private void ShowExtension()
        {
            Registry.CurrentUser.OpenSubKey(KEYFOLDER, true).SetValue("HideFileExt", 0, RegistryValueKind.DWord);
            Registry.CurrentUser.Close();
        }

        private void HideExtension()
        {
            Registry.CurrentUser.OpenSubKey(KEYFOLDER, true).SetValue("HideFileExt", 1, RegistryValueKind.DWord);
            Registry.CurrentUser.Close();
        }

        private void OpenHyperlink(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            Process.Start(new ProcessStartInfo(link.NavigateUri.AbsoluteUri));
        }

        private void OnQuickChecked(object sender, RoutedEventArgs e)
        {
            entire = false;
            killer.Content = "快 速 查 杀";
        }

        private void OnEntireChecked(object sender, RoutedEventArgs e)
        {
            entire = true;
            killer.Content = "全 盘 查 杀";
        }
    }
}
