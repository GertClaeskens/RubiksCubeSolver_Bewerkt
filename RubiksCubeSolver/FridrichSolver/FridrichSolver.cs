using RubiksCubeLib;
using RubiksCubeLib.RubiksCube;
using RubiksCubeLib.Solver;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace FridrichSolver
{
    public class FridrichSolver : CubeSolver
    {
        public override string Name => "Fridrich";

        public override string Description => "Full Fridrich method without ELL and COLL";

        public FridrichSolver()
        {
            this.AddSolutionSteps();
        }

        protected override sealed void AddSolutionSteps()
        {
            this.SolutionSteps = new Dictionary<string, Tuple<Action, SolutionStepType>>();
            this.AddSolutionStep("Cross on bottom layer", this.SolveFirstCross);
            this.AddSolutionStep("Complete first two layers", this.CompleteF2L);
            this.AddSolutionStep("Orientation top layer", this.Oll);
            this.AddSolutionStep("Permutation last layer", this.Pll);
        }

        private void SolveFirstCross()
        {
            // Step 1: Get the edges with target position on the bottom layer
            var bottomEdges = this.Rubik.Cubes.Where(c => c.IsEdge && this.GetTargetFlags(c).HasFlag(CubeFlag.BottomLayer));

            // Step 2: Rotate a correct orientated edge of the bottom layer to target position
            var solvedBottomEdges = bottomEdges.Where(bE => bE.Position.Flags == this.GetTargetFlags(bE) && bE.Faces.First(f => f.Color == this.Rubik.BottomColor).Position == FacePosition.Bottom);
            if (bottomEdges.Count(bE => bE.Position.HasFlag(CubeFlag.BottomLayer) && bE.Faces.First(f => f.Color == this.Rubik.BottomColor).Position == FacePosition.Bottom) > 0)
            {
                while (!solvedBottomEdges.Any())
                {
                    this.SolverMove(CubeFlag.BottomLayer, true);
                    solvedBottomEdges = bottomEdges.Where(bE => bE.Position.Flags == this.GetTargetFlags(bE) && bE.Faces.First(f => f.Color == this.Rubik.BottomColor).Position == FacePosition.Bottom);
                }
            }

            // Step 3: Solve incorrect edges of the bottom layer
            while (solvedBottomEdges.Count() < 4)
            {
                var unsolvedBottomEdges = bottomEdges.Except(solvedBottomEdges);
                var e = (unsolvedBottomEdges.FirstOrDefault(c => c.Position.HasFlag(CubeFlag.TopLayer)) != null)
                    ? unsolvedBottomEdges.FirstOrDefault(c => c.Position.HasFlag(CubeFlag.TopLayer)) : unsolvedBottomEdges.First();
                var secondColor = e.Colors.First(co => co != this.Rubik.BottomColor && co != Color.Black);

                if (e.Position.Flags != this.GetTargetFlags(e))
                {
                    // Rotate to top layer
                    var layer = CubeFlagService.FromFacePosition(e.Faces.First(f => (f.Color == this.Rubik.BottomColor || f.Color == secondColor)
                      && f.Position != FacePosition.Top && f.Position != FacePosition.Bottom).Position);

                    var targetLayer = CubeFlagService.FromFacePosition(this.StandardCube.Cubes.First(cu => CollectionMethods.ScrambledEquals(cu.Colors, e.Colors))
                      .Faces.First(f => f.Color == secondColor).Position);

                    if (e.Position.HasFlag(CubeFlag.MiddleLayer))
                    {
                        if (layer == targetLayer)
                        {
                            while (!e.Position.HasFlag(CubeFlag.BottomLayer)) this.SolverMove(layer, true);
                        }
                        else
                        {
                            this.SolverMove(layer, true);
                            if (e.Position.HasFlag(CubeFlag.TopLayer))
                            {
                                this.SolverMove(CubeFlag.TopLayer, true);
                                this.SolverMove(layer, false);
                            }
                            else
                            {
                                for (var i = 0; i < 2; i++) this.SolverMove(layer, true);
                                this.SolverMove(CubeFlag.TopLayer, true);
                                this.SolverMove(layer, true);
                            }
                        }
                    }
                    if (e.Position.HasFlag(CubeFlag.BottomLayer)) for (var i = 0; i < 2; i++) this.SolverMove(layer, true);

                    // Rotate over target position
                    while (!e.Position.HasFlag(targetLayer)) this.SolverMove(CubeFlag.TopLayer, true);

                    //Rotate to target position
                    for (var i = 0; i < 2; i++) this.SolverMove(targetLayer, true);
                    //COMMENTED
                    //var targetPos = this.GetTargetFlags(e);
                }

                // Flip the incorrect orientated edges with the algorithm: Fi D Ri Di
                if (Solvability.GetOrientation(this.Rubik, e) != 0)
                {
                    var frontSlice = CubeFlagService.FromFacePosition(e.Faces.First(f => f.Color == this.Rubik.BottomColor).Position);

                    this.SolverMove(frontSlice, false);
                    this.SolverMove(CubeFlag.BottomLayer, true);

                    var rightSlice = CubeFlagService.FromFacePosition(e.Faces.First(f => f.Color == secondColor).Position);

                    this.SolverMove(rightSlice, false);
                    this.SolverMove(CubeFlag.BottomLayer, false);
                }
                //COMMENTED
                //var faces = e.Faces.ToList();
                solvedBottomEdges = bottomEdges.Where(bE => bE.Position.Flags == this.GetTargetFlags(bE) && bE.Faces.First(f => f.Color == this.Rubik.BottomColor).Position == FacePosition.Bottom);
            }
        }

        private void CompleteF2L()
        {
            var unsolvedPairs = this.GetPairs(this.Rubik).ToList();

            while (unsolvedPairs.Count > 0) // 4 pairs
            {
                var currentPair = unsolvedPairs.First();

                var edge = currentPair.Item1;
                var corner = currentPair.Item2;

                var target = new CubePosition(this.Rubik.GetTargetFlags(corner));

                if (!corner.Position.HasFlag(CubeFlag.TopLayer) && this.Rubik.GetTargetFlags(corner) != corner.Position.Flags)
                {
                    var rotationLayer = CubeFlagService.FirstNotInvalidFlag(corner.Position.Flags, CubeFlag.BottomLayer);
                    var direction = new TestScenario(this.Rubik, new LayerMove(rotationLayer)).TestCubePosition(corner, CubeFlag.TopLayer);
                    this.SolverMove(rotationLayer, direction);
                    this.SolverMove(CubeFlag.TopLayer, true);
                    this.SolverMove(rotationLayer, !direction);
                }
                // move edge to top position if necessary
                if (!edge.Position.HasFlag(CubeFlag.TopLayer) && this.Rubik.GetTargetFlags(edge) != edge.Position.Flags)
                {
                    var rotationLayer = CubeFlagService.FirstNotInvalidFlag(edge.Position.Flags, CubeFlag.MiddleLayer);
                    var direction = new TestScenario(this.Rubik, new LayerMove(rotationLayer)).TestCubePosition(edge, CubeFlag.TopLayer);
                    this.SolverMove(rotationLayer, direction);
                    while ((corner.Position.HasFlag(rotationLayer) && !corner.Position.HasFlag(CubeFlag.BottomLayer)) || edge.Position.HasFlag(rotationLayer)) this.SolverMove(CubeFlag.TopLayer, true);
                    this.SolverMove(rotationLayer, !direction);
                }

                // detect right and front slice
                var rightSlice = CubeFlagService.ToInt(target.X) == CubeFlagService.ToInt(target.Z) ? target.Z : target.X;
                var frontSlice = CubeFlagService.FirstNotInvalidFlag(target.Flags, CubeFlag.YFlags | rightSlice);

                while (!corner.Position.HasFlag(target.Flags & ~CubeFlag.BottomLayer)) this.SolverMove(CubeFlag.TopLayer, true);

                var filter = new PatternFilter(new Func<Pattern, Pattern, bool>(delegate (Pattern p1, Pattern p2)
                {
                    var item = new PatternItem(corner.Position, Solvability.GetOrientation(this.Rubik, corner), target.Flags);
                    return p2.Items.Any(i => i.Equals(item));
                }), true);

                for (var i = 0; i < 4; i++)
                {
                    var pattern = new F2LPattern(this.Rubik.GetTargetFlags(edge), this.Rubik.GetTargetFlags(corner), rightSlice, frontSlice);
                    var algo = pattern.FindBestMatch(Pattern.FromRubik(this.Rubik), CubeFlag.None, filter);
                    if (algo != null)
                    {
                        this.SolverAlgorithm(algo); break;
                    }
                    this.SolverMove(CubeFlag.TopLayer, true);
                }

                var count = unsolvedPairs.Count;
                unsolvedPairs = this.GetPairs(this.Rubik).ToList();
                if (unsolvedPairs.Count != count)
                {
                    continue;
                }
                this.BroadcastOnSolutionError("Complete first two layers", "Wrong algorithm");
                return;
            }
        }

        private IEnumerable<Tuple<Cube, Cube>> GetPairs(Rubik rubik) => from edge in rubik.Cubes.Where(c => c.IsEdge && this.Rubik.GetTargetFlags(c).HasFlag(CubeFlag.MiddleLayer)) let corner = rubik.Cubes.First(c => c.IsCorner && (rubik.GetTargetFlags(c) & ~CubeFlag.BottomLayer) == (rubik.GetTargetFlags(edge) & ~CubeFlag.MiddleLayer)) where !rubik.IsCorrect(corner) || !rubik.IsCorrect(edge) select new Tuple<Cube, Cube>(edge, corner);

        private void Oll()
        {
            var p = new OllPattern();
            var oll = p.FindBestMatch(Pattern.FromRubik(this.Rubik), CubeFlag.TopLayer, PatternFilter.SameFlipCount);
            if (oll != null) this.SolverAlgorithm(oll); // else no oll algorithm required
        }

        private void Pll()
        {
            var p = new PllPattern();
            for (var i = 0; i < 4; i++)
            {
                var pll = p.FindBestMatch(Pattern.FromRubik(this.Rubik), CubeFlag.TopLayer, PatternFilter.SameInversionCount);
                if (pll != null)
                {
                    this.SolverAlgorithm(pll); break;
                }
                this.SolverMove(CubeFlag.TopLayer, true);
            }
        }
    }
}