namespace TwoPhaseAlgorithmSolver
{
    using System;
    using System.Collections.Generic;

    public class CoordCube
    {
        public const short N_CORNER = 8;
        public const short N_EDGE = 12;
        public static byte[] Empty8x => new byte[N_CORNER];

        public static byte[] Empty12x => new byte[N_EDGE];

        public CoordCube()
      : this(FromInversions(Empty8x), FromInversions(Empty12x), Empty8x, Empty12x)
        {
        }

        public CoordCube(byte[] cp, byte[] ep, byte[] co, byte[] eo)
        {
            this.EdgePermutation = ep;
            this.eo = eo;
            this.CornerPermutation = cp;
            this.co = co;
        }

        #region Main data and coordinates

        // Main data
        private byte[] co;

        private byte[] eo;

        public short Twist
        {
            get { return this.GetTwist(); }
            set
            {
                this.SetTwist(value);
            }
        }

        public short Flip
        {
            get { return this.GetFlip(); }
            set
            {
                this.SetFlip(value);
            }
        }

        public short FRtoBR
        {
            get { return this.GetFRtoBR(); }
            set
            {
                this.SetFRtoBR(value);
            }
        }

        public short URFtoDLF
        {
            get { return this.GetURFtoDLF(); }
            set
            {
                this.SetURFtoDLF(value);
            }
        }

        public short Parity => this.GetCornerParity();

        public short URtoUL
        {
            get { return this.GetURtoUL(); }
            set
            {
                this.SetURtoUL(value);
            }
        }

        public short UBtoDF
        {
            get { return this.GetUBtoDF(); }
            set
            {
                this.SetUBtoDF(value);
            }
        }

        public int URtoDF
        {
            get { return this.GetURtoDF(); }
            set
            {
                this.SetURtoDF(value);
            }
        }

        public int URFtoDRB
        {
            get { return this.GetURFtoDLB(); }
            set
            {
                this.SetURFtoDLB(value);
            }
        }

        public int URtoBR
        {
            get { return this.GetURtoBR(); }
            set
            {
                this.SetURtoBR(value);
            }
        }

        public byte[] CornerPermutation { get; set; }

        public byte[] EdgePermutation { get; set; }

        #endregion Main data and coordinates

        public CoordCube DeepClone()
        {
            var newCubie = new CoordCube
            {
                co = this.co,
                CornerPermutation = this.CornerPermutation,
                eo = this.eo,
                EdgePermutation = this.EdgePermutation
            };
            return newCubie;
        }

        #region Coordinate conversions

        private short GetTwist()
        {
            short res = 0;
            for (var i = 0; i < N_CORNER; i++)
                res += (short)(this.co[i] * Math.Pow(3, N_CORNER - (i + 2)));
            return res;
        }

        private void SetTwist(short twist)
        {
            var sum = 0;
            for (var i = 0; i < N_CORNER - 1; i++)
            {
                var divisor = (int)Math.Pow(3, N_CORNER - (i + 2));
                this.co[i] = (byte)(twist / divisor);
                sum += twist / divisor;
                twist = (short)(twist % divisor);
            }
            this.co[N_CORNER - 1] = (byte)((3 - sum % 3) % 3);
        }

        private short GetFlip()
        {
            short res = 0;
            for (var i = 0; i < N_EDGE; i++)
                res += (short)(this.eo[i] * Math.Pow(2, N_EDGE - (i + 2)));
            return res;
        }

        private void SetFlip(short flip)
        {
            var sum = 0;
            for (var i = 0; i < N_EDGE - 1; i++)
            {
                var divisor = (int)Math.Pow(2, N_EDGE - (i + 2));
                this.eo[i] = (byte)(flip / divisor);
                sum += flip / divisor;
                flip = (short)(flip % divisor);
            }
            this.eo[N_EDGE - 1] = (byte)((2 - sum % 2) % 2);
        }

        private short GetCornerParity()
        {
            var s = 0;
            for (var i = 7; i > 0; i--)
                for (var j = i - 1; j >= 0; j--)
                    if (this.CornerPermutation[j] > this.CornerPermutation[i])
                        s++;
            return (short)(s % 2);
        }

        private static void RotateLeft(IList<byte> arr, int l, int r)
        {
            var temp = arr[l];
            for (var i = l; i < r; i++)
                arr[i] = arr[i + 1];
            arr[r] = temp;
        }

        private static void RotateRight(IList<byte> arr, int l, int r)
        {
            var temp = arr[r];
            for (var i = r; i > l; i--)
                arr[i] = arr[i - 1];
            arr[l] = temp;
        }

        private short GetFRtoBR()
        {
            var a = 0;
            var x = 0;
            var edge4 = new byte[4];
            for (var j = 11; j >= 0; j--)
                if (this.EdgePermutation[j] >= 9)
                {
                    a += Utils.BinomialCoefficient(11 - j, x + 1);
                    edge4[3 - x++] = this.EdgePermutation[j];
                }

            var b = 0;
            for (var j = 3; j > 0; j--)// compute the index b < 4! for the
                                       // permutation in perm
            {
                var k = 0;
                while (edge4[j] - 1 != j + 8)
                {
                    RotateLeft(edge4, 0, j);
                    k++;
                }
                b = (j + 1) * b + k;
            }
            return (short)(24 * a + b);
        }

        private readonly byte[] _setFRtoBR = { 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8 };

        private void SetFRtoBR(short idx)
        {
            byte[] edge4 = { 9, 10, 11, 12 };
            byte[] otherEdges = { 1, 2, 3, 4, 5, 6, 7, 8 };
            var b = idx % 24;
            var a = idx / 24;
            this.EdgePermutation = _setFRtoBR;

            for (var i = 1; i < 4; i++)
            {
                var k = b % (i + 1);
                b /= i + 1;
                while (k-- > 0) RotateRight(edge4, 0, i);
            }
            var x = 3;
            for (var i = 0; i < 12; i++)
            {
                if (a - Utils.BinomialCoefficient(11 - i, x + 1) < 0)
                {
                    continue;
                }
                this.EdgePermutation[i] = edge4[3 - x];
                a -= Utils.BinomialCoefficient(11 - i, x-- + 1);
            }
            x = 0;
            for (var j = 0; j < 12; j++)
            {
                if (this.EdgePermutation[j] == 8) this.EdgePermutation[j] = otherEdges[x++];
            }
        }

        private short GetURFtoDLF()
        {
            var a = 0;
            var x = 0;
            var corner6 = new byte[6];
            for (var i = 0; i < N_CORNER; i++)
            {
                if (this.CornerPermutation[i] > 6)
                {
                    continue;
                }
                a += Utils.BinomialCoefficient(i, x + 1);
                corner6[x++] = this.CornerPermutation[i];
            }

            var b = 0;
            for (var j = 5; j > 0; j--)
            {
                var k = 0;
                while ((corner6[j] - 1) != j)
                {
                    RotateLeft(corner6, 0, j);
                    k++;
                }
                b = (j + 1) * b + k;
            }
            return (short)(720 * a + b);
        }

        private readonly byte[] _setURFtoDLF = { 8, 8, 8, 8, 8, 8, 8, 8 };

        private void SetURFtoDLF(short idx)
        {
            byte[] corner6 = { 1, 2, 3, 4, 5, 6 };
            byte[] otherCorner = { 7, 8 };
            var b = idx % 720;
            var a = idx / 720;
            this.CornerPermutation = _setURFtoDLF;

            for (var i = 1; i < 6; i++)
            {
                var k = b % (i + 1);
                b /= i + 1;
                while (k-- > 0) RotateRight(corner6, 0, i);
            }
            var x = 5;
            for (var i = 7; i >= 0; i--)
            {
                if (a - Utils.BinomialCoefficient(i, x + 1) < 0)
                {
                    continue;
                }
                this.CornerPermutation[i] = corner6[x];
                a -= Utils.BinomialCoefficient(i, x-- + 1);
            }
            x = 0;
            for (var j = 0; j < 8; j++)
            {
                if (this.CornerPermutation[j] == 8) this.CornerPermutation[j] = otherCorner[x++];
            }
        }

        private int GetURtoDF()
        {
            var a = 0;
            var x = 0;
            var edge6 = new byte[6];
            for (var i = 0; i < 12; i++)
            {
                if (this.EdgePermutation[i] > 6)
                {
                    continue;
                }
                a += Utils.BinomialCoefficient(i, x + 1);
                edge6[x++] = this.EdgePermutation[i];
            }

            var b = 0;
            for (var i = 5; i > 0; i--)
            {
                var k = 0;
                while (edge6[i] - 1 != i)
                {
                    RotateLeft(edge6, 0, i);
                    k++;
                }
                b = (i + 1) * b + k;
            }
            return (720 * a + b);
        }

        public static int GetURtoDF(short idx1, short idx2)
        {
            var a = new CoordCube();
            var b = new CoordCube();
            a.URtoUL = idx1;
            b.UBtoDF = idx2;
            for (var i = 0; i < 8; i++)
                if (a.EdgePermutation[i] != 12)
                {
                    if (b.EdgePermutation[i] != 12)
                        return -1;
                    b.EdgePermutation[i] = a.EdgePermutation[i];
                }
            return b.URtoDF;
        }

        private readonly byte[] _12x12 = { 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12 };

        private void SetURtoDF(int idx)
        {
            byte[] edge6 = { 1, 2, 3, 4, 5, 6 };
            byte[] otherEdges = { 7, 8, 9, 10, 11, 12 };
            var b = idx % 720;
            var a = idx / 720;
            this.EdgePermutation = _12x12;

            for (var i = 1; i < 6; i++)
            {
                var k = b % (i + 1);
                b /= i + 1;
                while (k-- > 0) RotateRight(edge6, 0, i);
            }
            var x = 5;
            for (var i = 11; i >= 0; i--)
            {
                if (a - Utils.BinomialCoefficient(i, x + 1) < 0)
                {
                    continue;
                }
                this.EdgePermutation[i] = edge6[x];
                a -= Utils.BinomialCoefficient(i, x-- + 1);
            }
            x = 0;
            for (var i = 0; i < 12; i++)
            {
                if (this.EdgePermutation[i] == 12) this.EdgePermutation[i] = otherEdges[x++];
            }
        }

        private short GetURtoUL()
        {
            var a = 0;
            var x = 0;
            var edge3 = new byte[3];
            for (var i = 0; i < 12; i++)
            {
                if (this.EdgePermutation[i] > 3)
                {
                    continue;
                }
                a += Utils.BinomialCoefficient(i, x + 1);
                edge3[x++] = this.EdgePermutation[i];
            }

            var b = 0;
            for (var i = 2; i > 0; i--)
            {
                var k = 0;
                while (edge3[i] - 1 != i)
                {
                    RotateLeft(edge3, 0, i);
                    k++;
                }
                b = (i + 1) * b + k;
            }
            return (short)(6 * a + b);
        }

        private void SetURtoUL(short idx)
        {
            byte[] edge3 = { 1, 2, 3 };
            var b = idx % 6;
            var a = idx / 6;
            this.EdgePermutation = _12x12;

            for (var i = 1; i < 3; i++)
            {
                var k = b % (i + 1);
                b /= i + 1;
                while (k-- > 0) RotateRight(edge3, 0, i);
            }
            var x = 2;
            for (var i = 11; i >= 0; i--)
            {
                if (a - Utils.BinomialCoefficient(i, x + 1) < 0)
                {
                    continue;
                }
                this.EdgePermutation[i] = edge3[x];
                a -= Utils.BinomialCoefficient(i, x-- + 1);
            }
        }

        private short GetUBtoDF()
        {
            var a = 0;
            var x = 0;
            var edge3 = new byte[3];
            for (var i = 0; i < 12; i++)
            {
                if (this.EdgePermutation[i] < 4 || this.EdgePermutation[i] > 6)
                {
                    continue;
                }
                a += Utils.BinomialCoefficient(i, x + 1);
                edge3[x++] = this.EdgePermutation[i];
            }

            var b = 0;
            for (var i = 2; i > 0; i--)
            {
                var k = 0;
                while (edge3[i] - 1 != i + 3)
                {
                    RotateLeft(edge3, 0, i);
                    k++;
                }
                b = (i + 1) * b + k;
            }
            return (short)(6 * a + b);
        }

        private void SetUBtoDF(short idx)
        {
            byte[] edge3 = { 4, 5, 6 };
            var b = idx % 6;
            var a = idx / 6;
            this.EdgePermutation = _12x12;

            for (var i = 1; i < 3; i++)
            {
                var k = b % (i + 1);
                b /= i + 1;
                while (k-- > 0) RotateRight(edge3, 0, i);
            }
            var x = 2;
            for (var i = 11; i >= 0; i--)
            {
                if (a - Utils.BinomialCoefficient(i, x + 1) < 0)
                {
                    continue;
                }
                this.EdgePermutation[i] = edge3[x];
                a -= Utils.BinomialCoefficient(i, x-- + 1);
            }
        }

        private int GetURFtoDLB()
        {
            var perm = new byte[N_CORNER];
            var b = 0;
            for (var i = 0; i < N_CORNER; i++)
                perm[i] = this.CornerPermutation[i];
            for (var i = 7; i > 0; i--)
            {
                var k = 0;
                while (perm[i] - 1 != i)
                {
                    RotateLeft(perm, 0, i);
                    k++;
                }
                b = (i + 1) * b + k;
            }
            return b;
        }

        private readonly byte[] _1to8 = { 1, 2, 3, 4, 5, 6, 7, 8 };

        private void SetURFtoDLB(int idx)
        {
            var perm = _1to8;
            for (var i = 1; i < 8; i++)
            {
                var k = idx % (i + 1);
                idx /= i + 1;
                while (k-- > 0) RotateRight(perm, 0, i);
            }

            var x = 7;
            for (var i = 7; i >= 0; i--)
            {
                this.CornerPermutation[i] = perm[x--];
            }
        }

        private int GetURtoBR()
        {
            var perm = new byte[N_EDGE];
            var b = 0;
            for (var i = 0; i < N_EDGE; i++)
                perm[i] = this.EdgePermutation[i];
            for (var i = 11; i > 0; i--)
            {
                var k = 0;
                while (perm[i] - 1 != i)
                {
                    RotateLeft(perm, 0, i);
                    k++;
                }
                b = (i + 1) * b + k;
            }
            return b;
        }

        private readonly byte[] _1to12 = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

        private void SetURtoBR(int idx)
        {
            var perm = _1to12;
            for (var i = 1; i < 12; i++)
            {
                var k = idx % (i + 1);
                idx /= i + 1;
                while (k-- > 0) RotateRight(perm, 0, i);
            }
            var x = 11;
            for (var i = 11; i >= 0; i--) this.EdgePermutation[i] = perm[x--];
        }

        #endregion Coordinate conversions

        #region Permutation inversion conversion: Position <-> Inversions

        public static byte[] FromInversions(byte[] perm)
        {
            var cubies = new byte[perm.Length];
            var upperBound = (byte)perm.Length;
            byte lowerBound = 1;

            var cancelled = false;
            var not = false;
            for (var index = perm.Length - 1; index >= 0; index--)
            {
                if (!not)
                {
                    if (index == perm[index])
                    {
                        cubies[index] = lowerBound;
                        lowerBound++;
                        not = true;
                    }
                }

                if (cancelled)
                {
                    continue;
                }
                if (perm[index] != 0)
                {
                    continue;
                }
                cubies[index] = upperBound;
                upperBound--;
                cancelled = true;
            }

            while (upperBound >= lowerBound)
            {
                for (var i = perm.Length - 1; i >= 0; i--)
                {
                    if (cubies[i] != 0)
                    {
                        continue;
                    }
                    var count = 0;
                    for (var j = 0; j < i; j++)
                    {
                        if (cubies[j] > upperBound) count++;
                    }
                    if (count != perm[i])
                    {
                        continue;
                    }
                    cubies[i] = upperBound;
                    upperBound--;
                    break;
                }
            }
            return cubies;
        }

        public static byte[] ToInversions(byte[] perm)
        {
            var inversions = new byte[perm.Length];
            for (var i = 0; i < perm.Length; i++)
            {
                var count = 0;
                for (var j = 0; j < i; j++)
                {
                    if (perm[j] > perm[i]) count++;
                }
                inversions[i] = (byte)count;
            }
            return inversions;
        }

        #endregion Permutation inversion conversion: Position <-> Inversions

        public void Multiply(CoordCube b)
        {
            var edgeMult = Multiply(this.EdgePermutation, this.eo, b.EdgePermutation, b.eo);
            var cornerMult = Multiply(this.CornerPermutation, this.co, b.CornerPermutation, b.co);
            this.EdgePermutation = edgeMult.Item1;
            this.eo = edgeMult.Item2;
            this.CornerPermutation = cornerMult.Item1;
            this.co = cornerMult.Item2;
        }

        private static Tuple<byte[], byte[]> Multiply(IList<byte> permA, IList<byte> orieA, IList<byte> permB, IList<byte> orieB)
        {
            var AxB = new byte[permA.Count];
            var AxBo = new byte[permA.Count];
            var mod = permA.Count == 8 ? 3 : 2;

            for (var i = 0; i < permA.Count; i++)
            {
                // (A*B)(x).c=A(B(x).c).c
                AxB[i] = permA[permB[i] - 1];

                // (A*B)(x).o=A(B(x).c).o+B(x).o
                AxBo[i] = (byte)((orieA[permB[i] - 1] + orieB[i]) % mod);
            }
            return new Tuple<byte[], byte[]>(AxB, AxBo);
        }
    }
}