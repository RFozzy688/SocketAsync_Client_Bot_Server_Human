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
using System.Threading;

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
        Random _random;
        List<string> _requests;
        public MainWindow()
        {
            InitializeComponent();

            _random = new Random();
            _requests = new List<string>();

            _requests.Add("Request 1");
            _requests.Add("Request 2");
            _requests.Add("Request 3");
            _requests.Add("<Bye>");
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

                    RandomRequest();
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
        private void RandomRequest()
        {

            Dispatcher.Invoke(async () =>
            {
                int index = 0;

                while (true)
                {
                    Thread.Sleep(500);

                    switch (_random.Next(1, 11))
                    {
                        case 1:
                        case 2:
                        case 3:
                            index = 0;
                            break;
                        case 4:
                        case 5:
                        case 6:
                            index = 1;
                            break;
                        case 7:
                        case 8:
                        case 9:
                            index = 2;
                            break;
                        case 10:
                            index = 3;
                            break;
                    }

                    string message = "Client: " + _requests[index];
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

                        break;
                    }

                    ChatBox.Text += response + "\r\n";
                }
            });
        }
    }
}
