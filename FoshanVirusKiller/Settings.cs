using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Collections;

namespace FoshanVirusKiller
{
    public struct VirusInfo
    {
        public long size;
        public string info;
        public bool keep;

        public VirusInfo(long size, string info, bool keep)
        {
            this.size = size;
            this.info = info;
            this.keep = keep;
        }
    }

    public class VirusItem
    {
        public string Key { set; get; }
        public string SHA1 { set; get; }
        public long Size { set; get; }
        public string Info { set; get; }
        public bool Keep { set; get; }

        public VirusItem(string sha1, long size, string info, bool keep)
        {
            this.SHA1 = sha1;
            this.Key = sha1.Replace("-", "");
            this.Size = size;
            this.Info = info;
            this.Keep = keep;
        }
    }

    public partial class MainWindow : Window
    {
        private static Dictionary<string, VirusInfo> virusInfos = new Dictionary<string, VirusInfo>();

        private static HashSet<long> SIZES = new HashSet<long>();
        private static HashSet<string> VHASH = new HashSet<string>();

        private static void Internal_Settings()
        {
            virusInfos.Add("E6-DB-74-2A-81-9E-B5-CC-87-4A-80-6E-AF-A0-EF-06-DB-A7-02-12", new VirusInfo(80235, "winmgr.exe", true));
            virusInfos.Add("F0-EA-23-8D-9C-6C-C8-67-F1-6C-26-48-8F-D2-2E-FC-40-00-71-D0", new VirusInfo(237568, "DeviceConfigManager.exe", true));
            virusInfos.Add("ED-02-7B-13-7F-D7-31-04-B3-AD-DD-EF-9E-46-13-0F-03-FB-B6-36", new VirusInfo(480768, "svhost.exe", true));
            virusInfos.Add("1F-6F-E8-49-39-DA-40-51-CE-47-B8-11-30-82-36-65-29-8A-2B-60", new VirusInfo(376832, "__folder.exe", true));
            virusInfos.Add("06-29-3D-EA-80-E3-9C-7E-B7-EE-2B-DB-00-D6-0B-58-D9-32-FA-8A", new VirusInfo(680511, "rundll32.exe", true));
            virusInfos.Add("87-D6-FE-33-4D-34-BC-59-A7-FD-C3-92-64-A5-FA-2B-3D-E5-FF-18", new VirusInfo(684607, "__folder.exe", true));
            virusInfos.Add("63-40-6D-FB-43-B3-47-88-D5-F3-63-69-92-25-1F-A6-EB-FD-96-B8", new VirusInfo(688703, "__folder.exe", true));
            virusInfos.Add("28-95-F4-B3-F3-A2-89-BF-34-40-A5-68-B2-D4-02-19-F7-C9-77-26", new VirusInfo(253952, "sysmwef.exe", true));
        }
    }
}
