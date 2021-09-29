using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfTestOnVIF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        UriBuilder deviceURI;
        Media.Media2Client media;
        Media.MediaProfile[] profiles;
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            deviceURI = new UriBuilder("http:/onvif/device_service");

            string[] addr = address.Text.Split(':');
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
                media.ClientCredentials.HttpDigest.ClientCredential.UserName = login.Text;
                media.ClientCredentials.HttpDigest.ClientCredential.Password = password.Text;
                media.ClientCredentials.HttpDigest.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;

                profiles = media.GetProfiles(null, null);
                if (profiles != null)
                    foreach (var p in profiles)
                        listBox.Items.Add(p);
            }

        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UriBuilder uri = new UriBuilder(media.GetStreamUri("RtspOverHttp", profiles[listBox.SelectedIndex].token));

            uri.Host = deviceURI.Host;
            uri.Port = deviceURI.Port;
            uri.Scheme = "rtsp";
            infoBox.Content = uri.Path;

            string[] options = { ":rtsp-http", ":rtsp-http-port=" + uri.Port, ":rtsp-user=" + login.Text, ":rtsp-pwd=" + password.Text, };
            
        }
    }
}
