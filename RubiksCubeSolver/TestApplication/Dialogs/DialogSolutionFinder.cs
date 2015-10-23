using RubiksCubeLib;
using RubiksCubeLib.RubiksCube;
using RubiksCubeLib.Solver;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TestApplication
{
    using System.ComponentModel;

    public partial class DialogSolutionFinder : Form
    {
        private readonly CubeSolver solver;

        private readonly List<PictureBox> stepImgs = new List<PictureBox>();
        private readonly List<Label> stepLabels = new List<Label>();
        private int currentIndex;

        public Algorithm Algorithm { get; private set; }

        public DialogSolutionFinder(CubeSolver solver, Rubik rubik, Form parent = null)
        {
            this.solver = solver;
            this.InitializeComponent();
            if (parent != null)
            {
                this.Owner = parent;
                this.StartPosition = FormStartPosition.CenterParent;
            }
            this.ShowInTaskbar = false;

            this.AddStepLabels(solver);

            solver.TrySolveAsync(rubik);
            solver.OnSolutionStepCompleted += this.solver_OnSolutionStepCompleted;
            solver.OnSolutionError += this.solver_OnSolutionError;

            this.stepLabels[this.currentIndex].Text = "In progress ...";
            this.stepImgs[this.currentIndex].Image = Properties.Resources.refresh;
        }

        //METHOD TOEGEVOEGD
        protected override void OnClosing(CancelEventArgs e)
        {
            this.solver.OnSolutionStepCompleted -= this.solver_OnSolutionStepCompleted;
            this.solver.OnSolutionError -= this.solver_OnSolutionError;
        }

        private void solver_OnSolutionError(object sender, SolutionErrorEventArgs e)
        {
            var currentStepImg = this.stepImgs[this.currentIndex];
            var currentStep = this.stepLabels[this.currentIndex];
            if (currentStepImg.InvokeRequired) currentStepImg.Invoke((MethodInvoker)delegate { currentStepImg.Image = Properties.Resources.cross_icon; });
            if (currentStep.InvokeRequired) currentStep.Invoke((MethodInvoker)delegate { currentStep.Text = "Failed"; });
            if (this.lblHeader.InvokeRequired) this.lblHeader.Invoke((MethodInvoker)delegate { lblHeader.Text = "Solving error."; });
            this.solver.OnSolutionStepCompleted -= this.solver_OnSolutionStepCompleted;
        }

        private void solver_OnSolutionStepCompleted(object sender, SolutionStepCompletedEventArgs e)
        {
            if (!e.Finished)
            {
                var currentStepImg = this.stepImgs[this.currentIndex];
                var currentStep = this.stepLabels[this.currentIndex];
                if (currentStepImg.InvokeRequired)
                {
                    var img = currentStepImg;
                    currentStepImg.Invoke((MethodInvoker)delegate { img.Image = Properties.Resources.ok; });
                }
                if (currentStep.InvokeRequired)
                {
                    var step = currentStep;
                    currentStep.Invoke((MethodInvoker)delegate
                        {
                            step.Text = e.Type == SolutionStepType.Standard
                                                   ? $"{e.Algorithm.Moves.Count} moves"
                                                   : string.Empty;
                        });
                }
                this.currentIndex++;

                if (this.currentIndex >= this.stepImgs.Count)
                {
                    return;
                }
                currentStepImg = this.stepImgs[this.currentIndex];
                currentStep = this.stepLabels[this.currentIndex];
                if (currentStepImg.InvokeRequired) currentStepImg.Invoke((MethodInvoker)delegate { currentStepImg.Image = Properties.Resources.refresh; });
                if (currentStep.InvokeRequired) currentStep.Invoke((MethodInvoker)delegate { currentStep.Text = "In progress ..."; });
            }
            else
            {
                if (this.lblTimeHeader.InvokeRequired)
                    this.lblTimeHeader.Invoke((MethodInvoker)delegate
                        {
                            lblTime.Text = $"{e.Milliseconds / 1000.0:f2}s";
                        });
                if (this.lblMovesHeader.InvokeRequired)
                    this.lblMovesHeader.Invoke((MethodInvoker)delegate
                        {
                            lblMoves.Text = $"{e.Algorithm.Moves.Count} moves";
                        });
                if (this.lblHeader.InvokeRequired) this.lblHeader.Invoke((MethodInvoker)delegate { lblHeader.Text = "Solution found."; });
                if (this.btnAdd.InvokeRequired) this.btnAdd.Invoke((MethodInvoker)delegate { btnAdd.Enabled = true; });
                this.solver.OnSolutionStepCompleted -= this.solver_OnSolutionStepCompleted;
                this.Algorithm = e.Algorithm;
            }
        }

        //Parameter hernoemt om interferentie uit te sluiten met property
        private void AddStepLabels(CubeSolver solvr)
        {
            this.lblSolvingMethod.Text = solvr.Name;
            // Start pos 15,62
            var y = 62;
            const int X = 15;

            foreach (var step in solvr.SolutionSteps)
            {
                var l = new Label
                {
                    AutoSize = true,
                    Font = new Font(
                                      "Segoe UI",
                                      9F,
                                      FontStyle.Regular,
                                      GraphicsUnit.Point,
                                      0),
                    Location = new Point(X, y),
                    Name = $"label{step.Key}",
                    Text = step.Key
                };
                this.panel1.Controls.Add(l);

                var p = new PictureBox
                {
                    Location = new Point(200, y),
                    Name = $"pb{step.Key}",
                    Size = new Size(17, 15),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    TabStop = false
                };
                this.panel1.Controls.Add(p);
                this.stepImgs.Add(p);

                l = new Label
                {
                    AutoSize = true,
                    Font = new Font(
                                "Segoe UI",
                                9F,
                                FontStyle.Regular,
                                GraphicsUnit.Point,
                                0),
                    Location = new Point(217, y),
                    Name = $"labelMoves{step.Key}"
                };
                this.panel1.Controls.Add(l);
                this.stepLabels.Add(l);

                y += 18;
            }

            y += 10;
            this.lblTimeHeader.Location = new Point(10, y);
            y += 18;
            this.lblMovesHeader.Location = new Point(10, y);
            this.Size = new Size(338, y + 103);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}