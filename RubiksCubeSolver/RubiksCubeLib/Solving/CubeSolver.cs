using RubiksCubeLib.RubiksCube;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace RubiksCubeLib.Solver
{
    /// <summary>
    /// Represents the CubeSolver, and forces all implementing classes to have several methods
    /// </summary>
    public abstract class CubeSolver : IPluginable
    {
        // *** PROPERTIES ***

        /// <summary>
        /// The Rubik which will be used to solve the transferred Rubik
        /// </summary>
        protected Rubik Rubik { get; set; }

        /// <summary>
        /// A solved Rubik
        /// </summary>
        protected Rubik StandardCube { get; set; }

        /// <summary>
        /// Returns the solution for this solver used for the Rubik
        /// </summary>
        protected Algorithm Algorithm { get; set; }

        /// <summary>
        /// The name of this solver
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// The descrption of this solver
        /// </summary>
        public abstract string Description { get; }

        public Dictionary<string, Tuple<Action, SolutionStepType>> SolutionSteps { get; protected set; }

        private readonly List<IMove> _movesOfStep = new List<IMove>();
        private Thread solvingThread;

        public delegate void SolutionStepCompletedEventHandler(object sender, SolutionStepCompletedEventArgs e);

        public event SolutionStepCompletedEventHandler OnSolutionStepCompleted;

        public delegate void SolutionErrorEventHandler(object sender, SolutionErrorEventArgs e);

        public event SolutionErrorEventHandler OnSolutionError;

        protected void BroadcastOnSolutionError(string step, string message)
        {
            this.OnSolutionError?.Invoke(this, new SolutionErrorEventArgs(step, message));
            this.solvingThread.Abort();
        }

        protected void AddSolutionStep(string key, Action action, SolutionStepType type = SolutionStepType.Standard)
        {
            this.SolutionSteps.Add(key, new Tuple<Action, SolutionStepType>(action, type));
        }

        // *** METHODS ***

        /// <summary>
        /// Returns the solution for the transferred Rubik
        /// </summary>
        /// <param name="cube">Defines the Rubik to be solved</param>
        protected virtual void Solve(Rubik cube)
        {
            this.Rubik = cube.DeepClone();
            this.Algorithm = new Algorithm();
            this.InitStandardCube();

            this.GetSolution();
            this.RemoveUnnecessaryMoves();
        }

        protected void GetSolution()
        {
            var sw = new Stopwatch();
            foreach (var step in this.SolutionSteps)
            {
                sw.Restart();
                step.Value.Item1();
                sw.Stop();
                this.OnSolutionStepCompleted?.Invoke(this, new SolutionStepCompletedEventArgs(step.Key, false, new Algorithm { Moves = this._movesOfStep }, (int)sw.ElapsedMilliseconds, step.Value.Item2));
                this._movesOfStep.Clear();
            }
        }

        protected abstract void AddSolutionSteps();

        public void TrySolveAsync(Rubik rubik)
        {
            this.solvingThread = new Thread(() => this.SolveAsync(rubik));
            this.solvingThread.Start();
        }

        private void SolveAsync(Rubik rubik)
        {
            var solvable = Solvability.FullTest(rubik);
            if (solvable)
            {
                var sw = new Stopwatch();
                sw.Start();
                this.Solve(rubik);
                sw.Stop();
                this.OnSolutionStepCompleted?.Invoke(this, new SolutionStepCompletedEventArgs(this.Name, true, this.Algorithm, (int)sw.ElapsedMilliseconds));
                this.solvingThread.Abort();
            }
            else
            {
                this.BroadcastOnSolutionError(this.Name, "Unsolvable cube");
            }
        }

        /// <summary>
        /// Removes all unnecessary moves from the solution algorithm
        /// </summary>
        protected void RemoveUnnecessaryMoves()
        {
            var finished = false;
            while (!finished)
            {
                finished = true;
                for (var i = 0; i < this.Algorithm.Moves.Count; i++)
                {
                    var currentMove = this.Algorithm.Moves[i];
                    if (i < this.Algorithm.Moves.Count - 1)
                        if (currentMove.ReverseMove.Equals(this.Algorithm.Moves[i + 1]))
                        {
                            finished = false;
                            this.Algorithm.Moves.RemoveAt(i + 1);
                            this.Algorithm.Moves.RemoveAt(i);
                            if (i != 0) i--;
                        }

                    if (i >= this.Algorithm.Moves.Count - 2)
                    {
                        continue;
                    }
                    if (!currentMove.Equals(this.Algorithm.Moves[i + 1]) || !currentMove.Equals(this.Algorithm.Moves[i + 2]))
                    {
                        continue;
                    }
                    finished = false;
                    var reverse = this.Algorithm.Moves[i + 2].ReverseMove;
                    this.Algorithm.Moves.RemoveAt(i + 1);
                    this.Algorithm.Moves.RemoveAt(i);
                    this.Algorithm.Moves[i] = reverse;
                    if (i != 0) i--;
                }
            }
        }

        /// <summary>
        /// Initializes the StandardCube
        /// </summary>
        protected void InitStandardCube()
        {
            this.StandardCube = this.Rubik.GenStandardCube();
        }

        /// <summary>
        /// Returns the position of given cube where it has to be when the Rubik is solved
        /// </summary>
        /// <param name="cube">Defines the cube to be analyzed</param>
        /// <returns></returns>
        protected CubeFlag GetTargetFlags(Cube cube) => this.StandardCube.Cubes.First(cu => CollectionMethods.ScrambledEquals(cu.Colors, cube.Colors)).Position.Flags;

        /// <summary>
        /// Adds n move to the solution and executes it on the Rubik
        /// </summary>
        /// <param name="layer">Defines the layer to be rotated</param>
        /// <param name="direction">Defines the direction of the rotation</param>
        protected void SolverMove(CubeFlag layer, bool direction)
        {
            this.Rubik.RotateLayer(layer, direction);
            this.Algorithm.Moves.Add(new LayerMove(layer, direction));
            this._movesOfStep.Add(new LayerMove(layer, direction));
        }

        /// <summary>
        /// Adds a move to the solution and executes it on the Rubik
        /// </summary>
        /// <param name="move">Defines the move to be rotated</param>
        protected void SolverMove(IMove move)
        {
            this.Rubik.RotateLayer(move);
            this.Algorithm.Moves.Add(move);
            this._movesOfStep.Add(move);
        }

        /// <summary>
        /// Executes the given algorithm
        /// </summary>
        /// <param name="moves">Defines a notation string, which is filled with placeholders</param>
        /// <param name="placeholders">Defines the objects to be inserted for the placeholders</param>
        protected void SolverAlgorithm(string moves, params object[] placeholders)
        {
            var algorithm = new Algorithm(moves, placeholders);
            this.SolverAlgorithm(algorithm);
        }

        /// <summary>
        /// Executes the given alorithm on the Rubik
        /// </summary>
        /// <param name="algorithm">Defines the algorithm to be executed</param>
        protected void SolverAlgorithm(Algorithm algorithm)
        {
            foreach (var m in algorithm.Moves) this.SolverMove(m);
        }
    }
}