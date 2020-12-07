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
using System.Collections.Concurrent;

namespace FoshanVirusKiller
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private static DateTime last = DateTime.Now;
        private static bool entire = false;
        private static string KEYFOLDER = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        private static List<long> SIZES = new List<long> {
            80235,  // winmgr.exe
            237568, // DeviceConfigManager
            480768, // svhost.exe
            376832, // fold.exe
            680511, // rundll32.exe
            32768,  // 感染型木马_system~.ini
            684607, // folder.exe
            688703, // folder.exe
            253952, // sysmwef.exe
        };
        private static List<string> VHASH = new List<string>{
            "E6-DB-74-2A-81-9E-B5-CC-87-4A-80-6E-AF-A0-EF-06-DB-A7-02-12",// winmgr.exe
            "F0-EA-23-8D-9C-6C-C8-67-F1-6C-26-48-8F-D2-2E-FC-40-00-71-D0",// DeviceConfigManager.exe
            "ED-02-7B-13-7F-D7-31-04-B3-AD-DD-EF-9E-46-13-0F-03-FB-B6-36",// svhost.exe
            "1F-6F-E8-49-39-DA-40-51-CE-47-B8-11-30-82-36-65-29-8A-2B-60",// folder.exe
            "06-29-3D-EA-80-E3-9C-7E-B7-EE-2B-DB-00-D6-0B-58-D9-32-FA-8A",// rundll32.exe
            "87-D6-FE-33-4D-34-BC-59-A7-FD-C3-92-64-A5-FA-2B-3D-E5-FF-18",// folder.exe
            "63-40-6D-FB-43-B3-47-88-D5-F3-63-69-92-25-1F-A6-EB-FD-96-B8",// folder.exe
            "28-95-F4-B3-F3-A2-89-BF-34-40-A5-68-B2-D4-02-19-F7-C9-77-26",// sysmwef.exe
        };
        private static ConcurrentBag<Task> TASKS = new ConcurrentBag<Task>();
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
                if (top)
                {
                    Dispatcher.BeginInvoke(Println, console, "已删除病毒文件：" + target.FullName);
                }
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
            last = DateTime.Now;
            TASKS = new ConcurrentBag<Task>();

            Dispatcher.BeginInvoke(EnableControl, killer, false);
            Dispatcher.BeginInvoke(EnableControl, checkQuick, false);
            Dispatcher.BeginInvoke(EnableControl, checkEntire, false);
            Dispatcher.BeginInvoke(ClearText, console);

            // 检查系统进程
            Dispatcher.BeginInvoke(Println, console, "正在检查系统进程 . . .");
            KillProcess();

            // 检查磁盘
            KillDisks();

            Task.WaitAll(TASKS.ToArray());


            // 查杀完毕
            Dispatcher.BeginInvoke(ClearStatus, status);
            Dispatcher.BeginInvoke(Println, console, "查杀完毕！");
            Dispatcher.BeginInvoke(EnableControl, killer, true);
            Dispatcher.BeginInvoke(EnableControl, checkQuick, true);
            Dispatcher.BeginInvoke(EnableControl, checkEntire, true);
            //Dispatcher.BeginInvoke(ClearStatus, status);
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
            List<Task> tasks = new List<Task>();
            foreach (DriveInfo info in DriveInfo.GetDrives())
            {
                if (entire || info.DriveType == DriveType.Removable)
                {
                    Task task = Task.Factory.StartNew(() =>
                    {
                        Dispatcher.BeginInvoke(Println, console, "正在检查磁盘：" + info.VolumeLabel + " ( " + info.ToString() + " ) ");
                        CheckDirectory(info.RootDirectory, info.DriveType == DriveType.Removable);
                        Dispatcher.BeginInvoke(Println, console, "磁盘：" + info.VolumeLabel + " ( " + info.ToString() + " ) 检查完毕！");
                    });
                    tasks.Add(task);
                }
            }
            if (!entire)
            {
                Dispatcher.BeginInvoke(Println, console, "正在检查用户文件夹，文件数量较大，请耐心等待！");
                Task task = Task.Factory.StartNew(() => CheckDirectory(new DirectoryInfo(@"C:\Users\"), false));
                tasks.Add(task);
            }
            Task.WaitAll(tasks.ToArray());
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

                if ((DateTime.Now - last).TotalMilliseconds > 25)
                {
                    Dispatcher.BeginInvoke(ShowStatus, status, file.FullName);
                    last = DateTime.Now;
                }
                try
                {
                    if (usb)
                    {
                        file.Attributes = FileAttributes.Normal;
                        if (file.Extension.Contains("lnk") || file.Name.Contains("autorun.inf") || file.Name.Contains("DeviceConfigManager.vbs"))
                        {
                            DeleteFile(file, true);
                        }
                    }
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
