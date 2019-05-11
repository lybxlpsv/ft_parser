using MikuMikuLibrary.Archives;
using MikuMikuLibrary.Archives.Farc;
using MikuMikuLibrary.Databases;
using MikuMikuLibrary.IO;
using MikuMikuLibrary.Models;
using MikuMikuLibrary.Motions;
using MikuMikuLibrary.Sprites;
using MikuMikuModel.DivaScriptConverter;
using MikuMikuModel.FarcPack;
using MikuMikuModel.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ft_module_parser.pdaconversion.ftdx
{
    class mass_convert
    {
        private bool ConvertToAc()
        {
            string ac_path = "";
            string ft_path = "";
            string fs_path = "";
            string ct_path = "";
            bool skip_objset = false;
            bool skip_robs = false;
            bool skip_2d = false;
            bool skip_auth3d = false;
            bool ask_user = false;
            bool skip_db = false;
            if (File.Exists("config.txt"))
            {
                StreamReader sr = new StreamReader("config.txt");
                while (sr.Peek() > -1)
                {

                    var line = sr.ReadLine();
                    var lines = line.Split('=');
                    if (lines[0] == "AC")
                    {
                        ac_path = lines[1];
                    }
                    if (lines[0] == "FT")
                    {
                        ft_path = lines[1];
                    }
                    if (lines[0] == "FS")
                    {
                        fs_path = lines[1];
                    }
                    if (lines[0] == "CT")
                    {
                        ct_path = lines[1];
                    }
                    if (lines[0] == "I_HAVE_CHANGED_THIS")
                    {
                        if (lines[1] == "TRUE")
                            ask_user = true;
                    }
                    if (lines[0] == "THREAD_NUM")
                    {
                        Tools.maxWorker = int.Parse(lines[1]);
                    }
                    if (lines[0] == "SKIP_OBJSET")
                    {
                        if (lines[1] == "TRUE")
                            skip_objset = true;
                    }

                    if (lines[0] == "SKIP_ROBS")
                    {
                        if (lines[1] == "TRUE")
                            skip_robs = true;
                    }

                    if (lines[0] == "SKIP_2D")
                    {
                        if (lines[1] == "TRUE")
                            skip_2d = true;
                    }

                    if (lines[0] == "SKIP_AUTH3D")
                    {
                        if (lines[1] == "TRUE")
                            skip_auth3d = true;
                    }

                    if (lines[0] == "SKIP_DB")
                    {
                        if (lines[1] == "TRUE")
                            skip_db = true;
                    }
                }
                sr.Close();
                if (ask_user)
                {
                    DialogResult dialogResult = MessageBox.Show("Start the convertion procedure? Make sure you already made configurations for FT and Arcade! DO NOT TOUCH ANTHING DURING THIS PROCESS!", "Convert", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        Logs.WriteLine("FT to AC START!");

                        if (!skip_db)
                        {
                            File.Copy(@"ported_db\mot_db.farc", ac_path + @"\rob\mot_db.farc", true);
                        }

                        if (!skip_auth3d)
                        {
                            //   SetTitle("auth_3d");
                            Logs.WriteLine("Auth3D Start");
                            Tools.Repack(ft_path + @"\auth_3d\", ac_path + @"\auth_3d\");
                            if (fs_path != "")
                            {
                                Logs.WriteLine("Auth3D FS Start");
                                Tools.Repack(fs_path + @"\auth_3d\", ac_path + @"\auth_3d\");
                            }
                            if (ct_path != "")
                            {
                                Logs.WriteLine("Auth3D CT Start");
                                Tools.Repack(ct_path + @"\auth_3d\", ac_path + @"\auth_3d\");
                            }
                            Logs.WriteLine("Auth3D Finished");
                        }
                        else Logs.WriteLine("Auth3D Skipped");
                        if (!skip_objset)
                        {
                            //   SetTitle("objset");
                            Logs.WriteLine("Objset Start");
                            Tools.RepackReplace(ft_path + @"\objset\", ac_path + @"\objset\", true);
                            if (fs_path != "")
                            {
                                Logs.WriteLine("Objset FS Start");
                                Tools.RepackReplace(fs_path + @"\objset\", ac_path + @"\objset\", true);
                            }
                            if (ct_path != "")
                            {
                                Logs.WriteLine("Objset CT Start");
                                Tools.RepackReplace(ct_path + @"\objset\", ac_path + @"\objset\", true);
                            }
                            Logs.WriteLine("Objset Finish");
                        }
                        else Logs.WriteLine("Objset Skipped");

                        /*
                        ConfigurationList configlist = ConfigurationList.Instance;
                        var config = configlist.Configurations.Where(c => c.Name == "Arcade").First();
                        config.BoneDatabaseFilePath = ac_path + @"\bone_data.bin";
                        config.TextureDatabaseFilePath = ac_path + @"\objset\tex_db.bin";
                        config.MotionDatabaseFilePath = ac_path + @"\rob\mot_db.farc";
                        config.ObjectDatabaseFilePath = ac_path + @"\objset\obj_db.bin";
                        config.Save();
                        config = configlist.Configurations.Where(c => c.Name == "FT").First();
                        config.BoneDatabaseFilePath = ft_path + @"\bone_data.bin";
                        config.TextureDatabaseFilePath = ft_path + @"\objset\tex_db.bin";
                        config.MotionDatabaseFilePath = ft_path + @"\rob\mot_db.farc";
                        config.ObjectDatabaseFilePath = ft_path + @"\objset\obj_db.bin";
                        config.Save();
                        configlist.Save();
                        */
                        Directory.CreateDirectory(ft_path + @"\rob_bins");
                        if (fs_path != "") Directory.CreateDirectory(fs_path + @"\rob_bins");
                        if (ct_path != "") Directory.CreateDirectory(ct_path + @"\rob_bins");
                        Directory.CreateDirectory(ac_path + @"\rob_bins");
                        Directory.CreateDirectory(ac_path + @"\rob_farc");
                        if (!skip_robs)
                        {
                            Logs.WriteLine("Robs Extract Skipped");
                            //   SetTitle("rob");
                            //Tools.MassExtractRob(ft_path + @"\rob\", ft_path + @"\rob_bins\", ac_path);
                            if (fs_path != "")
                            {
                                Logs.WriteLine("Robs FS Extract Skipped");
                                //Tools.MassExtractRob(fs_path + @"\rob\", fs_path + @"\rob_bins\", ac_path);
                            }
                            if (ct_path != "")
                            {
                                Logs.WriteLine("Robs CT Extract Skipped");
                                //Tools.MassExtractRob(ct_path + @"\rob\", ct_path + @"\rob_bins\", ac_path);
                            }
                            Logs.WriteLine("Robs Extract Finish");
                        }
                        else Logs.WriteLine("Robs Extract Skipped");
                        return true;
                    }
                    else if (dialogResult == DialogResult.No)
                    {
                        return false;
                        //Application.Exit();
                    }
                }
                else
                {
                    Console.WriteLine("You did not edit config.ini");
                    Console.ReadKey();
                    return false;
                }
            }
            else
            {
                //MessageBox.Show("config.txt not found");
                Console.WriteLine("config.txt not found");
                Console.ReadKey();
                return false;
            }
            return false;
        }

        private void do2DConvert()
        {
            string ac_path = "";
            string ft_path = "";
            string fs_path = "";
            string ct_path = "";
            bool ask_user = false;
            bool skip_robs = false;
            if (File.Exists("config.txt"))
            {
                StreamReader sr = new StreamReader("config.txt");
                while (sr.Peek() > -1)
                {
                    var line = sr.ReadLine();
                    var lines = line.Split('=');
                    if (lines[0] == "AC")
                    {
                        ac_path = lines[1];
                    }
                    if (lines[0] == "FT")
                    {
                        ft_path = lines[1];
                    }
                    if (lines[0] == "FS")
                    {
                        fs_path = lines[1];
                    }
                    if (lines[0] == "CT")
                    {
                        ct_path = lines[1];
                    }
                    if (lines[0] == "I_HAVE_CHANGED_THIS")
                    {
                        if (lines[1] == "TRUE")
                            ask_user = true;
                    }
                    if (lines[0] == "SKIP_2D")
                    {
                        if (lines[1] == "TRUE")
                            skip_robs = true;
                    }
                }
                sr.Close();
            }

            if ((ask_user) && (!skip_robs))
            {
                Logs.WriteLine("Sel2D Start");
                string[] filespl = System.IO.Directory.GetFiles(ft_path + @"\2d\", "spr_sel_pv*.farc", SearchOption.AllDirectories);
                string[] filesfs = { };
                string[] filesct = { };
                if (fs_path != "") filesfs = System.IO.Directory.GetFiles(fs_path + @"\2d\", "spr_sel_pv*.farc", SearchOption.AllDirectories);
                if (ct_path != "") filesct = System.IO.Directory.GetFiles(ct_path + @"\2d\", "spr_sel_pv*.farc", SearchOption.AllDirectories);
                string[] files = filespl.Concat(filesfs).ToArray().Concat(filesct).ToArray();

                
                foreach (var filePath in files)
                {
                    if ((!File.Exists(ac_path + @"\2d\" + Path.GetFileNameWithoutExtension(filePath) + ".farc")) && (Path.GetFileNameWithoutExtension(filePath) != "spr_sel_pv"))
                    {
                        Logs.WriteLine("Processing " + Path.GetFileNameWithoutExtension(filePath));
                        string farcpath = filePath;

                        var spriteset = new SpriteSet();

                        using (var farcArchive = BinaryFile.Load<FarcArchive>(farcpath))
                        using (var entryStream = farcArchive.Open(farcArchive.Entries.First(), EntryStreamMode.MemoryStream))
                            spriteset.Load(entryStream);

                        foreach (var i in spriteset.Sprites)
                            i.Field01 = 0x0000000D;


                        spriteset.Save("temp");

                        FarcArchive newfarc = new FarcArchive();
                        newfarc.Add(Path.GetFileNameWithoutExtension(filePath) + ".bin", "temp");
                        newfarc.Save(ac_path + "\\2d\\" + Path.GetFileName(filePath));
                    }
                    //newfarc.Dispose();
                }

            }
        }

        private void doRobConvert()
        {
            string ac_path = "";
            string ft_path = "";
            string fs_path = "";
            string ct_path = "";
            bool ask_user = false;
            bool skip_robs = false;
            if (File.Exists("config.txt"))
            {
                StreamReader sr = new StreamReader("config.txt");
                while (sr.Peek() > -1)
                {
                    var line = sr.ReadLine();
                    var lines = line.Split('=');
                    if (lines[0] == "AC")
                    {
                        ac_path = lines[1];
                    }
                    if (lines[0] == "FT")
                    {
                        ft_path = lines[1];
                    }
                    if (lines[0] == "FS")
                    {
                        fs_path = lines[1];
                    }
                    if (lines[0] == "CT")
                    {
                        ct_path = lines[1];
                    }
                    if (lines[0] == "I_HAVE_CHANGED_THIS")
                    {
                        if (lines[1] == "TRUE")
                            ask_user = true;
                    }
                    if (lines[0] == "SKIP_ROBS")
                    {
                        if (lines[1] == "TRUE")
                            skip_robs = true;
                    }
                }
                sr.Close();
            }
            if ((ask_user) && (!skip_robs))
            {
                Logs.WriteLine("Rob Retarget Start");
                string[] filespl = System.IO.Directory.GetFiles(ft_path + @"\rob\", "*.farc", SearchOption.AllDirectories);
                string[] filesfs = { };
                string[] filesct = { };
                if (fs_path != "") filesfs = System.IO.Directory.GetFiles(fs_path + @"\rob\", "*.farc", SearchOption.AllDirectories);
                if (ct_path != "") filesct = System.IO.Directory.GetFiles(ct_path + @"\rob\", "*.farc", SearchOption.AllDirectories);
                string[] files = filespl.Concat(filesfs).ToArray().Concat(filesct).ToArray();

                
                var BoneDatabaseFT = new BoneDatabase();
                var BoneDatabaseAC = new BoneDatabase();
                var MotionDatabaseFT = new MotionDatabase();
                var MotionDatabaseAC = new MotionDatabase();

                BoneDatabaseFT.Load(ft_path + "\\bone_data.bin");
                BoneDatabaseAC.Load(ac_path + "\\bone_data.bin");

                using (var farcArchive = BinaryFile.Load<FarcArchive>(ac_path + "\\rob\\mot_db.farc"))
                using (var entryStream = farcArchive.Open(farcArchive.Entries.First(), EntryStreamMode.MemoryStream))
                    MotionDatabaseAC.Load(entryStream);

                using (var farcArchive = BinaryFile.Load<FarcArchive>(ft_path + "\\rob\\mot_db.farc"))
                using (var entryStream = farcArchive.Open(farcArchive.Entries.First(), EntryStreamMode.MemoryStream))
                    MotionDatabaseFT.Load(entryStream);

                var SkeletonEntryAC = BoneDatabaseAC.Skeletons[0];
                var SkeletonEntryFT = BoneDatabaseFT.Skeletons[0];
                
                foreach (var filePath in files)
                {
                    var motionset = new MotionSet();
                    if (!File.Exists(ac_path + "\\rob\\" + Path.GetFileName(filePath)))
                        if (!filePath.Contains("db"))
                    {
                        Logs.WriteLine("Processing " + Path.GetFileNameWithoutExtension(filePath));

                        string farcpath = filePath;
                            
                        using (var farcArchive = BinaryFile.Load<FarcArchive>(farcpath))
                        using (var entryStream = farcArchive.Open(farcArchive.Entries.First(), EntryStreamMode.MemoryStream))
                            motionset.Load(entryStream, SkeletonEntryFT, MotionDatabaseFT);
                        {
                            motionset.Save("temp", SkeletonEntryAC, MotionDatabaseAC);
                            //motionset.Dispose();
                            
                            
                            FarcArchive newfarc = new FarcArchive();
                            newfarc.Add(Path.GetFileNameWithoutExtension(filePath) + ".bin", "temp");
                            newfarc.Save(ac_path + "\\rob\\" + Path.GetFileName(filePath));
                        }
                    }
                }
            }
        }

        private void afterConvert()
        {
            string ac_path = "";
            string ft_path = "";
            string fs_path = "";
            string ct_path = "";
            bool ask_user = false;
            bool skip_objset = false;
            bool skip_robs = false;
            bool skip_ui = false;
            bool skip_filecopy = false;
            bool skip_2d = false;
            bool skip_db = false;
            bool alt_db = false;
            bool skip_movie = false;
            if (File.Exists("config.txt"))
            {
                StreamReader sr = new StreamReader("config.txt");
                while (sr.Peek() > -1)
                {
                    var line = sr.ReadLine();
                    var lines = line.Split('=');
                    if (lines[0] == "AC")
                    {
                        ac_path = lines[1];
                    }
                    if (lines[0] == "FT")
                    {
                        ft_path = lines[1];
                    }
                    if (lines[0] == "FS")
                    {
                        fs_path = lines[1];
                    }
                    if (lines[0] == "CT")
                    {
                        ct_path = lines[1];
                    }
                    if (lines[0] == "I_HAVE_CHANGED_THIS")
                    {
                        if (lines[1] == "TRUE")
                            ask_user = true;
                    }

                    if (lines[0] == "SKIP_OBJSET")
                    {
                        if (lines[1] == "TRUE")
                            skip_objset = true;
                    }

                    if (lines[0] == "SKIP_ROBS")
                    {
                        if (lines[1] == "TRUE")
                            skip_robs = true;
                    }

                    if (lines[0] == "UI_MOD")
                    {
                        if (lines[1] == "TRUE")
                            skip_ui = true;
                    }

                    if (lines[0] == "SKIP_2D")
                    {
                        if (lines[1] == "TRUE")
                            skip_2d = true;
                    }

                    if (lines[0] == "SKIP_FILECOPY")
                    {
                        if (lines[1] == "TRUE")
                            skip_filecopy = true;
                    }

                    if (lines[0] == "SKIP_DB")
                    {
                        if (lines[1] == "TRUE")
                            skip_filecopy = true;
                    }

                    if (lines[0] == "SKIP_MOVIE")
                    {
                        if (lines[1] == "TRUE")
                            skip_movie = true;
                    }

                    if (lines[0] == "ALT_DB")
                    {
                        if (lines[1] == "TRUE")
                            alt_db = true;
                    }
                }
                sr.Close();
            }

            if (ask_user)
            {
                if (!skip_2d)
                {
                    Logs.WriteLine("2D Start");
                    //   SetTitle("2d");
                    Tools.Repack(ft_path + @"\2d\", ac_path + @"\2d\");
                    if (fs_path != "")
                    {
                        Logs.WriteLine("2D FS Start");
                        Tools.Repack(fs_path + @"\2d\", ac_path + @"\2d\");
                    }
                    if (ct_path != "")
                    {
                        Logs.WriteLine("2D CT Start");
                        Tools.Repack(ct_path + @"\2d\", ac_path + @"\2d\");
                    }
                    Logs.WriteLine("2D Finish");
                }
                else Logs.WriteLine("2D Skipped");

                if (!skip_robs)
                {
                    Logs.WriteLine("Rob Start");
                    //   SetTitle("rob");
                    Tools.MassPack(ac_path + @"\rob_bins\", ac_path + @"\rob");
                    Logs.WriteLine("Rob Finish");
                }
                Logs.WriteLine("Rob Skipped");

                if (!skip_filecopy)
                {
                    Logs.WriteLine("FileCopy stage_param Start");
                    //   SetTitle("stage_param");
                    Tools.CopyFiles(ft_path + @"\stage_param\", ac_path + @"\stage_param\");
                    if (fs_path != "")
                    {
                        Logs.WriteLine("FileCopy stage_param FS Start");
                        Tools.CopyFiles(fs_path + @"\stage_param\", ac_path + @"\stage_param\");
                    }
                    if (ct_path != "")
                    {
                        Logs.WriteLine("FileCopy stage_param CT Start");
                        Tools.CopyFiles(ct_path + @"\stage_param\", ac_path + @"\stage_param\");
                    }
                    //   SetTitle("light_param");
                    Logs.WriteLine("FileCopy light_param Start");
                    Tools.CopyFiles(ft_path + @"\light_param\", ac_path + @"\light_param\");
                    if (fs_path != "")
                    {
                        Logs.WriteLine("FileCopy light_param FS Start");
                        Tools.CopyFiles(fs_path + @"\light_param\", ac_path + @"\light_param\");
                    }
                    if (ct_path != "")
                    {
                        Logs.WriteLine("FileCopy light_param CT Start");
                        Tools.CopyFiles(ct_path + @"\light_param\", ac_path + @"\light_param\");
                    }
                    //   SetTitle("ibl");
                    Logs.WriteLine("FileCopy ubl Start");
                    Tools.CopyFiles(ft_path + @"\ibl\", ac_path + @"\ibl\");
                    if (fs_path != "")
                    {
                        Logs.WriteLine("FileCopy ibl FS Start");
                        Tools.CopyFiles(fs_path + @"\ibl\", ac_path + @"\ibl\");
                    }
                    if (ct_path != "")
                    {
                        Logs.WriteLine("FileCopy ibl CT Start");
                        Tools.CopyFiles(ct_path + @"\ibl\", ac_path + @"\ibl\");
                    }
                    //   SetTitle("songs");
                    Logs.WriteLine("FileCopy song Start");
                    Tools.CopyFiles(ft_path + @"\sound\song\", ac_path + @"\sound\song\");
                    if (fs_path != "")
                    {
                        Logs.WriteLine("FileCopy song FS Start");
                        Tools.CopyFiles(fs_path + @"\sound\song\", ac_path + @"\sound\song\");
                    }
                    if (ct_path != "")
                    {
                        Logs.WriteLine("FileCopy song CT Start");
                        Tools.CopyFiles(ct_path + @"\sound\song\", ac_path + @"\sound\song\");
                    }
                    Logs.WriteLine("FileCopy Finish");
                }
                else Logs.WriteLine("FileCopy Skipped");

                if (!skip_movie)
                {
                    Logs.WriteLine("Movie Copy Start");
                    Tools.CopyFiles(ft_path + @"\movie\", ac_path + @"\movie\");
                    if (fs_path != "")
                    {
                        Logs.WriteLine("Movie Copy FS Start");
                        Tools.CopyFiles(fs_path + @"\movie\", ac_path + @"\movie\");
                    }
                    if (ct_path != "")
                    {
                        Logs.WriteLine("Movie Copy CT Start");
                        Tools.CopyFiles(ct_path + @"\movie\", ac_path + @"\movie\");
                    }
                    Logs.WriteLine("Movie Copy Finish");
                }
                else Logs.WriteLine("Movie Copy Skipped");
                //SetTitle("songs");
                //Tools.CopyFiles(@"dsc\", ac_path + @"\script\");
                if (!skip_robs)
                {
                    Logs.WriteLine("Rob Bins Start");
                    //   SetTitle("rob_copy_bins");
                    Tools.CopyFiles(ft_path + @"\rob\", ac_path + @"\rob\");
                    if (fs_path != "")
                    {
                        Logs.WriteLine("Rob Bins FS Start");
                        Tools.CopyFiles(fs_path + @"\rob\", ac_path + @"\rob\");
                    }
                    if (ct_path != "")
                    {
                        Logs.WriteLine("Rob Bins CT Start");
                        Tools.CopyFiles(ct_path + @"\rob\", ac_path + @"\rob\");
                    }
                    Logs.WriteLine("Rob Bins Finish");
                }
                else Logs.WriteLine("Rob Bins Skipped");

                if (!skip_filecopy)
                {
                    Logs.WriteLine("2d Bins Start");
                    //   SetTitle("2d_copy_bins");
                    Tools.CopyFiles(ft_path + @"\2d\", ac_path + @"\2d\");
                    if (fs_path != "")
                    {
                        Logs.WriteLine("2d Bins FS Start");
                        Tools.CopyFiles(fs_path + @"\2d\", ac_path + @"\2d\");
                    }
                    if (ct_path != "")
                    {
                        Logs.WriteLine("2d Bins CT Start");
                        Tools.CopyFiles(ct_path + @"\2d\", ac_path + @"\2d\");
                    }
                    Logs.WriteLine("2d Bins Finish");
                }
                else Logs.WriteLine("2d Bins Skipped");

                //   SetTitle("dbs");
                if (skip_ui == false)
                {
                    Logs.WriteLine("SPRAET Copy");
                    File.Copy(@"ported_db\aet_db.bin", ac_path + @"\2d\aet_db.bin", true);
                    File.Copy(@"ported_db\spr_db.bin", ac_path + @"\2d\spr_db.bin", true);
                }
                else Logs.WriteLine("SPRAET Skipped");

                if (!skip_db)
                {
                    Logs.WriteLine("Copying Databases...");
                    File.Copy(@"ported_db\auth_3d_db.bin", ac_path + @"\auth_3d\auth_3d_db.bin", true);
                    
                    File.Copy(@"ported_db\stage_data.bin", ac_path + @"\stage_data.bin", true);
                    File.Copy(@"ported_db\tex_db.bin", ac_path + @"\objset\tex_db.bin", true);
                    File.Copy(@"ported_db\obj_db.bin", ac_path + @"\objset\obj_db.bin", true);
                    File.Copy(@"ported_db\pv_field.txt", ac_path + @"\pv_field.txt", true);
                    File.Copy(@"ported_db\pv_db.txt", ac_path + @"\pv_db.txt", true);
                    File.Copy(@"ported_db\gm_pv_list_tbl.farc", ac_path + @"\gm_pv_list_tbl.farc", true);
                

                if (alt_db)
                {
                    File.Copy(@"alt_db\auth_3d_db.bin", ac_path + @"\auth_3d\auth_3d_db.bin", true);
                    File.Copy(@"alt_db\pv_db.txt", ac_path + @"\pv_db.txt", true);
                    File.Copy(@"alt_db\pv_field.txt", ac_path + @"\pv_field.txt", true);
                    File.Copy(@"alt_db\gm_pv_list_tbl.farc", ac_path + @"\gm_pv_list_tbl.farc", true);
                    File.Copy(@"alt_db\stage_data.bin", ac_path + @"\stage_data.bin", true);
                }
                else Logs.WriteLine("WARNING : USER DOES NOT SELECT ALT_DB! USING FRANKENTONE DBS...");

                }

                //   SetTitle("removing temporary folders");
                Logs.WriteLine("removing temporary folder");
                Directory.Delete(ft_path + @"\rob_bins", true);
                if (fs_path != "")
                {
                    Logs.WriteLine("removing temporary folder FS");
                    Directory.Delete(fs_path + @"\rob_bins", true);
                }
                if (ct_path != "")
                {
                    Logs.WriteLine("removing temporary folder CT");
                    Directory.Delete(ct_path + @"\rob_bins", true);
                }
                Logs.WriteLine("removing temporary folder AC");
                Directory.Delete(ac_path + @"\rob_bins", true);
                Directory.Delete(ac_path + @"\rob_farc", true);
                Logs.WriteLine("DONE");
                //   SetTitle("DONE");
            }
        }

        private void doConvertDsc()
        {
            //   SetTitle("dsc");
            string ac_path = "";
            string ft_path = "";
            string fs_path = "";
            string ct_path = "";
            bool ask_user = false;
            if (File.Exists("config.txt"))
            {
                StreamReader sr = new StreamReader("config.txt");
                while (sr.Peek() > -1)
                {
                    var line = sr.ReadLine();
                    var lines = line.Split('=');
                    if (lines[0] == "AC")
                    {
                        ac_path = lines[1];
                    }
                    if (lines[0] == "FT")
                    {
                        ft_path = lines[1];
                    }
                    if (lines[0] == "FS")
                    {
                        fs_path = lines[1];
                    }
                    if (lines[0] == "CT")
                    {
                        ct_path = lines[1];
                    }
                    if (lines[0] == "I_HAVE_CHANGED_THIS")
                    {
                        if (lines[1] == "TRUE")
                            ask_user = true;
                    }
                    if (lines[0] == "DSC_THREAD_NUM")
                    {
                        DivaScriptConverter.maxWorker = int.Parse(lines[1]);
                    }
                }
                sr.Close();
            }

            if (ask_user)
            {
                DivaScriptConverter.ConvertDsc(ft_path + "\\script\\", ac_path + "\\script\\", false, ft_path + "\\pv_expression\\");
                if (fs_path != "") DivaScriptConverter.ConvertDsc(fs_path + "\\script\\", ac_path + "\\script\\", false, fs_path + "\\pv_expression\\");
                if (ct_path != "") DivaScriptConverter.ConvertDsc(ct_path + "\\script\\", ac_path + "\\script\\", false, ct_path + "\\pv_expression\\");
            }
        }

        private void doUiStuff()
        {
            do2DConvert();
            doRobConvert();
        }

        private void fixObjSet()
        {
            string ac_path = "";
            string ft_path = "";
            bool ask_user = false;
            bool skip_objset = false;
            bool skip_robs = false;
            bool skip_ui = false;
            bool skip_filecopy = false;
            bool skip_2d = false;
            bool skip_db = false;
            bool alt_db = false;
            bool skip_movie = false;
            if (File.Exists("config.txt"))
            {
                StreamReader sr = new StreamReader("config.txt");
                while (sr.Peek() > -1)
                {
                    var line = sr.ReadLine();
                    var lines = line.Split('=');
                    if (lines[0] == "AC")
                    {
                        ac_path = lines[1];
                    }
                    if (lines[0] == "FT")
                    {
                        ft_path = lines[1];
                    }
                    if (lines[0] == "I_HAVE_CHANGED_THIS")
                    {
                        if (lines[1] == "TRUE")
                            ask_user = true;
                    }

                    if (lines[0] == "SKIP_OBJSET")
                    {
                        if (lines[1] == "TRUE")
                            skip_objset = true;
                    }

                    if (lines[0] == "SKIP_ROBS")
                    {
                        if (lines[1] == "TRUE")
                            skip_robs = true;
                    }

                    if (lines[0] == "UI_MOD")
                    {
                        if (lines[1] == "TRUE")
                            skip_ui = true;
                    }

                    if (lines[0] == "SKIP_2D")
                    {
                        if (lines[1] == "TRUE")
                            skip_2d = true;
                    }

                    if (lines[0] == "SKIP_FILECOPY")
                    {
                        if (lines[1] == "TRUE")
                            skip_filecopy = true;
                    }

                    if (lines[0] == "SKIP_DB")
                    {
                        if (lines[1] == "TRUE")
                            skip_filecopy = true;
                    }

                    if (lines[0] == "SKIP_MOVIE")
                    {
                        if (lines[1] == "TRUE")
                            skip_movie = true;
                    }

                    if (lines[0] == "ALT_DB")
                    {
                        if (lines[1] == "TRUE")
                            alt_db = true;
                    }
                }
                sr.Close();
            }

            string path = ft_path;
            string path2 = ac_path;

            List<string> files = new List<string>();

            string[] filess = System.IO.Directory.GetFiles(path + @"\objset\", "*hrc.farc", SearchOption.AllDirectories);

            foreach (var i in filess)
            {
                files.Add(i);
            }

            filess = System.IO.Directory.GetFiles(path + @"\objset\", "effchrpv*.farc", SearchOption.AllDirectories);

            foreach (var i in filess)
            {
                files.Add(i);
            }

            foreach (var filePath in files)
            {
                try
                {
                    //if ((!File.Exists(ac_path + @"\2d\" + Path.GetFileNameWithoutExtension(filePath) + ".farc")) && (Path.GetFileNameWithoutExtension(filePath) != "spr_sel_pv"))
                    {
                        Logs.WriteLine("Processing " + Path.GetFileNameWithoutExtension(filePath));

                        var models = new Model();

                        string obj = "";
                        string tex = "";

                        using (var farcArchive = BinaryFile.Load<FarcArchive>(filePath))
                        using (var entryStream = farcArchive.Open(farcArchive.Entries.Where(c => c.Contains("obj")).First(), EntryStreamMode.MemoryStream))
                        {
                            models.Load(entryStream);
                            obj = farcArchive.Entries.Where(c => c.Contains("obj")).First();
                        }

                        using (var farcArchive = BinaryFile.Load<FarcArchive>(filePath))
                        using (var entryStream = farcArchive.Open(farcArchive.Entries.Where(c => c.Contains("tex")).First(), EntryStreamMode.MemoryStream))
                        {
                            tex = farcArchive.Entries.Where(c => c.Contains("tex")).First();
                        }

                        foreach (var mesh in models.Meshes)
                        {
                            foreach (var mat in mesh.Materials)
                            {
                                mat.Shader = "ITEM";
                            }
                        }

                        models.Save("temp");
                        var newfarc = new FarcArchive();
                        newfarc.Add(obj, "temp");

                        using (var farcArchive = BinaryFile.Load<FarcArchive>(filePath))
                        using (var entryStream = farcArchive.Open(farcArchive.Entries.Where(c => c.Contains("tex")).First(), EntryStreamMode.MemoryStream))
                            newfarc.Add(tex, entryStream, false);

                        newfarc.Save(ac_path + "\\" + "objset" + "\\" + Path.GetFileName(filePath));

                    }
                } catch (Exception e)
                {
                    Logs.WriteLine(e.ToString());
                    //Console.WriteLine(e.ToString());
                }
            }

        }

        public void doConvert()
        {
            var val = ConvertToAc();
            if (val)
            {
                doUiStuff();
                doConvertDsc();
                afterConvert();
                fixObjSet();
            }
            //if (File.Exists("temp"))
            //    File.Delete("temp");
        }

        public async void doConvert2()
        {
            var val = await System.Threading.Tasks.Task.Run(() => ConvertToAc());
            if (val == true)
            {
                doUiStuff();
                await System.Threading.Tasks.Task.Run(() => doConvertDsc());
                await System.Threading.Tasks.Task.Run(() => afterConvert());
            }
        }

    }
}
