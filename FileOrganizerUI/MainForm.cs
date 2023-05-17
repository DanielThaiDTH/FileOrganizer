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
using FileOrganizerUI.Windows;
using System.IO;

namespace FileOrganizerUI
{
    public partial class MainForm : Form
    {
        private ILogger logger;
        private FileOrganizer core;
        
        private OpenFileDialog FileDialog;
        private SettingsForm SettingsDialog;
        private FileInfoForm FileInfoModal;
        private TagInfoForm TagInfoModal;
        private AdvancedWindow AdvancedModal;
        private CollectionInfoForm CollectionInfoModal;
        private SearchParser parser;
        private ThumbnailProxy thumbnailProxy;
        private ImageList imageList;
        
        private int selectedFileID = -1;
        private Queue<string> searchHistory;
        private HashSet<string> searchSet;
        private readonly int HistoryLimit = 50;

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
            SearchBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            SearchBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
            SearchBox.AutoCompleteCustomSource = new AutoCompleteStringCollection();
            searchHistory = new Queue<string>();
            searchSet = new HashSet<string>();

            FilePanel.AutoScroll = true;
            FileListView.MultiSelect = true;
            FileListView.View = View.Tile;
            FileListView.BackColor = Color.White;
            FileListView.MouseDoubleClick += FileListItem_DoubleClick;
            FileListView.MouseClick += FileListItem_Click;
            FileListView.KeyDown += FileListView_SelectAll;
            FileListView.SelectedIndexChanged += FileListView_SelectionChanged;

            imageList = new ImageList();
            imageList.ImageSize = new Size(32, 32);

            FileInfoModal = new FileInfoForm(logger, core);
            FileInfoModal.FormBorderStyle = FormBorderStyle.SizableToolWindow;

            TagSearchBox.KeyDown += TagSearch_Enter;
            TagSearchBox.AutoCompleteMode = AutoCompleteMode.Suggest;
            TagSearchBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
            var tagSource = new AutoCompleteStringCollection();
            tagSource.AddRange(core.AllTags.ConvertAll(t => t.Name).ToArray());
            TagSearchBox.AutoCompleteCustomSource = tagSource;

            TagListView.MultiSelect = true;
            TagListView.ShowItemToolTips = true;
            TagListView.View = View.List;
            TagListView.MouseDoubleClick += TagListItem_DoubleClick;
            TagListView.SelectedIndexChanged += TagListView_SelectionChanged;

            AddNewTagButton.Click += AddNewTagButton_Click;

            AssignTagButton.Enabled = false;
            AssignTagButton.Click += AssignTag_Click;

            RemoveTagButton.Enabled = false;
            RemoveTagButton.Click += RemoveTag_Click;

            TagInfoModal = new TagInfoForm(logger, core);
            TagInfoModal.FormBorderStyle = FormBorderStyle.SizableToolWindow;

            AdvancedModal = new AdvancedWindow(logger, core);

            CollectionSearchBox.KeyDown += CollectionSearch_Enter;
            CollectionSearchBox.AutoCompleteMode = AutoCompleteMode.Suggest;
            CollectionSearchBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
            var collectionSource = new AutoCompleteStringCollection();
            collectionSource.AddRange(core.SearchFileCollection("").Result.ConvertAll(c => c.Name).ToArray());
            CollectionSearchBox.AutoCompleteCustomSource = collectionSource;

            CollectionListView.View = View.List;
            CollectionListView.MouseClick += CollectionListItem_Click;
            CollectionListView.MouseDoubleClick += CollectionListItem_DoubleClick;
            CollectionListView.SelectedIndexChanged += CollectionListView_SelectionChanged;

            CollectionResultAddButton.Click += CollectionResultButton_Click;

            CollectionAddFileButton.Click += CollectionFileAddButton_Click;
            CollectionPickerAddButton.Click += CollectionFilePickerButton_Click;

            CollectionSymlinkButton.Enabled = false;
            CollectionSymlinkButton.Click += CollectionSymlinkButton_Click;
            CollectionSymlinkTooltip.SetToolTip(CollectionSymlinkButton, "Generated symlinks will have the position as filenames");

            ShowCollectionFilesButton.Enabled = false;
            ShowCollectionFilesButton.Click += ShowCollectionFilesButton_Click;

            CollectionInfoModal = new CollectionInfoForm(logger, core);

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
            RemoveTagButton.Enabled = false;
            selectedFileID = -1;
        }

        private void Search_Enter(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) {
                SearchFiles();
                RemoveTagButton.Enabled = false;
                selectedFileID = -1;
                e.SuppressKeyPress = true;
            }
        }

        private void TagSearch_Enter(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) {
                SearchTags((sender as TextBox).Text);
                RemoveTagButton.Enabled = false;
                e.SuppressKeyPress = true;
            }
        }

        private void CollectionSearch_Enter(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) {
                SearchCollections((sender as TextBox).Text);
                CollectionAddFileButton.Enabled = false;
                e.SuppressKeyPress = true;
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
                        
                        foreach (var collection in core.ActiveCollections) {
                            if (collection.Files.Any(f => f.FileID == selectedFile.ID)) {
                                core.SaveActiveFilesBackup();
                                var refreshedCollection = core.GetFileCollection(collection.ID).Result;
                                collection.Files = refreshedCollection.Files;
                            }
                        }
                        UpdateMessage("File removed", Color.Black);
                    }

                } else {
                    logger.LogError($"File {item.ImageKey} was missing from cached search results");
                }
            }
        }

        private void FileListItem_Click(object sender, MouseEventArgs e)
        {
            RemoveTagButton.Enabled = false;
            selectedFileID = -1;
            if (e.Button == MouseButtons.Right) {
                ListViewItem item = FileListView.GetItemAt(e.X, e.Y);
                ViewFileTags(item);
                RemoveTagButton.Enabled = true;
            }
        }

        private void FileListView_SelectionChanged(object sender, EventArgs e)
        {
            AddFileToCollectionsEnableVerify();
            AssignTagsToFileVerify();
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

        private void RemoveTag_Click(object sender, EventArgs e)
        {
            var selectedTags = TagListView.SelectedItems;
            List<ListViewItem> removedTags = new List<ListViewItem>();

            if (selectedTags.Count > 0) {
                ActionResult<bool> result = new ActionResult<bool>();
                result.SetResult(true);
                foreach (ListViewItem tagItem in selectedTags) {
                    var tag = core.AllTags.Find(t => t.Name == tagItem.Text);
                    var remRes = core.DeleteTagFromFile(selectedFileID, tag.ID);
                    if (!remRes.Result) {
                        result.SetResult(false);
                        ActionResult.AppendErrors(result, remRes);
                    } else {
                        removedTags.Add(tagItem);
                    }
                }

                foreach (var remTag in removedTags) {
                    TagListView.Items.Remove(remTag);
                }

                if (result.Result) {
                    UpdateMessage("Removed tags from file", Color.Black);
                } else {
                    UpdateMessage("Removing tags from file resulted in errors", ErrorMsgColor);
                    UpdateMessageToolTip(result.Messages);
                }
            }
        }

        private void TagListView_SelectionChanged(object sender, EventArgs e)
        {
            AssignTagsToFileVerify();
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
                            UpdateMessage("Tag updated", Color.Black);
                            List<string> tooltipMsgs = new List<string>();
                            
                            if (TagInfoModal.NameUpdated) {
                                foreach (ListViewItem tag in TagListView.Items) {
                                    if (tag.Text == selectedTag.Name) {
                                        tag.Text = core.AllTags.Find(t => t.ID == selectedTag.ID).Name;
                                        TagSearchBox.AutoCompleteCustomSource.Add(tag.Text);
                                        TagSearchBox.AutoCompleteCustomSource.Remove(selectedTag.Name);
                                        selectedTag.Name = tag.Text;
                                        break;
                                    }
                                }

                                tooltipMsgs.Add("Updated tag name");
                            }

                            if (TagInfoModal.CategoryUpdated) {
                                item.ToolTipText = selectedTag.Category;
                                tooltipMsgs.Add("Updated category to " + selectedTag.Category);
                            }

                            UpdateMessageToolTip(tooltipMsgs);

                            TagInfoModal.ClearTagInfo();
                        }
                    } else if (dialogResult == DialogResult.No && TagInfoModal.IsDeleted) {
                        core.GetTags();
                        ListViewItem remItem = null;
                        foreach (ListViewItem tag in TagListView.Items) {
                            if (tag.Text == selectedTag.Name) {
                                remItem = tag;
                            }
                        }
                        if (remItem != null) {
                            TagListView.Items.Remove(remItem);
                            TagSearchBox.AutoCompleteCustomSource.Remove(selectedTag.Name);
                        }

                        TagInfoModal.ClearTagInfo();
                    }
                } else {
                    logger.LogError($"Tag {item.Text} was missing from tags");
                }
            }
        }

        private void AdvancedActions_Click(object sender, EventArgs e)
        {
            AdvancedModal.CategoriesChanged = false;
            AdvancedModal.ShowDialog(this);
        }

        private void CollectionResultButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(CollectionNameBox.Text.Trim())) {
                var selectedFiles = FileListView.SelectedItems;
                var fileIDs = new List<int>();
                foreach (ListViewItem file in selectedFiles) {
                    int id = core.ActiveFiles.Find(f => f.Fullname == file.ImageKey).ID;
                    fileIDs.Add(id);
                }

                var res = core.AddCollection(CollectionNameBox.Text.Trim(), fileIDs);
                if (res.Result) {
                    UpdateMessage($"Added collection {CollectionNameBox.Text.Trim()}", Color.Black);
                    CollectionNameBox.Clear();
                    CollectionNameBox.AutoCompleteCustomSource.Add(CollectionNameBox.Text.Trim());
                } else {
                    UpdateMessage("Failed to add collection", ErrorMsgColor);
                    UpdateMessageToolTip(res.Messages);
                }
            }
        }
        
        private void CollectionFilePickerButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(CollectionNameBox.Text.Trim())) {
                return;
            }

            if (FileDialog.ShowDialog() == DialogResult.OK) {
                core.SaveActiveFilesBackup();
                List<string> filesToAdd = new List<string>();
                FileSearchFilter filter = new FileSearchFilter();
                List<FileSearchFilter> subfilters = new List<FileSearchFilter>();

                foreach (string filename in FileDialog.FileNames) {
                    filter.Reset().SetFullnameFilter(filename);
                    if (core.GetFileData(filter).Result.Count == 0) {
                        filesToAdd.Add(filename);
                        logger.LogInformation("Opened and adding new file: " + filename);
                    } else {
                        logger.LogInformation($"File {filename} already in DB");
                    }
                    subfilters.Add(new FileSearchFilter().SetOr(true).SetFullnameFilter(filename));
                }

                core.RestoreActiveFilesFromBackup();
                var addRes = core.AddFiles(filesToAdd);
                filter.Reset();
                foreach (var subfilter in subfilters) {
                    filter.AddSubfilter(subfilter);
                }
                var fileData = core.GetFileData(filter);
                var fileIds = fileData.Result.ConvertAll(f => f.ID);
                var res = core.AddCollection(CollectionNameBox.Text.Trim(), fileIds);

                if (res.HasError()) {
                    string errMsg = "";
                    int bad = res.Messages.Count;
                    int good = FileDialog.FileNames.Length - bad;
                    errMsg += $"Adding files to collection resulted in {good} successes and {bad} failures\n\n";
                    UpdateMessage(errMsg, ErrorMsgColor);
                    UpdateMessageToolTip(res.Messages);
                } else {
                    UpdateMessage($"{FileDialog.FileNames.Length} files added to collection, " +
                        $"{filesToAdd.Count} new files added to DB", 
                        Color.Black);
                    MessageTooltip.SetToolTip(MessageText, MessageText.Text);
                }
            }
        }

        private void CollectionFileAddButton_Click(object sender, EventArgs e)
        {
            if (CollectionListView.SelectedItems.Count > 0 && FileListView.SelectedItems.Count > 0 ) {
                var collection = core.ActiveCollections.Find(c => c.Name == CollectionListView.SelectedItems[0].Text);
                var res = new ActionResult<bool>();
                res.SetResult(true);
               
                foreach (ListViewItem key in FileListView.SelectedItems) {
                    int id = core.ActiveFiles.Find(f => f.Fullname == key.ImageKey).ID;
                    var addRes = core.AddFileToCollection(collection.ID, id);
                    ActionResult.AppendErrors(res, addRes);
                    if (!addRes.Result) res.SetResult(false);
                }

                if (res.Result) {
                    UpdateMessage($"Added {FileListView.SelectedItems.Count} files to " +
                        $"collection {CollectionListView.SelectedItems[0].Text}", 
                        Color.Black);
                } else {
                    UpdateMessage("Failed to add files to collection", ErrorMsgColor);
                    UpdateMessageToolTip(res.Messages);
                }
            }
        }

        private void CollectionListView_SelectionChanged(object sender, EventArgs e)
        {
            if (CollectionListView.SelectedItems.Count == 1) {
                CollectionSymlinkButton.Enabled = true;
                ShowCollectionFilesButton.Enabled = true;
            } else {
                CollectionSymlinkButton.Enabled = false;
                ShowCollectionFilesButton.Enabled = false;
            }
        }

        private void CollectionListItem_Click(object sender, MouseEventArgs e)
        {
            if (FileListView.GetItemAt(e.X, e.Y) != null) {
                AddFileToCollectionsEnableVerify();
            }
        }

        private void CollectionListItem_DoubleClick(object sender, MouseEventArgs e)
        {
            ListViewItem item = CollectionListView.GetItemAt(e.X, e.Y);
            if (item != null) {
                var selectedCollection = core.ActiveCollections.Find(c => c.Name == item.Text);
                if (selectedCollection != null) {
                    core.SaveActiveFilesBackup();
                    CollectionInfoModal.SetCollection(selectedCollection);
                    var dialogResult = CollectionInfoModal.ShowDialog(this);
                    core.RestoreActiveFilesFromBackup();

                    if (CollectionInfoModal.IsUpdated && !CollectionInfoModal.IsDeleted) {
                        if (CollectionInfoModal.NewName != item.Text) {
                            CollectionSearchBox.AutoCompleteCustomSource.Remove(item.Text);
                            CollectionSearchBox.AutoCompleteCustomSource.Add(CollectionInfoModal.NewName);
                            item.Text = CollectionInfoModal.NewName;
                        }
                        if (CollectionInfoModal.IsFilesChanged) {
                            core.ActiveCollections.Remove(selectedCollection);
                            core.ActiveCollections.Add(core.GetFileCollection(selectedCollection.ID).Result);
                        }
                    } else if (dialogResult == DialogResult.No && CollectionInfoModal.IsDeleted) {
                        core.ActiveCollections.Remove(selectedCollection);
                        CollectionListView.Items.Remove(item);
                        CollectionSearchBox.AutoCompleteCustomSource.Remove(item.Text);
                    }

                    CollectionInfoModal.ClearCollection();
                }
            }
        }

        private void CollectionSymlinkButton_Click(object sender, EventArgs e)
        {
            if (CollectionListView.SelectedItems.Count == 1) {
                var collection = core.ActiveCollections.Find(c => c.Name == CollectionListView.SelectedItems[0].Text);
                var res = core.CreateSymLinksCollection(collection);
                if (res.Result) {
                    UpdateMessage("Symlinks created for collection " + collection.Name, Color.Black);
                } else {
                    UpdateMessage($"Symlink creation for collection {collection.Name} resulted in failures", ErrorMsgColor);
                    UpdateMessageToolTip(res.Messages);
                }
            }
        }

        private void ShowCollectionFilesButton_Click(object sender, EventArgs e)
        {
            if (CollectionListView.SelectedItems.Count == 1) {
                var collection = core.ActiveCollections.Find(c => c.Name == CollectionListView.SelectedItems[0].Text);
                var filter = new FileSearchFilter();
                foreach (var file in collection.Files) {
                    var subfilter = new FileSearchFilter().SetIDFilter(file.FileID).SetOr(true);
                    filter.AddSubfilter(subfilter);
                }

                var fileData = core.GetFileData(filter);
                imageList.Images.Clear();
                FileListView.Clear();
                ShowFiles(fileData);
            }
        }
        #endregion

        #region Functionality

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
                ShowFiles(files);

                if (!searchSet.Contains(SearchBox.Text.Trim())) {
                    searchHistory.Enqueue(SearchBox.Text.Trim());
                    SearchBox.AutoCompleteCustomSource.Add(SearchBox.Text.Trim());
                    if (searchHistory.Count > HistoryLimit) {
                        SearchBox.AutoCompleteCustomSource.Remove(searchHistory.Dequeue());
                    }
                }
            }
        }

        private void ShowFiles(ActionResult<List<GetFileMetadataType>> fileData)
        {
            if (!fileData.HasError()) {
                var imageSet = fileData.Result.FindAll(f => f.FileType == "image").ConvertAll(f => f.Fullname).ToHashSet();
                var thumbnailMap = thumbnailProxy.GetThumbnails(fileData.Result.ConvertAll(f => f.Fullname), imageSet);
                imageList.Images.Clear();
                foreach (var pair in thumbnailMap) {
                    imageList.Images.Add(pair.Key, pair.Value);
                }

                FileListView.BeginUpdate();
                FileListView.LargeImageList = imageList;
                var sorted = fileData.Result.OrderBy(f => Path.GetFileNameWithoutExtension(f.Filename), new NaturalOrderComparer())
                                            .ToList();

                foreach (var filedata in sorted) {
                    FileListView.Items.Add(new ListViewItem(filedata.Filename, filedata.Fullname));
                }

                FileListView.EndUpdate();

                UpdateMessage($"Found {fileData.Result.Count} file(s)", Color.Black);
                MessageTooltip.RemoveAll();
            } else {
                string errMsg = "Failed to query files";
                UpdateMessage(errMsg, ErrorMsgColor);
                UpdateMessageToolTip(fileData.Messages);
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
                    var item = new ListViewItem(tag.Name);
                    item.ToolTipText = tag.Category;
                    TagListView.Items.Add(item);
                }
            }
        }

        private void ViewFileTags(ListViewItem item)
        {
            if (item != null) {
                var selectedFile = core.ActiveFiles.Find(it => it.Fullname == item.ImageKey);
                if (selectedFile != null) {
                    var fileTags = core.GetTagsForFile(selectedFile.ID);
                    TagListView.Clear();
                    foreach (var tag in fileTags.Result) {
                        TagListView.Items.Add(new ListViewItem(tag.Name));
                    }
                    selectedFileID = selectedFile.ID;
                    UpdateMessage($"Viewing tags for {selectedFile.Filename}", Color.Black);
                } else {
                    logger.LogError($"File {item.ImageKey} was missing from cached search results");
                    UpdateMessage("File missing from results", ErrorMsgColor);
                }
            }
        }

        private void AssignTagsToFileVerify()
        {
            AssignTagButton.Enabled = TagListView.SelectedItems.Count > 0 && FileListView.SelectedItems.Count > 0;
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

        private void SearchCollections(string query)
        {
            var collections = core.SearchFileCollection(query);
            CollectionListView.Clear();
            foreach (var collection in collections.Result) {
                CollectionListView.Items.Add(new ListViewItem(collection.Name));
            }
        }

        private void AddFileToCollectionsEnableVerify()
        {
            if (FileListView.SelectedItems.Count > 0 && CollectionListView.SelectedItems.Count > 0) {
                CollectionAddFileButton.Enabled = true;
            } else {
                CollectionAddFileButton.Enabled = false;
            }
        }

        private void UpdateMessage(string msg, Color color)
        {
            MessageText.Text = msg;
            MessageText.ForeColor = color;
        }

        private void UpdateMessageToolTip(List<string> msgs)
        {
            if (msgs is null || msgs.Count == 0) {
                logger.LogInformation("No error messages to dispaly on tooltip");
                return;
            }

            string errMsg = "";
            foreach (string msg in msgs) {
                errMsg += msg + "\n";
            }
            MessageTooltip.SetToolTip(MessageText, errMsg);
        }

        #endregion

        
    }
}
