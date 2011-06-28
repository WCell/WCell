using System;
using WCell.Util.Graphics;

namespace Terra.Geometry
{
    internal class Edge : ILabelled
    {
        private Edge qNext;
        private Edge qPrev;
        private Vector2 data;
        private Edge next;

        private Edge(Edge prev)
        {
            qPrev = prev;
            prev.qNext = this;
            LFace = null;
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

            LFace = null;
            Token = 0;
        }

        public void Delete()
        {
            if (qNext == null) return;

            var e1 = qNext;
            var e2 = qNext.qNext;
            var e3 = qPrev;

            e1.qNext = null;
            e2.qNext = null;
            e3.qNext = null;

            e1.Delete();
            e2.Delete();
            e3.Delete();
        }

        // Primitives
        public Edge ONext { get { return next; } }
        public Edge Sym { get { return qNext.qNext; } }
        public Edge Rot { get { return qNext; } }
        public Edge InvRot { get { return qPrev; } }

        // Synthesized
        public Edge OPrev { get { return Rot.ONext.Rot; } }
        public Edge DNext { get { return Sym.ONext.Sym; } }
        public Edge DPrev { get { return InvRot.ONext.InvRot; } }
        public Edge LNext { get { return InvRot.ONext.Rot; } }
        public Edge LPrev { get { return ONext.Sym; } }
        public Edge RNext { get { return Rot.ONext.InvRot; } }
        public Edge RPrev { get { return Sym.ONext; } }

        public Vector2 Orig
        {
            get { return data; }
        }

        public Vector2 Dest
        {
            get { return Sym.data; }
        }

        public Triangle LFace { get; set; }

        public void SetEndPoints(Vector2 orig, Vector2 dest)
        {
            data = orig;
            Sym.data = dest;
        }

        public static bool IsRightOf(Vector2 p, Edge e)
        {
            return GeometryHelpers.IsRightOf(p, e.Orig, e.Dest);
        }

        public static bool IsLeftOf(Vector2 p, Edge e)
        {
            return GeometryHelpers.IsLeftOf(p, e.Orig, e.Dest);
        }

        public static void Splice(Edge a, Edge b)
        {
            var alpha = a.ONext.Rot;
            var beta = b.ONext.Rot;

            var t1 = b.ONext;
            var t2 = a.ONext;
            var t3 = beta.ONext;
            var t4 = alpha.ONext;

            a.next = t1;
            b.next = t2;
            alpha.next = t3;
            beta.next = t4;
        }

        public override string ToString()
        {
            return string.Format("{0} ----> {1}", Orig, Dest);
        }

        public int Token { get; set; }
    }
}