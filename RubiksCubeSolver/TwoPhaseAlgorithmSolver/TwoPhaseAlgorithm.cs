namespace TwoPhaseAlgorithmSolver
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using RubiksCubeLib;
    using RubiksCubeLib.RubiksCube;
    using RubiksCubeLib.Solver;

    public partial class TwoPhaseAlgorithm : CubeSolver//, IDisposable
    {
        #region constants

        // phase 1 coordinates
        public const short N_TWIST = 2187; // 3^7 possible corner orientations

        public const short N_FLIP = 2048; // 2^11 possible edge flips
        public const short N_SLICE1 = 495; // 12 choose 4 possible positions of FR, FL, BL, BR edges

        // phase 2 coordinates
        public const short N_SLICE2 = 24; // 4! permutations of FR, FL, BL, BR edges

        public const short N_PARITY = 2; // 2 possible corner parities
        public const short N_URFtoDLF = 20160; // 8! / (8 - 6)! permutation of URF, UFL, ULB, UBR, DFR, DLF corners
        public const short N_FRtoBR = 11880; // 12! / (12 - 4)! permutation of FR, FL, BL, BR edges
        public const short N_URtoUL = 1320; // 12! / (12 - 3)! permutation of UR, UF, UL edges
        public const short N_UBtoDF = 1320; // 12! / (12 - 3)! permutation of UB, DR, DF edges
        public const short N_URtoDF = 20160; // 8! / (8 - 6)! permutation of UR, UF, UL, UB, DR, DF edges

        public const int N_URFtoDLB = 40320; // 8! permutations of the corners
        public const int N_URtoBR = 479001600; //12! permutations of the edges

        public const short N_MOVE = 18;
        public const short N_CORNER = 8;
        public const short N_EDGE = 12;

        #endregion constants

        public override string Name => "Two Phase Algorithm";

        public override string Description => "An Algorithm from Kociemba for solving Rubik's Cube with a little amount of moves. The program uses a search algorithm which is called iterative deepening A* with a lowerbound heuristic function (IDA*)";


        //METHOD SEALED GEMAAKT
        protected sealed override void AddSolutionSteps()
        {
            this.SolutionSteps = new Dictionary<string, Tuple<Action, SolutionStepType>>();
            this.AddSolutionStep("Init move tables", this.InitMoveTables, SolutionStepType.Initialization);
            this.AddSolutionStep("Init pruning tables", this.InitPruningTables, SolutionStepType.Initialization);
            this.AddSolutionStep("IDA* search for solution", this.Solution);
        }

        public string TablePath { get; set; }
        public int MaxDepth { get; set; }
        public long TimeOut { get; set; }
        private CoordCube _coordCube;

        public TwoPhaseAlgorithm()
        {
            this.AddSolutionSteps();
            this.TablePath = @"tables\";
            if (!Directory.Exists(this.TablePath))
                Directory.CreateDirectory(this.TablePath);
        }

        protected override void Solve(Rubik cube)
        {
            this.Rubik = cube.DeepClone();
            this._coordCube = ToCoordCube(cube);
            this.MaxDepth = 30;
            this.TimeOut = 10000;
            this.Algorithm = new Algorithm();
            this.InitStandardCube();

            this.GetSolution();
            this.RemoveUnnecessaryMoves();
        }

        #region solving logic

        private readonly int[] ax = new int[31]; // The axis of the move
        private readonly int[] po = new int[31]; // The power of the move

        private readonly int[] flip = new int[31]; // phase1 coordinates
        private readonly int[] twist = new int[31];
        private readonly int[] slice = new int[31];

        private readonly int[] parity = new int[31]; // phase2 coordinates
        private readonly int[] urfdlf = new int[31];
        private readonly int[] frbr = new int[31];
        private readonly int[] urul = new int[31];
        private readonly int[] ubdf = new int[31];
        private readonly int[] urdf = new int[31];

        private readonly int[] minDistPhase1 = new int[31]; // IDA* distance do goal estimations
        private readonly int[] minDistPhase2 = new int[31];

        public void Solution()
        {
            this.po[0] = 0;
            this.ax[0] = 0;
            this.flip[0] = this._coordCube.Flip;
            this.twist[0] = this._coordCube.Twist;
            this.parity[0] = this._coordCube.Parity;
            this.slice[0] = this._coordCube.FRtoBR / 24;
            this.urfdlf[0] = this._coordCube.URFtoDLF;
            this.frbr[0] = this._coordCube.FRtoBR;
            this.urul[0] = this._coordCube.URtoUL;
            this.ubdf[0] = this._coordCube.UBtoDF;

            this.minDistPhase1[1] = 1;
            var n = 0;
            var busy = false;
            var depthPhase1 = 0;

            long tStart = DateTime.Now.Millisecond;

            do
            {
                do
                {
                    if ((depthPhase1 - n > this.minDistPhase1[n + 1]) && !busy)
                    {
                        if (this.ax[n] == 0 || this.ax[n] == 3) this.ax[++n] = 1;
                        else this.ax[++n] = 0;
                        this.po[n] = 1;
                    }
                    else if (++this.po[n] > 3)
                    {
                        do
                        {
                            // increment axis
                            if (++this.ax[n] > 5)
                            {
                                if (DateTime.Now.Millisecond - tStart > this.TimeOut << 10) return; // Error: Timeout, no solution within given time
                                if (n == 0)
                                {
                                    if (depthPhase1 >= this.MaxDepth) return; // Error: No solution exists for the given maxDepth
                                    depthPhase1++;
                                    this.ax[n] = 0;
                                    this.po[n] = 1;
                                    busy = false;
                                    break;
                                }
                                n--;
                                busy = true;
                                break;
                            }
                            this.po[n] = 1;
                            busy = false;
                        } while (n != 0 && (this.ax[n - 1] == this.ax[n] || this.ax[n - 1] - 3 == this.ax[n]));
                    }
                    else busy = false;
                } while (busy);

                var mv = 3 * this.ax[n] + this.po[n] - 1;
                this.flip[n + 1] = this.flipMove[this.flip[n], mv];
                this.twist[n + 1] = this.twistMove[this.twist[n], mv];
                this.slice[n + 1] = this.FRtoBR_Move[this.slice[n] * 24, mv] / 24;
                this.minDistPhase1[n + 1] = Math.Max(GetPruning(this.sliceFlipPrun, N_SLICE1 * this.flip[n + 1]
                + this.slice[n + 1]), GetPruning(this.sliceTwistPrun, N_SLICE1 * this.twist[n + 1]
                + this.slice[n + 1]));

                if (this.minDistPhase1[n + 1] != 0 || n < depthPhase1 - 5)
                {
                    continue;
                }
                this.minDistPhase1[n + 1] = 10;// instead of 10 any value >5 is possible
                int s;
                if (n != depthPhase1 - 1 || (s = this.TotalDepth(depthPhase1, this.MaxDepth)) < 0)
                {
                    continue;
                }
                // solution found
                for (var i = 0; i < s; i++)
                {
                    if (this.po[i] == 0) break;
                    this.SolverMove(IntsToLayerMove(this.ax[i], this.po[i]));
                }
                return;
            } while (true);
        }

        private int TotalDepth(int depthPhase1, int maxDepth)
        {
            int mv, d1, d2;
            var maxDepthPhase2 = Math.Min(10, maxDepth - depthPhase1);
            for (var i = 0; i < depthPhase1; i++)
            {
                mv = 3 * this.ax[i] + this.po[i] - 1;
                this.urfdlf[i + 1] = this.URFtoDLF_Move[this.urfdlf[i], mv];
                this.frbr[i + 1] = this.FRtoBR_Move[this.frbr[i], mv];
                this.parity[i + 1] = this.parityMove[this.parity[i], mv];
            }

            if ((d1 = GetPruning(this.sliceURFtoDLF_Prun,
              (N_SLICE2 * this.urfdlf[depthPhase1] + this.frbr[depthPhase1]) * 2 + this.parity[depthPhase1])) > maxDepthPhase2)
                return -1;

            for (var i = 0; i < depthPhase1; i++)
            {
                mv = 3 * this.ax[i] + this.po[i] - 1;
                this.urul[i + 1] = this.URtoUL_Move[this.urul[i], mv];
                this.ubdf[i + 1] = this.UBtoDF_Move[this.ubdf[i], mv];
            }
            this.urdf[depthPhase1] = this.mergeURtoULandUBtoDF[this.urul[depthPhase1], this.ubdf[depthPhase1]];

            if ((d2 = GetPruning(this.sliceURtoDF_Prun,
                (N_SLICE2 * this.urdf[depthPhase1] + this.frbr[depthPhase1]) * 2 + this.parity[depthPhase1])) > maxDepthPhase2)
                return -1;

            if ((this.minDistPhase2[depthPhase1] = Math.Max(d1, d2)) == 0)// already solved
                return depthPhase1;

            // now set up search

            var depthPhase2 = 1;
            var n = depthPhase1;
            var busy = false;
            this.po[depthPhase1] = 0;
            this.ax[depthPhase1] = 0;
            this.minDistPhase2[n + 1] = 1;// else failure for depthPhase2=1, n=0
                                          // +++++++++++++++++++ end initialization +++++++++++++++++++++++++++++++++
            do
            {
                do
                {
                    if ((depthPhase1 + depthPhase2 - n > this.minDistPhase2[n + 1]) && !busy)
                    {
                        if (this.ax[n] == 0 || this.ax[n] == 3)// Initialize next move
                        {
                            this.ax[++n] = 1;
                            this.po[n] = 2;
                        }
                        else
                        {
                            this.ax[++n] = 0;
                            this.po[n] = 1;
                        }
                    }
                    else if ((this.ax[n] == 0 || this.ax[n] == 3) ? (++this.po[n] > 3) : ((this.po[n] = this.po[n] + 2) > 3))
                    {
                        do
                        {// increment axis
                            if (++this.ax[n] > 5)
                            {
                                if (n == depthPhase1)
                                {
                                    if (depthPhase2 >= maxDepthPhase2)
                                        return -1;
                                    depthPhase2++;
                                    this.ax[n] = 0;
                                    this.po[n] = 1;
                                    busy = false;
                                    break;
                                }
                                n--;
                                busy = true;
                                break;
                            }
                            if (this.ax[n] == 0 || this.ax[n] == 3) this.po[n] = 1;
                            else this.po[n] = 2;
                            busy = false;
                        } while (n != depthPhase1 && (this.ax[n - 1] == this.ax[n] || this.ax[n - 1] - 3 == this.ax[n]));
                    }
                    else
                        busy = false;
                } while (busy);
                // +++++++++++++ compute new coordinates and new minDist ++++++++++
                mv = 3 * this.ax[n] + this.po[n] - 1;

                this.urfdlf[n + 1] = this.URFtoDLF_Move[this.urfdlf[n], mv];
                this.frbr[n + 1] = this.FRtoBR_Move[this.frbr[n], mv];
                this.parity[n + 1] = this.parityMove[this.parity[n], mv];
                this.urdf[n + 1] = this.URtoDF_Move[this.urdf[n], mv];

                this.minDistPhase2[n + 1] = Math.Max(GetPruning(this.sliceURtoDF_Prun, (N_SLICE2
                  * this.urdf[n + 1] + this.frbr[n + 1])
                  * 2 + this.parity[n + 1]), GetPruning(this.sliceURFtoDLF_Prun, (N_SLICE2
                  * this.urfdlf[n + 1] + this.frbr[n + 1])
                  * 2 + this.parity[n + 1]));
                // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            } while (this.minDistPhase2[n + 1] != 0);
            return depthPhase1 + depthPhase2;
        }

        //#region IDisposable Support
        //private bool disposedValue = false; // To detect redundant calls

        //protected virtual void Dispose(bool disposing)
        //{
        //    if (this.disposedValue)
        //    {
        //        return;
        //    }
        //    if (disposing)
        //    {
        //        // TODO: dispose managed state (managed objects).
        //    }

        //    // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
        //    // TODO: set large fields to null.

        //    this.disposedValue = true;
        //}

        //// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        //// ~TwoPhaseAlgorithm() {
        ////   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        ////   Dispose(false);
        //// }

        //// This code added to correctly implement the disposable pattern.
        //public void Dispose()
        //{
        //    // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //    Dispose(true);
        //    // TODO: uncomment the following line if the finalizer is overridden above.
        //    // GC.SuppressFinalize(this);
        //}
        //#endregion

        #endregion solving logic
    }
}