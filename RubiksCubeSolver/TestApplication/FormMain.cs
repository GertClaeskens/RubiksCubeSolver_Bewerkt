using RubiksCubeLib;
using System;
using System.ComponentModel;
using System.Windows.Forms;
using TwoPhaseAlgorithmSolver;

namespace TestApplication
{
    using System.Linq;

    public partial class FormMain : Form
    {
        //private readonly PluginCollection<CubeSolver> solverPlugins = new PluginCollection<CubeSolver>();
        private readonly BindingList<IMove> rotations = new BindingList<IMove>();

        public FormMain()
        {
            this.InitializeComponent();

            //foreach (string path in Properties.Settings.Default.PluginPaths)
            //{
            //  solverPlugins.AddDll(path);
            //}

            this.cubeModel.StartRender();

            foreach (var flag in Enum.GetValues(typeof(CubeFlag)).Cast<CubeFlag>().Where(flag => flag != CubeFlag.None && flag != CubeFlag.XFlags && flag != CubeFlag.YFlags && flag != CubeFlag.ZFlags))
            {
                this.comboBoxLayers.Items.Add(flag.ToString());
            }

            this.listBoxQueue.DataSource = this.rotations;
            this.listBoxQueue.DisplayMember = "Name";
        }

        //private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    var fbd = new FolderBrowserDialog();
        //    if (fbd.ShowDialog() == DialogResult.OK)
        //    {
        //        this.solverPlugins.AddFolder(fbd.SelectedPath);
        //    }
        //}

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.cubeModel.ResetCube();
        }

        private void scrambleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.cubeModel.Rubik.Scramble(50);
        }

        private void solveToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //2 onderstaande variabelen implementen IDisposable niet -> nakijken
            var rbk = this.cubeModel.Rubik;
            var tpa = new TwoPhaseAlgorithm();
            using(var dlg = new DialogSolutionFinder(tpa, rbk, this))
            {
                //var dlg = new DialogSolutionFinder(new TwoPhaseAlgorithm(), this.cubeModel.Rubik, this);
                //DialogSolutionFinder dlg = new DialogSolutionFinder(new BeginnerSolver(), this.cubeModel.Rubik, this);
                //DialogSolutionFinder dlg = new DialogSolutionFinder(new FridrichSolver(), this.cubeModel.Rubik, this);
                if (dlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                this.rotations.Clear();
                dlg.Algorithm.Moves.ForEach(m => this.rotations.Add(m));
            }
            //dlg.Dispose();
        }

        private void parityTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var parityCheck = new DialogParityCheckResult(this.cubeModel.Rubik, this))
            {
                parityCheck.ShowDialog();
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "XML-Files|*.xml";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    this.cubeModel.SavePattern(sfd.FileName);
                }
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "XML-Files|*.xml";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    this.cubeModel.LoadPattern(ofd.FileName);
                }
            }
        }

        private void btnRotate_Click(object sender, EventArgs e)
        {
            var layer = (CubeFlag)Enum.Parse(typeof(CubeFlag), this.comboBoxLayers.SelectedItem.ToString());
            this.cubeModel.RotateLayerAnimated(layer, this.checkBoxDirection.Checked);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            this.rotations.Clear();
        }

        private void btnAddToQueue_Click(object sender, EventArgs e)
        {
            var layer = (CubeFlag)Enum.Parse(typeof(CubeFlag), this.comboBoxLayers.SelectedItem.ToString());
            this.rotations.Add(new LayerMove(layer, this.checkBoxDirection.Checked));
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            foreach (var move in this.rotations) this.cubeModel.RotateLayerAnimated(move);
        }

        private void manageSolversToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var frmAbout = new FormAbout())
            {
                frmAbout.ShowDialog();
            }
        }

        private void FormMain_Activated(object sender, EventArgs e)
        {
            //cubeModel.StartRender();
        }

        private void FormMain_Deactivate(object sender, EventArgs e)
        {
            //cubeModel.StopRender();
        }
    }
}