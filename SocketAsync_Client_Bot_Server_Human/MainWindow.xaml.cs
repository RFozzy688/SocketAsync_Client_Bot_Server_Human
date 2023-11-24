using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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

namespace SocketAsync_Client_Bot_Server_Human
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Regex _ipAddress = new Regex("[^0-9.]");
        private static readonly Regex _port = new Regex("[^0-9]");
        IPEndPoint _ipEndPoint;
        Socket _socket;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }
        private static bool IsTextAllowed(string text)
        {
            return !_ipAddress.IsMatch(text);
        }
        private void TextBox_PreviewTextInput_1(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed_1(e.Text);
        }
        private static bool IsTextAllowed_1(string text)
        {
            return !_port.IsMatch(text);
        }
        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            BtnConnect.IsEnabled = false;

            try
            {
                IPAddress ipAddress = IPAddress.Parse(IPAddressText.Text);
                int port = Int32.Parse(PortText.Text);

                _ipEndPoint = new IPEndPoint(ipAddress, port);

                try
                {
                    _socket = new Socket(_ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    await _socket.ConnectAsync(_ipEndPoint);

                    ChatBox.Text = "Client connected to " +
                        _socket.RemoteEndPoint?.ToString() + " " + DateTime.Now.ToString() + "\r\n";
                }
                catch (SocketException ex)
                {
                    MessageBox.Show(ex.Message);
                    BtnConnect.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                BtnConnect.IsEnabled = true;
            }
        }
        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            BtnSend.IsEnabled = false;

            Dispatcher.Invoke(async () =>
            {
                string message = "Client: " + TextInput.Text;
                byte[]? msgByte = Encoding.Unicode.GetBytes(message);
                await _socket.SendAsync(msgByte, SocketFlags.None);

                ChatBox.Text += message + "\r\n";

                byte[] buffer = new byte[1024];
                int received = await _socket.ReceiveAsync(buffer, SocketFlags.None);
                string response = Encoding.Unicode.GetString(buffer, 0, received);

                if (response.IndexOf("<Bye>") > -1)
                {
                    ChatBox.Text += "Disconnected to server " + DateTime.Now.ToString();

                    _socket.Shutdown(SocketShutdown.Both);
                    _socket.Close();

                    return;
                }

                ChatBox.Text += response + "\r\n";

                BtnSend.IsEnabled = true;
            });
        }
    }
}
