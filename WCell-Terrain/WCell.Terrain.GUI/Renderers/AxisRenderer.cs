using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WCell.Constants;
using WCell.Terrain;
using WCell.Terrain.GUI.Util;


namespace WCell.Terrain.GUI.Renderers
{
    class AxisRenderer : RendererBase
    {
        public override void Initialize()
        {
            base.Initialize();

            _vertexDeclaration = new VertexDeclaration(VertexPositionNormalColored.VertexElements);
        }

        public AxisRenderer(Game game) : base(game)
        {
        }

        protected override void BuildVerticiesAndIndicies()
        {
            var tileId = TileIdentifier.DefaultTileIdentifier;
            var tempVertices = new List<VertexPositionNormalColored>();
            var tempIndices = new List<int>();
            
            var baseXPos = TerrainConstants.CenterPoint - (tileId.X + 1)*TerrainConstants.TileSize;
            var baseYPos = TerrainConstants.CenterPoint - (tileId.Y + 1)*TerrainConstants.TileSize;
            var baseZPos = -100.0f;

            // The Bottom-Righthand corner of a Tile in WoW coords
            var baseAxisVec = new Vector3(baseXPos, baseYPos, baseZPos);
            
            // The Top-Lefthand corner of a Tile in WoW coords
            var endAxisXVec = baseAxisVec + new Vector3(TerrainConstants.TileSize*2, 0.0f, 0.0f);
            var endAxisYVec = baseAxisVec + new Vector3(0.0f, TerrainConstants.TileSize*2, 0.0f);
            var endAxisZVec = baseAxisVec + new Vector3(0.0f, 0.0f, TerrainConstants.TileSize*2);

            var upperLeftForward = new Vector3(1.0f, 1.0f, 1.0f);
            var upperLeftBackward = new Vector3(-1.0f, 1.0f, 1.0f);
            var upperRightForward = new Vector3(1.0f, -1.0f, 1.0f);
            var upperRightBackward = new Vector3(-1.0f, -1.0f, 1.0f);
            var lowerLeftForward = new Vector3(1.0f, 1.0f, -1.0f);
            var lowerLeftBackward = new Vector3(-1.0f, 1.0f, -1.0f);
            var lowerRightForward = new Vector3(1.0f, -1.0f, -1.0f);
            var lowerRightBackward = new Vector3(-1.0f, -1.0f, -1.0f);
            
            XNAUtil.TransformWoWCoordsToXNACoords(ref baseAxisVec);
            XNAUtil.TransformWoWCoordsToXNACoords(ref endAxisXVec);
            XNAUtil.TransformWoWCoordsToXNACoords(ref endAxisYVec);
            XNAUtil.TransformWoWCoordsToXNACoords(ref endAxisZVec);
            XNAUtil.TransformWoWCoordsToXNACoords(ref upperLeftForward);
            XNAUtil.TransformWoWCoordsToXNACoords(ref upperLeftBackward);
            XNAUtil.TransformWoWCoordsToXNACoords(ref upperRightForward);
            XNAUtil.TransformWoWCoordsToXNACoords(ref upperRightBackward);
            XNAUtil.TransformWoWCoordsToXNACoords(ref lowerLeftForward);
            XNAUtil.TransformWoWCoordsToXNACoords(ref lowerLeftBackward);
            XNAUtil.TransformWoWCoordsToXNACoords(ref lowerRightForward);
            XNAUtil.TransformWoWCoordsToXNACoords(ref lowerRightBackward);

            var xAxisColor = Color.Red;
            var yAxisColor = Color.White;
            var zAxisColor = Color.Blue;

            // The WoW X-axis drawn in XNA coords
            // FrontFace
            tempVertices.Add(new VertexPositionNormalColored(baseAxisVec + upperLeftBackward, xAxisColor, Vector3.Up));  //0
            tempVertices.Add(new VertexPositionNormalColored(baseAxisVec + upperRightBackward, xAxisColor, Vector3.Up)); //1
            tempVertices.Add(new VertexPositionNormalColored(baseAxisVec + lowerLeftBackward, xAxisColor, Vector3.Up));  //2
            tempVertices.Add(new VertexPositionNormalColored(baseAxisVec + lowerRightBackward, xAxisColor, Vector3.Up)); //3
            tempIndices.Add(0); tempIndices.Add(1); tempIndices.Add(2);
            tempIndices.Add(1); tempIndices.Add(3); tempIndices.Add(2);
            // BackFace
            tempVertices.Add(new VertexPositionNormalColored(endAxisXVec + upperLeftForward, xAxisColor, Vector3.Up));  //4
            tempVertices.Add(new VertexPositionNormalColored(endAxisXVec + upperRightForward, xAxisColor, Vector3.Up)); //5
            tempVertices.Add(new VertexPositionNormalColored(endAxisXVec + lowerLeftForward, xAxisColor, Vector3.Up));  //6
            tempVertices.Add(new VertexPositionNormalColored(endAxisXVec + lowerRightForward, xAxisColor, Vector3.Up)); //7
            tempIndices.Add(4); tempIndices.Add(5); tempIndices.Add(6);
            tempIndices.Add(5); tempIndices.Add(7); tempIndices.Add(6);
            // TopFace
            tempIndices.Add(4); tempIndices.Add(5); tempIndices.Add(0);
            tempIndices.Add(5); tempIndices.Add(1); tempIndices.Add(0);
            // BottomFace
            tempIndices.Add(7); tempIndices.Add(6); tempIndices.Add(3);
            tempIndices.Add(6); tempIndices.Add(2); tempIndices.Add(3);
            // LeftFace
            tempIndices.Add(6); tempIndices.Add(4); tempIndices.Add(2);
            tempIndices.Add(4); tempIndices.Add(0); tempIndices.Add(2);
            // RightFace
            tempIndices.Add(5); tempIndices.Add(7); tempIndices.Add(1);
            tempIndices.Add(7); tempIndices.Add(3); tempIndices.Add(1);

            //tempVertices.Add(new VertexPositionNormalColored(new Vector3(baseAxisVec.X, baseAxisVec.Y, endAxisVec.Z), Color.Red, Vector3.Up));
			

            // The WoW Y-axis drawn in XNA coords
            tempVertices.Add(new VertexPositionNormalColored(endAxisYVec + upperLeftBackward, yAxisColor, Vector3.Up));  //8
            tempVertices.Add(new VertexPositionNormalColored(baseAxisVec + upperRightBackward, yAxisColor, Vector3.Up)); //9
            tempVertices.Add(new VertexPositionNormalColored(endAxisYVec + lowerLeftBackward, yAxisColor, Vector3.Up));  //10
            tempVertices.Add(new VertexPositionNormalColored(baseAxisVec + lowerRightBackward, yAxisColor, Vector3.Up)); //11
            tempIndices.Add(8); tempIndices.Add(9); tempIndices.Add(10);
            tempIndices.Add(9); tempIndices.Add(11); tempIndices.Add(10);
            // BackFace
            tempVertices.Add(new VertexPositionNormalColored(endAxisYVec + upperLeftForward, yAxisColor, Vector3.Up));  //12
            tempVertices.Add(new VertexPositionNormalColored(baseAxisVec + upperRightForward, yAxisColor, Vector3.Up)); //13
            tempVertices.Add(new VertexPositionNormalColored(endAxisYVec + lowerLeftForward, yAxisColor, Vector3.Up));  //14
            tempVertices.Add(new VertexPositionNormalColored(baseAxisVec + lowerRightForward, yAxisColor, Vector3.Up)); //15
            tempIndices.Add(12); tempIndices.Add(13); tempIndices.Add(14);
            tempIndices.Add(13); tempIndices.Add(15); tempIndices.Add(14);
            // TopFace
            tempIndices.Add(12); tempIndices.Add(13); tempIndices.Add(8);
            tempIndices.Add(13); tempIndices.Add(9); tempIndices.Add(8);
            // BottomFace
            tempIndices.Add(15); tempIndices.Add(14); tempIndices.Add(11);
            tempIndices.Add(14); tempIndices.Add(10); tempIndices.Add(11);
            // LeftFace
            tempIndices.Add(14); tempIndices.Add(12); tempIndices.Add(10);
            tempIndices.Add(12); tempIndices.Add(8); tempIndices.Add(10);
            // RightFace
            tempIndices.Add(13); tempIndices.Add(15); tempIndices.Add(9);
            tempIndices.Add(15); tempIndices.Add(11); tempIndices.Add(9);
            

            // The WoW Z-axis
            tempVertices.Add(new VertexPositionNormalColored(endAxisZVec + upperLeftBackward, zAxisColor, Vector3.Up));  //16
            tempVertices.Add(new VertexPositionNormalColored(endAxisZVec + upperRightBackward, zAxisColor, Vector3.Up)); //17
            tempVertices.Add(new VertexPositionNormalColored(baseAxisVec + lowerLeftBackward, zAxisColor, Vector3.Up));  //18
            tempVertices.Add(new VertexPositionNormalColored(baseAxisVec + lowerRightBackward, zAxisColor, Vector3.Up)); //19
            tempIndices.Add(16); tempIndices.Add(17); tempIndices.Add(18);
            tempIndices.Add(17); tempIndices.Add(19); tempIndices.Add(18);
            // BackFace
            tempVertices.Add(new VertexPositionNormalColored(endAxisZVec + upperLeftForward, zAxisColor, Vector3.Up));  //20
            tempVertices.Add(new VertexPositionNormalColored(endAxisZVec + upperRightForward, zAxisColor, Vector3.Up)); //21
            tempVertices.Add(new VertexPositionNormalColored(baseAxisVec + lowerLeftForward, zAxisColor, Vector3.Up));  //22
            tempVertices.Add(new VertexPositionNormalColored(baseAxisVec + lowerRightForward, zAxisColor, Vector3.Up)); //23
            tempIndices.Add(20); tempIndices.Add(21); tempIndices.Add(22);
            tempIndices.Add(21); tempIndices.Add(23); tempIndices.Add(22);
            // TopFace
            tempIndices.Add(20); tempIndices.Add(21); tempIndices.Add(16);
            tempIndices.Add(21); tempIndices.Add(17); tempIndices.Add(16);
            // BottomFace
            tempIndices.Add(23); tempIndices.Add(22); tempIndices.Add(19);
            tempIndices.Add(22); tempIndices.Add(18); tempIndices.Add(19);
            // LeftFace
            tempIndices.Add(22); tempIndices.Add(20); tempIndices.Add(18);
            tempIndices.Add(20); tempIndices.Add(16); tempIndices.Add(18);
            // RightFace
            tempIndices.Add(21); tempIndices.Add(23); tempIndices.Add(17);
            tempIndices.Add(23); tempIndices.Add(19); tempIndices.Add(17);

            _cachedIndices = tempIndices.ToArray();
            _cachedVertices = tempVertices.ToArray();

            _renderCached = true;
        }
    }
}
