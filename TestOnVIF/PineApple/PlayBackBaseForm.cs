using PreviewDemo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;
using CamView;

namespace PlayBackDemo
{
    public partial class PlayBackForm : Form
    {
        private bool m_bInitSDK = false;
        private uint iLastErr = 0;
        private Int32 m_lUserID = -1;
        private Int32 m_lFindHandle = -1;
        private Int32 m_lPlayHandle = -1;
        private Int32 m_lDownHandle = -1;
        private string str;
        private string str1;
        private string str2;
        private string str3;
        private string m_sTitle;
        private string sPlayBackFileName = null;
        private Int32 i = 0;
        private Int32 m_lTree = 0;
        private uint m_nSpeed = 1;

        private bool m_bPause = false;
        private bool m_bReverse = false;
        private bool m_bSound = false;

        public string m_IPAddress = "redcam.dfdm.in.th";
        public Int16 m_nPort = 8000;
        public string m_User = "admin";
        public string m_Password = "digitalfusion123";
        public Int16 m_nChannel = 1;

        private long iSelIndex = 0;
        private uint dwAChanTotalNum = 0;
        private uint dwDChanTotalNum = 0;
        public CHCNetSDK.NET_DVR_DEVICEINFO_V30 DeviceInfo;
        public CHCNetSDK.NET_DVR_IPPARACFG_V40 m_struIpParaCfgV40;
        public CHCNetSDK.NET_DVR_GET_STREAM_UNION m_unionGetStream;
        public CHCNetSDK.NET_DVR_IPCHANINFO m_struChanInfo;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 96, ArraySubType = UnmanagedType.U4)]
        private int[] iChannelNum;
        
        public PlayBackForm()
        {
            InitializeComponent();
            this.Size = new Size(520, 380);

            m_bInitSDK = CHCNetSDK.NET_DVR_Init();
            if (m_bInitSDK == false)
            {
                MessageBox.Show("NET_DVR_Init error!");
                return;
            }
            else
            {
                //Save log of SDK
                CHCNetSDK.NET_DVR_SetLogToFile(3, "C:\\SdkLog\\", true);
                iChannelNum = new int[96];
            }
            //Login();
        }

        private void Login()
        {
            if (m_lUserID < 0)
            {
                string DVRIPAddress = m_IPAddress; //IP or domain of device
                Int16 DVRPortNumber = m_nPort;//Service port of device
                string DVRUserName = m_User;//Login name of deivce
                string DVRPassword = m_Password;//Login password of device

                //    DeviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V30();

                //Login the device
                m_lUserID = CHCNetSDK.NET_DVR_Login_V30(DVRIPAddress, DVRPortNumber, DVRUserName, DVRPassword, ref DeviceInfo);
                if (m_lUserID < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str1 = "NET_DVR_Login_V30 failed, error code= " + iLastErr; //Login failed,print error code
                    MessageBox.Show(str1);
                    return;
                }
                else
                {
                    //Login successsfully
                    //MessageBox.Show("Login Success!");
                }

            }
            else
            {
                if (m_lPlayHandle >= 0)
                {
                    MessageBox.Show("Please stop playback firstly"); //Please stop playback before logout
                    return;
                }

                //Logout the device
                if (!CHCNetSDK.NET_DVR_Logout(m_lUserID))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str1 = "NET_DVR_Logout failed, error code= " + iLastErr;
                    MessageBox.Show(str1);
                    return;
                }
                m_lUserID = -1;
            }
            return;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(button2.Text == "Play")
            {
                button2.Text = "Stop";
                if (m_lPlayHandle >= 0)
                {
                    //Please stop playback if playbacking now.
                    if (!CHCNetSDK.NET_DVR_StopPlayBack(m_lPlayHandle))
                    {
                        iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                        str = "NET_DVR_StopPlayBack failed, error code= " + iLastErr;
                        MessageBox.Show(str);
                        return;
                    }


                    m_lPlayHandle = -1;

                }

                CHCNetSDK.NET_DVR_VOD_PARA struVodPara = new CHCNetSDK.NET_DVR_VOD_PARA();
                struVodPara.dwSize = (uint)Marshal.SizeOf(struVodPara);
                struVodPara.struIDInfo.dwChannel = (uint)m_nChannel; //Channel number  
                struVodPara.hWnd = VideoPlayWnd.Handle;//handle of playback

                //Set the starting time to search video files
                struVodPara.struBeginTime.dwYear = (uint)dateTimeStart.Value.Year;
                struVodPara.struBeginTime.dwMonth = (uint)dateTimeStart.Value.Month;
                struVodPara.struBeginTime.dwDay = (uint)dateTimeStart.Value.Day;
                struVodPara.struBeginTime.dwHour = (uint)dateTimeStart.Value.Hour;
                struVodPara.struBeginTime.dwMinute = (uint)dateTimeStart.Value.Minute;
                struVodPara.struBeginTime.dwSecond = (uint)dateTimeStart.Value.Second;

                //Set the stopping time to search video files
                struVodPara.struEndTime.dwYear = (uint)dateTimeEnd.Value.Year;
                struVodPara.struEndTime.dwMonth = (uint)dateTimeEnd.Value.Month;
                struVodPara.struEndTime.dwDay = (uint)dateTimeEnd.Value.Day;
                struVodPara.struEndTime.dwHour = (uint)dateTimeEnd.Value.Hour;
                struVodPara.struEndTime.dwMinute = (uint)dateTimeEnd.Value.Minute;
                struVodPara.struEndTime.dwSecond = (uint)dateTimeEnd.Value.Second;

                //Playback by time
                m_lPlayHandle = CHCNetSDK.NET_DVR_PlayBackByTime_V40(m_lUserID, ref struVodPara);
                if (m_lPlayHandle < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_PlayBackByTime_V40 failed, error code= " + iLastErr;
                    MessageBox.Show(str);
                    return;
                }

                uint iOutValue = 0;
                if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lPlayHandle, CHCNetSDK.NET_DVR_PLAYSTART, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_PLAYSTART failed, error code= " + iLastErr; //Playback controlling failed,print error code.
                    MessageBox.Show(str);
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

        private void PlayBackForm_Load(object sender, EventArgs e)
        {
            m_sTitle = this.Text;
            this.Text = m_sTitle + string.Format(" @{0}x", m_nSpeed);
            Login();
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            if(btnDownload.Text == "Download")
            {
                if (m_lDownHandle >= 0)
                {
                    MessageBox.Show("Downloading, please stop firstly!");//Please stop downloading
                    return;
                }

                CHCNetSDK.NET_DVR_PLAYCOND struDownPara = new CHCNetSDK.NET_DVR_PLAYCOND();
                struDownPara.dwChannel = (uint)m_nChannel; //Channel number  

                //Set the starting time
                struDownPara.struStartTime.dwYear = (uint)dateTimeStart.Value.Year;
                struDownPara.struStartTime.dwMonth = (uint)dateTimeStart.Value.Month;
                struDownPara.struStartTime.dwDay = (uint)dateTimeStart.Value.Day;
                struDownPara.struStartTime.dwHour = (uint)dateTimeStart.Value.Hour;
                struDownPara.struStartTime.dwMinute = (uint)dateTimeStart.Value.Minute;
                struDownPara.struStartTime.dwSecond = (uint)dateTimeStart.Value.Second;

                //Set the stopping time
                struDownPara.struStopTime.dwYear = (uint)dateTimeEnd.Value.Year;
                struDownPara.struStopTime.dwMonth = (uint)dateTimeEnd.Value.Month;
                struDownPara.struStopTime.dwDay = (uint)dateTimeEnd.Value.Day;
                struDownPara.struStopTime.dwHour = (uint)dateTimeEnd.Value.Hour;
                struDownPara.struStopTime.dwMinute = (uint)dateTimeEnd.Value.Minute;
                struDownPara.struStopTime.dwSecond = (uint)dateTimeEnd.Value.Second;

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = "data";
                saveFileDialog.Filter = "|*.mp4";
                string path = AppDomain.CurrentDomain.BaseDirectory + "savedata";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                saveFileDialog.InitialDirectory = path;
                var res = saveFileDialog.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.OK)
                {
                        string sVideoFileName;  //the path and file name to save      
                    sVideoFileName = saveFileDialog.FileName;
                    //sVideoFileName = string.Format("D:\\Download{0}_{1}-{2}.mp4", struDownPara.dwChannel, dateTimeStart.Value.ToString("yyyyMMddHHmmss"), dateTimeEnd.Value.ToString("yyyyMMddHHmmss"));

                    //Download by time
                    m_lDownHandle = CHCNetSDK.NET_DVR_GetFileByTime_V40(m_lUserID, sVideoFileName, ref struDownPara);
                    if (m_lDownHandle < 0)
                    {
                        iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                        str = "NET_DVR_GetFileByTime_V40 failed, error code= " + iLastErr;
                        MessageBox.Show(str);
                        return;
                    }

                    uint iOutValue = 0;
                    if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lDownHandle, CHCNetSDK.NET_DVR_PLAYSTART, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
                    {
                        iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                        str = "NET_DVR_PLAYSTART failed, error code= " + iLastErr; //Download controlling failed,print error code
                        MessageBox.Show(str);
                        return;
                    }

                    timerDownload.Interval = 1000;
                    timerDownload.Enabled = true;
                    label1.Visible = false;
                    label2.Visible = false;
                    dateTimeStart.Visible = false;
                    dateTimeEnd.Visible = false;
                    DownloadProgressBar.Visible = true;
                    btnDownload.Text = "Stop";
                    return;
                }
                else
                {
                    btnDownload.Text = "Download";
                    saveFileDialog.Dispose();
                    return;
                }


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
            if (m_lDownHandle < 0)
            {
                return;
            }

            if (!CHCNetSDK.NET_DVR_StopGetFile(m_lDownHandle))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_StopGetFile failed, error code= " + iLastErr; //Download controlling failed,print error code
                MessageBox.Show(str);
                return;
            }

            timerDownload.Stop();

            MessageBox.Show("The downloading has been stopped succesfully!");
            m_lDownHandle = -1;
            DownloadProgressBar.Value = 0;
            label1.Visible = true;
            label2.Visible = true;
            dateTimeStart.Visible = true;
            dateTimeEnd.Visible = true;
            DownloadProgressBar.Visible = false;

        }

        public void StopPlayback()
        {
            if (m_lPlayHandle < 0)
            {
                return;
            }

            //Stop playback
            if (!CHCNetSDK.NET_DVR_StopPlayBack(m_lPlayHandle))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_StopPlayBack failed, error code= " + iLastErr;
                MessageBox.Show(str);
                return;
            }

            m_lPlayHandle = -1;
            VideoPlayWnd.Invalidate();//Refresh window
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
                //MessageBox.Show("Download Finished");
                InforForm info = new InforForm();
                info.StartPosition = FormStartPosition.CenterParent;
                info.TopMost = true;
                info.ShowDialog();
                
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
                        Normal();
                    }
                    this.Text = m_sTitle + string.Format(" @{0}x", m_nSpeed);
                    break;
                case Keys.Space:
                    if (!m_bPause)
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

        private void Pause()
        {
            uint iOutValue = 0;

            if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lPlayHandle, CHCNetSDK.NET_DVR_PLAYPAUSE, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_PLAYNORMAL failed, error code= " + iLastErr; //Playback controlling failed,print error code
                MessageBox.Show(str);
            }
        }

        private void Normal()
        {
            m_nSpeed = 1;
            uint iOutValue = 0;

            if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lPlayHandle, CHCNetSDK.NET_DVR_PLAYNORMAL, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_PLAYNORMAL failed, error code= " + iLastErr; //Playback controlling failed,print error code
                MessageBox.Show(str);
            }
        }

        private void Resume()
        {
            m_nSpeed = 1;
            uint iOutValue = 0;

            if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lPlayHandle, CHCNetSDK.NET_DVR_PLAYNORMAL, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_PLAYNORMAL failed, error code= " + iLastErr; //Playback controlling failed,print error code
                MessageBox.Show(str);
                return;
            }
        }
        private void SingleFrame()
        {
            uint iOutValue = 0;

            if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lPlayHandle, CHCNetSDK.NET_DVR_PLAYFRAME, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_PLAYFRAME failed, error code= " + iLastErr; //Playback controlling failed,print error code
                MessageBox.Show(str);
                return;
            }
        }

        private void PlayFast()
        {

            uint iOutValue = 0;

            if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(m_lPlayHandle, CHCNetSDK.NET_DVR_PLAYFAST, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_PLAYFAST failed, error code= " + iLastErr; //Playback controlling failed,print error code
                MessageBox.Show(str);
                return;
            }
        }
    }
}
