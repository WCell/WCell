using System;
using System.Diagnostics;

namespace QSlim.MxKit
{
    internal class MxMatrix : MxBlock2<double>
    {
        private const int MaxRotations = 60;
        private const double EPSILON = 1e-6;

        internal MxMatrix(int n)
            : base(n, n)
        {

        }

        internal MxMatrix(MxMatrix mat)
            : base(mat.Dim, mat.Dim)
        {
            Copy(mat);
        }

        internal int Dim
        {
            get { return Columns; }
        }

        internal bool Jacobi(MxMatrix input, ref double[] eigenValues, ref double[] eigenVectors)
        {
            var a = new[] {new double[3], new double[3], new double[3]};
            var v = new[] { new double[3], new double[3], new double[3] };
            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    a[i][j] = input[i, j];
                }
            }

            var result = InternalJacobi(a, ref eigenValues, ref v);
            if (!result) return false;

            var index = 0;
            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    eigenVectors[index++] = v[j][i];
                }
            }

            return true;
        }

        public static MxMatrix operator +(MxMatrix mat1, MxMatrix mat2)
        {
            Debug.Assert(mat1.Dim == mat2.Dim);
            AddInto(ref mat1, mat2, mat1.Dim);
            return mat1;
        }

        public static MxMatrix operator -(MxMatrix mat1, MxMatrix mat2)
        {
            Debug.Assert(mat1.Dim == mat2.Dim);
            SubFrom(ref mat1, mat2, mat1.Dim);
            return mat1;
        }

        public static MxMatrix operator *(MxMatrix mat, double val)
        {
            Scale(ref mat, val, mat.Dim);
            return mat;
        }

        public static MxMatrix operator /(MxMatrix mat, double val)
        {
            InvScale(ref mat, val, mat.Dim);
            return mat;
        }

        internal static void AddInto(ref MxMatrix into, MxMatrix addend, int dim)
        {
            for(var i = 0; i < dim*dim; i++)
            {
                into[i] += addend[i];
            }
        }

        internal static void SubFrom(ref MxMatrix from, MxMatrix subtractor, int dim)
        {
            for (var i = 0; i < dim*dim; i++)
            {
                from[i] -= subtractor[i];
            }
        }

        internal static void Scale(ref MxMatrix mat, double scalar, int dim)
        {
            for (var i = 0; i < dim * dim; i++)
            {
                mat[i] *= scalar;
            }
        }

        internal static void InvScale(ref MxMatrix mat, double scalar, int dim)
        {
            var invScale = 1.0/scalar;
            for (var i = 0; i < dim * dim; i++)
            {
                mat[i] *= invScale;
            }
        }

        internal static double Invert(MxMatrix matA, MxMatrix matInvA)
        {
            var a2 = new MxMatrix(matInvA);
            return InternalInvert(matA, ref a2);
        }

        private static double InternalInvert(MxMatrix matA, ref MxMatrix matInvA)
        {
            var dim = matA.Dim;
            int i, j = 0, k;
            double max, t, det, pivot;

            //Initialize the inverse matrix as the Identity matrix
            for (i = 0; i < dim; i++)
            {
                for (j = 0; j < dim; j++)
                {
                    matInvA[i, j] = ((i == j) ? 1.0 : 0.0);
                }
            }

            det = 1.0;
            // eliminate in column i, below the diagonal
            for (i = 0; i < dim; i++)
            {
                max = -1.0;
                for (k = i; k < dim; k++)
                {
                    var abs = Math.Abs(matA[k, i]);
                    if (abs <= max) continue;

                    max = abs;
                    j = k;
                }

                if (max <= 0.0) return 0.0;
                if (j != i)
                {
                    for (k = i; k < dim; k++)
                    {
                        t = matA[i, k];
                        matA[i, k] = matA[j, k];
                        matA[j, k] = t;
                    }
                    for (k = 0; k < dim; k++)
                    {
                        t = matInvA[i, k];
                        matInvA[i, k] = matInvA[j, k];
                        matInvA[j, k] = t;
                    }
                    det = -det;
                }

                pivot = matA[i, i];
                det *= pivot;

                for (k = i+1; k<dim; k++)
                {
                    matA[i, k] /= pivot;
                }
                for (k = 0; k < dim; k++)
                {
                    matInvA[i, k] /= pivot;
                }

                for (j = i+1; j < dim; j++)
                {
                    t = matA[j, i];
                    for (k = i+1; k < dim; k++)
                    {
                        matA[j, k] -= matA[i, k]*t;
                    }
                    for (k = 0; k < dim; k++)
                    {
                        matInvA[j, k] -= matInvA[i, k]*t;
                    }
                }
            }

            for (i = (dim - 1); i > 0; i--)
            {
                for (j = 0; j < i; j++)
                {
                    t = matA[j, i];
                    for (k = 0; k < dim; k++)
                    {
                        matInvA[j, k] -= matInvA[i, k]*t;
                    }
                }
            }

            return det;
        }

        private static bool InternalJacobi(double[][] input, ref double[] eigenValues, ref double[][] eigenVectors)
        {
            int i;
            int j;
            int iq;
            int ip;

            // Initialize eigenVectors to the identity matrix.
            for (ip = 0; ip < 3; ip++)
            {
                for (iq = 0; iq < 3; iq++)
                {
                    eigenVectors[ip][iq] = 0.0;
                }
                eigenVectors[ip][ip] = 1.0;
            }

            // Initialize the eigenValues
            var b = new double[3];
            var z = new double[3];
            for (ip = 0; ip < 3; ip++)
            {
                b[ip] = eigenValues[ip] = input[ip][ip];
                z[ip] = 0.0;
            }

            // Do rotation sequence
            for (i = 0; i < MaxRotations; i++)
            {
                var sm = 0.0;
                for (ip = 0; ip < 2; ip++)
                {
                    for (iq = ip + 1; iq < 3; iq++)
                    {
                        sm += Math.Abs(input[ip][iq]);
                    }
                }
                if (sm.IsCloseEnoughTo(0.0)) break;

                var tresh = (i < 4) ? 0.2*sm/9 : 0.0;

                for (ip = 0; ip < 2; ip++)
                {
                    for (iq = 0; iq < 3; iq++)
                    {
                        var g = 100.0*Math.Abs(input[ip][iq]);
                        if ((i > 4) && 
                            ((Math.Abs(eigenValues[ip]) + g).IsCloseEnoughTo(Math.Abs(eigenValues[ip]))) &&
                            ((Math.Abs(eigenValues[iq]) + g).IsCloseEnoughTo(Math.Abs(eigenValues[iq]))))
                        {
                            input[ip][iq] = 0.0;
                        } 
                        else if (Math.Abs(input[ip][iq]) > tresh)
                        {
                            var h = eigenValues[iq] - eigenValues[ip];
                            double t;
                            if ((Math.Abs(h) + g).IsCloseEnoughTo(Math.Abs(h)))
                            {
                                t = (input[ip][iq])/h;
                            }
                            else
                            {
                                var theta = 0.5*h/(input[ip][iq]);
                                t = 1.0/(Math.Abs(theta) + Math.Sqrt(1.0 + (theta*theta)));
                                if (theta < 0.0) t = -t;
                            }

                            var c = 1.0/Math.Sqrt(1 + t*t);
                            var s = t*c;
                            var tau = s/(1.0 + c);
                            h = t*(input[ip][iq]);
                            z[ip] -= h;
                            z[iq] += h;
                            eigenValues[ip] -= h;
                            eigenValues[iq] += h;
                            input[ip][iq] = 0.0;

                            for (j = 0; j < (ip-1); j++)
                            {
                                //      i, j, k, l
                                //ROT(a,j, ip,j, iq)
                                g = input[j][ip]; 
                                h = input[j][iq];
                                input[j][ip] = g - s*(h + g*tau);
                                input[j][iq] = h + s*(g - h*tau);
                            }
                            for (j = (ip + 1); j < (iq - 1); j++)
                            {
                                //      i, j, k, l
                                //ROT(a,ip,j, j, iq)
                                g = input[ip][j]; 
                                h = input[j][iq];
                                input[ip][j] = g - s*(h + g*tau);
                                input[j][iq] = h + s*(g - h*tau);
                            }
                            for (j = (iq + 1); j < 3; j++)
                            {
                                //      i, j, k, l
                                //ROT(a,ip,j,iq,j)
                                g = input[ip][j];
                                h = input[iq][j];
                                input[ip][j] = g - s*(h + g*tau);
                                input[iq][j] = h + s*(g - h*tau);
                            }
                            for (j = 0; j < 3; j++)
                            {
                                //      i, j, k, l
                                //ROT(v,j,ip,j,iq)
                                g = input[j][ip];
                                h = input[j][iq];
                                eigenVectors[j][ip] = g - s*(h + g*tau);
                                eigenVectors[j][iq] = h + s*(g - h*tau);
                            }
                        }
                    }
                }

                for (ip = 0; ip < 3; ip++)
                {
                    b[ip] += z[ip];
                    eigenValues[ip] = b[ip];
                    z[ip] = 0.0;
                }
            }

            if (i >= MaxRotations)
            {
                return false;
            }

            // sort eigenfunctions
            for (j = 0; j < 3; j++)
            {
                var k = j;
                var tmp = eigenValues[k];
                for (i = j; i < 3; i++)
                {
                    if (eigenValues[i] < tmp) continue;

                    k = i;
                    tmp = eigenValues[k];
                }

                if (k == j) continue;

                eigenValues[k] = eigenValues[j];
                eigenValues[j] = tmp;
                for (i = 0; i < 3; i++)
                {
                    tmp = eigenVectors[i][j];
                    eigenVectors[i][j] = eigenVectors[i][k];
                    eigenVectors[i][k] = tmp;
                }
            }

            for (j = 0; j < 3; j++)
            {
                var numPos = 0;
                for (i = 0; i < 3; i++)
                {
                    if (eigenVectors[i][j] < 0.0) continue;
                    numPos++;
                }
                if (numPos >= 2) continue;
                
                for (i = 0; i < 3;i++)
                {
                    eigenVectors[i][j] *= -1.0;
                }
            }

            return true;
        }

        private static bool InternalJacobi4(double[][] input, ref double[] eigenValues, ref double[][] eigenVectors)
        {
            int i;
            int j;
            int iq;
            int ip;

            // Initialize eigenVectors to the identity matrix.
            for (ip = 0; ip < 4; ip++)
            {
                for (iq = 0; iq < 4; iq++)
                {
                    eigenVectors[ip][iq] = 0.0;
                }
                eigenVectors[ip][ip] = 1.0;
            }

            // Initialize the eigenValues
            var b = new double[4];
            var z = new double[4];
            for (ip = 0; ip < 4; ip++)
            {
                b[ip] = eigenValues[ip] = input[ip][ip];
                z[ip] = 0.0;
            }

            // Do rotation sequence
            for (i = 0; i < MaxRotations; i++)
            {
                var sm = 0.0;
                for (ip = 0; ip < 3; ip++)
                {
                    for (iq = ip + 1; iq < 4; iq++)
                    {
                        sm += Math.Abs(input[ip][iq]);
                    }
                }
                if (sm.IsCloseEnoughTo(0.0)) break;

                var tresh = (i < 4) ? 0.2*sm/16 : 0.0;

                for (ip = 0; ip < 3; ip++)
                {
                    for (iq = 0; iq < 4; iq++)
                    {
                        var g = 100.0 * Math.Abs(input[ip][iq]);
                        if ((i > 4) &&
                            ((Math.Abs(eigenValues[ip]) + g).IsCloseEnoughTo(Math.Abs(eigenValues[ip]))) &&
                            ((Math.Abs(eigenValues[iq]) + g).IsCloseEnoughTo(Math.Abs(eigenValues[iq]))))
                        {
                            input[ip][iq] = 0.0;
                        }
                        else if (Math.Abs(input[ip][iq]) > tresh)
                        {
                            var h = eigenValues[iq] - eigenValues[ip];
                            double t;
                            if ((Math.Abs(h) + g).IsCloseEnoughTo(Math.Abs(h)))
                            {
                                t = (input[ip][iq]) / h;
                            }
                            else
                            {
                                var theta = 0.5*h/(input[ip][iq]);
                                t = 1.0/(Math.Abs(theta) + Math.Sqrt(1.0 + (theta*theta)));
                                if (theta < 0.0) t = -t;
                            }

                            var c = 1.0/Math.Sqrt(1 + t*t);
                            var s = t*c;
                            var tau = s/(1.0 + c);
                            h = t*(input[ip][iq]);
                            z[ip] -= h;
                            z[iq] += h;
                            eigenValues[ip] -= h;
                            eigenValues[iq] += h;
                            input[ip][iq] = 0.0;

                            for (j = 0; j < (ip - 1); j++)
                            {
                                //      i, j, k, l
                                //ROT(a,j, ip,j, iq)
                                g = input[j][ip];
                                h = input[j][iq];
                                input[j][ip] = g - s * (h + g * tau);
                                input[j][iq] = h + s * (g - h * tau);
                            }
                            for (j = (ip + 1); j < (iq - 1); j++)
                            {
                                //      i, j, k, l
                                //ROT(a,ip,j, j, iq)
                                g = input[ip][j];
                                h = input[j][iq];
                                input[ip][j] = g - s * (h + g * tau);
                                input[j][iq] = h + s * (g - h * tau);
                            }
                            for (j = (iq + 1); j < 4; j++)
                            {
                                //      i, j, k, l
                                //ROT(a,ip,j,iq,j)
                                g = input[ip][j];
                                h = input[iq][j];
                                input[ip][j] = g - s * (h + g * tau);
                                input[iq][j] = h + s * (g - h * tau);
                            }
                            for (j = 0; j < 4; j++)
                            {
                                //      i, j, k, l
                                //ROT(v,j,ip,j,iq)
                                g = input[j][ip];
                                h = input[j][iq];
                                eigenVectors[j][ip] = g - s * (h + g * tau);
                                eigenVectors[j][iq] = h + s * (g - h * tau);
                            }
                        }
                    }
                }

                for (ip = 0; ip < 3; ip++)
                {
                    b[ip] += z[ip];
                    eigenValues[ip] = b[ip];
                    z[ip] = 0.0;
                }
            }

            if (i >= MaxRotations)
            {
                return false;
            }

            // sort eigenfunctions
            for (j = 0; j < 3; j++)
            {
                var k = j;
                var tmp = eigenValues[k];
                for (i = j; i < 3; i++)
                {
                    if (eigenValues[i] < tmp) continue;

                    k = i;
                    tmp = eigenValues[k];
                }

                if (k == j) continue;

                eigenValues[k] = eigenValues[j];
                eigenValues[j] = tmp;
                for (i = 0; i < 3; i++)
                {
                    tmp = eigenVectors[i][j];
                    eigenVectors[i][j] = eigenVectors[i][k];
                    eigenVectors[i][k] = tmp;
                }
            }

            for (j = 0; j < 3; j++)
            {
                var numPos = 0;
                for (i = 0; i < 3; i++)
                {
                    if (eigenVectors[i][j] < 0.0) continue;
                    numPos++;
                }
                if (numPos >= 2) continue;

                for (i = 0; i < 3; i++)
                {
                    eigenVectors[i][j] *= -1.0;
                }
            }

            return true;
        }
    }
}
