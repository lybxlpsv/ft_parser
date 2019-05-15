using MikuMikuLibrary.Archives;
using MikuMikuLibrary.Archives.Farc;
using MikuMikuLibrary.IO;
using MikuMikuLibrary.Models;
using MikuMikuLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikuMikuLibrary.Databases;
using System.IO;
using MikuMikuLibrary.Models.Processing;
using MikuMikuModel.Logs;

namespace ft_module_parser.pdaconversion.divax
{
    class objset
    {
        ushort FindUnusedID(StageDatabase staged)
        {
            ushort id = 0;
            
            var stage = staged.Stages.Where(c => c.Id == 0).FirstOrDefault();
            while (stage != null)
            {
                id++;
                stage = staged.Stages.Where(c => c.Id == id).FirstOrDefault();
                
            }
            Logs.WriteLine("new stage at id = " + id);
            return id;
        }

        ushort FindUnusedID(ObjectDatabase staged)
        {
            ushort id = 0;
            
            var stage = staged.Objects.Where(c => c.Id == 0).FirstOrDefault();
            while (stage != null)
            {
                id++;
                stage = staged.Objects.Where(c => c.Id == id).FirstOrDefault();
                
            }
            
            //id = (ushort)(staged.Objects.Max(c => c.Id) + 1);
            Logs.WriteLine("new object at id = " + id);
            return id;
        }

        public models Stripify(string filePath)
        {
            var stgpv = new Model();
            var textures = new MikuMikuLibrary.Textures.TextureSet();
            var texdb = new TextureDatabase();

            using (var farcArchive = BinaryFile.Load<FarcArchive>(filePath))
            {
                using (var entryStream = farcArchive.Open(farcArchive.Entries.Where(c => c.Contains("txi")).First(), EntryStreamMode.MemoryStream))
                    texdb.Load(entryStream);
                using (var entryStream = farcArchive.Open(farcArchive.Entries.Where(c => c.Contains("txd")).First(), EntryStreamMode.MemoryStream))
                    textures.Load(entryStream);
                using (var entryStream = farcArchive.Open(farcArchive.Entries.First(), EntryStreamMode.MemoryStream))
                    stgpv.Load(entryStream, textures, texdb);
            }
            
            foreach (var meshes in stgpv.Meshes)
            {
                foreach (var submeshes in meshes.SubMeshes)
                {
                    foreach (var indexTable in submeshes.IndexTables)
                    {
                        ushort[] triangleStrip = Stripifier.Stripify(indexTable.Indices);
                        if (triangleStrip != null)
                        {
                            indexTable.PrimitiveType = PrimitiveType.TriangleStrip;
                            indexTable.Indices = triangleStrip;
                        }
                    }
                }
            }

            var le_model = new models();
            le_model.model = stgpv;
            le_model.fileName = filePath;
            Logs.WriteLine("Stripified " + Path.GetFileName(filePath));
            return le_model;
        }

        public void GenerateObjSet(string filePath, Model stgpv, ObjectDatabase objdb, TextureDatabase texturedb, StageDatabase staged, string acpath, divamodgen divamods, bool doProcess = true, bool debug = false)
        {
            /*
            var stgpv = new Model();
            var textures = new MikuMikuLibrary.Textures.TextureSet();
            var texdb = new TextureDatabase();

            using (var farcArchive = BinaryFile.Load<FarcArchive>(filePath))
            using (var entryStream = farcArchive.Open(farcArchive.Entries.Where(c => c.Contains("txi")).First(), EntryStreamMode.MemoryStream))
                texdb.Load(entryStream);

            if (debug)
            {
                string farcpath = acpath + "\\rom\\objset\\" + Path.GetFileNameWithoutExtension(filePath).Replace("stgpv0", "stgpv8") + ".farc";

                using (var farcArchive = BinaryFile.Load<FarcArchive>(farcpath))
                using (var entryStream = farcArchive.Open(farcArchive.Entries.Where(c => c.Contains("tex")).First(), EntryStreamMode.MemoryStream))
                    textures.Load(entryStream);

            }
            else
            {
                using (var farcArchive = BinaryFile.Load<FarcArchive>(filePath))
                using (var entryStream = farcArchive.Open(farcArchive.Entries.Where(c => c.Contains("txd")).First(), EntryStreamMode.MemoryStream))
                    textures.Load(entryStream);

                using (var farcArchive = BinaryFile.Load<FarcArchive>(filePath))
                using (var entryStream = farcArchive.Open(farcArchive.Entries.First(), EntryStreamMode.MemoryStream))
                    stgpv.Load(entryStream, textures, texdb);
            }

            */

            if (Path.GetFileNameWithoutExtension(filePath).Contains("effpv"))
            {
                filePath = Path.GetDirectoryName(filePath) + "\\" + Path.GetFileNameWithoutExtension(filePath).Replace("effpv", "stgpv") + "hrc2.farc";
            }

            var texdb = new TextureDatabase();

            //using (var farcArchive = BinaryFile.Load<FarcArchive>(filePath))
            //using (var entryStream = farcArchive.Open(farcArchive.Entries.Where(c => c.Contains("txi")).First(), EntryStreamMode.MemoryStream))
            //   texdb.Load(entryStream);

            var objentry = new ObjectEntry();
            objentry.ArchiveFileName = Path.GetFileName(filePath).Replace("stgpv0", "stgpv8");
            objentry.FileName = Path.GetFileNameWithoutExtension(filePath).Replace("stgpv0", "stgpv8") + "_obj.bin";
            objentry.Id = FindUnusedID(objdb);
            objentry.Name = Path.GetFileNameWithoutExtension(filePath).Replace("stgpv0", "stgpv8").ToUpper();
            objentry.TextureFileName = Path.GetFileNameWithoutExtension(filePath).Replace("stgpv0", "stgpv8") + "_tex.bin";
            
            int ground_id = -1;
                        
            ushort counter = 1;
            foreach (var meshes in stgpv.Meshes)
            {
                meshes.SubMeshes.RemoveAll(x => x.Vertices == null || x.Vertices.Length == 0);
                meshes.Name = meshes.Name.Replace("STGPV0", "STGPV8");
                meshes.Name = meshes.Name.Replace("EFFPV", "STGPV");

                if (Path.GetFileName(filePath).Contains("hrc"))
                {
                    int pvid = int.Parse(Path.GetFileName(filePath).Substring(5, 3));

                    if (pvid < 200)
                    {
                        pvid = pvid + 800;
                    }

                    {
                        var check2 = divamods.Divamods.Where(c => c.pvid == pvid).FirstOrDefault();
                        if (check2 == null)
                        {
                            divamods.Divamods.Add(new pdaconversion.divamods(pvid));
                            Logs.WriteLine("objset: Created new PV at id " + pvid);
                            check2 = divamods.Divamods.Where(c => c.pvid == pvid).First();
                        }
                        check2.item_pv.Add(meshes.Name);
                        Logs.WriteLine("objset: Added item_pv for PV at id " + pvid + "," + meshes.Name);
                    }

                    if (pvid >= 800)
                    {
                        var check2 = divamods.Divamods.Where(c => c.pvid == (pvid - 100)).FirstOrDefault();
                        if (check2 == null)
                        {
                            divamods.Divamods.Add(new pdaconversion.divamods(pvid - 100));
                            Logs.WriteLine("objset: Created new PV at id " + (pvid - 100));
                            check2 = divamods.Divamods.Where(c => c.pvid == (pvid - 100)).First();
                        }
                        check2.item_pv.Add(meshes.Name);
                        Logs.WriteLine("objset: Added item_pv for PV at id " + (pvid - 100) + "," + meshes.Name);
                    }
                }

                var meshentry = new MeshEntry();
                meshes.Id = counter;
                meshentry.Id = (ushort)meshes.Id;
                meshentry.Name = meshes.Name;
                
                if (meshes.Name.Contains("GND"))
                    ground_id = meshes.Id;

                objentry.Meshes.Add(meshentry);

                if (doProcess)
                {

                    if (!debug)
                    {
                        /*
                        foreach (var submeshes in meshes.SubMeshes)
                        {
                            foreach (var indexTable in submeshes.IndexTables)
                            {
                                ushort[] triangleStrip = Stripifier.Stripify(indexTable.Indices);
                                if (triangleStrip != null)
                                {
                                    indexTable.PrimitiveType = PrimitiveType.TriangleStrip;
                                    indexTable.Indices = triangleStrip;
                                }
                            }
                        }
                        */
                    }
                    
                    //foreach (var textures in M)

                    foreach (var material in meshes.Materials)
                    {
                        if ((Path.GetFileName(filePath).Contains("hrc2")) || (Path.GetFileName(filePath).Contains("hrc")))
                        {
                            material.Shader = "ITEM";
                            material.IsAlphaEnabled = false;
                            //material.Field00 = 0x00000001;
                            //material.Field02 = 0x00000A80;

                            //material.Diffuse.Field00 = 0x00000000;
                            //material.Diffuse.Field01 = 0x016400E1;
                            //material.Diffuse.Field02 = 0x000000F1;

                            MikuMikuLibrary.Misc.Color asdf = new MikuMikuLibrary.Misc.Color(0, 0, 0, 0);

                            material.SpecularColor = asdf;

                            //material.AmbientColor = asdf;
                            //material.EmissionColor = asdf;
                            //material.SpecularColor = asdf;

                            //material.Shininess = 1;

                            //material.Diffuse.Field00 = 0x00000000;
                            //material.Diffuse.Field01 = 0x002400E0;
                            //material.Diffuse.Field02 = 0x000000F1;

                            if (material.Ambient.TextureId == 1509989155)
                                material.Ambient.TextureId = -1;
                            if (material.Normal.TextureId == 1509989155)
                                material.Normal.TextureId = -1;
                            if (material.Specular.TextureId == 1509989155)
                            {
                                material.Specular.TextureId = -1;
                                material.SpecularColor = asdf;
                            }
                            //else { material.Field00 = 0x0000002D; }

                            /*

                            // this blacken the screen on STAGE SHADER
                            material.Specular.Field01 = 0x002418C3;
                            material.Specular.Field02 = 0x000000F3;
                            material.Specular.Field05 = 1;
                            material.Specular.Field06 = 1;
                            material.Specular.Field11 = 1;
                            material.Specular.Field16 = 1;
                            material.Specular.Field21 = 1;
                            
                            
                            material.Specular.TextureId = -1;
                            material.Specular.Field01 = 0x00000000;
                            material.Specular.Field02 = 0x000000F0;
                            material.Specular.Field05 = 1;
                            material.Specular.Field06 = 0;
                            material.Specular.Field11 = 0;
                            material.Specular.Field16 = 0;
                            material.Specular.Field21 = 0;

                            material.Reflection.TextureId = -1;
                            material.Reflection.Field01 = 0x00000000;
                            material.Reflection.Field02 = 0x000000F0;
                            material.Reflection.Field05 = 1;
                            material.Reflection.Field06 = 0;
                            material.Reflection.Field11 = 0;
                            material.Reflection.Field16 = 0;
                            material.Reflection.Field21 = 0;
                            */

                            if (material.Reflection.TextureId == 1509989155)
                                material.Reflection.TextureId = -1;
                            if (material.ToonCurve.TextureId == 1509989155)
                                material.ToonCurve.TextureId = -1;
                            if (material.SpecularPower.TextureId == 1509989155)
                                material.SpecularPower.TextureId = -1;
                            if (material.Texture08.TextureId == 1509989155)
                                material.Texture08.TextureId = -1;
                        }
                        else
                        {
                            material.Shader = "BLINN";
                            //material.Field00 = 0x0000000D;
                            //material.Field02 = 0x00000A80;

                            MikuMikuLibrary.Misc.Color asdf = new MikuMikuLibrary.Misc.Color(0, 0, 0, 0);

                            //material.AmbientColor = asdf;
                            //material.EmissionColor = asdf;
                            //material.Shininess = 1;
                            //material.Diffuse.Field00 = 0x00000230;
                            //material.Diffuse.Field02 = 0x002418C3;

                            if (material.Ambient.TextureId == 1509989155)
                                material.Ambient.TextureId = -1;
                            if (material.Normal.TextureId == 1509989155)
                                material.Normal.TextureId = -1;
                            if (material.Specular.TextureId == 1509989155)
                            {
                                material.Specular.TextureId = -1;
                                material.SpecularColor = asdf;
                            }
                            //else { material.Field00 = 0x0000002D; }

                            /*

                            // this blacken the screen on STAGE SHADER
                            material.Specular.Field01 = 0x002418C3;
                            material.Specular.Field02 = 0x000000F3;
                            material.Specular.Field05 = 1;
                            material.Specular.Field06 = 1;
                            material.Specular.Field11 = 1;
                            material.Specular.Field16 = 1;
                            material.Specular.Field21 = 1;
                            
                            */

                            /*
                            material.Specular.TextureId = -1;
                            material.Specular.Field01 = 0x00000000;
                            material.Specular.Field02 = 0x000000F0;
                            material.Specular.Field05 = 5;
                            material.Specular.Field06 = 0;
                            material.Specular.Field11 = 0;
                            material.Specular.Field16 = 0;
                            material.Specular.Field21 = 0;

                            
                            material.Normal.Field01 = 0x00000000;
                            material.Normal.Field02 = 0x000000F0;
                            material.Normal.Field05 = 5;
                            material.Normal.Field06 = 0;
                            material.Normal.Field11 = 0;
                            material.Normal.Field16 = 0;
                            material.Normal.Field21 = 0;
                            

                            material.ToonCurve.TextureId = -1;
                            material.ToonCurve.Field01 = 0x00000000;
                            material.ToonCurve.Field02 = 0x000000F0;
                            material.ToonCurve.Field05 = 5;
                            material.ToonCurve.Field06 = 0;
                            material.ToonCurve.Field11 = 0;
                            material.ToonCurve.Field16 = 0;
                            material.ToonCurve.Field21 = 0;
                            */

                            if (material.Reflection.TextureId == 1509989155)
                                material.Reflection.TextureId = -1;
                            if (material.ToonCurve.TextureId == 1509989155)
                                material.ToonCurve.TextureId = -1;
                            if (material.SpecularPower.TextureId == 1509989155)
                                material.SpecularPower.TextureId = -1;
                            if (material.Texture08.TextureId == 1509989155)
                                material.Texture08.TextureId = -1;
                        }
                    }
                }
                counter++;
            }
            
            stgpv.TextureSet.Format = BinaryFormat.DT;
            stgpv.Format = BinaryFormat.DT;
            
            

            if (doProcess)
                stgpv.Save("temp\\" + Path.GetFileNameWithoutExtension(filePath) + "_obj.bin", null, texturedb, null);
            objdb.Objects.Add(objentry);

            if (Path.GetFileNameWithoutExtension(filePath).Count() == 8)
            {
                StageEntry stage = new StageEntry();
                stage.Id = FindUnusedID(staged);
                stage.Name = Path.GetFileNameWithoutExtension(filePath).Replace("stgpv0", "stgpv8").ToUpper();
                stage.ObjectGroundId = (short)ground_id;
                stage.ObjectGroundIdFlag = (short)objentry.Id;
                stage.ObjectId3 = -1;
                stage.ObjectIdFlag3 = -1;
                stage.ObjectId5 = -1;
                stage.ObjectIdFlag5 = -1;
                stage.ObjectId7 = -1;
                stage.ObjectIdFlag7 = -1;
                stage.RingRectangleX = -8;
                stage.RingRectangleY = -8;
                stage.RingRectangleWidth = 16;
                stage.RingRectangleHeight = 16;
                stage.RingRingHeight = 0;
                stage.RingOutHeight = -1000;
                stage.Field00 = 1;
                stage.Field11 = -1;
                stage.Field02 = -1;

                stage.LensFlareScaleX = -1;
                stage.LensFlareScaleY = -1;
                stage.LensFlareScaleZ = -1;

                stage.ObjectSkyId = -1;
                stage.ObjectSkyIdFlag = -1;
                stage.ObjectReflectId = -1;
                stage.ObjectReflectIdFlag = -1;

                stage.CollisionFilePath = @"rom/STGTST_COLI.000.bin";
                
                stage.ObjectId1 = (short)objentry.Id;
                stage.StageEffect1 = StageEntry.StageEffect.Empty;
                stage.Auth3dName = "EFF" + stage.Name;
                staged.Stages.Add(stage);
            }

            if (doProcess)
            {
                var newfarc = new FarcArchive();
                newfarc.Add(Path.GetFileNameWithoutExtension(filePath).Replace("stgpv0", "stgpv8") + "_obj.bin", "temp\\" + Path.GetFileNameWithoutExtension(filePath) + "_obj.bin");
                newfarc.Add(Path.GetFileNameWithoutExtension(filePath).Replace("stgpv0", "stgpv8") + "_tex.bin", "temp\\" + Path.GetFileNameWithoutExtension(filePath) + "_tex.bin");
                newfarc.Alignment = 16;
                newfarc.IsCompressed = false;
                newfarc.Save(acpath + "\\rom\\objset\\" + Path.GetFileNameWithoutExtension(filePath).Replace("stgpv0", "stgpv8") + ".farc");
            }
            
            
        }
    }
}
