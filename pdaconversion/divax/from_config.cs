using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ft_module_parser.pdaconversion.divax
{
    class from_config
    {
        public static void doConvert()
        {
            var mot_db = "";
            var a3d_db = "";
            var stage_data = "";
            var obj_db = "";
            var tex_db = "";
            var ac = "";
            var x = "";
            var adv_path = false;
            var ichange = false;

            if (!File.Exists("config_x.txt"))
            {
                Console.WriteLine("config_x.txt not found");
                Console.ReadKey();
            }

            using (StreamReader modtxt = new StreamReader("config_x.txt"))
            {
                while (modtxt.Peek() >= 0)
                {
                    var line = modtxt.ReadLine();
                    int index = line.IndexOf('=');
                    if (!(line.StartsWith("#")) && (index > 0))
                    {
                        string first = line.Substring(0, index);
                        string second = line.Substring(index + 1);

                        if (first == "I_HAVE_CHANGED_THIS")
                            if (second == "TRUE") ichange = true;

                        if (first == "AC")
                            ac = second;

                        if (first == "X")
                            x = second + "\\";

                        if (first == "ADV_PATH")
                            if (second == "TRUE") adv_path = true;

                        if (first == "OBJ_DB")
                            obj_db = second;

                        if (first == "TEX_DB")
                            tex_db = second;

                        if (first == "STAGE_DATA")
                            stage_data = second;

                        if (first == "A3D_DB")
                            a3d_db = second;

                        if (first == "MOT_DB")
                            mot_db = second;
                    }
                }
            }

            if (ichange)
            {
                if (adv_path)
                {
                    pdaconversion.divax.mass_convert divax = new pdaconversion.divax.mass_convert();
                    divax.Convert(x, obj_db, tex_db, stage_data, a3d_db, mot_db, ac);
                }
                else
                {
                    pdaconversion.divax.mass_convert divax = new pdaconversion.divax.mass_convert();
                    divax.Convert(x, ac + "\\objset\\obj_db.bin", "\\objset\\tex_db.bin", "\\stage_data.bin", "\\auth_3d\\auth_3d_db.bin", "\\rob\\mot_db.farc", ac);
                }
            }
            else
            {
                Console.WriteLine("You did not edit config.ini");
                Console.ReadKey();
            }
        }
    }
}
