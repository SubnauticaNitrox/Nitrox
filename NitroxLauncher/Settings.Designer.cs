namespace NitroxLauncher
{
    partial class Settings
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
            if (disposing && (components != null))
            {
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
            this.FolderLabel = new System.Windows.Forms.Label();
            this.FolderText = new System.Windows.Forms.Label();
            this.ChangeFolder = new System.Windows.Forms.Button();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.Save = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // FolderLabel
            // 
            this.FolderLabel.AutoSize = true;
            this.FolderLabel.Location = new System.Drawing.Point(12, 9);
            this.FolderLabel.Name = "FolderLabel";
            this.FolderLabel.Size = new System.Drawing.Size(96, 13);
            this.FolderLabel.TabIndex = 0;
            this.FolderLabel.Text = "Subnautica Folder:";
            // 
            // FolderText
            // 
            this.FolderText.AutoSize = true;
            this.FolderText.Location = new System.Drawing.Point(12, 32);
            this.FolderText.Name = "FolderText";
            this.FolderText.Size = new System.Drawing.Size(0, 13);
            this.FolderText.TabIndex = 2;
            // 
            // ChangeFolder
            // 
            this.ChangeFolder.Location = new System.Drawing.Point(114, 4);
            this.ChangeFolder.Name = "ChangeFolder";
            this.ChangeFolder.Size = new System.Drawing.Size(75, 23);
            this.ChangeFolder.TabIndex = 3;
            this.ChangeFolder.Text = "Change";
            this.ChangeFolder.UseVisualStyleBackColor = true;
            this.ChangeFolder.Click += new System.EventHandler(this.ChangeFolder_Click);
            // 
            // Save
            // 
            this.Save.Location = new System.Drawing.Point(165, 114);
            this.Save.Name = "Save";
            this.Save.Size = new System.Drawing.Size(75, 23);
            this.Save.TabIndex = 4;
            this.Save.Text = "Save";
            this.Save.UseVisualStyleBackColor = true;
            this.Save.Click += new System.EventHandler(this.Save_Click);
            // 
            // Cancel
            // 
            this.Cancel.Location = new System.Drawing.Point(276, 114);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 23);
            this.Cancel.TabIndex = 5;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(563, 149);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.Save);
            this.Controls.Add(this.ChangeFolder);
            this.Controls.Add(this.FolderText);
            this.Controls.Add(this.FolderLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Settings";
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.Settings_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label FolderLabel;
        private System.Windows.Forms.Label FolderText;
        private System.Windows.Forms.Button ChangeFolder;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.Button Save;
        private System.Windows.Forms.Button Cancel;
    }
}
