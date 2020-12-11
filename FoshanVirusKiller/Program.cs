using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FoshanVirusKiller
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            //AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
            //Assembly.Load(Properties.Resources.Everything64);
            File.WriteAllBytes(@"C:\ProgramData\FoshanVirusKiller\Everything64.dll", Properties.Resources.Everything64);
            //Assembly.LoadFile(@"C:\ProgramData\FoshanVirusKiller\Everything64.dll");
            App.Main();
        }

        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            AssemblyName assemblyName = new AssemblyName(args.Name);

            string path = assemblyName.Name + ".dll";
            if (assemblyName.CultureInfo.Equals(CultureInfo.InvariantCulture) == false)
            {
                path = String.Format(@"{0}\{1}", assemblyName.CultureInfo, path);
            }

            using (Stream stream = executingAssembly.GetManifestResourceStream(path))
            {
                if (stream == null)
                {
                    return null;
                }
                byte[] assemblyRawBytes = new byte[stream.Length];
                stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
                return Assembly.Load(assemblyRawBytes);
            }
        }
    }
}
