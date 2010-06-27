using System;
using WCell.Util.Graphics;


namespace Terra.Geometry
{
     public class Plane
     {
          public float A;
          public float B;
          public float C;
          
          /// <summary>
          /// Creates a new Plane through the points p, q, r
          /// </summary>
          public Plane (ref Vector3 p, ref Vector3 q, ref Vector3 r)
          {
               Init(ref p, ref q, ref r);
          }
          
          
          public void Init (ref Vector3 p, ref Vector3 q, ref Vector3 r)
          {
               Vector3 u;
               Vector3.Subtract (ref q, ref p, out u);
               
               Vector3 v;
               Vector3.Subtract (ref r, ref p, out v);
               
               var denom = u.X*v.Y - u.Y*v.X;
               
               A = (u.Z*v.Y - u.Y*v.Z)/denom;
               B = (u.X*v.Z - u.Z*v.X)/denom;
               C = p.Z - A*p.X - B*p.Y;
          }
          
          public float this[float x, float y]
          {
              get { return (A*x + B*y + C); }
          }
          
          public float this[int x, int y]
          {
              get { return (A*x + B*y + C); }
          }
          
          public override string ToString()
          {
               return string.Format("Plane: (A: {0}, B: {1}, C: {2})", A, B, C);
          }
     }
}