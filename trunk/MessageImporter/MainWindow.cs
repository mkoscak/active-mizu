using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using System.IO;
using System.Globalization;
using System.Xml.Serialization;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;

namespace MessageImporter
{

    public partial class FrmActiveStyle : Form
    {   
        internal const string productCode = "Product Code:";
        internal const string delivery = "Delivery:";
        internal const string deliveryText = "Delivery";
        internal const string orderRef = "Order Reference:";
        internal const string ourRef = "Our Reference:";

        _Application outlook = new ApplicationClass();

        List<CSVFile> allOrders = new List<CSVFile>();
        List<StockEntity> allMessages = new List<StockEntity>();
        
        // data sources
        List<Invoice> AllInvoices = new List<Invoice>();
        List<StockItem> AllStocks = new List<StockItem>();
        List<StockItem> WaitingToUpdate = new List<StockItem>();
       
        //////////////////////////////////////////////////////////////////////////////////////////
        BindingList<Invoice> GetInvoiceDS()
        {
            return gridInvoices.DataSource as BindingList<Invoice>;
        }
        void SetInvoiceDS(BindingList<Invoice> dataSource)
        {
            gridInvoices.DataSource = dataSource;
        }

        BindingList<InvoiceItem> GetInvoiceItemsDS()
        {
            return gridInvItems.DataSource as BindingList<InvoiceItem>;
        }
        void SetInvoiceItemsDS(BindingList<InvoiceItem> dataSource)
        {
            gridInvItems.DataSource = dataSource;
        }

        BindingList<StockItem> GetProductsDS()
        {
            return gridStocks.DataSource as BindingList<StockItem>;
        }
        void SetProductsDS(BindingList<StockItem> dataSource)
        {
            gridStocks.DataSource = dataSource;
        }

        //////////////////////////////////////////////////////////////////////////////////////////


        public FrmActiveStyle()
        {
            InitializeComponent();

            txtInputPath.Text = System.Windows.Forms.Application.StartupPath + @"\InData";
            txtOutDir.Text = System.Windows.Forms.Application.StartupPath + @"\OutData";
            chkMoveProcessed.Checked = false;   // TODO - zmenit na true!!?

            btnSetWaiting.Image = Icons.Waiting;
            btnSetWaiting.TextImageRelation = TextImageRelation.ImageBeforeText;

            btnReplaceReload_Click(btnReplaceReload, new EventArgs());
            btnChildReload_Click(btnChildReload, new EventArgs());

            cbWaitingInvValidity.SelectedIndex = 0;
            cbWaitingStockValidity.SelectedIndex = 0;

            // stiahnutie a import kurzoveho listka
            try
            {
                log("");
                log("Downloading exchange rates from "+Properties.Settings.Default.ExchRateXMLAddress);
                DownloadExchangeRateXML();
                log("done.");
                log("Updating ex. rates in settings..");
                UpdateExRates();
                log("done.");
            }
            catch (System.Exception ex)
            {
                log("failed!");
                MessageBox.Show(this, "Failed to load exchange rates from "+Properties.Settings.Default.ExchRateXMLAddress+"! Exception: "+ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // aktualizacia nastaveni v GUI - hlavne exchange rates
            btnSettingsLoad_Click(btnSettingsLoad, new EventArgs());

            btnProcess_Click(btnProcess, new EventArgs());
        }

        private void UpdateExRates()
        {
            var exr = DBProvider.GetExRateDayBefore(DateTime.Now);
            if(exr == null)
                exr = DBProvider.GetExRate(DateTime.Now.ToString("yyyy-MM-dd"));

            if (exr == null)
            {
                log("   Failed to load exchange rates for yesterday!");
                return;
            }

            var prop = Properties.Settings.Default;

            prop.ExRateCzk = Math.Round(exr.RateCZK, 3).ToString();
            prop.ExRatePln = Math.Round(exr.RatePLN, 4).ToString();
            prop.ExRateHuf = Math.Round(exr.RateHUF, 2).ToString();

            prop.Save();
        }

        internal void btnRead_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            // validacia
            if (txtInputPath.Text.Trim() == string.Empty)
            {
                MessageBox.Show(this, "Select input directory!", "Error");
                return;
            }

            if (txtOutDir.Text.Trim() == string.Empty)
            {
                MessageBox.Show(this, "Select output directory!", "Error");
                return;
            }
            // validacia - koniec

            try
            {
                logNewSection("Reading input directory..");

                var files = new List<FileItem>();
                
                log("MSG files: ");
                var orders = new List<StockEntity>();
                foreach (string fileName in Directory.GetFiles(txtInputPath.Text, "*.msg", SearchOption.TopDirectoryOnly))
                {
                    log("\t" + Common.ExtractFileName(fileName));
                    files.Add(new FileItem(true, fileName));
                }

                log("");
                log("CSV files: ");
                foreach (string fileName in Directory.GetFiles(txtInputPath.Text, "*.csv", SearchOption.TopDirectoryOnly))
                {
                    log("\t" + Common.ExtractFileName(fileName));
                    files.Add(new FileItem(true, fileName));
                }

                log("");
                log("CSVX files: ");
                foreach (string fileName in Directory.GetFiles(txtInputPath.Text, "*.csvx", SearchOption.TopDirectoryOnly))
                {
                    log("\t" + Common.ExtractFileName(fileName));
                    files.Add(new FileItem(true, fileName));
                }

                dataFiles.DataSource = files;

                dataFiles.Columns["OrderDate"].DefaultCellStyle.Format = "dd.MM.yyyy";

                SetInvoiceDS(new BindingList<Invoice>());
                SetInvoiceItemsDS(new BindingList<InvoiceItem>());
                SetProductsDS(new BindingList<StockItem>());
                UpdateProductSet();
            }
            catch (System.Exception ex)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show(this, ex.ToString(), "Error");
            }

            Cursor.Current = Cursors.Default;
        }

        internal StockEntity decodeMessage(string messageBody, FileItem file)
        {
            try
            {
                var lines = messageBody.Split(Environment.NewLine.ToCharArray()).Where(s => s != null && s.Trim().Length > 0).ToArray();
                var positionQTY = 5;
                if (lines.Contains("\tConfirmation Note\t "))
                    positionQTY = 4;
             

                var order = new StockEntity();
                List<StockItem> items = new List<StockItem>();

                file.ProdCount = 0;
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];

                    if (line.Contains(productCode))
                    {
                        StockItem item = new StockItem();

                        item.ProductCode = line.Substring(line.IndexOf(':') + 1).Trim();
                        line = lines[i - positionQTY];//i - 5
                        item.Ord_Qty = int.Parse(line.Trim());
                        line = lines[i - 4];
                        item.Disp_Qty = int.Parse(line.Trim());
                        line = lines[i - 3];
                        item.Description = line.Trim();
                        line = lines[i - 2];
                        line = line.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                        //item.Price = double.Parse(line.Trim().Substring(1));// Regex.Replace(strPara, @"\([A-9]\)", "");  [^0-9.,]
                        item.Price = double.Parse(Regex.Replace(line, @"[^0-9.,]", ""));
                        line = lines[i - 1];
                        line = line.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                       // item.Total = double.Parse(line.Trim().Substring(1));
                        item.Total = double.Parse(Regex.Replace(line, @"[^0-9.,]", ""));
                        item.Currency = line.Substring(0, 1);

                        item.FromFile = file;
                        file.ProdCount++;

                        if (item.State == StockItemState.PermanentStorage)
                            item.Sklad = "02";
                        else if (item.State == StockItemState.Waiting)
                            item.Sklad = Properties.Settings.Default.Storage;

                        items.Add(item);
                    }

                    if (line.Contains(delivery))//!!!!!NIKDY NENASTANE!!!!!!
                    {
                        StockItem item = new StockItem();

                        line = lines[i + 1];
                        line = line.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                        item.Price = double.Parse(line.Trim().Substring(1));
                        item.Total = item.Price;
                        item.Currency = line.Substring(0, 1);

                        item.Description = deliveryText;
                        item.Disp_Qty = 1;
                        item.Ord_Qty = 1;
                        item.ProductCode = item.Description;

                        item.FromFile = file;

                        file.Delivery += item.Price;
                        //file.ProdCount++;
                        //items.Add(item);  // doprava nebude polozka ale spojena so suborom
                    }

                    if (line.Contains(orderRef))
                    {
                        line = lines[i + 1];
                        order.OrderReference = line.Trim();
                    }

                    if (line.Contains(ourRef))
                    {
                        line = lines[i + 1];
                        order.OurReference = line.Trim();
                    }
                }
                
                DecomposeMultipleItems(items);

                order.Items = items.ToArray();

                return order;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), "Error");
            }

            return null;
        }

        private static void DecomposeMultipleItems(List<StockItem> items)
        {
            // viacpoctove produkty sa rozkuskuju
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Disp_Qty > 1)
                {
                    var count = items[i].Disp_Qty;
                    items[i].Disp_Qty = 1;

                    for (int j = 0; j < count - 1; j++)
                        items.Insert(i, items[i].Clone() as StockItem);

                    i += count;
                }
            }
        }
   
        internal void log(string message)
        {
            txtLog.AppendText(message + Environment.NewLine);
        }

        internal void logNewSection(string message)
        {
            txtLog.AppendText(Environment.NewLine + message + Environment.NewLine);
        }

        internal string SelectDirectory(string title)
        {
            try
            {
                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    dialog.Description = title;
                    dialog.ShowNewFolderButton = true;
                    dialog.RootFolder = Environment.SpecialFolder.MyComputer;
                    dialog.SelectedPath = System.Windows.Forms.Application.StartupPath;
                    if (dialog.ShowDialog() == DialogResult.OK)
                        return dialog.SelectedPath;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), "Error");
            }

            return null;
        }

        internal void btnChoose_Click_1(object sender, EventArgs e)
        {
            var folder = SelectDirectory("Choose a directory with .msg and .cvs files");
            if (folder == null)
                return;

            logNewSection("Input directory changed..");
            txtInputPath.Text = folder;
        }

        internal void btnOutDir_Click_1(object sender, EventArgs e)
        {
            var folder = SelectDirectory("Choose an output directory");
            if (folder == null)
                return;

            logNewSection("Output directory changed..");
            txtOutDir.Text = folder;
        }

        internal void btnClear_Click_1(object sender, EventArgs e)
        {
           txtLog.Text = string.Empty;
        }

        internal const string processedDirectoryName = "processed";
        internal void BackupFile(string fileName)
        {
            var dir = fileName.Substring(0, fileName.LastIndexOf('\\')+1);
            var procDir = dir + processedDirectoryName;
            
            if (!Directory.Exists(procDir))
                Directory.CreateDirectory(procDir);

            File.Move(fileName, procDir + @"\" + Common.ExtractFileName(fileName));
        }

        /// <summary>
        /// spracovanie vybranych vstupnych suborov
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void btnProcess_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            
            logNewSection("Begin processing..");
            
            try
            {
                SetInvoiceDS(new BindingList<Invoice>());
                SetInvoiceItemsDS(new BindingList<InvoiceItem>());
                SetProductsDS(new BindingList<StockItem>());

                // nacitanie dat do allMessages a allOrders
                if (!ProcessSelectedFiles())
                    return;

                var stocks = allMessages.SelectMany(o => o.Items).ToList();

                // naplni allInvoices a nastavi datasource
                CreateInvoice(allOrders);
                // pridanie poloziek "Cena za dopravu"
                AddShippingItems(AllInvoices);
                SetInvoiceDS(new BindingList<Invoice>(AllInvoices));
                // kontrola na nejasnosti v kodoch produktov
                CheckPairByHand(stocks);
                //AllStocks = allMessages.SelectMany(o => o.Items).ToList();
                SetProductsDS(new BindingList<StockItem>(stocks));
                //UniqueStocks();
                                
                // dopocitanie cien s dopravou
                CalcBuyingPrice(GetProductsDS());
                // parovanie produktov
                PairProducts();
                // aktualizacia mnoziny volnych produktov
                UpdateProductSet();
                // nastavenie vybavenosti objednavkam
                CheckAllEqipped();
                // kurzovy prepocet a nastavenie datumu objednavky
                CalcRateOrderdate(dataFiles.DataSource as List<FileItem>);

                // postprocessing pre MandMdirect
                PostProcessMandM(stocks);

                gridStocks.Columns["OrderDate"].DefaultCellStyle.Format = "dd.MM.yyyy";
                gridInvItems.Columns["Datetime"].DefaultCellStyle.Format = "dd.MM.yyyy";
                
                if (chkMoveProcessed.Checked)
                    btnRead.PerformClick();

               /* gridStocks.Refresh();
                UpdateProductSet();
                gridStocks.Refresh();
                dataFiles.Refresh();*/

                RefreshTab();
            }
            catch (System.Exception ex)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show(this, ex.ToString(), "Error");
            }

            Cursor.Current = Cursors.Default;
        }

        private void PostProcessMandM(List<StockItem> stocks)
        {
            foreach (var stock in stocks)
            {
                if (stock.FromFile.Type == MSG_TYPE.MANDM_DIRECT)
                {
                    stock.ProductCode = "AM_" + stock.ProductCode;
                }
            }

            TabChanged(tabData, new EventArgs());
        }

        private BindingList<StockItem> UniqueStocks()
        {
            var ds = GetProductsDS();
            if (ds == null)
                return null;

            BindingList<StockItem> newDs = new BindingList<StockItem>();
            foreach (var item in ds)
            {
                var c = ds.Count(s => s.ProductCode == item.ProductCode);
                if (c == 1)
                {
                    newDs.Add(item);
                    continue;
                }

                if (newDs.Count(s => s.ProductCode == item.ProductCode) > 0)    // uz existuje
                    continue;

                item.Disp_Qty = c;  // nastavime celkovy pocet
                item.Ord_Qty = c;  // nastavime celkovy pocet
                newDs.Add(item);
            }

            return newDs;
        }

        private void CheckPairByHand(List<StockItem> bindingList)
        {
            var checkLength = Properties.Settings.Default.SubProductLength;

            foreach (var stock in bindingList)
            {
                if (stock.PairByHand)
                    continue;

                if (stock.ProductCode.Length <= checkLength)
                    continue;
                
                var toCheck = stock.ProductCode.Substring(0, checkLength);
                // vsetky polozky ktore zacinaju na N rovnakych cisel ale celkovy kod je rozny
                var found = bindingList.Where(it => it != stock && 
                    it.ProductCode.Length > checkLength && 
                    it.ProductCode.Substring(0,checkLength) == toCheck && 
                    it.ProductCode != stock.ProductCode).ToList();

                if(found != null && found.Count > 0)
                {
                    stock.PairByHand = true;
                    found.ForEach(it => it.PairByHand = true);
                }
            }
        }

        /// <summary>
        /// Fake produkt cena za dopravu
        /// </summary>
        /// <param name="allInvoices"></param>
        private void AddShippingItems(List<Invoice> allInvoices)
        {
            foreach (var inv in allInvoices)
            {
                InvoiceItem shipping = new InvoiceItem(inv);
                StockItem shippingStock = new StockItem();

                shipping.PairProduct = shippingStock;   // previazanie poloziek

                var config = new CountrySetting(inv.Country);
                var price = inv.OrderShipping;

                if (Common.GetPrice(price) == 0.0)
                    shipping.ItemPrice = price;
                else
                    shipping.ItemPrice = config.ShipPrice.ToString();

                inv.OrderShipping = shipping.ItemPrice;

                shippingStock.Description = config.ShipText;
                shippingStock.ProductCode = Properties.Settings.Default.ShippingCode;

                shipping.ItemQtyOrdered = "1";
                shippingStock.OrderDate = DateTime.Now;

                inv.InvoiceItems.Add(shipping);
            }
        }

        void RefreshTab()
        {
            var sel = (Tabs)tabData.SelectedIndex;

            switch (sel)
            {
                case Tabs.Invoices:
                    {
                        gridInvoices.Refresh();
                        gridInvItems.Refresh();
                    }
                    break;

                case Tabs.Stocks:
                    {
                        gridStocks.Refresh();

                        var ds = GetProductsDS();
                        if (ds == null)
                            return;

                        // nastavenie farieb
                        for (int i = 0; i < ds.Count; i++)
                        {
                            if (ds[i].ChangeColor)
                            {
                                gridStocks["PriceEURnoTaxEUR", i].Style.BackColor = Color.Green;
                                gridStocks["PriceEURnoTaxEUR", i].Style.ForeColor = Color.White;
                            }

                            if (ds[i].PairByHand && ds[i].PairProduct == null)
                            {
                                gridStocks["ProductCode", i].Style.BackColor = Color.Blue;
                                gridStocks["ProductCode", i].Style.ForeColor = Color.White;
                            }

                            var tmp = ds[i].State;  // refresh stavu na naplnenie Skladu
                        }
                    }
                    break;

                case Tabs.Reader:
                    {
                        RefreshReader();
                    }
                    break;

                case Tabs.Waiting:
                    {
                        RefreshWaiting();
                    }
                    break;

                default:
                    break;
            }

            dataFiles.Refresh();
        }

        /// <summary>
        /// Nacita waiting produkty podla aktualneho filtra..
        /// </summary>
        private void RefreshWaiting()
        {
            RefreshWaitingInvoices(cbWaitingInvValidity.SelectedIndex);
            RefreshWaitingStocks(cbWaitingStockValidity.SelectedIndex);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="validity">0 - valid, 1 - invalid, 2 - all</param>
        private void RefreshWaitingInvoices(int validity)
        {
            var valid = Math.Abs(validity - 1);

            var query = string.Format("select * from {0} where {1}", DBProvider.T_WAIT_INVOICES, validity < 2 ? "valid = " + valid.ToString() : "1=1");
            var found = DBProvider.ExecuteQuery(query);
            gridWaitingInv.DataSource = null;
            var table = found.Tables[0];
            gridWaitingInv.DataSource = table;

            lblWaitingInvCount.Text = table.Rows.Count.ToString() + " items";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="validity">0 - valid, 1 - invalid, 2 - all</param>
        private void RefreshWaitingStocks(int validity)
        {
            var valid = Math.Abs(validity - 1);

            var query = string.Format("select * from {0} where {1}", DBProvider.T_WAIT_STOCK, validity < 2 ? "valid = " + valid.ToString() : "1=1");
            var found = DBProvider.ExecuteQuery(query);
            gridWaitingStock.DataSource = null;
            var table = found.Tables[0];
            gridWaitingStock.DataSource = table;

            lblWaitingStockCount.Text = table.Rows.Count.ToString() + " items";
        }

        private void btnWaitingInvSetUsed_Click(object sender, EventArgs e)
        {
            var ids = GetSelectedIds(gridWaitingInv);
            DBProvider.UpdateWaitingValidity(DBProvider.T_WAIT_INVOICES, 0, ids);

            RefreshWaitingInvoices(cbWaitingInvValidity.SelectedIndex);
        }

        private void btnWaitingStockSetUsed_Click(object sender, EventArgs e)
        {
            var ids = GetSelectedIds(gridWaitingStock);
            DBProvider.UpdateWaitingValidity(DBProvider.T_WAIT_STOCK, 0, ids);

            RefreshWaitingStocks(cbWaitingStockValidity.SelectedIndex);
        }

        private void btnSetValidWaitingInv_Click(object sender, EventArgs e)
        {
            var ids = GetSelectedIds(gridWaitingInv);
            DBProvider.UpdateWaitingValidity(DBProvider.T_WAIT_INVOICES, 1, ids);

            RefreshWaitingInvoices(cbWaitingInvValidity.SelectedIndex);
        }

        private void btnSetValidWaitingStocks_Click(object sender, EventArgs e)
        {
            var ids = GetSelectedIds(gridWaitingStock);
            DBProvider.UpdateWaitingValidity(DBProvider.T_WAIT_STOCK, 1, ids);

            RefreshWaitingStocks(cbWaitingStockValidity.SelectedIndex);
        }

        private void cbWaitingInvValidity_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshWaitingInvoices(cbWaitingInvValidity.SelectedIndex);
        }

        private void cbWaitingStockValidity_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshWaitingStocks(cbWaitingStockValidity.SelectedIndex);
        }

        private int[] GetSelectedIds(CustomDataGridView grid)
        {
            var sel = grid.SelectedCells;
            var ret = new List<int>();

            for (int i = 0; i < sel.Count; i++)
            {
                var rowi = sel[i].RowIndex;
                var item = grid.Rows[rowi].DataBoundItem as DataRowView;
                var id = Convert.ToInt32(item[0]);

                ret.Add(id);
            }

            return ret.ToArray();
        }

        private void RefreshReader()
        {
            string cond = "VALID = 1";
            if (!chkOnlyValid.Checked)
                cond = "1 = 1";

            if (!string.IsNullOrEmpty(txtFilOrderNr.Text))
                cond += string.Format(" AND ORDER_NUMBER like \"%{0}%\" ", txtFilOrderNr.Text.Trim());
            if (!string.IsNullOrEmpty(txtFilSKU.Text))
                cond += string.Format(" AND SKU like \"%{0}%\" ", txtFilSKU.Text.Trim());
            if (!string.IsNullOrEmpty(txtFilStoreNr.Text))
                cond += string.Format(" AND STORE_NR like \"%{0}%\" ", txtFilStoreNr.Text.Trim());

            string query = string.Format("SELECT * FROM {0} WHERE {1} ORDER BY ORDER_NUMBER", DBProvider.T_READER, cond);

            var res = DBProvider.ExecuteQuery(query);
            if (res != null && res.Tables != null && res.Tables.Count > 0)
                gridReader.DataSource = res.Tables[0];
        }

        internal bool ProcessSelectedFiles()
        {
            allMessages.Clear();
            allOrders.Clear();

            var files = dataFiles.DataSource as List<FileItem>;
            if (files == null)
            {
                log("\tNo files loaded! Read the input direcotyr first.");
                return false;
            }

            foreach (var file in files)
            {
                file.Delivery = 0.0;

                if (file.Process)
                {
                    log("processing " + file.FileName);

                    if (file.FileName.EndsWith(".msg"))
                    {
                        var ret = ProcessMessage(file);
                        if (ret != null)
                        {
                            allMessages.Add(ret);

                            // presunieme spracovany subor
                            if (chkMoveProcessed.Checked)
                                BackupFile(file.FullFileName);

                            log("\tOK");
                        }
                        else
                        {
                            log("\tFAILED");
                        }
                    }
                    else if (file.FileName.EndsWith(".csv"))
                    {
                        var ret = ProcessCSV(file);
                        if (ret != null)
                        {
                            allOrders.Add(ret);

                            // presunieme spracovany subor
                            if (chkMoveProcessed.Checked)
                                BackupFile(file.FullFileName);

                            log("\tOK");
                        }
                        else
                        {
                            log("\tFAILED");
                        }
                    }
                    else if (file.FileName.EndsWith(".csvx"))   // systemom generovane exporty - temp exporty
                    {
                        var ret = ProcessCSVX(file);
                        if (ret != null)
                        {
                            allMessages.Add(ret);

                            // presunieme spracovany subor
                            if (chkMoveProcessed.Checked)
                                BackupFile(file.FullFileName);

                            log("\tOK");
                        }
                        else
                        {
                            log("\tFAILED");
                        }
                    }
                }
            }

            return true;
        }

        internal StockEntity ProcessCSVX(FileItem file)
        {
            if (file == null)
                return null;
             
            StockEntity ret = new StockEntity();
            try
            {
                string fileContent = File.ReadAllText(file.FullFileName);

                var lines = fileContent.Split(Environment.NewLine.ToCharArray());
                var orderItems = new List<StockItem>();
                foreach (var line in lines)
                {
                    if (line == null || line.Trim().Length == 0)
                        continue;

                    var split = line.Split(';');

                    StockItem newItem = new StockItem();
                    newItem.ProductCode = split[0];
                    newItem.Description = split[1];
                    newItem.Ord_Qty = int.Parse(split[2]);
                    newItem.Disp_Qty = int.Parse(split[3]);
                    newItem.Price = double.Parse(split[4]);
                    newItem.Total = double.Parse(split[5]);
                    newItem.Currency = split[6];
                    newItem.FromFile = file;

                    orderItems.Add(newItem);
                }
                ret.Items = orderItems.ToArray();
            }
            catch (System.Exception ex)
            {
                log(ex.Message);
                return null;
            }

            return ret;
        }

        internal void CreateInvoice(List<CSVFile> allOrders)
        {
            AllInvoices = new List<Invoice>();
            Invoice inv = null;

            foreach (var order in allOrders)
            {
                string actualOrderNumber = string.Empty;
                foreach (var item in order.Items)
                {
                    // vytvorenie novej objednavky
                    if (item.OrderNumber != actualOrderNumber)
                    {
                        actualOrderNumber = item.OrderNumber;

                        if (inv != null)
                            AllInvoices.Add(inv);

                        inv = new Invoice();

                        inv.OrderPurchasedFrom = item.OrderPurchasedFrom;
                        if (inv.OrderPurchasedFrom.Contains(".sk"))
                            inv.Country = Country.Slovakia;
                        else if (inv.OrderPurchasedFrom.Contains(".hu"))
                            inv.Country = Country.Hungary;
                        else if (inv.OrderPurchasedFrom.Contains(".pl"))
                            inv.Country = Country.Poland;
                        else if (inv.OrderPurchasedFrom.Contains(".cz"))
                            inv.Country = Country.CzechRepublic;
                        else
                            inv.Country = Country.Unknown;

                        inv.fromFile = order.File;
                        inv.TotQtyOrdered = item.TotQtyOrdered;
                        inv.BillingCity = item.BillingCity;
                        inv.BillingCompany = item.BillingCompany;
                        inv.BillingCountry = item.BillingCountry;
                        inv.BillingCountryName = item.BillingCountryName;
                        inv.BillingName = item.BillingName;
                        inv.BillingPhoneNumber = Common.ModifyPhoneNr(item.BillingPhoneNumber, inv.Country);
                        inv.BillingState = item.BillingState;
                        inv.BillingStateName = item.BillingStateName;
                        inv.BillingStreet = item.BillingStreet;
                        inv.BillingZip = item.BillingZip;
                        inv.CustomerEmail = item.CustomerEmail;
                        inv.CustomerName = item.CustomerName;
                        inv.OrderDate = item.OrderDate;
                        inv.OrderDiscount = item.OrderDiscount;
                        inv.OrderDue = item.OrderDue;
                        inv.OrderGrandTotal = item.OrderGrandTotal;
                        inv.OrderNumber = item.OrderNumber;
                        inv.OrderPaid = item.OrderPaid;
                        inv.OrderPaymentMethod = item.OrderPaymentMethod;

                        inv.OrderRefunded = item.OrderRefunded;
                        inv.OrderShipping = item.OrderShipping;
                        inv.OrderShippingMethod = item.OrderShippingMethod;
                        inv.OrderStatus = item.OrderStatus;
                        // ak je stav objednavky zrusena v CSV, nastavime tento stav ak objednavke
                        if (inv.OrderStatus.ToLower().Contains("cancel"))
                            inv.Cancelled = true;
                        inv.OrderSubtotal = item.OrderSubtotal;
                        inv.OrderTax = item.OrderTax;
                        inv.ShippingCity = item.ShippingCity;
                        inv.ShippingCompany = item.ShippingCompany;
                        inv.ShippingCountry = item.ShippingCountry;
                        inv.ShippingCountryName = item.ShippingCountryName;
                        inv.ShippingName = item.ShippingName;
                        inv.ShippingPhoneNumber = Common.ModifyPhoneNr(item.ShippingPhoneNumber, inv.Country);
                        inv.ShippingState = item.ShippingState;
                        inv.ShippingStateName = item.ShippingStateName;
                        inv.ShippingStreet = item.ShippingStreet;
                        inv.ShippingZip = item.ShippingZip;

                        /*IČO,DIČ,Company*/
                       // inv.company = item.
                     //   inv.TestValues = item.TestValues;
                    }

                    if (inv != null)
                    {
                        InvoiceItem newItem = new InvoiceItem(inv);
                        newItem.ItemDiscount = item.ItemDiscount;
                        newItem.ItemName = item.ItemName;
                        newItem.ItemOptions = item.ItemOptions;
                        newItem.ItemOrigPrice = item.ItemOrigPrice;
                        newItem.ItemPrice = item.ItemPrice;
                        newItem.ItemQtyCanceled = item.ItemQtyCanceled;
                        newItem.ItemQtyInvoiced = item.ItemQtyInvoiced;
                        newItem.ItemQtyOrdered = item.ItemQtyOrdered;
                        newItem.ItemQtyRefunded = item.ItemQtyRefunded;
                        newItem.ItemQtyShipped = item.ItemQtyShipped;
                        newItem.invSKU = item.ItemSKU;
                        newItem.ItemStatus = item.ItemStatus;
                        newItem.ItemTax = item.ItemTax;
                        newItem.ItemTotal = item.ItemTotal;
                        newItem.OrderItemIncrement = item.OrderItemIncrement;
                        
                        if (inv.InvoiceItems == null)
                            inv.InvoiceItems = new List<InvoiceItem>();
                        inv.InvoiceItems.Add(newItem);
                    }
                }

                if (inv != null)
                    AllInvoices.Add(inv);
                inv = null;
            }

           /**/ WaitingToUpdate = new List<StockItem>();
            foreach (var item in AllInvoices)
            {
                var toAdd = DBProvider.ReadWaitingInvoices(item.OrderNumber, ref WaitingToUpdate);
                foreach (var n in toAdd)
                {
                    n.Parent = item;
                    
                }
                /*Toto neisté, bolo vypnuté ale niekedy to asi treba*/
                //item.InvoiceItems.AddRange(toAdd);
                /*Fix*/
                if (toAdd.Count() > 0)
                {
                    bool containSKU = false;
                    foreach (var product in item.InvoiceItems)
                        if (product.invSKU == toAdd.First().invSKU)
                            containSKU = true;
                    if (!containSKU)
                        item.InvoiceItems.AddRange(toAdd);
                }
            }
        }

        /// <summary>
        /// Kurzovy prepocet, nakupna cena s dopravou..
        /// </summary>
        internal void CalcBuyingPrice(BindingList<StockItem> items)
        {
            if (items == null)
                return;

            var actualFile = string.Empty;

            foreach (var item in items.Where(i => i.FromFile != null))
            {
                var deliverySum = item.FromFile.Delivery;   // suma dopravy
                var prods = items.Where(i => i.FromFile == item.FromFile);  // vsetky produkty s jedneho MSG suboru
                int prodCount = 0;
                foreach (var prod in prods)
                {
                    prodCount += prod.Disp_Qty;
                }
                double deliveryPricePerItem = deliverySum / prodCount;

                if (item.Description != deliveryText)
                    item.PriceWithDelivery = Math.Round(item.Total + item.Disp_Qty * deliveryPricePerItem, 2);
                else
                    item.PriceWithDelivery = item.Total;

                if (item.OrderDate.Year == 1)
                    item.OrderDate = DateTime.Now;

                if (item.FromFile.FileName != actualFile)
                {
                    log("recalculating delivery for '" + item.FromFile.FileName + "':");
                    log("\tdelivery sum = " + deliverySum);
                    log("\tall products count = " + prodCount);
                    log("\tdelivery price per product = " + Math.Round(deliveryPricePerItem, 2));

                    actualFile = item.FromFile.FileName;
                }
            }            
        }

        string ConvertInvoiceItem(string item)
        {
            if (item == null || item == "")
                return null;

            //string productCode = new string(item.ToCharArray().Where(c => "0123456789".Contains(c)).ToArray());
            string productCode = item.Substring(2);
            // 8 mieste kody produtov treba doplnit o lomitko
            if (productCode.Length == 8)
            {
                var a = productCode.Substring(0, productCode.Length - 2);
                var b = productCode.Substring(6);

                productCode = a + "/" + b;
            }

            return productCode;
        }

        /// <summary>
        /// Prepojenie a doplnenie nacitanych udajov
        /// </summary>
        internal void PairProducts()
        {
            foreach (var CSV in AllInvoices)
            {
                foreach (var product in CSV.InvoiceItems)
                {
                    //string productCode="";
                    string productCode = ConvertInvoiceItem(product.invSKU);
                    /*FIX
                    if (product.invSKU!="")
                        productCode = ConvertInvoiceItem(product.invSKU);*/
                    /*END FIX*/

                    if (productCode == null || productCode=="")
                        continue;

                    var prodDS = GetProductsDS();
                    //foreach (var msg in allMessages)
                    {

                        var foundItems = prodDS.Where(orderItem => orderItem.ProductCode != null && stringFunctions.ContainsCaseInsensitive(orderItem.ProductCode, productCode) && orderItem.PairProduct == null).ToList();

                      //  var foundItems = prodDS.Where(orderItem => orderItem.ProductCode != null && orderItem.ProductCode.Contains(productCode) && orderItem.PairProduct == null).ToList();

                        // ak nic nenajdeme skusime opacne parovanie
                        if (foundItems.Count == 0)
                            foundItems = prodDS.Where(orderItem => orderItem.ProductCode != null && product.invSKU.Contains(orderItem.ProductCode) && orderItem.PairProduct == null).ToList();

                        // GetTheLabel produkty nemaju kody, treba skusit parovanie cez nazov
                        if (foundItems.Count == 0)
                                  foundItems = prodDS.Where(orderItem => orderItem.ProductCode != null &&  stringFunctions.ContainsCaseInsensitive(product.ItemName,orderItem.Description) && orderItem.PairProduct == null && product.ItemOptions != null && orderItem.Size != null && orderItem.Size.Trim() == product.ItemOptions.Trim()).ToList();

                     //       foundItems = prodDS.Where(orderItem => orderItem.ProductCode != null && product.ItemName.Contains(orderItem.ProductCode) && orderItem.PairProduct == null && product.ItemOptions != null && orderItem.Size != null && orderItem.Size.Trim() == product.ItemOptions.Trim()).ToList();

                        if (foundItems.Count == 1)
                        {
                            CompleteOrderItem(product, foundItems[0]);
                        }
                        else if (foundItems.Count == 0) // vsetko v pohode
                        {
                        }

                        else //viacnasobne produkty
                        {
                            var count = int.Parse(product.ItemQtyOrdered);
                            var index=-1;
                            for (int i = 0; i < count; i++)
                            {
                                if (foundItems.Count == i)
                                    break;

                                if (foundItems[i].PairByHand)
                                    continue;
                                
                                if (i == 0 && count <= foundItems.Count)
                                {
                                    
                                    //string size;
                                    for (int j = 0; j < foundItems.Count; j++)
                                    {
                                        if (foundItems.ElementAt(j).Size == null)
                                        {
                                            if (stringFunctions.ContainsCaseInsensitive(foundItems.ElementAt(j).Description,product.ItemOptions))
                                            {
                                                if (index == -1)
                                                {
                                                    index = j;
                                                }
                                                else
                                                    index = -2;//sme tu po druhe je zle                                       
                                            }
                                        }
                                        else
                                        {
                                            if (stringFunctions.ContainsCaseInsensitive(product.ItemOptions, foundItems.ElementAt(j).Size))
                                            {
                                                if (index == -1)
                                                {
                                                    index = j;
                                                }
                                                else
                                                    index = -2;//sme tu po druhe je zle                                       
                                            }
                                        }
                                    }
                                    if (index >= 0)
                                    {
                                        product.PairProduct = foundItems[index];
                                        foundItems[index].PairProduct = product;
                                    }
                                }


                                /*if (i < foundItems.Count){
                                    if (index>=0)
                                        foundItems[index].PairProduct = product;     // n produktov zo stock sa naviaze na jeden produkt z CSV (n pocet objednanych v CSV)
                                    else
                                        foundItems[i].PairProduct = product; 
                                }*/
                            }
                        }
                    }
                   /* string x;
                    if (product.invSKU == "AS53201025")
                        x=product.PairCode;*/

                    if (product.PairProduct == null)
                    {
                        var req = string.Format("SELECT * FROM "+DBProvider.T_WAIT_STOCK+" WHERE ORDER_NUMBER = \"{0}\" AND INV_SKU = \"{1}\" AND VALID = 1", Common.ModifyOrderNumber2(CSV.OrderNumber), product.invSKU);
                        var res = DBProvider.ExecuteQuery(req);
                        if (res != null && res.Tables != null && res.Tables.Count > 0)
                        {
                            var tab = res.Tables[0];
                            if (tab.Rows.Count == 1)
                            {
                                DataRow row = tab.Rows[0];

                                var SKU = (string)row["SKU"];
                                var INV_SKU = (string)row["INV_SKU"];
                                var DESC= (string)row["DESCRIPTION"];

                                StockItem newitem = new StockItem();
                                newitem.State = StockItemState.Paired;
                                newitem.ProductCode = SKU;
                                newitem.Description = DESC;
                                newitem.IsFromDB = true;

                                product.PairProduct = newitem;

                                
                            }
                        }
                    }
                }
            }
        }

        void CompleteOrderItem(InvoiceItem toComplete, StockItem product)
        {
            if (toComplete == null || product == null)
                return;
            // vzajomne prepojenie
            toComplete.PairProduct = product;
            //product.PairProduct = toComplete; // odstranene 3.11.2012 - produkt bude sparovany automaticky v setteri invoiceitemu (predch. riadok)
        }

        internal StockEntity ProcessMessage(FileItem file)
        {
            StockEntity order = null;

            try
            {
                MailItem item = (MailItem)outlook.CreateItemFromTemplate(file.FullFileName, Type.Missing);

                if (file.Type == MSG_TYPE.SPORTS_DIRECT)
                    order = decodeMessage(item.Body, file);
                else if (file.Type == MSG_TYPE.MANDM_DIRECT)
                    order = decodeMandMMessage(item.Body, file);
                else if (file.Type == MSG_TYPE.GETTHELABEL)
                    order = decodeGTLMessage(item.Body, file);

                file.OrderNumber = order.OrderReference;
            }
            catch (System.Exception ex)
            {
                log(ex.Message);
                return order;
            }
            
            return order;
        }

        private StockEntity decodeGTLMessage(string messageBody, FileItem file)
        {
            try
            {
                var order = new StockEntity();
                List<StockItem> items = new List<StockItem>();

                var lines = messageBody.Split(Environment.NewLine.ToCharArray()).Where(s => s != null && s.Trim().Length > 0).ToArray();
                
                // cislo objednavky
                var orderNum = lines.Where(s => s.ToUpper().Contains("ORDER NUMBER:")).FirstOrDefault();
                if (!string.IsNullOrEmpty(orderNum))
                {
                    var from = orderNum.IndexOf(':') + 1;

                    order.OrderReference = orderNum.Substring(from).Trim();
                    order.OurReference = order.OrderReference;
                }

                var relevant = lines.Where(s => s.ToCharArray().Count(c => c == '\t') == 5).ToList();
                file.ProdCount = 0;
                for (int i = 0; i < relevant.Count; i++)
                {
                    string line = relevant[i];
                    if (line.ToLower().StartsWith("item"))
                        continue;

                    var cols = line.Split('\t');

                    StockItem item = new StockItem();
                    item.Description = cols[0];
                    
                    item.Description = item.Description.Remove(0, item.Description.IndexOf(" ") + 1);
                    item.ProductCode = item.Description;
                    //item.ProductCode = item.ProductCode.Remove(0,item.ProductCode.IndexOf(" ")+1);//odstranime prve slovo Mens,Womens.. aby sa dalo porovnať s náazvom v csv
                    item.Ord_Qty = int.Parse(cols[1].Trim());
                    item.Disp_Qty = item.Ord_Qty;
                    item.Price = Common.GetPrice(cols[4]);
                    item.Total = Common.GetPrice(cols[4]) / item.Ord_Qty;   // suma deleno pocet = jednotkova cena
                    item.Currency = "EUR";
                    item.FromFile = file;
                    item.Size = cols[2].Trim();
                    
                    file.ProdCount++;

                    if (item.State == StockItemState.PermanentStorage)
                        item.Sklad = "02";
                    else if (item.State == StockItemState.Waiting)
                        item.Sklad = Properties.Settings.Default.Storage;

                    items.Add(item);
                }

                DecomposeMultipleItems(items);

                order.Items = items.ToArray();

                return order;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), "Error");
            }

            return null;
        }

        private StockEntity decodeMandMMessage(string messageBody, FileItem file)
        {
            try
            {
                var order = new StockEntity();
                List<StockItem> items = new List<StockItem>();

                var lines = messageBody.Split(Environment.NewLine.ToCharArray()).Where(s => s != null && s.Trim().Length > 0).ToArray();
                
                // cislo objednavky
                var orderNum = lines.Where(s => s.Contains("order confirmation number")).FirstOrDefault();
                if (!string.IsNullOrEmpty(orderNum))
                {
                    var from = orderNum.IndexOf(':') + 1;

                    order.OrderReference = orderNum.Substring(from).Trim();
                    order.OurReference = order.OrderReference;
                }

                var relevant = lines.Where(s => s.ToCharArray().Count(c => c == '\t') == 5).ToList();
                file.ProdCount = 0;
                for (int i = 0; i < relevant.Count; i++)
                {
                    string line = relevant[i];
                    if (line.ToLower().StartsWith("item"))
                        continue;

                    var cols = line.Split('\t');

                    StockItem item = new StockItem();
                    item.Description = cols[0];
                    item.ProductCode = item.Description.Split(' ')[0].Trim();
                    item.Size = cols[1].Trim();
                    item.Ord_Qty = int.Parse(cols[2].Trim());
                    item.Disp_Qty = item.Ord_Qty;
                    item.Price = Common.GetPrice(cols[3]);
                    item.Total = Common.GetPrice(cols[4]);
                    item.Currency = cols[3].Substring(0, 1);
                    item.FromFile = file;
                    
                    file.ProdCount++;

                    if (item.State == StockItemState.PermanentStorage)
                        item.Sklad = "02";
                    else if (item.State == StockItemState.Waiting)
                        item.Sklad = Properties.Settings.Default.Storage;

                    items.Add(item);
                }

                DecomposeMultipleItems(items);

                order.Items = items.ToArray();

                return order;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), "Error");
            }

            return null;
        }

        internal CSVFile ProcessCSV(FileItem fromFile)
        {
            CSVFile ret = new CSVFile(fromFile);
            try
            {
                string fileContent = File.ReadAllText(fromFile.FullFileName);

                var lines = fileContent.Split(Environment.NewLine.ToCharArray());

                var csvContent = new List<CSVFileItem>();
                for (int i = 1; i < lines.Count(); i++)
                {
                    if (lines[i].Trim().Length == 0)
                        continue;

                    CSVFileItem item = new CSVFileItem(lines[i]);
                    csvContent.Add(item);
                }
                ret.Items = csvContent.ToArray();
            }
            catch (System.Exception ex)
            {
                log(ex.Message);
                return null;
            }

            return ret;
        }

        internal void btnSelectAll_Click(object sender, EventArgs e)
        {
            var files = dataFiles.DataSource as List<FileItem>;
            foreach (var f in files)
            {
                f.Process = true;
            }

            dataFiles.DataSource = files;
            dataFiles.Refresh();
        }

        internal void btnInverse_Click(object sender, EventArgs e)
        {
            var files = dataFiles.DataSource as List<FileItem>;
            foreach (var f in files)
            {
                f.Process = !f.Process;
            }

            dataFiles.DataSource = files;
            dataFiles.Refresh();
        }

        internal void btnDeselectAll_Click(object sender, EventArgs e)
        {
            var files = dataFiles.DataSource as List<FileItem>;
            foreach (var f in files)
            {
                f.Process = false;
            }

            dataFiles.DataSource = files;
            dataFiles.Refresh();
        }

        /// <summary>
        /// Export vybranej zalozky do XML
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void btn2XML_Click_1(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            
            if (!Directory.Exists(txtOutDir.Text))
                Directory.CreateDirectory(txtOutDir.Text);

            switch (tabData.SelectedIndex)
	        {
                    // invoice
                case (int)Tabs.Invoices:
                    StoreInvoice();
                    break;

                    // stock
                case (int)Tabs.Stocks:
                    StoreStock();
                    break;

		        default:
                    break;
	        }
            Cursor.Current = Cursors.Default;
        }

        const string InvoiceDir = "Invoice";
        void StoreInvoice()
        {
            var invDS = GetInvoiceDS();
            var invItemsDS = GetInvoiceItemsDS();

            if (invDS == null || invItemsDS == null)
                return;

            var outDir = txtOutDir.Text + "/" + InvoiceDir + "/";
            if (!Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);


            dataPackType dp = new dataPackType();
            dp.version = dataPackVersionType.Item20;
            dp.note = "Export ActiveStyle";
            dp.id = "inv_" + DateTime.Now.Ticks + ".xml";
            dp.ico = Properties.Settings.Default.ActiveStyle_ICO;
            dp.application = "MessageImporter";

            List<dataPackItemType> items = new List<dataPackItemType>();
            foreach (var inv in invDS)
            {
                // do exportu pojdu len vybavene objednavky, nestornovane
                if (!inv.Equipped || inv.Cancelled) 
                    continue;

                var orderNr = Common.ModifyOrderNumber(inv.OrderNumber);

                dataPackItemType newDatapack = new dataPackItemType();
                newDatapack.ItemElementName = ItemChoiceType4.invoice;
                newDatapack.id = orderNr;
                newDatapack.version = dataPackItemVersionType.Item20;
                
                // faktura
                invoiceType newInv = new invoiceType();
                newInv.version = invVersionType.Item20;

                // header
                newInv.invoiceHeader = new invoiceHeaderType();
                newInv.invoiceHeader.symVar = orderNr;
                newInv.invoiceHeader.symPar = orderNr;
                newInv.invoiceHeader.invoiceType = invoiceTypeType.issuedInvoice;
                newInv.invoiceHeader.dateAccounting = DateTime.Now;
                newInv.invoiceHeader.dateAccountingSpecified = true;
                newInv.invoiceHeader.dateOrder = DateTime.Parse(inv.OrderDate);
                newInv.invoiceHeader.dateOrderSpecified = true;
                newInv.invoiceHeader.dateTax = DateTime.Now;
                newInv.invoiceHeader.dateTaxSpecified = true;
                newInv.invoiceHeader.dateDue = DateTime.Now.AddDays(Properties.Settings.Default.DueDateAdd);
                newInv.invoiceHeader.dateDueSpecified = true;
                newInv.invoiceHeader.accounting = new accountingType();
                newInv.invoiceHeader.accounting.ids = Properties.Settings.Default.Accounting + GetAccountingSuffix(inv.Country);
                newInv.invoiceHeader.classificationVAT = new classificationVATType();
                newInv.invoiceHeader.classificationVAT.ids = Properties.Settings.Default.ClasifficationVAT;
                newInv.invoiceHeader.classificationVAT.classificationVATType1 = classificationVATTypeClassificationVATType.inland;
                newInv.invoiceHeader.text = inv.TrashNumber + " " + inv.OrderGrandTotal;

                // header->identity
                newInv.invoiceHeader.partnerIdentity = new address();
                newInv.invoiceHeader.partnerIdentity.address1 = new addressType();
                newInv.invoiceHeader.partnerIdentity.address1.name = inv.BillingName;

                newInv.invoiceHeader.partnerIdentity.address1.ico = inv.icoNumber;
                newInv.invoiceHeader.partnerIdentity.address1.dic = inv.dicNumber;

                newInv.invoiceHeader.partnerIdentity.address1.street = inv.BillingStreet;
                newInv.invoiceHeader.partnerIdentity.address1.zip = inv.BillingZip;
                newInv.invoiceHeader.partnerIdentity.address1.phone = inv.BillingPhoneNumber;
                newInv.invoiceHeader.partnerIdentity.address1.city = inv.BillingCity;
                newInv.invoiceHeader.partnerIdentity.address1.company = inv.BillingCompany;
                newInv.invoiceHeader.partnerIdentity.address1.country = new refType();
                newInv.invoiceHeader.partnerIdentity.address1.country.ids = inv.BillingCountry;
                newInv.invoiceHeader.partnerIdentity.address1.email = inv.CustomerEmail;
                newInv.invoiceHeader.partnerIdentity.shipToAddress = new shipToAddressType[1];
                newInv.invoiceHeader.partnerIdentity.shipToAddress[0] = new shipToAddressType();
                newInv.invoiceHeader.partnerIdentity.shipToAddress[0].name = inv.ShippingName;
                newInv.invoiceHeader.partnerIdentity.shipToAddress[0].street = inv.ShippingStreet;
                newInv.invoiceHeader.partnerIdentity.shipToAddress[0].city = inv.ShippingCity;
                newInv.invoiceHeader.partnerIdentity.shipToAddress[0].zip = inv.ShippingZip;
                newInv.invoiceHeader.partnerIdentity.shipToAddress[0].company = inv.ShippingCompany;

                newInv.invoiceHeader.numberOrder = orderNr;
                newInv.invoiceHeader.dateSpecified = true;
                newInv.invoiceHeader.date = DateTime.Now;
                newInv.invoiceHeader.symConst = Properties.Settings.Default.ConstSym;
                newInv.invoiceHeader.note = inv.CustomerEmail;
                newInv.invoiceHeader.intNote = inv.ShippingPhoneNumber;

                newInv.invoiceHeader.paymentType = new paymentType();
                newInv.invoiceHeader.paymentType.ids = inv.OrderPaymentMethod;

                if (inv.Country == Country.Hungary)
                {
                    newInv.invoiceHeader.regVATinEU = new refTypeRegVATinEU();
                    newInv.invoiceHeader.regVATinEU.ids = "HU26912248251";
                }
                newInv.invoiceHeader.account = new accountType();
                newInv.invoiceHeader.account.bankCode = Properties.Settings.Default.BankCode;
                newInv.invoiceHeader.account.ids = Properties.Settings.Default.Bank;

                newInv.invoiceHeader.number = new numberType();
                if (Properties.Settings.Default.UseSkkSerie && inv.Country == Country.Slovakia)
                    newInv.invoiceHeader.number.ids = Properties.Settings.Default.SkkSerie;
                else if (Properties.Settings.Default.UseCzkSerie && inv.Country == Country.CzechRepublic)
                    newInv.invoiceHeader.number.ids = Properties.Settings.Default.CzkSerie;
                else if (Properties.Settings.Default.UseHufSerie && inv.Country == Country.Hungary)
                    newInv.invoiceHeader.number.ids = Properties.Settings.Default.HufSerie;
                else if (Properties.Settings.Default.UsePlnSerie && inv.Country == Country.Poland)
                    newInv.invoiceHeader.number.ids = Properties.Settings.Default.PlnSerie;
                else
                    newInv.invoiceHeader.number = null;

                // polozky faktury
                var invItems = new List<invoiceItemType>();
                foreach (var invItem in inv.InvoiceItems)
                {
                    invoiceItemType xmlItem = new invoiceItemType();

                    var code = "";
                    if (invItem.PairCode != null)
                        code = invItem.PairCode;

                    // zlava
                    var zlava = Common.GetPrice(invItem.Zlava_Pohoda);
                    if (double.IsNaN(zlava))
                        zlava = 0;
                    xmlItem.discountPercentage = (float)zlava;

                    // specialna polozka "cena za dopravu"
                    if (code == Properties.Settings.Default.ShippingCode)
                    {
                        xmlItem.text = invItem.MSG_SKU;
                        xmlItem.quantitySpecified = true;
                        xmlItem.quantity = 1;
                        if (inv.Country == Country.Slovakia)
                        {
                            xmlItem.homeCurrency = new typeCurrencyHomeItem();
                            xmlItem.homeCurrency.unitPriceSpecified = true;
                            xmlItem.homeCurrency.unitPrice = Common.GetPrice(invItem.ItemPrice);
                        }
                        else
                        {
                            xmlItem.foreignCurrency = new typeCurrencyForeignItem();
                            xmlItem.foreignCurrency.unitPriceSpecified = true;
                            xmlItem.foreignCurrency.unitPrice = Common.GetPrice(invItem.ItemPrice);
                        }
                        xmlItem.accounting = new refType();
                        xmlItem.accounting.ids = "2" + GetAccountingSuffix(inv.Country);

                        xmlItem.payVAT = boolean.@true;
                        if (inv.Country == Country.Hungary)
                        {
                            xmlItem.rateVAT = vatRateType.historyHigh;
                            xmlItem.percentVATSpecified = true;
                            xmlItem.percentVAT = 27;
                        }
                        else
                        {
                            xmlItem.rateVAT = vatRateType.high;
                            xmlItem.percentVATSpecified = true;
                            xmlItem.percentVAT = 20;
                        }
                    }
                    else
                    {
                        xmlItem.code = code;
                        xmlItem.text = code;// invItem.ItemName;
                        xmlItem.quantitySpecified = true;
                        float qty = 1;
                        if (!float.TryParse(invItem.ItemQtyOrdered, out qty))
                            qty = 1;
                        xmlItem.quantity = qty;
                        xmlItem.unit = "ks";
                        /*xmlItem.homeCurrency = new typeCurrencyHomeItem();
                        xmlItem.homeCurrency.unitPriceSpecified = true;
                        xmlItem.homeCurrency.unitPrice = Common.GetPrice(invItem.ItemPrice);
                        xmlItem.homeCurrency.priceVATSpecified = true;
                        xmlItem.homeCurrency.priceVAT = Common.GetPrice(invItem.ItemTax);
                        xmlItem.homeCurrency.priceSpecified = true;
                        xmlItem.homeCurrency.price = Common.GetPrice(invItem.ItemTotal) - Common.GetPrice(invItem.ItemDiscount) / 1.2;
                        xmlItem.homeCurrency.priceSumSpecified = true;
                        xmlItem.homeCurrency.priceSum = xmlItem.homeCurrency.price;*/
                        xmlItem.percentVATSpecified = true;
                        xmlItem.percentVAT = Properties.Settings.Default.DPH_percent;

                        xmlItem.payVAT = boolean.@true;
                        if (inv.Country == Country.Hungary)
                        {
                            xmlItem.rateVAT = vatRateType.historyHigh;
                            xmlItem.percentVATSpecified = true;
                            xmlItem.percentVAT = 27;
                        }

                        // stock item
                        xmlItem.stockItem = new stockItemType();
                        xmlItem.stockItem.stockItem = new stockRefType();
                        xmlItem.stockItem.stockItem.ids = code;
                    }

                    // ak su ine krajiny ako SK, vyplnime predajnu cenu do foreignCurrency
                    if (!double.IsNaN(invItem.PredajnaCena))
                    {
                        if (inv.Country != Country.Slovakia)
                        {
                            xmlItem.foreignCurrency = new typeCurrencyForeignItem();
                            xmlItem.foreignCurrency.unitPriceSpecified = true;
                            xmlItem.foreignCurrency.unitPrice = invItem.PredajnaCena;
                        }
                        else
                        {
                            xmlItem.homeCurrency = new typeCurrencyHomeItem();
                            xmlItem.homeCurrency.unitPriceSpecified = true;
                            xmlItem.homeCurrency.unitPrice = invItem.PredajnaCena;
                        }
                    }

                    invItems.Add(xmlItem);
                }
                /*if (inv.OrderShippingMethod.Contains("freeshipping"))
                {
                    // za dopravu sa neplati
                }
                else
                {
                    var shipping = new invoiceItemType();
                    shipping.text = "Cena za dopravu";
                    shipping.quantitySpecified = true;
                    shipping.quantity = 1;
                    shipping.rateVAT = vatRateType.high;
                    shipping.homeCurrency = new typeCurrencyHomeItem();
                    shipping.homeCurrency.unitPriceSpecified = true;
                    shipping.homeCurrency.unitPrice = (Math.Ceiling(Common.GetPrice(inv.OrderShipping) * 1.2 * 100)-1) / 100;

                    invItems.Add(shipping);
                }*/

                newInv.invoiceDetail = invItems.ToArray();

                // specialita pre madarsko
                if (inv.Country == Country.Hungary)
                {
                    newInv.invoiceSummary = new invoiceSummaryType();
                    newInv.invoiceSummary.foreignCurrency = new typeCurrencyForeign();
                    newInv.invoiceSummary.foreignCurrency.currency = new refType();
                    newInv.invoiceSummary.foreignCurrency.currency.ids = "HUF";
                }
                if (inv.Country == Country.CzechRepublic)
                {
                    newInv.invoiceSummary = new invoiceSummaryType();
                    newInv.invoiceSummary.foreignCurrency = new typeCurrencyForeign();
                    newInv.invoiceSummary.foreignCurrency.currency = new refType();
                    newInv.invoiceSummary.foreignCurrency.currency.ids = "CZK";
                }
                if (inv.Country == Country.Poland)
                {
                    newInv.invoiceSummary = new invoiceSummaryType();
                    newInv.invoiceSummary.foreignCurrency = new typeCurrencyForeign();
                    newInv.invoiceSummary.foreignCurrency.currency = new refType();
                    newInv.invoiceSummary.foreignCurrency.currency.ids = "PLN";
                }

                // invoice do datapacku a datapack do vysledneho pola
                newDatapack.Item = newInv;
                items.Add(newDatapack);
            }

            // polozky do xml
            dp.dataPackItem = items.ToArray();

            try
            {
                // ulozenie zmien do xml
                var fname = outDir + dp.id;
                dp.SaveToFile(fname);
                
                if (!ValidateXML(fname))
                {
                    //File.Delete(fname);
                    MessageBox.Show(this, "Validation failed! Generated xml file is not valid!", "Invoice generation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }

            ///////////////////////////////////////////////////////////////////////////////////////////
            // export pre citacku
            var readerItems = new List<ReaderItem>();
            int storeNr = 1;
            foreach (var inv in invDS)
            {
                bool nextStore = true;
                string strStore = storeNr.ToString();
                var orderNr = Common.ModifyOrderNumber(inv.OrderNumber);
                
                if (inv.Cancelled)
                {
                    strStore = "Cancelled";
                    nextStore = false;
                }

                foreach (var invItem in inv.InvoiceItems)
                {
                    if (invItem.PairCode == "shipping")
                        continue;

                    // ulozenie zaznamu pre citacku
                    ReaderItem readerItem = new ReaderItem();
                    readerItem.OrderNr = orderNr;

                    if (!string.IsNullOrEmpty(invItem.PairCode))
                        readerItem.SKU = invItem.PairCode.Replace("/", "");
                    else
                        readerItem.SKU = invItem.invSKU;


                    if (!inv.Cancelled && invItem.PairProduct != null && invItem.PairProduct.State == StockItemState.Waiting)
                    {
                        strStore = "Waiting";
                        nextStore = false;
                        if (!string.IsNullOrEmpty(invItem.PairProduct.WaitingOrderNum))
                            readerItem.OrderNr = invItem.PairProduct.WaitingOrderNum;
                    }
                    else if (!inv.Cancelled && !inv.Equipped && invItem.PairProduct == null)
                    {
                        strStore = "Non paired";
                        nextStore = false;
                    }

                    readerItem.StoreNr = strStore;
                    readerItem.Name = invItem.Parent.CustomerName;
                    if (invItem.MSG_SKU != null)
                        readerItem.ProdName = invItem.MSG_SKU.Trim();
                    /*Fix popisWEB*/
                    //if (invItem.Parent.fromFile != null && invItem.Parent.fromFile.PopisWEB)         if ((invItem.Parent.fromFile != null || invItem.Parent.fromFile.ToString() != "CHYBA_FAKTURA!!!") && invItem.Parent.fromFile.PopisWEB)
                    if (invItem.Parent.fromFile.PopisWEB)
                    {
                        //readerItem.ProdName = string.Empty;// TOTO neviem načo je? vymazať a potom iba možna vyplniť? radšej nechám staré ak by sa nedoplnilo nič 

                        if (invItem.ItemName != null)
                            readerItem.ProdName = invItem.ItemName.Trim();
                    }
                    if (invItem.itemOptions != null)
                        readerItem.Size = invItem.ItemOptions.Replace(";", ",").Trim();
                    readerItem.Valid = 1;
                    readerItem.Note = inv.Note;

                    DBProvider.InsertReaderItem(readerItem);
                    readerItems.Add(readerItem);
                }

                if (nextStore)
                    storeNr++;  // dalsia faktura pojde do dalsieho policka
            }
            StringBuilder readerStrings = new StringBuilder();
            readerStrings.AppendFormat("Store number;Order number;SKU;Customer name;Product name;Size;Note{0}", Environment.NewLine);
            foreach (var item in readerItems)
            {
                readerStrings.AppendFormat("{0};{1};{2};{3};{4};{5};{6}{7}", item.StoreNr, item.OrderNr, item.SKU, item.Name, item.ProdName, item.Size, item.Note, Environment.NewLine);
            }
            File.WriteAllText(outDir + "reader_"+DateTime.Now.ToString("yyyyMMdd_hhmmss")+".csv", readerStrings.ToString());

            MessageBox.Show("Invoice XML generated!", "Save XML", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        static bool ValidationResult = false;
        private bool ValidateXML(string xml)
        {
            // Set the validation settings.
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);

            // schema na validaciu
            settings.Schemas.Add(@"http://www.stormware.cz/schema/version_2/data.xsd", System.Windows.Forms.Application.StartupPath + @"\XSD\data.xsd");

            // Create the XmlReader object.
            XmlReader reader = XmlReader.Create(xml, settings);

            // Parse the file. 
            ValidationResult = true;
            while (reader.Read()) ;

            return ValidationResult;
        }

        // Display any warnings or errors.
        private static void ValidationCallBack(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Warning)
                MessageBox.Show(null, "Validation warning: " + args.Message, "XML Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
                MessageBox.Show(null, "Validation error: " + args.Message, "XML Validation", MessageBoxButtons.OK, MessageBoxIcon.Error);

            ValidationResult = false;
        }

        private string GetAccountingSuffix(Country country)
        {
            string ret = string.Empty;

            switch (country)
            {
                case Country.Unknown:
                    break;
                case Country.Slovakia:
                    break;
                case Country.Hungary:
                    ret = "_HU";
                    break;
                case Country.Poland:
                    ret = "_PL";
                    break;
                case Country.CzechRepublic:
                    ret = "_CZ";
                    break;
                default:
                    break;
            }

            return ret;
        }

        const string StockDir = "Stock";
        void StoreStock()
        {
            //var prodDS = GetProductsDS();
            var prodDS = UniqueStocks();

            if (prodDS == null)
                return;

            var outDir = txtOutDir.Text + "/" + StockDir + "/";
            if (!Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);

            dataPackType dp = new dataPackType();
            dp.version = dataPackVersionType.Item20;
            dp.note = "Export ActiveStyle";
            dp.id = "stock_" + DateTime.Now.Ticks + ".xml";
            dp.ico = Properties.Settings.Default.ActiveStyle_ICO;
            dp.application = "MessageImporter";
            
            List<dataPackItemType> dataPacks = new List<dataPackItemType>();
            List<dataPackItemType> prijemky = new List<dataPackItemType>();
            List<dataPackItemType> invoices = new List<dataPackItemType>();

            List<prijemkaItemType> prijItems = new List<prijemkaItemType>();
            List<invoiceItemType> invItems = new List<invoiceItemType>();

            bool GBP_part = false;
            // referencna polozka
            StockItem refProd = null;

            var allProds = new List<StockItem>();
            //allProds.AddRange(WaitingToUpdate); // waiting nepojdu do stock, iba invoice..
            allProds.AddRange(prodDS);

            /////////////////////////////////////////////// UPDATE POLOZIEK
            /*foreach (var prod in toUpdate)
            {
                if (!prod.EquippedInv && prod.State != StockItemState.Waiting) // do exportu len produkty z vybavenych objednavok
                    continue;

                var code = prod.ProductCode;

                if (string.IsNullOrEmpty(prod.Sklad) || string.IsNullOrEmpty(prod.FictivePrice))
                {
                    MessageBox.Show(this, "Not all 'Sklad' and/or 'Fiktívna cena' are filled! Missing in product with code: " + code, "Missing fields", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                dataPackItemType newDatapack = new dataPackItemType();
                newDatapack.id = code+"_update";
                newDatapack.ItemElementName = ItemChoiceType4.stock;
                newDatapack.version = dataPackItemVersionType.Item20;

                stockType stock = new stockType();
                stock.version = stkVersionType.Item20;

                // header
                stock.stockHeader = new stockHeaderType();
                // zakomentovane kvoli waiting produktom z DB, ktore tuto cenu nemaju
                //stock.stockHeader.purchasingPriceSpecified = true;
                //stock.stockHeader.purchasingPrice = prod.PriceEURnoTax;
                stock.stockHeader.sellingPrice = Common.GetPrice(prod.FictivePrice);
                stock.stockHeader.name = prod.Description;
                stock.stockHeader.nameComplement = prod.SizeInv;
                stock.stockHeader.isSalesSpecified = true;
                stock.stockHeader.isSales = boolean.@true;
                stock.stockHeader.orderName = prod.OrderDate.ToString("dd.MM.yyyy ") + prod.FromFile.ToString();
                stock.stockHeader.isInternetSpecified = true;
                stock.stockHeader.isInternet = boolean.@true;
                stock.stockHeader.sellingRateVAT = vatRateType.high;
                stock.stockHeader.stockTypeSpecified = true;
                stock.stockHeader.stockType = stockTypeType.card;
                stock.stockHeader.code = code;
                stock.stockHeader.typePrice = new refType();
                stock.stockHeader.typePrice.ids = Properties.Settings.Default.TypePrice;
                stock.stockHeader.storage = new refTypeStorage();
                stock.stockHeader.storage.ids = prod.Sklad;

                // action type - update
                stock.actionType = new actionTypeType1();
                stock.actionType.Item = new requestStockType();
                stock.actionType.ItemElementName = ItemChoiceType3.update;
                stock.actionType.Item.filter = new filterStocksType();
                stock.actionType.Item.filter.code = code;

                newDatapack.Item = stock;
                dataPacks.Add(newDatapack);
            }*/
            /////////////////////////////////////////////// STORE POLOZIEK                
            
            /*Fix pre opakujuce sa order number (alternativa by boli maily)*/
            foreach (var inv in AllInvoices)
            {
                foreach (var invItem in inv.InvoiceItems)
                {
                    if (invItem.FromDB)
                    {
                      //  var orderNum = (string.IsNullOrEmpty(invItem.Parent.OrderNumber.WaitingOrderNum) ? Common.ModifyOrderNumber2(prod.PairProduct.Parent.OrderNumber) : prod.WaitingOrderNum);
                        var update = string.Format("UPDATE {0} SET VALID = \"-1\" WHERE ORDER_NUMBER=\"{1}\" AND INV_SKU = \"{2}\"", DBProvider.T_WAIT_STOCK, invItem.Parent.OrderNumber,invItem.invSKU);//prod.PairProduct.invSKU
                        DBProvider.ExecuteNonQuery(update);
                    }
                }
            }

            foreach (var prod in allProds)
            {
                if (!prod.EquippedInv && prod.State != StockItemState.Waiting) // do exportu len produkty z vybavenych objednavok
                    continue;

                // referencny produkt bude prvy korektny
                if (refProd == null)
                    refProd = prod;

                /////////////////////////////////////////////////// stock item

                //var code = prod.ProductCode.Replace("/", "");
                var code = prod.ProductCode;

                if (string.IsNullOrEmpty(prod.Sklad) || string.IsNullOrEmpty(prod.FictivePrice))
                {
                    MessageBox.Show(this, "Not all 'Sklad' and/or 'Fiktívna cena' are filled! Missing in product with code: "+code, "Missing fields", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                dataPackItemType newDatapack = new dataPackItemType();
                newDatapack.id = code;
                newDatapack.ItemElementName = ItemChoiceType4.stock;
                newDatapack.version = dataPackItemVersionType.Item20;

                stockType stock = new stockType();
                stock.version = stkVersionType.Item20;

                // defaultna akcia add
                stock.actionType = new actionTypeType1();
                stock.actionType.Item = new requestStockType();
                stock.actionType.Item.add = boolean.@true;
                stock.actionType.ItemElementName = ItemChoiceType3.update;
                // filter pridany 13.3.2014
                stock.actionType.Item.filter = new filterStocksType();
                stock.actionType.Item.filter.code = code;
                stock.actionType.Item.filter.store = new refType();
                stock.actionType.Item.filter.store.ids = prod.Sklad;

                // header
                stock.stockHeader = new stockHeaderType();
                stock.stockHeader.stockTypeSpecified = true;
                stock.stockHeader.stockType = stockTypeType.card;
                stock.stockHeader.code = code;
                stock.stockHeader.EAN = code;
                stock.stockHeader.isSalesSpecified = true;
                stock.stockHeader.isSales = boolean.@true;
                stock.stockHeader.isInternetSpecified = true;
                stock.stockHeader.isInternet = boolean.@true;
                stock.stockHeader.name = prod.Description;
                stock.stockHeader.nameComplement = prod.ItemNameInv;
                stock.stockHeader.unit = "ks";
                stock.stockHeader.description = prod.Description;
                stock.stockHeader.description2 = prod.ItemNameInv;

                stock.stockHeader.storage = new refTypeStorage();
                stock.stockHeader.storage.ids = prod.Sklad;


                if (prod.State == StockItemState.Waiting)
                {
                    try
                    {
                        var orderNum = (string.IsNullOrEmpty(prod.WaitingOrderNum) ? Common.ModifyOrderNumber2(prod.PairProduct.Parent.OrderNumber) : prod.WaitingOrderNum);
                        var update = string.Format("UPDATE {0} SET ORDER_NUMBER = \"{1}\" WHERE INV_SKU=\"{2}\"", DBProvider.T_WAIT_STOCK, orderNum, prod.PairProduct.invSKU);
                        DBProvider.ExecuteNonQuery(update);

                        // ulozenie produktu do DB
                        var insert = string.Format("INSERT INTO " + DBProvider.T_WAIT_STOCK + " VALUES ({0},\"{1}\",\"{2}\",\"{3}\",\"{4}\",{5})", "null", orderNum, prod.PairProduct.invSKU, prod.ProductCode, prod.Description, 1);
                        log(insert);
                        DBProvider.ExecuteNonQuery(insert);
                        if (prod.PairProduct != null)
                            DBProvider.InsertWaitingInvoice(prod.PairProduct);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show(this, "Exception during inserting waiting product into database: " + ex.ToString(), "Execute insert", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                stock.stockHeader.typePrice = new refType();
                stock.stockHeader.typePrice.ids = Properties.Settings.Default.TypePrice;

                stock.stockHeader.purchasingPriceSpecified = true;
                stock.stockHeader.purchasingPrice = prod.PriceEURnoTax;
                stock.stockHeader.sellingPrice = Common.GetPrice(prod.SellPriceEUR);
                stock.stockHeader.limitMin = 0;
                stock.stockHeader.limitMax = 0;
                stock.stockHeader.orderName = prod.OrderDate.ToString("dd.MM.yyyy ") + prod.FromFile.ToString();
                stock.stockHeader.orderQuantitySpecified = true;
                stock.stockHeader.orderQuantity = prod.Ord_Qty;
                stock.stockHeader.shortName = code;
                stock.stockHeader.guaranteeType = guaranteeTypeType.year;
                stock.stockHeader.guaranteeTypeSpecified = true;
                stock.stockHeader.guarantee = "2";

                stock.stockHeader.yield = "604000";

                //stock.stockHeader.note = prod.FictivePrice;
                stock.stockHeader.sellingPrice = Common.GetPrice(prod.FictivePrice);
                
                stock.stockHeader.nameComplement = prod.SizeInv;

                stock.stockHeader.sellingRateVAT = vatRateType.high;

                stock.stockHeader.acc = "132100";

                newDatapack.Item = stock;
                dataPacks.Add(newDatapack);

                /////////////////////////////////////////////////// prijemky polozky
                prijemkaItemType prijItem = new prijemkaItemType();
                prijItem.code = code;
                /*prijItem.quantitySpecified = true;
                prijItem.quantity = prod.Disp_Qty;
                prijItem.unit = "ks";
                prijItem.stockItem = new stockItemType();
                prijItem.stockItem.stockItem = new stockRefType();
                prijItem.stockItem.stockItem.ids = code;*/
                prijItems.Add(prijItem);

                /////////////////////////////////////////////////// faktura polozky
                invoiceItemType xmlItem = new invoiceItemType();
                xmlItem.code = code;
                xmlItem.text = code;
                xmlItem.stockItem = new stockItemType();
                xmlItem.stockItem.stockItem = new stockRefType();
                xmlItem.stockItem.stockItem.ids = code;
                xmlItem.quantitySpecified = true;
                xmlItem.quantity = prod.Disp_Qty;

                invItems.Add(xmlItem);

                // GBP cast pojde ak existuje subor MSG s kurzom inym ako 1
                if (!GBP_part && prod.FromFile.ExchRate != 1.0)
                    GBP_part = true;
            }

            var ticks = DateTime.Now.Ticks;

            // zabalenie prijemok
            dataPackItemType prijmekaDatapack = new dataPackItemType();
            prijmekaDatapack.id = "prijemka_" + ticks;
            prijmekaDatapack.ItemElementName = ItemChoiceType4.prijemka;
            prijmekaDatapack.version = dataPackItemVersionType.Item20;
            prijemkaType prijemka = new prijemkaType();
            prijemka.version = priVersionType.Item20;
            prijemka.prijemkaHeader = new prijemkaHeaderType();
            prijemka.prijemkaDetail = prijItems.ToArray();
            prijemka.prijemkaSummary = new prijemkaSummaryType();
            prijmekaDatapack.Item = prijemka;
            prijemky.Add(prijmekaDatapack);

            /////////////////////////////////////////////////// invoice
            dataPackItemType invDatapack = new dataPackItemType();
            invDatapack.id = "invoice_" + ticks;
            invDatapack.ItemElementName = ItemChoiceType4.invoice;
            invDatapack.version = dataPackItemVersionType.Item20;
            invoiceType newInv = new invoiceType();
            newInv.version = invVersionType.Item20;
            newInv.invoiceHeader = new invoiceHeaderType();
            newInv.invoiceHeader.invoiceType = invoiceTypeType.receivedInvoice;
            newInv.invoiceHeader.dateAccounting = DateTime.Now;
            newInv.invoiceHeader.dateAccountingSpecified = true;
            newInv.invoiceHeader.dateOrder = DateTime.Now;
            newInv.invoiceHeader.dateOrderSpecified = true;
            newInv.invoiceHeader.dateTax = DateTime.Now;
            newInv.invoiceHeader.dateTaxSpecified = true;
            newInv.invoiceHeader.dateDue = DateTime.Now.AddDays(Properties.Settings.Default.DueDateAdd);
            newInv.invoiceHeader.dateDueSpecified = true;
            newInv.invoiceHeader.accounting = new accountingType();
            newInv.invoiceHeader.accounting.ids = "1 GBP";
            newInv.invoiceHeader.classificationVAT = new classificationVATType();
            newInv.invoiceHeader.classificationVAT.ids = "PDnadEU";
            newInv.invoiceHeader.classificationVAT.classificationVATType1 = classificationVATTypeClassificationVATType.inland;
            newInv.invoiceHeader.text = refProd.FromFile.Type.ToString() + "_" + (allMessages.Count > 0 ? allMessages[0].OrderReference : "<err>");
            newInv.invoiceHeader.partnerIdentity = new address();
            switch (refProd.FromFile.Type)
            {
                case MSG_TYPE.SPORTS_DIRECT:
                    newInv.invoiceHeader.partnerIdentity.id = Properties.Settings.Default.PartnerSports;
                    break;
                case MSG_TYPE.MANDM_DIRECT:
                    newInv.invoiceHeader.partnerIdentity.id = Properties.Settings.Default.PartnerMandM;
                    break;
                case MSG_TYPE.GETTHELABEL:
                    newInv.invoiceHeader.partnerIdentity.id = Properties.Settings.Default.PartnerLabel;
                    break;
                default:
                    newInv.invoiceHeader.partnerIdentity.id = "24";
                    break;
            }
          
            // polozky z faktury.. zatial fiktivne
            if (refProd.FromFile != null)
            {
                newInv.invoiceHeader.symVar = refProd.FromFile.OrderNumber;
                newInv.invoiceHeader.symPar = refProd.FromFile.OrderNumber;
            }
            newInv.invoiceHeader.numberOrder = "numOrder";
            newInv.invoiceHeader.dateSpecified = true;
            newInv.invoiceHeader.date = DateTime.Now;
            newInv.invoiceHeader.paymentType = new paymentType();
            newInv.invoiceHeader.paymentType.ids = "cashondelivery";
                        
            newInv.invoiceDetail = invItems.ToArray();

            if (GBP_part) 
            {
                newInv.invoiceSummary = new invoiceSummaryType();
                newInv.invoiceSummary.foreignCurrency = new typeCurrencyForeign();
                newInv.invoiceSummary.foreignCurrency.currency = new refType();
                newInv.invoiceSummary.foreignCurrency.currency.ids = "GBP";
            }

            // naplnenie cudzej meny ak je zadana
            if (refProd.FromFile != null && !string.IsNullOrEmpty(refProd.FromFile.Currency))
            {
                if (newInv.invoiceSummary == null)
                {
                    newInv.invoiceSummary = new invoiceSummaryType();
                    newInv.invoiceSummary.foreignCurrency = new typeCurrencyForeign();
                    newInv.invoiceSummary.foreignCurrency.currency = new refType();
                }
                newInv.invoiceSummary.foreignCurrency.currency.ids = refProd.FromFile.Currency;
            }

            invDatapack.Item = newInv;
            invoices.Add(invDatapack);

            // polozky do xml
            //dataPacks.AddRange(prijemky); // 6.11.2012 prijemky nejdu do exportu
            dataPacks.AddRange(invoices);

            // datapack doprava pre faktury zo sportsdirect
            dataPackItemType shippingDatapack = null;
            if (refProd.FromFile.Type == MSG_TYPE.SPORTS_DIRECT)
            {
                shippingDatapack = new dataPackItemType();
                shippingDatapack.id = "shipping_" + ticks;
                shippingDatapack.ItemElementName = ItemChoiceType4.invoice;
                shippingDatapack.version = dataPackItemVersionType.Item20;
                newInv = new invoiceType();
                newInv.version = invVersionType.Item20;
                newInv.invoiceHeader = new invoiceHeaderType();
                newInv.invoiceHeader.invoiceType = invoiceTypeType.receivedInvoice;
                newInv.invoiceHeader.dateAccounting = DateTime.Now;
                newInv.invoiceHeader.dateAccountingSpecified = true;
                newInv.invoiceHeader.dateOrder = DateTime.Now;
                newInv.invoiceHeader.dateOrderSpecified = true;
                newInv.invoiceHeader.dateTax = DateTime.Now;
                newInv.invoiceHeader.dateTaxSpecified = true;
                newInv.invoiceHeader.dateDue = DateTime.Now.AddDays(Properties.Settings.Default.DueDateAdd);
                newInv.invoiceHeader.dateDueSpecified = true;
                newInv.invoiceHeader.accounting = new accountingType();
                newInv.invoiceHeader.accounting.ids = "1 GBP";
                newInv.invoiceHeader.classificationVAT = new classificationVATType();
                newInv.invoiceHeader.classificationVAT.ids = "PD";
                newInv.invoiceHeader.classificationVAT.classificationVATType1 = classificationVATTypeClassificationVATType.inland;
                newInv.invoiceHeader.text = "SportsDirect_doprava_" + (allMessages.Count > 0 ? allMessages[0].OrderReference : "<err>");
                newInv.invoiceHeader.partnerIdentity = new address();
                newInv.invoiceHeader.partnerIdentity.id = "23";

                // polozky z faktury.. zatial fiktivne
                if (refProd.FromFile != null)
                {
                    newInv.invoiceHeader.symVar = refProd.FromFile.OrderNumber;
                    newInv.invoiceHeader.symPar = refProd.FromFile.OrderNumber;
                }
                newInv.invoiceHeader.numberOrder = "numOrder";
                newInv.invoiceHeader.dateSpecified = true;
                newInv.invoiceHeader.date = DateTime.Now;
                newInv.invoiceHeader.paymentType = new paymentType();
                newInv.invoiceHeader.paymentType.ids = "cashondelivery";
                // detail
                List<invoiceItemType> details = new List<invoiceItemType>();
                invoiceItemType xmlItem = new invoiceItemType();
                xmlItem.text = "doprava";
                xmlItem.quantity = 1;
                xmlItem.quantitySpecified = true;
                xmlItem.homeCurrency = new typeCurrencyHomeItem();
                xmlItem.homeCurrency.unitPriceSpecified = true;
                xmlItem.homeCurrency.unitPrice = refProd.FromFile.Delivery;
                details.Add(xmlItem);
                newInv.invoiceDetail = details.ToArray();

                shippingDatapack.Item = newInv;
            }
            if (shippingDatapack != null)
                dataPacks.Add(shippingDatapack);
            // datapack doprava

            dp.dataPackItem = dataPacks.ToArray();

            try
            {
                // ulozenie zmien do xml
                var fname = outDir + dp.id;
                dp.SaveToFile(fname);

                if (!ValidateXML(fname))
                {
                    //File.Delete(fname);
                    MessageBox.Show(this, "Validation failed! Generated xml file is not valid!", "Stock generation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }

            MessageBox.Show("Stock XML(s) generated!", "Save XML", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        double ToNumber(string x)
        {
            return double.Parse(new string(x.ToCharArray().Where(c => "1234567890.,".Contains(c)).ToArray()));
        }

        internal void InvoiceChanged(object sender, DataGridViewCellEventArgs e)
        {
            var items = GetInvoiceDS();

            if (items != null && items[e.RowIndex].InvoiceItems != null)
                SetInvoiceItemsDS(new BindingList<InvoiceItem>(items[e.RowIndex].InvoiceItems));
            else
                // prazdny zoznam poloziek 
                SetInvoiceItemsDS(new BindingList<InvoiceItem>());
        }

        internal void CheckAllEqipped()
        {
            var allInvoices = GetInvoiceDS();
            if (allInvoices == null)
                return;

            foreach (var item in allInvoices)
            {
                if (item.InvoiceItems == null)
                    continue;

                // objednavka je vybavena ak ma vsetky produkty priradene 
                item.Equipped = Common.IsEquipped(item);
            }
        }

        internal void CheckEqipped(Invoice inv)
        {
            if (inv == null)
                return;

            // objednavka je vybavena ak ma vsetky produkty priradene
            inv.Equipped = Common.IsEquipped(inv);
        }

        internal void UpdateProductSet()
        {
            var allProducts = GetProductsDS();
            var allInvoices = GetInvoiceDS();
            if (allProducts == null || allInvoices == null)
                return;
            
            var paired = allInvoices.Where(i => i.InvoiceItems != null).SelectMany(inv => inv.InvoiceItems).Where(i => Common.IsItemPaired(i)).Select(ii => ii.PairProduct.ProductCode).ToList();
         
            lbNonPaired.Items.Clear();
            //allProducts.Where(p => !paired.Contains(p.ProductCode) && p.ProductCode != null).ToList().ForEach(i => lbNonPaired.Items.Add(i.ProductCode));
            allProducts.Where(p => p.PairProduct == null && p.ProductCode != null).ToList().ForEach(i => lbNonPaired.Items.Add(i.ProductCode));
            lblUnpiredCount.Text = lbNonPaired.Items.Count.ToString() + " unpaired items";
        }

        internal void btnInvoiceAdd_Click(object sender, EventArgs e)
        {
            var ds = GetInvoiceDS();
            if (ds == null)
                return;
            var added = ds.AddNew();
        }

        internal void btnInvoiceRemove_Click(object sender, EventArgs e)
        {
            var selCell = gridInvoices.SelectedCells;
            if (selCell != null && selCell.Count > 0)
            {
                if (MessageBox.Show(this, "Really delete?", "Remove items", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;

                var selItem = gridInvoices.Rows[selCell[0].RowIndex].DataBoundItem as Invoice;

                var ds = GetInvoiceDS();
                if (ds == null)
                    return;
                ds.Remove(selItem);
            }

            CheckAllEqipped();
            UpdateProductSet();
        }

        internal void btnAssignProd_Click(object sender, EventArgs e)
        {
            var selprodcode = lbNonPaired.SelectedItem as string;
            if (tabItems.SelectedIndex == 1)
                selprodcode = lbFilteredItems .SelectedItem as string;
            if (selprodcode == null || selprodcode.Length == 0)
                return;

            var selCells = gridInvItems.SelectedCells;
            if (selCells != null && selCells.Count > 0)
            {
                var selItem = selCells[0];
                var dsMSG = GetProductsDS();
                if (dsMSG == null)
                    return;
                var selProd = dsMSG.Where(o => o.ProductCode == selprodcode && o.PairProduct == null).ToArray()[0];//
                
                var ds = GetInvoiceItemsDS();
                if (ds == null)
                    return;
                var selInv = ds[selItem.RowIndex];

                if (selInv.PairProduct != null)
                {
                    if (selInv.PairProductStack == null)
                        selInv.PairProductStack= new List<StockItem>();
                    selInv.PairProductStack.Add(selProd);
                    selProd.PairProduct = selInv;
                   // selInv.ItemQtyOrdered =   Convert.ToString(Convert.ToInt32(selInv.ItemQtyOrdered) + 1);
                }
                else
                    selInv.PairProduct = selProd;

                CheckEqipped(selInv.Parent);
                UpdateProductSet();
                RefreshTab();
            }
        }

        internal void btnInvoiceItemNew_Click(object sender, EventArgs e)
        {
            var allInvoices = GetInvoiceDS();
            var ds = GetInvoiceItemsDS();
            if (ds == null)
                return;
            var added = new InvoiceItem();
            added.ItemDiscount = "0.00";
            added.ItemQtyOrdered = "1";

            if (ds.Count > 0)
                ds.Insert(ds.Count-1, added);
            else
                ds.Insert(0, added);

            var selcells = gridInvoices.SelectedCells;
            if (selcells != null && selcells.Count > 0)
            {
                var item = allInvoices[selcells[0].RowIndex];
                added.Parent = item;
            }

            CheckAllEqipped();
            RefreshTab();
        }

        internal void btnInvoiceItemRemove_Click(object sender, EventArgs e)
        {
            var selCell = gridInvItems.SelectedCells;
            if (selCell != null && selCell.Count > 0)
            {
                if (MessageBox.Show(this, "Really delete?", "Remove items", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;

                var selItem = gridInvItems.Rows[selCell[0].RowIndex].DataBoundItem as InvoiceItem;

                var ds = GetInvoiceItemsDS();
                ds.Remove(selItem);
            }

            CheckAllEqipped();
            UpdateProductSet();
        }

        internal void btnAddMsg_Click(object sender, EventArgs e)
        {
            var ds = GetProductsDS();
            if (ds == null)
                return;
            var added = ds.AddNew();
            ds.EndNew(ds.IndexOf(added));
            if (added == null)
                return;
            AllStocks.Add(added);

            CheckAllEqipped();
            UpdateProductSet();
        }

        internal void btnRemoveMSG_Click(object sender, EventArgs e)
        {
            var selCell = gridStocks.SelectedCells;
            if (selCell != null && selCell.Count > 0)
            {
                if (MessageBox.Show(this, "Really delete?", "Remove items", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;

                var selItem = gridStocks.Rows[selCell[0].RowIndex].DataBoundItem as StockItem;

                Unpair(selItem);
                
                var ds = GetProductsDS();
                if (ds == null)
                    return;
                ds.Remove(selItem);
            }

            CheckAllEqipped();
            UpdateProductSet();
        }

        internal void Unpair(StockItem orderItem)
        {
            var allInvoices = GetInvoiceDS();
            var paired = allInvoices.Where(i => i.InvoiceItems != null).SelectMany(i => i.InvoiceItems).Where(ii => ii.PairProduct != null && ii.PairProduct.ProductCode == orderItem.ProductCode).ToList();
            foreach (var order in paired)
            {
                order.PairProduct = null;
            }
        }

        internal void TabChanged(object sender, EventArgs e)
        {
            UpdateProductSet();
            RefreshTab();
        }

        internal void btnUnpairInvoiceItem_Click(object sender, EventArgs e)
        {
            var selCell = gridInvItems.SelectedCells;
            if (selCell != null && selCell.Count > 0)
            {
                var selItem = gridInvItems.Rows[selCell[0].RowIndex].DataBoundItem as InvoiceItem;

                selItem.PairProduct = null;
                if (selItem.PairProductStack != null)
                {
                    for (int i = 0; i < selItem.PairProductStack.Count(); i++)
                    {
                        selItem.PairProductStack.ElementAt(i).PairProduct = null;
                    //    selItem.PairProductStack.ElementAt(i).SizeInv = null;
                    }
                    selItem.PairProductStack.Clear();
                }
            }

            CheckAllEqipped();
            UpdateProductSet();
            RefreshTab();
        }

        internal void btnUnpairAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < gridInvItems.RowCount; i++)
            {
                var selItem = gridInvItems.Rows[i].DataBoundItem as InvoiceItem;

                selItem.PairProduct = null;
            }

            CheckAllEqipped();
            UpdateProductSet();
            RefreshTab();
        }

        internal void btnUnpairProductMSG_Click(object sender, EventArgs e)
        {
            var selCell = gridStocks.SelectedCells;
            if (selCell != null && selCell.Count > 0)
            {
                var selItem = gridStocks.Rows[selCell[0].RowIndex].DataBoundItem as StockItem;

                Unpair(selItem);
            }

            CheckAllEqipped();
            UpdateProductSet();

            RefreshTab();
        }

        internal void btnUnpairAllMSG_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < gridStocks.RowCount; i++)
            {
                var selItem = gridStocks.Rows[i].DataBoundItem as StockItem;

                Unpair(selItem);
            }

            CheckAllEqipped();
            UpdateProductSet();

            RefreshTab();
        }

        internal void btnExportMSG_Click(object sender, EventArgs e)
        {
            Export exporter = new Export(GetProductsDS());
            exporter.ShowDialog(this);

            var remove = exporter.ResultDelItems;
            if (remove != ResultRemoving.None)
            {
                RemoveMSG(remove);
            }
        }

        void RemoveMSG(ResultRemoving result)
        {
            var ds = GetProductsDS();
            if (ds == null)
                return;

            while(true)
            {
                var found = ds.Where(msg => (result == ResultRemoving.Selected && msg.EquippedInv) ||
                     (result == ResultRemoving.Unselected && !msg.EquippedInv) ||
                     (result == ResultRemoving.All)).ToList();

                if (found.Count == 0)
                    break;

                Unpair(found[0]);
                ds.Remove(found[0]);
            }

            CheckAllEqipped();
            UpdateProductSet();
        }

        internal void dataFiles_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            var ds = dataFiles.DataSource as List<FileItem>;
            if (ds == null)
                return;

            switch (e.ColumnIndex)
            {
                    // process?
                case 1:
                    break;
                    // nazov
                case 2:
                    break;
                    // order date
                case 4:
                    OrderdateChanged(ds[e.RowIndex]);
                    break;
                // kurz
                // delivery price
                case 3:
                case 5:
                    CalcBuyingPrice(GetProductsDS());
                    RateChanged(ds[e.RowIndex]);
                    gridStocks.Refresh();
                    gridInvoices.Refresh();
                    gridInvItems.Refresh();
                    break;

                default:
                    break;
            }

            dataFiles.Refresh();
        }

        void RateChanged(FileItem file)
        {
            double kurz = file.ExchRate;

            if (!double.IsNaN(kurz))
            {
                var ds = GetProductsDS();
                if (ds == null)
                    return;

                var list = ds.Where(oi => oi.FromFile == file).ToList();

                foreach (var item in list)
                {
                    item.PriceEURnoTax = Math.Round(item.Price * kurz, 2);
                    item.TotalEUR = Math.Round(item.Total * kurz, 2);
                    item.PriceWithDeliveryEUR = Math.Round(item.PriceWithDelivery * kurz, 2);
                }

                RefreshTab();
            }
            else
            {
                log("Exchange rate is not a number!");
                MessageBox.Show(this, "Exchange rate is not a number!", "Error");
            }
        }

        void OrderdateChanged(FileItem file)
        {
            var ds = GetProductsDS();
            if (ds == null)
                return;

            var list = ds.Where(oi => oi.FromFile == file).ToList();

            foreach (var item in list)
            {
                item.OrderDate = file.OrderDate;
            }

            RefreshTab();
        }

        void CalcRateOrderdate(List<FileItem> files)
        {
            foreach (var file in files)
            {
                double kurz = file.ExchRate;

                if (!double.IsNaN(kurz))
                {
                    var ds = GetProductsDS();
                    if (ds == null)
                        return;

                    var list = ds.Where(oi => oi.FromFile == file).ToList();

                    foreach (var item in list)
                    {
                        item.PriceEURnoTax = Math.Round(item.Price * kurz, 2);
                        item.TotalEUR = Math.Round(item.Total * kurz, 2);
                        item.PriceWithDeliveryEUR = Math.Round(item.PriceWithDelivery * kurz, 2);
                        item.OrderDate = file.OrderDate;
                    }
                }
                else
                {
                    RefreshTab();
                    log("Exchange rate is not a number!");
                    MessageBox.Show(this, "Exchange rate is not a number! (file: '"+file.FileName+"')", "Error");
                    return;
                }
            }

            RefreshTab();
        }

        internal void InvoiceValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            RefreshTab();
        }

        internal void StockValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            RefreshTab();
        }

        internal void InvoiceItemSelChanged(object sender, EventArgs e)
        {
            var all = lbNonPaired.Items;
            var selCells = gridInvItems.SelectedCells;
            if (selCells == null || selCells.Count == 0)
                return;

            var ds = GetInvoiceItemsDS();
            if (ds == null)
                return;
            var selInv = ds[selCells[0].RowIndex];

            lbFilteredItems.Items.Clear();
            foreach (var code in all)
            {
                var conv = ConvertInvoiceItem(selInv.invSKU);
                if (conv != null && code.ToString().Contains(conv))
                    lbFilteredItems.Items.Add(code.ToString());
            }
        }

        private void btnSettingsLoad_Click(object sender, EventArgs e)
        {
            var prop = Properties.Settings.Default;

            txtSettingsIco.Text = prop.ActiveStyle_ICO;
            txtSettingsProdLength.Text = prop.SubProductLength.ToString();

            // exchange rates
            txtSetCzkEx.Text = prop.ExRateCzk;
            txtSetHufEx.Text = prop.ExRateHuf;
            txtSetPlnEx.Text = prop.ExRatePln;
            txtSetExRatePath.Text = prop.ExchRateXMLAddress;

            // shipping
            txtSetSkkText.Text = prop.ShipTextSkk;
            txtSetCzkText.Text = prop.ShipTextCzk;
            txtSetHufText.Text = prop.ShipTextHuf;
            txtSetPlnText.Text = prop.ShipTextPln;
            txtSetSkkPrice.Text = prop.ShipPriceSkk;
            txtSetCzkPrice.Text = prop.ShipPriceCzk;
            txtSetHufPrice.Text = prop.ShipPriceHuf;
            txtSetPlnPrice.Text = prop.ShipPricePln;

            // tax
            txtSetSkkTax.Text = prop.TaxSkk;
            txtSetCzkTax.Text = prop.TaxCzk;
            txtSetHufTax.Text = prop.TaxHuf;
            txtSetPlnTax.Text = prop.TaxPln;

            // partner identity
            txtPartnerSport.Text = prop.PartnerSports;
            txtPartnerMandM.Text = prop.PartnerMandM;
            txtPartnerLabel.Text = prop.PartnerLabel;

            // number series
            chkSkkSerie.Checked = prop.UseSkkSerie;
            chkCzkSerie.Checked = prop.UseCzkSerie;
            chkHufSerie.Checked = prop.UseHufSerie;
            chkPlnSerie.Checked = prop.UsePlnSerie;
            txtSkkSerie.Text = prop.SkkSerie;
            txtCzkSerie.Text = prop.CzkSerie;
            txtHufSerie.Text = prop.HufSerie;
            txtPlnSerie.Text = prop.PlnSerie;

            txtSetDefStorage.Text = prop.Storage;
        }

        private void btnSettingsSave_Click(object sender, EventArgs e)
        {
            var prop = Properties.Settings.Default;

            prop.ActiveStyle_ICO = txtSettingsIco.Text;
            prop.SubProductLength = int.Parse(txtSettingsProdLength.Text);

            // exchange rates
            prop.ExRateCzk = Common.CleanPrice(txtSetCzkEx.Text);
            prop.ExRateHuf = Common.CleanPrice(txtSetHufEx.Text);
            prop.ExRatePln = Common.CleanPrice(txtSetPlnEx.Text);
            prop.ExchRateXMLAddress = txtSetExRatePath.Text;

            // shipping
            prop.ShipTextSkk = txtSetSkkText.Text;
            prop.ShipTextCzk = txtSetCzkText.Text;
            prop.ShipTextHuf = txtSetHufText.Text;
            prop.ShipTextPln = txtSetPlnText.Text;
            prop.ShipPriceSkk = Common.CleanPrice(txtSetSkkPrice.Text);
            prop.ShipPriceCzk = Common.CleanPrice(txtSetCzkPrice.Text);
            prop.ShipPriceHuf = Common.CleanPrice(txtSetHufPrice.Text);
            prop.ShipPricePln = Common.CleanPrice(txtSetPlnPrice.Text);

            // tax
            prop.TaxSkk = Common.CleanPrice(txtSetSkkTax.Text);
            prop.TaxCzk = Common.CleanPrice(txtSetCzkTax.Text);
            prop.TaxHuf = Common.CleanPrice(txtSetHufTax.Text);
            prop.TaxPln = Common.CleanPrice(txtSetPlnTax.Text);

            // partner identity
            prop.PartnerSports = txtPartnerSport.Text;
            prop.PartnerMandM = txtPartnerMandM.Text;
            prop.PartnerLabel = txtPartnerLabel.Text;

            // number series
            prop.UseSkkSerie = chkSkkSerie.Checked;
            prop.UseCzkSerie = chkCzkSerie.Checked;
            prop.UseHufSerie = chkHufSerie.Checked;
            prop.UsePlnSerie = chkPlnSerie.Checked;
            prop.SkkSerie = txtSkkSerie.Text;
            prop.CzkSerie = txtCzkSerie.Text;
            prop.HufSerie = txtHufSerie.Text;
            prop.PlnSerie = txtPlnSerie.Text;   

            prop.Storage = txtSetDefStorage.Text;

            prop.Save();
        }

        private void btnReplaceReload_Click(object sender, EventArgs e)
        {
            var fName = System.Windows.Forms.Application.StartupPath + "\\Resources\\replacements.txt";
            try
            {
                var lines = File.ReadAllLines(fName);
                List<ReplacementPair> ds = new List<ReplacementPair>();

                foreach (var line in lines)
                {
                    var trimLine = line.Trim();

                    if (trimLine.StartsWith("//"))
                        continue;

                    int eqIndex = trimLine.IndexOf('=');
                    if (eqIndex == -1)
                        continue;

                    var str1 = trimLine.Substring(0, eqIndex);
                    var str2 = trimLine.Substring(eqIndex + 1);

                    ds.Add(new ReplacementPair(str1, str2));
                }

                gridReplacements.DataSource = new BindingList<ReplacementPair>(ds);

                // nastavenie glovalneho objektu do StockItem triedy
                StockItem.Replacements = gridReplacements.DataSource as BindingList<ReplacementPair>;
                gridStocks.Refresh();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), "Error while reading replacements.txt from resources directory!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnReplaceSave_Click(object sender, EventArgs e)
        {
            var ds = gridReplacements.DataSource as BindingList<ReplacementPair>;
            if (ds == null)
                return;
            List<string> lines = new List<string>();

            foreach (var item in ds)
            {
                lines.Add(item.ValueToFind + "=" + item.ValueToReplace);
            }

            try
            {
                var fName = System.Windows.Forms.Application.StartupPath + "\\Resources\\replacements.txt";
                File.WriteAllLines(fName, lines.ToArray());
                
                btnReplaceReload.PerformClick();

                MessageBox.Show(this, "Replacements.txt successfully saved in Resources directory.", "Write replacements", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), "Error while writing replacements.txt to resources directory!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnReplacementAdd_Click(object sender, EventArgs e)
        {
            var ds = gridReplacements.DataSource as BindingList<ReplacementPair>;
            if (ds == null)
                return;

            ds.Add(new ReplacementPair("", ""));
        }

        private void btnReplacementRemove_Click(object sender, EventArgs e)
        {
            var selCell = gridReplacements.SelectedCells;
            if (selCell != null && selCell.Count > 0)
            {
                var selItem = gridReplacements.Rows[selCell[0].RowIndex].DataBoundItem as ReplacementPair;

                var ds = gridReplacements.DataSource as BindingList<ReplacementPair>;
                if (ds == null)
                    return;
                ds.Remove(selItem);
            }
        }

        private void btnChildAdd_Click(object sender, EventArgs e)
        {
            var ds = gridChilds.DataSource as BindingList<ChildItem>;
            if (ds == null)
                return;

            ds.Add(new ChildItem(""));
        }

        private void btnChildRemove_Click(object sender, EventArgs e)
        {
            var selCell = gridChilds.SelectedCells;
            if (selCell != null && selCell.Count > 0)
            {
                var selItem = gridChilds.Rows[selCell[0].RowIndex].DataBoundItem as ChildItem;

                var ds = gridChilds.DataSource as BindingList<ChildItem>;
                if (ds == null)
                    return;
                ds.Remove(selItem);
            }
        }

        private void btnChildReload_Click(object sender, EventArgs e)
        {
            var fName = System.Windows.Forms.Application.StartupPath + "\\Resources\\childItems.txt";
            try
            {
                var lines = File.ReadAllLines(fName);
                List<ChildItem> ds = new List<ChildItem>();
                foreach (var line in lines)
                {
                    if (line.Trim().Length == 0)
                        continue;

                    ds.Add(new ChildItem(line));
                }
                gridChilds.DataSource = new BindingList<ChildItem>(ds);

                // nastavenie glovalneho objektu do StockItem triedy
                StockItem.ChildItems = gridChilds.DataSource as BindingList<ChildItem>;
                gridStocks.Refresh();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), "Error while reading childitems.txt from resources directory!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnChildSave_Click(object sender, EventArgs e)
        {
            var ds = gridChilds.DataSource as BindingList<ChildItem>;
            if (ds == null)
                return;
            List<string> lines = new List<string>();

            foreach (var item in ds)
            {
                if (string.IsNullOrEmpty(item.ItemText))
                    continue;

                lines.Add(item.ItemText);
            }

            try
            {
                var fName = System.Windows.Forms.Application.StartupPath + "\\Resources\\childItems.txt";
                File.WriteAllLines(fName, lines.ToArray());

                btnReplaceReload.PerformClick();

                MessageBox.Show(this, "childItems.txt successfully saved in Resources directory.", "Write childItems", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), "Error while writing childItems.txt to resources directory!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDbHelper_Click(object sender, EventArgs e)
        {
            new DBHelper().Show(this);
        }

        private void btnSetWaiting_Click(object sender, EventArgs e)
        {
            var items = GetInvoiceItemsDS();
            if (items == null || items.Count == 0)
            {
                MessageBox.Show(this, "No items to set!", "Waiting for products", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string inittext = string.Empty;
            if (items[0].Parent != null)
                inittext = items[0].Parent.OrderNumber;
            TextInputForm orderNum = new TextInputForm("Enter an order number", inittext);
            orderNum.ShowDialog(this);
            if (orderNum.ReturnText == null)
                return;

            int count = 0;
            foreach (var item in items)
            {
                if (item.PairProduct == null || (item.PairCode != null && item.PairCode == "shipping"))
                    continue;

                // produkty oznacime ako cakajuce na dalsie, pri exporte sa ulozia do DB..
                item.PairProduct.State = StockItemState.Waiting;
                item.PairProduct.Sklad = Properties.Settings.Default.Storage;
                item.PairProduct.WaitingOrderNum = orderNum.ReturnText;
                count++;
            }

            MessageBox.Show(this, string.Format("{0} products set as waiting.", count), "Waiting for products", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        const string DPDShipperDirName = "DPDShipper";
        private void btnExportToShipper_Click(object sender, EventArgs e)
        {
            try
            {
                DoShipperExport();

                MessageBox.Show(this, "Export completed!", "DPD shipper export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(this, string.Format("Error while export: {0}", ex.ToString()), "DPD shipper export", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DoShipperExport()
        {
            var ds = GetInvoiceDS();
            if (ds == null)
                return;

            List<DPDShipper> outdataSK = new List<DPDShipper>();
            List<DPDShipper> outdataHU = new List<DPDShipper>();
            List<DPDShipper> outdataPL = new List<DPDShipper>();
            List<DPDShipper> outdataCZ = new List<DPDShipper>();
            foreach (var item in ds)
            {
                if (!item.Equipped || item.Cancelled || IsPostaShipping(item))
                    continue;

                var orderNr = Common.ModifyOrderNumber(item.OrderNumber);

                var shipper = new DPDShipper();
                shipper.AdressId = "1";//A
                shipper.ParcelWeight = "1";//B
                shipper.ParcelType = "D";//C
                shipper.NrOfTotal = "1 z 1";//D
                if (item.OrderPaymentMethod.ToLower().Contains("cashondelivery"))
                {
                    shipper.ParcelCOD = "Y";//E
                    shipper.ParcelCODAmount = item.OrderGrandTotal;//F
                    shipper.ParcelCODCurrency = "EUR";//G
                    shipper.ParcelCODvarSym = orderNr;//H
                    shipper.ParcelCODCardPay = "N";//I
                }
                else //if (item.OrderPaymentMethod.ToLower().Contains("checkmo"))
                {
                    shipper.ParcelCOD = "N";//E
                    shipper.ParcelCODAmount = "";//F
                    shipper.ParcelCODCurrency = "";//G
                    shipper.ParcelCODvarSym = "";//H
                    shipper.ParcelCODCardPay = "N";//I
                }
                shipper.ParcelOrderNumber = orderNr;//J
                shipper.CustRef = orderNr;//K
                shipper.CustName = item.ShippingName;//L
                shipper.CustStreet = item.ShippingStreet;//M
                shipper.CustZip = item.ShippingZip;//N
                shipper.CustCity = item.ShippingCity;//O
                shipper.CustCountry = "703";//P
                shipper.CustPhone = item.ShippingPhoneNumber;//Q
                shipper.CustEmail = item.CustomerEmail;//R
                shipper.SMSPreAdvice = "Y";//S
                shipper.PhoneNumber = item.ShippingPhoneNumber;//T
                shipper.ParcelNote = "ActiveStyle.sk";//U
                shipper.Suma = item.OrderGrandTotal;

                if (item.Country == Country.Hungary)
                    outdataHU.Add(shipper);
                else if (item.Country == Country.Poland)
                    outdataPL.Add(shipper);
                else if (item.Country == Country.CzechRepublic)
                {
                    shipper.ParcelType = "NCP,NN,PRO";//C
                    shipper.NrOfTotal = "Cash";//D
                    shipper.ParcelCODCurrency = "CZK";//G
                    shipper.CustCountry = "203";//P
                    shipper.SMSPreAdvice = "S";//S
                    shipper.PhoneNumber = item.ShippingPhoneNumber.Replace("+420", "+420#");//T
                    shipper.ParcelNote = "1";//U
                    shipper.V = "CZ";//V
                    shipper.W = "E";//W
                    shipper.X = item.CustomerEmail;//X
                    shipper.Y = "2";//Y
                    shipper.Z = "CZ";//Z

                    outdataCZ.Add(shipper);
                }
                else
                    outdataSK.Add(shipper);
            }

            if (outdataSK.Count > 0)
                SaveShipper(outdataSK, Country.Slovakia);
            if (outdataHU.Count > 0)
                SaveShipper(outdataHU, Country.Hungary);
            if (outdataPL.Count > 0)
                SaveShipper(outdataPL, Country.Poland);
            if (outdataCZ.Count > 0)
                SaveShipper(outdataCZ, Country.CzechRepublic);
        }

        private void SaveShipper(List<DPDShipper> outdata, Country country)
        {
            // vystup pojde sem
            StringBuilder sb = new StringBuilder();

            // formatovanie hlavicky
            if (country != Country.Hungary)
            {
                sb.Append("address_ID;");
                sb.Append("parcel_weight;");
                sb.Append("parcel_type;");
                sb.Append("nr_of_total;");
                sb.Append("parcel_COD;");
                sb.Append("parcel_COD_amount;");
                sb.Append("parcel_COD_currency;");
                sb.Append("parcel_COD_variable_symbol;");
                sb.Append("parcel_COD_cardpay;");
                sb.Append("parcel_order_number;");
                sb.Append("customer_reference;");
                sb.Append("customer_name;");
                sb.Append("customer_street;");
                sb.Append("customer_zipcode;");
                sb.Append("customer_city;");
                sb.Append("customer_country_ID;");
                sb.Append("customer_phone;");
                sb.Append("customer_email;");
                sb.Append("sms_preadvice;");
                sb.Append("phone_number;");
                sb.Append("parcel_note");
            }
            else
            {
                /*sb.Append("c_nev;");//név
                sb.Append("c_szemely;");
                sb.Append("c_irsz;");//irányítószám
                sb.Append("c_helyseg;");//város
                sb.Append("c_utca;");//cím
                sb.Append("c_telefon;");//telefonszám
                sb.Append("c_email;");//e-mail
                sb.Append("c_vevokod;");//ország
                sb.Append("szamlaszam;");//utánvét hivatkozás
                sb.Append("aruertek;");//Utánvét
                sb.Append("utanvetel;");
                sb.Append("fuvardij;");
                sb.Append("darab;");
                sb.Append("egyseg;");
                sb.Append("suly;");
                sb.Append("tartalom;");
                sb.Append("termekneve;");
                sb.Append("szallido;");
                sb.Append("okmany;");
                sb.Append("szombati;");
                sb.Append("kezbemil;");
                sb.Append("kezbsms;");
                sb.Append("szlafizhat;");
                sb.Append("azon_kelle;");
                sb.Append("felado_osztaly;");
                sb.Append("fizetofel;");
                sb.Append("instrukcio;");
                sb.Append("kezbesites;");
                sb.Append("visszaru;");
                sb.Append("feladobetujel");*/
                sb.Append("Utánvét;");
                sb.Append("utánvét hivatkozás;");
                sb.Append("név;");
                sb.Append("cím;");
                sb.Append("telefonszám;");            
                sb.Append("irányítószám;");
                sb.Append("város;");                            
                sb.Append("e-mail;");
                sb.Append("ország;");
                
                
            }

            // formatovanie dat
            foreach (var shipper in outdata)
            {
                sb.Append(Environment.NewLine);

                if (country != Country.Hungary)
                {
                    sb.Append(shipper.AdressId + ";");
                    sb.Append(shipper.ParcelWeight + ";");
                    sb.Append(shipper.ParcelType + ";");
                    sb.Append(shipper.NrOfTotal + ";");
                    sb.Append(shipper.ParcelCOD + ";");
                    sb.Append(shipper.ParcelCODAmount + ";");
                    sb.Append(shipper.ParcelCODCurrency + ";");
                    sb.Append(shipper.ParcelCODvarSym + ";");
                    sb.Append(shipper.ParcelCODCardPay + ";");
                    sb.Append(shipper.ParcelOrderNumber + ";");
                    sb.Append(shipper.CustRef + ";");
                    sb.Append(shipper.CustName + ";");
                    sb.Append(shipper.CustStreet + ";");
                    sb.Append(shipper.CustZip + ";");
                    sb.Append(shipper.CustCity + ";");
                    sb.Append(shipper.CustCountry + ";");
                    sb.Append(shipper.CustPhone + ";");
                    sb.Append(shipper.CustEmail + ";");
                    sb.Append(shipper.SMSPreAdvice + ";");
                    sb.Append(shipper.PhoneNumber + ";");
                    sb.Append(shipper.ParcelNote);

                    if (country == Country.CzechRepublic)
                    {
                        sb.Append(";");
                        sb.Append(shipper.V + ";");
                        sb.Append(shipper.W + ";");
                        sb.Append(shipper.X + ";");
                        sb.Append(shipper.Y + ";");
                        sb.Append(shipper.Z);
                    }
                }
                else // madarske
                {
                   /* sb.Append(shipper.CustName + ";");
                    sb.Append(shipper.CustName + ";");
                    sb.Append(shipper.CustZip + ";");
                    sb.Append(shipper.CustCity + ";");
                    sb.Append(shipper.CustStreet + ";");
                    var phone = shipper.PhoneNumber;
                    if (phone.StartsWith("06"))
                        phone = "+36" + phone.Substring(2);
                    if (phone.StartsWith("36"))
                        phone = "+" + phone;
                    if (!phone.StartsWith("+36"))
                        phone = "+36" + phone;
                    phone = phone.Replace(" ", "").Replace("-", "").Replace("/","").Replace("\\", "");

                    sb.Append(phone + ";");//todo upravit
                    sb.Append(shipper.CustEmail + ";");
                    sb.Append("Activestyle.hu;");
                    sb.Append(shipper.ParcelOrderNumber + ";");
                    sb.Append(shipper.Suma+ ";");
                    sb.Append(shipper.Suma + ";");
                    sb.Append(";");
                    sb.Append("1;");
                    sb.Append(";");
                    sb.Append("1;");
                    sb.Append(";");
                    sb.Append("ruházat;");
                    sb.Append("1 munkanapos;");
                    sb.Append(";");
                    sb.Append(";");
                    sb.Append(shipper.CustEmail + ";");
                    sb.Append(phone + ";");
                    sb.Append(";");
                    sb.Append(";");
                    sb.Append(";");
                    sb.Append(";");
                    sb.Append("Hívás!! Hívás!! Hívás!!;");
                    sb.Append("1;");
                    sb.Append("0;");
                    sb.Append("MMR");*/
                    var suma=shipper.Suma.Replace(",",".");
                    if (suma.Contains("."))
                        suma=suma.Substring(0, suma.LastIndexOf(".")).Replace(".","");//Utánvét
                    sb.Append(suma + ";");//Utánvét
                    sb.Append(shipper.ParcelOrderNumber + ";");//utánvét hivatkozás
                    sb.Append(shipper.CustName + ";");//nev
                    sb.Append(shipper.CustStreet + ";");//cím
                    var phone = shipper.PhoneNumber;
                    if (phone.StartsWith("06"))
                        phone = "+36" + phone.Substring(2);
                    if (phone.StartsWith("36"))
                        phone = "+" + phone;
                    if (!phone.StartsWith("+36"))
                        phone = "+36" + phone;
                    phone = phone.Replace(" ", "").Replace("-", "").Replace("/", "").Replace("\\", "");

                    sb.Append(phone + ";");//todo upravit telefonszám                   
                    sb.Append(shipper.CustZip + ";");//irányítószám
                    sb.Append(shipper.CustCity + ";");//város
                    sb.Append(shipper.CustEmail + ";");//e-mail
                    sb.Append("Magyarország;");//ország
                }
            }

            // zapis do suboru
            var outDir = txtOutDir.Text + "/" + DPDShipperDirName + "/";
            if (!Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);
            var fname = "SK";
            switch (country)
            {
                case Country.Unknown:
                    fname = "UNKNOWN";
                    break;
                case Country.Slovakia:
                    fname = "SK";
                    break;
                case Country.Hungary:
                    fname = "HU";
                    break;
                case Country.Poland:
                    fname = "PL";
                    break;
                case Country.CzechRepublic:
                    fname = "CZ";
                    break;
                default:
                    break;
            }
            fname += "_shipper_" + DateTime.Now.Ticks + ".csv";

            switch (country)
            {
                case Country.Slovakia:
                    File.WriteAllText(outDir + fname, sb.ToString(), Encoding.UTF8);
                    break;
                case Country.Hungary:
                    File.WriteAllText(outDir + fname, sb.ToString(), Encoding.UTF8);
                    break;
                case Country.Poland:
                    File.WriteAllText(outDir + fname, sb.ToString(), Encoding.GetEncoding(1252));
                    break;
                case Country.CzechRepublic:
                    File.WriteAllText(outDir + fname, sb.ToString(), Encoding.GetEncoding(1252));
                    break;

                case Country.Unknown:
                default:
                    File.WriteAllText(outDir + fname, sb.ToString(), Encoding.UTF8);
                    break;
            }
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            RefreshReader();
        }

        private void btnFilClear_Click(object sender, EventArgs e)
        {
            txtFilStoreNr.Text = txtFilSKU.Text = txtFilOrderNr.Text = string.Empty;
        }

        private void txtFilSKU_Click(object sender, EventArgs e)
        {
            if (!(sender is TextBox))
                return;
            var txt = sender as TextBox;

            txt.SelectAll();
        }

        private void btnOrderEquipped_Click(object sender, EventArgs e)
        {
            if (gridReader.SelectedCells.Count == 0)
                return;

            var row = gridReader.Rows[gridReader.SelectedCells[0].RowIndex];
            if (row == null)
                return;

            var orderNumber = row.Cells["ORDER_NUMBER"].Value.ToString();
            if (string.IsNullOrEmpty(orderNumber))
                return;

            var cmd = string.Format("update {0} set VALID = 0 where ORDER_NUMBER = \"{1}\"", DBProvider.T_READER, orderNumber);

            try
            {
                DBProvider.ExecuteNonQuery(cmd);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(this, "Error while updating READER table! Exception: " + ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            RefreshReader();
        }

        private void btnDeleteAllReader_Click(object sender, EventArgs e)
        {
            var query = string.Format("UPDATE {0} SET VALID = 0", DBProvider.T_READER);

            DBProvider.ExecuteNonQuery(query);

            RefreshReader();
        }

        private void btnInvCopy_Click(object sender, EventArgs e)
        {
            var selCells = gridInvoices.SelectedCells;
            if (selCells == null || selCells.Count == 0)
                return;

            var toCopy = gridInvoices.Rows[selCells[0].RowIndex].DataBoundItem as Invoice;

            var ds = GetInvoiceDS();
            if (ds == null)
                return;
            ds.Add(new Invoice(toCopy));
        }

        private void btnStockCopy_Click(object sender, EventArgs e)
        {
            var selCells = gridStocks.SelectedCells;
            if (selCells == null || selCells.Count == 0)
                return;

            var toCopy = gridStocks.Rows[selCells[0].RowIndex].DataBoundItem as StockItem;

            var ds = GetProductsDS();
            if (ds == null)
                return;
            ds.Add(toCopy.Clone() as StockItem);
        }

        void DownloadExchangeRateXML()
        {
            var address = Properties.Settings.Default.ExchRateXMLAddress;
            XmlTextReader reader = new XmlTextReader(address);
            string currName = "currency";
            string rateName = "rate";
            string timeName = "time";
            ExRateItem newRate = new ExRateItem();
            newRate.Date = DateTime.Now.ToString("yyyy-MM-dd");

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element: // The node is an element.
                        {
                            string curr = null;
                            string rate = null;

                            while (reader.MoveToNextAttribute())
                            {
                                if (reader.Name.ToLower() == timeName)  // datum
                                    newRate.Date = reader.Value;

                                if (reader.Name.ToLower() == currName)  // mena
                                    curr = reader.Value;
                                if (reader.Name.ToLower() == rateName)  // kurz
                                    rate = reader.Value;
                            }

                            if (curr == "CZK")
                                newRate.RateCZK = Common.GetPrice(rate);
                            if (curr == "PLN")
                                newRate.RatePLN = Common.GetPrice(rate);
                            if (curr == "HUF")
                                newRate.RateHUF = Common.GetPrice(rate);
                        }
                        break;
                }
            }

            DBProvider.InsertExRate(newRate);
        }

        private void dataGridInvItems_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != 2) // SKU pre invoice item dotahovane zo stock
                return;

            gridInvItems.InvalidateRow(e.RowIndex);
            
            var selCells = gridInvItems.SelectedCells;
            if (selCells != null && selCells.Count > 0)
            {
                var selItem = selCells[0];
                var ds = GetInvoiceItemsDS();
                if (ds == null)
                    return;
                var selInv = ds[selItem.RowIndex];

                if (selInv != null)
                    CheckEqipped(selInv.Parent);
                gridInvoices.InvalidateRow(gridInvoices.SelectedCells[0].RowIndex);
            }
        }

        private void btnRefreshTotalInvSum_Click(object sender, EventArgs e)
        {
            RefreshTab();
        }

        private void btnPostaExport_Click(object sender, EventArgs e)
        {
            try
            {
                DoPostaExport();

                MessageBox.Show(this, "Export completed!", "DPD shipper export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(this, string.Format("Error while export: {0}", ex.ToString()), "DPD shipper export", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DoPostaExport()
        {
            var ds = GetInvoiceDS();
            if (ds == null)
                return;

            var datum = DateTime.Now.ToString("yyyyMMdd");

            var xml = new ephType();
            xml.verzia = "3";

            // zasielky
            xml.Zasielky = new List<ephTypeZasielka>();
            foreach (var invoice in ds)
            {
                if (!invoice.Equipped || invoice.Cancelled)
                    continue;

                SpracujZasielku(xml, invoice);
            }

            xml.InfoEPH = new ephTypeInfoEPH();
            xml.InfoEPH.Datum = datum;
            xml.InfoEPH.DruhPPP = "";
            xml.InfoEPH.DruhZasielky = "1";
            xml.InfoEPH.EPHID = "";
            xml.InfoEPH.Mena = "EUR";
            xml.InfoEPH.PocetZasielok = xml.Zasielky.Count.ToString();
            xml.InfoEPH.SposobSpracovania = "";
            xml.InfoEPH.TypEPH = "1";
            xml.InfoEPH.Uhrada = new List<ephTypeInfoEPHUhrada>();
            xml.InfoEPH.Uhrada.Add(new ephTypeInfoEPHUhrada());
            xml.InfoEPH.Uhrada[0].SposobUhrady = "5";
            xml.InfoEPH.Uhrada[0].SumaUhrady = "0.00";
            xml.InfoEPH.Odosielatel = new ephTypeInfoEPHOdosielatel();
            //xml.InfoEPH.Odosielatel.CisloUctu = "info@activestyle.sk";
            xml.InfoEPH.Odosielatel.Email = "info@activestyle.sk";
            xml.InfoEPH.Odosielatel.Krajina = "";
            xml.InfoEPH.Odosielatel.Meno = "Activestyle.sk";
            xml.InfoEPH.Odosielatel.Mesto = "Rimavská Sobota 1";
            xml.InfoEPH.Odosielatel.OdosielatelID = "";
            xml.InfoEPH.Odosielatel.Organizacia = "MM Retail s.r.o.";
            xml.InfoEPH.Odosielatel.PSC = "979 01";
            xml.InfoEPH.Odosielatel.Telefon = "0948544211";
            xml.InfoEPH.Odosielatel.Ulica = "B.Bartóka 1048/24";

            var dir = txtOutDir.Text;
            if (!dir.EndsWith(@"\"))
                dir += @"\";
            dir += @"Posta\";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            xml.SaveToFile(dir + "export_"+DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")+".xml");
        }

        private void SpracujZasielku(ephType xml, Invoice invoice)
        {
            if (!IsPostaShipping(invoice))
                return;

            var zasielka = new ephTypeZasielka();

            zasielka.Adresat = new ephTypeZasielkaAdresat();
            zasielka.Adresat.Email = invoice.CustomerEmail;
            zasielka.Adresat.Krajina = invoice.ShippingCountry;
            zasielka.Adresat.Meno = invoice.ShippingName;
            zasielka.Adresat.Mesto = invoice.ShippingCity;
            zasielka.Adresat.Organizacia = "";
            zasielka.Adresat.PSC = invoice.ShippingZip;
            zasielka.Adresat.Telefon = invoice.ShippingPhoneNumber;
            zasielka.Adresat.Ulica = invoice.ShippingStreet;
            
            zasielka.Info = new ephTypeZasielkaInfo();
            zasielka.Info.ZasielkaID = invoice.OrderNumber;
            zasielka.Info.Hmotnost = "0.000";
            zasielka.Info.CenaDobierky = invoice.OrderGrandTotal;//VratCenuDopravy(invoice.InvoiceItems);
            zasielka.Info.Trieda = "1";
            zasielka.Info.CisloUctu = "2800328484/8330";
            zasielka.Info.SymbolPrevodu = zasielka.Info.ZasielkaID; // TODO?
            zasielka.Info.Poznamka = zasielka.Info.SymbolPrevodu + " Activestyle.sk";
            zasielka.Info.DruhPPP = "5";

            zasielka.PouziteSluzby = new List<string>();
            zasielka.PouziteSluzby.Add("");

            zasielka.Spat = null;
            zasielka.DalsieUdaje = null;

            xml.Zasielky.Add(zasielka);
        }

        bool IsPostaShipping(Invoice inv)
        {
            var method = inv.OrderShippingMethod.ToLower();
            return method.Contains("flatrate3") || method.Contains("flatrate4");
        }

        private string VratCenuDopravy(List<InvoiceItem> list)
        {
            var price = list.Where(ii => ii.PairCode == "shipping").LastOrDefault();
            if (price == null || string.IsNullOrEmpty(price.ItemPrice))
                return "0.00";

            return price.ItemPrice.Replace(",", ".");
        }

        private void FrmActiveStyle_Load(object sender, EventArgs e)
        {
            // nacitanie sirok stlpcov
            LoadColWidths();
        }

        private void LoadColWidths()
        {
            var path = System.Windows.Forms.Application.StartupPath;
            path += @"\cols.dat";

            if (!File.Exists(path))
                return;

            var lines = File.ReadAllLines(path);

            SetGridWidths(gridInvoices, lines[0]);
            SetGridWidths(gridInvItems, lines[1]);
            SetGridWidths(gridStocks, lines[2]);
        }

        private void SetGridWidths(DataGridView grid, string widths)
        {
            var w = widths.Split(';');
            for (int i = 0; i < w.Length; i++)
            {
                if (i >= grid.Columns.Count)
                    break;

                grid.Columns[i].Width = int.Parse(w[i]);
            }
        }

        private void FrmActiveStyle_FormClosing(object sender, FormClosingEventArgs e)
        {
            // ulozenie sirok stlpcov
            SaveColWidths();
        }

        private void SaveColWidths()
        {
            var path = System.Windows.Forms.Application.StartupPath;
            path += @"\cols.dat";

            var lines = new List<string>();

            // grid invoices
            lines.Add(GetGridWidths(gridInvoices));
            lines.Add(GetGridWidths(gridInvItems));
            lines.Add(GetGridWidths(gridStocks));

            File.WriteAllLines(path, lines.ToArray());
        }

        string GetGridWidths(DataGridView grid)
        {
            var sb = new StringBuilder();
            string toAdd;

            for (int i = 0; i < grid.Columns.Count; i++)
            {
                sb.AppendFormat("{0};", grid.Columns[i].Width);
            }
            toAdd = sb.ToString();

            return toAdd.TrimEnd(';');
        }

        private void btnSetTrashNr_Click(object sender, EventArgs e)
        {
            TextInputForm frm = new TextInputForm("Číslo košíka", "Zadajte číslo košíka");
            var res = frm.ShowDialog();
            if (res == DialogResult.OK)
            {
                SetTrashNumber(frm.ReturnText);
            }
        }

        private void SetTrashNumber(string p)
        {
            var ds = GetInvoiceDS();
            if (ds == null)
                return;

            foreach (var item in ds)
            {
                item.TrashNumber = p;
            }

            RefreshTab();
        }

        private void btnStoreState_Click(object sender, EventArgs e)
        {
            var folder = SelectDirectory("Choose a directory to save");
            if (folder == null)
                return;
            var fname = DateTime.Now.Ticks;

            var inv = GetInvoiceDS();
            if (inv == null)
                return;
            Serializer.Serialize<BindingList<Invoice>>(inv, folder+@"\"+fname+".inv");
            MessageBox.Show(this, "Invoice state saved!", "LoadState", MessageBoxButtons.OK, MessageBoxIcon.Information);

            var stk = GetProductsDS();
            if (stk == null)
                return;
            Serializer.Serialize<BindingList<StockItem>>(stk, folder + @"\" + fname + ".stk");
            MessageBox.Show(this, "Stock state saved!", "LoadState", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnDeserialize_Click(object sender, EventArgs e)
        {
            var folder = SelectDirectory("Choose a directory to load");
            if (folder == null)
                return;

            var inv = Directory.GetFiles(folder, "*.inv");
            if (inv != null && inv.Length > 0)
            {
                var ds = Serializer.Deserialize<BindingList<Invoice>>(inv[0]);
                if (ds != null)
                {
                    SetInvoiceDS(ds);
                    MessageBox.Show(this, "Invoice state loaded!", "LoadState", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            var stk = Directory.GetFiles(folder, "*.stk");
            if (stk != null && stk.Length > 0)
            {
                var ds = Serializer.Deserialize<BindingList<StockItem>>(stk[0]);
                if (ds != null)
                {
                    SetProductsDS(ds);
                    MessageBox.Show(this, "Stock state loaded!", "LoadState", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

       private void generateNomen_Click(object sender, EventArgs e)
        {
       /*      var stockItems = GetProductsDS();
            ms_SQL msSQLConnect = new ms_SQL();

            foreach (var item in stockItems)
            {
                
                //item.nomenklatura=item.ProductCode;
                string sku = item.ProductCode;//ProductCode

                if (sku.Contains('/'))//sportdirect
                {
                    sku = sku.Remove(sku.LastIndexOf('/'));
                    sku = sku.Replace("/", "");
                }
                else if (sku.Contains('_'))//mandmdirect,getthelabel
                {

                    //sku = sku.Remove(sku.LastIndexOf('_'));
                    if (sku.Count(f => f == '_')>1)
                        sku = sku.Substring(sku.IndexOf('_')+1, sku.LastIndexOf('_') - sku.IndexOf('_')-1);
                    else
                        sku = sku.Substring(sku.IndexOf('_')+1);
                   // sku = sku.Replace("/", "");
                }
                item.nomenklatura = msSQLConnect.getNomenclature(sku);
            }
           // MessageBox.Show(this, skus, skus, MessageBoxButtons.OK, MessageBoxIcon.Information);
          //  MessageBox.Show(this, skus.Count.ToString(), skus.First().ToString(), MessageBoxButtons.OK, MessageBoxIcon.Information);*/
        }
    }
    
    class ChildItem
    {
        public string ItemText { get; set; }

        public ChildItem(string name)
        {
            ItemText = name;
        }
    }

    class ReplacementPair
    {
        public string ValueToFind { get; set; }
        public string ValueToReplace { get; set; }

        public ReplacementPair(string s1, string s2)
        {
            ValueToFind = s1;
            ValueToReplace = s2;
        }
    }

    /// <summary>
    /// Indexy tabov
    /// </summary>
    enum Tabs
    {
        Invoices,
        Stocks,
        Reader,
        Waiting
    }

    class CustomDataGridView : DataGridView
    {
        public CustomDataGridView()
        {
            DoubleBuffered = true;
        }
    }
    public static class stringFunctions
    {
        public static bool ContainsCaseInsensitive(this string source, string value)
        {

            int results = source.IndexOf(value, StringComparison.CurrentCultureIgnoreCase);

            return results == -1 ? false : true;

        }
    }
}