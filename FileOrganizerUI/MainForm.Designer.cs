﻿
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
            this.SuspendLayout();
            // 
            // OpenFilePicker
            // 
            this.OpenFilePicker.Location = new System.Drawing.Point(1094, 591);
            this.OpenFilePicker.Name = "OpenFilePicker";
            this.OpenFilePicker.Size = new System.Drawing.Size(183, 32);
            this.OpenFilePicker.TabIndex = 0;
            this.OpenFilePicker.Text = "Open File Picker";
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
            // MessageTooltip
            // 
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1320, 656);
            this.Controls.Add(this.MessageText);
            this.Controls.Add(this.OpenFilePicker);
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button OpenFilePicker;
        private System.Windows.Forms.Label MessageText;
        private System.Windows.Forms.ToolTip MessageTooltip;
    }
}