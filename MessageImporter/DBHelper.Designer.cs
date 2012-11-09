namespace MessageImporter
{
    partial class DBHelper
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DBHelper));
            this.gridDBres = new System.Windows.Forms.DataGridView();
            this.txtQuery = new System.Windows.Forms.TextBox();
            this.txtNonQueryRes = new System.Windows.Forms.TextBox();
            this.btnExecQuery = new System.Windows.Forms.Button();
            this.btnExecNonQuery = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.gridDBres)).BeginInit();
            this.SuspendLayout();
            // 
            // gridDBres
            // 
            this.gridDBres.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.gridDBres.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridDBres.Location = new System.Drawing.Point(12, 233);
            this.gridDBres.Name = "gridDBres";
            this.gridDBres.Size = new System.Drawing.Size(1059, 526);
            this.gridDBres.TabIndex = 0;
            // 
            // txtQuery
            // 
            this.txtQuery.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.txtQuery.Location = new System.Drawing.Point(143, 12);
            this.txtQuery.Multiline = true;
            this.txtQuery.Name = "txtQuery";
            this.txtQuery.Size = new System.Drawing.Size(432, 215);
            this.txtQuery.TabIndex = 1;
            // 
            // txtNonQueryRes
            // 
            this.txtNonQueryRes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNonQueryRes.ForeColor = System.Drawing.Color.DarkRed;
            this.txtNonQueryRes.Location = new System.Drawing.Point(581, 12);
            this.txtNonQueryRes.Multiline = true;
            this.txtNonQueryRes.Name = "txtNonQueryRes";
            this.txtNonQueryRes.Size = new System.Drawing.Size(490, 215);
            this.txtNonQueryRes.TabIndex = 2;
            // 
            // btnExecQuery
            // 
            this.btnExecQuery.Location = new System.Drawing.Point(12, 12);
            this.btnExecQuery.Name = "btnExecQuery";
            this.btnExecQuery.Size = new System.Drawing.Size(125, 33);
            this.btnExecQuery.TabIndex = 3;
            this.btnExecQuery.Text = "Execute query";
            this.btnExecQuery.UseVisualStyleBackColor = true;
            this.btnExecQuery.Click += new System.EventHandler(this.btnExecQuery_Click);
            // 
            // btnExecNonQuery
            // 
            this.btnExecNonQuery.Location = new System.Drawing.Point(12, 51);
            this.btnExecNonQuery.Name = "btnExecNonQuery";
            this.btnExecNonQuery.Size = new System.Drawing.Size(125, 33);
            this.btnExecNonQuery.TabIndex = 4;
            this.btnExecNonQuery.Text = "Execute NONquery";
            this.btnExecNonQuery.UseVisualStyleBackColor = true;
            this.btnExecNonQuery.Click += new System.EventHandler(this.btnExecNonQuery_Click);
            // 
            // DBHelper
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1083, 771);
            this.Controls.Add(this.btnExecNonQuery);
            this.Controls.Add(this.btnExecQuery);
            this.Controls.Add(this.txtNonQueryRes);
            this.Controls.Add(this.txtQuery);
            this.Controls.Add(this.gridDBres);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DBHelper";
            this.Text = "DBHelper";
            ((System.ComponentModel.ISupportInitialize)(this.gridDBres)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView gridDBres;
        private System.Windows.Forms.TextBox txtQuery;
        private System.Windows.Forms.TextBox txtNonQueryRes;
        private System.Windows.Forms.Button btnExecQuery;
        private System.Windows.Forms.Button btnExecNonQuery;
    }
}