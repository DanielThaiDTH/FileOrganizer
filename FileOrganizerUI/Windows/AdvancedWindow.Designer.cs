
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
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.CategoriesTab = new System.Windows.Forms.TabPage();
            this.AdvancedTabs = new System.Windows.Forms.TabControl();
            this.layoutCategories = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.CatagorySelectLabel = new System.Windows.Forms.Label();
            this.CategoryComboBox = new System.Windows.Forms.ComboBox();
            this.CategoryDeleteButton = new System.Windows.Forms.Button();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.RenameLabel = new System.Windows.Forms.Label();
            this.CategoryRenameBox = new System.Windows.Forms.TextBox();
            this.RenameCategoryButton = new System.Windows.Forms.Button();
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.DescriptionLabel = new System.Windows.Forms.Label();
            this.DoneButton = new System.Windows.Forms.Button();
            this.MessageLabel = new System.Windows.Forms.Label();
            this.AddCateboryBox = new System.Windows.Forms.TextBox();
            this.AddButton = new System.Windows.Forms.Button();
            this.CategoriesTab.SuspendLayout();
            this.AdvancedTabs.SuspendLayout();
            this.layoutCategories.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(791, 433);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Other";
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
            // AdvancedTabs
            // 
            this.AdvancedTabs.Controls.Add(this.CategoriesTab);
            this.AdvancedTabs.Controls.Add(this.tabPage2);
            this.AdvancedTabs.Location = new System.Drawing.Point(13, 13);
            this.AdvancedTabs.Name = "AdvancedTabs";
            this.AdvancedTabs.SelectedIndex = 0;
            this.AdvancedTabs.Size = new System.Drawing.Size(799, 462);
            this.AdvancedTabs.TabIndex = 0;
            // 
            // layoutCategories
            // 
            this.layoutCategories.Controls.Add(this.flowLayoutPanel1);
            this.layoutCategories.Controls.Add(this.flowLayoutPanel2);
            this.layoutCategories.Controls.Add(this.flowLayoutPanel3);
            this.layoutCategories.Location = new System.Drawing.Point(4, 4);
            this.layoutCategories.Name = "layoutCategories";
            this.layoutCategories.Size = new System.Drawing.Size(784, 426);
            this.layoutCategories.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.CatagorySelectLabel);
            this.flowLayoutPanel1.Controls.Add(this.CategoryComboBox);
            this.flowLayoutPanel1.Controls.Add(this.CategoryDeleteButton);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(650, 31);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // CatagorySelectLabel
            // 
            this.CatagorySelectLabel.AutoSize = true;
            this.CatagorySelectLabel.Location = new System.Drawing.Point(3, 6);
            this.CatagorySelectLabel.Margin = new System.Windows.Forms.Padding(3, 6, 45, 0);
            this.CatagorySelectLabel.Name = "CatagorySelectLabel";
            this.CatagorySelectLabel.Size = new System.Drawing.Size(94, 17);
            this.CatagorySelectLabel.TabIndex = 0;
            this.CatagorySelectLabel.Text = "Tag Category";
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
            this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 40);
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
            this.flowLayoutPanel3.Location = new System.Drawing.Point(3, 75);
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
            // DoneButton
            // 
            this.DoneButton.Location = new System.Drawing.Point(729, 482);
            this.DoneButton.Name = "DoneButton";
            this.DoneButton.Size = new System.Drawing.Size(75, 23);
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
            // AdvancedWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(824, 517);
            this.Controls.Add(this.MessageLabel);
            this.Controls.Add(this.DoneButton);
            this.Controls.Add(this.AdvancedTabs);
            this.Name = "AdvancedWindow";
            this.Text = "Advanced";
            this.CategoriesTab.ResumeLayout(false);
            this.AdvancedTabs.ResumeLayout(false);
            this.layoutCategories.ResumeLayout(false);
            this.layoutCategories.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.flowLayoutPanel3.ResumeLayout(false);
            this.flowLayoutPanel3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage CategoriesTab;
        private System.Windows.Forms.FlowLayoutPanel layoutCategories;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label CatagorySelectLabel;
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
    }
}