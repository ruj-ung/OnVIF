using GMap.NET;
using GMap.NET.MapProviders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PreviewDemo;
using CamView;
using System.Data.OleDb;
using PlayBackDemo;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System.IO;
using PineApple;

namespace CamView
{
    public partial class Form1 : Form
    {
        FilterForm filterfrm = new FilterForm();
        bool bShowFilter = false;
        List<Form> listFrm;
        private List<Camera> listCam = new List<Camera>();
        internal DataTable dtCam;
        uint m_nProvider;

        public uint m_nPointLimit;

        //string cnn = string.Format(
        //    "Provider=Microsoft.ACE.OLEDB.12.0;" +
        //    "data source={0}{1}{2};" +
        //    "Extended Properties=Excel 8.0;HDR=No",
        //    fileLocation, fileName, fileExtension);

        private static string hotkeyList =
                "Space  = Normal Speed\n" +
                "X = Single Frame Speed\n" +
                "A = Faster Speed\n" +
                "M = Minimize\n" +
                "V = Switch View\n" +
                "Z = Zoom In\n" +
                "Ctrl+Z = Zoom Out\n" +
                "Ctrl+F = On/Off Filter Window\n" +
                "F5 = Fit to Screen\n" +
                "F1 = Help and About PineApple Eyes";

        private GMapOverlay markers = new GMapOverlay("markersLayer");
        private GMapOverlay polygons = new GMapOverlay("polygonsLayer");
        Bitmap markerEW = (Bitmap)Image.FromFile("img/markerEW.png");
        Bitmap markerWE = (Bitmap)Image.FromFile("img/markerWE.png");
        Bitmap markerNS = (Bitmap)Image.FromFile("img/markerNS.png");
        Bitmap markerSN = (Bitmap)Image.FromFile("img/markerSN.png");
        public Form1()
        {
            InitializeComponent();
            this.Text = Program.version;
            m_nPointLimit = Program.PointLimit;
            m_nProvider = 1;
            listFrm = new List<Form>();
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            GMapProvider.WebProxy = WebRequest.GetSystemWebProxy();
            GMapProvider.WebProxy.Credentials = CredentialCache.DefaultNetworkCredentials;
            gmap.DragButton = MouseButtons.Left;
            //gmap.MapProvider = GMap.NET.MapProviders.BingMapProvider.Instance; 
            gmap.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            //gmap.MapProvider = GMap.NET.MapProviders.GoogleHybridMapProvider.Instance;
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;
            //gmap.SetPositionByKeywords("Bangkok, Thailand");
            double lat, lng, zoom;
            using (StreamReader reader = new StreamReader(@"PineApple.cfg"))
            {
                lat = Double.Parse(reader.ReadLine());
                lng = Double.Parse(reader.ReadLine());
                zoom = Double.Parse(reader.ReadLine());
            }
            //gmap.Position = new PointLatLng(16.4321336165282, 102.823630571365);
            gmap.Position = new PointLatLng(lat, lng);
            gmap.Zoom = zoom;
            gmap.ShowCenter = true;

            LoadData();
            //listCam = new List<Camera>();
            FillList(listCam);
            AddMarkers();

            gmap.Overlays.Add(markers);

            gmap.Overlays.Add(polygons);




            //gmap.ZoomAndCenterMarkers("markersLayer");


            //lat = points.Max(item => item.Lat);
            //lng = points.Min(item => item.Lng);

            //double height = lat - points.Min(item => item.Lat);
            //double width = points.Max(item => item.Lng) - lng;

            //var rect = new RectLatLng(lat, lng, width, height);
            //gmap.SetZoomToFitRect(getPolygonBBox());

            LogoForm logo = new LogoForm();
            logo.TopMost = true;
            logo.Show();

            filterfrm.parent = this;
        }

        internal int GetMarkerCount()
        {
            return markers.Markers.Count;
        }
        internal int GetMarkerCount(int stationID)
        {
            return markers.Markers.Count;
        }
        internal int GetMarkerCount(List<TreeNode> nodes)
        {
            Program.CheckLicense();
            markers.Clear();
            listCam.Clear();
            foreach (TreeNode node in nodes)
            {
                List<Camera> tempList = new List<Camera>();
                DataRow[] rows = dtCam.Select(string.Format("Station={0}", node.Tag.ToString()));
                FillList(tempList, rows);
                listCam.AddRange(tempList);
            }
            AddMarkers();
            return markers.Markers.Count;
        }


        internal void AddMarkers()
        {
            markers.Clear();
            int nTag;
            for (nTag = 0; nTag < listCam.Count; nTag++)
            {
                Camera cam = listCam[nTag];
                Bitmap bmap;
                switch (cam.Direction)
                {
                    case "EW":
                        bmap = markerEW;
                        break;
                    case "WE":
                        bmap = markerWE;
                        break;
                    case "NS":
                        bmap = markerNS;
                        break;
                    case "SN":
                        bmap = markerSN;
                        break;
                    default:
                        bmap = markerEW;
                        break;
                }
                markers.Markers.Add(new GMap.NET.WindowsForms.Markers.GMarkerGoogle(
                        new GMap.NET.PointLatLng(cam.Lat, cam.Long),
                        //GMap.NET.WindowsForms.Markers.GMarkerGoogleType.red_dot));
                        bmap));
                markers.Markers[nTag].Tag = nTag;
                DataRow[] rows = FilterForm.st.dtStation.Select(string.Format("ID={0}", cam.stationID.ToString()));
                markers.Markers[nTag].ToolTipText = cam.Name + "\n" + rows[0].ItemArray[1].ToString();
                switch (cam.Owner)
                {
                    case 0:
                        markers.Markers[nTag].ToolTip.Fill = Brushes.Red;
                        markers.Markers[nTag].ToolTip.Foreground = Brushes.White;
                        break;
                    case 1:
                        markers.Markers[nTag].ToolTip.Fill = Brushes.Blue;
                        markers.Markers[nTag].ToolTip.Foreground = Brushes.White;
                        break;
                    case 2:
                        markers.Markers[nTag].ToolTip.Fill = Brushes.Aqua;
                        markers.Markers[nTag].ToolTip.Foreground = Brushes.Black;
                        break;
                    default:
                        markers.Markers[nTag].ToolTip.Fill = Brushes.Black;
                        markers.Markers[nTag].ToolTip.Foreground = Brushes.White;
                        break;
                }
                //markers.Markers[nTag].ToolTip.Fill = Brushes.Red;
                //markers.Markers[nTag].ToolTip.Foreground = Brushes.White;
                //markers.Markers[nTag].ToolTip.TextPadding = new Size(10, 0);

            }
        }

        private RectLatLng getPolygonBBox()
        {
            List<PointLatLng> points = polygons.Polygons[0].Points;
            double lat = points.Max(item => item.Lat);
            double lng = points.Min(item => item.Lng);

            double height = lat - points.Min(item => item.Lat);
            double width = points.Max(item => item.Lng) - lng;

            var rect = new RectLatLng(lat, lng, width, height);
            return rect;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            //DataRow[] rows;
            switch (keyData)
            {
                case Keys.F | Keys.Control:
                    button1_Click_1(this, new EventArgs());
                    break;
                case Keys.F5:
                    gmap.ZoomAndCenterMarkers("markersLayer");
                    break;

                case Keys.F1:
                    string str = "This program was developed by Mr.Rujchai Ung-arunyawee\nEmail: rujchai@gmail.com\n" +
                        "\u00A9 \u2777\u24FF\u2777\u24FF All rights reserved.\n\n" +
                        "Hot keys:\n" + hotkeyList;
                    MessageBox.Show(str, string.Format("About PineApple Eyes Version {0}", Program.version), 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case Keys.V:
                    m_nProvider++;
                    if (m_nProvider == 4) m_nProvider = 1;
                    switch(m_nProvider)
                    {
                        case 1:
                            gmap.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
                            break;
                        case 2:
                            gmap.MapProvider = GMap.NET.MapProviders.GoogleHybridMapProvider.Instance;
                            break;
                        case 3:
                            gmap.MapProvider = GMap.NET.MapProviders.GoogleSatelliteMapProvider.Instance;
                            break;

                    }
                    //if (gmap.MapProvider == GMap.NET.MapProviders.GoogleMapProvider.Instance)
                    //    gmap.MapProvider = GMap.NET.MapProviders.GoogleHybridMapProvider.Instance;
                    //    //gmap.MapProvider = GMap.NET.MapProviders.GoogleSatelliteMapProvider.Instance;
                    //else
                    //    gmap.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
                    break;
                case Keys.Escape:
                    DialogResult result = MessageBox.Show("คุณต้องการออกจากโปรแกรมหรือไม่?", "ปิดโปรแกรม", 
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        using (StreamWriter sw = new StreamWriter(@"PineApple.cfg", false))
                        {
                            sw.WriteLine(gmap.Position.Lat.ToString());
                            sw.WriteLine(gmap.Position.Lng.ToString());
                            sw.WriteLine(gmap.Zoom.ToString());
                        }
                        this.Close();
                    }
                    break;
                case Keys.M:
                    MinimizeChildForms();
                    this.WindowState = FormWindowState.Minimized;
                    break;
                case Keys.Delete:
                    clearPolygons();
                    break;
                case Keys.Add:
                case Keys.Right:
                    IncreaseSize();
                    break;
                case Keys.Z:
                    gmap.Zoom++;
                    break;
                case Keys.Subtract:
                case Keys.Left:
                    DecreaseSize();
                    break;
                case Keys.Control | Keys.Z:
                    gmap.Zoom--;
                    break;
                default:
                    return base.ProcessCmdKey(ref msg, keyData);
            }
            return true;

        }

        private void IncreaseSize()
        {
            var forms = Application.OpenForms;
            foreach (Form form in forms)
            {
                if (form.GetType() == typeof(CamView.CamForm) || form.GetType() == typeof(CamView.CamFormDH) || form.GetType() == typeof(CamView.CamFormOV))
                {
                    form.Width = (int)(1.1 * form.Width);
                    form.Height = (int)(1.1 * form.Height);
                }
            }
        }
        private void DecreaseSize()
        {
            var forms = Application.OpenForms;
            foreach(Form form in forms)
            {
                if (form.GetType() == typeof(CamView.CamForm) || form.GetType() == typeof(CamView.CamFormDH) || form.GetType() == typeof(CamView.CamFormOV))
                {
                    form.Width = (int)(0.9 * form.Width);
                    form.Height = (int)(0.9 * form.Height);
                }
            }
        }

        private void MinimizeChildForms()
        {
            foreach(Form frm in this.MdiChildren)
            {
                MessageBox.Show(frm.Text);
            }
        }

        internal void FillList(List<Camera> list, DataRow[] rows)
        {
            list.Clear();
            int nCount = 1;
            foreach (DataRow row in rows)
            {
                if (nCount > m_nPointLimit) break;
                Camera cam = new Camera();

                cam.Name = row.ItemArray[0].ToString();
                cam.IPAddress = row.ItemArray[1].ToString();
                cam.User = row.ItemArray[2].ToString();
                if (cam.IPAddress == "redcam.dfdm.in.th" || cam.IPAddress == "10.101.0.200")
                    cam.Password = "digitalfusion123";
                else
                    cam.Password = row.ItemArray[3].ToString();
                cam.Port = int.Parse(row.ItemArray[4].ToString());
                cam.Channel = int.Parse(row.ItemArray[5].ToString());
                cam.Lat = (double)row.ItemArray[6];
                cam.Long = (double)row.ItemArray[7];
                cam.Direction = row.ItemArray[8].ToString();
                cam.Company = row.ItemArray[9].ToString();
                cam.Owner = uint.Parse(row.ItemArray[10].ToString());
                cam.stationID = int.Parse(row.ItemArray[11].ToString());
                list.Add(cam);
            }
        }

        internal void FillList(List<Camera> list)
        {
            list.Clear();
            int nCount = 1;
            foreach (DataRow row in dtCam.Rows)
            {
                if (nCount > m_nPointLimit) break;
                Camera cam = new Camera();

                cam.Name = row.ItemArray[0].ToString();
                cam.IPAddress = row.ItemArray[1].ToString();
                cam.User = row.ItemArray[2].ToString();
                if (cam.IPAddress == "redcam.dfdm.in.th" || cam.IPAddress == "10.101.0.200")
                    cam.Password = "digitalfusion123";
                else
                    cam.Password = row.ItemArray[3].ToString();
                cam.Port = int.Parse(row.ItemArray[4].ToString());
                cam.Channel = int.Parse(row.ItemArray[5].ToString());
                cam.Lat = (double)row.ItemArray[6];
                cam.Long = (double)row.ItemArray[7];
                cam.Direction = row.ItemArray[8].ToString();
                cam.Company = row.ItemArray[9].ToString();
                cam.Owner = uint.Parse(row.ItemArray[10].ToString());
                cam.stationID = int.Parse(row.ItemArray[11].ToString());

                list.Add(cam);
                nCount++;
            }
        }

        private void LoadData()
        {
            dtCam = new DataTable();
            var fileName = @"ipcamera.xlsx";
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
                        adapter.Fill(dtCam);
                    }
                }
                catch (OleDbException ex)
                {
                    MessageBox.Show(ex.Message);
                    throw;
                }
            }
            //string cnn = string.Format(
            //    "Provider=Microsoft.ACE.OLEDB.12.0;" +
            //    "data source={0}{1}{2};" +
            //    "Extended Properties=Excel 8.0;HDR=No",
            //    fileLocation, fileName, fileExtension);
        }
        private void SaveData()
        {
            dtCam = new DataTable();
            var fileName = @"ipcamera.xlsx";
            var connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileName + ";Extended Properties=\"Excel 12.0;IMEX=3;HDR=YES;TypeGuessRows=0;ImportMixedTypes=Text\""; 
            using (var conn = new OleDbConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    OleDbCommand commande = new OleDbCommand(
                      "INSERT INTO [Sheet1$](IPAddress,User,Password,) VALUES ('A3','B3','C3');", conn);
                    commande.ExecuteNonQuery();
                    //"INSERT INTO [Sheet1$](IPAddress,User,Password,Port,Channel,Lat,Long,Direction,Brand) VALUES ('A3','B3','C3');", conn);

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
        private void button1_Click(object sender, EventArgs e)
        {
            gmap.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            //GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;
            gmap.Position = new PointLatLng(23.0, 102.0);
            gmap.MaxZoom = 10;
            gmap.Zoom = 0;
            ;
        }


        private void gmap_OnMarkerClick(GMapMarker item, MouseEventArgs e)
        {
            Size size = this.Size;
            Program.CheckLicense();
            if (polygons.Polygons.Count  >  0 &&  
                PointInPolygon(item.Position, polygons.Polygons[0]))
                    return;
            switch (e.Button)
            {
                case MouseButtons.Right:
                    if (listCam[(int)item.Tag].Company == "HK")
                    {
                        PlayBackForm frm1 = new PlayBackForm();
                        frm1.Text += "-" + listCam[(int)item.Tag].Name;
                        frm1.m_IPAddress = listCam[(int)item.Tag].IPAddress;
                        frm1.m_nPort = (Int16)listCam[(int)item.Tag].Port;
                        frm1.m_User = listCam[(int)item.Tag].User;
                        frm1.m_Password = listCam[(int)item.Tag].Password;
                        frm1.m_nChannel = (Int16)listCam[(int)item.Tag].Channel;
                        frm1.StartPosition = FormStartPosition.Manual;
                        Point form_pt1 = e.Location;
                        Point screen_pt1 = this.PointToScreen(form_pt1);
                        frm1.Location = screen_pt1;
                        frm1.TopMost = true;
                        frm1.Show();
                    }
                    else if ((listCam[(int)item.Tag].Company == "DH"))
                    {
                        PlayBackFormDH frm1 = new PlayBackFormDH();
                        frm1.Text += "-" + listCam[(int)item.Tag].Name;
                        frm1.m_IPAddress = listCam[(int)item.Tag].IPAddress;
                        frm1.m_nPort = (ushort)(Int16)listCam[(int)item.Tag].Port;
                        frm1.m_User = listCam[(int)item.Tag].User;
                        frm1.m_Password = listCam[(int)item.Tag].Password;
                        frm1.m_nChannel = (Int16)listCam[(int)item.Tag].Channel;
                        Point form_pt1 = e.Location;
                        Point screen_pt1 = this.PointToScreen(form_pt1);
                        frm1.StartPosition = FormStartPosition.Manual;
                        frm1.Location = screen_pt1;
                        frm1.TopMost = true;
                        frm1.Show();
                    }
                    else
                    {
                        MessageBox.Show("Sorry! ONVIF does not support playback.");
                    }
                    break;
                case MouseButtons.Left:
                    if ((int)item.Tag < listCam.Count)
                    {
                        if (isOpen(listCam[(int)item.Tag].Name, e.Location))
                            return;
                        else
                        {
                            if(listCam[(int)item.Tag].Company == "HK")
                            {
                                CamForm frm = new CamForm();
                                frm.Text = listCam[(int)item.Tag].Name;
                                frm.m_IPAddress = listCam[(int)item.Tag].IPAddress;
                                frm.m_nPort = (UInt16)listCam[(int)item.Tag].Port;
                                frm.m_User = listCam[(int)item.Tag].User;
                                frm.m_Password = listCam[(int)item.Tag].Password;
                                frm.m_nChannel = (Int16)listCam[(int)item.Tag].Channel;
                                Point form_pt = e.Location;

                                // Translate into the screen coordinate system.
                                frm.StartPosition = FormStartPosition.Manual;
                                Point screen_pt = this.PointToScreen(form_pt);
                                frm.Location = screen_pt;
                                frm.TopMost = true;
                                frm.Show();
                            }
                            else if((listCam[(int)item.Tag].Company == "DH"))
                            {

                                CamFormDH frm = new CamFormDH();
                                frm.Text = listCam[(int)item.Tag].Name;
                                frm.m_IPAddress = listCam[(int)item.Tag].IPAddress;
                                frm.m_nPort = (ushort)listCam[(int)item.Tag].Port;
                                frm.m_User = listCam[(int)item.Tag].User;
                                frm.m_Password = listCam[(int)item.Tag].Password;
                                frm.m_nChannel = (Int16)listCam[(int)item.Tag].Channel;
                                Point form_pt = e.Location;

                                // Translate into the screen coordinate system.
                                Point screen_pt = this.PointToScreen(form_pt);
                                frm.StartPosition = FormStartPosition.Manual;
                                frm.Location = screen_pt;
                                frm.TopMost = true;
                                frm.Show();
                            }
                            else
                            {
                                CamFormOV frm = new CamFormOV();
                                frm.Text = listCam[(int)item.Tag].Name;
                                frm.m_IPAddress = listCam[(int)item.Tag].IPAddress;
                                frm.m_nPort = (ushort)listCam[(int)item.Tag].Port;
                                frm.m_User = listCam[(int)item.Tag].User;
                                frm.m_Password = listCam[(int)item.Tag].Password;
                                frm.m_nChannel = (Int16)listCam[(int)item.Tag].Channel;
                                Point form_pt = e.Location;

                                // Translate into the screen coordinate system.
                                Point screen_pt = this.PointToScreen(form_pt);
                                frm.StartPosition = FormStartPosition.Manual;
                                frm.Location = screen_pt;
                                frm.TopMost = true;
                                frm.Show();

                            }

                        }
                    }
                    else
                        MessageBox.Show("Ha Ha");
                    break;
            }
            this.Focus();
            return;
        }
        private bool isOpen(string name, Point location)
        {
            bool bOpen = false;
            foreach (Form f in Application.OpenForms)
            {
                if (f.Text == name)
                {
                    bOpen = true;
                    f.Location = location;
                    f.Focus();
                    break;
                }
            }

            if (bOpen == false)
            {
                return bOpen;
            }
            return bOpen;
        }

        private void gmap_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            PointLatLng clickPoint = (gmap.FromLocalToLatLng(e.X, e.Y));
            //MessageBox.Show(string.Format("Lat:    {0}\nLong: {1}", clickPoint.Lat, clickPoint.Lng));
            CamInfoForm frm = new CamInfoForm();
            frm.m_Lat = clickPoint.Lat;
            frm.m_Long = clickPoint.Lng;
            Point form_pt1 = e.Location;
            Point screen_pt1 = this.PointToScreen(form_pt1);
            frm.StartPosition = FormStartPosition.Manual;
            frm.Location = screen_pt1;
            frm.ShowDialog();
        }

        private void gmap_OnMarkerDoubleClick(GMapMarker item, MouseEventArgs e)
        {
            MessageBox.Show("asas");
            PlayBackForm frm1 = new PlayBackForm();
            frm1.Text += "-" + listCam[(int)item.Tag].Name;
            frm1.m_IPAddress = listCam[(int)item.Tag].IPAddress;
            frm1.m_nPort = (Int16)listCam[(int)item.Tag].Port;
            frm1.m_User = listCam[(int)item.Tag].User;
            frm1.m_Password = listCam[(int)item.Tag].Password;
            frm1.m_nChannel = (Int16)listCam[(int)item.Tag].Channel;
            Point form_pt1 = e.Location;
            Point screen_pt1 = this.PointToScreen(form_pt1);
            frm1.StartPosition = FormStartPosition.Manual;
            frm1.Location = screen_pt1;
            frm1.TopMost = true;
            frm1.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (StreamWriter sw = new StreamWriter(@"PineApple.cfg", false))
            {
                sw.WriteLine(gmap.Position.Lat.ToString());
                sw.WriteLine(gmap.Position.Lng.ToString());
                sw.WriteLine(gmap.Zoom.ToString());
            }
            this.Close();
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            FormCollection formCollect =  Application.OpenForms;
            foreach(Form f in formCollect)
            {
                if(f.GetType().Name != "Form1")
                {
                    f.WindowState = FormWindowState.Minimized;
                }
            }
        }

        private bool PointInPolygon(PointLatLng pt, GMapPolygon polygon)
        {
            bool inside = false;
            int i, j, nvert;
            nvert = polygon.Points.Count;
            for (i = 0, j = nvert - 1; i < nvert; i++)
            {
                PointLatLng pi = polygon.Points[i];
                PointLatLng pj = polygon.Points[j];
                if (((pi.Lat > pt.Lat) != (pj.Lat > pt.Lat)) &&
                    (pt.Lng < (pj.Lng - pi.Lng) * (pt.Lat - pi.Lat) / (pj.Lat - pi.Lat) + pi.Lng))
                    inside = !inside;
                j = i;
            }
            return inside;
        }

        bool drag = false;
        PointLatLng p1, p2, p3, p4;

        private void gmap_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                p1 = gmap.FromLocalToLatLng(e.X, e.Y);
                drag = true;
            }
        }

        private void gmap_OnPolygonClick(GMapPolygon item, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                clearPolygons();
                return;
            }
            int nCamera =markers.Markers.Count;
            int nCount = 0;
            foreach(GMapMarker mark in markers.Markers)
            {
                if (PointInPolygon(mark.Position, item))
                {
                    nCount++;
                    //CamForm frm = new CamForm();
                    //frm.Text = listCam[(int)mark.Tag].Name;
                    //frm.m_IPAddress = listCam[(int)mark.Tag].IPAddress;
                    //frm.m_nPort = (Int16)listCam[(int)mark.Tag].Port;
                    //frm.m_User = listCam[(int)mark.Tag].User;
                    //frm.m_Password = listCam[(int)mark.Tag].Password;
                    //frm.m_nChannel = (Int16)listCam[(int)mark.Tag].Channel;
                    //GPoint gpoint =  gmap.FromLatLngToLocal(mark.Position);
                    //Point form_pt = new Point((int)gpoint.X, (int)gpoint.Y);

                    //// Translate into the screen coordinate system.
                    //Point screen_pt = this.PointToScreen(form_pt);
                    //frm.StartPosition = FormStartPosition.Manual;
                    //frm.Location = form_pt;
                    //frm.TopMost = true;
                    //frm.Show();
                }
            }
            MessageBox.Show(string.Format("จำนวนกล้องรวม = {0}", nCount));
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            switch(this.WindowState)
            {
                case FormWindowState.Maximized:
                    FormCollection formCollect = Application.OpenForms;
                    foreach (Form f in formCollect)
                    {
                        if (f.GetType().Name != "Form1")
                        {
                            f.WindowState = FormWindowState.Normal;
                        }
                    }
                    break;
                case FormWindowState.Minimized:
                    break;
                case FormWindowState.Normal:
                    break;
            }

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            bShowFilter = !bShowFilter;
            if(!bShowFilter)
            {
                filterfrm.TopMost = false;
                filterfrm.Hide();
            }
            else
            {
                filterfrm.TopMost = true;
                filterfrm.Show();
            }
            this.Focus();
        }

        private void buttonFit_Click(object sender, EventArgs e)
        {
            gmap.ZoomAndCenterMarkers("markersLayer");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label2.Text = "PineApple V" + Program.version;
        }

        private void gmap_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && drag == true)
            {
                p3 = gmap.FromLocalToLatLng(e.X, e.Y);
                p2 = new PointLatLng(p1.Lat, p3.Lng);
                p4 = new PointLatLng(p3.Lat, p1.Lng);
                List<PointLatLng> points = new List<PointLatLng>() { p1, p2, p3, p4 };
                clearPolygons();

                GMapPolygon polygon = new GMapPolygon(points, "Select Area");
                polygon.Fill = new SolidBrush(Color.FromArgb(50, Color.Red));
                polygon.Stroke = new Pen(Color.Red, 1);
                polygon.Tag = 1;
                polygon.IsHitTestVisible = true;
                polygons.Polygons.Add(polygon);
            }
        }

        private void gmap_MouseUp(object sender, MouseEventArgs e)
        {
            PointLatLng p = gmap.FromLocalToLatLng(e.X, e.Y);
            if (e.Button == MouseButtons.Right && drag == true && polygons.Polygons.Count > 0 )
            {
                gmap.SetZoomToFitRect(getPolygonBBox());
                int nCamera = markers.Markers.Count;
                int nCount = 0;
                foreach (GMapMarker mark in markers.Markers)
                    if (PointInPolygon(mark.Position, polygons.Polygons[0]))
                        nCount++;
                MessageBox.Show(string.Format("จำนวนกล้องรวม = {0}", nCount));
                drag = false;
            }

        }

        private void clearPolygons()
        {
            foreach (GMapOverlay overlay in gmap.Overlays)
            {
                if (overlay.Id == "polygonsLayer")
                {
                    overlay.Polygons.Clear();
                }
            }
        }


    }

}
