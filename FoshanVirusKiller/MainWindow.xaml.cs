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

        private static string KEYFOLDER = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        private static List<long> SIZES = new List<int> {
            32768,665
        };
        private static List<string> VHASH = new List<string>{
            "06293dea80e39c7eb7ee2bdb00d60b58d932fa8a"
        };

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
        private static Action<Button, bool> EnableButton = new Action<Button, bool>((button, enable) => button.IsEnabled = enable);

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
            Dispatcher.BeginInvoke(EnableButton, killer, false);
            Dispatcher.BeginInvoke(ClearText, console);
            HashSet<string> files = new HashSet<string>();

            Dispatcher.BeginInvoke(Println, console, "正在检查系统进程...");
            KillProcess();
            
            // C:\Users\Himmelt\AppData\Roaming\Microsoft\Office
            //foreach (var user in Directory.GetDirectories("C:\\Users"))
            //{
            //    files.Add(user + "\\AppData\\Roaming\\Microsoft\\Office\\rundll32.exe");
            //}
            // 获取当前目录
            string currentPath = Directory.GetCurrentDirectory();
            // 获取驱动器
            // 遍历文件夹 和 文件
            if (new DriveInfo(currentPath).DriveType == DriveType.Removable)
            {
                Dispatcher.BeginInvoke(Println, console, "当前运行路径是U盘，将尝试恢复所有隐藏文件...");
                foreach (var dir in Directory.GetDirectories(currentPath))
                {
                    try
                    {
                        Dispatcher.BeginInvoke(Println, console, "正在恢复文件夹：" + dir);
                        new DirectoryInfo(dir).Attributes = FileAttributes.Normal;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                foreach (var file in Directory.GetFiles(currentPath))
                {
                    try
                    {
                        File.SetAttributes(file, FileAttributes.Normal);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
            files.UnionWith(Directory.GetFiles(currentPath));
            foreach (var path in files)
            {
                try
                {
                    FileStream file = File.OpenRead(path);
                    string hash = BitConverter.ToString(SHA1.ComputeHash(file));
                    file.Close();
                    Dispatcher.BeginInvoke(Println, console, "正在检查：" + path);
                    if (VHASH.Contains(hash))
                    {
                        Dispatcher.BeginInvoke(Println, console, "发现病毒：" + path);
                        File.SetAttributes(path, FileAttributes.Normal);
                        File.Delete(path);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    //Dispatcher.BeginInvoke(Println, console, "无法访问目标文件：" + e.Message);
                }
            }
            Dispatcher.BeginInvoke(Println, console, "查杀完毕！");
            Dispatcher.BeginInvoke(EnableButton, killer, true);
        }

        private void KillProcess()
        {
            //遍历电脑中的进程
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    FileInfo info = new FileInfo(process.MainModule.FileName);
                    if (info.Exists && SIZES.Contains(info.Length))
                    {
                        //Dispatcher.BeginInvoke(Println, console, "进程：" + path);
                        FileStream file = info.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        string hash = BitConverter.ToString(SHA1.ComputeHash(file));
                        file.Close();
                        if (VHASH.Contains(hash))
                        {
                            Dispatcher.BeginInvoke(Println, console, "发现病毒进程：" + info.Name);
                            process.Kill();
                            info.Attributes = FileAttributes.Normal;
                            info.Delete();
                            Dispatcher.BeginInvoke(Println, console, "已删除病毒文件：" + info.FullName);
                            //File.SetAttributes(path, FileAttributes.Normal);
                            //File.Delete(path);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
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

    }
}
