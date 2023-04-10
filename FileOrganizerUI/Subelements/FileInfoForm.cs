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

namespace FileOrganizerUI.Subelements
{
    public partial class FileInfoForm : Form
    {
        ILogger logger;
        GetFileMetadataType fileInfo;
        private Dictionary<string, FlowLayoutPanel> detailLines;

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
            detailLines["Size"].Controls[1].Text = file.Size.ToString();
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
                MainVPanel.Controls.Add(detailPair.Value);
            }
        }
        #endregion
    }
}
