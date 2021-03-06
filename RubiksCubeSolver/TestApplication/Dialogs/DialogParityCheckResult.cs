﻿using RubiksCubeLib.RubiksCube;
using RubiksCubeLib.Solver;
using System;
using System.Windows.Forms;

namespace TestApplication
{
    public partial class DialogParityCheckResult : Form
    {
        public DialogParityCheckResult(Rubik rubik, Form parent = null)
        {
            InitializeComponent();
            this.ShowInTaskbar = false;
            if (parent != null)
            {
                this.Owner = parent;
                this.StartPosition = FormStartPosition.CenterParent;
            }

            // Color test
            var colors = Solvability.CorrectColors(rubik);
            lblColorTest.Text = colors ? "Passed" : "Failed";
            pbColorTest.Image = colors ? Properties.Resources.ok : Properties.Resources.cross_icon;

            if (!colors)
            {
                lblPermutationTest.Text = "Not tested";
                lblCornerTest.Text = "Not tested";
                lblEdgeTest.Text = "Not tested";

                pbCornerTest.Image = Properties.Resources.questionmark;
                pbEdgeTest.Image = Properties.Resources.questionmark;
                pbPermutationTest.Image = Properties.Resources.questionmark;
                lblHeader.Text = "This cube is unsolvable.";
            }
            else
            {
                // Permutation parity test
                var permutation = Solvability.PermutationParityTest(rubik);
                lblPermutationTest.Text = permutation ? "Passed" : "Failed";
                pbPermutationTest.Image = permutation ? Properties.Resources.ok : Properties.Resources.cross_icon;

                // Corner parity test
                var corner = Solvability.CornerParityTest(rubik);
                lblCornerTest.Text = corner ? "Passed" : "Failed";
                pbCornerTest.Image = corner ? Properties.Resources.ok : Properties.Resources.cross_icon;

                // Edge parity test
                var edge = Solvability.EdgeParityTest(rubik);
                lblEdgeTest.Text = edge ? "Passed" : "Failed";
                pbEdgeTest.Image = edge ? Properties.Resources.ok : Properties.Resources.cross_icon;

                lblHeader.Text = permutation && corner && edge && colors ? "This cube is solvable." : "This cube is unsolvable.";
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}