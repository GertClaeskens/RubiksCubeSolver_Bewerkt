using System;
using System.Linq;

namespace RubiksCubeLib.Solver
{
    /// <summary>
    /// Represents an item in a pattern
    /// </summary>
    [Serializable]
    public class PatternItem
    {
        /// <summary>
        /// Gets or sets the current cube position
        /// </summary>
        public CubePosition CurrentPosition { get; set; }

        /// <summary>
        /// Gets or sets the current cube orientation
        /// </summary>
        public Orientation CurrentOrientation { get; set; }

        /// <summary>
        /// Gets or sets the target position
        /// </summary>
        public CubeFlag TargetPosition { get; set; }

        /// <summary>
        /// Initializes a new instance of the PatternItem class
        /// </summary>
        /// <param name="currPos">Current position</param>
        /// <param name="currOrientation">
        /// Current orientation
        /// Corner: Correct = 0, Clockwise = 1, Counter-Clockwise = 2
        /// Edge: Correct = 0, Flipped = 1
        /// Center always 0
        /// </param>
        /// <param name="targetPos">Target position</param>
        public PatternItem(CubePosition currPos, Orientation currOrientation, CubeFlag targetPos)
        {
            this.CurrentPosition = currPos;
            this.CurrentOrientation = currOrientation;
            this.TargetPosition = targetPos;
        }

        /// <summary>
        /// Initializes a new instance of the PatternItem class
        /// </summary>
        public PatternItem() { }

        /// <summary>
        /// Parses a string to a pattern item
        /// </summary>
        /// <param name="s">The string to be parsed
        /// 1: "currentPos, targetPos, currentOrientation"
        /// 2: "currentPos, currentOrientation" => any target position
        /// 3: "currentPos, targetPos" => any orientation
        /// </param>
        public static PatternItem Parse(string s)
        {
            var split = s.Split(',');

            switch (split.Length)
            {
                case 1:
                    {
                        var currPos = CubeFlagService.Parse(split[0]);
                        if (Pattern.Positions.Count(p => p.Flags == currPos) != 1) throw new Exception("At least one orientation or position is not possible");
                        return new PatternItem(new CubePosition(currPos), 0, currPos);
                    }
                case 2:
                    {
                        var currPos = CubeFlagService.Parse(split[0]);
                        var pos = CubeFlagService.Parse(split[1]);
                        int orientation;
                        if (Pattern.Positions.Count(p => p.Flags == currPos) != 1 || (!int.TryParse(split[1], out orientation) && Pattern.Positions.Count(p => p.Flags == pos) != 1)) throw new Exception("At least one orientation or position is not possible");
                        return new PatternItem(new CubePosition(currPos), (Orientation)orientation, pos);
                    }
                case 3:
                    {
                        // check valid cube position
                        var currPos = CubeFlagService.Parse(split[0]);
                        var targetPos = CubeFlagService.Parse(split[1]);
                        if (!Pattern.Positions.Contains(new CubePosition(currPos)) || !Pattern.Positions.Contains(new CubePosition(targetPos)))
                            throw new Exception("At least one position does not exist");

                        // check valid orientation
                        int orientation;
                        if (!int.TryParse(split[2], out orientation)) throw new Exception("At least one orientation is not possible");

                        return new PatternItem(new CubePosition(currPos), (Orientation)orientation, targetPos);
                    }
                default:
                    throw new Exception("Parsing error");
            }
        }

        /// <summary>
        /// True, if the item has the same current position and the other equality conditions
        /// </summary>
        /// <param name="item">Item to compare</param>
        public bool Equals(PatternItem item)
        {
            var sameOrientation = (item.CurrentOrientation == Orientation.None || this.CurrentOrientation == Orientation.None) || item.CurrentOrientation == this.CurrentOrientation;
            var sameTargetPos = (item.TargetPosition == CubeFlag.None || this.TargetPosition == CubeFlag.None) || item.TargetPosition == this.TargetPosition;
            var result = item.CurrentPosition.Flags == this.CurrentPosition.Flags && sameOrientation && sameTargetPos;
            return result;
        }

        /// <summary>
        /// Transforms the pattern item
        /// </summary>
        /// <param name="type">Transformation axis</param>
        public void Transform(CubeFlag rotationLayer)
        {
            if (this.CurrentPosition.HasFlag(rotationLayer))
            {
                var oldFlags = this.CurrentPosition.Flags;
                this.CurrentPosition.NextFlag(rotationLayer, true);

                var newOrientation = this.CurrentOrientation;
                if (CubePosition.IsCorner(this.CurrentPosition.Flags) && !CubeFlagService.IsYFlag(rotationLayer))
                {
                    newOrientation = new CubePosition(oldFlags).Y == new CubePosition(this.CurrentPosition.Flags).Y ? (Orientation)(((int)newOrientation + 0x2) % 3) : (Orientation)(((int)newOrientation + 1) % 3);
                }
                else if (CubePosition.IsEdge(this.CurrentPosition.Flags) && CubeFlagService.IsZFlag(rotationLayer))
                {
                    newOrientation = (Orientation)((int)newOrientation ^ 0x1);
                }

                this.CurrentOrientation = newOrientation;
            }
            if (this.TargetPosition.HasFlag(rotationLayer)) this.TargetPosition = CubeFlagService.NextFlags(this.TargetPosition, rotationLayer);
        }

        public override string ToString() => $"{this.CurrentPosition}->{this.TargetPosition} {this.CurrentOrientation}";
    }
}