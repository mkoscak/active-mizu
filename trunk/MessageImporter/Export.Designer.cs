namespace MessageImporter
{
    partial class Export
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rbSelected = new System.Windows.Forms.RadioButton();
            this.rbNonSelected = new System.Windows.Forms.RadioButton();
            this.chbRemoveAfterExport = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtFileName = new System.Windows.Forms.TextBox();
            this.btnExport = new System.Windows.Forms.Button();
            this.rbAll = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.rbAll);
            this.groupBox1.Controls.Add(this.txtFileName);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.rbNonSelected);
            this.groupBox1.Controls.Add(this.rbSelected);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(379, 139);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Parameters";
            // 
            // rbSelected
            // 
            this.rbSelected.AutoSize = true;
            this.rbSelected.Location = new System.Drawing.Point(6, 19);
            this.rbSelected.Name = "rbSelected";
            this.rbSelected.Size = new System.Drawing.Size(105, 17);
            this.rbSelected.TabIndex = 0;
            this.rbSelected.Text = "Selected (paired)";
            this.rbSelected.UseVisualStyleBackColor = true;
            // 
            // rbNonSelected
            // 
            this.rbNonSelected.AutoSize = true;
            this.rbNonSelected.Checked = true;
            this.rbNonSelected.Location = new System.Drawing.Point(6, 42);
            this.rbNonSelected.Name = "rbNonSelected";
            this.rbNonSelected.Size = new System.Drawing.Size(129, 17);
            this.rbNonSelected.TabIndex = 1;
            this.rbNonSelected.TabStop = true;
            this.rbNonSelected.Text = "Deselected (unpaired)";
            this.rbNonSelected.UseVisualStyleBackColor = true;
            // 
            // chbRemoveAfterExport
            // 
            this.chbRemoveAfterExport.AutoSize = true;
            this.chbRemoveAfterExport.Location = new System.Drawing.Point(185, 31);
            this.chbRemoveAfterExport.Name = "chbRemoveAfterExport";
            this.chbRemoveAfterExport.Size = new System.Drawing.Size(187, 17);
            this.chbRemoveAfterExport.TabIndex = 1;
            this.chbRemoveAfterExport.Text = "Remove items from list after export";
            this.chbRemoveAfterExport.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 116);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "File name";
            // 
            // txtFileName
            // 
            this.txtFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFileName.Location = new System.Drawing.Point(64, 113);
            this.txtFileName.Name = "txtFileName";
            this.txtFileName.Size = new System.Drawing.Size(296, 20);
            this.txtFileName.TabIndex = 3;
            this.txtFileName.Text = "filename.csv";
            // 
            // btnExport
            // 
            this.btnExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExport.Location = new System.Drawing.Point(293, 157);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(98, 23);
            this.btnExport.TabIndex = 2;
            this.btnExport.Text = "Export";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // rbAll
            // 
            this.rbAll.AutoSize = true;
            this.rbAll.Location = new System.Drawing.Point(6, 65);
            this.rbAll.Name = "rbAll";
            this.rbAll.Size = new System.Drawing.Size(36, 17);
            this.rbAll.TabIndex = 4;
            this.rbAll.Text = "All";
            this.rbAll.UseVisualStyleBackColor = true;
            // 
            // Export
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(403, 192);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.chbRemoveAfterExport);
            this.Controls.Add(this.groupBox1);
            this.Name = "Export";
            this.Text = "Export items";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rbNonSelected;
        private System.Windows.Forms.RadioButton rbSelected;
        private System.Windows.Forms.CheckBox chbRemoveAfterExport;
        private System.Windows.Forms.TextBox txtFileName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.RadioButton rbAll;
    }
}