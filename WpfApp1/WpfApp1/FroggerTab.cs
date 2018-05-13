using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Thorlabs.MotionControl.DeviceManagerCLI;
using Thorlabs.MotionControl.GenericMotorCLI;
using Thorlabs.MotionControl.KCube.DCServoCLI;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private KCubeDCServo KDC101;

        private void PlotHeatMap()
        {
            int N = 127;
            int M = 127;

            double[] x = new double[M + 1];
            double[] y = new double[N + 1];
            double[,] f = new double[M, N];
            double phase = 0;

            // Coordinate grid is constant
            for (int i = 0; i <= N; i++)
                x[i] = -Math.PI + 2 * i * Math.PI / N;

            for (int j = 0; j <= M; j++)
                y[j] = -Math.PI / 2 + j * Math.PI / M;


            // Data array is updated
            for (int i = 0; i < N; i++)
                for (int j = 0; j < M; j++)
                    f[i, j] = Math.Sqrt(x[i] * x[i] + y[j] * y[j]) * Math.Abs(Math.Cos(x[i] * x[i] + y[j] * y[j] + phase));

            long mapid = heatMap1.Plot(f, x, y);

        }


        private void singleButton_Click(object sender, RoutedEventArgs e)
        {
            DisconnectDevice();

            // open a connection to the device.
            try
            {
                KDC101 = KCubeDCServo.CreateKCubeDCServo(motorNO);
                KDC101.Connect(motorNO);
                KDC101.WaitForSettingsInitialized(5000);

                KDC101.GetMotorConfiguration(motorNO, DeviceConfiguration.DeviceSettingsUseOptionType.UseConfiguredSettings);

                // start the device polling
                KDC101.StartPolling(250);
                // needs a delay so that the current enabled state can be obtained
                Thread.Sleep(500);
                // enable the channel otherwise any move is ignored 
                KDC101.EnableDevice();
                // needs a delay to give time for the device to be enabled
                //Thread.Sleep(500);

                // display info about device
                DeviceInfo deviceInfo = KDC101.GetDeviceInfo();
                infoTextBox.Text +=$"Device {deviceInfo.SerialNumber} = {deviceInfo.Name}";

                KDC101.StatusChanged += statusChangedFun;

            }
            catch (Exception)
            {
                // connection failed
                infoTextBox.Text +=("Failed to open device");
                return;
            }


        }

        private void continuousButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO: continuous scan
            KDC101.MoveTo(6.0005m, 0);
        }

        private void statusChangedFun(object sender, StatusEventArgs e)
        {
            //TODO: 这段应该删掉改用waitcallback
            //这段没有意义因为如果在home或者move，每次轮询都会产生一个此事件
            MotorStates motorStates = KDC101.State;
            infoTextBox.Dispatcher.BeginInvoke(DispatcherPriority.Loaded,
                new Action(() => {
                    infoTextBox.Text += $"Status changed: {motorStates.ToString()}";
                }));
            
        }



        private void forcestopButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO: force stop        
            KDC101.Home(0);
        }

        private string motorNO;

    }
}
