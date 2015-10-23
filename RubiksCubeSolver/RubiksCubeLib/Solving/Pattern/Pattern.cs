using RubiksCubeLib.RubiksCube;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace RubiksCubeLib.Solver
{
    /// <summary>
    /// Represents a cube pattern
    /// </summary>
    [Serializable]
    public class Pattern
    {
        #region Position definitions

        /// <summary>
        /// Gets all possible edge positions
        /// </summary>
        public static IEnumerable<CubePosition> EdgePositions => new List<CubePosition>()
                                                                 {
                                                                     new CubePosition(CubeFlag.MiddleSliceSides,CubeFlag.TopLayer, CubeFlag.BackSlice),
                                                                     new CubePosition(CubeFlag.RightSlice,CubeFlag.TopLayer, CubeFlag.MiddleSlice),
                                                                     new CubePosition(CubeFlag.MiddleSliceSides,CubeFlag.TopLayer, CubeFlag.FrontSlice),
                                                                     new CubePosition(CubeFlag.LeftSlice,CubeFlag.TopLayer, CubeFlag.MiddleSlice),

                                                                     new CubePosition(CubeFlag.LeftSlice,CubeFlag.MiddleLayer, CubeFlag.BackSlice),
                                                                     new CubePosition(CubeFlag.RightSlice,CubeFlag.MiddleLayer, CubeFlag.BackSlice),
                                                                     new CubePosition(CubeFlag.RightSlice,CubeFlag.MiddleLayer, CubeFlag.FrontSlice),
                                                                     new CubePosition(CubeFlag.LeftSlice,CubeFlag.MiddleLayer, CubeFlag.FrontSlice),

                                                                     new CubePosition(CubeFlag.MiddleSliceSides,CubeFlag.BottomLayer, CubeFlag.BackSlice),
                                                                     new CubePosition(CubeFlag.RightSlice,CubeFlag.BottomLayer, CubeFlag.MiddleSlice),
                                                                     new CubePosition(CubeFlag.MiddleSliceSides,CubeFlag.BottomLayer, CubeFlag.FrontSlice),
                                                                     new CubePosition(CubeFlag.LeftSlice,CubeFlag.BottomLayer, CubeFlag.MiddleSlice),
                                                                 };

        /// <summary>
        /// Gets all possible corner positions
        /// </summary>
        public static IEnumerable<CubePosition> CornerPositions => new List<CubePosition>()
                                                                   {
                                                                       new CubePosition(CubeFlag.LeftSlice,CubeFlag.TopLayer, CubeFlag.BackSlice),
                                                                       new CubePosition(CubeFlag.RightSlice,CubeFlag.TopLayer, CubeFlag.BackSlice),
                                                                       new CubePosition(CubeFlag.RightSlice,CubeFlag.TopLayer, CubeFlag.FrontSlice),
                                                                       new CubePosition(CubeFlag.LeftSlice,CubeFlag.TopLayer, CubeFlag.FrontSlice),

                                                                       new CubePosition(CubeFlag.LeftSlice,CubeFlag.BottomLayer, CubeFlag.BackSlice),
                                                                       new CubePosition(CubeFlag.RightSlice,CubeFlag.BottomLayer, CubeFlag.BackSlice),
                                                                       new CubePosition(CubeFlag.RightSlice,CubeFlag.BottomLayer, CubeFlag.FrontSlice),
                                                                       new CubePosition(CubeFlag.LeftSlice,CubeFlag.BottomLayer, CubeFlag.FrontSlice),
                                                                   };

        /// <summary>
        /// Gets all possible cube positions
        /// </summary>
        public static IEnumerable<CubePosition> Positions => CornerPositions.Union(EdgePositions);

        #endregion Position definitions

        /// <summary>
        /// Gets or sets the specific pattern elements
        /// </summary>
        public List<PatternItem> Items { get; set; }

        /// <summary>
        /// Gets the number of required inversions
        /// </summary>
        public int Inversions => this.CornerInversions + this.EdgeInversions;

        /// <summary>
        /// Gets the number of required corner inversions
        /// </summary>
        public int CornerInversions
        {
            get
            {
                var newOrder = (from p in CornerPositions let affected = this.Items.FirstOrDefault(i => i.TargetPosition == p.Flags) select affected != null ? affected.CurrentPosition : p into pos select pos.Flags).ToList();

                return CountInversions(CornerPositions.Select(pos => pos.Flags).ToList(), newOrder);
            }
        }

        /// <summary>
        /// Gets the number of required edge inversions
        /// </summary>
        public int EdgeInversions
        {
            get
            {
                var newOrder = (from p in EdgePositions let affected = this.Items.FirstOrDefault(i => i.TargetPosition == p.Flags) select affected != null ? affected.CurrentPosition : p into pos select pos.Flags).ToList();

                return CountInversions(EdgePositions.Select(pos => pos.Flags).ToList(), newOrder);
            }
        }

        /// <summary>
        /// Gets the number of required 120° corner rotations
        /// </summary>
        public int CornerRotations => this.Items.Where(i => CornerPositions.Contains(i.CurrentPosition)).Sum(c => (int)c.CurrentOrientation);

        /// <summary>
        /// Gets the number of flipped edges
        /// </summary>
        public int EdgeFlips => this.Items.Where(i => EdgePositions.Contains(i.CurrentPosition)).Sum(c => (int)c.CurrentOrientation);

        /// <summary>
        /// True, if pattern is possible
        /// </summary>
        public bool IsPossible => this.Inversions % 2 == 0 && this.CornerRotations % 3 == 0 && this.EdgeFlips % 2 == 0;

        /// <summary>
        /// Gets or sets the probalitiy of the pattern
        /// </summary>
        public double Probability { get; set; }

        /// <summary>
        /// Initializes a new instance of the Pattern class
        /// </summary>
        /// <param name="pattern">
        /// All pattern elements in the following format:
        /// Cube positions as string: "RFT" => Right | Front | Top
        /// Orientations as string "0" => 0 (max value = 2)
        /// 1: "currentPos, targetPos, currentOrientation"
        /// 2: "currentPos, currentOrientation" => any target position
        /// 3: "currentPos, targetPos" => any orientation
        /// </param>
        public Pattern(IEnumerable<string> pattern, double probability = 0)
        {
            this.Probability = probability;
            var newItems = pattern.Select(PatternItem.Parse).ToList();

            this.Items = Order(Positions, newItems).ToList();
        }

        /// <summary>
        /// Initializes a new instance of the pattern class
        /// </summary>
        public Pattern() { }

        public Pattern(IEnumerable<PatternItem> items, double probability = 0)
        {
            this.Probability = probability;
            this.Items = Order(Positions, items).ToList();
        }

        /// <summary>
        /// Converts a rubik to a pattern
        /// </summary>
        /// <param name="r">Rubik </param>
        /// <returns>The pattern of the given rubik</returns>
        public static Pattern FromRubik(Rubik r)
        {
            var p = new Pattern();
            var newItems = (from pos in Positions let cube = r.Cubes.First(c => r.GetTargetFlags(c) == pos.Flags) select new PatternItem(cube.Position, Solvability.GetOrientation(r, cube), pos.Flags)).ToList();
            p.Items = newItems;
            return p;
        }

        /// <summary>
        /// Counts the required inversions
        /// </summary>
        /// <param name="standard">Standard order of positions</param>
        /// <param name="input">Current Order of positions</param>
        /// <returns>Number of required inversions</returns>
        private static int CountInversions(IList<CubeFlag> standard, List<CubeFlag> input) => input.Select(standard.IndexOf).Select((index, i) => input.Select(standard.IndexOf).Where((index2, j) => index2 > index && j < i).Count()).Sum();

        /// <summary>
        /// True, if this pattern includes all the pattern elements of another pattern
        /// </summary>
        /// <param name="pattern">Pattern to compare</param>
        public bool IncludesAllPatternElements(Pattern pattern) => pattern.Items.All(item => this.Items.Any(i => i.Equals(item)));

        /// <summary>
        /// True, if this pattern has exactly the same pattern elemnts as the other pattern
        /// </summary>
        /// <param name="pattern">Pattern to compare</param>
        public bool Equals(Pattern pattern) => CollectionMethods.ScrambledEquals(pattern.Items, this.Items);

        /// <summary>
        /// Put to normal form
        /// </summary>
        /// <param name="standard">Normal form</param>
        /// <param name="newOrder">New order</param>
        /// <returns></returns>
        private static IEnumerable<PatternItem> Order(IEnumerable<CubePosition> standard, IEnumerable<PatternItem> newOrder)
        {
            var result = standard.Select(p => newOrder.FirstOrDefault(i => i.TargetPosition == p.Flags)).Where(affected => affected != null).ToList();
            result.AddRange(newOrder.Where(i => i.TargetPosition == CubeFlag.None));

            return result;
        }

        public Pattern DeepClone()
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, this);
                ms.Position = 0;

                return (Pattern)formatter.Deserialize(ms);
            }
        }

        /// <summary>
        /// Transforms the pattern
        /// </summary>
        /// <param name="type">Transformation axis</param>
        public Pattern Transform(CubeFlag rotationLayer)
        {
            var newPattern = this.DeepClone();
            foreach (var item in newPattern.Items)
            {
                item.Transform(rotationLayer);
            }
            return newPattern;
        }

        public void SaveXML(string path)
        {
            var serializer = new XmlSerializer(typeof(Pattern));
            using (var sw = new StreamWriter(path))
            {
                serializer.Serialize(sw, this);
            }
        }

        public static Pattern FromXml(string path)
        {
            Pattern pattern = null;
            try
            {
                var deserializer = new XmlSerializer(typeof(Pattern));
                using (var sr = new StreamReader(path))
                {
                    pattern = (Pattern)deserializer.Deserialize(sr);
                }
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Xml input error");
            }
            return pattern;
        }
    }
}