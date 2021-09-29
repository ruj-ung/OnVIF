using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VLCDemo
{
    public partial class Form1 : Form
    {
        UriBuilder deviceURI;
        Media.Media2Client media;
        Media.MediaProfile[] profiles; 
        
        public Form1()
        {
            InitializeComponent();
            deviceURI = new UriBuilder("http:/onvif/device_service");

            string[] addr = "10.101.0.67".Split(':');
            deviceURI.Host = addr[0];
            if (addr.Length == 2)
                deviceURI.Port = Convert.ToInt16(addr[1]);

            System.ServiceModel.Channels.Binding binding;
            HttpTransportBindingElement httpTransport = new HttpTransportBindingElement();
            httpTransport.AuthenticationScheme = System.Net.AuthenticationSchemes.Digest;
            binding = new CustomBinding(new TextMessageEncodingBindingElement(MessageVersion.Soap12WSAddressing10, Encoding.UTF8), httpTransport);

            Device.DeviceClient device = new Device.DeviceClient(binding, new EndpointAddress(deviceURI.ToString()));

            Device.Service[] services = device.GetServices(false);

            Device.Service xmedia = services.FirstOrDefault(s => s.Namespace == "http://www.onvif.org/ver20/media/wsdl");
            if (xmedia != null)
            {
                media = new Media.Media2Client(binding, new EndpointAddress(deviceURI.ToString()));
                media.ClientCredentials.HttpDigest.ClientCredential.UserName = "ruj";
                media.ClientCredentials.HttpDigest.ClientCredential.Password = "powwow123";
                media.ClientCredentials.HttpDigest.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;

                profiles = media.GetProfiles(null, null);

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void vlcControl1_VlcLibDirectoryNeeded(object sender, Vlc.DotNet.Forms.VlcLibDirectoryNeededEventArgs e)
        {
            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            // Default installation path of VideoLAN.LibVLC.Windows
            e.VlcLibDirectory = new DirectoryInfo(Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (profiles != null)
            {

                UriBuilder uri = new UriBuilder(media.GetStreamUri("RtspOverHttp", profiles[0].token));

                uri.Host = deviceURI.Host;
                uri.Port = deviceURI.Port;
                uri.Scheme = "rtsp";

                string[] options = { ":rtsp-http", "" +
                    ":rtsp-http-port=" + uri.Port, 
                    ":rtsp-user=" + media.ClientCredentials.HttpDigest.ClientCredential.UserName, 
                    ":rtsp-pwd=" + media.ClientCredentials.HttpDigest.ClientCredential.Password, };


                vlcControl1.Play(uri.Uri, options);
                //vlcControl1.Play(uri.Uri);

            }
            //UriBuilder uri = new UriBuilder("rtsp://admin:digitalfusion123@10.101.0.69:554/Streaming/Channels/101");//("D:/football.mp4");
            //vlcControl1.Play(uri.Uri);
        }
    }
}
