using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Sender
{
    public class Server
    {
        public TcpListener Listener { get; private set; }
        public static MainWindow MainWindow { get; private set; }       

        public Server(MainWindow mainWindow)
        {
            MainWindow = mainWindow;
        }

        public void Start(CancellationToken cancellationToken, int port)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            while (true)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                Thread.Sleep(50);
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            Listener.Stop();
                        }
                    });

                    Listener = new TcpListener(IPAddress.Any, port);
                    Listener.Start();

                    MainWindow.Dispatcher.Invoke(new Action(() =>
                    {
                        AddTextToLog(string.Format("Sender started on {0} at {1}...\n",
                        Listener.LocalEndpoint.ToString(), DateTime.Now.ToLongTimeString()), Brushes.Green);
                        MainWindow.ImageStatus.Source = new BitmapImage(new Uri("/Images/online-icon.png", UriKind.RelativeOrAbsolute));
                        MainWindow.TbPort.IsEnabled = false;
                    }));

                    while (true)
                    {
                        TcpClient client = Listener.AcceptTcpClient();
                        ClientObject clientObject = new ClientObject(client);                      
                        Task.Factory.StartNew(() => clientObject.Process(MainWindow, cancellationToken));
                        MainWindow.Dispatcher.Invoke(()=>
                        AddTextToLog(string.Format("Connected Client {0} at {1}\n", client.Client.RemoteEndPoint.ToString(), DateTime.Now.ToLongTimeString()),
                        System.Windows.Media.Brushes.Green));
                    }               
                }

                catch (Exception ex)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    if (Listener != null)
                    {
                        Listener.Stop();

                        MainWindow.Dispatcher.Invoke(() =>
                        {
                            AddTextToLog(string.Format("Sender stopped at {0}\n",
                            DateTime.Now.ToLongTimeString()), Brushes.Red);
                            MainWindow.ImageStatus.Source = new BitmapImage(new Uri("/Images/offline-icon.png", UriKind.RelativeOrAbsolute));
                            MainWindow.TbPort.IsEnabled = true;
                        });
                    }
                }
            });          
        }

        public static void AddTextToLog(string text, Brush brush)
        {
            TextRange tr = new TextRange(MainWindow.TbLog.Document.ContentEnd, MainWindow.TbLog.Document.ContentEnd);
            tr.Text += text;
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
            MainWindow.TbLog.ScrollToEnd();            
        }
    }
}
