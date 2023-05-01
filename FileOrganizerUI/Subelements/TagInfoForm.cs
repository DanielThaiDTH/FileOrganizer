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

namespace FileOrganizerUI.Subelements
{
    public partial class TagInfoForm : Form
    {
        ILogger logger;
        FileOrganizer core;
        static GetTagCategoryType DefaultCategory = new GetTagCategoryType { ID = -1, Name = "-- None --" };
        public bool IsUpdated;
        public bool IsDeleted;
        public bool NameUpdated { get; private set; }
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
            IsUpdated = false;
            IsDeleted = false;
            NameUpdated = false;

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
            },
            handler =>
            {
                NameBox.TextChanged -= handler;
                DescriptionBox.TextChanged -= handler;
                CategoryComboBox.SelectedIndexChanged -= handler;
            });
        }

        public void SetTagInfo(GetTagType info)
        {
            if (info is null) return;
            Info = info;
            IsUpdated = false;
            IsDeleted = false;
            NameUpdated = false;
            NameBox.Text = info.Name;
            DescriptionBox.Text = info.Description;
            MessageLabel.Text = "";
            RefreshTagCategoryComboBox();
            if (!string.IsNullOrWhiteSpace(info.Category) && info.CategoryID > -1) {
                CategoryComboBox.SelectedText = info.Category;
            }

            updateCheck = editObservable.Subscribe((args) =>
            {
                if (Info is null) return;
                bool changed = NameBox.Text != Info.Name || DescriptionBox.Text != Info.Description;
                changed = changed || IsCategoryChanged();
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
            updateCheck.Dispose();
            MessageLabel.Text = "";
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            if (NameBox.Text != Info.Name) {
                UpdateName();
            }

            if (IsCategoryChanged()) {
                UpdateCategory();
            }

            if (DescriptionBox.Text != Info.Description) {
                UpdateDescription();
            }

            UpdateMessage("Tag updated", Color.Black);
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
            foreach (var category in core.TagCategories) {
                CategoryComboBox.Items.Add(category);
            }

            CategoryComboBox.Items.Add(DefaultCategory);
            CategoryComboBox.SelectedItem = DefaultCategory;
        }

        private void UpdateMessage(string msg, Color color)
        {
            MessageLabel.Text = msg;
            MessageLabel.ForeColor = color;
        }

        private void UpdateName()
        {
            var nameRes = core.UpdateTagName(NameBox.Text, Info.ID);
            if (nameRes.Result) {
                IsUpdated = true;
                NameUpdated = true;
            } else {
                UpdateMessage(nameRes.GetErrorMessage(0), MainForm.ErrorMsgColor);
            }
        }

        private void UpdateCategory()
        {
            int newCategoryID;
            logger.LogDebug("Selected category option: " + CategoryComboBox.SelectedText);
            if ((CategoryComboBox.SelectedItem as GetTagCategoryType).Name == DefaultCategory.Name) {
                newCategoryID = -1;
            } else {
                newCategoryID = core.TagCategories.Find(tc => tc.Name == (CategoryComboBox.SelectedItem as GetTagCategoryType).Name).ID;
            }

            var categoryRes = core.UpdateTagCategory(Info.ID, newCategoryID);
            if (categoryRes.Result) {
                IsUpdated = true;
            } else {
                UpdateMessage(categoryRes.GetErrorMessage(0), MainForm.ErrorMsgColor);
            }
        }

        private void UpdateDescription()
        {
            var descRes = core.UpdateTagDescription(Info.ID, DescriptionBox.Text);
            if (descRes.Result) {
                IsUpdated = true;
            } else {
                UpdateMessage(descRes.GetErrorMessage(0), MainForm.ErrorMsgColor);
            }
        }

        private bool IsCategoryChanged()
        {
            return CategoryComboBox.SelectedIndex != -1 && (CategoryComboBox.SelectedItem as GetTagCategoryType).Name != DefaultCategory.Name;
        }
        #endregion
    }
}
