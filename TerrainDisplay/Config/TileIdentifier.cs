using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace TerrainDisplay
{
    public class TileIdentifier : IXmlSerializable
    {
    	public string TileName;

    	public int MapId;
        public string MapName;
        public int TileX;
        public int TileY;

        public TileIdentifier()
        {
        }

        public TileIdentifier(string tileName, int mapId, string mapName, int tileX, int tileY)
        {
            TileName = tileName;
            MapId = mapId;
            MapName = mapName;
            TileX = tileX;
            TileY = tileY;
        }

		public TileIdentifier Copy()
		{
			return new TileIdentifier(TileName, MapId, MapName, TileX, TileY);
		}

        public readonly static TileIdentifier Redridge = new TileIdentifier
        {
            TileName = "Redridge",
            MapId = 0,
            MapName = "Azeroth",
            TileX = 49,
            TileY = 36
        };

		public readonly static TileIdentifier CenterTile = new TileIdentifier
        {
            TileName = "Map Center",
            MapId = 0,
            MapName = "Azeroth",
            TileX = 32,
            TileY = 32
        };

		public readonly static TileIdentifier Stormwind = new TileIdentifier
        {
            TileName = "Stormwind",
            MapId = 0,
            MapName = "Azeroth",
            TileX = 48,
            TileY = 30
        };

		public readonly static TileIdentifier BurningSteppes = new TileIdentifier
        {
            TileName = "Burning Steppes",
            MapId = 0,
            MapName = "Azeroth",
            TileX = 49,
            TileY = 33
        };
        
        #region Implementation of IXmlSerializable

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(XmlReader reader)
        {
            reader.Read();
            if (!reader.IsStartElement("TileName"))
            {
                Console.WriteLine(
                    "Malformed TileIdentifer entry in the config Xml. TileName should be the first element.");
                TileName = "MapCenter";
            }
            else
            {
                TileName = reader.ReadElementContentAsString();
            }

            reader.Read();
            if (!reader.IsStartElement("MapId"))
            {
                Console.WriteLine(
                    "Malformed TileIdentifer entry in the config Xml. MapId should be the second element.");
                MapId = 0;
            }
            else 
            {
                MapId = reader.ReadElementContentAsInt();
            }
            
            reader.Read();
            if (!reader.IsStartElement("MapName"))
            {
                Console.WriteLine(
                    "Malformed TileIdentifer entry in the config Xml. MapName should be the third element.");
                MapName = "Map Center";
            }
            else
            {
                MapName = reader.ReadElementContentAsString();
            }
            
            reader.Read();
            if (!reader.IsStartElement("TileX"))
            {
                Console.WriteLine(
                    "Malformed TileIdentifer entry in the config Xml. TileX should be fourth element.");
                TileX = 32;
            }
            else
            {
                TileX = reader.ReadElementContentAsInt();
            }
            
            reader.Read();
            if (!reader.IsStartElement("TileY"))
            {
                Console.WriteLine(
                    "Malformed TileIdentifer entry in the config Xml. TileY should be the fifth element. ... (Speaking of Fifth Element, what a great show, yeah?)");
                TileY = 32;
            }
            else
            {
                TileY = reader.ReadElementContentAsInt();
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteElementString("TileName", TileName);
            writer.WriteElementString("MapId", MapId.ToString());
            writer.WriteElementString("MapName", MapName);
            writer.WriteElementString("TileX", TileX.ToString());
            writer.WriteElementString("TileY", TileY.ToString());
        }

        #endregion
    }
}
