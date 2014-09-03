namespace MainWindow
{
    partial class MainWinfow
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
            this.label_BaseURL = new System.Windows.Forms.Label();
            this.label_FilePath = new System.Windows.Forms.Label();
            this.textBox_BaseURL = new System.Windows.Forms.TextBox();
            this.textBox_FilePath = new System.Windows.Forms.TextBox();
            this.button_BaseURL = new System.Windows.Forms.Button();
            this.button_FilePath = new System.Windows.Forms.Button();
            this.button_Download = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.richTextBox_prompt = new System.Windows.Forms.RichTextBox();
            this.button1_ReadFile = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label_BaseURL
            // 
            this.label_BaseURL.AutoSize = true;
            this.label_BaseURL.Location = new System.Drawing.Point(48, 35);
            this.label_BaseURL.Name = "label_BaseURL";
            this.label_BaseURL.Size = new System.Drawing.Size(56, 13);
            this.label_BaseURL.TabIndex = 0;
            this.label_BaseURL.Text = "Base URL";
            // 
            // label_FilePath
            // 
            this.label_FilePath.AutoSize = true;
            this.label_FilePath.Location = new System.Drawing.Point(48, 73);
            this.label_FilePath.Name = "label_FilePath";
            this.label_FilePath.Size = new System.Drawing.Size(48, 13);
            this.label_FilePath.TabIndex = 0;
            this.label_FilePath.Text = "File Path";
            // 
            // textBox_BaseURL
            // 
            this.textBox_BaseURL.Location = new System.Drawing.Point(110, 32);
            this.textBox_BaseURL.Name = "textBox_BaseURL";
            this.textBox_BaseURL.Size = new System.Drawing.Size(493, 20);
            this.textBox_BaseURL.TabIndex = 1;
            // 
            // textBox_FilePath
            // 
            this.textBox_FilePath.Location = new System.Drawing.Point(110, 70);
            this.textBox_FilePath.Name = "textBox_FilePath";
            this.textBox_FilePath.Size = new System.Drawing.Size(493, 20);
            this.textBox_FilePath.TabIndex = 2;
            // 
            // button_BaseURL
            // 
            this.button_BaseURL.Location = new System.Drawing.Point(619, 32);
            this.button_BaseURL.Name = "button_BaseURL";
            this.button_BaseURL.Size = new System.Drawing.Size(27, 20);
            this.button_BaseURL.TabIndex = 2;
            this.button_BaseURL.Text = "...";
            this.button_BaseURL.UseVisualStyleBackColor = true;
            this.button_BaseURL.Click += new System.EventHandler(this.button_BaseURL_Click);
            // 
            // button_FilePath
            // 
            this.button_FilePath.Location = new System.Drawing.Point(619, 70);
            this.button_FilePath.Name = "button_FilePath";
            this.button_FilePath.Size = new System.Drawing.Size(27, 20);
            this.button_FilePath.TabIndex = 2;
            this.button_FilePath.Text = "...";
            this.button_FilePath.UseVisualStyleBackColor = true;
            this.button_FilePath.Click += new System.EventHandler(this.button_FilePath_Click);
            // 
            // button_Download
            // 
            this.button_Download.Location = new System.Drawing.Point(146, 438);
            this.button_Download.Name = "button_Download";
            this.button_Download.Size = new System.Drawing.Size(85, 35);
            this.button_Download.TabIndex = 3;
            this.button_Download.Text = "Download";
            this.button_Download.UseVisualStyleBackColor = true;
            this.button_Download.Click += new System.EventHandler(this.button_Download_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Location = new System.Drawing.Point(366, 438);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(85, 35);
            this.button_Cancel.TabIndex = 3;
            this.button_Cancel.Text = "Cancel";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // richTextBox_prompt
            // 
            this.richTextBox_prompt.Location = new System.Drawing.Point(51, 113);
            this.richTextBox_prompt.Name = "richTextBox_prompt";
            this.richTextBox_prompt.ReadOnly = true;
            this.richTextBox_prompt.Size = new System.Drawing.Size(551, 309);
            this.richTextBox_prompt.TabIndex = 4;
            this.richTextBox_prompt.Text = "";
            // 
            // button1_ReadFile
            // 
            this.button1_ReadFile.Location = new System.Drawing.Point(502, 438);
            this.button1_ReadFile.Name = "button1_ReadFile";
            this.button1_ReadFile.Size = new System.Drawing.Size(115, 35);
            this.button1_ReadFile.TabIndex = 5;
            this.button1_ReadFile.Text = "ReadFile";
            this.button1_ReadFile.UseVisualStyleBackColor = true;
            this.button1_ReadFile.Visible = false;
            this.button1_ReadFile.Click += new System.EventHandler(this.button1_ReadFile_Click);
            // 
            // MainWinfow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(697, 485);
            this.Controls.Add(this.button1_ReadFile);
            this.Controls.Add(this.richTextBox_prompt);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_Download);
            this.Controls.Add(this.button_FilePath);
            this.Controls.Add(this.button_BaseURL);
            this.Controls.Add(this.textBox_FilePath);
            this.Controls.Add(this.textBox_BaseURL);
            this.Controls.Add(this.label_FilePath);
            this.Controls.Add(this.label_BaseURL);
            this.Name = "MainWinfow";
            this.ShowIcon = false;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_BaseURL;
        private System.Windows.Forms.Label label_FilePath;
        private System.Windows.Forms.TextBox textBox_BaseURL;
        private System.Windows.Forms.TextBox textBox_FilePath;
        private System.Windows.Forms.Button button_BaseURL;
        private System.Windows.Forms.Button button_FilePath;
        private System.Windows.Forms.Button button_Download;
        private System.Windows.Forms.Button button_Cancel;
        public System.Windows.Forms.RichTextBox richTextBox_prompt;
        private System.Windows.Forms.Button button1_ReadFile;
    }
}

