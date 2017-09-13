using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace FoshanVirusKiller {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {

        private static string VHASH = "06293dea80e39c7eb7ee2bdb00d60b58d932fa8a";
        private static SHA1CryptoServiceProvider SHA1 = new SHA1CryptoServiceProvider();

        public MainWindow() {
            InitializeComponent();
        }

        private void OneKeyKill(object sender, RoutedEventArgs e) {
            Task.Factory.StartNew(KillTask);
        }


        /// <summary>
        /// 查杀任务!
        /// </summary>
        private void KillTask() {
            HashSet<string> files = new HashSet<string>();
            //遍历电脑中的进程
            Process[] processes = Process.GetProcesses();
            for (int i = 0; i < processes.GetLength(0); i++) {
                Process process = processes[i];
                try {
                    string path = process.MainModule.FileName;
                    FileStream file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    string hash = Byte2HexString(SHA1.ComputeHash(file));
                    Println(path);
                    if (VHASH.Equals(hash)) {
                        files.Add(path);
                        Println(path);
                        process.Kill();
                    }
                } catch (Win32Exception e) { } catch (Exception e) {
                    string name = e.ToString();
                } finally {
                    string name = process.ProcessName;
                }
            }
            // C:\Users\Himmelt\AppData\Roaming\Microsoft\Office
            string[] users = Directory.GetDirectories("C:\\Users");
            foreach (var user in users) {
                files.Add(user + "\\AppData\\Roaming\\Microsoft\\Office\\rundll32.exe");
                //File.Delete(user + "\\AppData\\Roaming\\Microsoft\\Office\\rundll32.exe");
            }
            Thread.Sleep(5000);

            // 获取当前目录
            string currentPath = Directory.GetCurrentDirectory();
            Println(currentPath);
            // 获取驱动器
            DriveInfo drive = new DriveInfo(currentPath);

            Println(@"遍历文件夹 和 文件");
            // 遍历文件夹 和 文件
            string[] dirs = Directory.GetDirectories(currentPath);
            foreach (var dir in dirs) {
                Println(dir);
                if (drive.DriveType == DriveType.Removable) {
                    try {
                        Println("U盘,去隐藏!");
                        DirectoryInfo info = new DirectoryInfo(dir);
                        Println("U盘,DirectoryInfo!");
                        info.Attributes = FileAttributes.Normal & FileAttributes.Directory;
                    } catch (Exception e) {
                        Println("无法访问!" + e.GetType().FullName);
                        Println(e.Message);
                        Println(e.StackTrace);
                    }
                }
            }
            string[] file_s = Directory.GetFiles(currentPath);
            files.UnionWith(file_s);

            foreach (var path in files) {
                try {
                    string hash = Byte2HexString(SHA1.ComputeHash(File.OpenRead(path)));
                    if (VHASH.Equals(hash)) {
                        Println(@"发现病毒!:" + path);
                        File.Delete(path);
                    }

                } catch (Exception e) {
                    Println("无法删除!" + e.GetType().FullName);
                    Println(e.Message);
                    Println(e.StackTrace);
                }
            }
        }

        private void UpdateButton(bool enable) {
            btn_kill.IsEnabled = enable;
        }

        private void ClearLog() {
            text_log.Text = "";
        }

        private void Println(string message) {
            Action<string> action = (text) => text_log.Text = text_log.Text + "\n" + text;
            Dispatcher.BeginInvoke(action, message);
        }

        private void Print(string text) {
            text_log.Text = text_log.Text + text;
        }

        public static string Byte2HexString(byte[] bytes) {
            string hex = "";
            for (int i = 0; i < bytes.Length; i++) {
                hex = hex + bytes[i].ToString("x2");
            }
            return hex;
        }
    }
}
