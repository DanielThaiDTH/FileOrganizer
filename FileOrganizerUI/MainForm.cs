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


namespace FileOrganizerUI
{
    public partial class MainForm : Form
    {
        private OpenFileDialog fileDialog;
        private ILogger logger;

        public MainForm(ILogger logger)
        {
            InitializeComponent();
            this.logger = logger;
            fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
        }

        private void OpenFilePicker_Click(object sender, EventArgs e)
        {
            if (fileDialog.ShowDialog() == DialogResult.OK) {
                List<string> filesToAdd = new List<string>();
                foreach (string filename in fileDialog.FileNames) {
                    filesToAdd.Add(filename);
                    logger.LogDebug("Opened file: " + filename);
                }
            }
        }

        
    }
}
