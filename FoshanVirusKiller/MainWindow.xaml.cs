using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
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

        private void Kill() {
            Action<String> updateAction = new Action<string>(Println);
            byte[] test = { 2, 3, 0, 13, 15 };
            this.Dispatcher.BeginInvoke(updateAction, test.ToString());
        }

        private void KillTask() {
            HashSet<string> files = new HashSet<string>();
            Action<String> updateAction = new Action<string>(Println);
            //遍历电脑中的进程
            Process[] processes = Process.GetProcesses();
            for (int i = 0; i < processes.GetLength(0); i++) {
                Process process = processes[i];
                try {
                    string path = process.MainModule.FileName;
                    FileStream file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    string hash = Byte2HexString(SHA1.ComputeHash(file));
                    if (VHASH.Equals(hash)) {
                        files.Add(path);
                        Dispatcher.BeginInvoke(updateAction, path);
                        process.Kill();
                    }
                } finally {
                    string name = process.ProcessName;
                }
            }
        }

        private void UpdateButton(bool enable) {
            btn_kill.IsEnabled = enable;
        }

        private void ClearLog() {
            text_log.Text = "";
        }

        private void Println(string text) {
            text_log.Text = text_log.Text + "\n" + text;
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
