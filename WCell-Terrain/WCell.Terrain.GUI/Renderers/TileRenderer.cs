using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using WCell.Terrain.Recast.NavMesh;

namespace WCell.Terrain.GUI.Renderers
{
    public class TileRenderer : GameComponent
    {
        private EnvironmentRenderer environs;
        private WireframeNormalRenderer normals;
        private LiquidRenderer liquids;
        private WireframeNavMeshRenderer wiredNavMesh;
        private SolidNavMeshRenderer solidNavMesh;

        private bool environsWereEnabled = true;
        private bool liquidsWereEnabled = true;
        private bool navMeshWasEnabled = true;

        public bool NavMeshEnabled
        {
            get { return wiredNavMesh.Enabled; }
            set
            {
                navMeshWasEnabled = value;
				if (!Enabled || wiredNavMesh == null) return;

                wiredNavMesh.Enabled = value;
                solidNavMesh.Enabled = value;
            }
        }

        public bool LiquidEnabled
        {
            get { return liquids.Enabled; }
            set
            {
                liquidsWereEnabled = value;
                if (!Enabled) return;

                liquids.Enabled = value;
            }
        }

        public bool EnvironsEnabled
        {
            get { return environs.Enabled; }
            set
            {
                environsWereEnabled = value;
                if (!Enabled) return;

                environs.Enabled = value;
            }
        }

        public TileRenderer(Game game, TerrainTile tile) : base(game)
        {
            environs = new EnvironmentRenderer(game, tile);
            normals = new WireframeNormalRenderer(game, environs);
			liquids = new LiquidRenderer(game, tile);

			Game.Components.Add(environs);
			Game.Components.Add(normals);
			Game.Components.Add(liquids);

			if (NavMeshBuilder.AutoLoadNavmeshes)
			{
				solidNavMesh = new SolidNavMeshRenderer(game, tile);
				wiredNavMesh = new WireframeNavMeshRenderer(game, tile);

				Game.Components.Add(solidNavMesh);
				Game.Components.Add(wiredNavMesh);
			}
            
            Disposed += (sender, args) => Cleanup();
            EnabledChanged += (sender, args) => EnabledToggled();
        }

        private void EnabledToggled()
        {
            if (Enabled)
            {
                environs.Enabled = environsWereEnabled;
                wiredNavMesh.Enabled = navMeshWasEnabled;
                solidNavMesh.Enabled = navMeshWasEnabled;
                liquids.Enabled = liquidsWereEnabled;
            }
            else
            {
                environs.Enabled = Enabled;
                wiredNavMesh.Enabled = Enabled;
                solidNavMesh.Enabled = Enabled;
                liquids.Enabled = Enabled;
            }
        }

        private void Cleanup()
        {
            RemoveSubComponents();

            environs.Dispose();
            liquids.Dispose();
			if (wiredNavMesh != null)
			{
				wiredNavMesh.Dispose();
			}
			if (solidNavMesh != null)
			{
				solidNavMesh.Dispose();
			}
        }

        private void RemoveSubComponents()
        {
            Game.Components.Remove(environs);
            Game.Components.Remove(liquids);
            Game.Components.Remove(wiredNavMesh);
            Game.Components.Remove(solidNavMesh);
        }
    }
}
