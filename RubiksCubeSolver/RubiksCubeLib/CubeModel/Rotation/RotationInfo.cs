using System.Collections.Generic;

namespace RubiksCubeLib.CubeModel
{
    /// <summary>
    /// Represents all the requiered information to perform an animated layer rotation
    /// </summary>
    public class RotationInfo
    {
        /// <summary>
        /// Gets or sets the duration of animated rotation
        /// </summary>
        public int Milliseconds { get; set; }

        /// <summary>
        /// Gets or sets the collection of animated layer movements
        /// </summary>
        public List<AnimatedLayerMove> Moves { get; set; }

        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the RotationInfo class
        /// </summary>
        /// <param name="move">Move or move collection that will be rotated animated</param>
        /// <param name="milliseconds">Duration of animated rotation</param>
        public RotationInfo(IMove move, int milliseconds)
        {
            this.Milliseconds = milliseconds;
            this.Name = move.Name;
            this.Moves = new List<AnimatedLayerMove>();
            if (move.MultipleLayers)
            {
                foreach (var m in (LayerMoveCollection)move)
                {
                    this.Moves.Add(new AnimatedLayerMove(m));
                }
            }
            else
            {
                this.Moves.Add(new AnimatedLayerMove((LayerMove)move));
            }
        }
    }

    /// <summary>
    /// Represents an animated layer move
    /// </summary>
    public class AnimatedLayerMove
    {
        /// <summary>
        /// Gets the target angle of rotation in degrees
        /// </summary>
        public int Target { get; set; }

        /// <summary>
        /// Gets the layer move of the animated rotation
        /// </summary>
        public LayerMove Move { get; set; }

        /// <summary>
        /// Initializes a new instance of the AnimatedLayerMove class
        /// </summary>
        /// <param name="move">Movement that will be performed</param>
        public AnimatedLayerMove(LayerMove move)
        {
            var d = move.Direction;
            if (move.Layer == CubeFlag.TopLayer || move.Layer == CubeFlag.MiddleLayer || move.Layer == CubeFlag.LeftSlice || move.Layer == CubeFlag.FrontSlice || move.Layer == CubeFlag.MiddleSlice) d = !d;
            var rotationTarget = move.Twice ? 180 : 90;
            if (d) rotationTarget *= -1;
            this.Target = rotationTarget;

            this.Move = new LayerMove(move.Layer, move.Direction, move.Twice);
        }
    }
}