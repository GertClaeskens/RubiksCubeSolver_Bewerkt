using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace RubiksCubeLib.CubeModel
{
    public partial class CubeModel
    {
        private List<double> _frameTimes;
        private IEnumerable<Face3D>[] _buffer;
        private Thread _updateThread, _renderThread;
        private AutoResetEvent[] _updateHandle;
        private AutoResetEvent[] _renderHandle;
        private int _currentBufferIndex;

        /// <summary>
        /// Gets or sets the zoom
        /// </summary>
        private double zoom;

        public double Zoom
        {
            get
            {
                var min = Math.Min(this.Screen.Height, this.Screen.Width);
                return this.zoom / (3 * (min / (double)400));
            }
            set
            {
                var min = Math.Min(this.Screen.Height, this.Screen.Width);
                this.zoom = 3 * (min / (double)400) * value;
            }
        }

        /// <summary>
        /// Gets or sets the screen the renderer projects on
        /// </summary>
        public Rectangle Screen { get; private set; }

        /// <summary>
        /// Gets or sets the FPS limit
        /// </summary>
        public double MaxFps { get; set; }

        /// <summary>
        /// Gets the current FPS
        /// </summary>
        public double Fps { get; private set; }

        /// <summary>
        /// Gets if the render cycle is running
        /// </summary>
        public bool IsRunning { get; private set; }

        private void InitRenderer()
        {
            this.SetDrawingArea(this.ClientRectangle);

            this._frameTimes = new List<double>();
            this.IsRunning = false;
            this.MaxFps = 30;

            this._updateHandle = new AutoResetEvent[2];
            for (var i = 0; i < this._updateHandle.Length; i++) this._updateHandle[i] = new AutoResetEvent(false);

            this._renderHandle = new AutoResetEvent[2];
            for (var i = 0; i < this._renderHandle.Length; i++) this._renderHandle[i] = new AutoResetEvent(true);

            this._buffer = new IEnumerable<Face3D>[2];
            for (var i = 0; i < this._buffer.Length; i++) this._buffer[i] = new List<Face3D>();
        }

        /// <summary>
        /// Reset the rendering screen
        /// </summary>
        /// <param name="screen">Screen measures</param>
        public void SetDrawingArea(Rectangle screen)
        {
            this.Screen = new Rectangle(screen.X, screen.Y, screen.Width, screen.Height - 50);
            this.Zoom = 1;
            if (screen.Width > screen.Height)
                screen.X = (screen.Width - screen.Height) / 2;
            else if (screen.Height > screen.Width)
                screen.Y = (screen.Height - screen.Width) / 2;
        }

        /// <summary>
        /// Starts the render cycle
        /// </summary>
        public void StartRender()
        {
            if (this.IsRunning)
            {
                return;
            }
            this.IsRunning = true;
            this._updateThread = new Thread(this.UpdateLoop);
            this._updateThread.Start();
            this._renderThread = new Thread(this.RenderLoop);
            this._renderThread.Start();
        }

        /// <summary>
        /// Stops the render cycle
        /// </summary>
        public void StopRender()
        {
            if (!this.IsRunning)
            {
                return;
            }
            this.IsRunning = false;
            this._updateThread.Join();
            this._renderThread.Join();
            this.Fps = 0;
            this._frameTimes.Clear();
        }

        /// <summary>
        /// Aborts the render cycle
        /// </summary>
        public void AbortRender()
        {
            if (!this.IsRunning)
            {
                return;
            }
            this.IsRunning = false;
            this.Fps = 0;
            this._frameTimes.Clear();
            this._updateThread.Abort();
            this._renderThread.Abort();
        }

        private readonly Stopwatch _sw = new Stopwatch();

        private void RenderLoop()
        {
            //DEZE METHOD NOG NAKIJKEN
            var bufferIndex = 0x0;

            while (this.IsRunning)
            {
                this._sw.Restart();
                this.Render(bufferIndex);
                bufferIndex ^= 0x1;

                //double start = _sw.Elapsed.TotalMilliseconds;
                //while (_sw.Elapsed.TotalMilliseconds < start + 20) { } // 20 ms timeout for rendering other UI controls

                //AANGEPAST ONDERSTAANDE 2 REGELS GECOMMENT
                var minTime = 1000.0 / this.MaxFps;
                while (_sw.Elapsed.TotalMilliseconds < minTime) { } // keep max fps

                this._sw.Stop();

                this._frameTimes.Add(this._sw.Elapsed.TotalMilliseconds);
                var counter = 0;
                var index = this._frameTimes.Count - 1;
                double ms = 0;
                while (index >= 0 && ms + this._frameTimes[index] <= 1000)
                {
                    ms += this._frameTimes[index];
                    counter++;
                    index--;
                }
                if (index > 0) this._frameTimes.RemoveRange(0, index);
                this.Fps = counter + ((1000 - ms) / this._frameTimes[0]);
            }
        }

        private void UpdateLoop()
        {
            var bufferIndex = 0x0;
            this._currentBufferIndex = 0x1;

            while (this.IsRunning)
            {
                this.Update(bufferIndex);
                this._currentBufferIndex = bufferIndex;
                bufferIndex ^= 0x1;
            }
        }

        private void Render(int bufferIndex)
        {
            this._updateHandle[bufferIndex].WaitOne();
            this.Invalidate();
            this._renderHandle[bufferIndex].Set();
        }

        private void Update(int bufferIndex)
        {
            this._renderHandle[bufferIndex].WaitOne();

            if (this.Moves.Count > 0)
            {
                var currentRotation = this.Moves.Peek();

                foreach (var rotation in currentRotation.Moves)
                {
                    var step = rotation.Target / (currentRotation.Milliseconds / 1000.0 * this.Fps);
                    this.LayerRotation[rotation.Move.Layer] += step;
                }

                if (this.RotationIsFinished(currentRotation.Moves))
                    this.RotationFinished(this.Moves.Dequeue());
            }

            if (this.DrawingMode == DrawingMode.ThreeDimensional)
            {
                this._buffer[bufferIndex] = this.GenFacesProjected(this.Screen, this.zoom);
            }
            else
            {
                var faces = new List<Face3D>();
                foreach (var c in this.Rubik.Cubes)
                    faces.AddRange(c.Faces.Where(f => c.Position.Flags.HasFlag(CubeFlagService.FromFacePosition(f.Position))).Select(f => new Face3D(null, f.Color, f.Position, c.Position.Flags)));
                this._buffer[bufferIndex] = faces;
            }

            this._updateHandle[bufferIndex].Set();
        }

        private bool RotationIsFinished(IEnumerable<AnimatedLayerMove> moves) => moves.All(m => m.Target > 0 && this.LayerRotation[m.Move.Layer] >= m.Target || m.Target < 0 && this.LayerRotation[m.Move.Layer] <= m.Target);

        private void RotationFinished(RotationInfo move)
        {
            this.ResetLayerRotation();
            foreach (var m in move.Moves)
            {
                this.Rubik.RotateLayer(new LayerMove(m.Move.Layer, m.Move.Direction, m.Move.Twice));
            }
            this._selections.Reset();

            this.State = this.Moves.Count > 0 ? $"Rotating {this.Moves.Peek().Name}" : "Ready";
            if (this.Moves.Count < 1) this.MouseHandling = true;
        }
    }
}