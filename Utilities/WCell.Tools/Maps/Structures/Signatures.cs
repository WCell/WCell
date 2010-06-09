using System;
using System.IO;

namespace WCell.Tools.Maps.Structures
{


    static class Signatures
    {
        public static uint MAIN = ToBin("MAIN");
        public static uint MAOF = ToBin("MAOF");
        public static uint MARE = ToBin("MARE");
        public static uint MCAL = ToBin("MCAL");
        public static uint MCIN = ToBin("MCIN");
        public static uint MCLQ = ToBin("MCLQ");
        public static uint MCLY = ToBin("MCLY");
        public static uint MCNK = ToBin("MCNK");

        public static uint MCNR = ToBin("MCNR");
        public static uint MCRF = ToBin("MCRF");
        public static uint MCSE = ToBin("MCSE");
        public static uint MCSH = ToBin("MCSH");
        public static uint MCVT = ToBin("MCVT");
        public static uint MCCV = ToBin("MCCV");
        public static uint MDDF = ToBin("MDDF");
        public static uint MFOG = ToBin("MFOG");
        public static uint MH2O = ToBin("MH2O");
        public static uint MHDR = ToBin("MHDR");
        public static uint MLIQ = ToBin("MLIQ");
        public static uint MMDX = ToBin("MMDX");
        public static uint MOBA = ToBin("MOBA");
        public static uint MOBN = ToBin("MOBN");
        public static uint MOBR = ToBin("MOBR");
        public static uint MOCV = ToBin("MOCV");
        public static uint MODD = ToBin("MODD");
        public static uint MODF = ToBin("MODF");
        public static uint MODN = ToBin("MODN");
        public static uint MODR = ToBin("MODR");
        public static uint MODS = ToBin("MODS");
        public static uint MOGI = ToBin("MOGI");
        public static uint MOGN = ToBin("MOGN");
        public static uint MOGP = ToBin("MOGP");
        public static uint MOHD = ToBin("MOHD");
        public static uint MOLR = ToBin("MOLR");
        public static uint MOLT = ToBin("MOLT");
        public static uint MOMT = ToBin("MOMT");
        public static uint MONR = ToBin("MONR");
        public static uint MOTV = ToBin("MOTV");
        public static uint MOPR = ToBin("MOPR");
        public static uint MOPT = ToBin("MOPT");
        public static uint MOPV = ToBin("MOPV");
        public static uint MOPY = ToBin("MOPY");
        public static uint MOSB = ToBin("MOSB");
        public static uint MOTX = ToBin("MOTX");
        public static uint MOVI = ToBin("MOVI");
        public static uint MOVT = ToBin("MOVT");
        public static uint MPHD = ToBin("MPHD");
        public static uint MTEX = ToBin("MTEX");
        public static uint MVER = ToBin("MVER");
        public static uint MWMO = ToBin("MWMO");
        public static uint MOVV = ToBin("MOVV");
        public static uint MOVB = ToBin("MOVB");
        public static uint MCVP = ToBin("MCVP");


        public static uint MPBV = ToBin("MPBV");
        public static uint MPBP = ToBin("MPBP");
        public static uint MPBI = ToBin("MPBI");
        public static uint MPBG = ToBin("MPBG");

        public static uint MORI = ToBin("MORI");
        public static uint MORB = ToBin("MORB");

        public static uint WDBC = ToBin("WDBC");

        private static uint ToBin(String s)
        {

            var ca = s.ToCharArray();
            var b0 = (uint) ca[0];
            var b1 = (uint) ca[1];
            var b2 = (uint) ca[2];
            var b3 = (uint) ca[3];
            var r = b3 | (b2 << 8) | (b1 << 16) | (b0 << 24);
            using (var file = new StreamWriter("sigs.txt", true))
            {
                file.WriteLine("{0} - {1}", s, r);
            }
            return r;
        }
    }
}
