using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPQNav.MPQ.ADT;

namespace TerrainDisplay.Extracted
{
    public class ExtractedADTManager : IADTManager
    {
        private IList<ADTBase> _mapTiles;

        public IList<ADTBase> MapTiles
        {
            get { return _mapTiles; }
        }

        public void LoadTile(int tileX, int tileY)
        {
            throw new NotImplementedException();
        }
    }
}
