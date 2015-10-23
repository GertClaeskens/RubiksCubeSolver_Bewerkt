using RubiksCubeLib;
using RubiksCubeLib.RubiksCube;
using RubiksCubeLib.Solver;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

/// <summary>
/// ORIGINEEL
/// </summary>
namespace BeginnerSolver
{
  public class BeginnerSolver : CubeSolver
  {
    public override string Name => "Beginner";

      public override string Description => "Easiest way to solve a rubiks cube. This algorithm requires between 100 and 150 moves";

      public BeginnerSolver()
    {
          this.AddSolutionSteps();
    }

    public BeginnerSolver(Rubik cube)
    {
        this.Rubik = cube.DeepClone();
      this.Algorithm = new Algorithm();
        this.InitStandardCube();
    }

    protected override sealed void AddSolutionSteps()
    {
      this.SolutionSteps = new Dictionary<string, Tuple<Action,SolutionStepType>>();
      this.AddSolutionStep("Cross on bottom layer", this.SolveFirstCross);
      this.AddSolutionStep("Complete bottom layer", this.CompleteFirstLayer);
      this.AddSolutionStep("Complete middle layer", this.CompleteMiddleLayer);
      this.AddSolutionStep("Cross on top layer", this.SolveCrossTopLayer);
      this.AddSolutionStep("Complete top layer", this.CompleteLastLayer);
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
          var targetPos = this.GetTargetFlags(e);
        }

        // Flip the incorrect orientated edges with the algorithm: Fi D Ri Di
        if (Solvability.GetOrientation(this.Rubik,e) != 0)
        {
          var frontSlice = CubeFlagService.FromFacePosition(e.Faces.First(f => f.Color == this.Rubik.BottomColor).Position);

            this.SolverMove(frontSlice, false);
            this.SolverMove(CubeFlag.BottomLayer, true);

          var rightSlice = CubeFlagService.FromFacePosition(e.Faces.First(f => f.Color == secondColor).Position);

            this.SolverMove(rightSlice, false);
            this.SolverMove(CubeFlag.BottomLayer, false);
        }
        var faces = e.Faces.ToList();
        solvedBottomEdges = bottomEdges.Where(bE => bE.Position.Flags == this.GetTargetFlags(bE) && bE.Faces.First(f => f.Color == this.Rubik.BottomColor).Position == FacePosition.Bottom);
      }
    }

    private void CompleteFirstLayer()
    {
      // Step 1: Get the corners with target position on bottom layer
      var bottomCorners = this.Rubik.Cubes.Where(c => c.IsCorner && this.GetTargetFlags(c).HasFlag(CubeFlag.BottomLayer));
      var solvedBottomCorners = bottomCorners.Where(bC => bC.Position.Flags == this.GetTargetFlags(bC) && bC.Faces.First(f => f.Color == this.Rubik.BottomColor).Position == FacePosition.Bottom);

      // Step 2: Solve incorrect edges
      while (solvedBottomCorners.Count() < 4)
      {
        var unsolvedBottomCorners = bottomCorners.Except(solvedBottomCorners);
        var c = (unsolvedBottomCorners.FirstOrDefault(bC => bC.Position.HasFlag(CubeFlag.TopLayer)) != null)
          ? unsolvedBottomCorners.First(bC => bC.Position.HasFlag(CubeFlag.TopLayer)) : unsolvedBottomCorners.First();

        if (c.Position.Flags != this.GetTargetFlags(c))
        {
          // Rotate to top layer
          if (c.Position.HasFlag(CubeFlag.BottomLayer))
          {
            var leftFace = c.Faces.First(f => f.Position != FacePosition.Bottom && f.Color != Color.Black);
            var leftSlice = CubeFlagService.FromFacePosition(leftFace.Position);

              this.SolverMove(leftSlice, false);
            if (c.Position.HasFlag(CubeFlag.BottomLayer))
            {
                this.SolverMove(leftSlice, true);
              leftFace = c.Faces.First(f => f.Position != FacePosition.Bottom && f.Color != leftFace.Color && f.Color != Color.Black);
              leftSlice = CubeFlagService.FromFacePosition(leftFace.Position);
                this.SolverMove(leftSlice, false);
            }
              this.SolverAlgorithm("U' {0} U", CubeFlagService.ToNotationString(leftSlice));
          }

          // Rotate over target position
          var targetPos = CubeFlagService.ExceptFlag(this.GetTargetFlags(c), CubeFlag.BottomLayer);

          while (!c.Position.HasFlag(targetPos)) this.SolverMove(CubeFlag.TopLayer, true);
        }

        // Rotate to target position with the algorithm: Li Ui L U
        var leftFac = c.Faces.First(f => f.Position != FacePosition.Top && f.Position != FacePosition.Bottom && f.Color != Color.Black);

        var leftSlic = CubeFlagService.FromFacePosition(leftFac.Position);

          this.SolverMove(leftSlic, false);
        if (!c.Position.HasFlag(CubeFlag.TopLayer))
        {
            this.SolverMove(leftSlic, true);
          leftFac = c.Faces.First(f => f.Position != FacePosition.Top && f.Position != FacePosition.Bottom && f.Color != leftFac.Color && f.Color != Color.Black);
          leftSlic = CubeFlagService.FromFacePosition(leftFac.Position);
        }
        else this.SolverMove(leftSlic, true);

        while (c.Faces.First(f => f.Color == this.Rubik.BottomColor).Position != FacePosition.Bottom)
        {
          if (c.Faces.First(f => f.Color == this.Rubik.BottomColor).Position == FacePosition.Top)
          {
              this.SolverAlgorithm("{0}' U U {0} U", CubeFlagService.ToNotationString(leftSlic));
          }
          else
          {
            var frontFac = c.Faces.First(f => f.Position != FacePosition.Top && f.Position != FacePosition.Bottom
              && f.Color != Color.Black && f.Position != CubeFlagService.ToFacePosition(leftSlic));

            if (c.Faces.First(f => f.Color == this.Rubik.BottomColor).Position == frontFac.Position && !c.Position.HasFlag(CubeFlag.BottomLayer))
            {
                this.SolverAlgorithm("U' {0}' U {0}", CubeFlagService.ToNotationString(leftSlic));
            }
            else this.SolverAlgorithm("{0}' U' {0} U", CubeFlagService.ToNotationString(leftSlic));
          }
        }
        solvedBottomCorners = bottomCorners.Where(bC => bC.Position.Flags == this.GetTargetFlags(bC) && bC.Faces.First(f => f.Color == this.Rubik.BottomColor).Position == FacePosition.Bottom);
      }
    }

    private void CompleteMiddleLayer()
    {
      // Step 1: Get the egdes of the middle layer
      var middleEdges = this.Rubik.Cubes.Where(c => c.IsEdge).Where(c => this.GetTargetFlags(c).HasFlag(CubeFlag.MiddleLayer)).ToList();

      var coloredFaces = new List<Face>();
        this.Rubik.Cubes.Where(cu => cu.IsCenter).ToList().ForEach(cu => coloredFaces.Add(cu.Faces.First(f => f.Color != Color.Black)));
      var solvedMiddleEdges = middleEdges.Where(mE => mE.Position.Flags == this.GetTargetFlags(mE) && mE.Faces.Count(f => coloredFaces.Count(cf => cf.Color == f.Color && cf.Position == f.Position) == 1) == 2);

      // Step 2: solve incorrect middle edges 
      while (solvedMiddleEdges.Count() < 4)
      {
        var unsolvedMiddleEdges = middleEdges.Except(solvedMiddleEdges);
        var c = (unsolvedMiddleEdges.FirstOrDefault(cu => !cu.Position.HasFlag(CubeFlag.MiddleLayer)) != null)
          ? unsolvedMiddleEdges.First(cu => !cu.Position.HasFlag(CubeFlag.MiddleLayer)) : unsolvedMiddleEdges.First();

        // Rotate to top layer
        if (!c.Position.HasFlag(CubeFlag.TopLayer))
        {
          var frontFace = c.Faces.First(f => f.Color != Color.Black);
          var frontSlice = CubeFlagService.FromFacePosition(frontFace.Position);
          var face = c.Faces.First(f => f.Color != Color.Black && f.Color != frontFace.Color);
          var slice = CubeFlagService.FromFacePosition(face.Position);

            this.SolverAlgorithm(
                new TestScenario(this.Rubik, new LayerMove(slice, true)).TestCubePosition(c, CubeFlag.TopLayer)
                    ? "U {0} U' {0}' U' {1}' U {1}"
                    : "U' {0}' U {0} U {1} U' {1}'",
                CubeFlagService.ToNotationString(slice),
                CubeFlagService.ToNotationString(frontSlice));
        }

        // Rotate to start position for the algorithm
        var centers = this.Rubik.Cubes.Where(cu => cu.IsCenter).Where(m => m.Colors.First(co => co != Color.Black)
            == c.Faces.First(f => f.Color != Color.Black && f.Position != FacePosition.Top).Color &&
            (m.Position.Flags & ~CubeFlag.MiddleLayer) == (c.Position.Flags & ~CubeFlag.TopLayer)).ToList();

        while (!centers.Any())
        {
            this.SolverMove(CubeFlag.TopLayer, true);

          centers = this.Rubik.Cubes.Where(cu => cu.IsCenter).Where(m => m.Colors.First(co => co != Color.Black)
            == c.Faces.First(f => f.Color != Color.Black && f.Position != FacePosition.Top).Color &&
            (m.Position.Flags & ~CubeFlag.MiddleLayer) == (c.Position.Flags & ~CubeFlag.TopLayer)).ToList();
        }

        // Rotate to target position
        var frontFac = c.Faces.First(f => f.Color != Color.Black && f.Position != FacePosition.Top);
        var frontSlic = CubeFlagService.FromFacePosition(frontFac.Position);

        var slic = CubeFlagService.FirstNotInvalidFlag(this.GetTargetFlags(c), CubeFlag.MiddleLayer | frontSlic);

          this.SolverAlgorithm(
              !new TestScenario(this.Rubik, new LayerMove(CubeFlag.TopLayer, true)).TestCubePosition(c, slic)
                  ? "U {0} U' {0}' U' {1}' U {1}"
                  : "U' {0}' U {0} U {1} U' {1}'",
              CubeFlagService.ToNotationString(slic),
              CubeFlagService.ToNotationString(frontSlic));
          solvedMiddleEdges = middleEdges.Where(
              mE =>
              mE.Faces.Count(f => coloredFaces.Count(cf => cf.Color == f.Color && cf.Position == f.Position) == 1) == 2);
      }
    }

    private void SolveCrossTopLayer()
    {
      // Step 1: Get edges with the color of the top face
      var topEdges = this.Rubik.Cubes.Where(c => c.IsEdge).Where(c => c.Position.HasFlag(CubeFlag.TopLayer));

      // Step 2: Check if the cube is insoluble
      if (topEdges.Count(tE => tE.Faces.First(f => f.Position == FacePosition.Top).Color == this.Rubik.TopColor) % 2 != 0) return;

      var correctEdges = topEdges.Where(c => c.Faces.First(f => f.Position == FacePosition.Top).Color == this.Rubik.TopColor);
      var solveTopCrossAlgorithmI = new Algorithm("F R U R' U' F'");
      var solveTopCrossAlgorithmII = new Algorithm("F S R U R' U' F' S'");

      // Step 3: Solve the cross on the top layer
      if (this.Rubik.CountEdgesWithCorrectOrientation() == 0)
      {
          this.SolverAlgorithm(solveTopCrossAlgorithmI);
        correctEdges = topEdges.Where(c => c.Faces.First(f => f.Position == FacePosition.Top).Color == this.Rubik.TopColor);
      }

      if (this.Rubik.CountEdgesWithCorrectOrientation() == 2)
      {
        var firstCorrect = correctEdges.First();
        var secondCorrect = correctEdges.First(f => f != firstCorrect);

        var opposite = (from CubeFlag flag in firstCorrect.Position.GetFlags() select CubeFlagService.GetOppositeFlag(flag)).Any(pos => secondCorrect.Position.HasFlag(pos) && pos != CubeFlag.None);

          if (opposite)
        {
          while (correctEdges.Count(c => c.Position.HasFlag(CubeFlag.RightSlice)) != 1) this.SolverMove(CubeFlag.TopLayer, true);
            this.SolverAlgorithm(solveTopCrossAlgorithmI);
        }
        else
        {
          while (correctEdges.Count(c => c.Position.HasFlag(CubeFlag.RightSlice) || c.Position.HasFlag(CubeFlag.FrontSlice)) != 2) this.SolverMove(CubeFlag.TopLayer, true);
            this.SolverAlgorithm(solveTopCrossAlgorithmII);
        }
      }

      // Step 4: Move the edges of the cross to their target positions
      while (topEdges.Count(c => c.Position.Flags == this.GetTargetFlags(c)) < 4)
      {
        var CorrectEdges = topEdges.Where(c => c.Position.Flags == this.GetTargetFlags(c));
        while (CorrectEdges.Count() < 2) this.SolverMove(CubeFlag.TopLayer, true);

        var rightSlice = CubeFlagService.FromFacePosition(CorrectEdges.First().Faces
          .First(f => f.Position != FacePosition.Top && f.Color != Color.Black).Position);
          this.SolverMove(CubeFlag.TopLayer, false);

        if (CorrectEdges.Count(c => c.Position.HasFlag(rightSlice)) == 0) this.SolverMove(CubeFlag.TopLayer, true);
        else
        {
            this.SolverMove(CubeFlag.TopLayer, true);
          rightSlice = CubeFlagService.FromFacePosition(CorrectEdges.First(cE => !cE.Position.HasFlag(rightSlice)).Faces
            .First(f => f.Position != FacePosition.Top && f.Color != Color.Black).Position);
        }

        // Algorithm: R U Ri U R U U Ri
          this.SolverAlgorithm("{0} U {0}' U {0} U U {0}'", CubeFlagService.ToNotationString(rightSlice));

        while (CorrectEdges.Count() < 2) this.SolverMove(CubeFlag.TopLayer, true);
      }
    }

    private void CompleteLastLayer()
    {
      // Step 1: Get edges with the color of the top face
      var topCorners = this.Rubik.Cubes.Where(c => c.IsCorner).Where(c => c.Position.HasFlag(CubeFlag.TopLayer));

      // Step 2: Bring corners to their target position
      while (topCorners.Count(c => c.Position.Flags == this.GetTargetFlags(c)) < 4)
      {
        var correctCorners = topCorners.Where(c => c.Position.Flags == this.GetTargetFlags(c));
        CubeFlag rightSlice;
        if (correctCorners.Count() != 0)
        {
          var firstCube = correctCorners.First();
          var rightFace = firstCube.Faces.First(f => f.Color != Color.Black && f.Position != FacePosition.Top);
          rightSlice = CubeFlagService.FromFacePosition(rightFace.Position);

          if (!new TestScenario(this.Rubik, new LayerMove(rightSlice, true)).TestCubePosition(firstCube, CubeFlag.TopLayer))
          {
            rightSlice = CubeFlagService.FromFacePosition(firstCube.Faces.First(f => f.Color != rightFace.Color && f.Color != Color.Black && f.Position != FacePosition.Top).Position);
          }
        }
        else rightSlice = CubeFlag.RightSlice;

          this.SolverAlgorithm("U {0} U' {1}' U {0}' U' {1}", CubeFlagService.ToNotationString(rightSlice), CubeFlagService.ToNotationString(CubeFlagService.GetOppositeFlag(rightSlice)));
      }

      // Step 3: Orientation of the corners on the top layer
      var rightFac = topCorners.First().Faces.First(f => f.Color != Color.Black && f.Position != FacePosition.Top);
      var rightSlic = !new TestScenario(this.Rubik, new LayerMove(CubeFlagService.FromFacePosition(rightFac.Position), true)).TestCubePosition(topCorners.First(), CubeFlag.TopLayer)
        ? CubeFlagService.FromFacePosition(rightFac.Position)
        : CubeFlagService.FromFacePosition(topCorners.First().Faces.First(f => f.Color != rightFac.Color && f.Color != Color.Black && f.Position != FacePosition.Top).Position); ;

      foreach (var c in topCorners)
      {
        while (!c.Position.HasFlag(rightSlic)) this.SolverMove(CubeFlag.TopLayer, true);

        if (!new TestScenario(this.Rubik, new LayerMove(rightSlic, true)).TestCubePosition(c, CubeFlag.TopLayer)) this.SolverMove(CubeFlag.TopLayer, true);

        // Algorithm: Ri Di R D
        while (c.Faces.First(f => f.Position == FacePosition.Top).Color != this.Rubik.TopColor) this.SolverAlgorithm("{0}' D' {0} D", CubeFlagService.ToNotationString(rightSlic));
      }
      while (topCorners.Count(tC => tC.Position.Flags == this.GetTargetFlags(tC)) != 4) this.SolverMove(CubeFlag.TopLayer, true);
    }
  }
}
