using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;

namespace FoshanVirusKiller {
    class Util {

        public static List<string> VHASH = new List<string>{
            "06293dea80e39c7eb7ee2bdb00d60b58d932fa8a"
        };
        public static SHA1CryptoServiceProvider SHA1 = new SHA1CryptoServiceProvider();


        public static Action<RichTextBox, string> Printline = new Action<RichTextBox, string>((RichTextBox textBox, string message) => {
            textBox.Document.Blocks.Add(new Paragraph(new Run(message)));
        });

        public static void EnableButton(Button button, bool enable) {
            button.IsEnabled = enable;
        }

        public static void ClearText(RichTextBox textBox) {
            textBox.Document = new FlowDocument();
        }

        public static void Println(RichTextBox textBox, string message) {
            textBox.Document.Blocks.Add(new Paragraph(new Run(message)));
        }

        public static void Print(RichTextBox textBox, string message) {
            textBox.Document.Blocks.Add(new Paragraph(new Run(message)));
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
