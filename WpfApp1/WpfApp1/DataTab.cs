using System;
using System.Windows;
using FROG.DataAnalysis;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private DataAnalysis da = new DataAnalysis();

        private void selectFileButton_Click(object sender, RoutedEventArgs e)
        {
            da.RunAlgorithmWithSelectedFileAsync();
        }

        private void resultShow()
        {
            da.GetResult(out double duration, out double spectralWidth, out double[][] data);
            Dispatcher.BeginInvoke(new Action(() =>
               {
                   outputTextBlock.Text = DateTime.Now.ToLongTimeString() + "\n" +
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