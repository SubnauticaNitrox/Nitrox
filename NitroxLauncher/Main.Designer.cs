namespace NitroxLauncher
{
    partial class Main
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.SinglePlayerButton = new System.Windows.Forms.Button();
            this.MultiplayerButton = new System.Windows.Forms.Button();
            this.ServerButton = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.QuitButton = new System.Windows.Forms.Button();
            this.SettingsButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // SinglePlayerButton
            // 
            this.SinglePlayerButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.SinglePlayerButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SinglePlayerButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.SinglePlayerButton.Image = ((System.Drawing.Image)(resources.GetObject("SinglePlayerButton.Image")));
            this.SinglePlayerButton.Location = new System.Drawing.Point(375, 440);
            this.SinglePlayerButton.Name = "SinglePlayerButton";
            this.SinglePlayerButton.Size = new System.Drawing.Size(460, 69);
            this.SinglePlayerButton.TabIndex = 0;
            this.SinglePlayerButton.UseVisualStyleBackColor = true;
            this.SinglePlayerButton.Click += new System.EventHandler(this.SinglePlayerButton_Click);
            // 
            // MultiplayerButton
            // 
            this.MultiplayerButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.MultiplayerButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MultiplayerButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MultiplayerButton.Image = ((System.Drawing.Image)(resources.GetObject("MultiplayerButton.Image")));
            this.MultiplayerButton.Location = new System.Drawing.Point(375, 549);
            this.MultiplayerButton.Name = "MultiplayerButton";
            this.MultiplayerButton.Size = new System.Drawing.Size(460, 69);
            this.MultiplayerButton.TabIndex = 1;
            this.MultiplayerButton.UseVisualStyleBackColor = true;
            this.MultiplayerButton.Click += new System.EventHandler(this.MultiplayerButton_Click);
            // 
            // ServerButton
            // 
            this.ServerButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ServerButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ServerButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ServerButton.Image = ((System.Drawing.Image)(resources.GetObject("ServerButton.Image")));
            this.ServerButton.Location = new System.Drawing.Point(613, 646);
            this.ServerButton.Name = "ServerButton";
            this.ServerButton.Size = new System.Drawing.Size(221, 54);
            this.ServerButton.TabIndex = 2;
            this.ServerButton.UseVisualStyleBackColor = true;
            this.ServerButton.Click += new System.EventHandler(this.ServerButton_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.BackgroundImage")));
            this.pictureBox1.Location = new System.Drawing.Point(253, 125);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(723, 282);
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // QuitButton
            // 
            this.QuitButton.BackColor = System.Drawing.Color.Transparent;
            this.QuitButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.QuitButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.QuitButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.QuitButton.Image = ((System.Drawing.Image)(resources.GetObject("QuitButton.Image")));
            this.QuitButton.Location = new System.Drawing.Point(1157, 26);
            this.QuitButton.Name = "QuitButton";
            this.QuitButton.Size = new System.Drawing.Size(46, 46);
            this.QuitButton.TabIndex = 4;
            this.QuitButton.UseVisualStyleBackColor = false;
            this.QuitButton.Click += new System.EventHandler(this.QuitButton_Click);
            // 
            // SettingsButton
            // 
            this.SettingsButton.BackColor = System.Drawing.Color.Transparent;
            this.SettingsButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.SettingsButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SettingsButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.SettingsButton.Image = ((System.Drawing.Image)(resources.GetObject("SettingsButton.Image")));
            this.SettingsButton.Location = new System.Drawing.Point(1105, 26);
            this.SettingsButton.Name = "SettingsButton";
            this.SettingsButton.Size = new System.Drawing.Size(46, 46);
            this.SettingsButton.TabIndex = 5;
            this.SettingsButton.UseVisualStyleBackColor = false;
            this.SettingsButton.Click += new System.EventHandler(this.SettingsButton_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(1215, 775);
            this.Controls.Add(this.SettingsButton);
            this.Controls.Add(this.QuitButton);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.ServerButton);
            this.Controls.Add(this.MultiplayerButton);
            this.Controls.Add(this.SinglePlayerButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Main";
            this.Load += new System.EventHandler(this.Main_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button SinglePlayerButton;
        private System.Windows.Forms.Button MultiplayerButton;
        private System.Windows.Forms.Button ServerButton;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button QuitButton;
        private System.Windows.Forms.Button SettingsButton;
    }
}

