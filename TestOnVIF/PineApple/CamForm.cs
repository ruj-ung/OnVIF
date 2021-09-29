using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using PreviewDemo;
using System.IO;

namespace CamView
{
    public partial class CamForm : Form
    {
        private static CamForm _camForm = null;

        public string m_IPAddress = "nanobanphai.dyndns.org";
        public UInt16 m_nPort = 8000;
        public string m_User = "admin";
        public string m_Password = "abc12345";
        public Int16 m_nChannel = 1;

        private uint iLastErr = 0;
        private Int32 m_lUserID = -1;
        private bool m_bInitSDK = false;
        private bool m_bRecord = false;
        private bool m_bTalk = false;
        private Int32 m_lRealHandle = -1;
        private int lVoiceComHandle = -1;
        private string str;

        CHCNetSDK.REALDATACALLBACK RealData = null;
        public CHCNetSDK.NET_DVR_PTZPOS m_struPtzCfg;


        IntPtr m_hWnd;
        public CamForm()
        {
            InitializeComponent();

            //m_bInitSDK = CHCNetSDK.NET_DVR_Init();
            //if (m_bInitSDK == false)
            //{
            //    MessageBox.Show("NET_DVR_Init error!");
            //    return;
            //}
            //else
            //{
            //    //To save the SDK log
            //    CHCNetSDK.NET_DVR_SetLogToFile(3, "C:\\SdkLog\\", true);
            //}

            //Login();
            //Preview();
        }

        public static CamForm Instance
        {
            get
            {
                if (_camForm == null)
                {
                    _camForm = new CamForm();
                }
                return _camForm;

            }
        }
        private void Preview()
        {
            if (m_lUserID < 0)
            {
                MessageBox.Show("Please login the device firstly");
                return;
            }

            if (m_lRealHandle < 0)
            {
                CHCNetSDK.NET_DVR_PREVIEWINFO lpPreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO();
                lpPreviewInfo.hPlayWnd = this.Handle;
                lpPreviewInfo.lChannel = m_nChannel;
                lpPreviewInfo.dwStreamType = 0;
                lpPreviewInfo.dwLinkMode = 0;
                lpPreviewInfo.bBlocked = true; 
                lpPreviewInfo.dwDisplayBufNum = 1; 
                lpPreviewInfo.byProtoType = 0;
                lpPreviewInfo.byPreviewMode = 0;


                //if (textBoxID.Text != "")
                //{
                //    lpPreviewInfo.lChannel = -1;
                //    byte[] byStreamID = System.Text.Encoding.Default.GetBytes(textBoxID.Text);
                //    lpPreviewInfo.byStreamID = new byte[32];
                //    byStreamID.CopyTo(lpPreviewInfo.byStreamID, 0);
                //}


                if (RealData == null)
                {
                    RealData = new CHCNetSDK.REALDATACALLBACK(RealDataCallBack);
                }

                IntPtr pUser = new IntPtr();//ÓÃ»§Êý¾Ý

                // Start live view 
                m_lRealHandle = CHCNetSDK.NET_DVR_RealPlay_V40(m_lUserID, ref lpPreviewInfo, null/*RealData*/, pUser);
                if (m_lRealHandle < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_RealPlay_V40 failed, error code= " + iLastErr; 
                    MessageBox.Show(str);
                    return;
                }
            }
            else
            {
                //Í£Ö¹Ô¤ÀÀ Stop live view 
                if (!CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_StopRealPlay failed, error code= " + iLastErr;
                    MessageBox.Show(str);
                    return;
                }
                m_lRealHandle = -1;
                //btnPreview.Text = "Live View";

            }
            return;
        }

        private bool Login()
        {
            if (m_lUserID < 0)
            {
                string DVRIPAddress = m_IPAddress;
                UInt16 DVRPortNumber = m_nPort;
                string DVRUserName = m_User;
                string DVRPassword = m_Password;

                CHCNetSDK.NET_DVR_DEVICEINFO_V30 DeviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V30();

                //µÇÂ¼Éè±¸ Login the device
                m_lUserID = CHCNetSDK.NET_DVR_Login_V30(DVRIPAddress, DVRPortNumber, DVRUserName, DVRPassword, ref DeviceInfo);
                if (m_lUserID < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "Login_V30 failed, error code= " + iLastErr; 
                    MessageBox.Show(str);
                    return false;
                }
            }
            else
            {
                //Logout the device
                if (m_lRealHandle >= 0)
                {
                    MessageBox.Show("Please stop live view firstly");
                    return true;
                }

                if (!CHCNetSDK.NET_DVR_Logout(m_lUserID))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_Logout failed, error code= " + iLastErr;
                    MessageBox.Show(str);
                    return  false;
                }
                m_lUserID = -1;
            }
            return true;
        }
        public void RealDataCallBack(Int32 lRealHandle, UInt32 dwDataType, IntPtr pBuffer, UInt32 dwBufSize, IntPtr pUser)
        {
            if (dwBufSize > 0)
            {
                byte[] sData = new byte[dwBufSize];
                Marshal.Copy(pBuffer, sData, 0, (Int32)dwBufSize);

                string str = "test.ps";
                FileStream fs = new FileStream(str, FileMode.Create);
                int iLen = (int)dwBufSize;
                fs.Write(sData, 0, iLen);
                fs.Close();
            }
        }

        private void CamForm_Load(object sender, EventArgs e)
        {
            if (Login())
            {
                Preview();
            }
            else
                this.BeginInvoke(new MethodInvoker(this.Close));
        }


        private void CamForm_ResizeEnd(object sender, EventArgs e)
        {
            var forms = Application.OpenForms;
            foreach (Form form in forms)
            {
                if (form.GetType() == typeof(Form1))
                {
                    form.Focus();
                }
            }
        }
    }
}
