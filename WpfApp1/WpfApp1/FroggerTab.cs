using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Thorlabs.ccs.interop64;
using Thorlabs.MotionControl.DeviceManagerCLI;
using Thorlabs.MotionControl.KCube.DCServoCLI;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private KCubeDCServo KDC101;

        private Task getFrogTraceTask;

        private Task saveToFileTask;

        private void PlotHeatMap()
        {

            long mapid = heatMap1.Plot(FrogTrace, tau, wavelength);

        }


        private void singleButton_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                if (KDC101 == null || ccsSeries == null)
                {
                    MessageBox.Show("Spectrometer or Motor not connected");
                    return;
                }

                currentposition = double.Parse(fromBox.Text);
                stopposition = double.Parse(toBox.Text);
                interval = double.Parse(stepBox.Text);
                ccsSeries.getIntegrationTime(out inttime_frog);

                if (scanTimer.IsEnabled)
                {
                    scanTimer.Stop();
                    scan_Button.Dispatcher.BeginInvoke(new Action(()=> { scan_Button.Background = connect_Button.Background.Clone(); }));

                }

                if (getFrogTraceTask == null)
                {
                    getFrogTraceTask = new Task(getFrogTrace);
                    getFrogTraceTask.Start();
                }
                else if (getFrogTraceTask.IsCompleted)
                {
                    getFrogTraceTask = new Task(getFrogTrace);
                    getFrogTraceTask.Start();
                }
                else
                    MessageBox.Show("Scanning in process");


            }
            catch (Exception err)
            {
                // connection failed
                MessageBox.Show(err.Message);
                infoTextBox.Text = $"Failed to open device\n"
                    + infoTextBox.Text;
                return;
            }

        }

        private void continuousButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO: continuous scan
            try
            {
                if (saveToFileTask == null)
                {
                    saveToFileTask = new Task(DataToTxt);
                    saveToFileTask.Start();
                }
                else if(saveToFileTask.IsCompleted)
                {
                    saveToFileTask = new Task(DataToTxt);
                    saveToFileTask.Start();
                }
                else
                    MessageBox.Show("Saving in progress");
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private ulong _taskID;

        private void CommandCompleteFunction(ulong taskID)
        {
            if ((_taskID > 0) && (_taskID == taskID))
            {
                infoTextBox.Dispatcher.BeginInvoke(DispatcherPriority.Loaded,
    new Action(() =>
    {
        infoTextBox.Text = $"Status changed: {KDC101.State}\n"
                            + $"Position: {KDC101.Position}\n"
                            + infoTextBox.Text;
    }));
            }

        }


        private void forcestopButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO: force stop
            try
            {
                if (getFrogTraceTask != null && !getFrogTraceTask.IsCompleted)
                {
                    forcestop = true;
                    getFrogTraceTask.Wait();
                }
                _taskID = KDC101.MoveTo(0m, CommandCompleteFunction);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private string motorNO = "27501200";


        //private int pollingTime = 250;

        private double[,] FrogTrace;

        private double[] tau;

        private double currentposition;
        private double stopposition;
        private double interval;
        private double inttime_frog;

        private bool forcestop = false;

        //private bool inProcess = false;

        private void getFrogTrace()
        {

            int nsteps = (int)Math.Floor((stopposition - currentposition) / interval) + 1;

            tau = new double[nsteps + 1];
            FrogTrace = new double[nsteps, 3647];

            for (int i = 0; i < nsteps + 1; i++)
            {
                tau[i] = currentposition + (i - 0.5) * interval;
            }

            for (int i = 0; i < nsteps; i++)
            {
                if (forcestop)
                {
                    forcestop = false;
                    return;
                }

                KDC101.MoveTo((decimal)currentposition, 10000);

                ccsSeries.getDeviceStatus(out int status);
                if (status == 17 || status == 1) ccsSeries.getScanData(spectrumdata);

                ccsSeries.startScan();

                while (true)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(inttime_frog));
                    ccsSeries.getDeviceStatus(out status);
                    if (status == 1)
                    {
                        ccsSeries.getScanData(spectrumdata);
                        break;
                    }
                    else if(status == 17)
                    {
                        ccsSeries.startScan();
                    }

                }

                for (int j = 0; j < 3647; j++)
                {
                    FrogTrace[i, j] = spectrumdata[j];
                }

                currentposition += interval;
                heatMap1.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(PlotHeatMap));
            }
        }

        private void DataToTxt()
        {
            if (FrogTrace == null)
            {
                MessageBox.Show("Data not availiable");
                return;
            }

            int M = FrogTrace.GetLength(0);
            int N = FrogTrace.GetLength(1);
            StringBuilder sb1 = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            StringBuilder sb3 = new StringBuilder();
            for (int j = 0; j < N; j++)
            {
                for (int i = 0; i < M; i++)
                {
                    if (i != M - 1)
                    {
                        sb1.Append($"{FrogTrace[i, j]} ");
                    }
                    else
                    {
                        sb1.Append($"{FrogTrace[i, j]}\r\n");
                    }
                }
            }

            for (int i = 0; i < tau.Length-1; i++)
            {
                sb2.Append($"{tau[i]+0.5*interval} ");
            }

            foreach (var item in wavelength)
            {
                sb3.Append($"{item} ");
            }

            DateTime currentTime = DateTime.Now;
            string day = currentTime.Day.ToString();
            string hour = currentTime.Hour.ToString();
            string minute = currentTime.Minute.ToString();
            string second = currentTime.Second.ToString();

            //var crp = Directory.GetCurrentDirectory();
            string path1 = $"./{day}_{hour}_{minute}_{second}_FROG.txt";
            string path2 = $"./{day}_{hour}_{minute}_{second}_Tau.txt";
            string path3 = $"./{day}_{hour}_{minute}_{second}_wavelenth.txt";

            using (StreamWriter sw = new StreamWriter(path1))
            {
                sw.Write(sb1.ToString());
            }


            using (StreamWriter sw = new StreamWriter(path2))
            {
                sw.Write(sb2.ToString());
            }

            using (StreamWriter sw = new StreamWriter(path3))
            {
                sw.Write(sb3.ToString());
            }
            infoTextBox.Dispatcher.BeginInvoke(new Action(()=> { infoTextBox.Text = "Frog Trace saved\n" + infoTextBox.Text; }));
        }

    }
}
