using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using Microsoft.Extensions.Logging;
using FileOrganizerCore;
using FileOrganizerUI.CodeBehind;
using FileDBManager;
using FileDBManager.Entities;
using FileOrganizerUI.Subelements;


namespace FileOrganizerUI
{
    public partial class MainForm : Form
    {
        private OpenFileDialog FileDialog;
        private SettingsForm SettingsDialog;
        private FileInfoForm FileInfoModal;
        private TagInfoForm TagInfoModal;
        private ILogger logger;
        private FileOrganizer core;
        private SearchParser parser;
        private ThumbnailProxy thumbnailProxy;
        private ImageList imageList;
        public static Color ErrorMsgColor = Color.FromArgb(200, 50, 50);
        private static GetTagCategoryType DefaultCategory = new GetTagCategoryType { ID = -1, Name = "-- None --" };

        public MainForm(ILogger logger, FileOrganizer core)
        {
            InitializeComponent();
            this.logger = logger;
            this.core = core;
            thumbnailProxy = new ThumbnailProxy(logger);
            parser = new SearchParser(logger);

            FileDialog = new OpenFileDialog();
            FileDialog.Multiselect = true;

            SettingsDialog = new SettingsForm(logger, core);

            SearchBox.KeyDown += new KeyEventHandler(Search_Enter);
            
            FilePanel.AutoScroll = true;
            FileListView.MultiSelect = true;
            FileListView.View = View.Tile;
            FileListView.BackColor = Color.White;
            FileListView.MouseDoubleClick += new MouseEventHandler(FileListItem_DoubleClick);
            FileListView.KeyDown += FileListView_SelectAll;

            imageList = new ImageList();
            imageList.ImageSize = new Size(32, 32);

            FileInfoModal = new FileInfoForm(logger, core);
            FileInfoModal.FormBorderStyle = FormBorderStyle.SizableToolWindow;

            TagSearchBox.KeyDown += TagSearch_Enter;

            TagListView.MultiSelect = true;
            TagListView.View = View.List;
            TagListView.MouseDoubleClick += TagListItem_DoubleClick;

            AddNewTagButton.Click += AddNewTagButton_Click;

            AssignTagButton.Click += AssignTag_Click;

            TagInfoModal = new TagInfoForm(logger, core);
            TagInfoModal.FormBorderStyle = FormBorderStyle.SizableToolWindow;

            RefreshTagCategoryComboBox();

            SearchBox.Focus();
        }

        #region Handlers
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
                    UpdateMessage(errMsg, ErrorMsgColor);
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
            SearchFiles();
        }

        private void Search_Enter(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) {
                SearchFiles();
            }
        }

        private void TagSearch_Enter(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) {
                SearchTags((sender as TextBox).Text);
            }
        }

        private void FileListItem_DoubleClick(object sender, MouseEventArgs e)
        {
            ListViewItem item = FileListView.GetItemAt(e.X, e.Y);
            if (item != null) {
                var selectedFile = core.ActiveFiles.Find(it => it.Fullname == item.ImageKey);
                if (selectedFile != null) {
                    FileInfoModal.SetFileInfo(selectedFile);
                    var dialogResult = FileInfoModal.ShowDialog(this);
                    
                    if (dialogResult == DialogResult.OK || dialogResult == DialogResult.None || dialogResult == DialogResult.Cancel) {
                        if (FileInfoModal.IsUpdated) {
                            var updated = core.GetFileData(new FileSearchFilter().SetIDFilter(selectedFile.ID));
                            if (updated.Result.Count == 1) {
                                core.ActiveFiles.Remove(selectedFile);
                                core.ActiveFiles.Add(updated.Result[0]);
                                FileListView.Items.Remove(item);

                                FileListView.BeginUpdate();
                                var thumbnail = thumbnailProxy.GetThumbnail(updated.Result[0].Fullname);
                                FileListView.LargeImageList.Images.RemoveByKey(selectedFile.Fullname);
                                FileListView.LargeImageList.Images.Add(updated.Result[0].Fullname, thumbnail);
                                FileListView.Items.Add(new ListViewItem(updated.Result[0].Filename, updated.Result[0].Fullname));
                                FileListView.EndUpdate();
                            } else {
                                UpdateMessage("Updated file missing", ErrorMsgColor);
                            }
                        }
                        FileInfoModal.ClearFileInfo(); 
                    } else if (dialogResult == DialogResult.No && FileInfoModal.IsDeleted) {
                        FileInfoModal.ClearFileInfo();
                        FileListView.BeginUpdate();
                        core.ActiveFiles.Remove(selectedFile);
                        FileListView.Items.Remove(item);
                        core.ActiveFiles.RemoveAll(f => f.ID == selectedFile.ID);
                        FileListView.LargeImageList.Images.RemoveByKey(selectedFile.Fullname);
                        FileListView.EndUpdate();
                    }

                } else {
                    logger.LogError($"File {item.ImageKey} was missing from cached search results");
                }
            }
        }

        private void FileListView_SelectAll(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Control) {
                foreach (ListViewItem item in FileListView.Items) {
                    item.Selected = true;
                }
                e.SuppressKeyPress = true;
            }
        }

        private void CreateSymLinksButton_Click(object sender, EventArgs e)
        {
            var status = core.CreateSymLinksFromActiveFiles();
            if (!status.Result) {
                UpdateMessage($"Create symlinks failure for folder {core.GetSymLinksRoot()}: {status.Count} errors ", ErrorMsgColor);
                UpdateMessageToolTip(status.Messages);
            } else {
                UpdateMessage("Created symlinks at " + core.GetSymLinksRoot(), Color.Black);
            }
        }

        private void AppSettingsButton_Click(object sender, EventArgs e)
        {
            SettingsDialog.ShowDialog(this);
            SaveSymLinkFolderRoot(core.GetSymLinksRoot());
        }

        private void AddNewTagButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NewTagNameBox.Text)) {
                string category = "";
                if ((NewTagCategoryComboBox.SelectedItem as GetTagCategoryType).ID >= 0) {
                    category = (NewTagCategoryComboBox.SelectedItem as GetTagCategoryType).Name;
                }

                var addRes = core.AddTag(NewTagNameBox.Text.Trim(), category);
                if (addRes.Result) {
                    UpdateMessage($"Tag {NewTagNameBox.Text} added.", Color.Black);
                    NewTagCategoryComboBox.SelectedItem = DefaultCategory;
                    NewTagNameBox.Clear();
                } else {
                    UpdateMessage(addRes.GetErrorMessage(0), ErrorMsgColor);
                }
            }
        }

        private void AssignTag_Click(object sender, EventArgs e)
        {
            var selectedTags = TagListView.SelectedItems;
            var selectedFiles = FileListView.SelectedItems;

            if (selectedFiles.Count > 0 && selectedTags.Count > 0) {
                ActionResult<bool> result = new ActionResult<bool>();
                result.SetResult(true);
                foreach (ListViewItem fileItem in selectedFiles) {
                    foreach (ListViewItem tagItem in selectedTags) {
                        var file = core.ActiveFiles.Find(f => f.Fullname == fileItem.ImageKey);
                        var tag = core.AllTags.Find(t => t.Name == tagItem.Text);
                        var addRes = core.AddTagToFile(file.ID, tag.Name);
                        if (!addRes.Result) {
                            result.SetResult(false);
                            ActionResult.AppendErrors(result, addRes);
                        }
                    }
                }

                if (result.Result) {
                    UpdateMessage("Added tags to files", Color.Black);
                } else {
                    UpdateMessage("Adding tags to files resulted in errors", ErrorMsgColor);
                    UpdateMessageToolTip(result.Messages);
                }
            }
        }

        private void TagListItem_DoubleClick(object sender, MouseEventArgs e)
        {
            ListViewItem item = TagListView.GetItemAt(e.X, e.Y);
            if (item != null) {
                var selectedTag = core.AllTags.Find(t => t.Name == item.Text);
                if (selectedTag != null) {
                    TagInfoModal.SetTagInfo(selectedTag);
                    var dialogResult = TagInfoModal.ShowDialog(this);
                    if (dialogResult == DialogResult.OK || dialogResult == DialogResult.Cancel 
                        || dialogResult == DialogResult.None || dialogResult == DialogResult.Abort) {
                        if (TagInfoModal.IsUpdated) {
                            core.GetTags();
                            if (TagInfoModal.NameUpdated) {
                                var listItems = TagListView.Items;
                                foreach (ListViewItem tag in listItems) {
                                    if (tag.Text == selectedTag.Name) {
                                        tag.Text = core.AllTags.Find(t => t.ID == selectedTag.ID).Name;
                                    }
                                }
                            }

                            TagInfoModal.ClearTagInfo();
                        }
                    }
                } else {
                    logger.LogError($"Tag {item.Text} was missing from tags");
                }
            }
        }
        #endregion

        #region Functionality
        private void UpdateMessage(string msg, Color color)
        {
            MessageText.Text = msg;
            MessageText.ForeColor = color;
        }

        private void SearchFiles()
        {
            string errMsg;
            parser.Reset();
            bool parseResult = parser.Parse(SearchBox.Text.Trim(), out errMsg);

            if (!parseResult) {
                UpdateMessage(errMsg, ErrorMsgColor);
            } else {
                imageList.Images.Clear();
                var files = core.GetFileData(parser.Filter);
                FileListView.Clear();
                if (!files.HasError()) {
                    var thumbnailMap = thumbnailProxy.GetThumbnails(files.Result.ConvertAll<string>(f => f.Fullname));
                    imageList.Images.Clear();
                    foreach (var pair in thumbnailMap) {
                        imageList.Images.Add(pair.Key, pair.Value);
                    }

                    FileListView.BeginUpdate();
                    FileListView.LargeImageList = imageList;

                    foreach (var filedata in files.Result) {
                        FileListView.Items.Add(new ListViewItem(filedata.Filename, filedata.Fullname));
                    }
                    FileListView.EndUpdate();

                    UpdateMessage($"Found {files.Result.Count} file(s)", Color.Black);
                    MessageTooltip.RemoveAll();
                } else {
                    errMsg = "Failed to query files";
                    UpdateMessage(errMsg, ErrorMsgColor);
                    UpdateMessageToolTip(files.Messages);
                }
            }
        }

        private void SaveSymLinkFolderRoot(string filepath)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["DefaultFolder"].Value = filepath;
            config.Save(ConfigurationSaveMode.Modified);
            var result = core.SetSymLinkFolder(filepath);
            if (!result.Result) {
                UpdateMessage(result.GetErrorMessage(0), ErrorMsgColor);
            }
        }

        private void SearchTags(string query)
        {
            if (!string.IsNullOrWhiteSpace(query)) {
                var tags = core.AllTags.FindAll(t => t.Name.ToLowerInvariant().Contains(query.ToLowerInvariant()));
                core.ActiveTags = tags;
                TagListView.Clear();
                foreach (var tag in tags) {
                    TagListView.Items.Add(new ListViewItem(tag.Name));
                }
            }
        }

        private void RefreshTagCategoryComboBox()
        {
            NewTagCategoryComboBox.Items.Clear();
            foreach (var category in core.TagCategories) {
                NewTagCategoryComboBox.Items.Add(category);
            }

            NewTagCategoryComboBox.Items.Add(DefaultCategory);
            NewTagCategoryComboBox.SelectedItem = DefaultCategory;
        }

        private void UpdateMessageToolTip(List<string> msgs)
        {
            string errMsg = "";
            foreach (string msg in msgs) {
                errMsg += msg + "\n";
            }
            MessageTooltip.SetToolTip(MessageText, errMsg);
        }

        #endregion
    }
}
