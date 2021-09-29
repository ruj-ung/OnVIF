using PreviewDemo;
using NetSDKCS;

namespace CamView
{
    partial class CamFormDH
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (m_lRealHandle >= 0)
            {
                NETClient.StopRealPlay(m_RealPlayID);
            }
            if (m_LoginID != System.IntPtr.Zero)
            {
                NETClient.Logout(m_LoginID);
            }
            NETClient.Cleanup();
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // CamFormDH
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(582, 353);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(265, 180);
            this.Name = "CamFormDH";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "CamFormDH";
            this.ResizeEnd += new System.EventHandler(this.CamFormDH_ResizeEnd);
            this.ResumeLayout(false);

        }

        #endregion
    }
}