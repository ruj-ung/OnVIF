using PineApple.Device;
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

namespace CamView
{
    public partial class CamFormOV : Form
    {

        public string m_IPAddress = "nanobanphai.dyndns.org";
        public UInt16 m_nPort = 8000;
        public string m_User = "admin";
        public string m_Password = "abc12345";
        public Int16 m_nChannel = 1;

        UriBuilder deviceURI;
        PineApple.Media.Media2Client media;
        PineApple.Media.MediaProfile[] profiles;

        public CamFormOV()
        {
            InitializeComponent();
            deviceURI = new UriBuilder("http:/onvif/device_service");

            string[] addr = "10.101.0.85".Split(':');
            deviceURI.Host = addr[0];
            if (addr.Length == 2)
                deviceURI.Port = Convert.ToInt16(addr[1]);

            System.ServiceModel.Channels.Binding binding;
            HttpTransportBindingElement httpTransport = new HttpTransportBindingElement();
            httpTransport.AuthenticationScheme = System.Net.AuthenticationSchemes.Digest;
            binding = new CustomBinding(new TextMessageEncodingBindingElement(MessageVersion.Soap12WSAddressing10, Encoding.UTF8), httpTransport);

            PineApple.Device.DeviceClient device = new PineApple.Device.DeviceClient(binding, new EndpointAddress(deviceURI.ToString()));

            PineApple.Device.Service[] services = device.GetServices(false);

            PineApple.Device.Service xmedia = services.FirstOrDefault(s => s.Namespace == "http://www.onvif.org/ver20/media/wsdl");
            if (xmedia != null)
            {
                media = new PineApple.Media.Media2Client(binding, new EndpointAddress(deviceURI.ToString()));
                media.ClientCredentials.HttpDigest.ClientCredential.UserName = "ruj";
                media.ClientCredentials.HttpDigest.ClientCredential.Password = "powwow123";
                media.ClientCredentials.HttpDigest.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;

                profiles = media.GetProfiles(null, null);

            }
        }

        private void vlcControl1_VlcLibDirectoryNeeded(object sender, Vlc.DotNet.Forms.VlcLibDirectoryNeededEventArgs e)
        {
            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            // Default installation path of VideoLAN.LibVLC.Windows
            e.VlcLibDirectory = new DirectoryInfo(Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));
        }

        private void CamFormOV_Load(object sender, EventArgs e)
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
            }
        }
    }
}
