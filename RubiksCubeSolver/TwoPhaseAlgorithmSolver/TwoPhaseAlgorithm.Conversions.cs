namespace TwoPhaseAlgorithmSolver
{
    using System.Linq;

    using RubiksCubeLib;
    using RubiksCubeLib.RubiksCube;
    using RubiksCubeLib.Solver;

    public partial class TwoPhaseAlgorithm
  {
    private static CoordCube ToCoordCube(Rubik rubik)
    {
      // get corner perm and orientation
      var corners = new[] { "UFR", "UFL", "UBL", "URB", "DFR", "DFL", "DBL", "DRB" };
      var cornerPermutation = new byte[N_CORNER];
      var cornerOrientation = new byte[N_CORNER];
      for (var i = 0; i < N_CORNER; i++)
      {
        var pos = CubeFlagService.Parse(corners[i]);
        var matchingCube = rubik.Cubes.First(c => c.Position.Flags == pos);
        var targetPos = rubik.GetTargetFlags(matchingCube);
        cornerOrientation[i] = (byte)Solvability.GetOrientation(rubik, matchingCube);

        for (var j = 0; j < N_CORNER; j++)
          if (corners[j] == CubeFlagService.ToNotationString(targetPos))
            cornerPermutation[i] = (byte)(j + 1);
      }

      // get edge perm and orientation
      var edges = new[] { "UR", "UF", "UL", "UB", "DR", "DF", "DL", "DB", "FR", "FL", "BL", "RB" };
      var edgePermutation = new byte[N_EDGE];
      var edgeOrientation = new byte[N_EDGE];
      for (var i = 0; i < N_EDGE; i++)
      {
        var pos = CubeFlagService.Parse(edges[i]);
        var matchingCube = rubik.Cubes.Where(c => c.IsEdge).First(c => c.Position.Flags.HasFlag(pos));
        var targetPos = rubik.GetTargetFlags(matchingCube);
        edgeOrientation[i] = (byte)Solvability.GetOrientation(rubik, matchingCube);

        for (var j = 0; j < N_EDGE; j++)
          if (CubeFlagService.ToNotationString(targetPos).Contains(edges[j]))
            edgePermutation[i] = (byte)(j + 1);
      }

      //var cornerInv = CoordCube.ToInversions(cornerPermutation);
      //var edgeInv = CoordCube.ToInversions(edgePermutation);

      return new CoordCube(cornerPermutation, edgePermutation, cornerOrientation, edgeOrientation);
    }

    private static LayerMove IntsToLayerMove(int axis, int power)
    {
      var axes = new[] { "U", "R", "F", "D", "L", "B" };
      var newMove = LayerMove.Parse($"{axes[axis]}{(power == 3 ? "'" : power == 2 ? "2" : "")}");
      return newMove;
    }
  }
}
