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
using FileDBManager;
using System.IO;
using FileOrganizerCore;

namespace FileOrganizerUI.Subelements
{
    public partial class FileInfoForm : Form
    {
        ILogger logger;
        GetFileMetadataType fileInfo;
        private Dictionary<string, FlowLayoutPanel> detailLines;
        Button HashRefreshButton;
        Button UpdateButton;
        Button CloseButton;

        public FileInfoForm(ILogger logger)
        {
            InitializeComponent();
            this.logger = logger;
            detailLines = new Dictionary<string, FlowLayoutPanel>();

            detailLines.Add("Filename", new FlowLayoutPanel());
            detailLines.Add("Path", new FlowLayoutPanel());
            detailLines.Add("Type", new FlowLayoutPanel());
            detailLines.Add("Altname", new FlowLayoutPanel());
            detailLines.Add("Hash", new FlowLayoutPanel());
            detailLines.Add("Size", new FlowLayoutPanel());
            LayoutSetup();
        }

        public void SetFileInfo(GetFileMetadataType file)
        {
            fileInfo = file;
            this.Text = file.Fullname;
            detailLines["Filename"].Controls[1].Text = file.Filename;
            detailLines["Path"].Controls[1].Text = file.Path;
            detailLines["Type"].Controls[1].Text = file.FileType;
            detailLines["Altname"].Controls[1].Text = file.Altname;
            detailLines["Hash"].Controls[1].Text = file.Hash;
            detailLines["Hash"].Controls[1].KeyDown += PreventInput;
            detailLines["Size"].Controls[1].Text = file.Size.ToString();
        }

        private void PreventInput(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Left && e.KeyCode != Keys.Right
                && e.KeyCode != Keys.Home && e.KeyCode != Keys.End) {
                e.SuppressKeyPress = true;
            }
        }

        private void HashRefresh_Click(object sender, EventArgs e)
        {
            RefreshHash(Path.Combine(detailLines["Path"].Controls[1].Text, detailLines["Filename"].Controls[1].Text));
        }

        #region Functionality
        public void LayoutSetup()
        {
            foreach (var detailPair in detailLines) {
                detailPair.Value.Parent = MainVPanel;
                detailPair.Value.FlowDirection = FlowDirection.LeftToRight;
                detailPair.Value.AutoSize = true;

                Label label = new Label();
                label.Text = detailPair.Key;
                label.Margin = Padding.Empty;
                TextBox box = new TextBox();
                box.Text = "";
                box.Margin = Padding.Empty;
                box.Width = 500;
                detailPair.Value.Controls.Add(label);
                detailPair.Value.Controls.Add(box);
                if (detailPair.Key == "Hash") {
                    HashRefreshButton = new Button();
                    HashRefreshButton.Text = "Refresh";
                    HashRefreshButton.Margin = Padding.Empty;
                    HashRefreshButton.Click += HashRefresh_Click;
                    detailPair.Value.Controls.Add(HashRefreshButton);
                }
                
                MainVPanel.Controls.Add(detailPair.Value);
            }

            FlowLayoutPanel lastPanel = new FlowLayoutPanel();
            lastPanel.Parent = MainVPanel;
            lastPanel.AutoSize = true;
            UpdateButton = new Button();
            CloseButton = new Button();
            UpdateButton.Text = "Update";
            CloseButton.Text = "Close";
            lastPanel.Controls.Add(UpdateButton);
            lastPanel.Controls.Add(CloseButton);
            MainVPanel.Controls.Add(lastPanel);
        }

        private void RefreshHash(string path)
        {
            var result = new ActionResult<bool>();
            logger.LogInformation("Re-hashing " + path);
            string hash = FileOrganizerCore.Utilities.Hasher.HashFile(path, in result);
            logger.LogDebug("New hash " + hash);
            detailLines["Hash"].Controls[1].Text = hash;
        }
        #endregion
    }
}
