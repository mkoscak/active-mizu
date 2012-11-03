namespace MessageImporter
{
    partial class FrmActiveStyle
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmActiveStyle));
            this.btnRead = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.txtInputPath = new System.Windows.Forms.TextBox();
            this.btnChoose = new System.Windows.Forms.Button();
            this.btn2XML = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.btnClear = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.btnOutDir = new System.Windows.Forms.Button();
            this.txtOutDir = new System.Windows.Forms.TextBox();
            this.chkMoveProcessed = new System.Windows.Forms.CheckBox();
            this.grpInputSettings = new System.Windows.Forms.GroupBox();
            this.btnProcess = new System.Windows.Forms.Button();
            this.btnSelectAll = new System.Windows.Forms.Button();
            this.btnInverse = new System.Windows.Forms.Button();
            this.btnDeselectAll = new System.Windows.Forms.Button();
            this.tabData = new System.Windows.Forms.TabControl();
            this.invoice = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btnInvoiceRemove = new System.Windows.Forms.Button();
            this.btnInvoiceAdd = new System.Windows.Forms.Button();
            this.dataCSV = new MessageImporter.CustomDataGridView();
            this.tabItems = new System.Windows.Forms.TabControl();
            this.tabAllItems = new System.Windows.Forms.TabPage();
            this.lbNonPaired = new System.Windows.Forms.ListBox();
            this.tabSelItems = new System.Windows.Forms.TabPage();
            this.lbFilteredItems = new System.Windows.Forms.ListBox();
            this.lblUnpiredCount = new System.Windows.Forms.Label();
            this.btnUnpairAll = new System.Windows.Forms.Button();
            this.btnUnpairInvoiceItem = new System.Windows.Forms.Button();
            this.btnInvoiceItemRemove = new System.Windows.Forms.Button();
            this.btnAssignProd = new System.Windows.Forms.Button();
            this.btnInvoiceItemNew = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.dataGridInvItems = new MessageImporter.CustomDataGridView();
            this.stock = new System.Windows.Forms.TabPage();
            this.btnExportMSG = new System.Windows.Forms.Button();
            this.btnUnpairAllMSG = new System.Windows.Forms.Button();
            this.btnUnpairProductMSG = new System.Windows.Forms.Button();
            this.btnRemoveMSG = new System.Windows.Forms.Button();
            this.btnAddMsg = new System.Windows.Forms.Button();
            this.dataGrid = new MessageImporter.CustomDataGridView();
            this.tabFilesLog = new System.Windows.Forms.TabControl();
            this.tabFoundFiles = new System.Windows.Forms.TabPage();
            this.dataFiles = new MessageImporter.CustomDataGridView();
            this.tabLog = new System.Windows.Forms.TabPage();
            this.grpInputSettings.SuspendLayout();
            this.tabData.SuspendLayout();
            this.invoice.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataCSV)).BeginInit();
            this.tabItems.SuspendLayout();
            this.tabAllItems.SuspendLayout();
            this.tabSelItems.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridInvItems)).BeginInit();
            this.stock.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).BeginInit();
            this.tabFilesLog.SuspendLayout();
            this.tabFoundFiles.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataFiles)).BeginInit();
            this.tabLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnRead
            // 
            this.btnRead.Location = new System.Drawing.Point(12, 122);
            this.btnRead.Name = "btnRead";
            this.btnRead.Size = new System.Drawing.Size(157, 46);
            this.btnRead.TabIndex = 0;
            this.btnRead.Text = "&1. Read input directory";
            this.btnRead.UseVisualStyleBackColor = true;
            this.btnRead.Click += new System.EventHandler(this.btnRead_Click);
            // 
            // txtLog
            // 
            this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLog.Location = new System.Drawing.Point(0, 0);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(554, 409);
            this.txtLog.TabIndex = 1;
            this.txtLog.WordWrap = false;
            // 
            // txtInputPath
            // 
            this.txtInputPath.Location = new System.Drawing.Point(102, 19);
            this.txtInputPath.Name = "txtInputPath";
            this.txtInputPath.Size = new System.Drawing.Size(397, 20);
            this.txtInputPath.TabIndex = 6;
            // 
            // btnChoose
            // 
            this.btnChoose.Location = new System.Drawing.Point(505, 19);
            this.btnChoose.Name = "btnChoose";
            this.btnChoose.Size = new System.Drawing.Size(50, 21);
            this.btnChoose.TabIndex = 7;
            this.btnChoose.Text = "...";
            this.btnChoose.UseVisualStyleBackColor = true;
            this.btnChoose.Click += new System.EventHandler(this.btnChoose_Click_1);
            // 
            // btn2XML
            // 
            this.btn2XML.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn2XML.Location = new System.Drawing.Point(1314, 643);
            this.btn2XML.Name = "btn2XML";
            this.btn2XML.Size = new System.Drawing.Size(114, 35);
            this.btn2XML.TabIndex = 8;
            this.btn2XML.Text = "&3. Write to XML";
            this.btn2XML.UseVisualStyleBackColor = true;
            this.btn2XML.Click += new System.EventHandler(this.btn2XML_Click_1);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Input directory";
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnClear.Location = new System.Drawing.Point(0, 414);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(113, 23);
            this.btnClear.TabIndex = 12;
            this.btnClear.Text = "Clear log";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click_1);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "Output directory";
            // 
            // btnOutDir
            // 
            this.btnOutDir.Location = new System.Drawing.Point(505, 46);
            this.btnOutDir.Name = "btnOutDir";
            this.btnOutDir.Size = new System.Drawing.Size(50, 20);
            this.btnOutDir.TabIndex = 14;
            this.btnOutDir.Text = "...";
            this.btnOutDir.UseVisualStyleBackColor = true;
            this.btnOutDir.Click += new System.EventHandler(this.btnOutDir_Click_1);
            // 
            // txtOutDir
            // 
            this.txtOutDir.Location = new System.Drawing.Point(102, 46);
            this.txtOutDir.Name = "txtOutDir";
            this.txtOutDir.Size = new System.Drawing.Size(397, 20);
            this.txtOutDir.TabIndex = 13;
            // 
            // chkMoveProcessed
            // 
            this.chkMoveProcessed.AutoSize = true;
            this.chkMoveProcessed.Location = new System.Drawing.Point(15, 75);
            this.chkMoveProcessed.Name = "chkMoveProcessed";
            this.chkMoveProcessed.Size = new System.Drawing.Size(237, 17);
            this.chkMoveProcessed.TabIndex = 16;
            this.chkMoveProcessed.Text = "Move processed files to \'processed\' directory";
            this.chkMoveProcessed.UseVisualStyleBackColor = true;
            // 
            // grpInputSettings
            // 
            this.grpInputSettings.Controls.Add(this.txtInputPath);
            this.grpInputSettings.Controls.Add(this.chkMoveProcessed);
            this.grpInputSettings.Controls.Add(this.btnChoose);
            this.grpInputSettings.Controls.Add(this.label2);
            this.grpInputSettings.Controls.Add(this.label1);
            this.grpInputSettings.Controls.Add(this.btnOutDir);
            this.grpInputSettings.Controls.Add(this.txtOutDir);
            this.grpInputSettings.Location = new System.Drawing.Point(12, 12);
            this.grpInputSettings.Name = "grpInputSettings";
            this.grpInputSettings.Size = new System.Drawing.Size(562, 104);
            this.grpInputSettings.TabIndex = 17;
            this.grpInputSettings.TabStop = false;
            this.grpInputSettings.Text = "Input settings";
            // 
            // btnProcess
            // 
            this.btnProcess.Location = new System.Drawing.Point(175, 122);
            this.btnProcess.Name = "btnProcess";
            this.btnProcess.Size = new System.Drawing.Size(157, 46);
            this.btnProcess.TabIndex = 19;
            this.btnProcess.Text = "&2. Process selected files";
            this.btnProcess.UseVisualStyleBackColor = true;
            this.btnProcess.Click += new System.EventHandler(this.btnProcess_Click);
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSelectAll.Location = new System.Drawing.Point(0, 409);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(87, 23);
            this.btnSelectAll.TabIndex = 20;
            this.btnSelectAll.Text = "Select all";
            this.btnSelectAll.UseVisualStyleBackColor = true;
            this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
            // 
            // btnInverse
            // 
            this.btnInverse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnInverse.Location = new System.Drawing.Point(94, 409);
            this.btnInverse.Name = "btnInverse";
            this.btnInverse.Size = new System.Drawing.Size(87, 23);
            this.btnInverse.TabIndex = 21;
            this.btnInverse.Text = "Invert selection";
            this.btnInverse.UseVisualStyleBackColor = true;
            this.btnInverse.Click += new System.EventHandler(this.btnInverse_Click);
            // 
            // btnDeselectAll
            // 
            this.btnDeselectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDeselectAll.Location = new System.Drawing.Point(188, 409);
            this.btnDeselectAll.Name = "btnDeselectAll";
            this.btnDeselectAll.Size = new System.Drawing.Size(87, 23);
            this.btnDeselectAll.TabIndex = 22;
            this.btnDeselectAll.Text = "Deselect all";
            this.btnDeselectAll.UseVisualStyleBackColor = true;
            this.btnDeselectAll.Click += new System.EventHandler(this.btnDeselectAll_Click);
            // 
            // tabData
            // 
            this.tabData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabData.Controls.Add(this.invoice);
            this.tabData.Controls.Add(this.stock);
            this.tabData.Location = new System.Drawing.Point(580, 12);
            this.tabData.Name = "tabData";
            this.tabData.SelectedIndex = 0;
            this.tabData.Size = new System.Drawing.Size(848, 625);
            this.tabData.TabIndex = 28;
            this.tabData.SelectedIndexChanged += new System.EventHandler(this.TabChanged);
            // 
            // invoice
            // 
            this.invoice.Controls.Add(this.splitContainer1);
            this.invoice.Location = new System.Drawing.Point(4, 22);
            this.invoice.Name = "invoice";
            this.invoice.Padding = new System.Windows.Forms.Padding(3);
            this.invoice.Size = new System.Drawing.Size(840, 599);
            this.invoice.TabIndex = 0;
            this.invoice.Text = "Invoice";
            this.invoice.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.btnInvoiceRemove);
            this.splitContainer1.Panel1.Controls.Add(this.btnInvoiceAdd);
            this.splitContainer1.Panel1.Controls.Add(this.dataCSV);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabItems);
            this.splitContainer1.Panel2.Controls.Add(this.lblUnpiredCount);
            this.splitContainer1.Panel2.Controls.Add(this.btnUnpairAll);
            this.splitContainer1.Panel2.Controls.Add(this.btnUnpairInvoiceItem);
            this.splitContainer1.Panel2.Controls.Add(this.btnInvoiceItemRemove);
            this.splitContainer1.Panel2.Controls.Add(this.btnAssignProd);
            this.splitContainer1.Panel2.Controls.Add(this.btnInvoiceItemNew);
            this.splitContainer1.Panel2.Controls.Add(this.label5);
            this.splitContainer1.Panel2.Controls.Add(this.label8);
            this.splitContainer1.Panel2.Controls.Add(this.dataGridInvItems);
            this.splitContainer1.Size = new System.Drawing.Size(841, 595);
            this.splitContainer1.SplitterDistance = 383;
            this.splitContainer1.TabIndex = 27;
            // 
            // btnInvoiceRemove
            // 
            this.btnInvoiceRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnInvoiceRemove.Location = new System.Drawing.Point(86, 353);
            this.btnInvoiceRemove.Name = "btnInvoiceRemove";
            this.btnInvoiceRemove.Size = new System.Drawing.Size(114, 23);
            this.btnInvoiceRemove.TabIndex = 25;
            this.btnInvoiceRemove.Text = "Remove selected";
            this.btnInvoiceRemove.UseVisualStyleBackColor = true;
            this.btnInvoiceRemove.Click += new System.EventHandler(this.btnInvoiceRemove_Click);
            // 
            // btnInvoiceAdd
            // 
            this.btnInvoiceAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnInvoiceAdd.Location = new System.Drawing.Point(5, 353);
            this.btnInvoiceAdd.Name = "btnInvoiceAdd";
            this.btnInvoiceAdd.Size = new System.Drawing.Size(75, 23);
            this.btnInvoiceAdd.TabIndex = 24;
            this.btnInvoiceAdd.Text = "Add new";
            this.btnInvoiceAdd.UseVisualStyleBackColor = true;
            this.btnInvoiceAdd.Click += new System.EventHandler(this.btnInvoiceAdd_Click);
            // 
            // dataCSV
            // 
            this.dataCSV.AllowUserToAddRows = false;
            this.dataCSV.AllowUserToDeleteRows = false;
            this.dataCSV.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataCSV.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataCSV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataCSV.Location = new System.Drawing.Point(5, 4);
            this.dataCSV.MultiSelect = false;
            this.dataCSV.Name = "dataCSV";
            this.dataCSV.Size = new System.Drawing.Size(827, 343);
            this.dataCSV.TabIndex = 23;
            this.dataCSV.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.InvoiceValueChanged);
            this.dataCSV.CellEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.InvoiceChanged);
            // 
            // tabItems
            // 
            this.tabItems.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.tabItems.Controls.Add(this.tabAllItems);
            this.tabItems.Controls.Add(this.tabSelItems);
            this.tabItems.Location = new System.Drawing.Point(7, 45);
            this.tabItems.Name = "tabItems";
            this.tabItems.SelectedIndex = 0;
            this.tabItems.Size = new System.Drawing.Size(121, 134);
            this.tabItems.TabIndex = 33;
            // 
            // tabAllItems
            // 
            this.tabAllItems.Controls.Add(this.lbNonPaired);
            this.tabAllItems.Location = new System.Drawing.Point(4, 22);
            this.tabAllItems.Name = "tabAllItems";
            this.tabAllItems.Padding = new System.Windows.Forms.Padding(3);
            this.tabAllItems.Size = new System.Drawing.Size(113, 108);
            this.tabAllItems.TabIndex = 0;
            this.tabAllItems.Text = "All";
            this.tabAllItems.UseVisualStyleBackColor = true;
            // 
            // lbNonPaired
            // 
            this.lbNonPaired.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lbNonPaired.FormattingEnabled = true;
            this.lbNonPaired.Location = new System.Drawing.Point(0, 0);
            this.lbNonPaired.Name = "lbNonPaired";
            this.lbNonPaired.Size = new System.Drawing.Size(113, 108);
            this.lbNonPaired.TabIndex = 28;
            // 
            // tabSelItems
            // 
            this.tabSelItems.Controls.Add(this.lbFilteredItems);
            this.tabSelItems.Location = new System.Drawing.Point(4, 22);
            this.tabSelItems.Name = "tabSelItems";
            this.tabSelItems.Padding = new System.Windows.Forms.Padding(3);
            this.tabSelItems.Size = new System.Drawing.Size(113, 108);
            this.tabSelItems.TabIndex = 1;
            this.tabSelItems.Text = "Selected";
            this.tabSelItems.UseVisualStyleBackColor = true;
            // 
            // lbFilteredItems
            // 
            this.lbFilteredItems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lbFilteredItems.FormattingEnabled = true;
            this.lbFilteredItems.Location = new System.Drawing.Point(0, 0);
            this.lbFilteredItems.Name = "lbFilteredItems";
            this.lbFilteredItems.Size = new System.Drawing.Size(113, 108);
            this.lbFilteredItems.TabIndex = 30;
            // 
            // lblUnpiredCount
            // 
            this.lblUnpiredCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblUnpiredCount.AutoSize = true;
            this.lblUnpiredCount.Location = new System.Drawing.Point(4, 182);
            this.lblUnpiredCount.Name = "lblUnpiredCount";
            this.lblUnpiredCount.Size = new System.Drawing.Size(84, 13);
            this.lblUnpiredCount.TabIndex = 32;
            this.lblUnpiredCount.Text = "0 unpaired items";
            // 
            // btnUnpairAll
            // 
            this.btnUnpairAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUnpairAll.Location = new System.Drawing.Point(720, 182);
            this.btnUnpairAll.Name = "btnUnpairAll";
            this.btnUnpairAll.Size = new System.Drawing.Size(114, 23);
            this.btnUnpairAll.TabIndex = 31;
            this.btnUnpairAll.Text = "Unpair all";
            this.btnUnpairAll.UseVisualStyleBackColor = true;
            this.btnUnpairAll.Click += new System.EventHandler(this.btnUnpairAll_Click);
            // 
            // btnUnpairInvoiceItem
            // 
            this.btnUnpairInvoiceItem.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUnpairInvoiceItem.Location = new System.Drawing.Point(600, 182);
            this.btnUnpairInvoiceItem.Name = "btnUnpairInvoiceItem";
            this.btnUnpairInvoiceItem.Size = new System.Drawing.Size(114, 23);
            this.btnUnpairInvoiceItem.TabIndex = 0;
            this.btnUnpairInvoiceItem.Text = "Unpair selected";
            this.btnUnpairInvoiceItem.UseVisualStyleBackColor = true;
            this.btnUnpairInvoiceItem.Click += new System.EventHandler(this.btnUnpairInvoiceItem_Click);
            // 
            // btnInvoiceItemRemove
            // 
            this.btnInvoiceItemRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnInvoiceItemRemove.Location = new System.Drawing.Point(212, 182);
            this.btnInvoiceItemRemove.Name = "btnInvoiceItemRemove";
            this.btnInvoiceItemRemove.Size = new System.Drawing.Size(114, 23);
            this.btnInvoiceItemRemove.TabIndex = 27;
            this.btnInvoiceItemRemove.Text = "Remove selected";
            this.btnInvoiceItemRemove.UseVisualStyleBackColor = true;
            this.btnInvoiceItemRemove.Click += new System.EventHandler(this.btnInvoiceItemRemove_Click);
            // 
            // btnAssignProd
            // 
            this.btnAssignProd.Location = new System.Drawing.Point(6, 16);
            this.btnAssignProd.Name = "btnAssignProd";
            this.btnAssignProd.Size = new System.Drawing.Size(120, 23);
            this.btnAssignProd.TabIndex = 30;
            this.btnAssignProd.Text = "Pair selected ->";
            this.btnAssignProd.UseVisualStyleBackColor = true;
            this.btnAssignProd.Click += new System.EventHandler(this.btnAssignProd_Click);
            // 
            // btnInvoiceItemNew
            // 
            this.btnInvoiceItemNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnInvoiceItemNew.Location = new System.Drawing.Point(134, 182);
            this.btnInvoiceItemNew.Name = "btnInvoiceItemNew";
            this.btnInvoiceItemNew.Size = new System.Drawing.Size(75, 23);
            this.btnInvoiceItemNew.TabIndex = 26;
            this.btnInvoiceItemNew.Text = "Add new";
            this.btnInvoiceItemNew.UseVisualStyleBackColor = true;
            this.btnInvoiceItemNew.Click += new System.EventHandler(this.btnInvoiceItemNew_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(103, 13);
            this.label5.TabIndex = 29;
            this.label5.Text = "Non paired products";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(129, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(69, 13);
            this.label8.TabIndex = 27;
            this.label8.Text = "Invoice items";
            // 
            // dataGridInvItems
            // 
            this.dataGridInvItems.AllowUserToAddRows = false;
            this.dataGridInvItems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridInvItems.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridInvItems.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridInvItems.Location = new System.Drawing.Point(134, 21);
            this.dataGridInvItems.MultiSelect = false;
            this.dataGridInvItems.Name = "dataGridInvItems";
            this.dataGridInvItems.Size = new System.Drawing.Size(698, 157);
            this.dataGridInvItems.TabIndex = 0;
            this.dataGridInvItems.SelectionChanged += new System.EventHandler(this.InvoiceItemSelChanged);
            // 
            // stock
            // 
            this.stock.Controls.Add(this.btnExportMSG);
            this.stock.Controls.Add(this.btnUnpairAllMSG);
            this.stock.Controls.Add(this.btnUnpairProductMSG);
            this.stock.Controls.Add(this.btnRemoveMSG);
            this.stock.Controls.Add(this.btnAddMsg);
            this.stock.Controls.Add(this.dataGrid);
            this.stock.Location = new System.Drawing.Point(4, 22);
            this.stock.Name = "stock";
            this.stock.Padding = new System.Windows.Forms.Padding(3);
            this.stock.Size = new System.Drawing.Size(840, 599);
            this.stock.TabIndex = 1;
            this.stock.Text = "Stock";
            this.stock.UseVisualStyleBackColor = true;
            // 
            // btnExportMSG
            // 
            this.btnExportMSG.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExportMSG.Location = new System.Drawing.Point(440, 576);
            this.btnExportMSG.Name = "btnExportMSG";
            this.btnExportMSG.Size = new System.Drawing.Size(114, 23);
            this.btnExportMSG.TabIndex = 35;
            this.btnExportMSG.Text = "Export";
            this.btnExportMSG.UseVisualStyleBackColor = true;
            this.btnExportMSG.Click += new System.EventHandler(this.btnExportMSG_Click);
            // 
            // btnUnpairAllMSG
            // 
            this.btnUnpairAllMSG.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUnpairAllMSG.Location = new System.Drawing.Point(720, 576);
            this.btnUnpairAllMSG.Name = "btnUnpairAllMSG";
            this.btnUnpairAllMSG.Size = new System.Drawing.Size(114, 23);
            this.btnUnpairAllMSG.TabIndex = 33;
            this.btnUnpairAllMSG.Text = "Unpair all";
            this.btnUnpairAllMSG.UseVisualStyleBackColor = true;
            this.btnUnpairAllMSG.Click += new System.EventHandler(this.btnUnpairAllMSG_Click);
            // 
            // btnUnpairProductMSG
            // 
            this.btnUnpairProductMSG.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUnpairProductMSG.Location = new System.Drawing.Point(598, 576);
            this.btnUnpairProductMSG.Name = "btnUnpairProductMSG";
            this.btnUnpairProductMSG.Size = new System.Drawing.Size(114, 23);
            this.btnUnpairProductMSG.TabIndex = 32;
            this.btnUnpairProductMSG.Text = "Unpair selected";
            this.btnUnpairProductMSG.UseVisualStyleBackColor = true;
            this.btnUnpairProductMSG.Click += new System.EventHandler(this.btnUnpairProductMSG_Click);
            // 
            // btnRemoveMSG
            // 
            this.btnRemoveMSG.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRemoveMSG.Location = new System.Drawing.Point(87, 576);
            this.btnRemoveMSG.Name = "btnRemoveMSG";
            this.btnRemoveMSG.Size = new System.Drawing.Size(114, 23);
            this.btnRemoveMSG.TabIndex = 27;
            this.btnRemoveMSG.Text = "Remove selected";
            this.btnRemoveMSG.UseVisualStyleBackColor = true;
            this.btnRemoveMSG.Click += new System.EventHandler(this.btnRemoveMSG_Click);
            // 
            // btnAddMsg
            // 
            this.btnAddMsg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAddMsg.Location = new System.Drawing.Point(6, 576);
            this.btnAddMsg.Name = "btnAddMsg";
            this.btnAddMsg.Size = new System.Drawing.Size(75, 23);
            this.btnAddMsg.TabIndex = 26;
            this.btnAddMsg.Text = "Add new";
            this.btnAddMsg.UseVisualStyleBackColor = true;
            this.btnAddMsg.Click += new System.EventHandler(this.btnAddMsg_Click);
            // 
            // dataGrid
            // 
            this.dataGrid.AllowUserToAddRows = false;
            this.dataGrid.AllowUserToOrderColumns = true;
            this.dataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGrid.Location = new System.Drawing.Point(6, 6);
            this.dataGrid.MultiSelect = false;
            this.dataGrid.Name = "dataGrid";
            this.dataGrid.Size = new System.Drawing.Size(828, 564);
            this.dataGrid.TabIndex = 3;
            this.dataGrid.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.StockValueChanged);
            // 
            // tabFilesLog
            // 
            this.tabFilesLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.tabFilesLog.Controls.Add(this.tabFoundFiles);
            this.tabFilesLog.Controls.Add(this.tabLog);
            this.tabFilesLog.Location = new System.Drawing.Point(12, 174);
            this.tabFilesLog.Name = "tabFilesLog";
            this.tabFilesLog.SelectedIndex = 0;
            this.tabFilesLog.Size = new System.Drawing.Size(562, 463);
            this.tabFilesLog.TabIndex = 29;
            // 
            // tabFoundFiles
            // 
            this.tabFoundFiles.Controls.Add(this.dataFiles);
            this.tabFoundFiles.Controls.Add(this.btnDeselectAll);
            this.tabFoundFiles.Controls.Add(this.btnSelectAll);
            this.tabFoundFiles.Controls.Add(this.btnInverse);
            this.tabFoundFiles.Location = new System.Drawing.Point(4, 22);
            this.tabFoundFiles.Name = "tabFoundFiles";
            this.tabFoundFiles.Padding = new System.Windows.Forms.Padding(3);
            this.tabFoundFiles.Size = new System.Drawing.Size(554, 437);
            this.tabFoundFiles.TabIndex = 0;
            this.tabFoundFiles.Text = "Found files";
            this.tabFoundFiles.UseVisualStyleBackColor = true;
            // 
            // dataFiles
            // 
            this.dataFiles.AllowUserToOrderColumns = true;
            this.dataFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataFiles.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataFiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataFiles.Location = new System.Drawing.Point(0, 0);
            this.dataFiles.MultiSelect = false;
            this.dataFiles.Name = "dataFiles";
            this.dataFiles.Size = new System.Drawing.Size(554, 405);
            this.dataFiles.TabIndex = 18;
            this.dataFiles.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataFiles_CellValueChanged);
            // 
            // tabLog
            // 
            this.tabLog.Controls.Add(this.txtLog);
            this.tabLog.Controls.Add(this.btnClear);
            this.tabLog.Location = new System.Drawing.Point(4, 22);
            this.tabLog.Name = "tabLog";
            this.tabLog.Padding = new System.Windows.Forms.Padding(3);
            this.tabLog.Size = new System.Drawing.Size(554, 437);
            this.tabLog.TabIndex = 1;
            this.tabLog.Text = "Log";
            this.tabLog.UseVisualStyleBackColor = true;
            // 
            // FrmActiveStyle
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1444, 690);
            this.Controls.Add(this.tabFilesLog);
            this.Controls.Add(this.tabData);
            this.Controls.Add(this.btnProcess);
            this.Controls.Add(this.grpInputSettings);
            this.Controls.Add(this.btn2XML);
            this.Controls.Add(this.btnRead);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FrmActiveStyle";
            this.Text = "ActiveStyle (c) XML exporter";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.grpInputSettings.ResumeLayout(false);
            this.grpInputSettings.PerformLayout();
            this.tabData.ResumeLayout(false);
            this.invoice.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataCSV)).EndInit();
            this.tabItems.ResumeLayout(false);
            this.tabAllItems.ResumeLayout(false);
            this.tabSelItems.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridInvItems)).EndInit();
            this.stock.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).EndInit();
            this.tabFilesLog.ResumeLayout(false);
            this.tabFoundFiles.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataFiles)).EndInit();
            this.tabLog.ResumeLayout(false);
            this.tabLog.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnRead;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.TextBox txtInputPath;
        private System.Windows.Forms.Button btnChoose;
        private System.Windows.Forms.Button btn2XML;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnOutDir;
        private System.Windows.Forms.TextBox txtOutDir;
        private System.Windows.Forms.CheckBox chkMoveProcessed;
        private System.Windows.Forms.GroupBox grpInputSettings;
        private System.Windows.Forms.Button btnProcess;
        private System.Windows.Forms.Button btnSelectAll;
        private System.Windows.Forms.Button btnInverse;
        private System.Windows.Forms.Button btnDeselectAll;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabData;
        private System.Windows.Forms.TabPage invoice;
        private System.Windows.Forms.TabPage stock;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button btnInvoiceAdd;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ListBox lbNonPaired;
        private System.Windows.Forms.Button btnAssignProd;
        private System.Windows.Forms.Button btnInvoiceRemove;
        private System.Windows.Forms.Button btnInvoiceItemRemove;
        private System.Windows.Forms.Button btnInvoiceItemNew;
        private System.Windows.Forms.Button btnRemoveMSG;
        private System.Windows.Forms.Button btnAddMsg;
        private CustomDataGridView dataGrid;
        private CustomDataGridView dataFiles;
        private CustomDataGridView dataCSV;
        private CustomDataGridView dataGridInvItems;
        private System.Windows.Forms.Button btnUnpairInvoiceItem;
        private System.Windows.Forms.Button btnUnpairAll;
        private System.Windows.Forms.Button btnUnpairAllMSG;
        private System.Windows.Forms.Button btnUnpairProductMSG;
        private System.Windows.Forms.Button btnExportMSG;
        private System.Windows.Forms.TabControl tabFilesLog;
        private System.Windows.Forms.TabPage tabFoundFiles;
        private System.Windows.Forms.TabPage tabLog;
        private System.Windows.Forms.Label lblUnpiredCount;
        private System.Windows.Forms.TabControl tabItems;
        private System.Windows.Forms.TabPage tabAllItems;
        private System.Windows.Forms.TabPage tabSelItems;
        private System.Windows.Forms.ListBox lbFilteredItems;
    }
}

