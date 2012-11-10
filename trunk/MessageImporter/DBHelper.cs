using System;
using System.Windows.Forms;

namespace MessageImporter
{
    public partial class DBHelper : Form
    {
        public DBHelper()
        {
            InitializeComponent();
            this.Text += " - " + DBProvider.DataSource;
        }

        private void btnExecQuery_Click(object sender, EventArgs e)
        {
            var q = txtQuery.SelectedText;

            try
            {
                var ds = DBProvider.ExecuteQuery(q);
                if (ds != null)
                    gridDBres.DataSource = ds.Tables[0];
            }
            catch (Exception ex)
            {
                txtNonQueryRes.AppendText(Environment.NewLine + ex.ToString());
            }
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

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtNonQueryRes.Clear();
            txtQuery.Clear();
        }
    }
}
