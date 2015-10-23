using RubiksCubeLib.RubiksCube;
using System;
using System.Linq;

namespace RubiksCubeLib.Solver
{
    public class TestScenario
    {
        public Rubik Rubik { get; }
        public Algorithm Algorithm { get; set; }

        public TestScenario(Rubik rubik, Algorithm moves)
        {
            this.Rubik = rubik.DeepClone();
            this.Algorithm = moves;
        }

        public TestScenario(Rubik rubik, LayerMove move) : this(rubik, new Algorithm(move.ToString()))
        {
        }

        public TestScenario(Rubik rubik) : this(rubik, new Algorithm())
        {
        }

        public bool Test(Func<Rubik, bool> func)
        {
            foreach (var move in this.Algorithm.Moves.Cast<LayerMove>())
            {
                this.Rubik.RotateLayer(move);
            }
            return func(this.Rubik);
        }

        public bool TestCubePosition(Cube c, CubeFlag endPos)
        {
            foreach (var move in this.Algorithm.Moves.Cast<LayerMove>())
            {
                this.Rubik.RotateLayer(move);
            }
            var result = this.RefreshCube(c).Position.HasFlag(endPos);
            return result;
        }

        private Cube RefreshCube(Cube c) => this.Rubik.Cubes.First(cu => CollectionMethods.ScrambledEquals(cu.Colors, c.Colors));
    }
}