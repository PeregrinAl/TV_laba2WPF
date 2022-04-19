using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OxyPlot;
using OxyPlot.Series;


namespace TV_laba2WPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel mn = DataContext as MainViewModel;
            Random rand = new Random();
            int value_ = Convert.ToInt32(value.Text);
            int mean_ = Convert.ToInt32(mean.Text);
            int disp_ = Convert.ToInt32(dispersion.Text);
            int num_of_intervals_ = Convert.ToInt32(num_of_intervals.Text);
            List<double> x_val ;
            int[] frequency = new int[num_of_intervals_];
           
            RectangleBarSeries rectangleBarSeries = new RectangleBarSeries();
            LineSeries lineSeries = new LineSeries();
            rectangleBarSeries.FillColor = OxyColor.FromRgb(200,150,250);
            rectangleBarSeries.StrokeColor = OxyColor.FromRgb(150, 100, 200);

            /*for (int i = 0; i < value_; i++) {
                x_val[i] = rand.NextDouble();
            }*/

            if (distribution_type.SelectedIndex == 0)
            {
                //Exp

                
            }

            if (distribution_type.SelectedIndex == 1)
            {
                //Normal
                x_val = new List<double>(new double[value_]);
                int disp_y = disp_*value_; //дисперсия случ. величины при выборке больше 100 - в N раз больше. Иначе - в (N-1)^2 / N
                double a = mean_ - Math.Sqrt(3) * Math.Sqrt(disp_y);
                double b = mean_ + Math.Sqrt(3) * Math.Sqrt(disp_y);
                int height = 0;

                for (var i = 0; i < value_; i++) //выборка по нормальному
                {
                    for (var j = 0; j < value_; j++) //равномерное распределение
                    {
                        x_val[i] = x_val[i] + a + rand.NextDouble() * (b - a);

                    }

                    x_val[i] /= value_;
                }

                double min = x_val.Min();
                double max = x_val.Max();
                var incr = (Math.Abs(min) + Math.Abs(max)) / num_of_intervals_;

                x_val.Sort();
                for (int i = 0; i < num_of_intervals_; i++)
                {
                    foreach (var x in x_val)
                    {
                        if (x >= min + incr * i && x < min + incr * (i + 1))
                        {
                            frequency[i]++;
                            height++;
                        }
                        else if (x >= min + incr * (i + 1)) {
                            double xx = ((min + incr * i) + (min + incr * (i + 1))) / 2;
                            double y = (1 / (disp_ * Math.Sqrt(2 * Math.PI))) * Math.Exp((-1 * ((Math.Pow(xx - mean_, 2)) / (2 * disp_ * disp_))));

                            lineSeries.Points.Add(new DataPoint(xx, y * incr * value_));
                            //lineSeries.Points.Add(new DataPoint(xx, y * incr * value_));
                            break;
                        }
                    }
                    rectangleBarSeries.Items.Add(new RectangleBarItem(min + incr * i, 0, min + incr * (i + 1), frequency[i]));
                }
                
                mn.Model.Series.Clear();
                mn.Model.PlotType = PlotType.XY;
                mn.Model.Series.Add(rectangleBarSeries);
                mn.Model.Series.Add(lineSeries);
                _plotView.InvalidatePlot(true);
            }

            if (distribution_type.SelectedIndex == 2)
            {
                //Uniform


            }
            
        }
    }
}
