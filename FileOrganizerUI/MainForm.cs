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
using FileOrganizerUI.CodeBehind;
using FileDBManager;
using FileOrganizerUI.Subelements;


namespace FileOrganizerUI
{
    public partial class MainForm : Form
    {
        private OpenFileDialog FileDialog;
        private FileInfoForm FileInfoModal;
        private ILogger logger;
        private FileOrganizer core;
        private SearchParser parser;
        private ThumbnailProxy thumbnailProxy;
        private ImageList imageList;
        private List<GetFileMetadataType> searchResults;

        public MainForm(ILogger logger, FileOrganizer core)
        {
            InitializeComponent();
            this.logger = logger;
            this.core = core;
            thumbnailProxy = new ThumbnailProxy(logger);
            parser = new SearchParser(logger);

            FileDialog = new OpenFileDialog();
            FileDialog.Multiselect = true;
            
            FilePanel.AutoScroll = true;
            FileListView.MultiSelect = true;
            FileListView.View = View.Tile;
            FileListView.BackColor = Color.White;
            FileListView.MouseDoubleClick += new MouseEventHandler(FileListItem_DoubleClick);

            imageList = new ImageList();
            imageList.ImageSize = new Size(32, 32);

            FileInfoModal = new FileInfoForm(logger);
            FileInfoModal.FormBorderStyle = FormBorderStyle.SizableToolWindow;
        }

        private void OpenFilePicker_Click(object sender, EventArgs e)
        {
            if (FileDialog.ShowDialog() == DialogResult.OK) {
                List<string> filesToAdd = new List<string>();
                foreach (string filename in FileDialog.FileNames) {
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
                    UpdateMessage(errMsg, Color.FromArgb(200, 50, 50));
                    foreach (string msg in res.Messages) {
                        errMsg += msg + "\n";
                    }
                    MessageTooltip.SetToolTip(MessageText, errMsg);
                } else {
                    UpdateMessage($"{filesToAdd.Count} files added", Color.Black);
                    MessageTooltip.SetToolTip(MessageText, MessageText.Text);
                }
            }
        }

        private void Search_Click(object sender, EventArgs e)
        {
            string errMsg;
            parser.Reset();
            imageList.Images.Clear();
            bool parseResult = parser.Parse(SearchBox.Text.Trim(), out errMsg);

            if (!parseResult) {
                UpdateMessage(errMsg, Color.FromArgb(200, 50, 50));
            } else {
                var files = core.GetFileData(parser.Filter);
                FileListView.Clear();
                if (!files.HasError()) {

                    searchResults = files.Result;
                    var thumbnailMap = thumbnailProxy.GetThumbnails(files.Result.ConvertAll<string>(f => f.Fullname));
                    imageList.Images.Clear();
                    foreach (var pair in thumbnailMap) {
                        imageList.Images.Add(pair.Key, pair.Value);
                    }

                    FileListView.BeginUpdate();
                    FileListView.LargeImageList = imageList;
                    FileListView.SmallImageList = imageList;
                    foreach (var filedata in files.Result) {
                        FileListView.Items.Add(new ListViewItem(filedata.Filename, filedata.Fullname));
                    }
                    FileListView.EndUpdate();

                    UpdateMessage($"Found {files.Result.Count} file(s)", Color.Black);
                    MessageTooltip.RemoveAll();
                } else {
                    errMsg = "Failed to query files";
                    UpdateMessage(errMsg, Color.FromArgb(200, 50, 50));
                    errMsg = "";
                    foreach (string msg in files.Messages) {
                        errMsg += msg + "\n";
                    }
                    MessageTooltip.SetToolTip(MessageText, errMsg);
                }
            }

        }

        private void FileListItem_DoubleClick(object sender, MouseEventArgs e)
        {
            ListViewItem item = FileListView.GetItemAt(e.X, e.Y);
            if (item != null) {
                var selectedFile = searchResults.Find(it => it.Fullname == item.ImageKey);
                if (selectedFile != null) {
                    FileInfoModal.Text = selectedFile.Filename;
                    FileInfoModal.ShowDialog(this);
                } else {
                    logger.LogError($"File {item.ImageKey} was missing from cached search results");
                }
            }
        }

        private void UpdateMessage(string msg, Color color)
        {
            MessageText.Text = msg;
            MessageText.ForeColor = color;
        }

    }
}
