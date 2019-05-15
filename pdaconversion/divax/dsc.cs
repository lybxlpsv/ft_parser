using MikuMikuModel.FarcPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DivaScriptConv;
using MikuMikuModel.Logs;

namespace ft_module_parser.pdaconversion.divax
{
    class dsc
    {
        public static void ConvertDSC(string path, string acpath, divamodgen divamods)
        {
            foreach (string file in Directory.EnumerateFiles(path, "pv_*_common.farc", SearchOption.TopDirectoryOnly))
            {
                Tools.Extract(file, "dsc\\common\\");
            }

            foreach (string file in Directory.EnumerateFiles(path, "*.dsc", SearchOption.TopDirectoryOnly))
            {
                File.Copy(file, "dsc\\" + Path.GetFileName(file), true);
            }

            foreach (string file in Directory.EnumerateFiles("dsc\\", "*.*", SearchOption.TopDirectoryOnly))
            {
                PD_Tool.DIVAFILE.Decrypt(0, file);
            }

            Console.Title = "LYB DIVA X";

            foreach (string file in Directory.EnumerateFiles(path, "*.dsc", SearchOption.TopDirectoryOnly))
            {
                List<string> args = new List<string>();
                args.Add("-i:x");
                args.Add(file);
                args.Add("-o:a");
                args.Add(acpath + "\\rom\\script\\" + Path.GetFileName(file));

                int pvid = int.Parse(Path.GetFileNameWithoutExtension(file).Substring(3, 3));

                string dexfile = path + "exp_pv" + string.Format("{0:000}", pvid) + ".dex";

                if (File.Exists(dexfile))
                {
                    args.Add("-e");
                    args.Add(dexfile);
                    Logs.WriteLine("PD_TOOL: DexReader: " + Path.GetFileName(dexfile));
                }

                var check = divamods.Divamods.Where(c => c.pvid == pvid).FirstOrDefault();
                if (check == null)
                {
                    divamods.Divamods.Add(new pdaconversion.divamods(pvid));
                    Logs.WriteLine("DSC: Created new PV at id " + (pvid));
                    check = divamods.Divamods.Where(c => c.pvid == pvid).First();
                }

                if (Path.GetFileName(file).Contains("easy"))
                    check.easy = true;
                if (Path.GetFileName(file).Contains("normal"))
                    check.normal = true;
                if (Path.GetFileName(file).Contains("hard"))
                    check.hard = true;
                if (Path.GetFileName(file).Contains("extreme"))
                    check.extreme = true;

                bool duet = false;
                
                {
                    if (check.duet)
                    {
                        duet = true;
                        Logs.WriteLine("DSC: " + pvid + " FORCEDUET");
                    }
                }

                DSC.duet = duet;
                DSC.Convert(args.ToArray());
            }

        }

    }
}
