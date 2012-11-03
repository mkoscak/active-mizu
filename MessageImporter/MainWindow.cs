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
        List<Invoice> allInvoices = new List<Invoice>();
        List<StockItem> allProducts = new List<StockItem>();

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
                    log("\t" + Functions.ExtractFileName(fileName));
                    files.Add(new FileItem(true, fileName));
                }

                log("");
                log("CSV files: ");
                foreach (string fileName in Directory.GetFiles(txtInputPath.Text, "*.csv", SearchOption.TopDirectoryOnly))
                {
                    log("\t" + Functions.ExtractFileName(fileName));
                    files.Add(new FileItem(true, fileName));
                }

                log("");
                log("CSVX files: ");
                foreach (string fileName in Directory.GetFiles(txtInputPath.Text, "*.csvx", SearchOption.TopDirectoryOnly))
                {
                    log("\t" + Functions.ExtractFileName(fileName));
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

            File.Move(fileName, procDir + @"\" + Functions.ExtractFileName(fileName));
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
                SetInvoiceDS(new BindingList<Invoice>(allInvoices));
                // datasource MSG sprav 
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

        void RefreshTab()
        {
            if (tabData.SelectedIndex == (int)Tabs.Invoices)
            {
                dataCSV.Refresh();
                dataGridInvItems.Refresh();
            }
            else
            {
                dataGrid.Refresh();
            }

            dataFiles.Refresh();
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
            allInvoices = new List<Invoice>();
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
                            allInvoices.Add(inv);

                        inv = new Invoice();

                        inv.TotQtyOrdered = item.TotQtyOrdered;
                        inv.BillingCity = item.BillingCity;
                        inv.BillingCompany = item.BillingCompany;
                        inv.BillingCountry = item.BillingCountry;
                        inv.BillingCountryName = item.BillingCountryName;
                        inv.BillingName = item.BillingName;
                        inv.BillingPhoneNumber = item.BillingPhoneNumber;
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
                        inv.ShippingPhoneNumber = item.ShippingPhoneNumber;
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
                        newItem.ItemSKU = item.ItemSKU;
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
                    allInvoices.Add(inv);
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
            foreach (var CSV in allInvoices)
            {
                foreach (var product in CSV.InvoiceItems)
                {
                    string productCode = ConvertInvoiceItem(product.ItemSKU);

                    foreach (var msg in allMessages)
                    {
                        var foundItems = msg.Items.Where(orderItem => orderItem.ProductCode.Contains(productCode)).ToList();
                        if (foundItems.Count == 1)
                        {
                            CompleteOrderItem(product, foundItems[0]);
                        }
                        else if (foundItems.Count == 0) // vsetko v pohode
                        {
                        }
                        else
                        {
                            /*ProductChooser pc = new ProductChooser();
                            pc.SetOrderItems(foundItems, product);

                            pc.ShowDialog(this);
                            
                            CompleteOrderItem(pc.Selected, product);*/
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

                order = decodeMessage(item.Body, file);
            }
            catch (System.Exception ex)
            {
                log(ex.Message);
                return order;
            }
            
            return order;
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

                dataPackItemType newDatapack = new dataPackItemType();
                newDatapack.ItemElementName = ItemChoiceType1.invoice;
                newDatapack.id = inv.OrderNumber;
                newDatapack.version = dataPackItemVersionType.Item20;
                
                // faktura
                invoiceType newInv = new invoiceType();
                newInv.version = invVersionType.Item20;

                // header
                newInv.invoiceHeader = new invoiceHeaderType();
                newInv.invoiceHeader.symVar = inv.OrderNumber;
                newInv.invoiceHeader.symPar = inv.OrderNumber;
                newInv.invoiceHeader.invoiceType = invoiceTypeType.issuedInvoice;
                newInv.invoiceHeader.dateAccounting = DateTime.Now;
                newInv.invoiceHeader.dateAccountingSpecified = true;
                newInv.invoiceHeader.dateOrder = DateTime.Now;
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
                newInv.invoiceHeader.text = inv.BillingName + ", " + inv.CustomerEmail;

                // header->identity
                newInv.invoiceHeader.partnerIdentity = new address();
                newInv.invoiceHeader.partnerIdentity.address1 = new addressType();
                newInv.invoiceHeader.partnerIdentity.address1.name = inv.BillingName;
                newInv.invoiceHeader.partnerIdentity.address1.street = inv.BillingStreet;
                newInv.invoiceHeader.partnerIdentity.address1.zip = inv.BillingZip;
                newInv.invoiceHeader.partnerIdentity.address1.phone = inv.BillingPhoneNumber;
                newInv.invoiceHeader.partnerIdentity.address1.city = inv.BillingCity;
                newInv.invoiceHeader.partnerIdentity.address1.company = inv.BillingCompany;
                newInv.invoiceHeader.partnerIdentity.shipToAddress = new shipToAddressType();
                newInv.invoiceHeader.partnerIdentity.shipToAddress.name = inv.ShippingName;
                newInv.invoiceHeader.partnerIdentity.shipToAddress.street = inv.ShippingStreet;
                newInv.invoiceHeader.partnerIdentity.shipToAddress.city = inv.ShippingCity;
                newInv.invoiceHeader.partnerIdentity.shipToAddress.zip = inv.ShippingZip;
                newInv.invoiceHeader.partnerIdentity.shipToAddress.company = inv.ShippingCompany;

                newInv.invoiceHeader.numberOrder = inv.OrderNumber;
                newInv.invoiceHeader.dateSpecified = true;
                newInv.invoiceHeader.date = DateTime.Parse(inv.OrderDate);
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
                        code = invItem.PairCode.Replace("/", "");

                    invoiceItemType xmlItem = new invoiceItemType();
                    xmlItem.code = code;
                    xmlItem.text = code;// invItem.ItemName;
                    xmlItem.quantity = float.Parse(invItem.ItemQtyOrdered);
                    xmlItem.unit = "ks";
                    xmlItem.homeCurrency = new typeCurrencyHomeItem();
                    xmlItem.homeCurrency.unitPriceSpecified = true;
                    xmlItem.homeCurrency.unitPrice = GetPrice(invItem.ItemPrice);
                    xmlItem.homeCurrency.priceVATSpecified = true;
                    xmlItem.homeCurrency.priceVAT = GetPrice(invItem.ItemTax);
                    xmlItem.homeCurrency.priceSpecified = true;
                    xmlItem.homeCurrency.price = GetPrice(invItem.ItemTotal) - GetPrice(invItem.ItemDiscount) / 1.2;
                    xmlItem.homeCurrency.priceSumSpecified = true;
                    xmlItem.homeCurrency.priceSum = xmlItem.homeCurrency.price;
                    xmlItem.percentVATSpecified = true;
                    xmlItem.percentVAT = Properties.Settings.Default.DPH_percent;

                    // stock item
                    xmlItem.stockItem = new stockItemType();
                    xmlItem.stockItem.stockItem = new stockRefType();
                    xmlItem.stockItem.stockItem.ids = code;

                    invItems.Add(xmlItem);
                }
                if (inv.OrderShippingMethod.Contains("freeshipping"))
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
                    shipping.homeCurrency.unitPrice = (Math.Ceiling(GetPrice(inv.OrderShipping) * 1.2 * 100)-1) / 100;

                    invItems.Add(shipping);
                }

                newInv.invoiceDetail = invItems.ToArray();

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

            MessageBox.Show("Invoice XML generated!", "Save XML", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        double GetPrice(string strPrice)
        {
            // cena obsahuje aj bodku aj ciarku, napr 1,000.25.. prvy znak vyhodime
            if (strPrice.Contains(',') && strPrice.Contains('.'))
            {
                int pointIndex = strPrice.LastIndexOf('.');
                int commaIndex = strPrice.LastIndexOf(',');

                if (pointIndex > commaIndex)
                    strPrice = strPrice.Replace(",", "");   // odstranime vsetky ciarky
                else
                    strPrice = strPrice.Replace(".", "");   // odstranime vsetky bodky
            }

            return double.Parse(strPrice.Replace('€', ' ').Trim().Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator).Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator));
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

            foreach (var prod in prodDS)
            {
                if (!prod.EquippedInv) // do exportu len produkty z vybavenych objednavok
                    continue;

                /////////////////////////////////////////////////// stock item

                var code = prod.ProductCode.Replace("/", "");

                dataPackItemType newDatapack = new dataPackItemType();
                newDatapack.id = code;
                newDatapack.ItemElementName = ItemChoiceType1.stock;
                newDatapack.version = dataPackItemVersionType.Item20;

                stockType stock = new stockType();
                stock.version = stkVersionType.Item20;

                // defaultna akcia add
                stock.actionType = new actionTypeType1();
                stock.actionType.Item = "";

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
                if (prod.State == StockItemState.PermanentStorage)
                    stock.stockHeader.storage.ids = "02";
                else
                    stock.stockHeader.storage.ids = Properties.Settings.Default.Storage;
                stock.stockHeader.typePrice = new refType();
                stock.stockHeader.typePrice.ids = Properties.Settings.Default.TypePrice;

                stock.stockHeader.purchasingPriceSpecified = true;
                stock.stockHeader.purchasingPrice = prod.PriceWithDeliveryEUR;
                stock.stockHeader.sellingPrice = GetPrice(prod.SellPriceInv);
                stock.stockHeader.limitMin = 0;
                stock.stockHeader.limitMax = 0;
                stock.stockHeader.orderName = prod.FromFile.FileName;
                stock.stockHeader.orderQuantitySpecified = true;
                stock.stockHeader.orderQuantity = prod.Ord_Qty;
                stock.stockHeader.shortName = code;
                stock.stockHeader.guaranteeType = guaranteeTypeType.year;
                stock.stockHeader.guaranteeTypeSpecified = true;
                stock.stockHeader.guarantee = "2";

                stock.stockHeader.yield = "604000";
                
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
            }

            var ticks = DateTime.Now.Ticks;

            // zabalenie prijemok
            dataPackItemType prijmekaDatapack = new dataPackItemType();
            prijmekaDatapack.id = "prijemka_" + ticks;
            prijmekaDatapack.ItemElementName = ItemChoiceType1.prijemka;
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
            invDatapack.ItemElementName = ItemChoiceType1.invoice;
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
            newInv.invoiceHeader.accounting.ids = "1";
            newInv.invoiceHeader.classificationVAT = new classificationVATType();
            newInv.invoiceHeader.classificationVAT.ids = Properties.Settings.Default.ClasifficationVAT;
            newInv.invoiceHeader.classificationVAT.classificationVATType1 = classificationVATTypeClassificationVATType.inland;
            newInv.invoiceHeader.text = "SportsDirect_" + DateTime.Now.ToString("ddMMyyyy");

            // polozky z faktury.. zatial fiktivne
            newInv.invoiceHeader.symVar = "symVar";
            newInv.invoiceHeader.symPar = "symPar";
            newInv.invoiceHeader.numberOrder = "numOrder";
            newInv.invoiceHeader.dateSpecified = true;
            newInv.invoiceHeader.date = DateTime.Now;
            newInv.invoiceHeader.paymentType = new paymentType();
            newInv.invoiceHeader.paymentType.ids = "cashondelivery";
                        
            newInv.invoiceDetail = invItems.ToArray();
            invDatapack.Item = newInv;
            invoices.Add(invDatapack);

            // polozky do xml
            dataPacks.AddRange(prijemky);
            dataPacks.AddRange(invoices);
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
            allProducts.Where(p => !paired.Contains(p.ProductCode) && p.ProductCode != null).ToList().ForEach(i => lbNonPaired.Items.Add(i.ProductCode));
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
                var selProd = dsMSG.Where(o => o.ProductCode == selprodcode).ToArray()[0];

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
            allProducts.Add(added);

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
                    item.PriceEUR = Math.Round(item.Price * kurz, 2);
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
                        item.PriceEUR = Math.Round(item.Price * kurz, 2);
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
                if (code.ToString().Contains(ConvertInvoiceItem(selInv.ItemSKU)))
                    lbFilteredItems.Items.Add(code.ToString());
            }
        }
    }

    /// <summary>
    /// Indexy tabov
    /// </summary>
    enum Tabs
    {
        Invoices,
        Stocks
    }

    class CustomDataGridView : DataGridView
    {
        public CustomDataGridView()
        {
            DoubleBuffered = true;
        }
    }
}
