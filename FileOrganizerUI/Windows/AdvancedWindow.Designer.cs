﻿
namespace FileOrganizerUI.Windows
{
    partial class AdvancedWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ExportTab = new System.Windows.Forms.TabPage();
            this.ExportResultsButton = new System.Windows.Forms.Button();
            this.ExportAllButton = new System.Windows.Forms.Button();
            this.OutputFileBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ExportOptionsGroup = new System.Windows.Forms.GroupBox();
            this.TagCheckbox = new System.Windows.Forms.CheckBox();
            this.CreatedCheckbox = new System.Windows.Forms.CheckBox();
            this.FileSizeLabel = new System.Windows.Forms.Label();
            this.SizeUnitCombobox = new System.Windows.Forms.ComboBox();
            this.SizeCheckbox = new System.Windows.Forms.CheckBox();
            this.RemoveSeparatorsCheckbox = new System.Windows.Forms.CheckBox();
            this.HashCheckbox = new System.Windows.Forms.CheckBox();
            this.PathCheckbox = new System.Windows.Forms.CheckBox();
            this.FullPathCheckbox = new System.Windows.Forms.CheckBox();
            this.ExportTypeGroup = new System.Windows.Forms.GroupBox();
            this.TextRadioButton = new System.Windows.Forms.RadioButton();
            this.JSONRadioButton = new System.Windows.Forms.RadioButton();
            this.CategoriesTab = new System.Windows.Forms.TabPage();
            this.layoutCategories = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.CategorySelectLabel = new System.Windows.Forms.Label();
            this.CategoryComboBox = new System.Windows.Forms.ComboBox();
            this.CategoryDeleteButton = new System.Windows.Forms.Button();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.RenameLabel = new System.Windows.Forms.Label();
            this.CategoryRenameBox = new System.Windows.Forms.TextBox();
            this.RenameCategoryButton = new System.Windows.Forms.Button();
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.DescriptionLabel = new System.Windows.Forms.Label();
            this.AddCateboryBox = new System.Windows.Forms.TextBox();
            this.AddButton = new System.Windows.Forms.Button();
            this.UpdateColorGroup = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel7 = new System.Windows.Forms.FlowLayoutPanel();
            this.ColorLabel = new System.Windows.Forms.Label();
            this.ColorBox = new System.Windows.Forms.TextBox();
            this.ColorPictureBox = new System.Windows.Forms.PictureBox();
            this.OpenColorDialogButton = new System.Windows.Forms.Button();
            this.UpdateColorButton = new System.Windows.Forms.Button();
            this.flowLayoutPanel6 = new System.Windows.Forms.FlowLayoutPanel();
            this.CatagoryComboLabel = new System.Windows.Forms.Label();
            this.CategoryColorComboBox = new System.Windows.Forms.ComboBox();
            this.AdvancedTabs = new System.Windows.Forms.TabControl();
            this.ImportTab = new System.Windows.Forms.TabPage();
            this.UpdateTab = new System.Windows.Forms.TabPage();
            this.PathGroup = new System.Windows.Forms.GroupBox();
            this.MoveFilesCheckbox = new System.Windows.Forms.CheckBox();
            this.UpdateResultsPathsButton = new System.Windows.Forms.Button();
            this.UpdatePathsButton = new System.Windows.Forms.Button();
            this.flowLayoutPanel5 = new System.Windows.Forms.FlowLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.NewPathBox = new System.Windows.Forms.TextBox();
            this.flowLayoutPanel4 = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.OldPathBox = new System.Windows.Forms.TextBox();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.DoneButton = new System.Windows.Forms.Button();
            this.MessageLabel = new System.Windows.Forms.Label();
            this.SelectFolderButton = new System.Windows.Forms.Button();
            this.ExportTab.SuspendLayout();
            this.ExportOptionsGroup.SuspendLayout();
            this.ExportTypeGroup.SuspendLayout();
            this.CategoriesTab.SuspendLayout();
            this.layoutCategories.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel3.SuspendLayout();
            this.UpdateColorGroup.SuspendLayout();
            this.flowLayoutPanel7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ColorPictureBox)).BeginInit();
            this.flowLayoutPanel6.SuspendLayout();
            this.AdvancedTabs.SuspendLayout();
            this.UpdateTab.SuspendLayout();
            this.PathGroup.SuspendLayout();
            this.flowLayoutPanel5.SuspendLayout();
            this.flowLayoutPanel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // ExportTab
            // 
            this.ExportTab.BackColor = System.Drawing.SystemColors.Control;
            this.ExportTab.Controls.Add(this.ExportResultsButton);
            this.ExportTab.Controls.Add(this.ExportAllButton);
            this.ExportTab.Controls.Add(this.OutputFileBox);
            this.ExportTab.Controls.Add(this.label2);
            this.ExportTab.Controls.Add(this.ExportOptionsGroup);
            this.ExportTab.Controls.Add(this.ExportTypeGroup);
            this.ExportTab.Location = new System.Drawing.Point(4, 25);
            this.ExportTab.Name = "ExportTab";
            this.ExportTab.Padding = new System.Windows.Forms.Padding(3);
            this.ExportTab.Size = new System.Drawing.Size(791, 433);
            this.ExportTab.TabIndex = 1;
            this.ExportTab.Text = "Export";
            // 
            // ExportResultsButton
            // 
            this.ExportResultsButton.Location = new System.Drawing.Point(571, 155);
            this.ExportResultsButton.Name = "ExportResultsButton";
            this.ExportResultsButton.Size = new System.Drawing.Size(163, 23);
            this.ExportResultsButton.TabIndex = 6;
            this.ExportResultsButton.Text = "Export Search Results";
            this.ExportResultsButton.UseVisualStyleBackColor = true;
            // 
            // ExportAllButton
            // 
            this.ExportAllButton.Location = new System.Drawing.Point(571, 99);
            this.ExportAllButton.Name = "ExportAllButton";
            this.ExportAllButton.Size = new System.Drawing.Size(163, 23);
            this.ExportAllButton.TabIndex = 5;
            this.ExportAllButton.Text = "Export All Files";
            this.ExportAllButton.UseVisualStyleBackColor = true;
            // 
            // OutputFileBox
            // 
            this.OutputFileBox.Location = new System.Drawing.Point(347, 27);
            this.OutputFileBox.Name = "OutputFileBox";
            this.OutputFileBox.Size = new System.Drawing.Size(417, 22);
            this.OutputFileBox.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(228, 27);
            this.label2.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(112, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "Output Filename";
            // 
            // ExportOptionsGroup
            // 
            this.ExportOptionsGroup.Controls.Add(this.TagCheckbox);
            this.ExportOptionsGroup.Controls.Add(this.CreatedCheckbox);
            this.ExportOptionsGroup.Controls.Add(this.FileSizeLabel);
            this.ExportOptionsGroup.Controls.Add(this.SizeUnitCombobox);
            this.ExportOptionsGroup.Controls.Add(this.SizeCheckbox);
            this.ExportOptionsGroup.Controls.Add(this.RemoveSeparatorsCheckbox);
            this.ExportOptionsGroup.Controls.Add(this.HashCheckbox);
            this.ExportOptionsGroup.Controls.Add(this.PathCheckbox);
            this.ExportOptionsGroup.Controls.Add(this.FullPathCheckbox);
            this.ExportOptionsGroup.Location = new System.Drawing.Point(7, 77);
            this.ExportOptionsGroup.Name = "ExportOptionsGroup";
            this.ExportOptionsGroup.Size = new System.Drawing.Size(453, 350);
            this.ExportOptionsGroup.TabIndex = 2;
            this.ExportOptionsGroup.TabStop = false;
            this.ExportOptionsGroup.Text = "Options";
            // 
            // TagCheckbox
            // 
            this.TagCheckbox.AutoSize = true;
            this.TagCheckbox.Location = new System.Drawing.Point(7, 162);
            this.TagCheckbox.Name = "TagCheckbox";
            this.TagCheckbox.Size = new System.Drawing.Size(111, 21);
            this.TagCheckbox.TabIndex = 8;
            this.TagCheckbox.Text = "Include Tags";
            this.TagCheckbox.UseVisualStyleBackColor = true;
            // 
            // CreatedCheckbox
            // 
            this.CreatedCheckbox.AutoSize = true;
            this.CreatedCheckbox.Location = new System.Drawing.Point(7, 134);
            this.CreatedCheckbox.Name = "CreatedCheckbox";
            this.CreatedCheckbox.Size = new System.Drawing.Size(163, 21);
            this.CreatedCheckbox.TabIndex = 7;
            this.CreatedCheckbox.Text = "Include Created Date";
            this.CreatedCheckbox.UseVisualStyleBackColor = true;
            // 
            // FileSizeLabel
            // 
            this.FileSizeLabel.AutoSize = true;
            this.FileSizeLabel.Location = new System.Drawing.Point(337, 112);
            this.FileSizeLabel.Name = "FileSizeLabel";
            this.FileSizeLabel.Size = new System.Drawing.Size(90, 17);
            this.FileSizeLabel.TabIndex = 6;
            this.FileSizeLabel.Text = "File Size Unit";
            // 
            // SizeUnitCombobox
            // 
            this.SizeUnitCombobox.FormattingEnabled = true;
            this.SizeUnitCombobox.Location = new System.Drawing.Point(221, 106);
            this.SizeUnitCombobox.Name = "SizeUnitCombobox";
            this.SizeUnitCombobox.Size = new System.Drawing.Size(109, 24);
            this.SizeUnitCombobox.TabIndex = 5;
            // 
            // SizeCheckbox
            // 
            this.SizeCheckbox.AutoSize = true;
            this.SizeCheckbox.Location = new System.Drawing.Point(7, 106);
            this.SizeCheckbox.Name = "SizeCheckbox";
            this.SizeCheckbox.Size = new System.Drawing.Size(132, 21);
            this.SizeCheckbox.TabIndex = 4;
            this.SizeCheckbox.Text = "Include File Size";
            this.SizeCheckbox.UseVisualStyleBackColor = true;
            // 
            // RemoveSeparatorsCheckbox
            // 
            this.RemoveSeparatorsCheckbox.AutoSize = true;
            this.RemoveSeparatorsCheckbox.Location = new System.Drawing.Point(221, 78);
            this.RemoveSeparatorsCheckbox.Name = "RemoveSeparatorsCheckbox";
            this.RemoveSeparatorsCheckbox.Size = new System.Drawing.Size(193, 21);
            this.RemoveSeparatorsCheckbox.TabIndex = 3;
            this.RemoveSeparatorsCheckbox.Text = "Remove Hash Separators";
            this.RemoveSeparatorsCheckbox.UseVisualStyleBackColor = true;
            // 
            // HashCheckbox
            // 
            this.HashCheckbox.AutoSize = true;
            this.HashCheckbox.Location = new System.Drawing.Point(7, 78);
            this.HashCheckbox.Name = "HashCheckbox";
            this.HashCheckbox.Size = new System.Drawing.Size(112, 21);
            this.HashCheckbox.TabIndex = 2;
            this.HashCheckbox.Text = "Include Hash";
            this.HashCheckbox.UseVisualStyleBackColor = true;
            // 
            // PathCheckbox
            // 
            this.PathCheckbox.AutoSize = true;
            this.PathCheckbox.Location = new System.Drawing.Point(7, 50);
            this.PathCheckbox.Name = "PathCheckbox";
            this.PathCheckbox.Size = new System.Drawing.Size(108, 21);
            this.PathCheckbox.TabIndex = 1;
            this.PathCheckbox.Text = "Include Path";
            this.PathCheckbox.UseVisualStyleBackColor = true;
            // 
            // FullPathCheckbox
            // 
            this.FullPathCheckbox.AutoSize = true;
            this.FullPathCheckbox.Location = new System.Drawing.Point(7, 22);
            this.FullPathCheckbox.Name = "FullPathCheckbox";
            this.FullPathCheckbox.Size = new System.Drawing.Size(190, 21);
            this.FullPathCheckbox.TabIndex = 0;
            this.FullPathCheckbox.Text = "Include Full Path in Name";
            this.FullPathCheckbox.UseVisualStyleBackColor = true;
            // 
            // ExportTypeGroup
            // 
            this.ExportTypeGroup.Controls.Add(this.TextRadioButton);
            this.ExportTypeGroup.Controls.Add(this.JSONRadioButton);
            this.ExportTypeGroup.Location = new System.Drawing.Point(7, 6);
            this.ExportTypeGroup.Name = "ExportTypeGroup";
            this.ExportTypeGroup.Size = new System.Drawing.Size(182, 55);
            this.ExportTypeGroup.TabIndex = 1;
            this.ExportTypeGroup.TabStop = false;
            this.ExportTypeGroup.Text = "Export Type";
            // 
            // TextRadioButton
            // 
            this.TextRadioButton.AutoSize = true;
            this.TextRadioButton.Location = new System.Drawing.Point(104, 22);
            this.TextRadioButton.Name = "TextRadioButton";
            this.TextRadioButton.Size = new System.Drawing.Size(56, 21);
            this.TextRadioButton.TabIndex = 1;
            this.TextRadioButton.Text = "Text";
            this.TextRadioButton.UseVisualStyleBackColor = true;
            // 
            // JSONRadioButton
            // 
            this.JSONRadioButton.AutoSize = true;
            this.JSONRadioButton.Checked = true;
            this.JSONRadioButton.Location = new System.Drawing.Point(6, 21);
            this.JSONRadioButton.Name = "JSONRadioButton";
            this.JSONRadioButton.Size = new System.Drawing.Size(66, 21);
            this.JSONRadioButton.TabIndex = 0;
            this.JSONRadioButton.TabStop = true;
            this.JSONRadioButton.Text = "JSON";
            this.JSONRadioButton.UseVisualStyleBackColor = true;
            // 
            // CategoriesTab
            // 
            this.CategoriesTab.BackColor = System.Drawing.SystemColors.Control;
            this.CategoriesTab.Controls.Add(this.layoutCategories);
            this.CategoriesTab.Location = new System.Drawing.Point(4, 25);
            this.CategoriesTab.Name = "CategoriesTab";
            this.CategoriesTab.Padding = new System.Windows.Forms.Padding(3);
            this.CategoriesTab.Size = new System.Drawing.Size(791, 433);
            this.CategoriesTab.TabIndex = 0;
            this.CategoriesTab.Text = "Manage Categories";
            // 
            // layoutCategories
            // 
            this.layoutCategories.Controls.Add(this.flowLayoutPanel1);
            this.layoutCategories.Controls.Add(this.flowLayoutPanel2);
            this.layoutCategories.Controls.Add(this.flowLayoutPanel3);
            this.layoutCategories.Controls.Add(this.UpdateColorGroup);
            this.layoutCategories.Location = new System.Drawing.Point(4, 4);
            this.layoutCategories.Name = "layoutCategories";
            this.layoutCategories.Size = new System.Drawing.Size(784, 426);
            this.layoutCategories.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.CategorySelectLabel);
            this.flowLayoutPanel1.Controls.Add(this.CategoryComboBox);
            this.flowLayoutPanel1.Controls.Add(this.CategoryDeleteButton);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(650, 30);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // CategorySelectLabel
            // 
            this.CategorySelectLabel.AutoSize = true;
            this.CategorySelectLabel.Location = new System.Drawing.Point(3, 6);
            this.CategorySelectLabel.Margin = new System.Windows.Forms.Padding(3, 6, 45, 0);
            this.CategorySelectLabel.Name = "CategorySelectLabel";
            this.CategorySelectLabel.Size = new System.Drawing.Size(94, 17);
            this.CategorySelectLabel.TabIndex = 0;
            this.CategorySelectLabel.Text = "Tag Category";
            // 
            // CategoryComboBox
            // 
            this.CategoryComboBox.FormattingEnabled = true;
            this.CategoryComboBox.Location = new System.Drawing.Point(145, 3);
            this.CategoryComboBox.Name = "CategoryComboBox";
            this.CategoryComboBox.Size = new System.Drawing.Size(421, 24);
            this.CategoryComboBox.TabIndex = 1;
            // 
            // CategoryDeleteButton
            // 
            this.CategoryDeleteButton.Location = new System.Drawing.Point(572, 3);
            this.CategoryDeleteButton.Name = "CategoryDeleteButton";
            this.CategoryDeleteButton.Size = new System.Drawing.Size(75, 23);
            this.CategoryDeleteButton.TabIndex = 2;
            this.CategoryDeleteButton.Text = "Delete";
            this.CategoryDeleteButton.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.Controls.Add(this.RenameLabel);
            this.flowLayoutPanel2.Controls.Add(this.CategoryRenameBox);
            this.flowLayoutPanel2.Controls.Add(this.RenameCategoryButton);
            this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 39);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(650, 29);
            this.flowLayoutPanel2.TabIndex = 1;
            // 
            // RenameLabel
            // 
            this.RenameLabel.AutoSize = true;
            this.RenameLabel.Location = new System.Drawing.Point(3, 6);
            this.RenameLabel.Margin = new System.Windows.Forms.Padding(3, 6, 17, 0);
            this.RenameLabel.Name = "RenameLabel";
            this.RenameLabel.Size = new System.Drawing.Size(122, 17);
            this.RenameLabel.TabIndex = 3;
            this.RenameLabel.Text = "Rename Category";
            // 
            // CategoryRenameBox
            // 
            this.CategoryRenameBox.Location = new System.Drawing.Point(145, 3);
            this.CategoryRenameBox.Name = "CategoryRenameBox";
            this.CategoryRenameBox.Size = new System.Drawing.Size(421, 22);
            this.CategoryRenameBox.TabIndex = 4;
            // 
            // RenameCategoryButton
            // 
            this.RenameCategoryButton.Location = new System.Drawing.Point(572, 3);
            this.RenameCategoryButton.Name = "RenameCategoryButton";
            this.RenameCategoryButton.Size = new System.Drawing.Size(75, 23);
            this.RenameCategoryButton.TabIndex = 5;
            this.RenameCategoryButton.Text = "Rename";
            this.RenameCategoryButton.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel3
            // 
            this.flowLayoutPanel3.AutoSize = true;
            this.flowLayoutPanel3.Controls.Add(this.DescriptionLabel);
            this.flowLayoutPanel3.Controls.Add(this.AddCateboryBox);
            this.flowLayoutPanel3.Controls.Add(this.AddButton);
            this.flowLayoutPanel3.Location = new System.Drawing.Point(3, 74);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Size = new System.Drawing.Size(650, 29);
            this.flowLayoutPanel3.TabIndex = 2;
            // 
            // DescriptionLabel
            // 
            this.DescriptionLabel.AutoSize = true;
            this.DescriptionLabel.Location = new System.Drawing.Point(3, 6);
            this.DescriptionLabel.Margin = new System.Windows.Forms.Padding(3, 6, 42, 0);
            this.DescriptionLabel.Name = "DescriptionLabel";
            this.DescriptionLabel.Size = new System.Drawing.Size(96, 17);
            this.DescriptionLabel.TabIndex = 4;
            this.DescriptionLabel.Text = "New Category";
            // 
            // AddCateboryBox
            // 
            this.AddCateboryBox.Location = new System.Drawing.Point(144, 3);
            this.AddCateboryBox.Name = "AddCateboryBox";
            this.AddCateboryBox.Size = new System.Drawing.Size(422, 22);
            this.AddCateboryBox.TabIndex = 5;
            // 
            // AddButton
            // 
            this.AddButton.Location = new System.Drawing.Point(572, 3);
            this.AddButton.Name = "AddButton";
            this.AddButton.Size = new System.Drawing.Size(75, 23);
            this.AddButton.TabIndex = 6;
            this.AddButton.Text = "Add";
            this.AddButton.UseVisualStyleBackColor = true;
            // 
            // UpdateColorGroup
            // 
            this.UpdateColorGroup.Controls.Add(this.flowLayoutPanel7);
            this.UpdateColorGroup.Controls.Add(this.flowLayoutPanel6);
            this.UpdateColorGroup.Location = new System.Drawing.Point(3, 109);
            this.UpdateColorGroup.Name = "UpdateColorGroup";
            this.UpdateColorGroup.Size = new System.Drawing.Size(650, 119);
            this.UpdateColorGroup.TabIndex = 3;
            this.UpdateColorGroup.TabStop = false;
            this.UpdateColorGroup.Text = "Update Tag Category Color";
            // 
            // flowLayoutPanel7
            // 
            this.flowLayoutPanel7.Controls.Add(this.ColorLabel);
            this.flowLayoutPanel7.Controls.Add(this.ColorBox);
            this.flowLayoutPanel7.Controls.Add(this.ColorPictureBox);
            this.flowLayoutPanel7.Controls.Add(this.OpenColorDialogButton);
            this.flowLayoutPanel7.Controls.Add(this.UpdateColorButton);
            this.flowLayoutPanel7.Location = new System.Drawing.Point(7, 65);
            this.flowLayoutPanel7.Name = "flowLayoutPanel7";
            this.flowLayoutPanel7.Size = new System.Drawing.Size(637, 33);
            this.flowLayoutPanel7.TabIndex = 2;
            // 
            // ColorLabel
            // 
            this.ColorLabel.AutoSize = true;
            this.ColorLabel.Location = new System.Drawing.Point(3, 6);
            this.ColorLabel.Margin = new System.Windows.Forms.Padding(3, 6, 89, 0);
            this.ColorLabel.Name = "ColorLabel";
            this.ColorLabel.Size = new System.Drawing.Size(41, 17);
            this.ColorLabel.TabIndex = 0;
            this.ColorLabel.Text = "Color";
            // 
            // ColorBox
            // 
            this.ColorBox.Enabled = false;
            this.ColorBox.Location = new System.Drawing.Point(136, 6);
            this.ColorBox.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.ColorBox.Name = "ColorBox";
            this.ColorBox.Size = new System.Drawing.Size(100, 22);
            this.ColorBox.TabIndex = 1;
            // 
            // ColorPictureBox
            // 
            this.ColorPictureBox.Location = new System.Drawing.Point(242, 6);
            this.ColorPictureBox.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.ColorPictureBox.Name = "ColorPictureBox";
            this.ColorPictureBox.Size = new System.Drawing.Size(27, 22);
            this.ColorPictureBox.TabIndex = 4;
            this.ColorPictureBox.TabStop = false;
            // 
            // OpenColorDialogButton
            // 
            this.OpenColorDialogButton.Location = new System.Drawing.Point(275, 3);
            this.OpenColorDialogButton.Name = "OpenColorDialogButton";
            this.OpenColorDialogButton.Size = new System.Drawing.Size(110, 30);
            this.OpenColorDialogButton.TabIndex = 2;
            this.OpenColorDialogButton.Text = "Choose Color";
            this.OpenColorDialogButton.UseVisualStyleBackColor = true;
            // 
            // UpdateColorButton
            // 
            this.UpdateColorButton.Location = new System.Drawing.Point(391, 3);
            this.UpdateColorButton.Name = "UpdateColorButton";
            this.UpdateColorButton.Size = new System.Drawing.Size(75, 30);
            this.UpdateColorButton.TabIndex = 3;
            this.UpdateColorButton.Text = "Update";
            this.UpdateColorButton.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel6
            // 
            this.flowLayoutPanel6.Controls.Add(this.CatagoryComboLabel);
            this.flowLayoutPanel6.Controls.Add(this.CategoryColorComboBox);
            this.flowLayoutPanel6.Location = new System.Drawing.Point(7, 22);
            this.flowLayoutPanel6.Name = "flowLayoutPanel6";
            this.flowLayoutPanel6.Size = new System.Drawing.Size(637, 36);
            this.flowLayoutPanel6.TabIndex = 1;
            // 
            // CatagoryComboLabel
            // 
            this.CatagoryComboLabel.AutoSize = true;
            this.CatagoryComboLabel.Location = new System.Drawing.Point(3, 6);
            this.CatagoryComboLabel.Margin = new System.Windows.Forms.Padding(3, 6, 65, 0);
            this.CatagoryComboLabel.Name = "CatagoryComboLabel";
            this.CatagoryComboLabel.Size = new System.Drawing.Size(65, 17);
            this.CatagoryComboLabel.TabIndex = 0;
            this.CatagoryComboLabel.Text = "Category";
            // 
            // CategoryColorComboBox
            // 
            this.CategoryColorComboBox.FormattingEnabled = true;
            this.CategoryColorComboBox.Location = new System.Drawing.Point(136, 3);
            this.CategoryColorComboBox.Name = "CategoryColorComboBox";
            this.CategoryColorComboBox.Size = new System.Drawing.Size(423, 24);
            this.CategoryColorComboBox.TabIndex = 0;
            // 
            // AdvancedTabs
            // 
            this.AdvancedTabs.Controls.Add(this.CategoriesTab);
            this.AdvancedTabs.Controls.Add(this.ExportTab);
            this.AdvancedTabs.Controls.Add(this.ImportTab);
            this.AdvancedTabs.Controls.Add(this.UpdateTab);
            this.AdvancedTabs.Controls.Add(this.tabPage1);
            this.AdvancedTabs.Location = new System.Drawing.Point(13, 13);
            this.AdvancedTabs.Name = "AdvancedTabs";
            this.AdvancedTabs.SelectedIndex = 0;
            this.AdvancedTabs.Size = new System.Drawing.Size(820, 462);
            this.AdvancedTabs.TabIndex = 0;
            // 
            // ImportTab
            // 
            this.ImportTab.BackColor = System.Drawing.SystemColors.Control;
            this.ImportTab.Location = new System.Drawing.Point(4, 25);
            this.ImportTab.Name = "ImportTab";
            this.ImportTab.Padding = new System.Windows.Forms.Padding(3);
            this.ImportTab.Size = new System.Drawing.Size(791, 433);
            this.ImportTab.TabIndex = 2;
            this.ImportTab.Text = "Import";
            // 
            // UpdateTab
            // 
            this.UpdateTab.BackColor = System.Drawing.SystemColors.Control;
            this.UpdateTab.Controls.Add(this.PathGroup);
            this.UpdateTab.Location = new System.Drawing.Point(4, 25);
            this.UpdateTab.Name = "UpdateTab";
            this.UpdateTab.Padding = new System.Windows.Forms.Padding(3);
            this.UpdateTab.Size = new System.Drawing.Size(812, 433);
            this.UpdateTab.TabIndex = 3;
            this.UpdateTab.Text = "Update";
            // 
            // PathGroup
            // 
            this.PathGroup.Controls.Add(this.MoveFilesCheckbox);
            this.PathGroup.Controls.Add(this.UpdateResultsPathsButton);
            this.PathGroup.Controls.Add(this.UpdatePathsButton);
            this.PathGroup.Controls.Add(this.flowLayoutPanel5);
            this.PathGroup.Controls.Add(this.flowLayoutPanel4);
            this.PathGroup.Location = new System.Drawing.Point(7, 7);
            this.PathGroup.Name = "PathGroup";
            this.PathGroup.Size = new System.Drawing.Size(799, 130);
            this.PathGroup.TabIndex = 0;
            this.PathGroup.TabStop = false;
            this.PathGroup.Text = "Path";
            // 
            // MoveFilesCheckbox
            // 
            this.MoveFilesCheckbox.AutoSize = true;
            this.MoveFilesCheckbox.Location = new System.Drawing.Point(405, 100);
            this.MoveFilesCheckbox.Name = "MoveFilesCheckbox";
            this.MoveFilesCheckbox.Size = new System.Drawing.Size(97, 21);
            this.MoveFilesCheckbox.TabIndex = 8;
            this.MoveFilesCheckbox.Text = "Move Files";
            this.MoveFilesCheckbox.UseVisualStyleBackColor = true;
            // 
            // UpdateResultsPathsButton
            // 
            this.UpdateResultsPathsButton.Location = new System.Drawing.Point(508, 94);
            this.UpdateResultsPathsButton.Name = "UpdateResultsPathsButton";
            this.UpdateResultsPathsButton.Size = new System.Drawing.Size(121, 30);
            this.UpdateResultsPathsButton.TabIndex = 7;
            this.UpdateResultsPathsButton.Text = "Update Results";
            this.UpdateResultsPathsButton.UseVisualStyleBackColor = true;
            // 
            // UpdatePathsButton
            // 
            this.UpdatePathsButton.Location = new System.Drawing.Point(635, 94);
            this.UpdatePathsButton.Name = "UpdatePathsButton";
            this.UpdatePathsButton.Size = new System.Drawing.Size(75, 30);
            this.UpdatePathsButton.TabIndex = 6;
            this.UpdatePathsButton.Text = "Update";
            this.UpdatePathsButton.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel5
            // 
            this.flowLayoutPanel5.Controls.Add(this.label3);
            this.flowLayoutPanel5.Controls.Add(this.NewPathBox);
            this.flowLayoutPanel5.Controls.Add(this.SelectFolderButton);
            this.flowLayoutPanel5.Location = new System.Drawing.Point(9, 57);
            this.flowLayoutPanel5.Name = "flowLayoutPanel5";
            this.flowLayoutPanel5.Size = new System.Drawing.Size(784, 27);
            this.flowLayoutPanel5.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 6);
            this.label3.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 17);
            this.label3.TabIndex = 1;
            this.label3.Text = "New Path";
            // 
            // NewPathBox
            // 
            this.NewPathBox.Location = new System.Drawing.Point(77, 3);
            this.NewPathBox.Name = "NewPathBox";
            this.NewPathBox.Size = new System.Drawing.Size(624, 22);
            this.NewPathBox.TabIndex = 3;
            // 
            // flowLayoutPanel4
            // 
            this.flowLayoutPanel4.Controls.Add(this.label1);
            this.flowLayoutPanel4.Controls.Add(this.OldPathBox);
            this.flowLayoutPanel4.Location = new System.Drawing.Point(9, 21);
            this.flowLayoutPanel4.Name = "flowLayoutPanel4";
            this.flowLayoutPanel4.Size = new System.Drawing.Size(784, 29);
            this.flowLayoutPanel4.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 6);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 6, 8, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Old Path";
            // 
            // OldPathBox
            // 
            this.OldPathBox.Location = new System.Drawing.Point(77, 3);
            this.OldPathBox.Name = "OldPathBox";
            this.OldPathBox.Size = new System.Drawing.Size(625, 22);
            this.OldPathBox.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(791, 433);
            this.tabPage1.TabIndex = 4;
            this.tabPage1.Text = "Other";
            // 
            // DoneButton
            // 
            this.DoneButton.Location = new System.Drawing.Point(754, 482);
            this.DoneButton.Name = "DoneButton";
            this.DoneButton.Size = new System.Drawing.Size(75, 31);
            this.DoneButton.TabIndex = 1;
            this.DoneButton.Text = "Done";
            this.DoneButton.UseVisualStyleBackColor = true;
            this.DoneButton.Click += new System.EventHandler(this.DoneButton_Click);
            // 
            // MessageLabel
            // 
            this.MessageLabel.AutoSize = true;
            this.MessageLabel.Location = new System.Drawing.Point(21, 482);
            this.MessageLabel.Name = "MessageLabel";
            this.MessageLabel.Size = new System.Drawing.Size(0, 17);
            this.MessageLabel.TabIndex = 2;
            // 
            // SelectFolderButton
            // 
            this.SelectFolderButton.Location = new System.Drawing.Point(707, 3);
            this.SelectFolderButton.Name = "SelectFolderButton";
            this.SelectFolderButton.Size = new System.Drawing.Size(64, 23);
            this.SelectFolderButton.TabIndex = 4;
            this.SelectFolderButton.Text = "Select";
            this.SelectFolderButton.UseVisualStyleBackColor = true;
            // 
            // AdvancedWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(845, 517);
            this.Controls.Add(this.MessageLabel);
            this.Controls.Add(this.DoneButton);
            this.Controls.Add(this.AdvancedTabs);
            this.Name = "AdvancedWindow";
            this.Text = "Advanced";
            this.ExportTab.ResumeLayout(false);
            this.ExportTab.PerformLayout();
            this.ExportOptionsGroup.ResumeLayout(false);
            this.ExportOptionsGroup.PerformLayout();
            this.ExportTypeGroup.ResumeLayout(false);
            this.ExportTypeGroup.PerformLayout();
            this.CategoriesTab.ResumeLayout(false);
            this.layoutCategories.ResumeLayout(false);
            this.layoutCategories.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.flowLayoutPanel3.ResumeLayout(false);
            this.flowLayoutPanel3.PerformLayout();
            this.UpdateColorGroup.ResumeLayout(false);
            this.flowLayoutPanel7.ResumeLayout(false);
            this.flowLayoutPanel7.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ColorPictureBox)).EndInit();
            this.flowLayoutPanel6.ResumeLayout(false);
            this.flowLayoutPanel6.PerformLayout();
            this.AdvancedTabs.ResumeLayout(false);
            this.UpdateTab.ResumeLayout(false);
            this.PathGroup.ResumeLayout(false);
            this.PathGroup.PerformLayout();
            this.flowLayoutPanel5.ResumeLayout(false);
            this.flowLayoutPanel5.PerformLayout();
            this.flowLayoutPanel4.ResumeLayout(false);
            this.flowLayoutPanel4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabPage ExportTab;
        private System.Windows.Forms.TabPage CategoriesTab;
        private System.Windows.Forms.FlowLayoutPanel layoutCategories;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label CategorySelectLabel;
        private System.Windows.Forms.ComboBox CategoryComboBox;
        private System.Windows.Forms.Button CategoryDeleteButton;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Label RenameLabel;
        private System.Windows.Forms.TextBox CategoryRenameBox;
        private System.Windows.Forms.Button RenameCategoryButton;
        private System.Windows.Forms.TabControl AdvancedTabs;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
        private System.Windows.Forms.Label DescriptionLabel;
        private System.Windows.Forms.Button DoneButton;
        private System.Windows.Forms.Label MessageLabel;
        private System.Windows.Forms.TextBox AddCateboryBox;
        private System.Windows.Forms.Button AddButton;
        private System.Windows.Forms.TabPage ImportTab;
        private System.Windows.Forms.GroupBox ExportTypeGroup;
        private System.Windows.Forms.RadioButton TextRadioButton;
        private System.Windows.Forms.RadioButton JSONRadioButton;
        private System.Windows.Forms.GroupBox ExportOptionsGroup;
        private System.Windows.Forms.CheckBox FullPathCheckbox;
        private System.Windows.Forms.CheckBox RemoveSeparatorsCheckbox;
        private System.Windows.Forms.CheckBox HashCheckbox;
        private System.Windows.Forms.CheckBox PathCheckbox;
        private System.Windows.Forms.CheckBox SizeCheckbox;
        private System.Windows.Forms.ComboBox SizeUnitCombobox;
        private System.Windows.Forms.CheckBox TagCheckbox;
        private System.Windows.Forms.CheckBox CreatedCheckbox;
        private System.Windows.Forms.Label FileSizeLabel;
        private System.Windows.Forms.TextBox OutputFileBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button ExportResultsButton;
        private System.Windows.Forms.Button ExportAllButton;
        private System.Windows.Forms.TabPage UpdateTab;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.GroupBox PathGroup;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox NewPathBox;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox OldPathBox;
        private System.Windows.Forms.Button UpdateResultsPathsButton;
        private System.Windows.Forms.Button UpdatePathsButton;
        private System.Windows.Forms.GroupBox UpdateColorGroup;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel7;
        private System.Windows.Forms.Label ColorLabel;
        private System.Windows.Forms.TextBox ColorBox;
        private System.Windows.Forms.Button OpenColorDialogButton;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel6;
        private System.Windows.Forms.Label CatagoryComboLabel;
        private System.Windows.Forms.ComboBox CategoryColorComboBox;
        private System.Windows.Forms.Button UpdateColorButton;
        private System.Windows.Forms.PictureBox ColorPictureBox;
        private System.Windows.Forms.CheckBox MoveFilesCheckbox;
        private System.Windows.Forms.Button SelectFolderButton;
    }
}