using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using Microsoft.Extensions.Logging;
using FileDBManager;
using FileDBManager.Entities;
using FileOrganizerCore;

namespace FileOrganizerUI.Windows
{
    public partial class FileInfoForm : Form
    {
        ILogger logger;
        GetFileMetadataType fileInfo;
        FileOrganizer core;
        Bitmap image;
        
        private Dictionary<string, FlowLayoutPanel> detailLines;
        private Dictionary<string, FlowLayoutPanel> specialDetailLines;
        
        Button HashRefreshButton;
        Button UpdateButton;
        Button CloseButton;
        Button DeleteButton;
        Label StatusMessage;

        IObservable<object> editObservable;
        IDisposable editDispose;
        DeleteConfirmModal DeleteModal;

        public bool IsUpdated { get; private set; } = false;
        public bool IsDeleted { get; private set; } = false;

        public FileInfoForm(ILogger logger, FileOrganizer core)
        {
            InitializeComponent();
            this.logger = logger;
            this.core = core;
            detailLines = new Dictionary<string, FlowLayoutPanel>();
            specialDetailLines = new Dictionary<string, FlowLayoutPanel>();

            detailLines.Add("Filename", new FlowLayoutPanel());
            detailLines.Add("Path", new FlowLayoutPanel());
            detailLines.Add("FileType", new FlowLayoutPanel());
            detailLines.Add("Altname", new FlowLayoutPanel());
            detailLines.Add("Hash", new FlowLayoutPanel());
            detailLines.Add("Size", new FlowLayoutPanel());
            specialDetailLines.Add("Created", new FlowLayoutPanel());
            DeleteModal = new DeleteConfirmModal();
            DeleteModal.FormBorderStyle = FormBorderStyle.FixedDialog;
            LayoutSetup();

            //Right panel setup
            OpenFileButton.Click += OpenFile_Click;
        }

        public void SetFileInfo(GetFileMetadataType file)
        {
            if (file is null) {
                logger.LogError("File info form open failure: File provided is null");
                throw new ArgumentNullException("File info form open failure: File provided is null");
            }

            fileInfo = file;
            this.Text = file.Fullname;
            detailLines["Filename"].Controls[1].Text = file.Filename;
            detailLines["Path"].Controls[1].Text = file.Path;
            detailLines["FileType"].Controls[1].Text = file.FileType;
            detailLines["Altname"].Controls[1].Text = file.Altname;
            detailLines["Hash"].Controls[1].Text = file.Hash;
            detailLines["Hash"].Controls[1].KeyDown += PreventInput;
            detailLines["Size"].Controls[1].Text = file.Size.ToString();
            specialDetailLines["Created"].Controls[1].Text = file.Created.ToString("yyyy-MM-dd");
            UpdateMessage("", Color.Black);

            //Refer to FileOrganizerCore to see that file extensions are images
            if (file.FileType == "image") {
                try {
                    image = new Bitmap(fileInfo.Fullname);
                    PictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                    PictureBox.Size = new Size(960, 540);
                    PictureBox.Image = image;
                } catch {
                    logger.LogWarning($"Image file {fileInfo.Fullname} not found");
                    UpdateMessage("File not found, cannot display image", MainForm.ErrorMsgColor);
                }
            }

            editDispose = editObservable.Subscribe((args) => {
                bool changed = detailLines.Any(dl => 
                    dl.Value.Controls[1].Text != typeof(GetFileMetadataType).GetProperty(dl.Key).GetValue(fileInfo, null).ToString())
                    || specialDetailLines["Created"].Controls[1].Text != file.Created.ToString("yyyy-MM-dd");
                UpdateButton.Enabled = changed;
            });
            UpdateButton.DialogResult = DialogResult.OK;
        }

        public void ClearFileInfo()
        {
            fileInfo = new GetFileMetadataType();
            IsUpdated = false;
            IsDeleted = false;
            Text = "";
            editDispose.Dispose();
            detailLines["Filename"].Controls[1].Text = "";
            detailLines["Path"].Controls[1].Text = "";
            detailLines["FileType"].Controls[1].Text = "";
            detailLines["Altname"].Controls[1].Text = "";
            detailLines["Hash"].Controls[1].Text = "";
            detailLines["Hash"].Controls[1].KeyDown -= PreventInput;
            detailLines["Size"].Controls[1].Text = "";
            detailLines["Size"].Controls[1].KeyDown -= PreventInput;
            specialDetailLines["Created"].Controls[1].Text = "";
            specialDetailLines["Created"].Controls[1].KeyDown -= PreventInput;
            UpdateButton.DialogResult = DialogResult.Abort;
            if (image != null) {
                image.Dispose();
                image = null;
                PictureBox.Image = null;
            }
        }

        private void PreventInput(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Left && e.KeyCode != Keys.Right
                && e.KeyCode != Keys.Home && e.KeyCode != Keys.End) {
                e.SuppressKeyPress = true;
            }
        }

        private void OpenFile_Click(object sender, EventArgs e)
        {
            if (fileInfo is null) return;

            using (Process ps = new Process()) {
                ps.StartInfo.FileName = fileInfo.Fullname;
                ps.StartInfo.UseShellExecute = true;
                try {
                    ps.Start();
                } catch (Exception ex) {
                    logger.LogWarning($"File {fileInfo.Fullname} unable to be opened: {ex.Message}");
                    logger.LogDebug(ex.StackTrace);
                    UpdateMessage($"File {fileInfo.Fullname} unable to be opened: {ex.Message}", MainForm.ErrorMsgColor);
                }
            }
        }

        /// <summary>
        /// Prevents illegal Windows path characters from being entered.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SanitizePathInput(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Shift) {
                switch (e.KeyCode) {
                    case Keys.Oemcomma:
                    case Keys.OemPeriod:
                    case Keys.OemSemicolon:
                    case Keys.OemQuotes:
                    case Keys.OemPipe:
                    case Keys.OemQuestion:
                        e.SuppressKeyPress = true;
                        break;
                    default:
                        break;
                }
            }
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            if (File.Exists(GetPath())) {
                RefreshHash(GetPath());
                RefreshSize(GetPath());
                RefreshCreated(GetPath());
            } else {
                UpdateMessage($"File {GetPath()} not found", MainForm.ErrorMsgColor);
            }
        }

        private void Update_Click(object sender, EventArgs e)
        {
            Func<string, string, bool> isChanged = (string label, string fileInfo) => detailLines[label].Controls[1].Text != fileInfo;
            FileMetadata newData = new FileMetadata();
            if (isChanged("Filename", fileInfo.Filename)) newData.SetFilename(detailLines["Filename"].Controls[1].Text);
            if (isChanged("Path", fileInfo.Path)) newData.SetPath(detailLines["Path"].Controls[1].Text);
            if (isChanged("FileType", fileInfo.FileType)) newData.SetFileType(detailLines["FileType"].Controls[1].Text);
            if (isChanged("Altname", fileInfo.Altname)) newData.SetAltname(detailLines["Altname"].Controls[1].Text);
            if (isChanged("Hash", fileInfo.Hash)) newData.SetHash(detailLines["Hash"].Controls[1].Text);
            if (isChanged("Size", fileInfo.Size.ToString())) newData.SetSize(long.Parse(detailLines["Size"].Controls[1].Text));
            if (specialDetailLines["Created"].Controls[1].Text != fileInfo.Created.ToString("yyyy-MM-dd")) {
                logger.LogDebug("Updating date to " + specialDetailLines["Created"].Controls[1].Text);
                newData.SetCreated(File.GetCreationTime(GetPath()));
            }

            var result = core.UpdateFileData(newData, fileInfo.ID);
            if (result.Result) {
                UpdateMessage("File updated", Color.Black);
                IsUpdated = true;
            } else {
                UpdateMessage(result.GetErrorMessage(0), Color.FromArgb(200, 50, 50));
            }
        }

        private void Close_Click(object sender, EventArgs e)
        {
            var result = CloseButton.DialogResult;
            Close();
            DialogResult = result;
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            DeleteModal.Text = "Delete " + fileInfo.Fullname;
            var result = DeleteModal.ShowDialog(this);
            if (result == DialogResult.Yes) {

                var deleteResult = core.DeleteFile(fileInfo.ID);
                if (deleteResult.Result) {
                    logger.LogInformation("Closing info screen for deleted file " + fileInfo.Fullname);
                    IsDeleted = true;
                    DialogResult = DialogResult.No;
                    Close();
                    DialogResult = DialogResult.No;
                } else {
                    UpdateMessage(deleteResult.GetErrorMessage(0), Color.FromArgb(200, 50, 50));
                }
            }
        }

        #region Functionality
        private string GetPath()
        {
            return Path.Combine(detailLines["Path"].Controls[1].Text, detailLines["Filename"].Controls[1].Text);
        }

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
                if (detailPair.Key == "Hash" || detailPair.Key == "Size") {
                    HashRefreshButton = new Button();
                    HashRefreshButton.Text = "Refresh";
                    HashRefreshButton.Margin = Padding.Empty;
                    HashRefreshButton.Click += Refresh_Click;
                    detailPair.Value.Controls.Add(HashRefreshButton);
                } else if (detailPair.Key == "Path" || detailPair.Key == "Filename") {
                    detailPair.Value.Controls[1].KeyDown += SanitizePathInput;
                }
                
                MainVPanel.Controls.Add(detailPair.Value);
            }

            //Created row
            specialDetailLines["Created"].AutoSize = true;
            Label specialLabel = new Label();
            specialLabel.Text = "Created";
            specialLabel.Margin = Padding.Empty;
            TextBox specialBox = new TextBox();
            specialBox.Text = "";
            specialBox.Margin = Padding.Empty;
            specialBox.Width = 500;
            HashRefreshButton = new Button();
            HashRefreshButton.Text = "Refresh";
            HashRefreshButton.Margin = Padding.Empty;
            HashRefreshButton.Click += Refresh_Click;
            specialDetailLines["Created"].Controls.Add(specialLabel);
            specialDetailLines["Created"].Controls.Add(specialBox);
            specialDetailLines["Created"].Controls.Add(HashRefreshButton);
            MainVPanel.Controls.Add(specialDetailLines["Created"]);

            //Buttons row
            FlowLayoutPanel lastPanel = new FlowLayoutPanel();
            lastPanel.Parent = MainVPanel;
            lastPanel.AutoSize = true;
            UpdateButton = new Button();
            CloseButton = new Button();
            DeleteButton = new Button();
            UpdateButton.Text = "Update";
            UpdateButton.Enabled = false;
            UpdateButton.Click += Update_Click;
            CloseButton.Text = "Close";
            CloseButton.DialogResult = DialogResult.OK;
            CloseButton.Click += Close_Click;
            DeleteButton.Text = "Delete";
            DeleteButton.Click += Delete_Click;
            lastPanel.Controls.Add(UpdateButton);
            lastPanel.Controls.Add(CloseButton);
            lastPanel.Controls.Add(DeleteButton);
            MainVPanel.Controls.Add(lastPanel);
            StatusMessage = new Label();
            MainVPanel.Controls.Add(StatusMessage);

            editObservable = Observable.FromEventPattern(handler => {
                detailLines["Filename"].Controls[1].TextChanged += handler;
                detailLines["Path"].Controls[1].TextChanged += handler;
                detailLines["FileType"].Controls[1].TextChanged += handler;
                detailLines["Altname"].Controls[1].TextChanged += handler;
                detailLines["Hash"].Controls[1].TextChanged += handler;
                detailLines["Size"].Controls[1].TextChanged += handler;
                specialDetailLines["Created"].Controls[1].TextChanged += handler;
            },
            handler => {
                detailLines["Filename"].Controls[1].TextChanged -= handler;
                detailLines["Path"].Controls[1].TextChanged -= handler;
                detailLines["FileType"].Controls[1].TextChanged -= handler;
                detailLines["Altname"].Controls[1].TextChanged -= handler;
                detailLines["Hash"].Controls[1].TextChanged -= handler;
                detailLines["Size"].Controls[1].TextChanged -= handler;
                specialDetailLines["Created"].Controls[1].TextChanged += handler;
            });
        }

        private void RefreshHash(string path)
        {
            var result = new ActionResult<bool>();
            logger.LogInformation("Re-hashing " + path);
            string hash = FileOrganizerCore.Utilities.Hasher.HashFile(path, in result);
            logger.LogDebug("New hash " + hash);
            if (string.IsNullOrEmpty(hash)) {
                logger.LogWarning("Unable to re-hash file");
                UpdateMessage("Unable to re-hash file", Color.FromArgb(200, 50, 50));
            } else {
                detailLines["Hash"].Controls[1].Text = hash;
            }
        }

        private void RefreshSize(string path)
        {
            var result = new ActionResult<bool>();
            long size = core.GetFileSize(path, in result);
            if (result.Result) {
                detailLines["Size"].Controls[1].Text = size.ToString();
            } else {
                UpdateMessage(result.GetErrorMessage(0), MainForm.ErrorMsgColor);
            }
        }

        private void RefreshCreated(string path)
        {
            try {
                var dt = File.GetCreationTime(path);
                specialDetailLines["Created"].Controls[1].Text = dt.ToString("yyyy-MM-dd");
            } catch (AccessViolationException ex) {
                UpdateMessage(ex.ToString() + ": No access to file", MainForm.ErrorMsgColor);
            } catch (IOException ex) {
                UpdateMessage(ex.ToString() + ": File is missing", MainForm.ErrorMsgColor);
            }
        }

        private void UpdateMessage(string msg, Color color)
        {
            StatusMessage.Text = msg;
            StatusMessage.ForeColor = color;
        }
        #endregion
    }
}
