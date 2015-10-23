namespace RubiksCubeLib.CubeModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Linq;
    using System.Windows.Forms;

    using RubiksCubeLib.RubiksCube;
    using RubiksCubeLib.Solver;

    /// <summary>
    /// Represents a 3D rubiks cube
    /// </summary>
    public partial class CubeModel : UserControl
    {
        // *** CONSTRUCTORS ***

        /// <summary>
        /// Initializes a new instance of the CubeModel class
        /// </summary>
        public CubeModel() : this(new Rubik()) { }

        /// <summary>
        /// Initializes a new instance of the CubeModel class
        /// </summary>
        /// <param name="rubik">Rubik's Cube that has to be drawn</param>
        public CubeModel(Rubik rubik)
        {
            this.Rubik = rubik;
            this.Rotation = new double[] { 0, 0, 0 };
            this.Moves = new Queue<RotationInfo>();
            this.MouseHandling = true;
            this.DrawingMode = DrawingMode.ThreeDimensional;
            this.State = "Ready";
            this.RotationSpeed = 250;

            this.InitColorPicker();
            this.ResetLayerRotation();
            this.InitSelection();
            this.InitRenderer();

            this.InitializeComponent();
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.SetStyle(ControlStyles.UserPaint, true);
        }

        // *** PRIVATE FIELDS ***

        private SelectionCollection _selections;

        #region Aanpassingen Probleem 1

        private readonly Brush[] _brushes =
            {
                new SolidBrush(Color.Orange), new SolidBrush(Color.Red),
                new SolidBrush(Color.Yellow), new SolidBrush(Color.White),
                new SolidBrush(Color.Blue), new SolidBrush(Color.Green),
                new HatchBrush(HatchStyle.Percent75, Color.Black, Color.Orange),
                new HatchBrush(HatchStyle.Percent75, Color.Black, Color.Red),
                new HatchBrush(HatchStyle.Percent75, Color.Black, Color.Yellow),
                new HatchBrush(HatchStyle.Percent75, Color.Black, Color.White),
                new HatchBrush(HatchStyle.Percent75, Color.Black, Color.Blue),
                new HatchBrush(HatchStyle.Percent75, Color.Black, Color.Green),
                new SolidBrush(Color.FromArgb(Color.Orange.A, (int)(Color.Orange.R * 0.3), (int)(Color.Orange.G * 0.3), (int)(Color.Orange.B * 0.3))),
                new SolidBrush(Color.FromArgb(Color.Red.A, (int)(Color.Red.R * 0.3), (int)(Color.Red.G * 0.3), (int)(Color.Red.B * 0.3))),
                new SolidBrush(Color.FromArgb(Color.Yellow.A, (int)(Color.Yellow.R * 0.3), (int)(Color.Yellow.G * 0.3), (int)(Color.Yellow.B * 0.3))),
                new SolidBrush(Color.FromArgb(Color.White.A, (int)(Color.White.R * 0.3), (int)(Color.White.G * 0.3), (int)(Color.White.B * 0.3))),
                new SolidBrush(Color.FromArgb(Color.Blue.A, (int)(Color.Blue.R * 0.3), (int)(Color.Blue.G * 0.3), (int)(Color.Blue.B * 0.3))),
                new SolidBrush(Color.FromArgb(Color.Green.A, (int)(Color.Green.R * 0.3), (int)(Color.Green.G * 0.3), (int)(Color.Green.B * 0.3))),
                new HatchBrush(HatchStyle.Percent30, Color.Black, Color.Orange),
                new HatchBrush(HatchStyle.Percent30, Color.Black, Color.Red),
                new HatchBrush(HatchStyle.Percent30, Color.Black, Color.Yellow),
                new HatchBrush(HatchStyle.Percent30, Color.Black, Color.White),
                new HatchBrush(HatchStyle.Percent30, Color.Black, Color.Blue),
                new HatchBrush(HatchStyle.Percent30, Color.Black, Color.Green)
    };

        #endregion Aanpassingen Probleem 1

        // *** PROPERTIES ***

        /// <summary>
        /// Gets the rotation angles in the direction of x, y and z
        /// </summary>
        public double[] Rotation { get; private set; }

        /// <summary>
        /// Gets the rotation angles of the specific layers
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Dictionary<CubeFlag, double> LayerRotation { get; private set; }

        /// <summary>
        /// Gets the moves in the queue
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Queue<RotationInfo> Moves { get; private set; }

        public BindingList<RotationInfo> MovesList
        {
            get
            {
                var moves = new BindingList<RotationInfo>();
                foreach (var info in this.Moves)
                    moves.Add(info);
                return moves;
            }
        }

        /// <summary>
        /// Gets the information about the drawn Rubik's Cube
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Rubik Rubik { get; private set; }

        /// <summary>
        /// Gets the current state of the rubik
        /// </summary>
        public string State { get; private set; }

        /// <summary>
        /// Gets or sets the drawing mode of the rubik
        /// </summary>
        public DrawingMode DrawingMode { get; set; }

        /// <summary>
        /// Gets or sets how fast the layer rotations will be performed
        /// </summary>
        public int RotationSpeed { get; set; }

        // *** METHODS ***

        /// <summary>
        /// Resets the Rubik object and reset all cube and layer rotations and all selections
        /// </summary>
        public void ResetCube()
        {
            this.Rubik = new Rubik();
            this.Rotation = new double[] { 0, 0, 0 };
            this.Moves = new Queue<RotationInfo>();
            this.MouseHandling = true;
            this.State = "Ready";
            this.Zoom = 1.0;
            this.ResetLayerRotation();
            this.InitSelection();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            this.SetDrawingArea(this.ClientRectangle);
            this.Invalidate();
            base.OnSizeChanged(e);
        }

        public void LoadPattern(string path)
        {
            this.Rubik = Rubik.FromPattern(Pattern.FromXml(path));
        }

        public void SavePattern(string path)
        {
            Pattern.FromRubik(this.Rubik).SaveXML(path);
        }

        /// <summary>
        /// Resets the rotation of specific layers
        /// </summary>
        private void ResetLayerRotation()
        {
            this.LayerRotation = new Dictionary<CubeFlag, double>();
            foreach (var rp in ((CubeFlag[])Enum.GetValues(typeof(CubeFlag))).Where(rp => rp != CubeFlag.XFlags && rp != CubeFlag.YFlags && rp != CubeFlag.ZFlags))
            {
                this.LayerRotation[rp] = 0;
            }
        }

        /// <summary>
        /// Executes the rotation on the real Rubik when the animation finished
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRotatingFinished(object sender, RotationFinishedEventArgs e)
        {
            this.ResetLayerRotation();
            foreach (var m in e.Info.Moves)
            {
                this.Rubik.RotateLayer(new LayerMove(m.Move.Layer, m.Move.Direction, m.Move.Twice));
            }
            this._selections.Reset();
        }

        // ** SELECTION **

        /// <summary>
        /// Add for all 54 sub faces of the cube an entry to a selection collection and set the selection to None
        /// </summary>
        private void InitSelection()
        {
            this._selections = new SelectionCollection();
            this.Rubik.Cubes.ForEach(c => c.Faces.ToList().ForEach(f =>
              {
                  if (f.Color != Color.Black) this._selections.Add(new PositionSpec { CubePosition = c.Position.Flags, FacePosition = f.Position }, Selection.None);
              }));
        }

        /// <summary>
        /// Set a selection to all entries in the selection collection
        /// </summary>
        /// <param name="selection">New selection</param>
        private void ResetFaceSelection(Selection selection)
        {
            this.Rubik.Cubes.ForEach(c => c.Faces.Where(f => f.Color != Color.Black).ToList().ForEach(f =>
              {
                  var pos = new PositionSpec { FacePosition = f.Position, CubePosition = c.Position.Flags };

                  if (this._selections[pos].HasFlag(Selection.Possible))
                  {
                      this._selections[pos] = Selection.Possible | selection;
                  }
                  else if (this._selections[pos].HasFlag(Selection.NotPossible))
                  {
                      this._selections[pos] = selection | Selection.NotPossible;
                  }
                  else
                  {
                      this._selections[pos] = selection;
                  }
              }));
        }

        /// <summary>
        /// Updates the selection
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this._selections.Reset();
                this._oldSelection = PositionSpec.Default;
                this._currentSelection = PositionSpec.Default;
            }
            base.OnKeyDown(e);
        }

        // ** COLOR PICKER **

        private void InitColorPicker()
        {
            this.ContextMenuStrip = new ContextMenuStrip();
            foreach (var col in this.Rubik.Colors)
            {
                //using gebruikt om te zorgen dat bmp ge disposed wordt
                using (var bmp = new Bitmap(16, 16))
                {
                    var g = Graphics.FromImage(bmp);
                    g.Clear(col);
                    g.DrawRectangle(Pens.Black, 0, 0, 15, 15);
                    this.ContextMenuStrip.Items.Add(col.Name, bmp);
                }
            }
            this.ContextMenuStrip.Opening += this.ContextMenuStrip_Opening;
            this.ContextMenuStrip.ItemClicked += this.ContextMenuStrip_ItemClicked;
            this.ContextMenuStrip.Closed += ((s, e) => this.MouseHandling = true);
        }

        private void ContextMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var col = this.Rubik.Colors.First();
            foreach (var t in this.Rubik.Colors.Where(t => e.ClickedItem.Text == t.Name))
            {
                col = t;
            }
            this.Rubik.SetFaceColor(this._currentSelection.CubePosition, this._currentSelection.FacePosition, col);
        }

        private void ContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            if ((ModifierKeys & Keys.Shift) != 0 && !this._currentSelection.IsDefault)
            {
                var clr = this.Rubik.GetFaceColor(this._currentSelection.CubePosition, this._currentSelection.FacePosition);
                for (var i = 0; i < this.ContextMenuStrip.Items.Count; i++)
                {
                    ((ToolStripMenuItem)this.ContextMenuStrip.Items[i]).Checked = (this.ContextMenuStrip.Items[i].Text == clr.Name);
                }
                this.MouseHandling = false;
                this._oldMousePos = new Point(-1, -1);
                this.ContextMenuStrip.Show(Cursor.Position);
            }
            else
                e.Cancel = true;
        }

        // ** RENDERING **

        /// <summary>
        /// Updates the cubeModel (including the selection)
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (this._buffer[this._currentBufferIndex] == null)
            {
                return;
            }
            var threeD = this.DrawingMode == DrawingMode.ThreeDimensional;
            var selectedPos = threeD ? this.Render(e.Graphics, this._buffer[this._currentBufferIndex], this.PointToClient(Cursor.Position))
                                  : this.Render2D(e.Graphics, this._buffer[this._currentBufferIndex], this.PointToClient(Cursor.Position));

            // disallow changes of current selection while color picker is visible
            if (this.ContextMenuStrip.Visible || !this.MouseHandling)
            {
                return;
            }
            this.ResetFaceSelection(Selection.None);

            // set selections
            this._selections[this._oldSelection] = Selection.Second;
            this._selections[selectedPos] |= Selection.First;
            this._currentSelection = selectedPos;
        }

        /// <summary>
        /// Renders the current frame
        /// </summary>
        /// <param name="g">Graphics</param>
        /// <param name="frame">Frame to render</param>
        /// <param name="mousePos">Current mouse position</param>
        /// <returns></returns>

        private PositionSpec Render(Graphics g, IEnumerable<Face3D> frame, Point mousePos)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var pos = PositionSpec.Default;
            var _brushIndex = 0;
            foreach (var face in frame)
            {
                var parr = face.Vertices.Select(p => new PointF((float)p.X, (float)p.Y)).ToArray();
                //Verbetering 1 : Minder Calls om objecten van BRush te Disposen

                if (face.Color == Color.Orange)
                {
                    _brushIndex = 0;
                }
                else if (face.Color == Color.Red)
                {
                    _brushIndex = 1;
                }
                else if (face.Color == Color.Yellow)
                {
                    _brushIndex = 2;
                }
                else if (face.Color == Color.White)
                {
                    _brushIndex = 3;
                }
                else if (face.Color == Color.Blue)
                {
                    _brushIndex = 4;
                }
                else if (face.Color == Color.Green)
                {
                    _brushIndex = 5;
                }
                //b = _brushes[_brushIndex];

                //b = new SolidBrush(face.Color);
                var factor = ((Math.Sin(Environment.TickCount / (double)200) + 1) / 4) + 0.75;
                var facePos = new PositionSpec { FacePosition = face.Position, CubePosition = face.MasterPosition };

                Brush b;
                if (this.MouseHandling)
                {
                    if (this._selections[facePos].HasFlag(Selection.Second))
                        b = this._brushes[_brushIndex + 6];
                    else if (this._selections[facePos].HasFlag(Selection.NotPossible))
                        b = this._brushes[_brushIndex + 12];
                    else if (this._selections[facePos].HasFlag(Selection.First))
                        b = this._brushes[_brushIndex + 18];
                    else if (this._selections[facePos].HasFlag(Selection.Possible))
                        b = new SolidBrush(Color.FromArgb(face.Color.A, (int)(Math.Min(face.Color.R * factor, 255)), (int)(Math.Min(face.Color.G * factor, 255)), (int)(Math.Min(face.Color.B * factor, 255))));
                    else b = this._brushes[_brushIndex];
                }
                else
                    b = this._brushes[_brushIndex];

                g.FillPolygon(b, parr);
                using (var pen = new Pen(Color.Black, 1))
                {
                    g.DrawPolygon(pen, parr);
                }

                using (
                var gp = new GraphicsPath())
                {
                    gp.AddPolygon(parr);
                    if (gp.IsVisible(mousePos))
                        pos = facePos;
                }
            }

            using (var solidBrush = new SolidBrush(this.BackColor))
            {
                g.FillRectangle(solidBrush, 0, this.Height - 25, this.Width - 1, 24);
            }
            g.DrawRectangle(Pens.Black, 0, this.Height - 25, this.Width - 1, 24);
            g.DrawString(
                $"[{this._currentSelection.CubePosition}] | {this._currentSelection.FacePosition}", this.Font, Brushes.Black, 5, this.Height - 20);

            //Code was -> g.FillRectangle(new SolidBrush(this.BackColor), 0, this.Height - 50, this.Width - 1, 25);
            using (var solidBrush = new SolidBrush(this.BackColor))
            {
                g.FillRectangle(solidBrush, 0, this.Height - 50, this.Width - 1, 25);
            }
            g.DrawRectangle(Pens.Black, 0, this.Height - 50, this.Width - 1, 25);
            g.DrawString(this.State, this.Font, Brushes.Black, 5, this.Height - 45);

            g.DrawRectangle(Pens.Black, 0, 0, this.Width - 1, this.Height - 50);

            return pos;
        }

        public PositionSpec Render2D(Graphics g, IEnumerable<Face3D> frame, Point mousePos)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var pos = PositionSpec.Default;

            int square, borderX = 5, borderY = 5;
            if (((this.Screen.Width - 10) / (double)(this.Screen.Height - 10)) > (4.0 / 3.0))
            {
                square = (int)(this.Screen.Height / 9.0);
                borderX = (this.Screen.Width - 12 * square) / 2;
            }
            else
            {
                square = (int)(this.Screen.Width / 12.0);
                borderY = (this.Screen.Height - 9 * square) / 2;
            }

            var faces = new List<Face3D>();
            foreach (var c in this.Rubik.Cubes)
                faces.AddRange(c.Faces.Where(f => c.Position.Flags.HasFlag(CubeFlagService.FromFacePosition(f.Position))).Select(f => new Face3D(null, f.Color, f.Position, c.Position.Flags)));
            frame = faces;

            foreach (var face in frame)
            {
                Brush b = new SolidBrush(face.Color);

                #region CalculatePoints

                var x = 0;
                var y = 0;
                var xOffs = borderX;
                var yOffs = borderY;

                if (face.Position.HasFlag(FacePosition.Front))
                {
                    xOffs += 3 * square; yOffs += 3 * square;
                    var cubePos = new CubePosition(face.MasterPosition);
                    x = xOffs + (CubeFlagService.ToInt(cubePos.X) + 1) * square;
                    y = yOffs + (CubeFlagService.ToInt(cubePos.Y) * (-1) + 1) * square;
                }

                if (face.Position.HasFlag(FacePosition.Top))
                {
                    xOffs += 3 * square;
                    var cubePos = new CubePosition(face.MasterPosition);
                    x = xOffs + (CubeFlagService.ToInt(cubePos.X) + 1) * square;
                    y = yOffs + (CubeFlagService.ToInt(cubePos.Z) + 1) * square;
                }

                if (face.Position.HasFlag(FacePosition.Bottom))
                {
                    xOffs += 3 * square; yOffs += 6 * square;
                    var cubePos = new CubePosition(face.MasterPosition);
                    x = xOffs + (CubeFlagService.ToInt(cubePos.X) + 1) * square;
                    y = yOffs + (CubeFlagService.ToInt(cubePos.Z) * (-1) + 1) * square;
                }

                if (face.Position.HasFlag(FacePosition.Left))
                {
                    yOffs += 3 * square;
                    var cubePos = new CubePosition(face.MasterPosition);
                    x = xOffs + (CubeFlagService.ToInt(cubePos.Z) + 1) * square;
                    y = yOffs + (CubeFlagService.ToInt(cubePos.Y) * (-1) + 1) * square;
                }

                if (face.Position.HasFlag(FacePosition.Right))
                {
                    xOffs += 6 * square; yOffs += 3 * square;
                    var cubePos = new CubePosition(face.MasterPosition);
                    x = xOffs + (CubeFlagService.ToInt(cubePos.Z) * (-1) + 1) * square;
                    y = yOffs + (CubeFlagService.ToInt(cubePos.Y) * (-1) + 1) * square;
                }

                if (face.Position.HasFlag(FacePosition.Back))
                {
                    xOffs += 9 * square; yOffs += 3 * square;
                    var cubePos = new CubePosition(face.MasterPosition);
                    x = xOffs + (CubeFlagService.ToInt(cubePos.X) * (-1) + 1) * square;
                    y = yOffs + (CubeFlagService.ToInt(cubePos.Y) * (-1) + 1) * square;
                }

                #endregion CalculatePoints

                var parr = new[] { new Point(x, y), new Point(x, y + square), new Point(x + square, y + square), new Point(x + square, y) };

                var factor = ((Math.Sin(Environment.TickCount / (double)200) + 1) / 4) + 0.75;
                var facePos = new PositionSpec { FacePosition = face.Position, CubePosition = face.MasterPosition };

                if (this.MouseHandling)
                {
                    if (this._selections[facePos].HasFlag(Selection.Second)) b = new HatchBrush(HatchStyle.Percent75, Color.Black, face.Color);
                    else if (this._selections[facePos].HasFlag(Selection.NotPossible))
                        b =
                            new SolidBrush(
                                Color.FromArgb(
                                    face.Color.A,
                                    (int)(face.Color.R * 0.3),
                                    (int)(face.Color.G * 0.3),
                                    (int)(face.Color.B * 0.3)));
                    else if (this._selections[facePos].HasFlag(Selection.First)) b = new HatchBrush(HatchStyle.Percent30, Color.Black, face.Color);
                    else if (this._selections[facePos].HasFlag(Selection.Possible))
                        b =
                            new SolidBrush(
                                Color.FromArgb(
                                    face.Color.A,
                                    (int)(Math.Min(face.Color.R * factor, 255)),
                                    (int)(Math.Min(face.Color.G * factor, 255)),
                                    (int)(Math.Min(face.Color.B * factor, 255))));
                }
                /* Code hieronder is overbodig omdat de brush al zo ge declareerd is
                else b = new SolidBrush(face.Color);
            }
                else
                    b = new SolidBrush(face.Color);  */

                g.FillPolygon(b, parr);
                using (var pen = new Pen(Color.Black, 1))
                {
                    g.DrawPolygon(pen, parr);
                }

                using (var gp = new GraphicsPath())
                {
                    gp.AddPolygon(parr);
                    if (gp.IsVisible(mousePos))
                        pos = facePos;

                    b.Dispose();
                }
            }

            g.DrawRectangle(Pens.Black, 0, this.Height - 25, this.Width - 1, 24);
            //g.DrawLine(Pens.Black, 0, this.Height - 25, this.Width, this.Height - 25);
            g.DrawString(
                $"[{this._currentSelection.CubePosition}] | {this._currentSelection.FacePosition}", this.Font, Brushes.Black, 5, this.Height - 20);

            g.DrawRectangle(Pens.Black, 0, this.Height - 50, this.Width - 1, 25);
            g.DrawString(this.State, this.Font, Brushes.Black, 5, this.Height - 45);

            g.DrawRectangle(Pens.Black, 0, 0, this.Width - 1, this.Height - 50);

            return pos;
        }

        /// <summary>
        /// Generates 3D cubes from the Rubik cubes
        /// </summary>
        /// <returns>Cubes from the Rubik converted to 3D cubes</returns>
        private IEnumerable<Cube3D> GenCubes3D()
        {
            var cubes = this.Rubik.Cubes;

            var cubes3D = new List<Cube3D>();
            const double D = 2.0 / 3.0;
            foreach (var c in cubes)
            {
                var cr = new Cube3D(new Point3D(D * CubeFlagService.ToInt(c.Position.X), D * CubeFlagService.ToInt(c.Position.Y), D * CubeFlagService.ToInt(c.Position.Z)), D / 2, c.Position.Flags, c.Faces);
                if (cr.Position.HasFlag(CubeFlag.TopLayer))
                    cr = cr.Rotate(RotationType.Y, this.LayerRotation[CubeFlag.TopLayer], new Point3D(0, D, 0));
                if (cr.Position.HasFlag(CubeFlag.MiddleLayer))
                    cr = cr.Rotate(RotationType.Y, this.LayerRotation[CubeFlag.MiddleLayer], new Point3D(0, 0, 0));
                if (cr.Position.HasFlag(CubeFlag.BottomLayer))
                    cr = cr.Rotate(RotationType.Y, this.LayerRotation[CubeFlag.BottomLayer], new Point3D(0, -D, 0));
                if (cr.Position.HasFlag(CubeFlag.FrontSlice))
                    cr = cr.Rotate(RotationType.Z, this.LayerRotation[CubeFlag.FrontSlice], new Point3D(0, 0, D));
                if (cr.Position.HasFlag(CubeFlag.MiddleSlice))
                    cr = cr.Rotate(RotationType.Z, this.LayerRotation[CubeFlag.MiddleSlice], new Point3D(0, 0, 0));
                if (cr.Position.HasFlag(CubeFlag.BackSlice))
                    cr = cr.Rotate(RotationType.Z, this.LayerRotation[CubeFlag.BackSlice], new Point3D(0, 0, -D));
                if (cr.Position.HasFlag(CubeFlag.LeftSlice))
                    cr = cr.Rotate(RotationType.X, this.LayerRotation[CubeFlag.LeftSlice], new Point3D(-D, 0, 0));
                if (cr.Position.HasFlag(CubeFlag.MiddleSliceSides))
                    cr = cr.Rotate(RotationType.X, this.LayerRotation[CubeFlag.MiddleSliceSides], new Point3D(0, 0, 0));
                if (cr.Position.HasFlag(CubeFlag.RightSlice))
                    cr = cr.Rotate(RotationType.X, this.LayerRotation[CubeFlag.RightSlice], new Point3D(D, 0, 0));

                cr = cr.Rotate(RotationType.Y, this.Rotation[1], new Point3D(0, 0, 0));
                cr = cr.Rotate(RotationType.Z, this.Rotation[2], new Point3D(0, 0, 0));
                cr = cr.Rotate(RotationType.X, this.Rotation[0], new Point3D(0, 0, 0));
                cubes3D.Add(cr);
            }

            return cubes3D;
        }

        /// <summary>
        /// Projects the 3D cubes to 2D view for rendering
        /// </summary>
        /// <param name="screen">Render screen</param>
        /// <param name="scale">Scale</param>
        /// <returns></returns>
        public IEnumerable<Face3D> GenFacesProjected(Rectangle screen, double scale)
        {
            var cubesRender = this.GenCubes3D();
            var facesProjected = cubesRender.Select(c => c.Project(screen.Width, screen.Height, 100, 4, scale).Faces).Aggregate((a, b) => a.Concat(b));
            facesProjected = facesProjected.OrderBy(f => f.Vertices.ElementAt(0).Z).Reverse();
            return facesProjected;
        }

        // ** ENLARGE RENDER QUEUE **

        /// <summary>
        /// Registers a new animated rotation
        /// </summary>
        /// <param name="layer">Rotation layer</param>
        /// <param name="direction">Direction of rotation</param>
        public void RotateLayerAnimated(CubeFlag layer, bool direction)
        {
            this.RotateLayerAnimated(new LayerMove(layer, direction), this.RotationSpeed);
        }

        /// <summary>
        /// Registers a new single animated move or a collection of new animated rotations
        /// </summary>
        /// <param name="move">Movement that will be performed</param>
        public void RotateLayerAnimated(IMove move)
        {
            this.RotateLayerAnimated(move, this.RotationSpeed);
        }

        /// <summary>
        /// Registers a collection of new animated moves
        /// </summary>
        /// <param name="moves">Collection of moves</param>
        /// <param name="milliseconds">Duration of animated rotation</param>
        public void RotateLayerAnimated(IMove moves, int milliseconds)
        {
            this.MouseHandling = false;

            if (this.DrawingMode == DrawingMode.TwoDimensional)
                milliseconds = 0;

            this.Moves.Enqueue(new RotationInfo(moves, milliseconds));
            this.MovesList.Add(new RotationInfo(moves, milliseconds));
            this.State = $"Rotating {this.Moves.Peek().Name}";
        }
    }
}