using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MikuMikuModel.DivaScriptConverter
{
    class DivaScriptConverter
    {
        static int currentWorker = 0;
        public static int maxWorker = 6;

        public static void ConvertDsc(string sourceFolder, string destinationFolder, bool replace, string pv_expr = "")
        {
            if (!Directory.Exists("dsc_json"))
            {
                Directory.CreateDirectory("dsc_json\\");
            }

            string[] files = System.IO.Directory.GetFiles(sourceFolder, "*.dsc", SearchOption.AllDirectories);
            foreach (var i in files)
            {
                if ((!File.Exists(destinationFolder + "\\" + Path.GetFileNameWithoutExtension(i) + ".dsc")) || (replace == true))
                {
                    while (currentWorker > maxWorker)
                    {
                        Thread.Sleep(33);
                    }
                    new Thread(() =>
                    {
                        var process = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "DivaScriptConv.exe",
                                Arguments = "-i:ft " + i + " -o:a " + Path.GetDirectoryName(destinationFolder) + "\\" + Path.GetFileNameWithoutExtension(i) + ".dsc",
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                CreateNoWindow = true
                            }
                        };
                        process.Start();
                        process.WaitForExit();
                        Logs.Logs.WriteLine("Converted - " + Path.GetFileName(i));
                        currentWorker--;
                    }).Start();
                    currentWorker++;
                }
            }
        }
    }
}
