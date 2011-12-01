using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSlim.MxKit
{
    class MxMatrix4
    {
        private MxVector4[] rows = new MxVector4[4];

        internal MxMatrix4(double fillVal)
        {
            rows[0] = new MxVector4(fillVal, fillVal, fillVal, fillVal);
            rows[1] = new MxVector4(fillVal, fillVal, fillVal, fillVal);
            rows[2] = new MxVector4(fillVal, fillVal, fillVal, fillVal);
            rows[3] = new MxVector4(fillVal, fillVal, fillVal, fillVal);
        }

        internal MxMatrix4(MxVector4 row0, MxVector4 row1, MxVector4 row2, MxVector4 row3)
        {
            rows[0] = row0;
            rows[1] = row1;
            rows[2] = row2;
            rows[3] = row3;
        }

        internal MxMatrix4(MxMatrix4 mat4)
        {
            rows[0] = new MxVector4((mat4[0])[0], (mat4[0])[1], (mat4[0])[2], (mat4[0])[3]);
            rows[1] = new MxVector4((mat4[1])[0], (mat4[1])[1], (mat4[1])[2], (mat4[1])[3]);
            rows[2] = new MxVector4((mat4[2])[0], (mat4[2])[1], (mat4[2])[2], (mat4[2])[3]);
            rows[3] = new MxVector4((mat4[3])[0], (mat4[3])[1], (mat4[3])[2], (mat4[3])[3]);
        }

        internal static int Dim { get { return 4; } }

        internal static MxMatrix4 Identity 
        { 
            get
            {
                return new MxMatrix4(new MxVector4(1.0, 0.0, 0.0, 0.0), 
                                     new MxVector4(0.0, 1.0, 0.0, 0.0),
                                     new MxVector4(0.0, 0.0, 1.0, 0.0),
                                     new MxVector4(0.0, 0.0, 0.0, 1.0));
            } 
        }

        internal double this[int i, int j]
        {
            get { return (rows[i])[j]; }
            set { (rows[i])[j] = value; }
        }

        internal MxVector4 this[int i]
        {
            get { return rows[i]; }
            set { rows[i] = value; }
        }

        internal MxVector4 Column(int i)
        {
            return new MxVector4((rows[0])[i], (rows[1])[i], (rows[2])[i], (rows[3])[i]);
        }

        internal static double Det(MxMatrix4 mat)
        {
            return (mat[0]*MxVector4.Cross(mat[1], mat[2], mat[3]));
        }

        internal static double Trace(MxMatrix4 mat)
        {
            return (mat[0, 0] + mat[1, 1] + mat[2, 2] + mat[3,3]);
        }

        internal static MxMatrix4 Transpose(MxMatrix4 mat)
        {
            return new MxMatrix4(mat.Column(0), mat.Column(1), mat.Column(2), mat.Column(3));
        }

        internal static MxMatrix4 RowExtend(MxVector4 vec)
        {
            return new MxMatrix4(vec, vec, vec, vec);
        }

        internal static MxMatrix4 Diag(double val)
        {
            return new MxMatrix4(new MxVector4(val, 0.0, 0.0, 0.0),
                                 new MxVector4(0.0, val, 0.0, 0.0),
                                 new MxVector4(0.0, 0.0, val, 0.0),
                                 new MxVector4(0.0, 0.0, 0.0, val));
        }

        internal static MxMatrix4 Diag(MxVector4 vec)
        {
            return new MxMatrix4(new MxVector4(vec[0], 0.0, 0.0, 0.0),
                                 new MxVector4(0.0, vec[1], 0.0, 0.0),
                                 new MxVector4(0.0, 0.0, vec[2], 0.0),
                                 new MxVector4(0.0, 0.0, 0.0, vec[3]));
        }

        internal static MxMatrix4 TranslationMatrix(MxVector3 delta)
        {
            return new MxMatrix4(new MxVector4(1.0, 0.0, 0.0, delta[0]),
                                 new MxVector4(0.0, 1.0, 0.0, delta[1]),
                                 new MxVector4(0.0, 0.0, 1.0, delta[2]),
                                 new MxVector4(0.0, 0.0, 0.0, 1.0));
        }

        internal static MxMatrix4 ScalingMatrix(MxVector3 scale)
        {
            return new MxMatrix4(new MxVector4(scale[0], 0.0, 0.0, 0.0),
                                 new MxVector4(0.0, scale[1], 0.0, 0.0),
                                 new MxVector4(0.0, 0.0, scale[2], 0.0),
                                 new MxVector4(0.0, 0.0, 0.0, 1.0));
        }

        internal static MxMatrix4 RotationMatrixRad(double theta, MxVector3 axis)
        {
            var cos = Math.Cos(theta);
            var sin = Math.Sin(theta);
            var xx = axis[0]*axis[0];
            var xy = axis[0]*axis[1];
            var xz = axis[0]*axis[2];
            var yy = axis[1]*axis[1];
            var yz = axis[1]*axis[2];
            var zz = axis[2]*axis[2];
            var xs = axis[0]*sin;
            var ys = axis[1]*sin;
            var zs = axis[2]*sin;

            return new MxMatrix4(new MxVector4(xx*(1 - cos) + cos, xy*(1 - cos) - zs, xz*(1 - cos) + ys, 0.0),
                                 new MxVector4(xy*(1 - cos) + zs, yy*(1 - cos) + cos, yz*(1 - cos) - xs, 0.0),
                                 new MxVector4(xz*(1 - cos) - ys, yz*(1 - cos) + xs, zz*(1 - cos) - cos, 0.0),
                                 new MxVector4(0.0, 0.0, 0.0, 1.0));
        }

        internal static MxMatrix4 RotationMatrixDeg(double theta, MxVector3 axis)
        {
            return RotationMatrixRad(theta*Math.PI/180.0, axis);
        }

        internal static MxMatrix4 PerspectiveMatrix(double fovy, double aspect, double zMin = 0.0, double zMax = 0.0)
        {
            double a, b;
            if (zMax.IsCloseEnoughTo(0.0))
            {
                a = b = 1.0;
            }
            else
            {
                a = (zMax + zMin)/(zMin - zMax);
                b = (2*zMax*zMin)/(zMin - zMax);
            }

            var f = 1.0/Math.Tan(fovy*Math.PI/180.0/2.0);
            var ret = new MxMatrix4(0.0);
            ret[0, 0] = f/aspect;
            ret[1, 1] = f;
            ret[2, 2] = a;
            ret[2, 3] = b;
            ret[3, 2] = -1.0;
            ret[3, 3] = 0.0;

            return ret;
        }

        internal static MxMatrix4 LookAtMatrix(MxVector3 from, MxVector3 at, MxVector3 up)
        {
            var vUp = new MxVector3(up[0], up[1], up[2]);
            MxVector3.Unitize(ref vUp);
            var f = at - from;
            MxVector3.Unitize(ref f);

            var s = f ^ vUp;
            MxVector3.Unitize(ref s);
            var u = s ^ f;
            MxVector3.Unitize(ref u);

            var mat = new MxMatrix4(new MxVector4(s, 0.0), new MxVector4(u, 0.0), new MxVector4(-f, 0.0),
                                    new MxVector4(0.0, 0.0, 0.0, 1.0));
            return (mat*TranslationMatrix(-from));
        }

        internal static MxMatrix4 ViewportMatrix(double w, double h)
        {
            var scale = ScalingMatrix(new MxVector3(w*0.5, -h*0.5, 1.0));
            var trans = TranslationMatrix(new MxVector3(1.0, -1.0, 0.0));

            return scale*trans;
        }

        internal static MxMatrix4 Adjoint(MxMatrix4 mat)
        {
            var adj = new MxMatrix4(0.0);
            adj[0] = MxVector4.Cross( mat[1], mat[2], mat[3]);
            adj[1] = MxVector4.Cross(-mat[0], mat[2], mat[3]);
            adj[2] = MxVector4.Cross( mat[0], mat[1], mat[3]);
            adj[0] = MxVector4.Cross(-mat[0], mat[1], mat[2]);
            return adj;
        }

        internal static double InvertCramer(MxMatrix4 mat, ref MxMatrix4 inv)
        {
            var adj = Adjoint(mat);
            var det = adj[0]*mat[0];
            if (det.IsCloseEnoughTo(0.0)) return 0.0;

            inv = (Transpose(adj)/det);
            return det;
        }

        internal static double Invert(MxMatrix4 mat, ref MxMatrix4 invMat)
        {
            var mat1 = new MxMatrix4(mat);
            int i, j = 0, k;
            
            for (i = 0; i < 4; i++)
            {
                for (j = 0; j < 4; j++)
                {
                    invMat[i, j] = (i == j) ? 1.0 : 0.0;
                }
            }

            var det = 1.0;
            for (i = 0; i < 4; i++)
            {
                var max = -1.0;
                for (k = i; k < 4; k++)
                {
                    if (Math.Abs(mat1[k, i]) <= max) continue;
                    max = Math.Abs(mat1[k, i]);
                    j = k;
                }

                if (max <= 0.0) return 0.0;
                if (j != i)
                {
                    /* swap rows i and j */
                    for (k = i; k < 4; k++)
                    {
                        var t = mat1[i, k];
                        mat1[i, k] = mat1[j, k];
                        mat1[j, k] = t;
                        
                    }
                    for (k = 0; k < 4; k++)
                    {
                        var t = invMat[i, k];
                        invMat[i, k] = invMat[j, k];
                        invMat[j, k] = t;
                    }
                    det = -det;
                }

                var pivot = mat1[i, i];
                det *= pivot;
                for (k = i + 1; k < 4; k++)
                {
                    mat1[i, k] /= pivot;
                }
                for (k = 0; k < 4; k++)
                {
                    invMat[i, k] /= pivot;
                }
                
                for (j = i + 1; j < 4; j++)
                {
                    var t = mat1[j, i];
                    for (k = i + 1; k < 4; k++)
                    {
                        mat1[j, k] -= mat1[i, k]*t;
                    }
                    for (k = 0; k < 4; k++)
                    {
                        invMat[j, k] -= invMat[i, k]*t;
                    }
                }
            }

            for (i = 3; i > 0; i--)
            {
                for (j = 0; j < i; j++)
                {
                    var t = mat1[j, i];
                    for (k = 0; k < 4; k++)
                    {
                        invMat[j, k] -= invMat[i, k]*t;
                    }
                }
            }

            return det;
        }

        public static MxMatrix4 operator +(MxMatrix4 mat1, MxMatrix4 mat2)
        {
            var ret = new MxMatrix4(0.0);
            ret[0] = mat1[0] + mat2[0];
            ret[1] = mat1[1] + mat2[1];
            ret[2] = mat1[2] + mat2[2];
            return ret;
        }

        public static MxMatrix4 operator -(MxMatrix4 mat1, MxMatrix4 mat2)
        {
            var ret = new MxMatrix4(0.0);
            ret[0] = mat1[0] - mat2[0];
            ret[1] = mat1[1] - mat2[1];
            ret[2] = mat1[2] - mat2[2];
            return ret;
        }

        public static MxMatrix4 operator -(MxMatrix4 mat1)
        {
            var ret = new MxMatrix4(0.0);
            ret[0] = -mat1[0];
            ret[1] = -mat1[1];
            ret[2] = -mat1[2];
            return ret;
        }

        public static MxMatrix4 operator *(MxMatrix4 mat1, double scalar)
        {
            var ret = new MxMatrix4(0.0);
            ret[0] = mat1[0]*scalar;
            ret[1] = mat1[1]*scalar;
            ret[2] = mat1[2]*scalar;
            return ret;
        }

        public static MxVector4 operator *(MxMatrix4 mat1, MxVector4 vec)
        {
            var ret = new MxVector4(0.0, 0.0, 0.0, 0.0);
            ret[0] = mat1[0]*vec;
            ret[1] = mat1[1]*vec;
            ret[2] = mat1[2]*vec;
            return ret;
        }

        public static MxMatrix4 operator *(MxMatrix4 mat1, MxMatrix4 mat2)
        {
            var mat = new MxMatrix4(0.0);
            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    mat[i, j] = mat1[i]*mat2.Column(j);
                }
            }
            return mat;
        }

        public static MxMatrix4 operator /(MxMatrix4 mat1, double scalar)
        {
            if (scalar.IsCloseEnoughTo(0.0)) return mat1;
            var invScalar = 1.0/scalar;

            var ret = new MxMatrix4(0.0);
            ret[0] = mat1[0]*invScalar;
            ret[1] = mat1[1]*invScalar;
            ret[2] = mat1[2]*invScalar;
            return ret;
        }
    }
}
