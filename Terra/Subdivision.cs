using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terra.Geometry;
using WCell.Util.Graphics;

namespace Terra
{
    public class Subdivision
    {
        private Edge startingEdge;
        private Triangle firstFace;

        
        public Subdivision(ref Vector2 a, ref Vector2 b, ref Vector2 c, ref Vector2 d)
        {
            InitMesh(ref a, ref b, ref c, ref d);
        }

        protected Subdivision()
        {
        }


        public virtual bool ShouldSwap(ref Vector2 point, Edge edge)
        {
            var start = edge.Orig;
            var a = edge.OPrev.Dest;
            var b = edge.Dest;

            return GeometryHelpers.IsInCircle(ref start, ref a, ref b, ref point);
        }

        public bool IsInterior(Edge edge)
        {
            return (edge.LNext.LNext.LNext == edge &&
                    edge.RNext.RNext.RNext == edge);
        }

        public Edge Spoke(Vector2 point, Edge edge)
        {
            var newFaces = new Triangle[4];
            var faceIdx = 0;

            // Note e is the edge returned by Locate(point)

            if ((point == edge.Orig) || (point == edge.Dest))
            {
                Console.WriteLine("WARNING: Tried to re-insert point: {0}", point);
                Console.WriteLine("         orig: {0}", edge.Orig);
                Console.WriteLine("         dest: {0}", edge.Dest);
                return null;
            }

            Edge boundaryEdge = null;

            var lFace = edge.LFace;
            lFace.DontAnchor(edge);
            newFaces[faceIdx++] = lFace;

            if (OnEdge(point, edge))
            {
                if (CCWBoundary(edge))
                {
                    // edge is on the boundary
                    // defer deletion until after new edges are added
                    boundaryEdge = edge;
                }
                else
                {
                    var symLFace = edge.Sym.LFace;
                    newFaces[faceIdx++] = symLFace;
                    symLFace.DontAnchor(edge.Sym);

                    edge = edge.OPrev;
                    DeleteEdge(edge.ONext);
                }
            }
            else
            {
                // point lies with the LFace of edge
            }

            var baseEdge = MakeEdge(edge.Orig, point);
            Edge.Splice(baseEdge, edge);

            do
            {
                baseEdge = Connect(edge, baseEdge.Sym);
                edge = baseEdge.OPrev;
            } while (edge.LNext != startingEdge);

            if (boundaryEdge != null)
            {
                DeleteEdge(boundaryEdge);
            }

            baseEdge = (boundaryEdge != null) ? startingEdge.RPrev : startingEdge.Sym;
            do
            {
                if (faceIdx > 0)
                {
                    newFaces[--faceIdx].Reshape(baseEdge);
                    baseEdge = baseEdge.ONext;
                    continue;
                }

                MakeFace(baseEdge);
                baseEdge = baseEdge.ONext;
            } while (baseEdge != startingEdge.Sym);

            return startingEdge;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point">The center point of the spokes to optimize.</param>
        /// <param name="edge">An edge pointing AWAY from point.</param>
        public void Optimize(ref Vector2 point, Edge edge)
        {
            var startSpoke = edge;
            var spoke = edge;

            while (true)
            {
                var e = spoke.LNext;
                if (IsInterior(e) && ShouldSwap(ref point, e))
                {
                    Swap(e);
                    continue;
                }

                spoke = spoke.ONext;
                if (spoke == startSpoke) break;
            }

            // Update all the triangles
            spoke = startSpoke;
            do
            {
                var e = spoke.LNext;
                var t = e.LFace;
                if (t != null)
                {
                    t.Update(this);
                }
                
                spoke = spoke.ONext;
            } while (spoke != startSpoke);
        }

        public Edge Locate(ref Vector2 point)
        {
            return Locate(ref point, startingEdge);
        }

        public Edge Locate(ref Vector2 point, Edge edgeHint)
        {
            var e = edgeHint;
            var t = GeometryHelpers.TriArea(point, e.Dest, e.Orig);

            //  DPrev------>d----->RPrev
            //              ^
            //              |  
            //             e|     .P
            //              |
            //  ONext<------o<-----RNext
            // point is to the right of e
            if (t > 0)
            {
                // Switch search to the left side
                t = -t;
                e = e.Sym;
            }

            while (true)
            {
                var eo = e.ONext;
                var ed = e.DPrev;

                // Notice that we're reversing the point order
                // this tests that the point is TO THE RIGHT OF the edge
                var to = GeometryHelpers.TriArea(point, eo.Dest, eo.Orig);
                var td = GeometryHelpers.TriArea(point, ed.Dest, ed.Orig);

                // point is below ed (to the right of ed)
                if (td > 0)
                {
                    // point is to the right of eo (interior)
                    // or is e.Orig
                    if (to > 0 || (to == 0 && t == 0))
                    {
                        //  DPrev------>d----->RPrev
                        //              ^
                        //              |  
                        //    .P       e|
                        //              |
                        //  ONext<------o<-----RNext
                        startingEdge = e;
                        return e;
                    }

                    // point is below ed and below eo
                    // move the search to eo
                    //  DPrev------>d----->RPrev
                    //              ^
                    //              |  
                    //             e|
                    //              |
                    //  ONext<------o<-----RNext
                    //          .P
                    t = to;
                    e = eo;
                    continue;
                }
                
                // point is on or above ed
                // point is above eo
                if (to > 0)
                {
                    // point is e.Dest
                    if (to == 0 && t == 0)
                    {
                        startingEdge = e;
                        return e;
                    }

                    // point is above eo and above ed
                    // move the search to ed
                    //          .P
                    //  DPrev------>d----->RPrev
                    //              ^
                    //              |  
                    //             e|
                    //              |
                    //  ONext<------o<-----RNext
                    t = td;
                    e = ed;
                    continue;
                }
                
                // point is on or to the left of eo
                // point is on e, but the subdivision is to the right
                if (t == 0 && !Edge.IsLeftOf(eo.Dest, e))
                {
                    e = e.Sym;
                    continue;
                }
                
                // point is on or above ed and on or below eo (what the ...)
                // step randomly
                if ((DateTime.Now.Ticks & 1) > 0)
                {
                    t = to;
                    e = eo;
                    continue;
                }

                t = td;
                e = ed;
            }
        }

        public Edge Insert(ref Vector2 point)
        {
            Insert(ref point, null);
        }

        public Edge Insert(ref Vector2 point, Triangle tri)
        {
            var edge = (tri != null) ? Locate(ref point, tri.Anchor) : Locate(ref point);
            var startSpoke = Spoke(point, edge);

            if (startSpoke != null)
            {
                Optimize(ref point, startSpoke.Sym);
            }

            return startSpoke;
        }

        public void OverEdges(EdgeCallback callback)
        {
            OverEdges(callback, null);
        }

        public void OverEdges(EdgeCallback callback, object closure)
        {
            if (++time == 0) time = 1;
            OverEdge(startingEdge, callback, closure);
        }

        private static int time;
        private static void OverEdge(Edge edge, EdgeCallback callback, object closure)
        {
            if (edge.Token == time) return;
            
            edge.Token = time;
            edge.Sym.Token = time;
            
            callback(edge, closure);

            OverEdge(edge.ONext, callback, closure);
            OverEdge(edge.OPrev, callback, closure);
            OverEdge(edge.DNext, callback, closure);
            OverEdge(edge.DPrev, callback, closure);
        }

        public void OverFaces(FaceCallback callback)
        {
            OverFaces(callback, null);
        }

        public void OverFaces(FaceCallback callback, object closure)
        {
            var tri = firstFace;

            while( tri != null)
            {
                callback(tri, closure);
                tri = tri.Link;
            }
        }

        protected void InitMesh(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            var edgeA = MakeEdge();
            edgeA.SetEndPoints(a, b);

            var edgeB = MakeEdge();
            Edge.Splice(edgeA.Sym, edgeB);
            edgeB.SetEndPoints(b, c);

            var edgeC = MakeEdge();
            Edge.Splice(edgeB.Sym, edgeC);
            edgeB.SetEndPoints(c, d);

            var edgeD = MakeEdge();
            Edge.Splice(edgeC.Sym, edgeD);
            edgeB.SetEndPoints(d, a);
            Edge.Splice(edgeD.Sym, edgeA);

            var diag = MakeEdge();
            Edge.Splice(edgeD.Sym, diag);
            Edge.Splice(edgeB.Sym, diag.Sym);
            diag.SetEndPoints(a, c);

            startingEdge = edgeA;
            firstFace = null;

            MakeFace(edgeA.Sym).Update(this);
            MakeFace(edgeC.Sym).Update(this);
        }

        protected Edge MakeEdge()
        {
            return new Edge();
        }

        protected Edge MakeEdge(Vector2 orig, Vector2 dest)
        {
            var edge = new Edge();
            edge.SetEndPoints(orig, dest);
            return edge;
        }

        protected virtual Triangle AllocFace(Edge edge)
        {
            return new Triangle(edge);
        }

        protected Triangle MakeFace(Edge edge)
        {
            var tri = AllocFace(edge);

            firstFace = tri.LinkTo(firstFace);
            return tri;
        }

        protected void DeleteEdge(Edge edge)
        {
            Edge.Splice(edge, edge.OPrev);
            Edge.Splice(edge.Sym, edge.Sym.OPrev);
        }

        protected Edge Connect(Edge a, Edge b)
        {
            var edge = MakeEdge();
            Edge.Splice(edge, a.LNext);
            Edge.Splice(edge.Sym, b);
            edge.SetEndPoints(a.Dest, b.Orig);

            return edge;
        }

        protected void Swap(Edge edge)
        {
            var face1 = edge.LFace;
            var face2 = edge.Sym.LFace;

            var edgeA = edge.OPrev;
            var edgeB = edge.Sym.OPrev;

            Edge.Splice(edge, edgeA);
            Edge.Splice(edge.Sym, edgeB);
            Edge.Splice(edge, edgeA.LNext);
            Edge.Splice(edge.Sym, edgeB.LNext);
            edge.SetEndPoints(edgeA.Dest, edgeB.Dest);

            face1.Reshape(edge);
            face2.Reshape(edge.Sym);
        }

        protected bool CCWBoundary(Edge edge)
        {
            return !Edge.IsRightOf(edge.OPrev.Dest, edge);
        }

        protected bool OnEdge(Vector2 point, Edge edge)
        {
            const float eps2 = GeometryHelpers.EpsilonSquared;

            var t1 = (point - edge.Orig).LengthSquared();
            var t2 = (point - edge.Dest).LengthSquared();
            if (t1 < eps2 || t2 < eps2) return true;

            
            var t3 = (edge.Orig - edge.Dest).LengthSquared();
            if (t1 > t3 || t2 > t3) return false;

            var line = new Line(edge.Orig, edge.Dest);
            var result = line.Eval(ref point);

            if (result < 0) return ((-result) < eps2);
            return (result < eps2);
        }
    }
}
