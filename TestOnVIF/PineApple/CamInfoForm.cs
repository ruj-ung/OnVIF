using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CamView
{
    public partial class CamInfoForm : Form
    {
        public double m_Lat;
        public double m_Long;

        public CamInfoForm()
        {
            InitializeComponent();
        }

        private void CamInfoForm_Load(object sender, EventArgs e)
        {
            //txtBoxLat.Enabled = false;
            txtBoxLat.Text = m_Lat.ToString();
            //txtBoxLong.Enabled = false;
            txtBoxLong.Text = m_Long.ToString();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var fileName = @"test.xlsx";
            var connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileName + ";Extended Properties=\"Excel 12.0;IMEX=3;HDR=YES;TypeGuessRows=0;ImportMixedTypes=Text\""; ;
            using (var conn = new OleDbConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    var sheets = conn.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });

                    OleDbCommand commande = new OleDbCommand();
                    commande.Connection = conn;
                    commande.CommandText = "Insert into [Sheet1$] ([name],[date],[weight],[privacy]) values('Coding Defined', '27/6/2016', '74', 'no')";
                    commande.ExecuteNonQuery();

                    conn.Close();
                    conn.Dispose();
                }
                catch (OleDbException ex)
                {
                    MessageBox.Show(ex.Message);
                    throw;
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(string.Format("{0} {1}", txtBoxLat.Text, txtBoxLong.Text));
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.C:
                    Clipboard.SetText(string.Format("{0} {1}", txtBoxLat.Text, txtBoxLong.Text));
                    break;
                default:
                    return base.ProcessCmdKey(ref msg, keyData);
            }
            return true;

        }
    }
}
