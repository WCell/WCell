using System;
using System.IO;

namespace WCell.Terrain.MPQ
{
    public static class Signatures
    {
        public static readonly uint MAIN = ToBin("MAIN");
        public static readonly uint MAOF = ToBin("MAOF");
        public static readonly uint MARE = ToBin("MARE");
        public static readonly uint MCAL = ToBin("MCAL");
        public static readonly uint MCIN = ToBin("MCIN");
        public static readonly uint MCLQ = ToBin("MCLQ");
        public static readonly uint MCLY = ToBin("MCLY");
        public static readonly uint MCNK = ToBin("MCNK");

        public static readonly uint MCNR = ToBin("MCNR");
        public static readonly uint MCRF = ToBin("MCRF");
        public static readonly uint MCSE = ToBin("MCSE");
        public static readonly uint MCSH = ToBin("MCSH");
        public static readonly uint MCVT = ToBin("MCVT");
        public static readonly uint MCCV = ToBin("MCCV");
        public static readonly uint MDDF = ToBin("MDDF");
        public static readonly uint MFOG = ToBin("MFOG");
        public static readonly uint MH2O = ToBin("MH2O");
        public static readonly uint MHDR = ToBin("MHDR");
        public static readonly uint MLIQ = ToBin("MLIQ");
        public static readonly uint MMDX = ToBin("MMDX");
        public static readonly uint MOBA = ToBin("MOBA");
        public static readonly uint MOBN = ToBin("MOBN");
        public static readonly uint MOBR = ToBin("MOBR");
        public static readonly uint MOCV = ToBin("MOCV");
        public static readonly uint MODD = ToBin("MODD");
        public static readonly uint MODF = ToBin("MODF");
        public static readonly uint MODN = ToBin("MODN");
        public static readonly uint MODR = ToBin("MODR");
        public static readonly uint MODS = ToBin("MODS");
        public static readonly uint MOGI = ToBin("MOGI");
        public static readonly uint MOGN = ToBin("MOGN");
        public static readonly uint MOGP = ToBin("MOGP");
        public static readonly uint MOHD = ToBin("MOHD");
        public static readonly uint MOLR = ToBin("MOLR");
        public static readonly uint MOLT = ToBin("MOLT");
        public static readonly uint MOMT = ToBin("MOMT");
        public static readonly uint MONR = ToBin("MONR");
        public static readonly uint MOTV = ToBin("MOTV");
        public static readonly uint MOPR = ToBin("MOPR");
        public static readonly uint MOPT = ToBin("MOPT");
        public static readonly uint MOPV = ToBin("MOPV");
        public static readonly uint MOPY = ToBin("MOPY");
        public static readonly uint MOSB = ToBin("MOSB");
        public static readonly uint MOTX = ToBin("MOTX");
        public static readonly uint MOVI = ToBin("MOVI");
        public static readonly uint MOVT = ToBin("MOVT");
        public static readonly uint MPHD = ToBin("MPHD");
        public static readonly uint MTEX = ToBin("MTEX");
        public static readonly uint MVER = ToBin("MVER");
        public static readonly uint MWMO = ToBin("MWMO");
        public static readonly uint MOVV = ToBin("MOVV");
        public static readonly uint MOVB = ToBin("MOVB");
        public static readonly uint MCVP = ToBin("MCVP");


        public static readonly uint MPBV = ToBin("MPBV");
        public static readonly uint MPBP = ToBin("MPBP");
        public static readonly uint MPBI = ToBin("MPBI");
        public static readonly uint MPBG = ToBin("MPBG");

        public static readonly uint MORI = ToBin("MORI");
        public static readonly uint MORB = ToBin("MORB");

        public static readonly uint WDBC = ToBin("WDBC");

        private static uint ToBin(String s)
        {

            var ca = s.ToCharArray();
            var b0 = (uint) ca[0];
            var b1 = (uint) ca[1];
            var b2 = (uint) ca[2];
            var b3 = (uint) ca[3];
            var r = b3 | (b2 << 8) | (b1 << 16) | (b0 << 24);
			//using (var file = new StreamWriter("sigs.txt", true))
			//{
			//    file.WriteLine("{0} - {1}", s, r);
			//}
            return r;
        }
    }
}
