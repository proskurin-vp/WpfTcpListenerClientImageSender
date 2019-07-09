using System;
using System.Collections.Generic;
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

namespace Sender
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CancellationTokenSource _cancellationTokenSource;
        CancellationToken _cancellationToken;       

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButStart_Click(object sender, RoutedEventArgs e)
        {             
            try
            {
                if (ButStart.Content.Equals("Start"))
                {
                    ButStart.Content = "Stop";                   
                    _cancellationTokenSource = new CancellationTokenSource();
                    _cancellationToken = _cancellationTokenSource.Token;

                    Server server = new Server(this);
                    int port = Convert.ToInt32(TbPort.Text);
                    Task.Factory.StartNew(()=>server.Start(_cancellationToken, port), _cancellationToken);
                }
                else if(ButStart.Content.Equals("Stop"))
                {
                    ButStart.Content = "Start";                   
                    _cancellationTokenSource.Cancel();                    
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
