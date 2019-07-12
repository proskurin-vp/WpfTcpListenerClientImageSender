using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sender
{
    public class ClientObject
    {
        private readonly TcpClient _client;
        private readonly EndPoint _remoteEndPoint;

        public ClientObject(TcpClient client)
        {
            _client = client;
            _remoteEndPoint = _client.Client.RemoteEndPoint;
        }       

        public async void Process(MainWindow mainWindow, CancellationToken cancellationToken)
        {           
            try
            {
                Rectangle rect = Screen.PrimaryScreen.Bounds;
                using (NetworkStream stream = _client.GetStream())
                {
                    using (BinaryWriter bw = new BinaryWriter(stream))
                    {
                        using (Bitmap bmp = new Bitmap(rect.Width, rect.Height))
                        {
                            using (Graphics gr = Graphics.FromImage(bmp))
                            {
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    int timeout = 0;                      
                                    while (true)
                                    {
                                        
                                        cancellationToken.ThrowIfCancellationRequested();
                                           
                                        mainWindow.Dispatcher.Invoke(() => timeout = 1000 / ((int)mainWindow.Slider1.Value));

                                        gr.CopyFromScreen(rect.Left, rect.Top, 0, 0, rect.Size);
                                        ms.Position = 0;
                                        bmp.Save(ms, ImageFormat.Jpeg);
                                        byte[] arr = ms.ToArray();
                                        bw.Write(arr.Length);
                                        bw.Write(arr);
                                        bw.Flush();
                                        
                                        //DateTime dt = DateTime.Now;
                                        //await mainWindow.Dispatcher.BeginInvoke(new Action(() => Server.AddTextToLog(string.Format("Send {0} bytes on {1} at {2}\n",
                                        //arr.Length, _client.Client.RemoteEndPoint.ToString(), dt.ToLongTimeString() + "." + dt.Millisecond),
                                        //System.Windows.Media.Brushes.Green)));

                                        Thread.Sleep(timeout);       
                                                                             
                                    }
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception)
            {
                if(_client.Client.Connected == false)
                {
                    DateTime dt = DateTime.Now;
                    await mainWindow.Dispatcher.BeginInvoke(new Action(() => Server.AddTextToLog(
                        $"Disconnected Client {_remoteEndPoint.ToString()}\n",
                    System.Windows.Media.Brushes.Green)));
                }
            }
        }
    }
}
