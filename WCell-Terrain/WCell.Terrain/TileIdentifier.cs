using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using TerrainDisplay.World.DBC;
using WCell.Constants.World;
using WCell.Util.Graphics;

namespace WCell.Terrain
{
    public class TileIdentifier : Point2D, IXmlSerializable, IEquatable<TileIdentifier>
    {
        // This dictionary is initialized on startup.
        public static readonly Dictionary<MapId, string> InternalMapNames;
        private const string mapDBCName = "Map.dbc";

        /// <summary>
        /// Optional name of the area in the map (i.e. Redridge, Burning Steppes)
        /// </summary>
    	public string TileName;
        public MapId MapId;

        /// <summary>
        /// The InternalName from Map.dbc
        /// </summary>
        public string MapName;

        static TileIdentifier()
        {
            InternalMapNames = new Dictionary<MapId, string>((int)MapId.End);
            var dbcPath = Path.GetFullPath(Path.Combine(WCellTerrainSettings.DBCDir, mapDBCName));
            var dbcMapReader = new MappedDBCReader<MapInfo, DBCMapEntryConverter>(dbcPath);
            foreach (var mapInfo in dbcMapReader.Entries.Select(entry => entry.Value))
            {
                InternalMapNames.Add(mapInfo.Id, mapInfo.InternalName);
            }

            Redridge = new TileIdentifier
            {
                TileName = "Redridge",
                MapId = MapId.EasternKingdoms,
                MapName = InternalMapNames[MapId.EasternKingdoms],
                X = 49,
                Y = 36
            };

            CenterTile = new TileIdentifier
            {
                TileName = "Map Center",
                MapId = MapId.EasternKingdoms,
                MapName = InternalMapNames[MapId.EasternKingdoms],
                X = 32,
                Y = 32
            };

            Stormwind = new TileIdentifier
            {
                TileName = "Stormwind",
                MapId = MapId.EasternKingdoms,
                MapName = InternalMapNames[MapId.EasternKingdoms],
                X = 48,
                Y = 30
            };

            BurningSteppes = new TileIdentifier
            {
                TileName = "Burning Steppes",
                MapId = MapId.EasternKingdoms,
                MapName = InternalMapNames[MapId.EasternKingdoms],
                X = 49,
                Y = 33
            };
        }

        public TileIdentifier()
        {
        }

        public TileIdentifier(string tileName, MapId mapId, string mapName, int tileX, int tileY)
        {
            TileName = tileName;
            MapId = mapId;
            MapName = mapName;
            X = tileX;
            Y = tileY;
        }

        public TileIdentifier Copy()
		{
			return new TileIdentifier(TileName, MapId, MapName, X, Y);
		}

        public static TileIdentifier ByPosition(MapId mapId, Vector3 position)
        {
            int tileX, tileY;
            if (!PositionUtil.GetTileXYForPos(position, out tileX, out tileY))
            {
                return null;
            }

            return new TileIdentifier
            {
                MapId = mapId,
                MapName = InternalMapNames[mapId],
                X = tileX,
                Y = tileY
            };
        }

        public static readonly TileIdentifier Redridge;

        public static readonly TileIdentifier CenterTile;

        public static readonly TileIdentifier Stormwind;

        public static readonly TileIdentifier BurningSteppes;

        public static TileIdentifier DefaultTileIdentifier = new TileIdentifier
        {
            TileName = "Redridge",
            MapId = MapId.EasternKingdoms,
            MapName = "Azeroth",
            X = 49,
            Y = 36
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
                var val = reader.ReadElementContentAsString();
                MapId = (MapId)Enum.Parse(typeof(MapId), val);
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
                X = 32;
            }
            else
            {
                X = reader.ReadElementContentAsInt();
            }
            
            reader.Read();
            if (!reader.IsStartElement("TileY"))
            {
                Console.WriteLine(
                    "Malformed TileIdentifer entry in the config Xml. TileY should be the fifth element. ... (Speaking of Fifth Element, what a great show, yeah?)");
                Y = 32;
            }
            else
            {
                Y = reader.ReadElementContentAsInt();
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteElementString("TileName", TileName);
            writer.WriteElementString("MapId", MapId.ToString());
            writer.WriteElementString("MapName", MapName);
            writer.WriteElementString("TileX", X.ToString());
            writer.WriteElementString("TileY", Y.ToString());
        }

        #endregion

        #region Implementation of IEquatable
        public bool Equals(TileIdentifier other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.MapId, MapId) && (other.X == X) && (other.Y == Y);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(TileIdentifier)) return false;
            return Equals((TileIdentifier)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = MapId.GetHashCode();
                result = (result*397) ^ X;
                result = (result*397) ^ Y;
                return result;
            }
        }

        public static bool operator ==(TileIdentifier left, TileIdentifier right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TileIdentifier left, TileIdentifier right)
        {
            return !Equals(left, right);
        }
        #endregion

        public override string ToString()
        {
            return String.Format("TileId: MapId: {0}, X: {1}, Y: {2}", MapId, X, Y);
        }
    }
}
