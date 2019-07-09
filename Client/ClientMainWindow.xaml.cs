using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
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

        private async void ButStart_Click(object sender, RoutedEventArgs e)
        {
            if (ButStart.Content.Equals("Start"))
            {
                EnableControls(false);
                try
                {
                    IPAddress ipAddress = IPAddress.Parse(TbSenderIP.Text);
                    int port = Convert.ToInt32(TbSenderPort.Text);
                    _cancellationTokenSource = new CancellationTokenSource();
                    _cancellationToken = _cancellationTokenSource.Token;                    

                    await Task.Factory.StartNew(() =>
                    {
                        using (TcpClient client = new TcpClient())
                        {
                            try
                            {
                                ProgressIndeterminate(true);

                                client.Connect(ipAddress, port);

                                ProgressIndeterminate(false);

                                using (NetworkStream stream = client.GetStream())
                                {
                                    using (BinaryReader br = new BinaryReader(stream))
                                    {
                                        while (true)
                                        {
                                            _cancellationToken.ThrowIfCancellationRequested();
                                            int len = br.ReadInt32();
                                            byte[] arr = br.ReadBytes(len);

                                            Bitmap bitmap = ByteToImage(arr);
                                            Dispatcher.Invoke(() => PictureBox.Source = ImageSourceForBitmap(bitmap));
                                        }
                                    }
                                }
                            }
                            catch(Exception ex)
                            {                              
                                MessageBox.Show(ex.Message);                                
                                EnableControls(true);
                            }
                        }                          
                    });
                }
                catch(OperationCanceledException)
                {                   
                }
                catch (Exception ex)
                {                   
                    MessageBox.Show(ex.Message);
                }
                EnableControls(true);
                ProgressIndeterminate(false);
            }
            else if(ButStart.Content.Equals("Stop"))
            {
                _cancellationTokenSource.Cancel();
            }
           
        }

        public Bitmap ByteToImage(byte[] buffer)
        {
            using (MemoryStream mStream = new MemoryStream())
            {
                mStream.Write(buffer, 0, buffer.Length);
                mStream.Seek(0, SeekOrigin.Begin);               

                return new Bitmap(mStream);
            }               
        }

        private void EnableControls(bool enable)
        {
            Action action = new Action(() =>
            {
                ButStart.Content = enable ? "Start" : "Stop";
                TbSenderIP.IsEnabled = TbSenderPort.IsEnabled = enable;
            });

            if (!Dispatcher.CheckAccess()) 
            {
                Dispatcher.Invoke(action);             
            }
            else
            {
                action();
            }
        }


        private void ProgressIndeterminate(bool show)
        {
            Action action = new Action(() =>
            {
                ProgressBar.IsIndeterminate = show;
                ProgressBar.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
                ButStart.IsEnabled = !show;
            });

            if (!Dispatcher.CheckAccess()) 
            {
                Dispatcher.Invoke(action);
            }
            else
            {
                action();
            }
        }

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public ImageSource ImageSourceForBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(handle);
            }
        }
    }
}
