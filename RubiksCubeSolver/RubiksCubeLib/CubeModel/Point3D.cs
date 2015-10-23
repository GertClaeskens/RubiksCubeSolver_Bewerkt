using System;

namespace RubiksCubeLib.CubeModel
{
    /// <summary>
    /// Represents a 3D point
    /// </summary>
    [Serializable]
    public class Point3D
    {
        // *** CONSTRUCTOR ***

        /// <summary>
        /// Initializes a new instance of the Point3D class
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="z">Z coordinate</param>
        public Point3D(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        // *** PROPERTIES ***

        /// <summary>
        /// Gets or sets the X coordinate of the 3D point
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Gets or sets the Y coordinate of the 3D point
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Gets or sets the Z coordinate of the 3D point
        /// </summary>
        public double Z { get; set; }

        // *** METHODS ***

        /// <summary>
        /// Rotates the point around a particular axis
        /// </summary>
        /// <param name="type">Rotation axis</param>
        /// <param name="angleInDeg">Angle to be rotated</param>
        public void Rotate(RotationType type, double angleInDeg)
        {
            // Rotation matrix: http://de.wikipedia.org/wiki/Drehmatrix
            var rad = angleInDeg * Math.PI / 180;
            var cosa = Math.Cos(rad);
            var sina = Math.Sin(rad);

            //Deze nog nakijken waarom nieuw punt declareren als ook gaat met de X,Y,Z?
            //var old = new Point3D(this.X, this.Y, this.Z);
            var x = this.X;
            var y = this.Y;
            var z = this.Z;

            switch (type)
            {
                case RotationType.X:
                    this.Y = y * cosa - z * sina;
                    this.Z = y * sina + z * cosa;
                    break;

                case RotationType.Y:
                    this.X = z * sina + x * cosa;
                    this.Z = z * cosa - x * sina;
                    break;

                case RotationType.Z:
                    this.X = x * cosa - y * sina;
                    this.Y = x * sina + y * cosa;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            //GC.Collect();
        }

        /// <summary>
        /// Projects the 3D point to 2D view
        /// </summary>
        /// <param name="viewWidth">Width of projection screen</param>
        /// <param name="viewHeight">Height of projection screen</param>
        /// <param name="fov">Factor</param>
        /// <param name="viewDistance">View distance to point</param>
        /// <param name="scale">Scale</param>
        /// <returns>Projected point</returns>
        public Point3D Project(int viewWidth, int viewHeight, int fov, int viewDistance, double scale)
        {
            var factor = fov / (viewDistance + this.Z) * scale;
            var Xn = this.X * factor + viewWidth / 2;
            var Yn = this.Y * factor + viewHeight / 2;
            return new Point3D(Xn, Yn, this.Z);
        }
    }
}