namespace RubiksCubeLib
{
    /// <summary>
    /// Describes the position of a face and the position of its parent cube
    /// </summary>
    public struct PositionSpec
    {
        // *** PROPERTIES ***

        /// <summary>
        /// Describes the CubePostion
        /// </summary>
        public CubeFlag CubePosition { get; set; }

        /// <summary>
        /// Describes the FacePosition
        /// </summary>
        public FacePosition FacePosition { get; set; }

        /// <summary>
        /// Returns the default position
        /// </summary>
        public static PositionSpec Default => new PositionSpec { CubePosition = CubeFlag.None, FacePosition = FacePosition.None };

        /// <summary>
        /// Returns true if this position is the default one
        /// </summary>
        public bool IsDefault => (this.CubePosition == CubeFlag.None || this.FacePosition == FacePosition.None);

        // *** METHODS ***

        /// <summary>
        /// Returns true if this and given PositionSpec is equal
        /// </summary>
        /// <param name="compare">Defines the PositionSpec to be compared with</param>
        /// <returns></returns>
        public bool Equals(PositionSpec compare) => (compare.CubePosition == this.CubePosition && compare.FacePosition == this.FacePosition);
    }
}