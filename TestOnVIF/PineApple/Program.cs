//#define LOGIN
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using Rujchai;
using System.Data;

namespace CamView
{
    static class Program
    {
        static public uint PointLimit = 250;
        static string user1 = "BSS-0123456789";
        //static string user2 = "FV902530E6XNVCJA1"; // เตย
        //static string user2 = "R911YNPL"; // Pithitha
        static string user2 = "L1HF63901PA"; // ThinkPad
        //static string user2 = "NBG6Z110026018AAAB3400"; //Bank
        //static string user2 = "NBQ6Z110020497447E3400"; // Up
        //static string user2 = "To be filled by O.E.M."; //Aj.Pao
        //static string user2 = "RESH09H0089";
        //static string user2 = "K835NRCV00L8K6MB"; // Jet
        //static string user2 = "A869430100AX113A"; // ผู้การนนท์ 
        public static string version = "1.2";
        /// <summary>
        /// The main entry point for the application. 
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            string mbinfo = "";
            ManagementClass mc = new ManagementClass("Win32_BaseBoard");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                mbinfo = mo.Properties["SerialNumber"].Value.ToString();
                break;
            }
           Process proc = Process.GetCurrentProcess();
            //if (mbinfo != " " && mbinfo != "BSS-0123456789") //Ray
            //if (mbinfo != "J816479322" && mbinfo != "BSS-0123456789") //Ning
            //if (mbinfo != "NBG6Z110026018AAAB3400" && mbinfo != "BSS-0123456789")
            //if (mbinfo != "K835NRCV00L8K6MB" && mbinfo != "BSS-0123456789") // Jet
            //if (mbinfo != "KB44NBCV002A0BMB" && mbinfo != "BSS-0123456789") // Make
            //if (mbinfo != "NBG6Z110026018AAAB3400" && mbinfo != "BSS-0123456789") // Bank
            //if (mbinfo != "BSN12345678901234567" && mbinfo != "BSS-0123456789") // Bob
            //if (mbinfo != "NBQ6Z110020497447E3400" && mbinfo != "BSS-0123456789") // Up
            //if (mbinfo != "A869430100AX113A" && mbinfo != "BSS-0123456789") // ผู้การนนท์
            //if (mbinfo != "R911YNPL" && mbinfo != "BSS-0123456789") // Pithitha
            //if (mbinfo != "FV902530E6XNVCJA1" && mbinfo != "BSS-0123456789") // เตย
            if (mbinfo != user1 && mbinfo != user2)
            {
                MessageBox.Show("Unlock code is "+ "\'" + mbinfo + "\'");
                proc.Kill();
            }
            List<Process> procs = Process.GetProcesses().Where(p =>p.ProcessName == proc.ProcessName).ToList();
            IntPtr hWnd = procs.First().MainWindowHandle;
            if (procs.Count > 1)
            {
                MessageBox.Show("PineApple Eyes is running...", "โปรดทราบ", 
                    MessageBoxButtons.OK, MessageBoxIcon.Stop);

                proc.Kill();
            }
            var oledb12Installed = new System.Data.OleDb.OleDbEnumerator()
                .GetElements().AsEnumerable()
                .Any(x => x.Field<string>("SOURCES_NAME") ==
                    "Microsoft.ACE.OLEDB.12.0");
            if (!oledb12Installed)
                MessageBox.Show("Please install Microsoft.ACE.OLEDB.12.0. [64-Bit]");

#if (LOGIN)
            // Log in 
            Credit cred = new Credit();
            cred.Message = "This will allow user to use PineApple App";
            bool bCancel;
            do
            {
                bool logged = cred.login(out bCancel);
                if (logged && !bCancel) break;
                if (bCancel) return;
                if (!logged && !bCancel)
                {
                    cred.Message = "Wrong user name or password, Please try again";
                }
            } while (true);

#endif
            Form1 frmMain = new Form1();

            Application.Run(frmMain);
        }

        public static bool CheckLicense()
        {
            bool result = false;
            string mbinfo = "";
            ManagementClass mc = new ManagementClass("Win32_BaseBoard");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                mbinfo = mo.Properties["SerialNumber"].Value.ToString();
                break;
            }

            Process proc = Process.GetCurrentProcess();
            //if (mbinfo != " " && mbinfo != "BSS-0123456789") //Ray
            //if (mbinfo != "J816479322" && mbinfo != "BSS-0123456789") //Ning
            //if (mbinfo != "NBG6Z110026018AAAB3400" && mbinfo != "BSS-0123456789")
            //if (mbinfo != "K835NRCV00L8K6MB" && mbinfo != "BSS-0123456789") // Jet
            //if (mbinfo != "KB44NBCV002A0BMB" && mbinfo != "BSS-0123456789") // Make
            //if (mbinfo != "NBG6Z110026018AAAB3400" && mbinfo != "BSS-0123456789") // Bank
            //if (mbinfo != "BSN12345678901234567" && mbinfo != "BSS-0123456789") // Bob
            //if (mbinfo != "NBQ6Z110020497447E3400" && mbinfo != "BSS-0123456789") // Up
            //if (mbinfo != "A869430100AX113A" && mbinfo != "BSS-0123456789") // ผู้การนนท์
            //if (mbinfo != "R911YNPL" && mbinfo != "BSS-0123456789") // Pithitha
            //if (mbinfo != "FV902530E6XNVCJA1" && mbinfo != "BSS-0123456789") // เตย
            if (mbinfo != user1 && mbinfo != user2 ) 
            {
                MessageBox.Show("Unlock code is " + "\'" + mbinfo + "\'");
                proc.Kill();
            }

            return result;
        }
        //static void Main()
        //{
        //    Application.EnableVisualStyles();
        //    Application.SetCompatibleTextRenderingDefault(false);

        //    LoginForm fLogin = new LoginForm();

        //    if (fLogin.ShowDialog() == DialogResult.OK)
        //    {
        //        Application.Run(new Form1(fLogin.UserName));

        //    }
        //    else
        //    {
        //        Application.Exit();
        //    }

        //    //Application.Run(new Form1());
        //}
    }
}
