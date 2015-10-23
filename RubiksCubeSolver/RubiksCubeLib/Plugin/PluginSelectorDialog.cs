using System;
using System.Windows.Forms;

namespace RubiksCubeLib
{
    /// <summary>
    /// Represents a windwos form to select plugins
    /// </summary>
    /// <typeparam name="T">A pluginable type</typeparam>
    public class PluginSelectorDialog<T> : Form where T : IPluginable
    {
        private ListBox lbPlugins;
        private Button btnOK;
        private Button btnCancel;

        private readonly PluginCollection<T> plugins;

        /// <summary>
        /// Gets the currently selected plugin
        /// </summary>
        public T SelectedPlugin { get; private set; }

        public PluginSelectorDialog(PluginCollection<T> plugins)
        {
            this.InitializeComponent();
            this.plugins = plugins;
            foreach (var plugin in this.plugins.GetAll())
            {
                this.lbPlugins.Items.Add(plugin.Name);
            }
        }

        #region Designer

        private void InitializeComponent()
        {
            this.lbPlugins = new ListBox();
            this.btnOK = new Button();
            this.btnCancel = new Button();
            this.SuspendLayout();
            //
            // lbPlugins
            //
            this.lbPlugins.FormattingEnabled = true;
            this.lbPlugins.Location = new System.Drawing.Point(12, 12);
            this.lbPlugins.Name = "lbPlugins";
            this.lbPlugins.Size = new System.Drawing.Size(258, 108);
            this.lbPlugins.TabIndex = 0;
            this.lbPlugins.SelectedIndexChanged += this.lbPlugins_SelectedIndexChanged;
            //
            // btnOK
            //
            this.btnOK.DialogResult = DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(195, 125);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            //
            // btnCancel
            //
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(12, 125);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            //
            // PluginSelectorDialog
            //
            this.AcceptButton = this.btnOK;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(282, 160);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lbPlugins);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PluginSelectorDialog";
            this.Text = "Select Plugin";
            this.Load += this.PluginSelectorDialog_Load;
            this.ResumeLayout(false);
        }

        #endregion Designer

        private void lbPlugins_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SelectedPlugin = this.plugins[this.lbPlugins.SelectedIndex];
            this.plugins.StandardPlugin = this.SelectedPlugin;
        }

        private void PluginSelectorDialog_Load(object sender, EventArgs e)
        {
            if (this.plugins.Count == 0)
            {
                MessageBox.Show("No plugins found!");
                this.Close();
            }
            else
            {
                this.lbPlugins.SelectedIndex = 0;
            }
        }
    }
}