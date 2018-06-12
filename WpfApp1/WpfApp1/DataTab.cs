using System;
using System.Windows;
using FROG.DataAnalysis;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private DataAnalysis da = new DataAnalysis();
        private DataAnalysis db = new DataAnalysis();

        private void selectFileButton_Click(object sender, RoutedEventArgs e)
        {
            db.TimeLimit = int.Parse(timeoutTextBox.Text);
            db.RunAlgorithmWithSelectedFileAsync();
        }

        private void resultShow()
        {
            if (db.ResultCount>0)
            {
                db.GetResult(out double duration, out double spectralWidth, out double[][] data);
                Dispatcher.BeginInvoke(new Action(() =>
                   {
                       outputTextBlock.Text =
                       $"Pulse Duration: {duration} fs\n" +
                       $"Spectral Width: {spectralWidth} nm\n\n" +
                       outputTextBlock.Text;
                       EtAmpGraph.Plot(data[2], data[0]);
                       EtPhaGraph.Plot(data[2], data[1]);
                       TEAmpGraph.Plot(data[5], data[3]);
                       TEPhaGraph.Plot(data[5], data[4]);
                   }));
            }
        }
    }
}