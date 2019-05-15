using MikuMikuLibrary.Databases;
using MikuMikuModel.FarcPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ft_module_parser.pdaconversion.divax
{
    class mass_convert
    {
        ObjectDatabase objdb = new ObjectDatabase();
        objset divax = new objset();
        StageDatabase staged = new StageDatabase();
        a3ds auth3d_db = new a3ds();
        divamodgen divamods = new divamodgen();
        ConcurrentStack<models> model_list = new ConcurrentStack<models>();

        public static int currentWorker = 0;
        public static int maxWorker = 4;
        public void Convert(string path, string objdbpath, string texdbpath, string stagedbpath, string a3ddbpath, string mot_db, string acpath)
        {
            maxWorker = Environment.ProcessorCount;

            Console.Title = "LYB DIVA X2A";
            Console.WriteLine("lyb's Diva X to A Conversion Utility");
            Console.WriteLine("Credit : Skyth (MikuMikuLibrary), KorenKonder (KKtLib)," +
            " Raki Saionji (DivaScriptConv),  Samyuu (DivaScript, ScriptUtilities)");
            Console.WriteLine("");

            divapvmod divapvmods = new divapvmod();
            divapvmods.RestoreDb(true);

            if (Directory.Exists("a3d"))
                Directory.Delete("a3d", true);
            if (Directory.Exists("farc"))
                Directory.Delete("farc", true);
            if (Directory.Exists("temp"))
                Directory.Delete("temp", true);
            if (Directory.Exists("dsc"))
                Directory.Delete("dsc", true);
            if (Directory.Exists("mot"))
                Directory.Delete("mot", true);
            
            Directory.CreateDirectory("temp");
            Directory.CreateDirectory("farc");
            Directory.CreateDirectory("dsc");
            Directory.CreateDirectory("farc");

            if (!Directory.Exists(acpath + "\\divamods"))
                Directory.CreateDirectory(acpath + "\\divamods");
            
            objdb.Load(objdbpath);
            staged.Load(stagedbpath);
            auth3d_db.load(a3ddbpath);
            
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                auth3d.ExtractA3D(path, acpath, auth3d_db);
                currentWorker--;
            }).Start();
            currentWorker++;

            foreach (string file in Directory.EnumerateFiles(path, "stgpv*.farc", SearchOption.TopDirectoryOnly))
            {
                while (currentWorker > maxWorker)
                {
                    Thread.Sleep(33);
                }
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    var le_model = divax.Stripify(file);
                    model_list.Push(le_model);
                    currentWorker--;
                }).Start();
                currentWorker++;
            }

            foreach (string file in Directory.EnumerateFiles(path, "effpv*.farc", SearchOption.TopDirectoryOnly))
            {
                while (currentWorker > maxWorker)
                {
                    Thread.Sleep(33);
                }
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    var le_model = divax.Stripify(file);
                    model_list.Push(le_model);
                    currentWorker--;
                }).Start();
                currentWorker++;
            }

            while (currentWorker != 0)
            {
                Thread.Sleep(33);
            }

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                auth3d.ConvertA3D(path, acpath, auth3d_db);
                currentWorker--;
            }).Start();
            currentWorker++;

            var texturedb = new TextureDatabase();
            texturedb.Load(texdbpath);

            while (model_list.Count > 0)
            {
                models stgpv;
                model_list.TryPop(out stgpv);
                Console.WriteLine(Path.GetFileName(stgpv.fileName));
                divax.GenerateObjSet(stgpv.fileName, stgpv.model, objdb, texturedb, staged, acpath, divamods, true, false);
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            texturedb.Save(acpath + @"\rom\objset\tex_db.bin");

            model_list.Clear();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            /*
            foreach (string file in Directory.EnumerateFiles(path, "stgpv*.farc", SearchOption.TopDirectoryOnly))
            {
                Console.WriteLine(Path.GetFileName(file));
                var stgpv = model_list.Where(c => c.fileName == Path.GetFileName(path)).First();
                divax.GenerateObjSet(file, stgpv.model, objdb, texdbpath, staged, acpath, divamods, true, false);
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            foreach (string file in Directory.EnumerateFiles(path, "effpv*.farc", SearchOption.TopDirectoryOnly))
            {
                Console.WriteLine(Path.GetFileName(file));
                var stgpv = model_list.Where(c => c.fileName == Path.GetFileName(path)).First();
                divax.GenerateObjSet(file, stgpv.model, objdb, texdbpath, staged, acpath, divamods, true, false);
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            */

            objdb.Save(acpath + @"\rom\objset\obj_db.bin");
            staged.Save(acpath + @"\rom\stage_data.bin");

            while (currentWorker != 0)
            {
                Thread.Sleep(33);
            }

            auth3d.CreateDBEntries(acpath, auth3d_db, divamods);
            
            auth3d_db.save(acpath + @"\rom\auth_3d\auth_3d_db.bin");
            
            motion.Convert(path, acpath, mot_db, divamods);
            
            //motion.dbgme(path, acpath, divamods);
            
            dsc.ConvertDSC(path, acpath, divamods);

            var csv = File.ReadAllLines("x_pv.csv");

            foreach (var i in csv)
            {
                var lines = i.Split(',');
                divamods.SetU1P2P(lines[0], lines[1], lines[2], lines[4]);
            }

            divamods.GenerateDivamods(acpath);
            
            divapvmods.ApplyMods(acpath, true);
            
            Console.WriteLine("DONE!");
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.Beep();
            Console.ReadKey();
        }
    }
}
