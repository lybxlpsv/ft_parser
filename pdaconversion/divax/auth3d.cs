using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KKtLib;
using MikuMikuModel.FarcPack;
using KKtMain = KKtLib.Main;
using KKtA3DA = KKtLib.A3DA.A3DA;
using System.Threading;
using MikuMikuModel.Logs;

namespace ft_module_parser.pdaconversion.divax
{
    class auth3d
    {
        public static int currentWorker = 0;
        public static int maxWorker = 4;

        public static void ExtractA3D(string path, string acpath, a3ds a3db, divamodgen divamods = null)
        {
            Directory.CreateDirectory("a3d");
            //Directory.CreateDirectory("a3d\\common");

            foreach (string file in Directory.EnumerateFiles(path, "effstgpv*.farc", SearchOption.TopDirectoryOnly))
            {
                if (!file.Contains("ptc"))
                {
                    Tools.Extract(file, "a3d\\" + Path.GetFileNameWithoutExtension(file).Replace("effstgpv0", "EFFSTGPV8").ToUpper() + "\\");
                    var check = a3db.a3d_dbs.Any(c => c.rawrows.Any(d => d.Contains(Path.GetFileNameWithoutExtension(file).Replace("effstgpv", "EFFSTGPV").ToUpper())));
                    if (!check)
                    {
                        a3d newa3d = new a3d(0);
                        newa3d.id = a3db.a3d_dbs.Max(c => c.id) + 1;
                        newa3d.rawrows.Add(".value=" + Path.GetFileNameWithoutExtension(file).Replace("effstgpv", "EFFSTGPV").ToUpper());
                        a3db.a3d_dbs.Add(newa3d);
                    }
                }
            }

            foreach (string file in Directory.EnumerateFiles("a3d\\", "*.a3da", SearchOption.AllDirectories))
            {
                File.Move(file, Path.GetDirectoryName(file) + "\\" + Path.GetFileNameWithoutExtension(file).ToUpper().Replace("STGPV0", "STGPV8") + ".a3da");
            }

            foreach (string file in Directory.EnumerateFiles(path, "a3d_effpv*.farc", SearchOption.TopDirectoryOnly))
            {
                Tools.Extract(file, "a3d\\" + Path.GetFileNameWithoutExtension(file).Replace("a3d_effpv", "EFFSTGPV").ToUpper() + "\\");

                var check = a3db.a3d_dbs.Any(c => c.rawrows.Any(d => d.Contains(Path.GetFileNameWithoutExtension(file).Replace("effstgpv", "EFFSTGPV").ToUpper())));
                if (!check)
                {
                    a3d newa3d = new a3d(0);
                    newa3d.id = a3db.a3d_dbs.Max(c => c.id) + 1;
                    newa3d.rawrows.Add(".value=" + Path.GetFileNameWithoutExtension(file).Replace("a3d_effpv", "EFFSTGPV").ToUpper());
                    a3db.a3d_dbs.Add(newa3d);
                }
            }

            foreach (string file in Directory.EnumerateFiles(path, "CAMPV*.farc", SearchOption.TopDirectoryOnly))
            {
                Tools.Extract(file, "a3d\\" + Path.GetFileNameWithoutExtension(file).ToUpper() + "\\");
                var check = a3db.a3d_dbs.Any(c => c.rawrows.Any(d => d.Contains(Path.GetFileNameWithoutExtension(file).ToUpper())));
                if (!check)
                {
                    a3d newa3d = new a3d(0);
                    newa3d.id = a3db.a3d_dbs.Max(c => c.id) + 1;
                    newa3d.rawrows.Add(".value=" + Path.GetFileNameWithoutExtension(file).ToUpper());
                    a3db.a3d_dbs.Add(newa3d);
                }
            }

            foreach (string file in Directory.EnumerateFiles("a3d\\", "*.a3da", SearchOption.AllDirectories))
            {
                File.Move(file, Path.GetDirectoryName(file) + "\\" + Path.GetFileNameWithoutExtension(file).ToUpper().Replace("EFFPV", "STGPV") + ".a3da");
            }
        }

        public static void ConvertA3D(string path, string acpath, a3ds a3db, divamodgen divamods = null)
        {
            maxWorker = Environment.ProcessorCount - 2;
            
            foreach (string file in Directory.EnumerateFiles("a3d\\", "*.a3da", SearchOption.AllDirectories))
            {
                while (currentWorker > maxWorker)
                {
                    Thread.Sleep(33);
                }
                new Thread(() =>
                {
                    string ext = Path.GetExtension(file);
                    string filepath = file.Replace(ext, "");

                    KKtMain.Format Format = KKtMain.Format.FT;
                    KKtA3DA A = new KKtA3DA();
                    A.A3DAReader(filepath);
                    bool MP = false;

                    if (!MP || Format > KKtMain.Format.NULL)
                        A.IO = KKtLib.IO.OpenWriter(filepath + ".a3da", true);
                    if (Format > KKtMain.Format.NULL)
                    {
                        if (A.Data.Header.Format < KKtMain.Format.F2LE)
                            A.Data._.CompressF16 = Format == KKtMain.Format.MGF ? 2 : 1;
                        A.Data.Header.Format = Format;
                    }
                    if (!MP && A.Data.Header.Format > KKtMain.Format.DT && A.Data.Header.Format != KKtMain.Format.FT)
                        A.A3DCWriter(filepath);
                    else if (!MP || Format > KKtMain.Format.NULL)
                    {
                        A.A3DC = false;
                        A.A3DAWriter(filepath);
                    }
                    else
                        A.MsgPackWriter(filepath);
                    Interlocked.Decrement(ref currentWorker);
                }).Start();
                Interlocked.Increment(ref currentWorker);

            }

            while (currentWorker != 0)
            {
                Thread.Sleep(33);
            }

        }

        public static a3ds CreateDBEntries(string acpath, a3ds a3db, divamodgen divamods)
        {
            foreach (string file in Directory.EnumerateFiles("a3d\\", "*.a3da", SearchOption.AllDirectories))
            {
                string[] a3da_lines = File.ReadAllLines(file);

                StreamWriter outp = new StreamWriter(file, false, Encoding.ASCII);
                outp.NewLine = "\n";

                foreach (var i in a3da_lines)
                {
                    var u = i.Replace("EFFPV", "STGPV").Replace("STGPV0", "STGPV8");
                    outp.WriteLine(u);
                }
                outp.Close();
                

                if ((divamods != null) && (file.Contains("STGPV")))
                {
                    int pvid = int.Parse(Path.GetFileName(file).Substring(5, 3));

                    if (pvid < 200)
                    {
                        pvid = pvid + 800;
                    }

                    {
                        var check2 = divamods.Divamods.Where(c => c.pvid == pvid).FirstOrDefault();
                        if (check2 == null)
                        {
                            divamods.Divamods.Add(new pdaconversion.divamods(pvid));
                            Logs.WriteLine("a3da: Created new PV at id " + (pvid));
                            check2 = divamods.Divamods.Where(c => c.pvid == pvid).First();
                        }
                        check2.a3da.Add(Path.GetFileNameWithoutExtension(file));
                        Logs.WriteLine("a3da: Added a3d for PV at id " + pvid + "," + Path.GetFileNameWithoutExtension(file));
                    }

                    if (pvid >= 800)
                    {
                        
                        if (!Path.GetFileNameWithoutExtension(file).Contains("effpv"))
                        {
                            var check2 = divamods.Divamods.Where(c => c.pvid == (pvid)).FirstOrDefault();
                            if (check2 == null)
                            {
                                divamods.Divamods.Add(new pdaconversion.divamods(pvid-100));
                                Logs.WriteLine("a3da: Created new PV at id " + (pvid-100));
                                check2 = divamods.Divamods.Where(c => c.pvid == (pvid-100)).First();
                            }
                            Logs.WriteLine("a3da: Added a3d for PV at id " + (pvid -100) + "," + Path.GetFileNameWithoutExtension(file));
                            check2.a3da.Add(Path.GetFileNameWithoutExtension(file));
                        }
                    }
                }
                
                a3dfixer a3Dfixer = new a3dfixer();
                a3d newa3d = new a3d(0);
                newa3d.id = a3db.a3d_dbs_uid.Max(c => c.id) + 1;
                var lepath = Path.GetDirectoryName(file).Split('\\');
                {
                    newa3d.rawrows.Add(".category=" + lepath[lepath.Count() - 1]);
                    newa3d.rawrows.Add(".size=" + a3Dfixer.getSize(file));
                    newa3d.rawrows.Add(".value=A " + Path.GetFileNameWithoutExtension(file).ToUpper());
                    a3db.a3d_dbs_uid.Add(newa3d);
                }
            }

            Logs.WriteLine("Packing farcs...");
            Tools.MassPackFolders("a3d", acpath + @"\rom\auth_3d\");

            return a3db;
        }
    }
}
