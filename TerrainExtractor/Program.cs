using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TerrainDisplay;

namespace TerrainExtractor
{
    public static class Program
    {
        private static TerrainDisplayConfig m_configuration;

        public static void Main(string[] args)
        {
            TerrainDisplayConfig.Initialize();
        }
    }
}
