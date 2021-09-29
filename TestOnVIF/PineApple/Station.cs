using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CamView
{
    class Station
    {
        public DataTable dtStation;
        public Station()
        {
            dtStation = new DataTable();
            LoadData();
        }
        private void LoadData()
        {
            var fileName = @"Stations.xlsx";
            var connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileName + ";Extended Properties=\"Excel 12.0;IMEX=1;HDR=YES;TypeGuessRows=0;ImportMixedTypes=Text\""; ;
            using (var conn = new OleDbConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    var sheets = conn.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT * FROM [" + sheets.Rows[0]["TABLE_NAME"].ToString() + "] ";

                        var adapter = new OleDbDataAdapter(cmd);
                        adapter.Fill(dtStation);
                    }
                }
                catch (OleDbException ex)
                {
                    MessageBox.Show(ex.Message);
                    throw;
                }
            }

        }


        private void LoadData1()
        {
            dtStation.Columns.Add("ID", typeof(int));
            dtStation.Columns.Add("Name", typeof(string));
            dtStation.PrimaryKey = new DataColumn[] { dtStation.Columns["ID"] };

            dtStation.Rows.Add(new Object[] { 401001, "สภ.เมืองขอนแก่น" });
            dtStation.Rows.Add(new Object[] { 401002, "สภ.บ้านไผ่" });
            dtStation.Rows.Add(new Object[] { 401003, "สภ.มหาวิทยาลัยขอนแก่น" });

            dtStation.Rows.Add(new Object[] { 402001, "สภ.เมืองอุดรธานี" });
            dtStation.Rows.Add(new Object[] { 403001, "สภ.เมืองกาฬสินธุ์" });
            dtStation.Rows.Add(new Object[] { 404001, "สภ.เมืองร้อยเอ็ด" });
            dtStation.Rows.Add(new Object[] { 405001, "สภ.เมืองหนองคาย" });
            dtStation.Rows.Add(new Object[] { 406001, "สภ.เมืองสกลนคร" });
            dtStation.Rows.Add(new Object[] { 407001, "สภ.เมืองมหาสารคาม" });
            dtStation.Rows.Add(new Object[] { 408001, "สภ.เมืองเลย" });
            dtStation.Rows.Add(new Object[] { 409001, "สภ.เมืองนครพนม" });

            dtStation.Rows.Add(new Object[] { 410001, "สภ.เมืองมุกดาหาร" });
            dtStation.Rows.Add(new Object[] { 410101, "สภ.ผึ่งแดด" });
            dtStation.Rows.Add(new Object[] { 410102, "สภ.นาคำน้อย" });

            dtStation.Rows.Add(new Object[] { 411001, "สภ.เมืองหนองบัวลำภู" });
            dtStation.Rows.Add(new Object[] { 412001, "สภ.เมืองบึงกาฬ" });
        }

        public string GetName(int id)
        {
            DataRow row = dtStation.Rows.Find(id);            
            return row[1].ToString();
        }
    }
}
