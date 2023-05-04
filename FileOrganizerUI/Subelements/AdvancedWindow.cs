using FileDBManager;
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

namespace FileOrganizerUI.Subelements
{
    public partial class AdvancedWindow : Form
    {
        ILogger logger;
        FileOrganizer core;
        DeleteConfirmModal DeleteModal;
        public bool CategoriesChanged { get; set; } = false;
        static GetTagCategoryType DefaultCategory = new GetTagCategoryType { ID = -1, Name = "-- None --" };

        public AdvancedWindow(ILogger logger, FileOrganizer core)
        {
            InitializeComponent();
            this.logger = logger;
            this.core = core;

            VisibleChanged += RefreshCategoryDropdownOnVisible;
            
            AddButton.Click += AddButton_Click;
            RenameCategoryButton.Click += CategoryUpdateButton_Click;
            CategoryDeleteButton.Click += CategoryDeleteButton_Click;

            CategoryComboBox.SelectedIndexChanged += CategoryComboBoxChange;

            DeleteModal = new DeleteConfirmModal();
            DeleteModal.SetMessage("Confirm deletion of tag category?");
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
        #endregion

        #region Functionality
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

        private void UpdateMessage(string msg, Color color)
        {
            MessageLabel.Text = msg;
            MessageLabel.ForeColor = color;
        }
        #endregion
    }
}
