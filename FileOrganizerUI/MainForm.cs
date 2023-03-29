using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using FileOrganizerCore;


namespace FileOrganizerUI
{
    public partial class MainForm : Form
    {
        private OpenFileDialog fileDialog;
        private ILogger logger;
        private FileOrganizer core;

        public MainForm(ILogger logger, FileOrganizer core)
        {
            InitializeComponent();
            this.logger = logger;
            this.core = core;
            fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            FilePanel.AutoScroll = true;
            FileListView.MultiSelect = true;
            FileListView.View = View.Tile;
        }

        private void OpenFilePicker_Click(object sender, EventArgs e)
        {
            if (fileDialog.ShowDialog() == DialogResult.OK) {
                List<string> filesToAdd = new List<string>();
                foreach (string filename in fileDialog.FileNames) {
                    filesToAdd.Add(filename);
                    logger.LogDebug("Opened file: " + filename);
                }
                var res = core.AddFiles(filesToAdd);
                if (res.HasError()) {
                    string errMsg = "";
                    int good = 0;
                    int bad = 0;
                    foreach (var status in res.Result) {
                        if (status) {
                            good++;
                        } else {
                            bad++;
                        }
                    }
                    errMsg += $"Adding files resulted in {good} successes and {bad} failures\n\n";
                    MessageText.Text = errMsg;
                    MessageText.ForeColor = Color.FromArgb(200, 50, 50);
                    foreach (string msg in res.Messages) {
                        errMsg += msg + "\n";
                    }
                    MessageTooltip.SetToolTip(MessageText, errMsg);
                } else {
                    MessageText.ForeColor = Color.Black;
                    MessageText.Text = $"{filesToAdd.Count} files added";
                    MessageTooltip.SetToolTip(MessageText, MessageText.Text);
                }
            }
        }

        private void Search_Click(object sender, EventArgs e)
        {
            FileListView.Items.Add(new ListViewItem("test_long_long"));
        }
    }
}
