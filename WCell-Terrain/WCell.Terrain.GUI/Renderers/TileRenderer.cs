using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace WCell.Terrain.GUI.Renderers
{
    public class TileRenderer : RendererBase
    {
        private EnvironmentRenderer environs;
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
                if (!Enabled) return;

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
            game.Components.Add(environs = new EnvironmentRenderer(game, tile));
            game.Components.Add(liquids = new LiquidRenderer(game, tile));
            game.Components.Add(wiredNavMesh = new WireframeNavMeshRenderer(game, tile));
            game.Components.Add(solidNavMesh = new SolidNavMeshRenderer(game, tile));

            Disposed += (sender, args) => Cleanup();
            EnabledChanged += (sender, args) => EnabledToggled();
        }

        protected override void BuildVerticiesAndIndicies()
        {
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
            wiredNavMesh.Dispose();
            solidNavMesh.Dispose();
        }

        private void RemoveSubComponents()
        {
            Viewer.Components.Remove(environs);
            Viewer.Components.Remove(liquids);
            Viewer.Components.Remove(wiredNavMesh);
            Viewer.Components.Remove(solidNavMesh);
        }
    }
}
