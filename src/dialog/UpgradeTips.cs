using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class UpgradeTips : Form
    {
        public UpgradeTips()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Abort;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void UpgradeTips_Load(object sender, EventArgs e)
        {

        }

        public void setTips(String tips)
        {
            this.label2.Text = tips;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
