using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;
using Thorlabs.CCS_Series;


namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //成员定义
        const int STATUS_IDLE = 0x0002;//if(STATUS_IDLE & status)
        const int STATUS_TRIGGERED = 0x0004;
        const int STATUS_SCANNING = 0x0008;
        const int STATUS_DATA = 0x0010;

        private TLCCS ccsSeries;

        private DispatcherTimer scanTimer = new DispatcherTimer();

        private double[] wavelength = new double[3648];

        private double[] spectrumdata = new double[3648];

        private double minWave, maxWave;

        private Random randomtest = new Random();   

        public MainWindow()
        {
            InitializeComponent();
            //PlotLineGraph();
            scanTimer.Tick += new EventHandler(timerScanFun);
        }

        private void timerScanFun(object sender, EventArgs e)
        {
            if (ccsSeries != null)
            {
                try
                {
                    int res = ccsSeries.getDeviceStatus(out int status);
                    switch (status)
                    {
                        case 37:
                            res = ccsSeries.getScanData(spectrumdata);
                            break;
                        case 1:
                            res = ccsSeries.startScan();
                            break;
                        default:
                            scanTimer.Stop();
                            MessageBox.Show($"Unknown status: {status.ToString()}");
                            break;
                    }
                }
                catch (Exception err)
                {
                    scanTimer.Stop();
                    MessageBox.Show(err.ToString());
                }
            }
            else
            {
                //TODO:测试定时器代码
                Action action = new Action(PlotLineGraph);
                scanTimer.Dispatcher.BeginInvoke(DispatcherPriority.Loaded,action);
                //PlotLineGraph();
            }
        }


        private void PlotLineGraph()
        {
            var x = (double[])wavelength.Clone();
            var y = (double[])spectrumdata.Clone();

            for (int i=0;i<3648;i++)
            {
                x[i] = i+1;
                y[i] = randomtest.NextDouble();
            }

            lineGraph1.Plot(x, y);

        }

        private void dispose_Button_Click(object sender, RoutedEventArgs e)
        {
            // release the device
            if (ccsSeries != null)
            {
                try
                {
                    ccsSeries.Dispose();
                }
                catch (Exception err)
                {
                    MessageBox.Show("Can't release CCS" + err.ToString());
                }
            }
            else
            {
                MessageBox.Show("No CCS connected!");
            }

        }

        private void connect_Button_Click(object sender, RoutedEventArgs e)
        {
            if (serialNoBox.Text.Length == 0)
            {
                MessageBox.Show("Please insert the 8 numerics of the serial number");
                return;
            }

            // set the busy cursor
            Cursor = Cursors.Wait;

            // 0x8081: CCS100
            // 0x8083: CCS125
            // 0x8085: CCS150
            // 0x8087: CCS175
            // 0x8089: CCS200
            try
            {
                if (ccsSeries == null)
                {
                    string instrumentNumber = "0x8089";
                    string resourceName = "USB0::0x1313::" + instrumentNumber + "::M" + serialNoBox.Text.ToString() + "::RAW";
                    // initialize device with the resource name (be sure the device is still connected)
                    ccsSeries = new TLCCS(resourceName, false, false);
                }
                int res = ccsSeries.getWavelengthData((short)0, wavelength, out minWave, out maxWave);
                DeviceInformation();
            }
            catch (Exception err)
            {
                MessageBox.Show("Connected failed" + err.ToString());
            }
            Cursor = Cursors.Arrow;
        }

        private void scan_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ccsSeries == null)
            {
                //MessageBox.Show("No CCS connected");
                //TODO:测试定时器
                if (scanTimer.IsEnabled)
                {
                    scan_Button.Background = connect_Button.Background.Clone();
                    scanTimer.Stop();
                }
                else
                {
                    scanTimer.Interval = TimeSpan.FromMilliseconds(1000*double.Parse(intTimeBox.Text));
                    scan_Button.Background = new SolidColorBrush(SystemColors.ActiveCaptionColor);
                    scanTimer.Start();
                }
            }
            else
            {
                if (scanTimer.IsEnabled)
                {
                    scanTimer.Stop();
                    scan_Button.Background = connect_Button.Background.Clone();
                }
                else
                {
                    double intTime = double.Parse(intTimeBox.Text);
                    scanTimer.Interval = TimeSpan.FromMilliseconds(1000*intTime);
                    int res = ccsSeries.setIntegrationTime(intTime);
                    scan_Button.Background = new SolidColorBrush(SystemColors.ActiveCaptionColor);
                    scanTimer.Start();
                }
            }
        }

        private void DeviceInformation()
        {
            System.Text.StringBuilder manufactuer, device, serial, firmware, driver;
            manufactuer = new System.Text.StringBuilder(256);
            device = new System.Text.StringBuilder(256);
            firmware = new System.Text.StringBuilder(256);
            driver = new System.Text.StringBuilder(256);
            serial = new System.Text.StringBuilder(256);
            int res = ccsSeries.getDeviceStatus(out int status);
            ccsSeries.identificationQuery(manufactuer, device, serial, firmware, driver);
            infoBox.Text
                = $"Device: {manufactuer.ToString()} " + $"{device.ToString()}\n"
                + $"SerialNumber: {serial.ToString()}\n"
                + $"Firmware: {firmware.ToString()}\n"
                + $"Driver: {driver.ToString()}\n"
                + $"Status: {status.ToString()}";
        }
    }


}

