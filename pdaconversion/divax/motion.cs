using MikuMikuLibrary.Archives;
using MikuMikuLibrary.Archives.Farc;
using MikuMikuLibrary.Databases;
using MikuMikuLibrary.IO;
using MikuMikuLibrary.Motions;
using MikuMikuModel.FarcPack;
using MikuMikuModel.Logs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ft_module_parser.pdaconversion.divax
{
    class motion
    {
        public static void dbgme(string path, string acpath, divamodgen divamods)
        {
            foreach (string file in Directory.EnumerateFiles(path + "temp", "*p1_00.mot", SearchOption.AllDirectories))
            {
                int pvid = int.Parse(Path.GetFileNameWithoutExtension(file).Substring(2, 3));

                var check2 = divamods.Divamods.Where(c => c.pvid == pvid).FirstOrDefault();
                if (check2 == null)
                {
                    divamods.Divamods.Add(new pdaconversion.divamods(pvid));
                    Logs.WriteLine("motions: Created new PV at id " + pvid);
                    check2 = divamods.Divamods.Where(c => c.pvid == pvid).First();
                }
            }

            foreach (string file in Directory.EnumerateFiles(path + "temp", "*p2_00.mot", SearchOption.AllDirectories))
            {
                int pvid = int.Parse(Path.GetFileNameWithoutExtension(file).Substring(2, 3));

                var check2 = divamods.Divamods.Where(c => c.pvid == pvid).FirstOrDefault();
                if (check2 == null)
                {
                    divamods.Divamods.Add(new pdaconversion.divamods(pvid));
                    Logs.WriteLine("motions: Created new PV at id " + pvid);
                    check2 = divamods.Divamods.Where(c => c.pvid == pvid).First();
                }
                check2.duet = true;
            }

        }
        public static void Convert(string path, string acpath, string mot_db, divamodgen divamods)
        {
            if (Directory.Exists(path + "temp"))
                Directory.Delete(path + "temp", true);
            if (Directory.Exists(path + "temp2"))
                Directory.Delete(path + "temp2", true);
            if (Directory.Exists(acpath + "\\rom\\rob_temp"))
                Directory.Delete(acpath + "\\rom\\rob_temp", true);

            Directory.CreateDirectory(path + "temp");
            Directory.CreateDirectory(path + "temp2");
            Directory.CreateDirectory(acpath + "\\rom\\rob_temp");

            MotionDatabase acmot = new MotionDatabase();
            BoneDatabase acbone = new BoneDatabase();
            acbone.Load(acpath + "\\rom\\bone_data.bin");
            var skeletonEntry = acbone.Skeletons[0];

            using (var farcArchive = BinaryFile.Load<FarcArchive>(mot_db))
            using (var entryStream = farcArchive.Open(farcArchive.Entries.First(), EntryStreamMode.MemoryStream))
                acmot.Load(entryStream);

            foreach (string file in Directory.EnumerateFiles(path, "mot_pv*.farc", SearchOption.TopDirectoryOnly))
            {
                Tools.Extract(file, path + "temp\\");
                //Console.WriteLine("Extracted " + Path.GetFileName(file));
            }

            var maxid = acmot.MotionSets.Max(c => c.Id);
            var maxid2 = -1;

            foreach (var i in acmot.MotionSets)
            {
                if (i.Id > maxid2)
                    maxid2 = i.Id;
            }

            foreach (string file in Directory.EnumerateFiles(path + "temp", "*p1_00.mot", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(path + "temp2\\mot_PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3));
                var moti = CombineBone(path, file, path + "temp2\\mot_PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3) + "\\mot_PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3) + "_p1.mot");
                MotionSet motion = new MotionSet();
                if (File.Exists(acpath + "\\rom\\rob_temp\\mot_PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3) + "\\mot_PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3) + ".bin"))
                    motion.Load(acpath + "\\rom\\rob_temp\\mot_PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3) + "\\mot_PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3) + ".bin");
                motion.Motions.Add(moti);
                if (!Directory.Exists(acpath + "\\rom\\rob_temp\\mot_PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3)))
                    Directory.CreateDirectory(acpath + "\\rom\\rob_temp\\mot_PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3));
                motion.Save(acpath + "\\rom\\rob_temp\\mot_PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3) + "\\mot_PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3) + ".bin", skeletonEntry, acmot);

                maxid++;
                maxid2++;

                var check = acmot.MotionSets.Where(c => c.Name == "PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3)).FirstOrDefault();
                if (check == null)
                {
                    var motset = new MotionSetEntry();
                    motset.Name = "PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3);
                    motset.Id = maxid + 1;
                    acmot.MotionSets.Add(motset);
                    check = acmot.MotionSets.Where(c => c.Name == "PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3)).First();
                }

                var motentry = new MotionEntry();
                motentry.Id = maxid2 + 1;
                motentry.Name = "PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3) + "_P1";
                check.Motions.Add(motentry);

                int pvid = int.Parse(Path.GetFileNameWithoutExtension(file).Substring(2, 3));

                var check2 = divamods.Divamods.Where(c => c.pvid == pvid).FirstOrDefault();
                if (check2 == null)
                {
                    divamods.Divamods.Add(new pdaconversion.divamods(pvid));
                    Logs.WriteLine("Motion: Created new PV at id " + (pvid));
                    check2 = divamods.Divamods.Where(c => c.pvid == pvid).First();
                }

                Console.WriteLine("Converted " + Path.GetFileName(file));
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            foreach (string file in Directory.EnumerateFiles(path + "temp", "*p2_00.mot", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(path + "temp2\\mot_PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3));
                var moti = CombineBone(path, file, path + "temp2\\mot_PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3) + "\\mot_PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3) + "_p2.mot");
                MotionSet motion = new MotionSet();
                if (File.Exists(acpath + "\\rom\\rob_temp\\mot_PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3) + "\\mot_PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3) + ".bin"))
                    motion.Load(acpath + "\\rom\\rob_temp\\mot_PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3) + "\\mot_PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3) + ".bin");
                motion.Motions.Add(moti);
                if (!Directory.Exists(acpath + "\\rom\\rob_temp\\mot_PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3)))
                    Directory.CreateDirectory(acpath + "\\rom\\rob_temp\\" + Path.GetFileNameWithoutExtension(file).Substring(2, 3));
                motion.Save(acpath + "\\rom\\rob_temp\\mot_PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3) + "\\mot_PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3) + ".bin", skeletonEntry, acmot);
                Console.WriteLine("Converted " + Path.GetFileName(file));

                maxid++;
                maxid2++;

                var check = acmot.MotionSets.Where(c => c.Name == "PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3)).FirstOrDefault();
                if (check == null)
                {
                    var motset = new MotionSetEntry();
                    motset.Name = "PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3);
                    motset.Id = maxid;
                    acmot.MotionSets.Add(motset);
                    check = acmot.MotionSets.Where(c => c.Name == "PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3)).First();
                }

                var motentry = new MotionEntry();
                motentry.Id = maxid2;
                motentry.Name = "PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3) + "_P2";
                check.Motions.Add(motentry);

                int pvid = int.Parse(Path.GetFileNameWithoutExtension(file).Substring(2, 3));

                var check2 = divamods.Divamods.Where(c => c.pvid == pvid).FirstOrDefault();
                if (check2 == null)
                {
                    divamods.Divamods.Add(new pdaconversion.divamods(pvid));
                    Logs.WriteLine("Motion: Created new PV at id " + (pvid));
                    check2 = divamods.Divamods.Where(c => c.pvid == pvid).First();
                    //check2.duet = true;
                } 

                check2.duet = true;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            foreach (string file in Directory.EnumerateFiles(path + "temp", "*p3_00.mot", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(path + "temp2\\mot_PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3));
                var moti = CombineBone(path, file, path + "temp2\\mot_PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3) + "\\mot_PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3) + "_p3.mot");
                MotionSet motion = new MotionSet();
                if (File.Exists(acpath + "\\rom\\rob_temp\\mot_PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3) + "\\mot_PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3) + ".bin"))
                    motion.Load(acpath + "\\rom\\rob_temp\\mot_PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3) + "\\mot_PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3) + ".bin");
                motion.Motions.Add(moti);
                if (!Directory.Exists(acpath + "\\rom\\rob_temp\\mot_PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3)))
                    Directory.CreateDirectory(acpath + "\\rom\\rob_temp\\mot_PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3));
                motion.Save(acpath + "\\rom\\rob_temp\\mot_PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3) + "\\mot_PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3) + ".bin", skeletonEntry, acmot);
                Console.WriteLine("Converted " + Path.GetFileName(file));

                maxid++;
                maxid2++;

                var check = acmot.MotionSets.Where(c => c.Name == "PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3)).FirstOrDefault();
                if (check == null)
                {
                    var motset = new MotionSetEntry();
                    motset.Name = "PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3);
                    motset.Id = maxid;
                    acmot.MotionSets.Add(motset);
                    check = acmot.MotionSets.Where(c => c.Name == "PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3)).First();
                }

                var motentry = new MotionEntry();
                motentry.Id = maxid2;
                motentry.Name = "PV" + Path.GetFileNameWithoutExtension(file).Substring(2, 3) + "_P3";
                check.Motions.Add(motentry);
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            Directory.CreateDirectory(acpath + "\\rom\\rob_temp\\mot_db");
            acmot.Save(acpath + "\\rom\\rob_temp\\mot_db\\mot_db.bin");

            Console.WriteLine("Packing farcs...");
            Tools.MassPackFolders(acpath + "\\rom\\rob_temp", acpath + "\\rom\\rob\\");
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public static Motion CombineBone(string path, string filePath, string outputFilePath)
        {
            string baseFilePath = Path.ChangeExtension(filePath, null);
            BoneDatabase bonedb = new BoneDatabase();
            bonedb.Load(path + "bone_data.bon");
            var rootMotion = new Motion();

            var skeletonEntry = bonedb.Skeletons[0];
            
            rootMotion.Load(filePath, skeletonEntry);

            var rootController = rootMotion.GetController();

            for (int i = 1; ; i++)
            {
                string divFilePath = $"{baseFilePath}_div_{i}.mot";
                if (!File.Exists(divFilePath))
                    break;

                var divMotion = new Motion();
                {
                    divMotion.Load(divFilePath, skeletonEntry);
                }

                var divController = divMotion.GetController();
                rootController.Merge(divController);
            }

            return rootMotion;
        }
    }
}
