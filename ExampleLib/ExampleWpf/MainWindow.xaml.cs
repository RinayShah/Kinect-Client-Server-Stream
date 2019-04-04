using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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

using ExampleLib;

namespace ExampleWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Client _client;
        private readonly CancellationTokenSource _cTokenSource;
        public MainWindow()
        {
            InitializeComponent();
            _client = new Client();
            
            _cTokenSource = new CancellationTokenSource();
            _client.NetData += _client_NetData;
            Trace.Listeners.Add(new TraceListnerWPF(TextBoxDebug));
        }

        private void _client_NetData(object sender, Client.NetDataEventArgs e)
        {
            TextBoxOutput.Text = e.Message;
        }

    

        private void TextBoxInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrEmpty(TextBoxInput.Text))
                {
                    _client.Write(TextBoxInput.Text);
                    TextBoxInput.Text = string.Empty;
                }
            }
        }

        private void LoadedWindow(object sender, RoutedEventArgs e)
        {

            _client.Start(_cTokenSource.Token);
        }
    }
}