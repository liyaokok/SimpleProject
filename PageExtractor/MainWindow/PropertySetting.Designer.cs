namespace MainWindow
{
    partial class PropertySettingWindow
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
            this.label_MAxDepth = new System.Windows.Forms.Label();
            this.label_MaxConnection = new System.Windows.Forms.Label();
            this.textBox_MaxDepth = new System.Windows.Forms.TextBox();
            this.textBox_MaxConnection = new System.Windows.Forms.TextBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label_MAxDepth
            // 
            this.label_MAxDepth.AutoSize = true;
            this.label_MAxDepth.Location = new System.Drawing.Point(12, 24);
            this.label_MAxDepth.Name = "label_MAxDepth";
            this.label_MAxDepth.Size = new System.Drawing.Size(59, 13);
            this.label_MAxDepth.TabIndex = 0;
            this.label_MAxDepth.Text = "Max Depth";
            // 
            // label_MaxConnection
            // 
            this.label_MaxConnection.AutoSize = true;
            this.label_MaxConnection.Location = new System.Drawing.Point(12, 60);
            this.label_MaxConnection.Name = "label_MaxConnection";
            this.label_MaxConnection.Size = new System.Drawing.Size(84, 13);
            this.label_MaxConnection.TabIndex = 0;
            this.label_MaxConnection.Text = "Max Connection";
            // 
            // textBox_MaxDepth
            // 
            this.textBox_MaxDepth.Location = new System.Drawing.Point(131, 21);
            this.textBox_MaxDepth.Name = "textBox_MaxDepth";
            this.textBox_MaxDepth.Size = new System.Drawing.Size(115, 20);
            this.textBox_MaxDepth.TabIndex = 1;
            // 
            // textBox_MaxConnection
            // 
            this.textBox_MaxConnection.Location = new System.Drawing.Point(131, 57);
            this.textBox_MaxConnection.Name = "textBox_MaxConnection";
            this.textBox_MaxConnection.Size = new System.Drawing.Size(115, 20);
            this.textBox_MaxConnection.TabIndex = 2;
            // 
            // button_OK
            // 
            this.button_OK.Location = new System.Drawing.Point(28, 245);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(81, 31);
            this.button_OK.TabIndex = 3;
            this.button_OK.Text = "OK";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Location = new System.Drawing.Point(155, 245);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(81, 31);
            this.button_Cancel.TabIndex = 3;
            this.button_Cancel.Text = "Cancel";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // PropertySettingWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(269, 288);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_MaxConnection);
            this.Controls.Add(this.textBox_MaxDepth);
            this.Controls.Add(this.label_MaxConnection);
            this.Controls.Add(this.label_MAxDepth);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PropertySettingWindow";
            this.ShowIcon = false;
            this.Text = "Property Setting";
            this.Load += new System.EventHandler(this.PropertySettingWindow_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_MAxDepth;
        private System.Windows.Forms.Label label_MaxConnection;
        private System.Windows.Forms.TextBox textBox_MaxDepth;
        private System.Windows.Forms.TextBox textBox_MaxConnection;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
    }
}