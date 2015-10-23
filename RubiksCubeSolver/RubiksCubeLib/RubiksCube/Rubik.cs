using RubiksCubeLib.ScanInput;
using RubiksCubeLib.Solver;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace RubiksCubeLib.RubiksCube
{
    /// <summary>
    /// Defines a Rubik's Cube
    /// </summary>
    [Serializable]
    public class Rubik
    {
        // **** CONSTRUCTORS ****

        /// <summary>
        /// Empty constructor (default colors will be set)
        /// </summary>
        public Rubik() : this(Color.Orange, Color.Red, Color.Yellow, Color.White, Color.Blue, Color.Green) { }

        /// <summary>
        /// Constructor with the colors of the faces
        /// </summary>
        /// <param name="cfront">Defines the color of the front face</param>
        /// <param name="cback">Defines the color of the back face</param>
        /// <param name="ctop">Defines the color of the top face</param>
        /// <param name="cbottom">Defines the color of the bot face</param>
        /// <param name="cright">Defines the color of the right face</param>
        /// <param name="cleft">Defines the color of the left face</param>
        public Rubik(Color cfront, Color cback, Color ctop, Color cbottom, Color cright, Color cleft)
        {
            this.Colors = new[] { cfront, cback, ctop, cbottom, cright, cleft };
            this.Cubes = new List<Cube>();
            for (var i = -1; i <= 1; i++)
            {
                for (var j = -1; j <= 1; j++)
                {
                    for (var k = -1; k <= 1; k++)
                    {
                        this.Cubes.Add(new Cube(this.GenSideFlags(i, j, k)));
                    }
                }
            }
            this.SetFaceColor(CubeFlag.FrontSlice, FacePosition.Front, cfront);
            this.SetFaceColor(CubeFlag.BackSlice, FacePosition.Back, cback);
            this.SetFaceColor(CubeFlag.TopLayer, FacePosition.Top, ctop);
            this.SetFaceColor(CubeFlag.BottomLayer, FacePosition.Bottom, cbottom);
            this.SetFaceColor(CubeFlag.RightSlice, FacePosition.Right, cright);
            this.SetFaceColor(CubeFlag.LeftSlice, FacePosition.Left, cleft);
        }

        // **** PROPERTIES ****

        /// <summary>
        /// The collection of cubes of this Rubik
        /// </summary>
        public List<Cube> Cubes { get; }

        /// <summary>
        /// The colors of this Rubik
        /// </summary>
        public Color[] Colors { get; }

        /// <summary>
        /// Returns the color of the front face
        /// </summary>
        public Color FrontColor => this.GetFaceColor(CubeFlag.FrontSlice | CubeFlag.MiddleSliceSides | CubeFlag.MiddleLayer, FacePosition.Front);

        /// <summary>
        /// Returns the color of the back face
        /// </summary>
        public Color BackColor => this.GetFaceColor(CubeFlag.BackSlice | CubeFlag.MiddleSliceSides | CubeFlag.MiddleLayer, FacePosition.Back);

        /// <summary>
        /// Returns the color of the top face
        /// </summary>
        public Color TopColor => this.GetFaceColor(CubeFlag.TopLayer | CubeFlag.MiddleSliceSides | CubeFlag.MiddleSlice, FacePosition.Top);

        /// <summary>
        /// Returns the color of the bottom face
        /// </summary>
        public Color BottomColor => this.GetFaceColor(CubeFlag.BottomLayer | CubeFlag.MiddleSliceSides | CubeFlag.MiddleSlice, FacePosition.Bottom);

        /// <summary>
        /// Returns the color of the right face
        /// </summary>
        public Color RightColor => this.GetFaceColor(CubeFlag.RightSlice | CubeFlag.MiddleLayer | CubeFlag.MiddleSlice, FacePosition.Right);

        /// <summary>
        /// Returns the color of the left face
        /// </summary>
        public Color LeftColor => this.GetFaceColor(CubeFlag.LeftSlice | CubeFlag.MiddleSlice | CubeFlag.MiddleLayer, FacePosition.Left);

        // **** METHODS ****

        public static Rubik FromPattern(Pattern pattern)
        {
            var rubik = new Rubik();
            foreach (var item in pattern.Items)
            {
                if (CubePosition.IsCorner(item.CurrentPosition.Flags))
                {
                    var xyzCorrect = !((item.CurrentPosition.Y == CubeFlag.TopLayer ^ (item.CurrentPosition.X == CubeFlag.RightSlice ^ item.CurrentPosition.Z == CubeFlag.FrontSlice))
                        ^ (CubeFlagService.FirstYFlag(item.TargetPosition) == CubeFlag.TopLayer ^ (CubeFlagService.FirstXFlag(item.TargetPosition) == CubeFlag.RightSlice ^ CubeFlagService.FirstZFlag(item.TargetPosition) == CubeFlag.FrontSlice)));

                    if (item.CurrentOrientation == Orientation.Correct)
                    {
                        rubik.SetFaceColor(item.CurrentPosition.Flags, CubeFlagService.ToFacePosition(item.CurrentPosition.Y), rubik.GetSliceColor(CubeFlagService.FirstYFlag(item.TargetPosition)));

                        var x = xyzCorrect ? CubeFlagService.ToFacePosition(item.CurrentPosition.X) : CubeFlagService.ToFacePosition(item.CurrentPosition.Z);
                        var z = xyzCorrect ? CubeFlagService.ToFacePosition(item.CurrentPosition.Z) : CubeFlagService.ToFacePosition(item.CurrentPosition.X);

                        rubik.SetFaceColor(item.CurrentPosition.Flags, x, rubik.GetSliceColor(CubeFlagService.FirstXFlag(item.TargetPosition)));
                        rubik.SetFaceColor(item.CurrentPosition.Flags, z, rubik.GetSliceColor(CubeFlagService.FirstZFlag(item.TargetPosition)));
                    }
                    else
                    {
                        var corr = (item.CurrentPosition.X == CubeFlag.RightSlice ^ item.CurrentPosition.Z == CubeFlag.BackSlice) ^ item.CurrentPosition.Y == CubeFlag.BottomLayer;
                        var x = (corr ^ item.CurrentOrientation == Orientation.Clockwise) ? CubeFlagService.ToFacePosition(item.CurrentPosition.X) : CubeFlagService.ToFacePosition(item.CurrentPosition.Y);
                        var y = (corr ^ item.CurrentOrientation == Orientation.Clockwise) ? CubeFlagService.ToFacePosition(item.CurrentPosition.Z) : CubeFlagService.ToFacePosition(item.CurrentPosition.X);
                        var z = (corr ^ item.CurrentOrientation == Orientation.Clockwise) ? CubeFlagService.ToFacePosition(item.CurrentPosition.Y) : CubeFlagService.ToFacePosition(item.CurrentPosition.Z);

                        rubik.SetFaceColor(item.CurrentPosition.Flags, x, rubik.GetSliceColor(CubeFlagService.FirstXFlag(item.TargetPosition)));
                        rubik.SetFaceColor(item.CurrentPosition.Flags, y, rubik.GetSliceColor(CubeFlagService.FirstYFlag(item.TargetPosition)));
                        rubik.SetFaceColor(item.CurrentPosition.Flags, z, rubik.GetSliceColor(CubeFlagService.FirstZFlag(item.TargetPosition)));
                    }
                }

                if (CubePosition.IsEdge(item.CurrentPosition.Flags))
                {
                    var flagOne = CubeFlagService.FirstNotInvalidFlag(item.CurrentPosition.Flags, CubeFlag.MiddleLayer | CubeFlag.MiddleSlice | CubeFlag.MiddleSliceSides);
                    var faceOne = CubeFlagService.ToFacePosition(flagOne);
                    var faceTwo = CubeFlagService.ToFacePosition(CubeFlagService.FirstNotInvalidFlag(item.CurrentPosition.Flags, CubeFlag.MiddleLayer | CubeFlag.MiddleSlice | CubeFlag.MiddleSliceSides | flagOne));

                    var tFlagOne = CubeFlagService.FirstNotInvalidFlag(item.TargetPosition, CubeFlag.MiddleLayer | CubeFlag.MiddleSlice | CubeFlag.MiddleSliceSides);

                    rubik.SetFaceColor(item.CurrentPosition.Flags, faceOne, rubik.GetSliceColor(tFlagOne));
                    rubik.SetFaceColor(item.CurrentPosition.Flags, faceTwo, rubik.GetSliceColor(CubeFlagService.FirstNotInvalidFlag(item.TargetPosition, CubeFlag.MiddleLayer | CubeFlag.MiddleSlice | CubeFlag.MiddleSliceSides | tFlagOne)));

                    if (Solvability.GetOrientation(rubik, rubik.Cubes.First(c => c.Position.Flags == item.CurrentPosition.Flags)) != item.CurrentOrientation)
                    {
                        rubik.SetFaceColor(item.CurrentPosition.Flags, faceTwo, rubik.GetSliceColor(tFlagOne));
                        rubik.SetFaceColor(item.CurrentPosition.Flags, faceOne, rubik.GetSliceColor(CubeFlagService.FirstNotInvalidFlag(item.TargetPosition, CubeFlag.MiddleLayer | CubeFlag.MiddleSlice | CubeFlag.MiddleSliceSides | tFlagOne)));
                    }
                }

                if (CubePosition.IsCenter(item.CurrentPosition.Flags))
                {
                    var flag = CubeFlagService.FirstNotInvalidFlag(item.CurrentPosition.Flags, CubeFlag.MiddleLayer | CubeFlag.MiddleSlice | CubeFlag.MiddleSliceSides);
                    var tFlag = CubeFlagService.FirstNotInvalidFlag(item.TargetPosition, CubeFlag.MiddleLayer | CubeFlag.MiddleSlice | CubeFlag.MiddleSliceSides);

                    rubik.SetFaceColor(item.CurrentPosition.Flags, CubeFlagService.ToFacePosition(flag), rubik.GetSliceColor(tFlag));
                }
            }
            return rubik;
        }

        /// <summary>
        /// Returns a clone of this cube (i.e. same properties but new instance)
        /// </summary>
        /// <returns></returns>
        public Rubik DeepClone()
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, this);
                ms.Position = 0;

                return (Rubik)formatter.Deserialize(ms);
            }
        }

        /// <summary>
        /// Returns the CubeFlag of the Cube at the given 3D position
        /// </summary>
        /// <param name="i">Defines the XFlag (left to right)</param>
        /// <param name="j">Defines the YFlag (top to bottom)</param>
        /// <param name="k">Defines the ZFlag (front to back)</param>
        /// <returns></returns>
        public CubeFlag GenSideFlags(int i, int j, int k)
        {
            var p = new CubeFlag();
            switch (i)
            {
                case -1:
                    p |= CubeFlag.LeftSlice;
                    break;

                case 0:
                    p |= CubeFlag.MiddleSliceSides;
                    break;

                case 1:
                    p |= CubeFlag.RightSlice;
                    break;
            }
            switch (j)
            {
                case -1:
                    p |= CubeFlag.TopLayer;
                    break;

                case 0:
                    p |= CubeFlag.MiddleLayer;
                    break;

                case 1:
                    p |= CubeFlag.BottomLayer;
                    break;
            }
            switch (k)
            {
                case -1:
                    p |= CubeFlag.FrontSlice;
                    break;

                case 0:
                    p |= CubeFlag.MiddleSlice;
                    break;

                case 1:
                    p |= CubeFlag.BackSlice;
                    break;
            }
            return p;
        }

        /// <summary>
        /// Sets the facecolor of the given cubes and faces with the given color
        /// </summary>
        /// <param name="affected">Defines the cubes to be changed</param>
        /// <param name="face">Defines the face of the cubes to be changed</param>
        /// <param name="color">Defines the color to be set</param>
        public void SetFaceColor(CubeFlag affected, FacePosition face, Color color)
        {
            this.Cubes.Where(c => c.Position.HasFlag(affected)).ToList().ForEach(c => c.SetFaceColor(face, color));
            this.Cubes.ToList().ForEach(c => { c.Colors.Clear(); c.Faces.ToList().ForEach(f => c.Colors.Add(f.Color)); });
        }

        /// <summary>
        /// Returns the color of the face of the first cube with the given CubeFlag
        /// </summary>
        /// <param name="position">Defines the CubeFlag which filters this cubes</param>
        /// <param name="face">Defines the face to be analyzed</param>
        /// <returns></returns>
        public Color GetFaceColor(CubeFlag position, FacePosition face) => this.Cubes.First(c => c.Position.Flags == position).GetFaceColor(face);

        /// <summary>
        /// Executes the given move (rotation)
        /// </summary>
        /// <param name="move">Defines the move to be executed</param>
        public void RotateLayer(IMove move)
        {
            if (move.MultipleLayers)
            {
                var moves = (LayerMoveCollection)move;
                foreach (var m in moves)
                {
                    this.RotateLayer(m);
                }
            }
            else this.RotateLayer((LayerMove)move);
        }

        /// <summary>
        /// Executes the given LayerMove
        /// </summary>
        /// <param name="move">Defines the LayerMove to be executed</param>
        private void RotateLayer(LayerMove move)
        {
            var repetitions = move.Twice ? 2 : 1;
            for (var i = 0; i < repetitions; i++)
            {
                var affected = this.Cubes.Where(c => c.Position.HasFlag(move.Layer));
                affected.ToList().ForEach(c => c.NextPos(move.Layer, move.Direction));
            }
        }

        /// <summary>
        /// Rotates a layer of the Rubik
        /// </summary>
        /// <param name="layer">Defines the layer to be rotated on</param>
        /// <param name="direction">Defines the direction of the rotation (true == clockwise)</param>
        public void RotateLayer(CubeFlag layer, bool direction)
        {
            var affected = this.Cubes.Where(c => c.Position.HasFlag(layer));
            affected.ToList().ForEach(c => c.NextPos(layer, direction));
        }

        /// <summary>
        /// Execute a rotation of the whole cube
        /// </summary>
        /// <param name="type">Defines the axis to be rotated on</param>
        /// <param name="direction">Defines the direction of the rotation (true == clockwise)</param>
        public void RotateCube(RotationType type, bool direction)
        {
            switch (type)
            {
                case RotationType.X:
                    this.RotateLayer(new LayerMove(CubeFlag.RightSlice, direction) & new LayerMove(CubeFlag.MiddleSliceSides, direction) & new LayerMove(CubeFlag.LeftSlice, !direction));
                    break;

                case RotationType.Y:
                    this.RotateLayer(new LayerMove(CubeFlag.TopLayer, direction) & new LayerMove(CubeFlag.MiddleLayer, direction) & new LayerMove(CubeFlag.BottomLayer, !direction));
                    break;

                case RotationType.Z:
                    this.RotateLayer(new LayerMove(CubeFlag.FrontSlice, direction) & new LayerMove(CubeFlag.MiddleSlice, direction) & new LayerMove(CubeFlag.BackSlice, !direction));
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        /// <summary>
        /// Execute random LayerMoves on the cube to scramble it
        /// </summary>
        /// <param name="moves">Defines the amount of moves</param>
        public void Scramble(int moves)
        {
            var rnd = new Random();
            for (var i = 0; i < moves; i++)
            {
                this.RotateLayer(new LayerMove((CubeFlag)Math.Pow(2, rnd.Next(0, 9)), Convert.ToBoolean(rnd.Next(0, 2))));
            }
        }

        /// <summary>
        /// Returns the amount of corners of the top layer which have the right orientation
        /// </summary>
        /// <returns></returns>
        public int CountCornersWithCorrectOrientation()
        {
            var topColor = this.GetFaceColor(CubeFlag.TopLayer | CubeFlag.MiddleSliceSides | CubeFlag.MiddleSlice, FacePosition.Top);
            return this.Cubes.Count(c => c.IsCorner && c.Faces.First(f => f.Position == FacePosition.Top).Color == topColor);
        }

        /// <summary>
        /// Returns the amount of edges of the top layer which have the right orientation
        /// </summary>
        /// <returns></returns>
        public int CountEdgesWithCorrectOrientation()
        {
            var topColor = this.GetFaceColor(CubeFlag.TopLayer | CubeFlag.MiddleSliceSides | CubeFlag.MiddleSlice, FacePosition.Top);
            return this.Cubes.Count(c => c.IsEdge && c.Faces.First(f => f.Position == FacePosition.Top).Color == topColor);
        }

        /// <summary>
        /// Returns a solved Rubik
        /// </summary>
        /// <returns></returns>
        public Rubik GenStandardCube()
        {
            var standardCube = new Rubik();
            standardCube.SetFaceColor(CubeFlag.TopLayer, FacePosition.Top, this.TopColor);
            standardCube.SetFaceColor(CubeFlag.BottomLayer, FacePosition.Bottom, this.BottomColor);
            standardCube.SetFaceColor(CubeFlag.RightSlice, FacePosition.Right, this.RightColor);
            standardCube.SetFaceColor(CubeFlag.LeftSlice, FacePosition.Left, this.LeftColor);
            standardCube.SetFaceColor(CubeFlag.FrontSlice, FacePosition.Front, this.FrontColor);
            standardCube.SetFaceColor(CubeFlag.BackSlice, FacePosition.Back, this.BackColor);

            return standardCube;
        }

        /// <summary>
        /// Returns the position of given cube where it has to be when the Rubik is solved
        /// </summary>
        /// <param name="cube">Defines the cube to be analyzed</param>
        /// <returns></returns>
        public CubeFlag GetTargetFlags(Cube cube) => this.GenStandardCube().Cubes.First(cu => CollectionMethods.ScrambledEquals(cu.Colors, cube.Colors)).Position.Flags;

        /// <summary>
        /// Returns the center color of a slice
        /// </summary>
        /// <param name="slice">Defines the slice whose center color will be returned</param>
        public Color GetSliceColor(CubeFlag slice)
        {
            switch (slice)
            {
                case CubeFlag.TopLayer:
                    return this.TopColor;

                case CubeFlag.BottomLayer:
                    return this.BottomColor;

                case CubeFlag.FrontSlice:
                    return this.FrontColor;

                case CubeFlag.BackSlice:
                    return this.BackColor;

                case CubeFlag.LeftSlice:
                    return this.LeftColor;

                case CubeFlag.RightSlice:
                    return this.RightColor;

                default:
                    return Color.Black;
            }
        }

        public bool IsCorrect(Cube cube) => cube.Position.Flags == this.GetTargetFlags(cube) && Solvability.GetOrientation(this, cube) == 0;

        /// <summary>
        /// Returns a scanned Rubik
        /// </summary>
        /// <param name="scanner">Defines the scanner to be used</param>
        /// <returns></returns>
        public static Rubik Scan(CubeScanner scanner) => scanner.Scan();
    }
}