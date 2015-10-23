using System;

namespace RubiksCubeLib.CubeModel
{
    /// <summary>
    /// Represents the class that contains event data for the rotation finished event
    /// </summary>
    [Serializable]
    public class RotationFinishedEventArgs : EventArgs
    {
        /// <summary>
        /// Required information of finished rotation
        /// </summary>
        public RotationInfo Info { get; }

        /// <summary>
        /// Initializes a new instance of the RotationFinishedEventArgs
        /// </summary>
        /// <param name="info">Rotation info</param>
        public RotationFinishedEventArgs(RotationInfo info)
        {
            this.Info = info;
        }
    }
}