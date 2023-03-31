
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
            this.FilePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // OpenFilePicker
            // 
            this.OpenFilePicker.Location = new System.Drawing.Point(1082, 591);
            this.OpenFilePicker.Name = "OpenFilePicker";
            this.OpenFilePicker.Size = new System.Drawing.Size(65, 32);
            this.OpenFilePicker.TabIndex = 0;
            this.OpenFilePicker.Text = "Add";
            this.OpenFilePicker.UseVisualStyleBackColor = true;
            this.OpenFilePicker.Click += new System.EventHandler(this.OpenFilePicker_Click);
            // 
            // MessageText
            // 
            this.MessageText.AutoSize = true;
            this.MessageText.Location = new System.Drawing.Point(446, 591);
            this.MessageText.MaximumSize = new System.Drawing.Size(1200, 0);
            this.MessageText.Name = "MessageText";
            this.MessageText.Size = new System.Drawing.Size(0, 17);
            this.MessageText.TabIndex = 1;
            // 
            // UpdateButton
            // 
            this.UpdateButton.Enabled = false;
            this.UpdateButton.Location = new System.Drawing.Point(1165, 591);
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
            this.FileResultHeader.Location = new System.Drawing.Point(247, 16);
            this.FileResultHeader.Name = "FileResultHeader";
            this.FileResultHeader.Size = new System.Drawing.Size(135, 32);
            this.FileResultHeader.TabIndex = 4;
            this.FileResultHeader.Text = "File Results";
            // 
            // SearchBox
            // 
            this.SearchBox.Location = new System.Drawing.Point(464, 22);
            this.SearchBox.Name = "SearchBox";
            this.SearchBox.Size = new System.Drawing.Size(683, 22);
            this.SearchBox.TabIndex = 5;
            // 
            // Search
            // 
            this.Search.Location = new System.Drawing.Point(1168, 20);
            this.Search.Name = "Search";
            this.Search.Size = new System.Drawing.Size(81, 24);
            this.Search.TabIndex = 6;
            this.Search.Text = "Search";
            this.Search.UseVisualStyleBackColor = true;
            this.Search.Click += new System.EventHandler(this.Search_Click);
            // 
            // FileListView
            // 
            this.FileListView.HideSelection = false;
            this.FileListView.Location = new System.Drawing.Point(3, 3);
            this.FileListView.MaximumSize = new System.Drawing.Size(995, -1);
            this.FileListView.MinimumSize = new System.Drawing.Size(995, 530);
            this.FileListView.Name = "FileListView";
            this.FileListView.Size = new System.Drawing.Size(995, 530);
            this.FileListView.TabIndex = 7;
            this.FileListView.UseCompatibleStateImageBehavior = false;
            // 
            // FilePanel
            // 
            this.FilePanel.Controls.Add(this.FileListView);
            this.FilePanel.Location = new System.Drawing.Point(246, 51);
            this.FilePanel.Name = "FilePanel";
            this.FilePanel.Size = new System.Drawing.Size(1003, 534);
            this.FilePanel.TabIndex = 8;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1320, 656);
            this.Controls.Add(this.FilePanel);
            this.Controls.Add(this.Search);
            this.Controls.Add(this.SearchBox);
            this.Controls.Add(this.FileResultHeader);
            this.Controls.Add(this.UpdateButton);
            this.Controls.Add(this.MessageText);
            this.Controls.Add(this.OpenFilePicker);
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.FilePanel.ResumeLayout(false);
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
    }
}