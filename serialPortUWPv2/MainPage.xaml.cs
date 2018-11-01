using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using System.Collections.ObjectModel;
using Windows.Devices.Enumeration;
using System.Threading;
using System.Threading.Tasks;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace serialPortUWPv2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SerialDevice serialPort = null;

        DataWriter dataWriterObject = null;
        DataReader dataReaderObject = null;

        private ObservableCollection<DeviceInformation> listOfDevices;

        private CancellationTokenSource ReadCancellationTokenSource;



        string received = "";






        public MainPage()
        {
            this.InitializeComponent();

            listOfDevices = new ObservableCollection<DeviceInformation>();
            ListAvailablePorts();
        }

        private async void ListAvailablePorts()
        {
            try
            {
                string aqs = SerialDevice.GetDeviceSelector();
                var dis = await DeviceInformation.FindAllAsync(aqs);

                for (int i = 0; i < dis.Count; i++)
                {
                    listOfDevices.Add(dis[i]);
                }

                lstSerialDevices.ItemsSource = listOfDevices;

                lstSerialDevices.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                txtMessage.Text = ex.Message;
                //txtMessage.Text = "List Port" + ex.Message;
            }
        }

        private void ButtonConnectToDevice_Click(object sender, RoutedEventArgs e)
        {
            SerialPortConfiguration();
        }

        private async void SerialPortConfiguration()
        {
            var selection = lstSerialDevices.SelectedItems;

            if (selection.Count <= 0)
            {
                txtMessage.Text = "Select an object for serial connection!";
                return;
            }

            DeviceInformation entry = (DeviceInformation)selection[0];

            try
            {
                serialPort = await SerialDevice.FromIdAsync(entry.Id);
                serialPort.WriteTimeout = TimeSpan.FromMilliseconds(1000);
                serialPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);
                serialPort.BaudRate = 115200;
                serialPort.Parity = SerialParity.None;
                serialPort.StopBits = SerialStopBitCount.One;
                serialPort.DataBits = 8;
                serialPort.Handshake = SerialHandshake.None;
                txtMessage.Text = "Serial port correctly configured!";

                ReadCancellationTokenSource = new CancellationTokenSource();
                Listen();
            }
            catch (Exception ex)
            {
                txtMessage.Text = ex.Message;
                //txtMessage.Text = "Port Config" + ex.Message;
            }
        }

        private async void Listen()
        {
            try
            {
                if (serialPort != null)
                {
                    dataReaderObject = new DataReader(serialPort.InputStream);

                    while (true)
                    {
                        await ReadData(ReadCancellationTokenSource.Token);
                    }
                }
            }
            catch (Exception ex)
            {
                txtMessage.Text = ex.Message;
                //txtMessage.Text = "Listen" + ex.Message;
                //received = "";

                //if (ex.GetType.Name=="TaskCancelledException")

            }
            finally
            {

            }

        }
        private async Task ReadData(CancellationToken cancellationToken)
        {
            Task<UInt32> loadAsyncTask;

            int calChkSum = 0;

            uint ReadBufferLength = 1;

            cancellationToken.ThrowIfCancellationRequested();

            dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;

            loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask(cancellationToken);

            UInt32 bytesRead = await loadAsyncTask;

            if (bytesRead > 0)
            {
                received += dataReaderObject.ReadString(bytesRead);
                //txtReceived.Text = received + txtReceived.Text;
                if (received[0] == '#')
                {
                    if (received.Length > 3)
                    {
                        if (received[2] == '#')
                        {
                            //txtReceived.Text = received;
                            if (received.Length > 42)
                            {
                                txtReceived.Text = received + txtReceived.Text;

                                txtPacketNum.Text = received.Substring(3, 3);
                                /* 
                                 int AN0 = Convert.ToInt32(received.Substring(6, 4));
                                 int AN1 = Convert.ToInt32(received.Substring(10, 4));
                                 int AN2 = Convert.ToInt32(received.Substring(14, 4));
                                 int AN3 = Convert.ToInt32(received.Substring(18, 4));
                                 int AN4 = Convert.ToInt32(received.Substring(22, 4));
                                 */

                                txtAN0.Text = received.Substring(6, 4);
                                txtAN1.Text = received.Substring(10, 4);
                                txtAN2.Text = received.Substring(14, 4);
                                txtAN3.Text = received.Substring(18, 4);
                                txtAN4.Text = received.Substring(22, 4);
                                txtAN5.Text = received.Substring(26, 4);
                                txtBinOut.Text = received.Substring(30, 8);
                                txtChkSum.Text = received.Substring(38, 3);

                                for (int i = 3; i < 38; i++)
                                {
                                    calChkSum += (byte)received[i];
                                }
                                txtCalChkSum.Text = Convert.ToString(calChkSum);
                                received = "";


                            }

                        }
                        else
                        {
                            received = "";
                        }
                    }
                }
                else
                {
                    received = "";
                }
            }


        }

        private async void ButtonWrite_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort != null)
            {
                var dataPacket = txtSend.Text.ToString();
                dataWriterObject = new DataWriter(serialPort.OutputStream);
                await sendPacket(dataPacket);

                if (dataWriterObject != null)
                {
                    dataWriterObject.DetachStream();
                    dataWriterObject = null;
                }

            }
        }

        private async Task sendPacket(string value)
        {
            var dataPacket = value;

            Task<UInt32> storeAsyncTask;

            if (dataPacket.Length !=0)
            {
                dataWriterObject.WriteString(dataPacket);

                storeAsyncTask = dataWriterObject.StoreAsync().AsTask();

                UInt32 bytesWritten = await storeAsyncTask;


                if (bytesWritten > 0)
                {
                    txtMessage.Text = "Value sent correcly";

                }
            }

            else
            {
                txtMessage.Text = "No Value Sent";
            }

        }
    }
}

       
        /*
       private void Page_Loaded(object sender, RoutedEventArgs e)
       {

       }

       private void Disconnect_Click(object sender, RoutedEventArgs e)
       {

       }

       private void RefreshPort_Click(object sender, RoutedEventArgs e)
       {
           lstAvailablePorts();                                           //On Refresh; reload Ports
       }
       */

 