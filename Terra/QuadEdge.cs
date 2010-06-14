using System;
using TerrainDisplay.Terra;
using WCell.Util.Graphics;

namespace Terra
{
     public class Edge : ILabelled
     {
         private Edge qNext;
         private Edge qPrev;
         private Vector2 data;
         private Edge next;
         private Triangle lFace;
         
         private Edge(Edge prev)
         {
             qPrev = prev;
             prev.qNext = this;
             lFace = null;
             Token = 0;
         }
         
         public Edge()
         {
             var e0 = this;
             var e1 = new Edge(e0);
             var e2 = new Edge(e1);
             var e3 = new Edge(e2);

             qPrev = e3;
             e3.qNext = e0;

             e0.next = e0;
             e1.next = e3;
             e2.next = e2;
             e3.next = e1;

             lFace = null;
             Token = 0;
         }
         
         // Primitive Methods
         public Edge ONext { get { return next; } }
         public Edge Sym { get { return qNext.qNext; } }
         public Edge Rot { get { return qNext; } }
         public Edge InvRot { get { return qPrev; } }
         
         // Synthesized Methods
         public Edge OPrev { get { return Rot.ONext.Rot; } }
         public Edge DNext { get { return Sym.ONext.Sym; } }
         public Edge DPrev { get { return InvRot.ONext.InvRot; } }
         public Edge LNext { get { return InvRot.ONext.Rot; } }
         public Edge LPrev { get { return ONext.Sym; } }
         public Edge RNext { get { return Rot.ONext.InvRot; }}
         public Edge RPrev { get { return Sym.ONext; } }

         public Vector2 Orig
         {
             get { return data; }
             set { data = value; }
         }

         public Vector2 Dest
         {
             get { return Sym.data; }
         }

         public Triangle LFace
         {
             get { return lFace; }
         }


         public void SetEndPoints (Vector2 orig, Vector2 dest)
         {
             data = orig;
             Sym.data = dest;
         }

         public static bool IsRightOf (ref Vector2 p, Edge e)
         {
             var orig = e.Orig;
             var dest = e.Dest;
             return GeometryHelpers.IsRightOf(ref p, ref orig, ref dest);
         }

         public static bool IsLeftOf (ref Vector2 p, Edge e)
         {
             var orig = e.Orig;
             var dest = e.Dest;
             return GeometryHelpers.IsLeftOf(ref p, ref orig, ref dest);
         }

         public override string ToString()
         {
             return string.Format("{0} ----> {1}", Orig, Dest);
         }

         public int Token { get; set; }
     }
}