using FileOrganizerCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileOrganizerUI.Windows
{
    public partial class SettingsForm : Form
    {
        ILogger logger;
        FileOrganizer core;
        FolderBrowserDialog FolderBrowser;
        private Dictionary<string, FlowLayoutPanel> settingsRows;
        Label MessageText;
        public SettingsForm(ILogger logger, FileOrganizer core)
        {
            InitializeComponent();
            this.core = core;
            this.logger = logger;
            FolderBrowser = new FolderBrowserDialog();
            MessageText = new Label();
            settingsRows = new Dictionary<string, FlowLayoutPanel>();
            settingsRows.Add("SymLink Folder", new FlowLayoutPanel());
            settingsRows.Add("Message", new FlowLayoutPanel());
            LayoutSetup();
            Shown += new EventHandler((object sender, EventArgs e) => {
                    settingsRows["SymLink Folder"].Controls[1].Text = core.GetSymLinksRoot();
                });
        }

        private void FolderButton_Click(object sender, EventArgs e)
        {
            logger.LogInformation("Refreshing SymLink folder in settings dialog");
            ChangeSymLinkFolder();
        }

        #region Functionality
        private void LayoutSetup()
        {
            Button FolderButton = new Button();
            FolderButton.Text = "SymLink Folder";
            FolderButton.Click += FolderButton_Click;
            FolderButton.Padding = Padding.Empty;
            Label FolderLabel = new Label();
            FolderLabel.Text = core.GetSymLinksRoot();
            FolderLabel.Margin = new Padding(0, 10, 0, 0);
            FolderLabel.AutoSize = true;

            settingsRows["SymLink Folder"].Controls.Add(FolderButton);
            settingsRows["SymLink Folder"].Controls.Add(FolderLabel);
            settingsRows["Message"].Controls.Add(MessageText);
            settingsRows["SymLink Folder"].AutoSize = true;
            settingsRows["Message"].AutoSize = true;
            SettingsLayoutPanel.Controls.Add(settingsRows["SymLink Folder"]);
            SettingsLayoutPanel.Controls.Add(settingsRows["Message"]);
        }

        private void ChangeSymLinkFolder()
        {
            if (FolderBrowser.ShowDialog(this) == DialogResult.OK) {
                var folder = FolderBrowser.SelectedPath;
                var result = core.SetSymLinkFolder(folder);
                if (result.Result) {
                    settingsRows["SymLink Folder"].Controls[1].Text = folder;
                } else {
                    UpdateMessage(result.GetErrorMessage(0), MainForm.ErrorMsgColor);
                }
            }
        }

        private void UpdateMessage(string msg, Color color)
        {
            MessageText.Text = msg;
            MessageText.ForeColor = color;
        }

        #endregion
    }
}
