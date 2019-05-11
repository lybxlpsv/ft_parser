using MikuMikuLibrary.Archives;
using MikuMikuLibrary.Archives.Farc;
using MikuMikuModel.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MikuMikuModel.FarcPack
{
    class Tools
    {
        static int currentWorker = 0;
        public static int maxWorker = 3;
        public static void MassExtract(string sourceFolder, string destinationFolder)
        {
            string[] files = System.IO.Directory.GetFiles(sourceFolder, "*.farc");
            foreach (var i in files)
            {
                MikuMikuModel.FarcPack.Tools.Compress(i, destinationFolder);
                Logs.Logs.WriteLine("Compress - " + Path.GetFileName(i));
            }
        }

        public static void MassExtractRob(string sourceFolder, string destinationFolder, string ac_path)
        {
            string[] files = System.IO.Directory.GetFiles(sourceFolder, "*.farc");
            foreach (var i in files)
            {
                if (!File.Exists(ac_path + @"\rob\" + Path.GetFileNameWithoutExtension(i) + ".farc"))
                {
                    MikuMikuModel.FarcPack.Tools.Compress(i, destinationFolder);
                    Logs.Logs.WriteLine("Extracted - " + Path.GetFileName(i));
                }

            }
        }

        public static void Extract(string sourceFolder)
        {
            MikuMikuModel.FarcPack.Tools.Compress(sourceFolder, null);
            Logs.Logs.WriteLine("Extracted - " + Path.GetFileName(sourceFolder));
        }

        public static void Extract(string sourceFolder, string targetFolder)
        {
            MikuMikuModel.FarcPack.Tools.Compress(sourceFolder, targetFolder);
            Logs.Logs.WriteLine("Extracted - " + Path.GetFileName(sourceFolder));
        }

        public static void MassPackFolders(string sourceFolder, string destinationFolder)
        {
            string[] folders = System.IO.Directory.GetDirectories(sourceFolder);
            foreach (var i in folders)
            {
                MikuMikuModel.FarcPack.Tools.Compress(i, destinationFolder + Path.GetFileName(i));
                Logs.Logs.WriteLine("MassPack - " + Path.GetFileName(i));
            }
        }
        public static void MassPack(string sourceFolder, string destinationFolder)
        {
            string[] files = System.IO.Directory.GetFiles(sourceFolder, "*.bin", SearchOption.AllDirectories);
            foreach (var i in files)
            {
                if (!File.Exists(destinationFolder + "\\" + Path.GetFileNameWithoutExtension(i) + ".farc"))
                {
                    while (currentWorker > maxWorker)
                    {
                        Thread.Sleep(33);
                    }
                    new Thread(() =>
                    {
                        Thread.CurrentThread.IsBackground = true;

                        MikuMikuModel.FarcPack.Tools.Archive(i, destinationFolder + "\\" + Path.GetFileNameWithoutExtension(i) + ".farc");
                        Logs.Logs.WriteLine("Repacked - " + destinationFolder + "\\" + Path.GetFileNameWithoutExtension(i) + ".farc");
                        currentWorker--;
                    }).Start();
                    currentWorker++;
                }
            }
        }

        public static void CopyFiles(string sourcePath, string destinationPath)
        {
            string[] files = Directory.GetFiles(sourcePath);

            foreach (string file in files)
            {
                string fname = file.Substring(sourcePath.Length);
                string dest = Path.Combine(destinationPath, fname);
                if (!File.Exists(dest))
                {
                    File.Copy(file, dest, true);
                    Logs.Logs.WriteLine("Copied - " + Path.GetFileName(fname));
                }
                else
                {
                    //Logs.Logs.WriteLine("Skipped - " + Path.GetFileName(fname));
                }
            }
        }

        public static void CleanIfExists(string sourceFolder, string destinationFolder)
        {
            string[] files = System.IO.Directory.GetFiles(sourceFolder, "*.farc");
            foreach (var i in files)
            {
                if (!File.Exists(destinationFolder + Path.GetFileName(i)))
                {
                    File.Delete(destinationFolder + Path.GetFileName(i));
                }
                else
                {

                }
            }
        }

        public static void RepackFile(string sourceFileName, string destinationFileName, bool compress = false)
        {
            int alignment = 16;

            //destinationFileName = Path.ChangeExtension(destinationFileName, null);

            using (var stream = File.OpenRead(sourceFileName))
            {
                var farcArchive = FarcArchive.Load<FarcArchive>(stream);
                farcArchive.IsCompressed = compress;
                farcArchive.Alignment = alignment;
                farcArchive.Save(destinationFileName);
            }
        }

        public static void Repack(string sourceFolder, string destinationFolder, bool compress = false)
        {
            string[] files = System.IO.Directory.GetFiles(sourceFolder, "*.farc");
            foreach (var i in files)
            {
                if (!File.Exists(destinationFolder + Path.GetFileName(i)))
                {
                    while (currentWorker > maxWorker)
                    {
                        Thread.Sleep(100);
                    }
                    new Thread(() =>
                    {
                        Thread.CurrentThread.IsBackground = true;


                        //MikuMikuModel.FarcPack.Tools.Compress(i, null);
                        //MikuMikuModel.FarcPack.Tools.Compress(Path.GetDirectoryName(i) + "\\" + Path.GetFileNameWithoutExtension(i), destinationFolder + Path.GetFileName(i));
                        //Directory.Delete(Path.GetDirectoryName(i) + "\\" + Path.GetFileNameWithoutExtension(i) + "\\", true);

                        RepackFile(i, destinationFolder + Path.GetFileName(i), compress);

                        Logs.Logs.WriteLine("Repacked - " + Path.GetFileName(i) + " compressed = " + compress);
                        
                        currentWorker--;
                    }).Start();
                    currentWorker++;
                }
                else
                {
                    //Logs.Logs.WriteLine("Skipped - " + Path.GetFileName(i));
                }
            }
        }

        public static void RepackReplace(string sourceFolder, string destinationFolder, bool compress = false)
        {
            string[] files = System.IO.Directory.GetFiles(sourceFolder, "*.farc");
            foreach (var i in files)
            {
                {

                    while (currentWorker > maxWorker)
                    {
                        Thread.Sleep(100);
                    }
                    new Thread(() =>
                    {
                        Thread.CurrentThread.IsBackground = true;

                        //MikuMikuModel.FarcPack.Tools.Compress(i, null);
                        //MikuMikuModel.FarcPack.Tools.Compress(Path.GetDirectoryName(i) + "\\" + Path.GetFileNameWithoutExtension(i), destinationFolder + Path.GetFileName(i));
                        //Directory.Delete(Path.GetDirectoryName(i) + "\\" + Path.GetFileNameWithoutExtension(i) + "\\", true);
                        RepackFile(i, destinationFolder + Path.GetFileName(i), compress);

                        Logs.Logs.WriteLine("Repacked - " + Path.GetFileName(i));

                        currentWorker--;
                    }).Start();
                    currentWorker++;
                }
            }
        }

        public static void Archive(string sourceFileName, string destinationFileName)
        {
            bool compress = false;
            int alignment = 16;
            var farcArchive = new FarcArchive();
            farcArchive.IsCompressed = compress;
            farcArchive.Alignment = alignment;
            farcArchive.Add(Path.GetFileName(sourceFileName), sourceFileName);
            farcArchive.Save(destinationFileName);
        }

        public static void Compress(string sourceFileName, string destinationFileName)
        {

            bool compress = false;
            int alignment = 16;

            if (destinationFileName == null)
                destinationFileName = sourceFileName;

            if (File.GetAttributes(sourceFileName).HasFlag(FileAttributes.Directory))
            {
                destinationFileName = Path.ChangeExtension(destinationFileName, "farc");

                var farcArchive = new FarcArchive();
                farcArchive.IsCompressed = compress;
                farcArchive.Alignment = alignment;

                foreach (var filePath in Directory.EnumerateFiles(sourceFileName))
                    farcArchive.Add(Path.GetFileName(filePath), filePath);

                farcArchive.Save(destinationFileName);
            }

            else if (sourceFileName.EndsWith(".farc", StringComparison.OrdinalIgnoreCase))
            {
                destinationFileName = Path.ChangeExtension(destinationFileName, null);

                using (var stream = File.OpenRead(sourceFileName))
                {
                    var farcArchive = FarcArchive.Load<FarcArchive>(stream);

                    Directory.CreateDirectory(destinationFileName);
                    foreach (var fileName in farcArchive)
                    {
                        using (var destination = File.Create(Path.Combine(destinationFileName, fileName)))
                        using (var source = farcArchive.Open(fileName, EntryStreamMode.OriginalStream))
                            source.CopyTo(destination);
                    }
                }
            }
            else if (sourceFileName.EndsWith(".bak", StringComparison.OrdinalIgnoreCase))
            {
                destinationFileName = Path.ChangeExtension(destinationFileName, null);

                using (var stream = File.OpenRead(sourceFileName))
                {
                    var farcArchive = FarcArchive.Load<FarcArchive>(stream);

                    Directory.CreateDirectory(destinationFileName);
                    foreach (var fileName in farcArchive)
                    {
                        using (var destination = File.Create(Path.Combine(destinationFileName, fileName)))
                        using (var source = farcArchive.Open(fileName, EntryStreamMode.OriginalStream))
                            source.CopyTo(destination);
                    }
                }
            }
        }
    }
}
