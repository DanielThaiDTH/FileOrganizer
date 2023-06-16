
namespace FileOrganizerUI.Windows
{
    partial class AddFileErrorDialog
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
            this.ErrorLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // ErrorLayoutPanel
            // 
            this.ErrorLayoutPanel.AutoScroll = true;
            this.ErrorLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.ErrorLayoutPanel.Location = new System.Drawing.Point(13, 13);
            this.ErrorLayoutPanel.Name = "ErrorLayoutPanel";
            this.ErrorLayoutPanel.Size = new System.Drawing.Size(824, 491);
            this.ErrorLayoutPanel.TabIndex = 0;
            // 
            // AddFileErrorDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(849, 527);
            this.Controls.Add(this.ErrorLayoutPanel);
            this.Name = "AddFileErrorDialog";
            this.Text = "Add File Errors";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel ErrorLayoutPanel;
    }
}