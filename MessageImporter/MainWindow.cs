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

        //////////////////////////////////////////////////////////////////////////////////////////
        BindingList<Invoice> GetInvoiceDS()
        {
            return dataCSV.DataSource as BindingList<Invoice>;
        }
        void SetInvoiceDS(BindingList<Invoice> dataSource)
        {
            dataCSV.DataSource = dataSource;
        }

        BindingList<InvoiceItem> GetInvoiceItemsDS()
        {
            return dataGridInvItems.DataSource as BindingList<InvoiceItem>;
        }
        void SetInvoiceItemsDS(BindingList<InvoiceItem> dataSource)
        {
            dataGridInvItems.DataSource = dataSource;
        }

        BindingList<StockItem> GetProductsDS()
        {
            return dataGrid.DataSource as BindingList<StockItem>;
        }
        void SetProductsDS(BindingList<StockItem> dataSource)
        {
            dataGrid.DataSource = dataSource;
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

            btnSettingsLoad_Click(btnSettingsLoad, new EventArgs());
            btnReplaceReload_Click(btnReplaceReload, new EventArgs());
            btnChildReload_Click(btnChildReload, new EventArgs());

            // stiahnutie a import kurzoveho listka
            try
            {
                log("");
                log("Downloading exchange rates from "+Properties.Settings.Default.ExchRateXMLAddress);
                DownloadExchangeRateXML();
                log("done.");
            }
            catch (System.Exception ex)
            {
                log("failed!");
                MessageBox.Show(this, "Failed to load exchange rates from "+Properties.Settings.Default.ExchRateXMLAddress+"! Exception: "+ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            btnProcess_Click(btnProcess, new EventArgs());
        }

        internal void btnRead_Click(object sender, EventArgs e)
        {
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
                MessageBox.Show(this, ex.ToString(), "Error");
            }
        }

        internal StockEntity decodeMessage(string messageBody, FileItem file)
        {
            try
            {
                var lines = messageBody.Split(Environment.NewLine.ToCharArray()).Where(s => s != null && s.Trim().Length > 0).ToArray();
                
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
                        line = lines[i - 5];
                        item.Ord_Qty = int.Parse(line.Trim());
                        line = lines[i - 4];
                        item.Disp_Qty = int.Parse(line.Trim());
                        line = lines[i - 3];
                        item.Description = line.Trim();
                        line = lines[i - 2];
                        line = line.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                        item.Price = double.Parse(line.Trim().Substring(1));
                        line = lines[i - 1];
                        line = line.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                        item.Total = double.Parse(line.Trim().Substring(1));

                        item.Currency = line.Substring(0, 1);

                        item.FromFile = file;
                        file.ProdCount++;

                        if (item.State == StockItemState.PermanentStorage)
                            item.Sklad = "02";
                        else if (item.State == StockItemState.Waiting)
                            item.Sklad = Properties.Settings.Default.Storage;

                        items.Add(item);
                    }

                    if (line.Contains(delivery))
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

                // viacpoctove produkty sa rozkuskuju
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].Disp_Qty > 1)
                    {
                        var count = items[i].Disp_Qty;
                        items[i].Disp_Qty = 1;

                        for (int j = 0; j < count-1; j++)
                            items.Insert(i, items[i].Clone() as StockItem);

                        i += count;
                    }
                }

                order.Items = items.ToArray();

                return order;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), "Error");
            }

            return null;
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
                    dialog.ShowNewFolderButton = false;
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
            logNewSection("Begin processing..");
            
            try
            {
                SetInvoiceDS(new BindingList<Invoice>());
                SetInvoiceItemsDS(new BindingList<InvoiceItem>());
                SetProductsDS(new BindingList<StockItem>());

                // nacitanie dat do allMessages a allOrders
                if (!ProcessSelectedFiles())
                    return;

                // naplni allInvoices a nastavi datasource
                CreateInvoice(allOrders);
                // pridanie poloziek "Cena za dopravu"
                AddShippingItems(AllInvoices);
                SetInvoiceDS(new BindingList<Invoice>(AllInvoices));
                // kontrola na nejasnosti v kodoch produktov
                CheckPairByHand(allMessages.SelectMany(o => o.Items).ToList());
                //AllStocks = allMessages.SelectMany(o => o.Items).ToList();
                SetProductsDS(new BindingList<StockItem>(allMessages.SelectMany(o => o.Items).ToList()));
                
                                
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

                dataGrid.Columns["OrderDate"].DefaultCellStyle.Format = "dd.MM.yyyy";
                dataGridInvItems.Columns["Datetime"].DefaultCellStyle.Format = "dd.MM.yyyy";
                
                if (chkMoveProcessed.Checked)
                    btnRead.PerformClick();

                RefreshTab();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), "Error");
            }
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
            if (tabData.SelectedIndex == (int)Tabs.Invoices)
            {
                dataCSV.Refresh();
                dataGridInvItems.Refresh();
            }
            else if (tabData.SelectedIndex == (int)Tabs.Stocks)
            {
                dataGrid.Refresh();

                var ds = GetProductsDS();
                if (ds == null)
                    return;

                // nastavenie farieb
                for (int i = 0; i < ds.Count; i++)
                {
                    if (ds[i].ChangeColor)
                    {
                        dataGrid["PriceEURnoTaxEUR", i].Style.BackColor = Color.Green;
                        dataGrid["PriceEURnoTaxEUR", i].Style.ForeColor = Color.White;
                    }

                    if (ds[i].PairByHand && ds[i].PairProduct == null)
                    {
                        dataGrid["ProductCode", i].Style.BackColor = Color.Blue;
                        dataGrid["ProductCode", i].Style.ForeColor = Color.White;
                    }
                }
            }
            else if (tabData.SelectedIndex == (int)Tabs.Reader)
            {
                RefreshReader();
            }

            dataFiles.Refresh();
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
                        var ret = ProcessCSV(file.FullFileName);
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

                        inv.TotQtyOrdered = item.TotQtyOrdered;
                        inv.BillingCity = item.BillingCity;
                        inv.BillingCompany = item.BillingCompany;
                        inv.BillingCountry = item.BillingCountry;
                        inv.BillingCountryName = item.BillingCountryName;
                        inv.BillingName = item.BillingName;
                        inv.BillingPhoneNumber = Common.ModifyPhoneNr(item.BillingPhoneNumber);
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
                        inv.ShippingPhoneNumber = Common.ModifyPhoneNr(item.ShippingPhoneNumber);
                        inv.ShippingState = item.ShippingState;
                        inv.ShippingStateName = item.ShippingStateName;
                        inv.ShippingStreet = item.ShippingStreet;
                        inv.ShippingZip = item.ShippingZip;
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
            if (item == null)
                return null;

            string productCode = new string(item.ToCharArray().Where(c => "0123456789".Contains(c)).ToArray());
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
                    string productCode = ConvertInvoiceItem(product.invSKU);
                    if (productCode == null)
                        continue;

                    var prodDS = GetProductsDS();
                    //foreach (var msg in allMessages)
                    {
                        var foundItems = prodDS.Where(orderItem => orderItem.ProductCode != null && orderItem.ProductCode.Contains(productCode) && orderItem.PairProduct == null).ToList();
                        
                        // ak nic nenajdeme skusime opacne parovanie
                        if (foundItems.Count == 0)
                            foundItems = prodDS.Where(orderItem => orderItem.ProductCode != null && product.invSKU.Contains(orderItem.ProductCode) && orderItem.PairProduct == null).ToList();

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

                            for (int i = 0; i < count; i++)
                            {
                                if (foundItems.Count == i)
                                    break;

                                if (foundItems[i].PairByHand)
                                    continue;

                                if (i == 0 && count <= foundItems.Count)
                                    product.PairProduct = foundItems[i];

                                if (i < foundItems.Count)
                                    foundItems[i].PairProduct = product;    // n produktov zo stock sa naviaze na jeden produkt z CSV (n pocet objednanych v CSV)
                            }
                        }
                    }

                    if (product.PairProduct == null)
                    {
                        var req = string.Format("SELECT * FROM "+DBProvider.T_WAIT_PRODS+" WHERE ORDER_NUMBER = \"{0}\" AND INV_SKU = \"{1}\" AND VALID = 1", Common.ModifyOrderNumber2(CSV.OrderNumber), product.invSKU);
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

                file.OrderNumber = order.OrderReference;
            }
            catch (System.Exception ex)
            {
                log(ex.Message);
                return order;
            }
            
            return order;
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

                order.Items = items.ToArray();

                return order;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), "Error");
            }

            return null;
        }

        internal CSVFile ProcessCSV(string path)
        {
            CSVFile ret = new CSVFile(path);
            try
            {
                string fileContent = File.ReadAllText(path);

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
                newInv.invoiceHeader.accounting.ids = Properties.Settings.Default.Accounting;
                newInv.invoiceHeader.classificationVAT = new classificationVATType();
                newInv.invoiceHeader.classificationVAT.ids = Properties.Settings.Default.ClasifficationVAT;
                newInv.invoiceHeader.classificationVAT.classificationVATType1 = classificationVATTypeClassificationVATType.inland;
                newInv.invoiceHeader.text = inv.OrderGrandTotal;

                // header->identity
                newInv.invoiceHeader.partnerIdentity = new address();
                newInv.invoiceHeader.partnerIdentity.address1 = new addressType();
                newInv.invoiceHeader.partnerIdentity.address1.name = inv.BillingName;
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

                newInv.invoiceHeader.account = new accountType();
                newInv.invoiceHeader.account.bankCode = Properties.Settings.Default.BankCode;
                newInv.invoiceHeader.account.ids = Properties.Settings.Default.Bank;

                // polozky faktury
                var invItems = new List<invoiceItemType>();
                foreach (var invItem in inv.InvoiceItems)
                {
                    var code = "";
                    if (invItem.PairCode != null)
                        code = invItem.PairCode;

                    invoiceItemType xmlItem = new invoiceItemType();
                    // specialna polozka "cena za dopravu"
                    if (code == Properties.Settings.Default.ShippingCode)
                    {
                        xmlItem.text = invItem.MSG_SKU;
                        xmlItem.quantitySpecified = true;
                        xmlItem.quantity = 1;
                        xmlItem.rateVAT = vatRateType.high;
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
                        xmlItem.accounting.ids = "2";

                        xmlItem.payVAT = boolean.@true;

                        xmlItem.percentVATSpecified = true;
                        xmlItem.percentVAT = 20;
                    }
                    else
                    {
                        xmlItem.code = code;
                        xmlItem.text = code;// invItem.ItemName;
                        xmlItem.quantitySpecified = true;
                        xmlItem.quantity = float.Parse(invItem.ItemQtyOrdered);
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

                        // stock item
                        xmlItem.stockItem = new stockItemType();
                        xmlItem.stockItem.stockItem = new stockRefType();
                        xmlItem.stockItem.stockItem.ids = code;
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
                dp.SaveToFile(outDir + dp.id);
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
                    readerItem.Valid = 1;

                    DBProvider.InsertReaderItem(readerItem);
                    readerItems.Add(readerItem);
                }

                if (nextStore)
                    storeNr++;  // dalsia faktura pojde do dalsieho policka
            }
            StringBuilder readerStrings = new StringBuilder();
            readerStrings.AppendFormat("Store number;Order number;SKU{0}", Environment.NewLine);
            foreach (var item in readerItems)
            {
                readerStrings.AppendFormat("{0};{1};{2}{3}", item.StoreNr, item.OrderNr, item.SKU, Environment.NewLine);
            }
            File.WriteAllText(outDir + "reader_"+DateTime.Now.ToString("yyyyMMdd_hhmmss")+".csv", readerStrings.ToString());

            MessageBox.Show("Invoice XML generated!", "Save XML", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        const string StockDir = "Stock";
        void StoreStock()
        {
            var prodDS = GetProductsDS();

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

            foreach (var prod in prodDS)
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
                stock.actionType.ItemElementName = ItemChoiceType3.add;

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
                    // ulozenie produktu do DB
                    var insert = string.Format("INSERT INTO " + DBProvider.T_WAIT_PRODS + " VALUES ({0},\"{1}\",\"{2}\",\"{3}\",\"{4}\",{5})", "null", (string.IsNullOrEmpty(prod.WaitingOrderNum) ? Common.ModifyOrderNumber2(prod.PairProduct.Parent.OrderNumber) : prod.WaitingOrderNum), prod.PairProduct.invSKU, prod.ProductCode, prod.Description, 1);
                    log(insert);

                    try
                    {
                        DBProvider.ExecuteNonQuery(insert);
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
                stock.stockHeader.purchasingPrice = prod.PriceEURnoTaxEUR;
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
            newInv.invoiceHeader.text = "SportsDirect_" + (allMessages.Count > 0 ? allMessages[0].OrderReference : "<err>");
            newInv.invoiceHeader.partnerIdentity = new address();
            newInv.invoiceHeader.partnerIdentity.id = "24";

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
                dp.SaveToFile(outDir + dp.id);
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
            var selCell = dataCSV.SelectedCells;
            if (selCell != null && selCell.Count > 0)
            {
                var selItem = dataCSV.Rows[selCell[0].RowIndex].DataBoundItem as Invoice;

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

            var selCells = dataGridInvItems.SelectedCells;
            if (selCells != null && selCells.Count > 0)
            {
                var selItem = selCells[0];
                var dsMSG = GetProductsDS();
                if (dsMSG == null)
                    return;
                var selProd = dsMSG.Where(o => o.ProductCode == selprodcode && o.PairProduct == null).ToArray()[0];

                var ds = GetInvoiceItemsDS();
                if (ds == null)
                    return;
                var selInv = ds[selItem.RowIndex];

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
            var added = ds.AddNew();

            var selcells = dataCSV.SelectedCells;
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
            var selCell = dataGridInvItems.SelectedCells;
            if (selCell != null && selCell.Count > 0)
            {
                var selItem = dataGridInvItems.Rows[selCell[0].RowIndex].DataBoundItem as InvoiceItem;

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
            var selCell = dataGrid.SelectedCells;
            if (selCell != null && selCell.Count > 0)
            {
                var selItem = dataGrid.Rows[selCell[0].RowIndex].DataBoundItem as StockItem;

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
            var selCell = dataGridInvItems.SelectedCells;
            if (selCell != null && selCell.Count > 0)
            {
                var selItem = dataGridInvItems.Rows[selCell[0].RowIndex].DataBoundItem as InvoiceItem;

                selItem.PairProduct = null;
            }

            CheckAllEqipped();
            UpdateProductSet();
            RefreshTab();
        }

        internal void btnUnpairAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGridInvItems.RowCount; i++)
            {
                var selItem = dataGridInvItems.Rows[i].DataBoundItem as InvoiceItem;

                selItem.PairProduct = null;
            }

            CheckAllEqipped();
            UpdateProductSet();
            RefreshTab();
        }

        internal void btnUnpairProductMSG_Click(object sender, EventArgs e)
        {
            var selCell = dataGrid.SelectedCells;
            if (selCell != null && selCell.Count > 0)
            {
                var selItem = dataGrid.Rows[selCell[0].RowIndex].DataBoundItem as StockItem;

                Unpair(selItem);
            }

            CheckAllEqipped();
            UpdateProductSet();

            RefreshTab();
        }

        internal void btnUnpairAllMSG_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGrid.RowCount; i++)
            {
                var selItem = dataGrid.Rows[i].DataBoundItem as StockItem;

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
                    dataGrid.Refresh();
                    dataCSV.Refresh();
                    dataGridInvItems.Refresh();
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
            var selCells = dataGridInvItems.SelectedCells;
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
                dataGrid.Refresh();
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
                dataGrid.Refresh();
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
                if (!item.Equipped || item.Cancelled)
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

                if (item.Country == Country.Hungary)
                {
                    if (item.OrderPaymentMethod.ToLower().Contains("cashondelivery"))
                    {
                        shipper.AdressId = "D-COD";
                        shipper.ParcelType = item.OrderGrandTotal;
                    }
                    else
                    {
                        shipper.AdressId = "COD";
                        shipper.ParcelType = "";
                    }
                    shipper.ParcelWeight = "";
                    shipper.NrOfTotal = orderNr;//D
                    shipper.ParcelCOD = orderNr;//E
                    shipper.ParcelCODAmount = "";//F
                    shipper.ParcelCODCurrency = item.ShippingName;//G
                    shipper.ParcelCODvarSym = "";//H
                    shipper.ParcelCODCardPay = item.ShippingStreet;//I
                    shipper.ParcelOrderNumber = "";//J
                    shipper.CustRef = "H";//K
                    shipper.CustName = item.ShippingZip;//L
                    shipper.CustStreet = item.ShippingCity;//M
                    shipper.CustZip = item.ShippingPhoneNumber;//N
                    shipper.CustCity = "";//O
                    shipper.CustCountry = item.CustomerEmail;//P
                    shipper.CustPhone = "Hívás!! Hívás!! Hívás!! Hívás!!";//Q
                    shipper.CustEmail = "";
                    shipper.SMSPreAdvice = "";
                    shipper.PhoneNumber = "";
                    shipper.ParcelNote = "";
                }

                if (item.Country == Country.Hungary)
                    outdataHU.Add(shipper);
                else if (item.Country == Country.Poland)
                    outdataPL.Add(shipper);
                else if (item.Country == Country.CzechRepublic)
                    outdataCZ.Add(shipper);
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

            // formatovanie dat
            foreach (var shipper in outdata)
            {
                sb.Append(Environment.NewLine);
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
                sb.Append(shipper.CustPhone);
                if (country != Country.Hungary)
                {
                    sb.Append(";");
                    sb.Append(shipper.CustEmail + ";");
                    sb.Append(shipper.SMSPreAdvice + ";");
                    sb.Append(shipper.PhoneNumber + ";");
                    sb.Append(shipper.ParcelNote);
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

            if (country != Country.Hungary)
                File.WriteAllText(outDir + fname, sb.ToString(), Encoding.GetEncoding(1252));
            else
                File.WriteAllText(outDir + fname, sb.ToString());
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
            var selCells = dataCSV.SelectedCells;
            if (selCells == null || selCells.Count == 0)
                return;

            var toCopy = dataCSV.Rows[selCells[0].RowIndex].DataBoundItem as Invoice;

            var ds = GetInvoiceDS();
            if (ds == null)
                return;
            ds.Add(new Invoice(toCopy));
        }

        private void btnStockCopy_Click(object sender, EventArgs e)
        {
            var selCells = dataGrid.SelectedCells;
            if (selCells == null || selCells.Count == 0)
                return;

            var toCopy = dataGrid.Rows[selCells[0].RowIndex].DataBoundItem as StockItem;

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
        Reader
    }

    class CustomDataGridView : DataGridView
    {
        public CustomDataGridView()
        {
            DoubleBuffered = true;
        }
    }
}
