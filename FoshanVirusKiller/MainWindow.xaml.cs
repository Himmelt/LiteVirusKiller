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
using System.Reflection;
using System.Text;

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
            loadSettings();
            var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
            versionLabel.Content = versionInfo.ProductVersion;
            copyrightLabel.Content = versionInfo.LegalCopyright;
        }

        private void loadSettings()
        {
            Init_Settings();
            string path = @"C:\ProgramData\FoshanVirusKiller\virus_info.txt";
            if (File.Exists(path))
            {
                try
                {
                    string[] lines = File.ReadAllLines(path);
                    foreach (var line in lines)
                    {
                        string[] ss = line.Trim().Split(',');
                        if (ss.Length >= 3)
                        {
                            try
                            {
                                long size = Convert.ToInt64(ss[1]);
                                if (!virusInfos.ContainsKey(ss[0]))
                                {
                                    virusInfos.Add(ss[0], new VirusInfo(size, ss[2], false));
                                }
                            }
                            catch (Exception) { }
                        }
                    }
                }
                catch (Exception) { }
            }
            updateVirusList();
        }
        private void SaveSettings()
        {
            string path = @"C:\ProgramData\FoshanVirusKiller\virus_info.txt";
            try
            {
                Directory.CreateDirectory(@"C:\ProgramData\FoshanVirusKiller\");
                StreamWriter writer = File.CreateText(path);
                foreach (var info in virusInfos)
                {
                    writer.WriteLine(info.Key + ", " + info.Value.size + "," + info.Value.info);
                }
                writer.Flush();
                writer.Close();
            }
            catch (Exception e)
            {
                Dispatcher.BeginInvoke(Println, console, e.Message);
            }
        }
        private void updateVirusList()
        {
            virusList.Items.Clear();
            VHASH.Clear();
            SIZES.Clear();
            foreach (var info in virusInfos)
            {
                VHASH.Add(info.Key);
                SIZES.Add(info.Value.size);
                virusList.Items.Add(new VirusItem(info.Key, info.Value.size, info.Value.info, info.Value.keep));
            }
        }

        private void StartEverything()
        {
            Everything_SetSearchW("size:" + 32768);
            Everything_SetRequestFlags(EVERYTHING_REQUEST_FULL_PATH_AND_FILE_NAME);
            Everything_QueryW(true);
            uint amount = Everything_GetNumResults();
            for (uint i = 0; i < amount; i++)
            {
                StringBuilder builder = new StringBuilder(2048);
                Everything_GetResultFullPathName(i, builder, 2048);
            }
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                Stream file = dialog.OpenFile();
                string hash = BitConverter.ToString(SHA1.ComputeHash(file));
                if (!virusInfos.ContainsKey(hash))
                {
                    virusInfos.Add(hash, new VirusInfo(file.Length, dialog.SafeFileName, false));
                }
                file.Close();
                updateVirusList();
                SaveSettings();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            object item = virusList.SelectedItem;
            if (item != null && item is VirusItem)
            {
                VirusItem virusItem = (VirusItem)item;
                virusInfos.Remove(virusItem.SHA1);
            }
            updateVirusList();
            SaveSettings();
        }
    }
}
