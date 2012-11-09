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
    public partial class DBHelper : Form
    {
        public DBHelper()
        {
            InitializeComponent();
        }

        private void btnExecQuery_Click(object sender, EventArgs e)
        {
            var q = txtQuery.SelectedText;

            var ds = DBProvider.ExecuteQuery(q);
            if (ds != null)
                gridDBres.DataSource = ds;
        }

        private void btnExecNonQuery_Click(object sender, EventArgs e)
        {
            var q = txtQuery.SelectedText;

            try
            {
                DBProvider.ExecuteNonQuery(q);

                txtNonQueryRes.AppendText(Environment.NewLine + "Exec successful!");
            }
            catch (Exception ex)
            {
                txtNonQueryRes.AppendText(Environment.NewLine + ex.ToString());
            }
            
        }
    }
}
