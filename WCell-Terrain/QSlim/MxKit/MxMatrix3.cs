using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSlim.MxKit
{
    internal class MxMatrix3
    {
        private MxVector3[] rows = new MxVector3[3];

        internal MxMatrix3(double fillVal)
        {
            rows[0] = new MxVector3(fillVal, fillVal, fillVal);
            rows[1] = new MxVector3(fillVal, fillVal, fillVal);
            rows[2] = new MxVector3(fillVal, fillVal, fillVal);
        }

        internal MxMatrix3(MxVector3 row0, MxVector3 row1, MxVector3 row2)
        {
            rows[0] = row0;
            rows[1] = row1;
            rows[2] = row2;
        }

        internal MxMatrix3(MxMatrix3 mat3)
        {
            rows[0] = new MxVector3((mat3[0])[0], (mat3[0])[1], (mat3[0])[2]);
            rows[1] = new MxVector3((mat3[1])[0], (mat3[1])[1], (mat3[1])[2]);
            rows[2] = new MxVector3((mat3[2])[0], (mat3[2])[1], (mat3[2])[2]);
        }

        internal static int Dim { get { return 3; } }

        internal static MxMatrix3 Identity 
        { 
            get
            {
                return new MxMatrix3(new MxVector3(1.0, 0.0, 0.0), 
                                     new MxVector3(0.0, 1.0, 0.0),
                                     new MxVector3(0.0, 0.0, 1.0));
            } 
        }

        internal double this[int i, int j]
        {
            get { return (rows[i])[j]; }
            set { (rows[i])[j] = value; }
        }

        internal MxVector3 this[int i]
        {
            get { return rows[i]; }
            set { rows[i] = value; }
        }

        internal MxVector3 Column(int i)
        {
            return new MxVector3((rows[0])[i], (rows[1])[i], (rows[2])[i]);
        }

        internal static double Det(MxMatrix3 mat)
        {
            return (mat[0]*(mat[1] ^ mat[2]));
        }

        internal static double Trace(MxMatrix3 mat)
        {
            return (mat[0, 0] + mat[1, 1] + mat[2, 2]);
        }

        internal static MxMatrix3 Transpose(MxMatrix3 mat)
        {
            return new MxMatrix3(mat.Column(0), mat.Column(1), mat.Column(2));
        }

        internal static MxMatrix3 RowExtend(MxVector3 vec)
        {
            return new MxMatrix3(vec, vec, vec);
        }

        internal static MxMatrix3 Diag(double val)
        {
            return new MxMatrix3(new MxVector3(val, 0.0, 0.0),
                                 new MxVector3(0.0, val, 0.0),
                                 new MxVector3(0.0, 0.0, val));
        }

        internal static MxMatrix3 Diag(MxVector3 vec)
        {
            return new MxMatrix3(new MxVector3(vec[0], 0.0, 0.0),
                                 new MxVector3(0.0, vec[1], 0.0),
                                 new MxVector3(0.0, 0.0, vec[2]));
        }

        internal static MxMatrix3 OuterProduct(MxVector3 vec)
        {
            var mat = new MxMatrix3(0.0);
            var x = vec[0];
            var y = vec[1];
            var z = vec[2];

            mat[0, 0] = x*x;
            mat[0, 1] = x*y;
            mat[0, 2] = x*z;
            mat[1, 0] = mat[0, 1];
            mat[1, 1] = y*y;
            mat[1, 2] = y*z;
            mat[2, 0] = mat[0, 2];
            mat[2, 1] = mat[1, 2];
            mat[2, 2] = z*z;

            return mat;
        }

        internal static MxMatrix3 OuterProduct(MxVector3 vec1, MxVector3 vec2)
        {
            var mat = new MxMatrix3(0.0);
            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    mat[i, j] = vec1[i]*vec2[j];
                }
            }
            return mat;
        }

        internal static MxMatrix3 Adjoint(MxMatrix3 mat)
        {
            return new MxMatrix3(mat[1] ^ mat[2],
                                 mat[2] ^ mat[0],
                                 mat[0] ^ mat[1]);
        }

        internal static double Invert(MxMatrix3 mat, ref MxMatrix3 inv)
        {
            var adj = Adjoint(mat);
            var det = adj[0]*mat[0];
            if (det.IsCloseEnoughTo(0.0)) return 0.0;

            inv = (Transpose(adj)/det);
            return det;
        }

        public static MxMatrix3 operator +(MxMatrix3 mat1, MxMatrix3 mat2)
        {
            var ret = new MxMatrix3(0.0);
            ret[0] = mat1[0] + mat2[0];
            ret[1] = mat1[1] + mat2[1];
            ret[2] = mat1[2] + mat2[2];
            return ret;
        }

        public static MxMatrix3 operator -(MxMatrix3 mat1, MxMatrix3 mat2)
        {
            var ret = new MxMatrix3(0.0);
            ret[0] = mat1[0] - mat2[0];
            ret[1] = mat1[1] - mat2[1];
            ret[2] = mat1[2] - mat2[2];
            return ret;
        }

        public static MxMatrix3 operator -(MxMatrix3 mat1)
        {
            var ret = new MxMatrix3(0.0);
            ret[0] = -mat1[0];
            ret[1] = -mat1[1];
            ret[2] = -mat1[2];
            return ret;
        }

        public static MxMatrix3 operator *(MxMatrix3 mat1, double scalar)
        {
            var ret = new MxMatrix3(0.0);
            ret[0] = mat1[0]*scalar;
            ret[1] = mat1[1]*scalar;
            ret[2] = mat1[2]*scalar;
            return ret;
        }

        public static MxVector3 operator *(MxMatrix3 mat1, MxVector3 vec)
        {
            var ret = new MxVector3(0.0, 0.0, 0.0);
            ret[0] = mat1[0]*vec;
            ret[1] = mat1[1]*vec;
            ret[2] = mat1[2]*vec;
            return ret;
        }

        public static MxMatrix3 operator *(MxMatrix3 mat1, MxMatrix3 mat2)
        {
            var mat = new MxMatrix3(0.0);
            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    mat[i, j] = mat1[i]*mat2.Column(j);
                }
            }
            return mat;
        }

        public static MxMatrix3 operator /(MxMatrix3 mat1, double scalar)
        {
            if (scalar.IsCloseEnoughTo(0.0)) return mat1;
            var invScalar = 1.0/scalar;

            var ret = new MxMatrix3(0.0);
            ret[0] = mat1[0]*invScalar;
            ret[1] = mat1[1]*invScalar;
            ret[2] = mat1[2]*invScalar;
            return ret;
        }
    }
}
