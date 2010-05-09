using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _3D_Test_Bench
{
    public class TreeManager
    {
        System.Windows.Forms.TreeView worldTree;
        TestBenchInterface currentTBI;

        public TreeManager(System.Windows.Forms.TreeView ObjectTree)
        {
            this.worldTree = ObjectTree;
        }

        public TestBenchInterface CurrentTestBench
        {
            set
            {
                if (this.currentTBI != null)
                {
                    this.currentTBI.PlainObjectAdded -= this.currentTBI_PlainObjectAdded;
                    this.currentTBI.PlainObjectRemoved -= this.currentTBI_PlainObjectRemoved;
                }
                this.currentTBI = value;
                currentTBI.PlainObjectAdded += new PlainObjectHandler(currentTBI_PlainObjectAdded);
                currentTBI.PlainObjectRemoved += new PlainObjectHandler(currentTBI_PlainObjectRemoved);
            }
            get
            {
                return this.currentTBI;
            }
        }

        public event PlainObjectHandler OnObjectAdded, OnObjectRemoved;

        protected virtual void FireObjectAdded(PlainObjectInterface plainObject)
        {
            if (this.OnObjectAdded != null)
                this.OnObjectAdded(plainObject);
        }

        protected virtual void FireObjectRemoved(PlainObjectInterface plainObject)
        {
            if (this.OnObjectRemoved != null)
                this.OnObjectRemoved(plainObject);
        }
 

        private delegate void TreeObjectDelegate(object newObject);
        private void AddObjectToTree(object newObject)
        {
            if (this.worldTree.InvokeRequired)
            {
                TreeObjectDelegate d = new TreeObjectDelegate(AddObjectToTree);
                this.worldTree.Invoke(d, new object[] { newObject });
            }
            else
            {
                PlainObjectInterface PO = (PlainObjectInterface)newObject;
                PlainTreeNodeContainer TN = new PlainTreeNodeContainer(PO);
                if (PO.ObjectParent == null)
                {
                    this.worldTree.Nodes[0].Nodes.Add(TN);
                }
                else
                {
                    PlainTreeNodeContainer parent = FindParentNode(PO.ObjectParent);
                    parent.Nodes.Add(TN);
                }

                this.FireObjectAdded(PO);
            }
        }

        private void RemoveObjectFromTree(object objectToRemove)
        {
            if (this.worldTree.InvokeRequired)
            {
                TreeObjectDelegate d = new TreeObjectDelegate(RemoveObjectFromTree);
                this.worldTree.Invoke(d, new object[] { objectToRemove });
            }
            else
            {
                PlainTreeNodeContainer treeObject = FindParentNode((PlainObjectInterface)objectToRemove);
                if (treeObject != null)
                    treeObject.Remove();

                this.FireObjectRemoved((PlainObjectInterface)objectToRemove);
            }
        }

        private PlainTreeNodeContainer FindParentNode(PlainObjectInterface parentObject)
        {
            foreach (System.Windows.Forms.TreeNode treeNode in this.worldTree.Nodes)
            {
                foreach (PlainTreeNodeContainer PTNC in treeNode.Nodes)
                {
                    PlainTreeNodeContainer tempNode = NodeInSandBox(PTNC, parentObject);
                    if (tempNode != null)
                        return tempNode;
                }
            }
            return null;
        }

        private PlainTreeNodeContainer NodeInSandBox(PlainTreeNodeContainer node, PlainObjectInterface parentObject)
        {
            foreach (PlainTreeNodeContainer PTNC in node.Nodes)
            {
                PlainTreeNodeContainer tempNode = NodeInSandBox(PTNC, parentObject);
                if (tempNode != null)
                    return tempNode;
            }
            if (node.PlainObject == parentObject)
                return node;
            else
                return null;
        }

        void currentTBI_PlainObjectRemoved(PlainObjectInterface plainObject)
        {
            RemoveObjectFromTree(plainObject);
        }

        void currentTBI_PlainObjectAdded(PlainObjectInterface plainObject)
        {
            AddObjectToTree(plainObject);
        }

        public void ExternalObjectInsert(PlainObjectInterface plainObject)
        {
            AddObjectToTree(plainObject);
        }

        public void ExternalObjectRemove(PlainObjectInterface plainObject)
        {
            RemoveObjectFromTree(plainObject);
        }
    }

    public class PlainTreeNodeContainer : System.Windows.Forms.TreeNode
    {
        PlainObjectInterface PO;
        public PlainTreeNodeContainer(PlainObjectInterface plainObject)
        {
            this.PO = plainObject;
            this.Text = PO.ToString();
        }

        public PlainObjectInterface PlainObject
        {
            get
            {
                return this.PO;
            }
        }

        public override string ToString()
        {
            return PO.ToString();
        }
        
    }
}
