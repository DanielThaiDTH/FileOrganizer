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
    public partial class TagInfoForm : Form
    {
        ILogger logger;
        FileOrganizer core;
        static GetTagCategoryType DefaultCategory = new GetTagCategoryType { ID = -1, Name = "-- None --" };

        public TagInfoForm(ILogger logger, FileOrganizer core)
        {
            InitializeComponent();
            this.logger = logger;
            this.core = core;
            RefreshTagCategoryComboBox();
        }

        public void SetTagInfo(GetTagType info)
        {
            NameBox.Text = info.Name;
            DescriptionBox.Text = info.Description;
            if (string.IsNullOrWhiteSpace(info.Category)) {
                CategoryComboBox.SelectedItem = DefaultCategory;
            } else {
                CategoryComboBox.SelectedText = info.Category;
            }
        }

        public void ClearTagInfo()
        {
            NameBox.Text = "";
            DescriptionBox.Text = "";
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
        #endregion
    }
}
