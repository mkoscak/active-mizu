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
    public partial class TextInputForm : Form
    {
        public string ReturnText { get; set; }

        public TextInputForm(string label, string text)
        {
            InitializeComponent();

            label1.Text = label;
            if (text != null)
                txtInput.Text = text;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtInput.Text))
                ReturnText = txtInput.Text;

            Close();
        }
    }
}
