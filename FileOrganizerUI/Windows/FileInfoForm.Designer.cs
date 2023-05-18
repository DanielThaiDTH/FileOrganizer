
namespace FileOrganizerUI.Windows
{
    partial class FileInfoForm
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
            if (disposing && editDispose != null) {
                editDispose.Dispose();
            }
            if (disposing && (components != null)) {
                components.Dispose();
            }
            if (disposing) {
                detailLines.Clear();
                fileInfo = null;
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
            this.MainVPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.RightPanel = new System.Windows.Forms.Panel();
            this.OpenFileButton = new System.Windows.Forms.Button();
            this.RightPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainVPanel
            // 
            this.MainVPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.MainVPanel.AutoScroll = true;
            this.MainVPanel.AutoScrollMinSize = new System.Drawing.Size(0, 475);
            this.MainVPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.MainVPanel.Location = new System.Drawing.Point(13, 13);
            this.MainVPanel.Name = "MainVPanel";
            this.MainVPanel.Size = new System.Drawing.Size(718, 475);
            this.MainVPanel.TabIndex = 0;
            this.MainVPanel.WrapContents = false;
            // 
            // RightPanel
            // 
            this.RightPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RightPanel.AutoScroll = true;
            this.RightPanel.Controls.Add(this.OpenFileButton);
            this.RightPanel.Location = new System.Drawing.Point(738, 13);
            this.RightPanel.Name = "RightPanel";
            this.RightPanel.Size = new System.Drawing.Size(696, 482);
            this.RightPanel.TabIndex = 1;
            // 
            // OpenFileButton
            // 
            this.OpenFileButton.Location = new System.Drawing.Point(4, 4);
            this.OpenFileButton.Name = "OpenFileButton";
            this.OpenFileButton.Size = new System.Drawing.Size(75, 23);
            this.OpenFileButton.TabIndex = 0;
            this.OpenFileButton.Text = "Open";
            this.OpenFileButton.UseVisualStyleBackColor = true;
            // 
            // FileInfoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1441, 500);
            this.Controls.Add(this.RightPanel);
            this.Controls.Add(this.MainVPanel);
            this.Name = "FileInfoForm";
            this.Text = "FileInfoForm";
            this.RightPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel MainVPanel;
        private System.Windows.Forms.Panel RightPanel;
        private System.Windows.Forms.Button OpenFileButton;
    }
}