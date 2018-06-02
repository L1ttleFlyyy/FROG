using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace FROG.DataAnalysis
{
    class DataAnalysis
    {
        //Work Directory which contains "DataAnalysis", "data" and "Debug"
        private static string wd = Path.GetDirectoryName(Directory.GetCurrentDirectory());
        //private const string wd = @"C:\Users\WinDev\source\repos\";
        private static ProcessStartInfo _startinfo = new ProcessStartInfo
        {
            FileName = wd + @"\DataAnalysis\DataAnalysis.exe",
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = wd + @"\data",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        private static OpenFileDialog _ofd = new OpenFileDialog
        {
            DefaultExt = "txt",
            InitialDirectory = wd + @"\data",
            Filter = "Frog Text Files|*FROG.txt",
            Multiselect = false,
            Title = "Choose a ****_FROG.txt data"
        };

        public delegate void AlgorithmFinishedHandler();
        public event AlgorithmFinishedHandler AlgorithmFinishedEvent;

        private ConcurrentQueue<double> _resultDurationBuffer = new ConcurrentQueue<double>();
        private ConcurrentQueue<double> _resultWaveBuffer = new ConcurrentQueue<double>();
        private ConcurrentQueue<double[]> _resultDataBuffer = new ConcurrentQueue<double[]>();

        public int ResultCount => _resultDurationBuffer.Count;

        public void GetResult(out double time, out double wavelenth, out double[][] data)
        {
            data = new double[6][];
            bool res;
            double[] tempdata;
            do
            {
                res = _resultDurationBuffer.TryDequeue(out time);
            } while (!res);

            do
            {
                res = _resultWaveBuffer.TryDequeue(out wavelenth);
            } while (!res);

            for (int ii = 0; ii < 6; ii++)
            {
                do
                {
                    res = _resultDataBuffer.TryDequeue(out tempdata);
                } while (!res);
                data[ii] = tempdata;
            }
        }

        public async void RunAlgorithmAsync(string str)
        {
            await AlgorithmKernel(str);
        }

        public async void RunAlgorithmWithSelectedFileAsync()
        {
            if (_ofd.ShowDialog() != true)
            {
                MessageBox.Show("No frog data selected!");
                return;
            }
            string fileprefix = Path.GetFileNameWithoutExtension(_ofd.FileName);
            fileprefix = fileprefix.Remove(fileprefix.Length - 4, 4);
            await AlgorithmKernel(fileprefix);
        }

        private async Task AlgorithmKernel(string str)
        {
            _startinfo.Arguments = str;
            await Task.Run(new Action(() =>
            {
                string rst;
                try
                {
                    using (Process _exe = new Process())
                    {
                        _exe.StartInfo = _startinfo;
                        _exe.Start();
                        rst = _exe.StandardOutput.ReadToEnd();
                        _exe.WaitForExit();
                        _exe.Close();
                    }
                    ReadResultData(wd + @"\data\" + str + "result.txt", ref _resultDataBuffer);
                    string[] output_List = rst.Split(new char[] { '\n' });
                    _resultDurationBuffer.Enqueue(double.Parse(output_List[0]));
                    _resultWaveBuffer.Enqueue(double.Parse(output_List[1]));
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message);
                }
                return;
            }));
            AlgorithmFinishedEvent?.Invoke();
        }

        private void ReadResultData(string path, ref ConcurrentQueue<double[]> databuffers)
        {
            using (StreamReader sr = new StreamReader(path))
            {
                for (int ii = 0; ii < 6; ii++)
                {
                    databuffers.Enqueue(String2DoubleArray(sr.ReadLine()));
                }
            }

        }

        private double[] String2DoubleArray(string str)
        {
            List<double> tempList = new List<double>();
            string tempString = string.Empty;
            foreach (char ii in str)
            {
                if (ii != ' ')
                {
                    tempString += ii;
                }
                else
                {
                    tempList.Add(double.Parse(tempString));
                    tempString = string.Empty;
                }
            }
            tempList.Add(double.Parse(tempString));
            return tempList.ToArray();
        }
    }
}
