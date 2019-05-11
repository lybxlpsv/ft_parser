using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MikuMikuModel.Logs
{
    class Logs
    {
        public static string logs = "";
        public static bool pending_write = false;
        public static List<string> logs_list = new List<string>();
        public static StreamWriter outp;

        public static void Initialize()
        {
            outp = new StreamWriter("logs.txt");
            outp.AutoFlush = true;
        }

        public static void Close()
        {
            outp.Close();
        }

        public static void WriteLine(string s)
        {
            Console.WriteLine(s);
            outp.Write(s + "\n");
            logs_list.Add(s);
            string temp_logs = "";
            for (int i = Math.Max(logs_list.Count - 50, 0); i < logs_list.Count; ++i)
            {
                temp_logs = temp_logs + logs_list[i] + "\r\n";
            }
            logs = temp_logs;
        }
    }
}
