namespace RubiksCubeLib.CubeModel
{
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;

    /// <summary>
    /// Represents a 3D rubiks cube
    /// </summary>
    public partial class CubeModel
    {
        // *** PROPERTIES ***

        /// <summary>
        /// Gets or sets the permission to perform mouse activities
        /// </summary>
        public bool MouseHandling { get; set; }

        // *** PRIVATE FIELDS ***

        private PositionSpec _oldSelection = PositionSpec.Default;
        private PositionSpec _currentSelection = PositionSpec.Default;

        private Point _oldMousePos = new Point(-1, -1);

        // *** METHODS ***

        // Editing angles

        /// <summary>
        /// Converts negative angles to positive
        /// </summary>
        /// <param name="angleInDeg"></param>
        /// <returns></returns>
        private double ToPositiveAngle(double angleInDeg) => angleInDeg < 0 ? 360 + angleInDeg : angleInDeg;

        /// <summary>
        /// Rounds a angle to 90 deg up
        /// </summary>
        /// <param name="angleInDeg"></param>
        /// <returns></returns>
        private double ToNextQuarter(double angleInDeg) => Math.Round(angleInDeg / 90) * 90;

        // Handle mouse interactions

        /// <summary>
        /// Detection and execution of the rotation of the whole cube
        /// </summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (this._oldMousePos.X != -1 && this._oldMousePos.Y != -1)
            {
                if (e.Button == MouseButtons.Right)
                {
                    this.Cursor = Cursors.SizeAll;
                    var dx = e.X - this._oldMousePos.X;
                    var dy = e.Y - this._oldMousePos.Y;

                    var min = Math.Min(this.ClientRectangle.Height, this.ClientRectangle.Width);
                    var scale = (min / (double)400) * 6.0;

                    this.Rotation[1] -= (dx / scale) % 360;
                    this.Rotation[0] += (dy / scale) % 360;
                }
                else
                    this.Cursor = Cursors.Arrow;
            }

            this._oldMousePos = e.Location;
            base.OnMouseMove(e);
        }

        /// <summary>
        /// Returns true if the value is between the given bounds
        /// </summary>
        /// <param name="value">Defines the value to be checked</param>
        /// <param name="lbound">Defines the left bound (inclusive)</param>
        /// <param name="rbound">Defines the right bound (exclusive)</param>
        /// <returns></returns>
        private bool IsInRange(double value, double lbound, double rbound) => value >= lbound && value < rbound;

        /// <summary>
        /// Detection and execution of mouse-controlled layer rotations
        /// </summary>
        /// <param name="e">todo: describe e parameter on OnMouseClick</param>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (this.MouseHandling && e.Button == MouseButtons.Left)
            {
                if (this._oldSelection.IsDefault)
                {
                    if (this._currentSelection.IsDefault)
                    {
                        this._selections.Reset();
                        this._oldSelection = PositionSpec.Default;
                        this._currentSelection = PositionSpec.Default;
                    }
                    else
                    {
                        if (!CubePosition.IsCorner(this._currentSelection.CubePosition))
                        {
                            this._oldSelection = this._currentSelection;
                            this.Rubik.Cubes.ForEach(c => c.Faces.Where(f => f.Color != Color.Black).ToList().ForEach(f =>
                              {
                                  var pos = new PositionSpec { CubePosition = c.Position.Flags, FacePosition = f.Position };

                                  if (this._currentSelection.CubePosition != c.Position.Flags && !CubePosition.IsCenter(c.Position.Flags) && this._currentSelection.FacePosition == f.Position)
                                  {
                                      var assocLayer = CubeFlagService.FromFacePosition(this._currentSelection.FacePosition);
                                      var commonLayer = CubeFlagService.GetFirstNotInvalidCommonFlag(this._currentSelection.CubePosition, c.Position.Flags, assocLayer);

                                      if (commonLayer != CubeFlag.None && c.Position.HasFlag(commonLayer))
                                      {
                                          this._selections[pos] |= Selection.Possible;
                                      }
                                      else
                                      {
                                          this._selections[pos] |= Selection.NotPossible;
                                      }
                                  }
                                  else
                                  {
                                      this._selections[pos] |= Selection.NotPossible;
                                  }
                              }));
                            this.State =
                                $"First selection [{this._currentSelection.CubePosition}] | {this._currentSelection.FacePosition}";
                        }
                        else
                        {
                            this._selections.Reset();
                            this.State = "Error: Invalid first selection, must not be a corner";
                        }
                    }
                }
                else
                {
                    if (this._currentSelection.IsDefault)
                    {
                        this.State = "Ready";
                    }
                    else
                    {
                        if (this._currentSelection.CubePosition != this._oldSelection.CubePosition)
                        {
                            if (!CubePosition.IsCenter(this._currentSelection.CubePosition))
                            {
                                if (this._oldSelection.FacePosition == this._currentSelection.FacePosition)
                                {
                                    var assocLayer = CubeFlagService.FromFacePosition(this._oldSelection.FacePosition);
                                    var commonLayer = CubeFlagService.GetFirstNotInvalidCommonFlag(this._oldSelection.CubePosition, this._currentSelection.CubePosition, assocLayer);
                                    var direction = true;

                                    if (commonLayer == CubeFlag.TopLayer || commonLayer == CubeFlag.MiddleLayer || commonLayer == CubeFlag.BottomLayer)
                                    {
                                        if (((this._oldSelection.FacePosition == FacePosition.Back) && this._currentSelection.CubePosition.HasFlag(CubeFlag.RightSlice))
                                        || ((this._oldSelection.FacePosition == FacePosition.Left) && this._currentSelection.CubePosition.HasFlag(CubeFlag.BackSlice))
                                        || ((this._oldSelection.FacePosition == FacePosition.Front) && this._currentSelection.CubePosition.HasFlag(CubeFlag.LeftSlice))
                                        || ((this._oldSelection.FacePosition == FacePosition.Right) && this._currentSelection.CubePosition.HasFlag(CubeFlag.FrontSlice)))
                                            direction = false;
                                        if (commonLayer == CubeFlag.TopLayer || commonLayer == CubeFlag.MiddleLayer)
                                            direction = !direction;
                                    }

                                    if (commonLayer == CubeFlag.LeftSlice || commonLayer == CubeFlag.MiddleSliceSides || commonLayer == CubeFlag.RightSlice)
                                    {
                                        if (((this._oldSelection.FacePosition == FacePosition.Bottom) && this._currentSelection.CubePosition.HasFlag(CubeFlag.BackSlice))
                                        || ((this._oldSelection.FacePosition == FacePosition.Back) && this._currentSelection.CubePosition.HasFlag(CubeFlag.TopLayer))
                                        || ((this._oldSelection.FacePosition == FacePosition.Top) && this._currentSelection.CubePosition.HasFlag(CubeFlag.FrontSlice))
                                        || ((this._oldSelection.FacePosition == FacePosition.Front) && this._currentSelection.CubePosition.HasFlag(CubeFlag.BottomLayer)))
                                            direction = false;
                                        if (commonLayer == CubeFlag.LeftSlice)
                                            direction = !direction;
                                    }

                                    if (commonLayer == CubeFlag.BackSlice || commonLayer == CubeFlag.MiddleSlice || commonLayer == CubeFlag.FrontSlice)
                                    {
                                        if (((this._oldSelection.FacePosition == FacePosition.Top) && this._currentSelection.CubePosition.HasFlag(CubeFlag.RightSlice))
                                        || ((this._oldSelection.FacePosition == FacePosition.Right) && this._currentSelection.CubePosition.HasFlag(CubeFlag.BottomLayer))
                                        || ((this._oldSelection.FacePosition == FacePosition.Bottom) && this._currentSelection.CubePosition.HasFlag(CubeFlag.LeftSlice))
                                        || ((this._oldSelection.FacePosition == FacePosition.Left) && this._currentSelection.CubePosition.HasFlag(CubeFlag.TopLayer)))
                                            direction = false;
                                        if (commonLayer == CubeFlag.FrontSlice || commonLayer == CubeFlag.MiddleSlice)
                                            direction = !direction;
                                    }

                                    if (commonLayer != CubeFlag.None)
                                    {
                                        this.RotateLayerAnimated(commonLayer, direction);
                                    }
                                    else
                                    {
                                        this.State = "Error: Invalid second selection, does not specify distinct layer";
                                    }
                                }
                                else
                                {
                                    this.State = "Error: Invalid second selection, must match orientation of first selection";
                                }
                            }
                            else
                            {
                                this.State = "Error: Invalid second selection, must not be a center";
                            }
                        }
                        else
                        {
                            this.State = "Error: Invalid second selection, must not be first selection";
                        }
                    }
                    this._selections.Reset();
                    this._oldSelection = PositionSpec.Default;
                    this._currentSelection = PositionSpec.Default;
                }
            }


            base.OnMouseClick(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            this.zoom = Math.Min(Math.Max(0.2, this.zoom + e.Delta / 100.0), 10.0);
            base.OnMouseWheel(e);
        }
    }
}