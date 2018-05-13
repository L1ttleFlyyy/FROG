using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
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
    }
}
