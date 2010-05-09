using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace _3D_Test_Bench
{
    public partial class TestBench : Form
    {
        private TreeManager treeManager;
        public TestBench()
        {
            InitializeComponent();
            treeManager = new TreeManager(this.worldObjects);
            treeManager.OnObjectAdded += new PlainObjectHandler(treeManager_OnObjectAdded);
            treeManager.OnObjectRemoved += new PlainObjectHandler(treeManager_OnObjectRemoved);

            //Test Code for the Tree Manager
            /*
            WireFrameObject obj1, obj2, obj3, obj4;
            obj1 = new WireFrameObject(null, Color.Red, new Microsoft.DirectX.Vector3(0, 0, 0), "obj1");
            obj2 = new WireFrameObject(obj1, Color.Red, new Microsoft.DirectX.Vector3(0, 0, 0), "obj2");
            obj3 = new WireFrameObject(obj1, Color.Red, new Microsoft.DirectX.Vector3(0, 0, 0), "obj3");
            obj4 = new WireFrameObject(obj2, Color.Red, new Microsoft.DirectX.Vector3(0, 0, 0), "obj4");

            treeManager.ExternalObjectInsert(obj1);
            treeManager.ExternalObjectInsert(obj2);
            treeManager.ExternalObjectInsert(obj3);
            treeManager.ExternalObjectInsert(obj4);

            treeManager.ExternalObjectRemove(obj2);
            */
            //End Test Code

            
        }

        //Used to inform the rendering engine that an object has been removed
        void treeManager_OnObjectRemoved(PlainObjectInterface plainObject)
        {
            
        }

        //Used to inform the rendering engine that an object has been added
        void treeManager_OnObjectAdded(PlainObjectInterface plainObject)
        {
            
        }

        private void TestBench_Load(object sender, EventArgs e)
        {
            
        }
    }
}
