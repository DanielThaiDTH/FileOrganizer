
namespace FileOrganizerUI
{
    partial class MainForm
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
                AdvancedModal.ResultPathUnsubscribe();
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
            this.components = new System.ComponentModel.Container();
            this.OpenFilePicker = new System.Windows.Forms.Button();
            this.MessageText = new System.Windows.Forms.Label();
            this.MessageTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.UpdateButton = new System.Windows.Forms.Button();
            this.FileResultHeader = new System.Windows.Forms.Label();
            this.SearchBox = new System.Windows.Forms.TextBox();
            this.Search = new System.Windows.Forms.Button();
            this.FileListView = new System.Windows.Forms.ListView();
            this.FilePanel = new System.Windows.Forms.Panel();
            this.CreateSymLinksButton = new System.Windows.Forms.Button();
            this.AppSettingsButton = new System.Windows.Forms.Button();
            this.SideTab = new System.Windows.Forms.TabControl();
            this.TagsTab = new System.Windows.Forms.TabPage();
            this.RemoveTagButton = new System.Windows.Forms.Button();
            this.AssignTagButton = new System.Windows.Forms.Button();
            this.AddTagGroup = new System.Windows.Forms.GroupBox();
            this.AddNewTagButton = new System.Windows.Forms.Button();
            this.NewTagCategoryComboBox = new System.Windows.Forms.ComboBox();
            this.NewTagCategoryLabel = new System.Windows.Forms.Label();
            this.NewTagNameBox = new System.Windows.Forms.TextBox();
            this.NewTagLabel = new System.Windows.Forms.Label();
            this.TagListView = new System.Windows.Forms.ListView();
            this.TagSearchLabel = new System.Windows.Forms.Label();
            this.TagSearchBox = new System.Windows.Forms.TextBox();
            this.CollectionsTab = new System.Windows.Forms.TabPage();
            this.CollectionSymlinkButton = new System.Windows.Forms.Button();
            this.ShowCollectionFilesButton = new System.Windows.Forms.Button();
            this.CollectionAddFileButton = new System.Windows.Forms.Button();
            this.AddCollectionGroup = new System.Windows.Forms.GroupBox();
            this.CollectionPickerAddButton = new System.Windows.Forms.Button();
            this.CollectionResultAddButton = new System.Windows.Forms.Button();
            this.CollectionNameBox = new System.Windows.Forms.TextBox();
            this.ColectionNameLabel = new System.Windows.Forms.Label();
            this.CollectionListView = new System.Windows.Forms.ListView();
            this.CollectionSearchBox = new System.Windows.Forms.TextBox();
            this.CollectionSearchLabel = new System.Windows.Forms.Label();
            this.AdvancedActionsButton = new System.Windows.Forms.Button();
            this.CollectionSymlinkTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.FilePanel.SuspendLayout();
            this.SideTab.SuspendLayout();
            this.TagsTab.SuspendLayout();
            this.AddTagGroup.SuspendLayout();
            this.CollectionsTab.SuspendLayout();
            this.AddCollectionGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // OpenFilePicker
            // 
            this.OpenFilePicker.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OpenFilePicker.Location = new System.Drawing.Point(1258, 591);
            this.OpenFilePicker.Name = "OpenFilePicker";
            this.OpenFilePicker.Size = new System.Drawing.Size(65, 32);
            this.OpenFilePicker.TabIndex = 13;
            this.OpenFilePicker.Text = "Add";
            this.OpenFilePicker.UseVisualStyleBackColor = true;
            this.OpenFilePicker.Click += new System.EventHandler(this.OpenFilePicker_Click);
            // 
            // MessageText
            // 
            this.MessageText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MessageText.AutoSize = true;
            this.MessageText.Location = new System.Drawing.Point(297, 588);
            this.MessageText.MaximumSize = new System.Drawing.Size(900, 0);
            this.MessageText.Name = "MessageText";
            this.MessageText.Size = new System.Drawing.Size(0, 17);
            this.MessageText.TabIndex = 1;
            // 
            // UpdateButton
            // 
            this.UpdateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.UpdateButton.Enabled = false;
            this.UpdateButton.Location = new System.Drawing.Point(1329, 591);
            this.UpdateButton.Name = "UpdateButton";
            this.UpdateButton.Size = new System.Drawing.Size(83, 32);
            this.UpdateButton.TabIndex = 2;
            this.UpdateButton.Text = "Update";
            this.UpdateButton.UseVisualStyleBackColor = true;
            // 
            // FileResultHeader
            // 
            this.FileResultHeader.AutoSize = true;
            this.FileResultHeader.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FileResultHeader.Location = new System.Drawing.Point(282, 16);
            this.FileResultHeader.Name = "FileResultHeader";
            this.FileResultHeader.Size = new System.Drawing.Size(135, 32);
            this.FileResultHeader.TabIndex = 4;
            this.FileResultHeader.Text = "File Results";
            // 
            // SearchBox
            // 
            this.SearchBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SearchBox.Location = new System.Drawing.Point(464, 22);
            this.SearchBox.Name = "SearchBox";
            this.SearchBox.Size = new System.Drawing.Size(683, 26);
            this.SearchBox.TabIndex = 0;
            // 
            // Search
            // 
            this.Search.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Search.Location = new System.Drawing.Point(1168, 20);
            this.Search.Name = "Search";
            this.Search.Size = new System.Drawing.Size(88, 28);
            this.Search.TabIndex = 6;
            this.Search.Text = "Search";
            this.Search.UseVisualStyleBackColor = true;
            this.Search.Click += new System.EventHandler(this.Search_Click);
            // 
            // FileListView
            // 
            this.FileListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FileListView.HideSelection = false;
            this.FileListView.Location = new System.Drawing.Point(20, 3);
            this.FileListView.MinimumSize = new System.Drawing.Size(800, 530);
            this.FileListView.Name = "FileListView";
            this.FileListView.Size = new System.Drawing.Size(1147, 534);
            this.FileListView.TabIndex = 7;
            this.FileListView.UseCompatibleStateImageBehavior = false;
            // 
            // FilePanel
            // 
            this.FilePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FilePanel.Controls.Add(this.FileListView);
            this.FilePanel.Location = new System.Drawing.Point(268, 51);
            this.FilePanel.Name = "FilePanel";
            this.FilePanel.Size = new System.Drawing.Size(1152, 534);
            this.FilePanel.TabIndex = 8;
            // 
            // CreateSymLinksButton
            // 
            this.CreateSymLinksButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CreateSymLinksButton.Location = new System.Drawing.Point(1258, 629);
            this.CreateSymLinksButton.Name = "CreateSymLinksButton";
            this.CreateSymLinksButton.Size = new System.Drawing.Size(154, 32);
            this.CreateSymLinksButton.TabIndex = 9;
            this.CreateSymLinksButton.Text = "Create Symlinks";
            this.CreateSymLinksButton.UseVisualStyleBackColor = true;
            this.CreateSymLinksButton.Click += new System.EventHandler(this.CreateSymLinksButton_Click);
            // 
            // AppSettingsButton
            // 
            this.AppSettingsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AppSettingsButton.Location = new System.Drawing.Point(1345, 20);
            this.AppSettingsButton.Name = "AppSettingsButton";
            this.AppSettingsButton.Size = new System.Drawing.Size(75, 28);
            this.AppSettingsButton.TabIndex = 10;
            this.AppSettingsButton.Text = "Settings";
            this.AppSettingsButton.UseVisualStyleBackColor = true;
            this.AppSettingsButton.Click += new System.EventHandler(this.AppSettingsButton_Click);
            // 
            // SideTab
            // 
            this.SideTab.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.SideTab.Controls.Add(this.TagsTab);
            this.SideTab.Controls.Add(this.CollectionsTab);
            this.SideTab.Location = new System.Drawing.Point(12, 22);
            this.SideTab.Name = "SideTab";
            this.SideTab.SelectedIndex = 0;
            this.SideTab.Size = new System.Drawing.Size(250, 668);
            this.SideTab.TabIndex = 11;
            // 
            // TagsTab
            // 
            this.TagsTab.BackColor = System.Drawing.Color.WhiteSmoke;
            this.TagsTab.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TagsTab.Controls.Add(this.RemoveTagButton);
            this.TagsTab.Controls.Add(this.AssignTagButton);
            this.TagsTab.Controls.Add(this.AddTagGroup);
            this.TagsTab.Controls.Add(this.TagListView);
            this.TagsTab.Controls.Add(this.TagSearchLabel);
            this.TagsTab.Controls.Add(this.TagSearchBox);
            this.TagsTab.Location = new System.Drawing.Point(4, 25);
            this.TagsTab.Margin = new System.Windows.Forms.Padding(0);
            this.TagsTab.Name = "TagsTab";
            this.TagsTab.Size = new System.Drawing.Size(242, 639);
            this.TagsTab.TabIndex = 0;
            this.TagsTab.Text = "Tags";
            // 
            // RemoveTagButton
            // 
            this.RemoveTagButton.Location = new System.Drawing.Point(66, 460);
            this.RemoveTagButton.Name = "RemoveTagButton";
            this.RemoveTagButton.Size = new System.Drawing.Size(81, 34);
            this.RemoveTagButton.TabIndex = 5;
            this.RemoveTagButton.Text = "Remove";
            this.RemoveTagButton.UseVisualStyleBackColor = true;
            // 
            // AssignTagButton
            // 
            this.AssignTagButton.Location = new System.Drawing.Point(153, 460);
            this.AssignTagButton.Name = "AssignTagButton";
            this.AssignTagButton.Size = new System.Drawing.Size(75, 34);
            this.AssignTagButton.TabIndex = 4;
            this.AssignTagButton.Text = "Assign";
            this.AssignTagButton.UseVisualStyleBackColor = true;
            // 
            // AddTagGroup
            // 
            this.AddTagGroup.Controls.Add(this.AddNewTagButton);
            this.AddTagGroup.Controls.Add(this.NewTagCategoryComboBox);
            this.AddTagGroup.Controls.Add(this.NewTagCategoryLabel);
            this.AddTagGroup.Controls.Add(this.NewTagNameBox);
            this.AddTagGroup.Controls.Add(this.NewTagLabel);
            this.AddTagGroup.Location = new System.Drawing.Point(6, 500);
            this.AddTagGroup.Name = "AddTagGroup";
            this.AddTagGroup.Size = new System.Drawing.Size(228, 131);
            this.AddTagGroup.TabIndex = 3;
            this.AddTagGroup.TabStop = false;
            this.AddTagGroup.Text = "Add New Tag";
            // 
            // AddNewTagButton
            // 
            this.AddNewTagButton.Location = new System.Drawing.Point(147, 90);
            this.AddNewTagButton.Name = "AddNewTagButton";
            this.AddNewTagButton.Size = new System.Drawing.Size(75, 23);
            this.AddNewTagButton.TabIndex = 4;
            this.AddNewTagButton.Text = "Add";
            this.AddNewTagButton.UseVisualStyleBackColor = true;
            // 
            // NewTagCategoryComboBox
            // 
            this.NewTagCategoryComboBox.FormattingEnabled = true;
            this.NewTagCategoryComboBox.Location = new System.Drawing.Point(80, 51);
            this.NewTagCategoryComboBox.Name = "NewTagCategoryComboBox";
            this.NewTagCategoryComboBox.Size = new System.Drawing.Size(142, 24);
            this.NewTagCategoryComboBox.TabIndex = 3;
            // 
            // NewTagCategoryLabel
            // 
            this.NewTagCategoryLabel.AutoSize = true;
            this.NewTagCategoryLabel.Location = new System.Drawing.Point(9, 54);
            this.NewTagCategoryLabel.Name = "NewTagCategoryLabel";
            this.NewTagCategoryLabel.Size = new System.Drawing.Size(65, 17);
            this.NewTagCategoryLabel.TabIndex = 2;
            this.NewTagCategoryLabel.Text = "Category";
            // 
            // NewTagNameBox
            // 
            this.NewTagNameBox.Location = new System.Drawing.Point(60, 23);
            this.NewTagNameBox.Name = "NewTagNameBox";
            this.NewTagNameBox.Size = new System.Drawing.Size(162, 22);
            this.NewTagNameBox.TabIndex = 1;
            // 
            // NewTagLabel
            // 
            this.NewTagLabel.AutoSize = true;
            this.NewTagLabel.Location = new System.Drawing.Point(9, 26);
            this.NewTagLabel.Name = "NewTagLabel";
            this.NewTagLabel.Size = new System.Drawing.Size(45, 17);
            this.NewTagLabel.TabIndex = 0;
            this.NewTagLabel.Text = "Name";
            // 
            // TagListView
            // 
            this.TagListView.HideSelection = false;
            this.TagListView.Location = new System.Drawing.Point(6, 60);
            this.TagListView.Name = "TagListView";
            this.TagListView.Size = new System.Drawing.Size(228, 393);
            this.TagListView.TabIndex = 2;
            this.TagListView.UseCompatibleStateImageBehavior = false;
            // 
            // TagSearchLabel
            // 
            this.TagSearchLabel.AutoSize = true;
            this.TagSearchLabel.Location = new System.Drawing.Point(4, 4);
            this.TagSearchLabel.Name = "TagSearchLabel";
            this.TagSearchLabel.Size = new System.Drawing.Size(53, 17);
            this.TagSearchLabel.TabIndex = 1;
            this.TagSearchLabel.Text = "Search";
            // 
            // TagSearchBox
            // 
            this.TagSearchBox.Location = new System.Drawing.Point(6, 31);
            this.TagSearchBox.Name = "TagSearchBox";
            this.TagSearchBox.Size = new System.Drawing.Size(228, 22);
            this.TagSearchBox.TabIndex = 0;
            // 
            // CollectionsTab
            // 
            this.CollectionsTab.BackColor = System.Drawing.Color.WhiteSmoke;
            this.CollectionsTab.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.CollectionsTab.Controls.Add(this.CollectionSymlinkButton);
            this.CollectionsTab.Controls.Add(this.ShowCollectionFilesButton);
            this.CollectionsTab.Controls.Add(this.CollectionAddFileButton);
            this.CollectionsTab.Controls.Add(this.AddCollectionGroup);
            this.CollectionsTab.Controls.Add(this.CollectionListView);
            this.CollectionsTab.Controls.Add(this.CollectionSearchBox);
            this.CollectionsTab.Controls.Add(this.CollectionSearchLabel);
            this.CollectionsTab.Location = new System.Drawing.Point(4, 25);
            this.CollectionsTab.Margin = new System.Windows.Forms.Padding(0);
            this.CollectionsTab.Name = "CollectionsTab";
            this.CollectionsTab.Size = new System.Drawing.Size(242, 639);
            this.CollectionsTab.TabIndex = 1;
            this.CollectionsTab.Text = "Collections";
            // 
            // CollectionSymlinkButton
            // 
            this.CollectionSymlinkButton.Location = new System.Drawing.Point(6, 487);
            this.CollectionSymlinkButton.Name = "CollectionSymlinkButton";
            this.CollectionSymlinkButton.Size = new System.Drawing.Size(75, 23);
            this.CollectionSymlinkButton.TabIndex = 7;
            this.CollectionSymlinkButton.Text = "Symlinks";
            this.CollectionSymlinkButton.UseVisualStyleBackColor = true;
            // 
            // ShowCollectionFilesButton
            // 
            this.ShowCollectionFilesButton.Location = new System.Drawing.Point(87, 487);
            this.ShowCollectionFilesButton.Name = "ShowCollectionFilesButton";
            this.ShowCollectionFilesButton.Size = new System.Drawing.Size(60, 23);
            this.ShowCollectionFilesButton.TabIndex = 6;
            this.ShowCollectionFilesButton.Text = "Show";
            this.ShowCollectionFilesButton.UseVisualStyleBackColor = true;
            // 
            // CollectionAddFileButton
            // 
            this.CollectionAddFileButton.Location = new System.Drawing.Point(153, 487);
            this.CollectionAddFileButton.Name = "CollectionAddFileButton";
            this.CollectionAddFileButton.Size = new System.Drawing.Size(75, 23);
            this.CollectionAddFileButton.TabIndex = 5;
            this.CollectionAddFileButton.Text = "Add File";
            this.CollectionAddFileButton.UseVisualStyleBackColor = true;
            // 
            // AddCollectionGroup
            // 
            this.AddCollectionGroup.Controls.Add(this.CollectionPickerAddButton);
            this.AddCollectionGroup.Controls.Add(this.CollectionResultAddButton);
            this.AddCollectionGroup.Controls.Add(this.CollectionNameBox);
            this.AddCollectionGroup.Controls.Add(this.ColectionNameLabel);
            this.AddCollectionGroup.Location = new System.Drawing.Point(7, 516);
            this.AddCollectionGroup.Name = "AddCollectionGroup";
            this.AddCollectionGroup.Size = new System.Drawing.Size(227, 118);
            this.AddCollectionGroup.TabIndex = 4;
            this.AddCollectionGroup.TabStop = false;
            this.AddCollectionGroup.Text = "Add Collection";
            // 
            // CollectionPickerAddButton
            // 
            this.CollectionPickerAddButton.Location = new System.Drawing.Point(59, 79);
            this.CollectionPickerAddButton.Name = "CollectionPickerAddButton";
            this.CollectionPickerAddButton.Size = new System.Drawing.Size(162, 23);
            this.CollectionPickerAddButton.TabIndex = 3;
            this.CollectionPickerAddButton.Text = "Add From File Picker";
            this.CollectionPickerAddButton.UseVisualStyleBackColor = true;
            // 
            // CollectionResultAddButton
            // 
            this.CollectionResultAddButton.Location = new System.Drawing.Point(59, 50);
            this.CollectionResultAddButton.Name = "CollectionResultAddButton";
            this.CollectionResultAddButton.Size = new System.Drawing.Size(162, 23);
            this.CollectionResultAddButton.TabIndex = 2;
            this.CollectionResultAddButton.Text = "Add From Selected";
            this.CollectionResultAddButton.UseVisualStyleBackColor = true;
            // 
            // CollectionNameBox
            // 
            this.CollectionNameBox.Location = new System.Drawing.Point(59, 22);
            this.CollectionNameBox.Name = "CollectionNameBox";
            this.CollectionNameBox.Size = new System.Drawing.Size(162, 22);
            this.CollectionNameBox.TabIndex = 1;
            // 
            // ColectionNameLabel
            // 
            this.ColectionNameLabel.AutoSize = true;
            this.ColectionNameLabel.Location = new System.Drawing.Point(7, 22);
            this.ColectionNameLabel.Name = "ColectionNameLabel";
            this.ColectionNameLabel.Size = new System.Drawing.Size(45, 17);
            this.ColectionNameLabel.TabIndex = 0;
            this.ColectionNameLabel.Text = "Name";
            // 
            // CollectionListView
            // 
            this.CollectionListView.HideSelection = false;
            this.CollectionListView.Location = new System.Drawing.Point(6, 60);
            this.CollectionListView.MultiSelect = false;
            this.CollectionListView.Name = "CollectionListView";
            this.CollectionListView.Size = new System.Drawing.Size(228, 421);
            this.CollectionListView.TabIndex = 3;
            this.CollectionListView.UseCompatibleStateImageBehavior = false;
            // 
            // CollectionSearchBox
            // 
            this.CollectionSearchBox.Location = new System.Drawing.Point(6, 31);
            this.CollectionSearchBox.Name = "CollectionSearchBox";
            this.CollectionSearchBox.Size = new System.Drawing.Size(228, 22);
            this.CollectionSearchBox.TabIndex = 1;
            // 
            // CollectionSearchLabel
            // 
            this.CollectionSearchLabel.AutoSize = true;
            this.CollectionSearchLabel.Location = new System.Drawing.Point(4, 4);
            this.CollectionSearchLabel.Name = "CollectionSearchLabel";
            this.CollectionSearchLabel.Size = new System.Drawing.Size(53, 17);
            this.CollectionSearchLabel.TabIndex = 0;
            this.CollectionSearchLabel.Text = "Search";
            // 
            // AdvancedActionsButton
            // 
            this.AdvancedActionsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.AdvancedActionsButton.Location = new System.Drawing.Point(1258, 667);
            this.AdvancedActionsButton.Name = "AdvancedActionsButton";
            this.AdvancedActionsButton.Size = new System.Drawing.Size(154, 32);
            this.AdvancedActionsButton.TabIndex = 12;
            this.AdvancedActionsButton.Text = "Advanced";
            this.AdvancedActionsButton.UseVisualStyleBackColor = true;
            this.AdvancedActionsButton.Click += new System.EventHandler(this.AdvancedActions_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1447, 702);
            this.Controls.Add(this.AdvancedActionsButton);
            this.Controls.Add(this.SideTab);
            this.Controls.Add(this.AppSettingsButton);
            this.Controls.Add(this.CreateSymLinksButton);
            this.Controls.Add(this.FilePanel);
            this.Controls.Add(this.Search);
            this.Controls.Add(this.SearchBox);
            this.Controls.Add(this.FileResultHeader);
            this.Controls.Add(this.UpdateButton);
            this.Controls.Add(this.MessageText);
            this.Controls.Add(this.OpenFilePicker);
            this.Name = "MainForm";
            this.Text = "File Organizer";
            this.FilePanel.ResumeLayout(false);
            this.SideTab.ResumeLayout(false);
            this.TagsTab.ResumeLayout(false);
            this.TagsTab.PerformLayout();
            this.AddTagGroup.ResumeLayout(false);
            this.AddTagGroup.PerformLayout();
            this.CollectionsTab.ResumeLayout(false);
            this.CollectionsTab.PerformLayout();
            this.AddCollectionGroup.ResumeLayout(false);
            this.AddCollectionGroup.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button OpenFilePicker;
        private System.Windows.Forms.Label MessageText;
        private System.Windows.Forms.ToolTip MessageTooltip;
        private System.Windows.Forms.Button UpdateButton;
        private System.Windows.Forms.Label FileResultHeader;
        private System.Windows.Forms.TextBox SearchBox;
        private System.Windows.Forms.Button Search;
        private System.Windows.Forms.ListView FileListView;
        private System.Windows.Forms.Panel FilePanel;
        private System.Windows.Forms.Button CreateSymLinksButton;
        private System.Windows.Forms.Button AppSettingsButton;
        private System.Windows.Forms.TabControl SideTab;
        private System.Windows.Forms.TabPage TagsTab;
        private System.Windows.Forms.TabPage CollectionsTab;
        private System.Windows.Forms.Label TagSearchLabel;
        private System.Windows.Forms.TextBox TagSearchBox;
        private System.Windows.Forms.GroupBox AddTagGroup;
        private System.Windows.Forms.ListView TagListView;
        private System.Windows.Forms.Button AssignTagButton;
        private System.Windows.Forms.TextBox NewTagNameBox;
        private System.Windows.Forms.Label NewTagLabel;
        private System.Windows.Forms.Button AddNewTagButton;
        private System.Windows.Forms.ComboBox NewTagCategoryComboBox;
        private System.Windows.Forms.Label NewTagCategoryLabel;
        private System.Windows.Forms.Button RemoveTagButton;
        private System.Windows.Forms.Button AdvancedActionsButton;
        private System.Windows.Forms.Label CollectionSearchLabel;
        private System.Windows.Forms.TextBox CollectionSearchBox;
        private System.Windows.Forms.ListView CollectionListView;
        private System.Windows.Forms.GroupBox AddCollectionGroup;
        private System.Windows.Forms.Button CollectionPickerAddButton;
        private System.Windows.Forms.Button CollectionResultAddButton;
        private System.Windows.Forms.TextBox CollectionNameBox;
        private System.Windows.Forms.Label ColectionNameLabel;
        private System.Windows.Forms.Button CollectionAddFileButton;
        private System.Windows.Forms.Button CollectionSymlinkButton;
        private System.Windows.Forms.Button ShowCollectionFilesButton;
        private System.Windows.Forms.ToolTip CollectionSymlinkTooltip;
    }
}