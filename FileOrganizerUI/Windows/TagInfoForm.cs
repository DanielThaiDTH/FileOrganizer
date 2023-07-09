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
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileOrganizerUI.Windows
{
    /// <summary>
    ///     Form for modifying tag. Assigned GetTagType will be modified on update (excluding name).
    /// </summary>
    public partial class TagInfoForm : Form
    {
        ILogger logger;
        FileOrganizer core;
        static GetTagCategoryType DefaultCategory = new GetTagCategoryType { ID = -1, Name = "-- None --" };
        
        public bool IsUpdated;
        public bool IsDeleted;
        public bool NameUpdated { get; private set; }
        public bool CategoryUpdated { get; private set; }
        private bool hasError;

        public GetTagType Info { get; private set; }
        DeleteConfirmModal DeleteModal;
        IObservable<object> editObservable;
        IDisposable updateCheck;

        public TagInfoForm(ILogger logger, FileOrganizer core)
        {
            InitializeComponent();
            this.logger = logger;
            this.core = core;
            RefreshTagCategoryComboBox();

            UpdateButton.Click += UpdateButton_Click;
            UpdateButton.Enabled = false;

            DeleteButton.Click += DeleteButton_Click;

            DeleteModal = new DeleteConfirmModal();
            DeleteModal.SetMessage("Confirm deletion of tag?");

            editObservable = Observable.FromEventPattern(handler =>
            {
                NameBox.TextChanged += handler;
                DescriptionBox.TextChanged += handler;
                CategoryComboBox.SelectedIndexChanged += handler;
                ParentComboBox.SelectedIndexChanged += handler;
            },
            handler =>
            {
                NameBox.TextChanged -= handler;
                DescriptionBox.TextChanged -= handler;
                CategoryComboBox.SelectedIndexChanged -= handler;
                ParentComboBox.SelectedIndexChanged -= handler;
            });
        }

        public void SetTagInfo(GetTagType info)
        {
            if (info is null) return;
            Info = info;
            IsUpdated = false;
            IsDeleted = false;
            NameUpdated = false;
            CategoryUpdated = false;
            UpdateButton.Enabled = false;
            NameBox.Text = info.Name;
            DescriptionBox.Text = info.Description;
            MessageLabel.Text = "";
            RefreshTagCategoryComboBox();
            RefreshParentComboBox();
            if (!string.IsNullOrWhiteSpace(info.Category) && info.CategoryID > -1) {
                CategoryComboBox.SelectedText = info.Category;
            }

            updateCheck = editObservable.Subscribe((args) =>
            {
                if (Info is null) return;
                bool changed = NameBox.Text != Info.Name || DescriptionBox.Text != Info.Description;
                changed = changed || IsCategoryChanged() || IsParentChanged();
                UpdateButton.Enabled = changed;
            });
        }

        public void ClearTagInfo()
        {
            Info = null;
            NameBox.Text = "";
            DescriptionBox.Text = "";
            CategoryComboBox.SelectedItem = DefaultCategory;
            IsUpdated = false;
            IsDeleted = false;
            NameUpdated = false;
            CategoryUpdated = false;
            updateCheck.Dispose();
            MessageLabel.Text = "";
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            hasError = false;

            if (NameBox.Text.Trim() != Info.Name) {
                UpdateName();
            }

            if (IsCategoryChanged()) {
                UpdateCategory();
            }

            if (DescriptionBox.Text != Info.Description) {
                UpdateDescription();
            }

            if (IsParentChanged()) {
                UpdateParent();
            }
            
            if (!hasError) UpdateMessage("Tag updated", Color.Black);
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (Info is null) return;
            var res = DeleteModal.ShowDialog(this);
            if (res == DialogResult.Yes) {
                var deleteResult = core.DeleteTag(Info.ID);
                if (deleteResult.Result) {
                    logger.LogInformation("Closing info screen for deleted tag " + Info.Name);
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
        private void RefreshTagCategoryComboBox()
        {
            CategoryComboBox.Items.Clear();
            var selectedCategory = DefaultCategory;
            foreach (var category in core.TagCategories) {
                CategoryComboBox.Items.Add(category);
                if (Info != null && category.Name == Info.Category) selectedCategory = category;
            }

            CategoryComboBox.Items.Add(DefaultCategory);
            CategoryComboBox.SelectedItem = selectedCategory;
        }

        private void RefreshParentComboBox()
        {
            ParentComboBox.Items.Clear();
            string selectedTag = DefaultCategory.Name;
            foreach (var tag in core.AllTags) {
                ParentComboBox.Items.Add(tag.Name);
                if (Info != null && tag.ID == Info.ParentTagID) selectedTag = tag.Name;
            }

            ParentComboBox.Items.Add(DefaultCategory.Name);
            ParentComboBox.SelectedItem = selectedTag;
        }

        private void UpdateMessage(string msg, Color color)
        {
            MessageLabel.Text = msg;
            MessageLabel.ForeColor = color;
        }

        private string SelectedCategoryText()
        {
            if ((CategoryComboBox.SelectedItem as GetTagCategoryType).ID == -1) {
                return "";
            } else {
                return (CategoryComboBox.SelectedItem as GetTagCategoryType).Name;
            }
        }

        private void UpdateName()
        {
            var nameRes = core.UpdateTagName(NameBox.Text.Trim(), Info.ID);
            if (nameRes.Result) {
                IsUpdated = true;
                NameUpdated = true;
            } else {
                hasError = true;
                UpdateMessage(nameRes.GetErrorMessage(0), MainForm.ErrorMsgColor);
            }
        }

        private void UpdateParent()
        {
            var parent = core.AllTags.Find(t => t.Name == (string) ParentComboBox.SelectedItem);
            int parentID = -1;
            if (parent != null) {
                parentID = parent.ID;
            }
            var res = core.UpdateTagParent(Info.ID, parentID);

            if (res.Result) {
                IsUpdated = true;
            } else {
                hasError = true;
                string msg = res.GetErrorMessage(0);
                if (msg.Length > 50) msg = "Failed to change parent due to ancestry loop"; 
                UpdateMessage(msg, MainForm.ErrorMsgColor);
            }
        }

        private void UpdateCategory()
        {
            int newCategoryID;
            logger.LogDebug("Selected category option: " + SelectedCategoryText());
            if (CategoryComboBox.SelectedItem.Equals(DefaultCategory)) {
                newCategoryID = -1;
            } else {
                newCategoryID = (CategoryComboBox.SelectedItem as GetTagCategoryType).ID;
            }

            var categoryRes = core.UpdateTagCategory(Info.ID, newCategoryID);
            if (categoryRes.Result) {
                IsUpdated = true;
                CategoryUpdated = true;
                Info.Category = SelectedCategoryText();
                Info.CategoryID = newCategoryID;
            } else {
                hasError = true;
                UpdateMessage(categoryRes.GetErrorMessage(0), MainForm.ErrorMsgColor);
            }
        }

        private void UpdateDescription()
        {
            var descRes = core.UpdateTagDescription(Info.ID, DescriptionBox.Text);
            if (descRes.Result) {
                IsUpdated = true;
                Info.Description = DescriptionBox.Text;
            } else {
                UpdateMessage(descRes.GetErrorMessage(0), MainForm.ErrorMsgColor);
            }
        }

        private bool IsCategoryChanged()
        {
            return CategoryComboBox.SelectedIndex != -1 && 
                !(CategoryComboBox.SelectedItem.Equals(DefaultCategory) && Info.Category is null) &&
                !(Info.Category != null && (CategoryComboBox.SelectedItem as GetTagCategoryType).Name == Info.Category);
        }

        private bool IsParentChanged()
        {
            if (Info is null) return false;
            var parentTag = core.AllTags.Find(t => t.ID == Info.ID);
            return ParentComboBox.SelectedIndex != -1 && 
                !(parentTag is null && (ParentComboBox.SelectedItem as string) == DefaultCategory.Name) && 
                !(parentTag != null && (ParentComboBox.SelectedItem as string) == parentTag.Name);
        }
        #endregion
    }
}
