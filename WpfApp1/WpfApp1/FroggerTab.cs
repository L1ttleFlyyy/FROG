using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Thorlabs.MotionControl.KCube.DCServoCLI;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private KCubeDCServo KDC101;

        private Task getFrogTraceTask;

        //private Task saveToFileTask;

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

                startposition = double.Parse(fromBox.Text);
                stopposition = double.Parse(toBox.Text);
                interval = double.Parse(stepBox.Text);
                ccsSeries.getIntegrationTime(out inttime_frog);

                // reset Spectrometer Tab
                if (scanTimer.IsEnabled)
                {
                    scanTimer.Stop();
                    //scan_Button.Dispatcher.BeginInvoke(new Action(() => { scan_Button.Background = connect_Button.Background.Clone(); }));
                    scan_Button.Background = connect_Button.Background.Clone();
                }

                if (getFrogTraceTask == null)
                {
                    da.TimeLimit = int.Parse(timeoutTextBox.Text);
                    getFrogTraceTask = new Task(getFrogTrace);
                    getFrogTraceTask.Start();
                    UpdateInfoBox("Scanning");
                }
                else if (getFrogTraceTask.IsCompleted)
                {
                    da.TimeLimit = int.Parse(timeoutTextBox.Text);
                    getFrogTraceTask = new Task(getFrogTrace);
                    getFrogTraceTask.Start();
                    UpdateInfoBox("Scanning");
                }
                else
                    MessageBox.Show("Scanning in process");


            }
            catch (Exception err)
            {
                // connection failed
                MessageBox.Show(err.Message);
                UpdateInfoBox("Scanning failed to start");
                return;
            }

        }

        private Task continuousTask;

        private void continuousButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO: continuous scan
            try
            {
                if (KDC101 == null || ccsSeries == null)
                {
                    MessageBox.Show("Spectrometer or Motor not connected");
                    return;
                }

                startposition = double.Parse(fromBox.Text);
                stopposition = double.Parse(toBox.Text);
                interval = double.Parse(stepBox.Text);
                ccsSeries.getIntegrationTime(out inttime_frog);

                // reset Spectrometer Tab
                if (scanTimer.IsEnabled)
                {
                    scanTimer.Stop();
                    //scan_Button.Dispatcher.BeginInvoke(new Action(() => { scan_Button.Background = connect_Button.Background.Clone(); }));
                    scan_Button.Background = connect_Button.Background.Clone();
                }

                if (continuousTask == null)
                {
                    da.TimeLimit = int.Parse(timeoutTextBox.Text);
                    continuousTask = new Task(continuousScan);
                    continuousTask.Start();
                    UpdateInfoBox("Continuous scan started");
                }
                else if (continuousTask.IsCompleted)
                {
                    da.TimeLimit = int.Parse(timeoutTextBox.Text);
                    continuousTask = new Task(continuousScan);
                    continuousTask.Start();
                    UpdateInfoBox("Continuous scan started");
                }
                else
                {
                    MessageBox.Show("Continuous scan in process");
                }
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
                UpdateInfoBox($"Status changed: {KDC101.State}");
                UpdateInfoBox($"Current position: {KDC101.Position}");
            }

        }


        private void forcestopButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO: force stop
            try
            {
                if (!forcestop)
                {
                    forcestop = true;
                }
                else
                {
                    MessageBox.Show("Program is trying to stop!");
                }
                if(!stopcontinuous)
                {
                    stopcontinuous = true;
                }
                else
                {
                    MessageBox.Show("Program is trying to stop!");
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private string motorNO = "27501200";

        private double[,] FrogTrace;

        private double[] tau;

        private double startposition;
        private double stopposition;
        private double interval;
        private double inttime_frog;

        private bool forcestop = false;
        private bool stopcontinuous = false;

        private void getFrogTrace()
        {
            double starttemp = startposition;
            double stoptemp = stopposition;
            int nsteps = (int)Math.Floor((stoptemp - starttemp) / interval) + 1;

            tau = new double[nsteps + 1];
            FrogTrace = new double[nsteps, 3647];

            for (int i = 0; i < nsteps + 1; i++)
            {
                tau[i] = starttemp + (i - 0.5) * interval;
            }

            bool direction = KDC101.Position < (decimal)tau[nsteps / 2];

            for (int i = 0; i < nsteps; i++)
            {
                if (forcestop)
                {
                    forcestop = false;
                    return;
                }

                if (direction)
                {
                    KDC101.MoveTo((decimal)starttemp, 10000);
                    ccsSeries.getDeviceStatus(out int status);
                    if (status == 17 || status == 1)
                    { ccsSeries.getScanData(spectrumdata); }
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
                        else if (status == 17)
                        {
                            ccsSeries.startScan();
                        }

                    }

                    for (int j = 0; j < 3647; j++)
                    {
                        FrogTrace[i, j] = spectrumdata[j];
                    }

                    starttemp += interval;
                }
                else
                {
                    KDC101.MoveTo((decimal)stoptemp, 10000);
                    ccsSeries.getDeviceStatus(out int status);
                    if (status == 17 || status == 1)
                    { ccsSeries.getScanData(spectrumdata); }
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
                        else if (status == 17)
                        {
                            ccsSeries.startScan();
                        }

                    }
                    for (int j = 0; j < 3647; j++)
                    {
                        FrogTrace[nsteps - i - 1, j] = spectrumdata[j];
                    }
                    stoptemp -= interval;
                }
                heatMap1.Dispatcher.Invoke(DispatcherPriority.Loaded, new Action(PlotHeatMap));
            }
            UpdateInfoBox("Trace Scan Finished");
            da.RunAlgorithmAsync(DataToTxt()).Wait();
            if (da.ResultCount > 0)
            {
                da.GetResult(out double duration, out double spectralwidth, out double[][] result);
                Dispatcher.Invoke(new Action(() =>
                {
                    lineGraph2.Plot(result[2], result[0]);
                }));
                UpdateInfoBox($"Pulse duration: {duration} fs");
                UpdateInfoBox($"Spectral width: {spectralwidth} nm");
            }
        }

        private string DataToTxt()
        {
            if (FrogTrace == null)
            {
                return null;
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
                        sb1.Append(FrogTrace[i, j].ToString("G4") + " ");
                    }
                    else
                    {
                        sb1.Append(FrogTrace[i, j].ToString("G4") + "\r\n");
                    }
                }
            }

            for (int i = 0; i < tau.Length - 1; i++)
            {
                sb2.Append($"{tau[i] + 0.5 * interval} ");
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
            string frog_data_id = $"{day}_{hour}_{minute}_{second}_";
            string path1 = $"../data/" + frog_data_id + "FROG.txt";
            string path2 = $"../data/" + frog_data_id + "Tau.txt";
            string path3 = $"../data/" + frog_data_id + "wavelenth.txt";

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
            //infoTextBox.Dispatcher.BeginInvoke(new Action(()=> { infoTextBox.Text = "Frog Trace saved\n" + infoTextBox.Text; }));
            UpdateInfoBox("Frog Trace Saved");
            return frog_data_id;
        }

        private void continuousScan()
        {
            if (getFrogTraceTask != null)
            {
                if (!getFrogTraceTask.IsCompleted)
                {
                    getFrogTraceTask.Wait();
                }
                while (!stopcontinuous)
                {
                    getFrogTrace();
                }
                stopcontinuous = false;
            }
            else
            {
                while (!stopcontinuous)
                {
                    getFrogTrace();
                }
                stopcontinuous = false;
            }
        }

        private void UpdateInfoBox(string str)
        {
            string currentTime = DateTime.Now.ToString();
            infoTextBox.Dispatcher.BeginInvoke(new Action(() =>
            {
                infoTextBox.Text = currentTime + " " + str + "\n" + infoTextBox.Text;
            }));
        }

    }
}
