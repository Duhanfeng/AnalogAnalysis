using Sparrow.Chart;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace AnalogAnalysisWpf.Performance
{
    /// <summary>
    /// PerformanceView.xaml 的交互逻辑
    /// </summary>
    public partial class PerformanceView : UserControl
    {
        public PerformanceView()
        {
            InitializeComponent();

            //DataContext = new PerformanceView();
        }

        public PointsCollection PointsCollection { get; set; } = new PointsCollection();

        public void SetData(short[] data)
        {
            PointsCollection = new PointsCollection();

            for (int i = 0; i < data.Length; i++)
            {
                PointsCollection.Add(new DoublePoint() { Data = i, Value = data[i] });
            }

            ((LineSeries)(Chart.Series[0])).Points = PointsCollection;
        }
    }
}
