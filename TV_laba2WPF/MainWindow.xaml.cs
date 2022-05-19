using System;
using System.Data;
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
using MathNet.Numerics.Statistics;



namespace TV_laba2WPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private double sum = 0;
        private int value;
        private int paramA;
        private int paramB;
        private int numOfIntervals;
        private List<double> x_val;
        private int[] frequency;
        private double incr;
        private double min;
        private double max;
        private MainViewModel mn;
        private Random rand = new Random();
        private RectangleBarSeries rectangleBarSeries = new RectangleBarSeries();
        private LineSeries lineSeries = new LineSeries();
        private LineSeries lineSeries_rp = new LineSeries();
        private Func<double, double> myMethodName;
        private double hn;

        private double expectationEstimate;
        private double varianceEstimate;
        private List<double> modeEstimate;
        private double medianEstimate;
        private double deviationEstimate;

        private double sumExpectationEstimate = 0;
        private double sumExpectationEstimateSquare = 0;
        private double sumVarianceEstimate = 0;
        private double maxModeEstimate = 0;
        private double sumMedianEstimate = 0;
        private double tmp = 0;
        private double sumDeviationEstimate = 0;

        private const double INCRIMENT = 0.01;


        public MainWindow()
        {
            InitializeComponent();
        }
        private void Index_Changed(object sender, RoutedEventArgs e) {
            if (distribution_type.SelectedIndex == 0)
            {
                mean.Text = "1";
                dispersion.IsEnabled = false;
                dispersion.Visibility = Visibility.Hidden;

                B_.Visibility = Visibility.Hidden;

                #region img_definition
                Image ImageContainer = new Image();
                ImageSource image_ = new BitmapImage(new Uri("/image/exp.jpg", UriKind.Relative));
                ImageContainer.Source = image_;
                image.Source = image_;
                x_val = new List<double>(new double[value]);
                #endregion img_definition
            }
            else if (distribution_type.SelectedIndex == 1)
            {
                dispersion.IsEnabled = true;
                dispersion.Visibility = Visibility.Visible;
                B_.Visibility = Visibility.Visible;
                mean.Text = "0";
                dispersion.Text = "1";

                #region img_definition
                Image ImageContainer = new Image();
                ImageSource image_ = new BitmapImage(new Uri("/image/normal.jpg", UriKind.Relative));
                ImageContainer.Source = image_;
                image.Source = image_;
                x_val = new List<double>(new double[value]);
                #endregion img_definition
            }
            else {
                dispersion.IsEnabled = true;
                dispersion.Visibility = Visibility.Visible;
                B_.Visibility = Visibility.Visible;
                mean.Text = "0";
                dispersion.Text = "1";

                #region img_definition
                Image ImageContainer = new Image();
                ImageSource image_ = new BitmapImage(new Uri("/image/uniform.jpg", UriKind.Relative));
                ImageContainer.Source = image_;
                image.Source = image_;
                x_val = new List<double>(new double[value]);
                #endregion img_definition
            }
        }

        private void Index_Changed_kernel(object sender, RoutedEventArgs e) { 
            switch(kernel_type.SelectedIndex)
            {
                case 0:
                    myMethodName = Parabolic_Kernel;
                    break;
                case 1:
                    myMethodName = Cauchy_Kernel;
                    break;
                case 2:
                    myMethodName = Epan_Kernel;
                    break;
                case 3:
                    myMethodName = Sigmoid_Kernel;
                    break;
                case 4:
                    myMethodName = Triangular_Kernel;
                    break;
                case 5:
                    myMethodName = Uniform_Kernel;
                    break;
                case 6:
                    myMethodName = Exp_Kernel;
                    break;
                default:
                    myMethodName = Parabolic_Kernel;
                    break;

            }
        }

        #region kernels
        private double Parabolic_Kernel(double u)
        {
            return Math.Exp(-Math.Abs(u)) / 2;
        }
        private double Epan_Kernel(double u)
        {
            if (Math.Abs(u) <= 1) return (double)3 / 4 * (1 - Math.Pow(u, 2));
            else return 0;
        }
        private double Cauchy_Kernel(double u)
        {
            return 1 / Math.PI  * ((double)1 / (1 + Math.Pow(u, 2)));
        }
        private double Sigmoid_Kernel(double u)
        {
            return 2 / Math.PI * (1 / (Math.Exp(u) + Math.Exp(-u)));
        }
        private double Triangular_Kernel(double u)
        {
            if (Math.Abs(u) <= 1) return (double)1 - Math.Abs(u);
            else return 0;
        }
        private double Uniform_Kernel(double u)
        {
            if (Math.Abs(u) <= 1) return (double)1 / 2;
            else return 0;
        }
        private double Exp_Kernel(double u)
        {
            if (u >= 0) return Math.Exp(-u);
            else return 0;
        }
        #endregion kernels

        private double Density_Estimate(List<double> values, double x, Func<double, double> myMethodName) {
            sum = 0;
            
            for (int i = 0; i < values.Count(); i++) {
                sum += myMethodName((x - values[i]) / hn);
            }

            double res = sum / values.Count() / hn;

            return res;
        }
        
        private bool Get_Data()
        {
            try
            {
                mn = DataContext as MainViewModel;
                value = Convert.ToInt32(val.Text);
                paramA = Convert.ToInt32(mean.Text);
                paramB = Convert.ToInt32(dispersion.Text);
                numOfIntervals = Convert.ToInt32(num_of_intervals.Text);
                frequency = new int[numOfIntervals];
                h.Text.Replace('.', ',');
                hn = Convert.ToDouble(h.Text);
            }
            catch (Exception) {
                MessageBox.Show("Пожалуйста, проверьте корректность введенных данных");
                return false;
            }
            return true;
        }

        private void Button_Click_Upd(object sender, RoutedEventArgs e) {
            #region charts
            Get_Data();
            _plotView.Model.Series.Clear();
            lineSeries.Points.Clear();
            lineSeries_rp.Points.Clear();
            rectangleBarSeries.Items.Clear();
            min = x_val.Min();
            max = x_val.Max();
            incr = (double)(Math.Abs(min) + Math.Abs(max)) / numOfIntervals;

            double xx;
            lineSeries_rp.Points.Add(new DataPoint(min, Density_Estimate(x_val, min, myMethodName)));
            #endregion charts

            if (distribution_type.SelectedIndex == 0)
            {
                x_val.Sort();
                for (int i = 0; i < numOfIntervals; i++)
                {
                    foreach (var x in x_val)
                    {
                        sum = 0;
                        if (x >= min + incr * i && x < min + incr * (i + 1))
                        {
                            frequency[i]++;
                        }
                        else if (x >= min + incr * (i + 1))
                        {
                            break;
                        }
                    }
                    lineSeries_rp.Points.Add(new DataPoint(((min + incr * i) + (min + incr * (i + 1))) / 2, Density_Estimate(x_val, ((min + incr * i) + (min + incr * (i + 1))) / 2, myMethodName)));

                    xx = ((min + incr * i) + (min + incr * (i + 1))) / 2;
                    if (i == 0) lineSeries.Points.Add(new DataPoint(min + incr * i, paramA * Math.Exp(-paramA * 0)));
                    else lineSeries.Points.Add(new DataPoint(min + incr * (i + 1), paramA * Math.Exp(-paramA * xx)));
                    rectangleBarSeries.Items.Add(new RectangleBarItem(min + incr * i, 0, min + incr * (i + 1), (double)frequency[i] / value / incr));
                }
                lineSeries_rp.Points.Add(new DataPoint(max, Density_Estimate(x_val, max, myMethodName)));

                mn.Model.Series.Add(rectangleBarSeries);
                mn.Model.Series.Add(lineSeries);
                mn.Model.Series.Add(lineSeries_rp);
                _plotView.InvalidatePlot(true);

            } // exp

            else if (distribution_type.SelectedIndex == 1)
            {
                int disp_y = paramB * value; //дисперсия случ. величины при выборке больше 100 - в N раз больше. Иначе - в (N-1)^2 / N

                x_val.Sort();

                double y_ = (1 / (paramB * Math.Sqrt(2 * Math.PI))) * Math.Exp((-1 * ((Math.Pow(min - paramA, 2)) / (2 * Math.Pow(paramB, 2)))));
                lineSeries.Points.Add(new DataPoint(min, y_));
                lineSeries_rp.Points.Add(new DataPoint(min, Density_Estimate(x_val, min, myMethodName)));

                for (int i = 0; i < numOfIntervals; i++)
                {
                    foreach (var x in x_val)
                    {
                        if (x >= min + incr * i && x < min + incr * (i + 1))
                        {
                            frequency[i]++;
                        }
                        else if (x >= min + incr * (i + 1))
                        {
                            break;
                        }
                    }
                    xx = ((min + incr * i) + (min + incr * (i + 1))) / 2;
                    double y = (1 / (paramB * Math.Sqrt(2 * Math.PI))) * Math.Exp((-1 * ((Math.Pow(xx - paramA, 2)) / (2 * Math.Pow(paramB, 2)))));
                    lineSeries_rp.Points.Add(new DataPoint(((min + incr * i) + (min + incr * (i + 1))) / 2, Density_Estimate(x_val, ((min + incr * i) + (min + incr * (i + 1))) / 2, myMethodName)));


                    lineSeries.Points.Add(new DataPoint(xx, y));
                    rectangleBarSeries.Items.Add(new RectangleBarItem(min + incr * i, 0, min + incr * (i + 1), (double)frequency[i] / value / incr));
                }
                lineSeries_rp.Points.Add(new DataPoint(max, Density_Estimate(x_val, max, myMethodName)));
                double x_ = min + incr * numOfIntervals;
                y_ = (1 / (paramB * Math.Sqrt(2 * Math.PI))) * Math.Exp((-1 * ((Math.Pow(x_ - paramA, 2)) / (2 * Math.Pow(paramB, 2)))));
                lineSeries.Points.Add(new DataPoint(x_, y_));
                mn.Model.Series.Add(rectangleBarSeries);
                mn.Model.Series.Add(lineSeries);
                mn.Model.Series.Add(lineSeries_rp);
                _plotView.InvalidatePlot(true);
            } // normal

            else {
                x_val.Sort();

                if (paramB - paramA != 0) lineSeries.Points.Add(new DataPoint(min, 1 / (max - min)));

                lineSeries_rp.Points.Add(new DataPoint(min, Density_Estimate(x_val, min, myMethodName)));

                for (int i = 0; i < numOfIntervals; i++)
                {
                    foreach (var x in x_val)
                    {
                        if (x >= min + incr * i && x < min + incr * (i + 1))
                        {
                            frequency[i]++;
                        }
                        else if (x >= min + incr * (i + 1))
                        {
                            break;
                        }
                    }
                    rectangleBarSeries.Items.Add(new RectangleBarItem(min + incr * i, 0, min + incr * (i + 1), (double)frequency[i] / value / incr));
                    lineSeries_rp.Points.Add(new DataPoint(((min + incr * i) + (min + incr * (i + 1))) / 2, Density_Estimate(x_val, ((min + incr * i) + (min + incr * (i + 1))) / 2, myMethodName)));
                }
                lineSeries_rp.Points.Add(new DataPoint(max, Density_Estimate(x_val, max, myMethodName)));
                if (paramB - paramA != 0)
                {
                    lineSeries.Points.Add(new DataPoint(min + incr * numOfIntervals, 1 / (max - min)));
                    mn.Model.Series.Add(rectangleBarSeries);
                    mn.Model.Series.Add(lineSeries);
                    mn.Model.Series.Add(lineSeries_rp);
                    _plotView.InvalidatePlot(true);
                }
            } // uniform
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Get_Data())
            {

                #region charts

                _plotView.Model.Series.Clear();
                _plotView.Background = Brushes.AliceBlue;
                lineSeries.Points.Clear();
                lineSeries_rp.Points.Clear();
                rectangleBarSeries.Items.Clear();



                lineSeries.Color = OxyColors.Blue;
                lineSeries.InterpolationAlgorithm = InterpolationAlgorithms.CanonicalSpline;

                lineSeries_rp.Color = OxyColors.OrangeRed;
                lineSeries_rp.InterpolationAlgorithm = InterpolationAlgorithms.CanonicalSpline;

                rectangleBarSeries.FillColor = OxyColor.FromRgb(150, 200, 230);
                rectangleBarSeries.StrokeColor = OxyColor.FromRgb(150, 100, 200);
                #endregion charts

                expectationEstimate = 0;

                if (distribution_type.SelectedIndex == 0)
                {
                    x_val = new List<double>(new double[value]);
                    for (var i = 0; i < value; i++)
                    {
                        x_val[i] = (double)-1 / paramA * Math.Log(rand.NextDouble());
                    }
                    min = x_val.Min();
                    max = x_val.Max();
                    incr = (double)(Math.Abs(min) + Math.Abs(max)) / numOfIntervals;
                    double xMean;

                    x_val.Sort();
                    lineSeries_rp.Points.Add(new DataPoint(min, Density_Estimate(x_val, min, myMethodName)));
                    for (int i = 0; i < numOfIntervals; i++)
                    {
                        foreach (var x in x_val)
                        {
                            if (x >= min + incr * i && x < min + incr * (i + 1))
                            {
                                frequency[i]++;
                            }
                            else if (x >= min + incr * (i + 1))
                            {
                                break;
                            }
                        }
                        lineSeries_rp.Points.Add(new DataPoint(((min + incr * i) + (min + incr * (i + 1))) / 2, Density_Estimate(x_val, ((min + incr * i) + (min + incr * (i + 1))) / 2, myMethodName)));

                        xMean = ((min + incr * i) + (min + incr * (i + 1))) / 2;

                        sumExpectationEstimate += xMean * ((double)frequency[i] / value); // мат ожидание без /n

                        if (i == 0) lineSeries.Points.Add(new DataPoint(min + incr * i, paramA * Math.Exp(-paramA * 0)));
                        else lineSeries.Points.Add(new DataPoint(min + incr * (i + 1), paramA * Math.Exp(-paramA * xMean)));
                        rectangleBarSeries.Items.Add(new RectangleBarItem(min + incr * i, 0, min + incr * (i + 1), (double)frequency[i] / value / incr));
                    }
                    lineSeries_rp.Points.Add(new DataPoint(max, Density_Estimate(x_val, max, myMethodName)));

                    expectationEstimate = sumExpectationEstimate / value;
                    mathMean.Content = "Выборочное среднее: " + Math.Round(expectationEstimate, 4);
                    median.Content = "Медиана: " + Math.Round(Statistics.Median(x_val));
                    varianceEstimate = Math.Round(Statistics.PopulationVariance(x_val), 4);
                    varianceValue.Content = "Выборочная дисперсия: " + varianceEstimate;
                    deviation.Content = "Выборочное ср. отклонение: " + Math.Round(Math.Sqrt(varianceEstimate), 4);

                    mn.Model.Series.Clear();
                    mn.Model.Series.Add(rectangleBarSeries);
                    mn.Model.Series.Add(lineSeries);
                    mn.Model.Series.Add(lineSeries_rp);
                    _plotView.InvalidatePlot(true);

                } // exp

                else if (distribution_type.SelectedIndex == 1)
                {
                    x_val = new List<double>(new double[value]);
                    modeEstimate = new List<double>();
                    int disp_y = paramB * value; //дисперсия случ. величины при выборке больше 100 - в N раз больше. Иначе - в (N-1)^2 / N
                    maxModeEstimate = 0;

                    for (var i = 0; i < value; i++)
                    {
                        x_val[i] = Math.Sqrt(-2 * Math.Log(rand.NextDouble())) * Math.Cos(2 * Math.PI * rand.NextDouble()) * paramB + paramA;
                    }
                    min = x_val.Min();
                    max = x_val.Max();
                    incr = (double)(Math.Abs(min) + Math.Abs(max)) / numOfIntervals;
                    x_val.Sort();

                    double y_ = (1 / (paramB * Math.Sqrt(2 * Math.PI))) * Math.Exp((-1 * ((Math.Pow(min - paramA, 2)) / (2 * Math.Pow(paramB, 2)))));
                    lineSeries.Points.Add(new DataPoint(min, y_));
                    lineSeries_rp.Points.Add(new DataPoint(min, Density_Estimate(x_val, min, myMethodName)));

                    for (int i = 0; i < numOfIntervals; i++)
                    {

                        foreach (var x in x_val)
                        {
                            if (x >= min + incr * i && x < min + incr * (i + 1))
                            {
                                frequency[i]++;
                            }
                            else if (x >= min + incr * (i + 1))
                            {
                                break;
                            }
                        }
                        double xMean = ((min + incr * i) + (min + incr * (i + 1))) / 2;

                        lineSeries_rp.Points.Add(new DataPoint(xMean, Density_Estimate(x_val, xMean, myMethodName)));
                        double o = Math.Round((Density_Estimate(x_val, xMean + INCRIMENT, myMethodName) - Density_Estimate(x_val, xMean, myMethodName)) / INCRIMENT, 4);

                        double derivative = Math.Round((Density_Estimate(x_val, xMean + INCRIMENT, myMethodName) - Density_Estimate(x_val, xMean, myMethodName)) / INCRIMENT, 3);

                        if (derivative < 0 && tmp > 0) {
                            modeEstimate.Add(Math.Round(xMean, 3));
                        }

                        tmp = Math.Round((Density_Estimate(x_val, xMean + INCRIMENT, myMethodName) - Density_Estimate(x_val, xMean, myMethodName)) / INCRIMENT, 3);

                        sumExpectationEstimate += xMean * ((double)frequency[i] / value); // мат ожидание без /n


                        double y = (1 / (paramB * Math.Sqrt(2 * Math.PI))) * Math.Exp((-1 * ((Math.Pow(xMean - paramA, 2)) / (2 * Math.Pow(paramB, 2)))));



                        lineSeries.Points.Add(new DataPoint(xMean, y));
                        rectangleBarSeries.Items.Add(new RectangleBarItem(min + incr * i, 0, min + incr * (i + 1), (double)frequency[i] / value / incr));
                    }
                    lineSeries_rp.Points.Add(new DataPoint(max, Density_Estimate(x_val, max, myMethodName)));
                    double x_ = min + incr * numOfIntervals;
                    y_ = (1 / (paramB * Math.Sqrt(2 * Math.PI))) * Math.Exp((-1 * ((Math.Pow(x_ - paramA, 2)) / (2 * Math.Pow(paramB, 2)))));

                    expectationEstimate = sumExpectationEstimate / value;
                    mathMean.Content = "Выборочное среднее: " + Math.Round(expectationEstimate, 4);
                    median.Content = "Медиана: " + Math.Round(Statistics.Median(x_val), 4);
                    varianceEstimate = Math.Round(Statistics.PopulationVariance(x_val), 4);
                    varianceValue.Content = "Выборочная дисперсия: " + varianceEstimate;
                    mode.Content = "Мода: ";
                    foreach (var x in modeEstimate) {
                        mode.Content += x + " ";
                    }
                    deviation.Content = "Выборочное ср. отклонение: " + Math.Round(Math.Sqrt(varianceEstimate), 4);

                    lineSeries.Points.Add(new DataPoint(x_, y_));
                    mn.Model.Series.Clear();
                    mn.Model.PlotType = PlotType.XY;
                    mn.Model.Series.Add(rectangleBarSeries);
                    mn.Model.Series.Add(lineSeries);
                    mn.Model.Series.Add(lineSeries_rp);
                    _plotView.InvalidatePlot(true);
                } // normal

                else
                {
                    sum = 0;
                    x_val = new List<double>(new double[value]);


                    _plotView.Model.Series.Clear();

                    for (int i = 0; i < value; i++)
                    {
                        x_val[i] = (paramB - paramA) * rand.NextDouble() + paramA;
                    }
                    min = x_val.Min();
                    max = x_val.Max();
                    incr = (double)(Math.Abs(min) + Math.Abs(max)) / numOfIntervals;

                    x_val.Sort();

                    if (paramB - paramA != 0) lineSeries.Points.Add(new DataPoint(min, 1 / (max - min)));

                    lineSeries_rp.Points.Add(new DataPoint(min, Density_Estimate(x_val, min, myMethodName)));

                    for (int i = 0; i < numOfIntervals; i++)
                    {
                        foreach (var x in x_val)
                        {
                            sum = 0;
                            if (x >= min + incr * i && x < min + incr * (i + 1))
                            {
                                frequency[i]++;
                            }
                            else if (x >= min + incr * (i + 1))
                            {
                                break;
                            }
                        }
                        double xMean = ((min + incr * i) + (min + incr * (i + 1))) / 2;
                        sumExpectationEstimate += xMean * ((double)frequency[i] / value); // мат ожидание без /n
                        lineSeries_rp.Points.Add(new DataPoint(xMean, Density_Estimate(x_val, xMean, myMethodName)));
                        rectangleBarSeries.Items.Add(new RectangleBarItem(min + incr * i, 0, min + incr * (i + 1), (double)frequency[i] / value / incr));
                    }
                    sum = 0;
                    lineSeries_rp.Points.Add(new DataPoint(min + incr * numOfIntervals, Density_Estimate(x_val, min + incr * numOfIntervals, myMethodName)));

                    expectationEstimate = sumExpectationEstimate / value;
                    mathMean.Content = "Выборочное среднее: " + Math.Round(expectationEstimate, 4);
                    median.Content = "Медиана: " + Math.Round(Statistics.Median(x_val), 4);
                    varianceEstimate = Math.Round(Statistics.PopulationVariance(x_val), 4);
                    varianceValue.Content = "Выборочная дисперсия: " + varianceEstimate;
                    deviation.Content = "Выборочное ср. отклонение: " + Math.Round(Math.Sqrt(varianceEstimate), 4);

                    if (paramB - paramA != 0)
                    {
                        lineSeries.Points.Add(new DataPoint(min + incr * numOfIntervals, 1 / (max - min)));
                        mn.Model.Series.Add(rectangleBarSeries);
                        mn.Model.Series.Add(lineSeries);
                        mn.Model.Series.Add(lineSeries_rp);
                        _plotView.InvalidatePlot(true);
                    }
                } // uniform
            }
        }

    }
}
