using PreviewDemo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using NetSDKCS;
using NetSDKCS.Control;
using System.Diagnostics;
using System.IO;

namespace PlayBackDemo
{
    public partial class PlayBackFormDH : Form
    {
        #region Field
        private const int m_WaitTime = 5000;
        private const double MAXSPEED = 16;
        private const double MINSPEED = 0.0625; // 1/16
        private const int DOWNLOAD_END = -1;
        private const int DOWNLOAD_FAILED = -2;
        private const double OSD_TIMER_INTERVAL = 62.5; // 1000ms/16(maxspeed)
        private static fDisConnectCallBack m_DisConnectCallBack;
        private static fHaveReConnectCallBack m_ReConnectCallBack;
        private static fTimeDownLoadPosCallBack m_DownloadPosCallBack;

        private IntPtr m_LoginID = IntPtr.Zero;
        private NET_DEVICEINFO_Ex m_DeviceInfo;
        private DateTime m_EndTime;
        private IntPtr m_PlayBackID = IntPtr.Zero;
        private bool m_IsPause = false;
        private System.Timers.Timer m_Timer = new System.Timers.Timer();
        private NET_TIME m_OsdTime = new NET_TIME();
        private NET_TIME m_OsdStartTime = new NET_TIME();
        private NET_TIME m_OsdEndTime = new NET_TIME();
        private double m_CurrentSpeed;
        private IntPtr m_DownloadID = IntPtr.Zero;
        private DateTime m_DateTimeNow;
        #endregion

        private uint iLastErr = 0;
        private string str;
        private string m_sTitle;
        private uint m_nSpeed = 1;

        private bool m_bPause = false;
        private bool m_bReverse = false;
        private bool m_bSound = false;
        private Int32 m_lPlayHandle = -1;
        private Int32 m_lDownHandle = -1;

        public string m_IPAddress = "192.168.44.201";
        public ushort m_nPort = 37777;
        public string m_User = "admin";
        public string m_Password = "admin123";
        public int m_nChannel = 0;

        public PlayBackFormDH()
        {
            InitializeComponent();
            this.Size = new Size(520, 380);
            m_Timer.Interval = OSD_TIMER_INTERVAL;
            m_Timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            this.Load += new EventHandler(PlayBackAndDownLoadDemo_Load);
        }
        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            NETClient.GetPlayBackOsdTime(m_PlayBackID, ref m_OsdTime, ref m_OsdStartTime, ref m_OsdEndTime);
            //playBackProgressBar.SeekByTime((DateTime)m_OsdTime.ToDateTime());
        }

        private void PlayBackAndDownLoadDemo_Load(object sender, EventArgs e)
        {
            m_DateTimeNow = DateTime.Now;
            //starttime_dateTimePicker.Value = m_DateTimeNow.AddHours(-1);
            m_DisConnectCallBack = new fDisConnectCallBack(DisConnectCallBack);
            m_ReConnectCallBack = new fHaveReConnectCallBack(ReConnectCallBack);
            m_DownloadPosCallBack = new fTimeDownLoadPosCallBack(DownLoadPosCallBack);
            try
            {
                NETClient.Init(m_DisConnectCallBack, IntPtr.Zero, null);
                NETClient.SetAutoReconnect(m_ReConnectCallBack, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Process.GetCurrentProcess().Kill();
            }
            m_sTitle = this.Text;
            this.Text = m_sTitle + string.Format(" @{0}x", m_nSpeed);
            Login();
            //InitOrLogoutUI();
        }
        #region CallBack
        private void DisConnectCallBack(IntPtr lLoginID, IntPtr pchDVRIP, int nDVRPort, IntPtr dwUser)
        {
            //this.BeginInvoke((Action)DisConnectUI);
        }

        private void ReConnectCallBack(IntPtr lLoginID, IntPtr pchDVRIP, int nDVRPort, IntPtr dwUser)
        {
            //this.BeginInvoke((Action)ReConnectUI);
        }

        private void DownLoadPosCallBack(IntPtr lPlayHandle, uint dwTotalSize, uint dwDownLoadSize, int index, NET_RECORDFILE_INFO recordfileinfo, IntPtr dwUser)
        {
            if (lPlayHandle == m_DownloadID)
            {
                int value = 0;
                if (DOWNLOAD_END == (int)dwDownLoadSize)
                {
                    value = DOWNLOAD_END;
                }
                else if (DOWNLOAD_FAILED == (int)dwDownLoadSize)
                {
                    value = DOWNLOAD_FAILED;
                }
                else
                {
                    value = (int)(dwDownLoadSize * 100 / dwTotalSize);
                }
                this.BeginInvoke((Action<int>)UpdateProgressBarUI, value);
            }
        }
        private void UpdateProgressBarUI(int value)
        {
            if (m_DownloadID != IntPtr.Zero)
            {
                if (DOWNLOAD_END == value)
                {
                    DownloadProgressBar.Value = 100;
                    NETClient.StopDownload(m_DownloadID);
                    MessageBox.Show(this, "Download End!");
                    m_DownloadID = IntPtr.Zero;
                    DownloadProgressBar.Value = 0;
                    label1.Visible = true;
                    label2.Visible = true;
                    dateTimeStart.Visible = true;
                    dateTimeEnd.Visible = true;
                    DownloadProgressBar.Visible = false;
                    btnDownload.Text = "Download";
                    return;
                }
                if (DOWNLOAD_FAILED == value)
                {
                    MessageBox.Show(this, "Download Failed!");
                    DownloadProgressBar.Value = 0;
                    label1.Visible = true;
                    label2.Visible = true;
                    dateTimeStart.Visible = true;
                    dateTimeEnd.Visible = true;
                    DownloadProgressBar.Visible = false;
                    btnDownload.Text = "Download";
                    return;
                }
                DownloadProgressBar.Value = value;
            }
        }
        #endregion
        private void Login()
        {
            if (IntPtr.Zero == m_LoginID)
            {
                m_DeviceInfo = new NET_DEVICEINFO_Ex();
                m_LoginID = NETClient.Login(m_IPAddress, m_nPort, m_User, m_Password, EM_LOGIN_SPAC_CAP_TYPE.TCP, IntPtr.Zero, ref m_DeviceInfo);
                if (IntPtr.Zero == m_LoginID)
                {
                    MessageBox.Show(this, NETClient.GetLastError());
                    return;
                }
                return;
                //LoginUI();
            }
            else
            {
                bool result = NETClient.Logout(m_LoginID);
                if (!result)
                {
                    MessageBox.Show(this, NETClient.GetLastError());
                    return;
                }
                m_LoginID = IntPtr.Zero;
                return;
                //InitOrLogoutUI();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(button2.Text == "Play")
            {
                button2.Text = "Stop";
                if (IntPtr.Zero != m_PlayBackID)
                {
                    NETClient.PlayBackControl(m_PlayBackID, PlayBackType.Stop);
                    //this.BeginInvoke((Action)PictureBoxRefresh);
                }
                NET_IN_PLAY_BACK_BY_TIME_INFO stuInfo = new NET_IN_PLAY_BACK_BY_TIME_INFO();
                NET_OUT_PLAY_BACK_BY_TIME_INFO stuOut = new NET_OUT_PLAY_BACK_BY_TIME_INFO();
                stuInfo.stStartTime = NET_TIME.FromDateTime(dateTimeStart.Value);
                stuInfo.stStopTime = NET_TIME.FromDateTime(dateTimeEnd.Value);
                stuInfo.hWnd = VideoPlayWnd.Handle;
                stuInfo.cbDownLoadPos = null;
                stuInfo.dwPosUser = IntPtr.Zero;
                stuInfo.fDownLoadDataCallBack = null;
                stuInfo.dwDataUser = IntPtr.Zero;
                stuInfo.nPlayDirection = 0;
                stuInfo.nWaittime = m_WaitTime;

                m_PlayBackID = NETClient.PlayBackByTime(m_LoginID, m_nChannel, stuInfo, ref stuOut);
                if (IntPtr.Zero == m_PlayBackID)
                {
                    MessageBox.Show(this, NETClient.GetLastError());
                    return;
                }
                return;
            }
            if(button2.Text == "Stop")
            {
                button2.Text = "Play";
                StopPlayback();
                return;
            }
        }
        public void StopPlayback()
        {
            NETClient.PlayBackControl(m_PlayBackID, PlayBackType.Stop);
            m_PlayBackID = IntPtr.Zero;
            VideoPlayWnd.Invalidate();//Refresh window
        }
        private void btnDownload_Click(object sender, EventArgs e)
        {
            if(m_PlayBackID != IntPtr.Zero)
            {
                MessageBox.Show("Please Stop playing back!");
                return;
            }
            if(btnDownload.Text == "Download")
            {
                btnDownload.Text = "Stop";
                //set stream type
                EM_STREAM_TYPE streamType = EM_STREAM_TYPE.MAIN; //(EM_STREAM_TYPE)play_stream_comboBox.SelectedIndex + 1;
                IntPtr pStream = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
                Marshal.StructureToPtr((int)streamType, pStream, true);
                NETClient.SetDeviceMode(m_LoginID, EM_USEDEV_MODE.RECORD_STREAM_TYPE, pStream);

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = "data";
                saveFileDialog.Filter = "|*.dav";
                string path = AppDomain.CurrentDomain.BaseDirectory + "savedata";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                saveFileDialog.InitialDirectory = path;
                var res = saveFileDialog.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.OK)
                {

                    m_DownloadID = NETClient.DownloadByTime(m_LoginID, m_nChannel, EM_QUERY_RECORD_TYPE.ALL, dateTimeStart.Value, dateTimeEnd.Value, saveFileDialog.FileName, m_DownloadPosCallBack, IntPtr.Zero, null, IntPtr.Zero, IntPtr.Zero);
                    if (IntPtr.Zero == m_DownloadID)
                    {
                        saveFileDialog.Dispose();
                        MessageBox.Show(this, NETClient.GetLastError());
                        return;
                    }
                }
                else
                {
                    btnDownload.Text = "Download";
                    saveFileDialog.Dispose();
                    return;
                }
                saveFileDialog.Dispose();

                timerDownload.Interval = 1000;
                //timerDownload.Enabled = true;
                label1.Visible = false;
                label2.Visible = false;
                dateTimeStart.Visible = false;
                dateTimeEnd.Visible = false;
                DownloadProgressBar.Visible = true;
                return;
            }
            if(btnDownload.Text == "Stop")
            {
                btnDownload.Text = "Download";
                StopDownload();
                return;
            }

        }
        public void StopDownload()
        {
            if (m_DownloadID == IntPtr.Zero)
            {
                return;
            }
            bool ret = NETClient.StopDownload(m_DownloadID);
            if (!ret)
            {
                MessageBox.Show(this, NETClient.GetLastError());
                return;
            }
            m_DownloadID = IntPtr.Zero;



            //timerDownload.Stop();

            MessageBox.Show("The downloading has been stopped succesfully!");
            DownloadProgressBar.Value = 0;
            label1.Visible = true;
            label2.Visible = true;
            dateTimeStart.Visible = true;
            dateTimeEnd.Visible = true;
            DownloadProgressBar.Visible = false;

        }

        private void timerDownload_Tick(object sender, EventArgs e)
        {
            DownloadProgressBar.Maximum = 100;
            DownloadProgressBar.Minimum = 0;

            int iPos = 0;

            //Get downloading process

            iPos = CHCNetSDK.NET_DVR_GetDownloadPos(m_lDownHandle);

            if ((iPos > DownloadProgressBar.Minimum) && (iPos < DownloadProgressBar.Maximum))
            {
                DownloadProgressBar.Value = iPos;
            }

            if (iPos == 100)  //Finish downloading
            {
                DownloadProgressBar.Value = iPos;
                if (!CHCNetSDK.NET_DVR_StopGetFile(m_lDownHandle))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_StopGetFile failed, error code= " + iLastErr; //Download controlling failed,print error code
                    MessageBox.Show(str);
                    return;
                }
                m_lDownHandle = -1;
                timerDownload.Stop();
                btnDownload.Text = "Download";
                label1.Visible = true;
                label2.Visible = true;
                dateTimeStart.Visible = true;
                dateTimeEnd.Visible = true;
                DownloadProgressBar.Visible = false;
                MessageBox.Show("Download Finished");
            }

            if (iPos == 200) //Network abnormal,download failed
            {
                MessageBox.Show("The downloading is abnormal for the abnormal network!");
                timerDownload.Stop();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.A: 
                    if(m_nSpeed < 8)
                    {
                        PlayFast();
                        m_nSpeed *= 2;
                    }
                    else
                    {
                        m_nSpeed = 1;
                        Normal();
                    }
                    this.Text = m_sTitle + string.Format(" @{0}x", m_nSpeed);
                    break;
                case Keys.Space:
                    if(!m_bPause)
                    {
                        Pause();
                        m_bPause = true;
                    }
                    else
                    {
                        m_bPause = false;
                        Resume();
                        this.Text = m_sTitle + string.Format(" @{0}x", m_nSpeed);
                    }
                    break;
                case Keys.X:
                case Keys.Down:
                    SingleFrame();
                    break;
                case Keys.Up:
                    //CHCNetSDK.NET_DVR_PlayBackControl(m_lPlayback, CHCNetSDK.NET_DVR_PLAYFRAME, 0, ref pOut);
                    break;
                default:
                    return base.ProcessCmdKey(ref msg, keyData);
            }
            return true;
        }

        private void Normal()
        {
            NETClient.PlayBackControl(m_PlayBackID, PlayBackType.Normal);
        }

        private void Pause()
        {
            NETClient.PlayBackControl(m_PlayBackID, PlayBackType.Pause);
        }

        private void Resume()
        {
            NETClient.PlayBackControl(m_PlayBackID, PlayBackType.Play);
        }
        private void SingleFrame()
        {
            NETClient.PlayBackControl(m_PlayBackID, PlayBackType.Step);
        }

        private void PlayFast()
        {
            NETClient.PlayBackControl(m_PlayBackID, PlayBackType.Fast);
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            NETClient.Cleanup();
        }
    }
}
