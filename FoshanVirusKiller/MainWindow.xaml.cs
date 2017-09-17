using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace FoshanVirusKiller {

    public partial class MainWindow : Window {

        public MainWindow() {
            InitializeComponent();
            TextRange textRange = new TextRange(console.Document.ContentStart, console.Document.ContentEnd);
            textRange.Load(new MemoryStream(Encoding.ASCII.GetBytes(Properties.Resources.readme)), DataFormats.Rtf);
        }

        private void OneKeyKill(object sender, RoutedEventArgs e) {
            Task.Factory.StartNew(KillTask);
        }

        private void KillTask() {

            HashSet<string> files = new HashSet<string>();

            //遍历电脑中的进程
            Process[] processes = Process.GetProcesses();
            foreach (var process in Process.GetProcesses()) {
                try {
                    string path = process.MainModule.FileName;
                    FileStream file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    string hash = Util.Byte2HexString(Util.SHA1.ComputeHash(file));
                    Dispatcher.BeginInvoke(Util.Printline, console, path);
                    if (Util.VHASH.Contains(hash)) {
                        files.Add(path);
                        Dispatcher.BeginInvoke(Util.Printline, console, path);
                        process.Kill();
                    }
                } catch (Exception e) {
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

            // 获取当前目录
            string currentPath = Directory.GetCurrentDirectory();
            // 获取驱动器
            DriveInfo drive = new DriveInfo(currentPath);

            //Println(@"遍历文件夹 和 文件");
            // 遍历文件夹 和 文件
            string[] dirs = Directory.GetDirectories(currentPath);
            foreach (var dir in dirs) {
                //Println(dir);
                if (drive.DriveType == DriveType.Removable) {
                    try {
                        ///Println("U盘,去隐藏!");
                        DirectoryInfo info = new DirectoryInfo(dir);
                        //Println("U盘,DirectoryInfo!");
                        info.Attributes = FileAttributes.Normal & FileAttributes.Directory;
                    } catch (Exception e) {
                        //Println("无法访问!" + e.GetType().FullName);
                        //Println(e.Message);
                        //Println(e.StackTrace);
                    }
                }
            }
            string[] file_s = Directory.GetFiles(currentPath);
            files.UnionWith(file_s);

            foreach (var path in files) {
                try {
                    FileStream file = File.OpenRead(path);
                    string hash = Util.Byte2HexString(Util.SHA1.ComputeHash(file));
                    file.Close();
                    if (Util.VHASH.Contains(hash)) {
                        //Println(@"发现病毒!:" + path);
                        File.SetAttributes(path, FileAttributes.Normal);
                        File.Delete(path);
                    }
                } catch (Exception e) {
                    //Println("无法删除!" + e.GetType().FullName);
                    //Println(e.Message);
                    //Println(e.StackTrace);
                }
            }
        }
    }
}
