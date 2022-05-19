using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Series;

namespace TV_laba2WPF
{
    public class MainViewModel
    {
        public MainViewModel()
        {
            chart = new Chart();
            chart.Draw();
            
        }

        public Chart chart;
        public RectangleBarSeries rectangleBarSeries { get { return this.rectangleBarSeries; } set { } }

        public class Chart
        {
            public PlotModel Model { get; set; }

            public void Draw()
            {
                RectangleBarSeries rectangleBarSeries = new RectangleBarSeries();
                Model = new PlotModel();
                Model.PlotType = PlotType.XY;
                Model.TextColor = OxyColor.FromRgb(100,100,100);
                Model.PlotAreaBorderColor = OxyColor.FromRgb(100, 100, 100);
                Model.Series.Add(rectangleBarSeries);
                Model.Background = OxyColors.AliceBlue;
            }
        }

        public PlotModel Model
        {
            get { return chart.Model; }
        }

    }
}
