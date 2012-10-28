using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace MessageImporter
{
    public enum ResultRemoving
    {
        None,
        Selected,
        Unselected,
        All
    }

    public partial class Export : Form
    {
        public ResultRemoving ResultDelItems = ResultRemoving.None;
        internal BindingList<StockItem> dataToExport;

        const string ExportDir = "Export_data";

        public Export(BindingList<StockItem> data)
        {
            InitializeComponent();

            var dt = DateTime.Now;
            txtFileName.Text = ExportDir + "/export_" + dt.Day + "." + dt.Month + "_" + dt.Hour + "." + dt.Minute + "." + dt.Second + ".csvx";

            dataToExport = data;
        }

        void ExportData()
        {
            StreamWriter sw = null;

            try
            {
                if (!Directory.Exists(ExportDir))
                    Directory.CreateDirectory(ExportDir);

                sw = new StreamWriter(txtFileName.Text);
            
                foreach (var msg in dataToExport)
                {
                    if ( (rbSelected.Checked && !msg.Equipped) ||
                        (rbNonSelected.Checked && msg.Equipped) )
                        continue;

                    string line = string.Format("{0};{1};{2};{3};{4};{5};{6}", msg.ProductCode, msg.Description, msg.Ord_Qty, msg.Disp_Qty, msg.Price, msg.Total, msg.Currency);
                    sw.WriteLine(line);
                }

                MessageBox.Show("Export finished.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                if (sw != null)
                    sw.Close();
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (txtFileName.Text.EndsWith(".csvx") == false)
                txtFileName.Text += ".csvx";

            ExportData();

            if (chbRemoveAfterExport.Checked)
            {
                if (rbSelected.Checked)
                    ResultDelItems = ResultRemoving.Selected;
                else if (rbNonSelected.Checked)
                    ResultDelItems = ResultRemoving.Unselected;
                else
                    ResultDelItems = ResultRemoving.All;
            }
            else
            {
                ResultDelItems = ResultRemoving.None;
            }

            Close();
        }
    }
}
