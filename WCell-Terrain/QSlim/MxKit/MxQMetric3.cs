using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSlim.MxKit
{
    internal class MxQuadric3
    {
        private double a2, ab, ac, ad;
        private double     b2, bc, bd;
        private double         c2, cd;
        private double             d2;

        private double r;


        public MxQuadric3()
        {
            Clear();
        }

        internal MxQuadric3(double a, double b, double c, double d, double area)
        {
            Init(a, b, c, d, area);
        }

        internal MxQuadric3(double[] n, double d, double area = 1.0)
            : this(n[0], n[1], n[2], d, area)
        {
        }

        internal MxQuadric3(MxVector3 vec, double d, double area = 1.0) 
            : this(vec[0], vec[1], vec[2], d, area)
        {
        }

        internal MxQuadric3(MxQuadric3 quad)
        {
            Coefficients = quad.Coefficients;
            Area = quad.Area;
        }


        internal MxMatrix3 Tensor
        {
            get
            {
                return new MxMatrix3(new MxVector3(a2, ab, ac),
                                     new MxVector3(ab, b2, bc),
                                     new MxVector3(ac, bc, c2));
            }
        }

        internal MxVector3 Vector { get { return new MxVector3(ad, bd, cd); } }

        internal double Offset { get { return d2; } }

        internal double Area
        {
            get { return r; } 
            set { r = value; }
        }

        internal MxMatrix4 Homogenous
        {
            get
            {
                return new MxMatrix4(new MxVector4(a2, ab, ac, ad),
                                     new MxVector4(ab, b2, bc, bd),
                                     new MxVector4(ac, bc, c2, cd),
                                     new MxVector4(ad, bd, cd, d2));
            }
        }

        internal double[] Coefficients
        {
            get { return new[] {a2, ab, ac, ad, b2, bc, bd, c2, cd, d2}; }
            set 
            { 
                a2 = value[0]; ab = value[1]; ac = value[2]; ad = value[3];
                               b2 = value[4]; bc = value[5]; bd = value[6];
                                              c2 = value[7]; cd = value[8];
                                                             d2 = value[9];
            }
        }

        
        internal void Clear(double fillVal = 0.0)
        {
            a2 = ab = ac = ad = fillVal;
                 b2 = bc = bd = fillVal;
                      c2 = cd = fillVal;
                           d2 = fillVal;
                            r = fillVal;
        }

        internal double Evaluate(double x, double y, double z)
        {
            var sum  = x*x*a2 + 2*x*y*ab + 2*x*z*ac + 2*x*ad;
                sum +=            y*y*b2 + 2*y*z*bc + 2*y*bd;
                sum +=                       z*z*c2 + 2*z*cd;
            return sum + d2;
        }

        internal double Evaluate(double[] vec)
        {
            return Evaluate(vec[0], vec[1], vec[2]);
        }

        internal double Evaluate(MxVector3 vec)
        {
            return Evaluate(vec[0], vec[1], vec[2]);
        }

        internal void PointConstraint(double[] point)
        {
            // A = Identity
            a2 = b2 = c2 = 1.0;
            ab = ac = bc = 0.0;
            
            // b = -point
            ad = -point[0];
            bd = -point[1];
            cd = -point[2];

            // c = p*p
            d2 = point[0]*point[0] + point[1]*point[1] + point[2]*point[2];
        }

        internal MxQuadric3 Transform(MxMatrix4 mat)
        {
            var matQ = Homogenous;
            var adj = MxMatrix4.Adjoint(mat);

            matQ = (adj*matQ)*adj;

            Init(matQ, r);
            return this;
        }

        internal bool Optimize(ref MxVector3 vec)
        {
            var invMat = new MxMatrix3(0.0);
            var det = MxMatrix3.Invert(Tensor, ref invMat);
            if (det.IsSqCloseEnoughTo(0.0)) return false;
            
            vec = -(invMat*Vector);
            return true;
        }

        internal bool Optimize(ref double x, ref double y, ref double z)
        {
            var vec = new MxVector3(0.0, 0.0, 0.0);
            var success = Optimize(ref vec);
            if (!success) return false;

            x = vec[0];
            y = vec[1];
            z = vec[2];
            return true;
        }

        internal bool Optimize(MxVector3 vec1, MxVector3 vec2, ref MxVector3 opt)
        {
            var diff = vec1 - vec2;
            var tens = Tensor;

            var tV2 = tens*vec2;
            var tDiff = tens*diff;

            var denom = 2.0*diff*tDiff;
            if (denom.IsSqCloseEnoughTo(0.0)) return false;

            var a = (-2.0*(Vector*diff) - (diff*tV2) - (vec2*tDiff))/(2.0*(diff*tDiff));
            if (a < 0.0) a = 0.0;
            else if (a > 1.0) a = 1.0;

            opt = a*diff + vec2;
            return true;
        }

        internal bool Optimize(MxVector3 vec1, MxVector3 vec2, MxVector3 vec3, ref MxVector3 opt)
        {
            var d13 = vec1 - vec3;
            var d23 = vec2 - vec3;
            var A = Tensor;
            var B = Vector;

            var Ad13 = A*d13;
            var Ad23 = A*d23;
            var Av3 = A*vec3;

            var d13D23 = (d13*Ad23) + (d23*Ad13);
            var v3D13 = (d13*Av3) + (vec3*Ad13);
            var v3D23 = (d23*Av3) + (vec3*Ad23);

            var d23AD23 = d23*Ad23;
            var d13AD13 = d13*Ad13;

            var denom = d13AD13*d23AD23 - 2.0*d13D23;
            if (denom.IsSqCloseEnoughTo(0.0)) return false;

            var a = ((d23AD23*(2.0*(B*d13) + v3D13)) -
                     (d13D23*(2.0*(B*d23) + v3D23)))/-denom;
            var b = ((d13AD13*(2.0*(B*d13) + v3D23)) - (d13D23*(2.0*(B*d13) + v3D13)))/-denom;

            if (a < 0.0) a = 0.0;
            else if (a > 1.0) a = 1.0;

            if (b < 0.0) b = 0.0;
            else if (b > 1.0) b = 1.0;

            opt = a*d13 + b*d23 + vec3;
            return true;
        }

        public static MxQuadric3 operator +(MxQuadric3 quad1, MxQuadric3 quad2)
        {
            var coef1 = quad1.Coefficients;
            var coef2 = quad2.Coefficients;
            var newArea = quad1.Area + quad2.Area;
            var newCoeffs = new double[10];
            for(var i = 0; i < 10; i++)
            {
                newCoeffs[i] = coef1[i] + coef2[i];
            }

            var newQuad = new MxQuadric3
            {
                Coefficients = newCoeffs,
                Area = newArea
            };
            return newQuad;
        }

        public static MxQuadric3 operator -(MxQuadric3 quad1, MxQuadric3 quad2)
        {
            var coef1 = quad1.Coefficients;
            var coef2 = quad2.Coefficients;
            var newArea = quad1.Area - quad2.Area;
            var newCoeffs = new double[10];
            for (var i = 0; i < 10; i++)
            {
                newCoeffs[i] = coef1[i] - coef2[i];
            }

            var newQuad = new MxQuadric3
            {
                Coefficients = newCoeffs,
                Area = newArea
            };
            return newQuad;
        }

        public static MxQuadric3 operator *(MxQuadric3 quad1, double scalar)
        {
            var coef1 = quad1.Coefficients;
            var newCoeffs = new double[10];
            for (var i = 0; i < 10; i++)
            {
                newCoeffs[i] = coef1[i]*scalar;
            }

            var newQuad = new MxQuadric3
            {
                Coefficients = newCoeffs,
                Area = quad1.Area
            };
            return newQuad;
        }

        private void Init(double a, double b, double c, double d, double area)
        {
            a2 = a*a; ab = a*b; ac = a*c; ad = a*d;
                      b2 = b*b; bc = b*c; bd = b*d; 
                                c2 = c*c; cd = c*d;
                                          d2 = d*d;
            r = area;
        }

        private void Init(MxMatrix4 mat, double area)
        {
            a2 = mat[0, 0]; ab = mat[0, 1]; ac = mat[0, 2]; ad = mat[0, 3];
                            b2 = mat[1, 1]; bc = mat[1, 2]; bd = mat[1, 3];
                                            c2 = mat[2, 2]; cd = mat[2, 3];
                                                            d2 = mat[3, 3];
            r = area;
        }
    }
}
