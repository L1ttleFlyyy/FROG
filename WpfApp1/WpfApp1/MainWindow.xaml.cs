﻿using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;
using Thorlabs.ccs.interop64;


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

        bool dataReady = false;

        private DispatcherTimer scanTimer = new DispatcherTimer();

        private double[] wavelength = new double[3648];
        private double[] simpleWav = new double[365];

        private double[] spectrumdata = new double[3648];
        private double[] simpleData = new double[365];

        private double minWave, maxWave;  

        public MainWindow()
        {
            InitializeComponent();
            scanTimer.Tick += new EventHandler(timerScanFun);
            PlotHeatMap();
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
                        case 1:
                            Action action = new Action(getData);
                            scanTimer.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, action);
                            break;
                        case 5:
                        case 277:
                        case 17:
                        case 21:
                        case 35:
                            Func<int> func = new Func<int>(ccsSeries.startScan);
                            scanTimer.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, func);
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
                scanTimer.Stop();
                MessageBox.Show("No CCS connected");
            }
        }

        private void getData()
        {
            int res =ccsSeries.getScanData(spectrumdata);
            simpleData = DataSimplify(spectrumdata);
            dataReady = true;
        }

        private void UpdateSpectrum(object sender, EventArgs e)
        {
            if (dataReady)
            {
                lineGraph1.Plot(simpleWav, simpleData);
                dataReady = false;
            }
        }

        private void dispose_Button_Click(object sender, RoutedEventArgs e)
        {
            // release the device
            if (ccsSeries != null)
            {
                try
                {
                    ccsSeries.Dispose();
                    ccsSeries = null;

                }
                catch (Exception err)
                {
                    MessageBox.Show("Can't release CCS" + err.ToString());
                }
                finally
                {
                    if (scanTimer.IsEnabled)
                    {
                        CompositionTarget.Rendering -= new EventHandler(UpdateSpectrum);
                        scanTimer.Stop();
                    }
                    scan_Button.Background = connect_Button.Background.Clone();
                    infoBox.Text = string.Empty;
                    
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
                simpleWav = DataSimplify(wavelength);
                DeviceInformation();
            }
            catch (Exception err)
            {
                MessageBox.Show("Connected failed\n" + err.ToString());
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        private void scan_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ccsSeries == null)
            {
                MessageBox.Show("No CCS connected");
            }
            else
            {
                if (scanTimer.IsEnabled)
                {
                    scanTimer.Stop();
                    scan_Button.Background = connect_Button.Background.Clone();
                    CompositionTarget.Rendering -= new EventHandler(UpdateSpectrum);
                }
                else
                {
                    CompositionTarget.Rendering += new EventHandler(UpdateSpectrum);
                    double intTime = double.Parse(intTimeBox.Text);
                    scanTimer.Interval = TimeSpan.FromMilliseconds(1000*intTime);
                    int res = ccsSeries.setIntegrationTime(intTime);
                    scan_Button.Background = new SolidColorBrush(SystemColors.HighlightColor);
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

        private double[] DataSimplify(double[] data)
        {
            double[] simdata = new double[365];

            for (int i=0; i<365;i++)
            {
                simdata[i] = data [10*i];
            }

            return simdata;
        }


    }


}

