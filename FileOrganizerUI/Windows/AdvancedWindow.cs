using FileDBManager;
using FileOrganizerCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FileOrganizerCore.JSONOutput;

namespace FileOrganizerUI.Windows
{
    public partial class AdvancedWindow : Form
    {
        ILogger logger;
        FileOrganizer core;
        DeleteConfirmModal DeleteModal;
        ToolTip MessageTooltip;
        public bool CategoriesChanged { get; set; } = false;
        static GetTagCategoryType DefaultCategory = new GetTagCategoryType { ID = -1, Name = "-- None --" };
        Action<string, string> ResultPathNotify;


        public AdvancedWindow(ILogger logger, FileOrganizer core)
        {
            InitializeComponent();
            this.logger = logger;
            this.core = core;
            MessageTooltip = new ToolTip();

            VisibleChanged += RefreshCategoryDropdownOnVisible;
            
            AddButton.Click += AddButton_Click;
            RenameCategoryButton.Click += CategoryUpdateButton_Click;
            CategoryDeleteButton.Click += CategoryDeleteButton_Click;

            CategoryComboBox.SelectedIndexChanged += CategoryComboBoxChange;

            DeleteModal = new DeleteConfirmModal();
            DeleteModal.SetMessage("Confirm deletion of tag category?");

            ExportTabSetup();
            UpdateTabSetup();
        }

        public void ResultPathSubscribe(Action<string, string> subscription)
        {
            ResultPathNotify = subscription;
        }

        public void ResultPathUnsubscribe()
        {
            ResultPathNotify = null;
        }

        #region Handlers
        private void DoneButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void RefreshCategoryDropdownOnVisible(object sender, EventArgs e)
        {
            RefreshTagCategoryComboBox();
        }

        private void CategoryComboBoxChange(object sender, EventArgs e)
        {
            if (CategoryComboBox.SelectedItem.Equals(DefaultCategory)) {
                RenameCategoryButton.Enabled = false;
                CategoryDeleteButton.Enabled = false;
            } else {
                RenameCategoryButton.Enabled = true;
                CategoryDeleteButton.Enabled = true;
            }
        }

        private void CategoryUpdateButton_Click(object sender, EventArgs e)
        {
            int id = (CategoryComboBox.SelectedItem as GetTagCategoryType).ID;
            string oldName = (CategoryComboBox.SelectedItem as GetTagCategoryType).Name;
            var res = core.UpdateTagCategoryName(id, CategoryRenameBox.Text.Trim());
            if (res.Result) {
                UpdateMessage($"Renamed {oldName} to {CategoryRenameBox.Text.Trim()}", Color.Black);
                RefreshTagCategoryComboBox();
                CategoriesChanged = true;
            } else {
                UpdateMessage($"Failed to rename {oldName} to {CategoryRenameBox.Text.Trim()}", MainForm.ErrorMsgColor);
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            var res = core.AddTagCategory(AddCateboryBox.Text.Trim());
            if (res.Result) {
                RefreshTagCategoryComboBox();
                CategoriesChanged = true;
                UpdateMessage("Added category " + AddCateboryBox.Text.Trim(), Color.Black);
            } else {
                UpdateMessage("Failed to add category " + AddCateboryBox.Text.Trim(), MainForm.ErrorMsgColor);
            }
        }

        private void CategoryDeleteButton_Click(object sender, EventArgs e)
        {
            int id = (CategoryComboBox.SelectedItem as GetTagCategoryType).ID;
            string delName = (CategoryComboBox.SelectedItem as GetTagCategoryType).Name;

            var confirmRes = DeleteModal.ShowDialog(this);
            if (confirmRes == DialogResult.Yes) {
                var res = core.DeleteTagCategory(id);

                if (res.Result) {
                    UpdateMessage($"Deleted category {delName}", Color.Black);
                    RefreshTagCategoryComboBox();
                    CategoriesChanged = true;
                } else {
                    UpdateMessage($"Failed to delete {delName}", MainForm.ErrorMsgColor);
                }
            }

        }

        private void ExportTypeRadioButton_Changed(object sender, EventArgs e)
        {
            PathCheckbox.Enabled = JSONRadioButton.Checked;
            HashCheckbox.Enabled = JSONRadioButton.Checked;
            RemoveSeparatorsCheckbox.Enabled = JSONRadioButton.Checked && HashCheckbox.Checked;
            SizeCheckbox.Enabled = JSONRadioButton.Checked;
            SizeUnitCombobox.Enabled = JSONRadioButton.Checked && SizeCheckbox.Checked;
            FileSizeLabel.Enabled = JSONRadioButton.Checked && SizeCheckbox.Checked;
            CreatedCheckbox.Enabled = JSONRadioButton.Checked;
            TagCheckbox.Enabled = JSONRadioButton.Checked;
        }

        private void SizeCheckbox_Changed(object sender, EventArgs e)
        {
            FileSizeLabel.Enabled = JSONRadioButton.Checked && SizeCheckbox.Checked;
            SizeUnitCombobox.Enabled = JSONRadioButton.Checked && SizeCheckbox.Checked;
        }

        private void HashCheckbox_Changed(object sender, EventArgs e)
        {
            RemoveSeparatorsCheckbox.Enabled = JSONRadioButton.Checked && HashCheckbox.Checked;
        }

        private void ExportAll_Click(object sender, EventArgs e)
        {
            core.SaveActiveFilesBackup();
            WriteOutputFile(core.GetFileData().Result);
            core.RestoreActiveFilesFromBackup();
        }

        private void ExportResults_Click(object sender, EventArgs e)
        {
            WriteOutputFile(core.ActiveFiles);
        }

        private void UpdateResultsPath_Click(object sender, EventArgs e)
        {
            ActionResult<bool> combinedRes = new ActionResult<bool>();
            combinedRes.SetResult(true);
            foreach (var file in core.ActiveFiles) {
                var res = core.ChangePathForFile(file.ID, NewPathBox.Text.Trim());
                combinedRes.SetResult(combinedRes.Result && res.Result);
                if (!res.Result) ActionResult.AppendErrors(combinedRes, res);
            }

            if (combinedRes.Result) {
                int pathID = core.GetPathID(NewPathBox.Text.Trim()).Result;
                foreach (var file in core.ActiveFiles) {
                    file.PathID = pathID;
                    file.Path = NewPathBox.Text.Trim();
                }

                foreach (var file in core.ActiveFiles) {
                    logger.LogDebug("AW Fullname: " + file.Fullname);
                }

                if (ResultPathNotify != null) ResultPathNotify(null, NewPathBox.Text.Trim());

                UpdateMessage("Path for file in results updated to " + NewPathBox.Text.Trim(), Color.Black);
            } else {
                UpdateMessage("Errors occured when changing path for files", MainForm.ErrorMsgColor);
                UpdateMessageToolTip(combinedRes.Messages);
            }
        }

        private void NewPathBox_TextChange(object sender, EventArgs e)
        {
            try {
                if (Path.IsPathRooted(NewPathBox.Text.Trim())) {
                    UpdateResultsPathsButton.Enabled = true;
                } else {
                    UpdateResultsPathsButton.Enabled = false;
                }
            } catch {
                UpdateResultsPathsButton.Enabled = false;
            }
        }
        #endregion

        #region Functionality
        private void ExportTabSetup()
        {
            RemoveSeparatorsCheckbox.Enabled = false;
            SizeUnitCombobox.Enabled = false;
            FileSizeLabel.Enabled = false;
            SizeUnitCombobox.Items.Add("Bytes");
            SizeUnitCombobox.Items.Add("Kilobytes");
            SizeUnitCombobox.Items.Add("Megabytes");
            SizeUnitCombobox.Items.Add("Gigabytes");
            SizeUnitCombobox.SelectedItem = "Bytes";

            FullPathCheckbox.Checked = true;
            TextRadioButton.CheckedChanged += ExportTypeRadioButton_Changed;
            SizeCheckbox.CheckedChanged += SizeCheckbox_Changed;
            HashCheckbox.CheckedChanged += HashCheckbox_Changed;

            ExportAllButton.Click += ExportAll_Click;
            ExportResultsButton.Click += ExportResults_Click;
        }

        private void UpdateTabSetup()
        {
            UpdateResultsPathsButton.Click += UpdateResultsPath_Click;
            UpdateResultsPathsButton.Enabled = false;

            NewPathBox.TextChanged += NewPathBox_TextChange;
        }

        private void RefreshTagCategoryComboBox()
        {
            CategoryComboBox.Items.Clear();
            foreach (var category in core.TagCategories) {
                CategoryComboBox.Items.Add(category);
            }

            CategoryComboBox.Items.Add(DefaultCategory);
            CategoryComboBox.SelectedItem = DefaultCategory;
            RenameCategoryButton.Enabled = false;
            CategoryDeleteButton.Enabled = false;
        }

        private void WriteOutputFile(List<GetFileMetadataType> files)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "output";
            dlg.DefaultExt = JSONRadioButton.Checked ? ".json" : ".txt";
            dlg.Filter = JSONRadioButton.Checked ? "JSON File|*.json" : "Text File|*.txt";

            if (dlg.ShowDialog(this) == DialogResult.OK) {
                if (JSONRadioButton.Checked) {
                    JSONOptions options = new JSONOptions
                    {
                        IncludeFullName = FullPathCheckbox.Checked,
                        IncludePath = PathCheckbox.Checked,
                        IncludeHash = HashCheckbox.Checked,
                        RemoveSeparators = RemoveSeparatorsCheckbox.Checked,
                        IncludeCreatedDate = CreatedCheckbox.Checked,
                        IncludeFileSize = SizeCheckbox.Checked,
                        SizeUnit = FileSizeUnit.Bytes,
                        IncludeTags = TagCheckbox.Checked
                    };

                    string sizeOption = (string)SizeUnitCombobox.SelectedItem;
                    if (sizeOption == "Bytes") {
                        options.SizeUnit = FileSizeUnit.Bytes;
                    } else if (sizeOption == "Kilobytes") {
                        options.SizeUnit = FileSizeUnit.Kilobytes;
                    } else if (sizeOption == "Megabytes") {
                        options.SizeUnit = FileSizeUnit.Megabytes;
                    } else if (sizeOption == "Gigabytes") {
                        options.SizeUnit = FileSizeUnit.Gigabytes;
                    }

                    try {
                        core.JSONOutputFileWrite(dlg.FileName, options, files);
                    }
                    catch (Exception ex) {
                        logger.LogWarning("Failed to output file due to " + ex.Message);
                        logger.LogDebug(ex.StackTrace);
                        UpdateMessage("Failed to write JSON file", MainForm.ErrorMsgColor);
                    }

                    UpdateMessage("Output JSON file saved", Color.Black);

                } else {

                    try {
                        core.TextOutputFileWrite(dlg.FileName, FullPathCheckbox.Checked, files);
                    }
                    catch (Exception ex) {
                        logger.LogWarning("Failed to output file due to " + ex.Message);
                        logger.LogDebug(ex.StackTrace);
                        UpdateMessage("Failed to write text file", MainForm.ErrorMsgColor);
                    }

                    UpdateMessage("Output text file saved", Color.Black);

                }
            }
        }

        private void UpdateMessage(string msg, Color color)
        {
            MessageLabel.Text = msg;
            MessageLabel.ForeColor = color;
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
            MessageTooltip.SetToolTip(MessageLabel, errMsg);
        }
        #endregion
    }
}
