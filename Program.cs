using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using MikuMikuModel.FarcPack;
using MikuMikuModel.Logs;

namespace ft_module_parser
{
    class Program
    {
        public static void Main(string[] args)
        {
            Logs.Initialize();

            var ftdx = new pdaconversion.ftdx.mass_convert();
            ftdx.doConvert();
        }
    }
}
