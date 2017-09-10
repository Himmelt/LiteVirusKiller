using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;

namespace Killer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string VHASH = "06293dea80e39c7eb7ee2bdb00d60b58d932fa8a";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OneKeyKill(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(KillTask);
        }

        private void Kill()
        {
            Action<String> updateAction = new Action<string>(Println);
            byte[] test = { 2, 3, 0, 13, 15 };
            this.Dispatcher.BeginInvoke(updateAction, test.ToString());
        }

        private void KillTask()
        {
            HashSet<string> files = new HashSet<string>();
            //遍历电脑中的进程
            Process[] processes = Process.GetProcesses();
            for (int i = 0; i < processes.GetLength(0); i++)
            {
                //我是要找到我需要的YZT.exe的进程,可以根据ProcessName属性判断
                Action<String> updateAction = new Action<string>(Println);
                Dispatcher.BeginInvoke(updateAction, processes[i].ProcessName);
                if (processes[i].ProcessName.Equals("rundll32"))
                {
                    //立即停止关联的进程,建议不要用Close()方法
                    FileStream file = File.Open(processes[i].MainModule.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    SHA1CryptoServiceProvider SHA1 = new SHA1CryptoServiceProvider();
                    string hash = Byte2HexString(SHA1.ComputeHash(file));
                    Dispatcher.BeginInvoke(updateAction, "["+hash+"]");
                    Dispatcher.BeginInvoke(updateAction, "[" + VHASH + "]");
                    if (VHASH.Equals(hash))
                    {
                        files.Add(processes[i].MainModule.FileName);
                        Dispatcher.BeginInvoke(updateAction, processes[i].MainModule.FileName);
                        processes[i].Kill();
                    }
                }
            }
        }

        private void UpdateButton(bool enable)
        {
            btn_kill.IsEnabled = enable;
        }

        private void ClearLog()
        {
            text_log.Text = "";
        }

        private void Println(string text)
        {
            text_log.Text = text_log.Text + "\n" + text;
        }

        private void Print(string text)
        {
            text_log.Text = text_log.Text + text;
        }

        public static string Byte2HexString(byte[] bytes)
        {
            string hex = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                hex = hex + bytes[i].ToString("x2");
            }
            return hex;
        }
    }
}
