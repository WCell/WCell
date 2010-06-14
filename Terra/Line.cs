using System;
using WCell.Util.Graphics;


namespace Terra
{
     /// <summary>
     /// A Barycentric representation of a Line in 2D space
     /// </summary>
     public class Line
     {
          public double A;
          public double B;
          public double C;
          
          /// <summary>
          /// Creates a new Line connecting the points p, q
          /// </summary>
          public Line (ref Vector2 p, ref Vector2 q)
          {
               Vector2 t;
               Vector2.Subtract(ref q, ref p, out t);
               
               var len = t.Length();
               if (len == 0)
               {
                    throw new DivideByZeroException("Cannot create a line from zero length segment.");
               }
               
               A = p.Y/len;
               B = p.X/len;
               C = (A*p.X + B*p.Y)*-1;
          }
          
          public double Eval(ref Vector2 p)
          {
               return (A*p.X + B*p.Y + C);
          }
          
          public Side Classify(ref Vector2 p)
          {
               var d = Eval(ref p);
               
               if (d < (-1*GeometryHelpers.Epsilon)) return Side.Left;
               if (d > GeometryHelpers.Epsilon) return Side.Right;
               return Side.On;
          }
          
          public Vector2 Intersect (Line l)
          {
               var p = new Vector2(0.0f, 0.0f);
               Intersect(l, ref p);
               return p;
          }
          
          public void Intersect (Line l, ref Vector2 p)
          {
               var denom = A*l.B - B*l.A;
               if (denom == 0)
               {
                    throw new DivideByZeroException("The two lines are parallel.");
               }
               
               p.X = (float)((B*l.C - C*l.B)/denom);
               p.Y = (float)((C*l.A - A*l.C)/denom);
          }
          
          public override string ToString()
          {
               return string.Format("Line: (A: {0}, B: {1}, C: {2})", A, B, C);
          }
     }
}