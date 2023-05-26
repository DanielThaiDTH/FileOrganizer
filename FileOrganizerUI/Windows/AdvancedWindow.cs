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

        Color selectedColor;
        
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

            CategoryColorComboBox.SelectedIndexChanged += ColorCategoryCombobox_SelectChange;

            OpenColorDialogButton.Click += OpenColorPicker_Click;
            OpenColorDialogButton.Enabled = false;
            selectedColor = Color.FromArgb(-1);
            ColorBox.Text = selectedColor.ToArgb().ToString("X");
            ColorPictureBox.BackColor = selectedColor;
            
            UpdateColorButton.Enabled = false;
            UpdateColorButton.Click += UpdateColorButton_Click;

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

        public void RefreshWindow()
        {
            RefreshTagCategoryComboBox();
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

        private void ColorCategoryCombobox_SelectChange(object sender, EventArgs e)
        {
            OpenColorDialogButton.Enabled = CategoryColorComboBox.SelectedIndex != -1;
            UpdateColorButton.Enabled = false;
            
            if (CategoryColorComboBox.SelectedIndex != -1) {
                var category = CategoryColorComboBox.SelectedItem as GetTagCategoryType;
                selectedColor = Color.FromArgb(category.Color);
                ColorBox.Text = category.Color.ToString("X");
            } else {
                selectedColor = Color.FromArgb(-1);
                ColorBox.Text = selectedColor.ToArgb().ToString("X");
            }
            ColorPictureBox.BackColor = selectedColor;
        }

        private void OpenColorPicker_Click(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();
            dlg.Color = selectedColor;

            if (dlg.ShowDialog(this) == DialogResult.OK) {
                selectedColor = dlg.Color;
                ColorBox.Text = selectedColor.ToArgb().ToString("X");
                ColorPictureBox.BackColor = selectedColor;
                UpdateColorButton.Enabled = IsColorDifferent();
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

        private void UpdateColorButton_Click(object sender, EventArgs e)
        {
            var category = CategoryColorComboBox.SelectedItem as GetTagCategoryType;
            var res = core.UpdateTagCategoryColor(category.ID, selectedColor);
            if (res.Result) {
                UpdateMessage($"Color for tag category {category.Name} changed to #{selectedColor.ToArgb().ToString("X")}", Color.Black);
                CategoriesChanged = true;
                category.Color = selectedColor.ToArgb();
                UpdateColorButton.Enabled = false;
            } else {
                UpdateMessage("Failed to update category color for " + category.Name, MainForm.ErrorMsgColor);
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
                    logger.LogDebug($"Updating path of cached file from {file.Path} to {NewPathBox.Text.Trim()}");
                    file.PathID = pathID;
                    file.Path = NewPathBox.Text.Trim();
                }

                if (ResultPathNotify != null) ResultPathNotify(null, NewPathBox.Text.Trim());

                UpdateMessage("Path for file in results updated to " + NewPathBox.Text.Trim(), Color.Black);
            } else {
                UpdateMessage("Errors occured when changing path for files", MainForm.ErrorMsgColor);
                UpdateMessageToolTip(combinedRes.Messages);
            }
        }

        private void UpdatePaths_Click(object sender, EventArgs e)
        {
            var result = core.ChangePathAll(OldPathBox.Text.Trim(), NewPathBox.Text.Trim());
            if (result.Result) {
                int id = core.GetPathID(NewPathBox.Text.Trim()).Result;
                foreach (var file in core.ActiveFiles) {
                    if (file.PathID == id) {
                        logger.LogDebug($"Updating path of cached file from {file.Path} to {NewPathBox.Text.Trim()}");
                        file.Path = NewPathBox.Text.Trim();
                    }
                }

                if (ResultPathNotify != null) ResultPathNotify(OldPathBox.Text.Trim(), NewPathBox.Text.Trim());

                UpdateMessage("Sucessfully updated paths for all files with that path", Color.Black);
            } else {
                UpdateMessage(result.GetErrorMessage(0), MainForm.ErrorMsgColor);
            }
        }

        private void PathBox_TextChange(object sender, EventArgs e)
        {
            try {

                if (Path.IsPathRooted(NewPathBox.Text.Trim())) {
                    UpdateResultsPathsButton.Enabled = true;
                } else {
                    UpdateResultsPathsButton.Enabled = false;
                }

                if (Path.IsPathRooted(OldPathBox.Text.Trim()) && 
                    Path.IsPathRooted(NewPathBox.Text.Trim()) &&
                    OldPathBox.Text.Trim() != NewPathBox.Text.Trim()) {
                    UpdatePathsButton.Enabled = true;
                } else {
                    UpdatePathsButton.Enabled = false;
                }

            } catch {
                UpdateResultsPathsButton.Enabled = false;
                UpdatePathsButton.Enabled = false;
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
            UpdatePathsButton.Click += UpdatePaths_Click;
            UpdatePathsButton.Enabled = false;

            NewPathBox.TextChanged += PathBox_TextChange;
            OldPathBox.TextChanged += PathBox_TextChange;
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

            CategoryColorComboBox.SelectedIndex = -1;
            CategoryColorComboBox.Items.Clear();
            foreach (var category in core.TagCategories) {
                CategoryColorComboBox.Items.Add(category);
            }

            OpenColorDialogButton.Enabled = false;
        }

        private bool IsColorDifferent()
        {
            if (CategoryColorComboBox.SelectedIndex != -1) {
                return selectedColor.ToArgb() != (CategoryColorComboBox.SelectedItem as GetTagCategoryType).Color;
            } else {
                return false;
            }
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
