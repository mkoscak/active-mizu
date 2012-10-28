using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MessageImporter
{
    public partial class ProductChooser : Form
    {
        public ProductChooser()
        {
            InitializeComponent();

            foundText = lblFound.Text;
        }

        List<StockItem> orderItems = null;
        StockItem selected = null;
        string foundText;

        public StockItem Selected 
        {
            get
            {
                return selected;
            }

            set
            {
                selected = value;
            }
        }

        public void SetOrderItems(List<StockItem> orders, CSVFileItem product)
        {
            orderItems = orders;

            foreach (var oi in orderItems)
            {
                cbProducts.Items.Add(oi);
            }
            cbProducts.SelectedIndex = 0;

            lblFound.Text = foundText.Replace("_", product.ToString()); 
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            selected = null;

            this.Close();
            this.Dispose();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            selected = cbProducts.SelectedItem as StockItem;

            this.Close();
            this.Dispose();
        }
    }
}
