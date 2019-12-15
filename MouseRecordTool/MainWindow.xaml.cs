using Caliburn.Micro;
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
using Framework.Infrastructure.Serialization;

namespace MouseRecordTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindowModel MainWindowModel { get; set; } = new MainWindowModel();

        public MainWindow()
        {
            InitializeComponent();

            DataContext = MainWindowModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BackgroudChart.SetValue(Canvas.LeftProperty, 0.0);
            BackgroudChart.SetValue(Canvas.TopProperty, 0.0);
            BackgroudChart.Width = Canvas.ActualWidth;
            BackgroudChart.Height = Canvas.ActualHeight;

            var data = new Dictionary<int, double>();
            data.Add(0, 0);
            MainWindowModel.SetData(data);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Canvas.Children.Clear();
            Canvas.Children.Add(BackgroudChart);
            Canvas.Children.Add(coordLabel);

            coordLabel.Text = "";

            BackgroudChart.SetValue(Canvas.LeftProperty, 0.0);
            BackgroudChart.SetValue(Canvas.TopProperty, 0.0);
            BackgroudChart.Width = Canvas.ActualWidth;
            BackgroudChart.Height = Canvas.ActualHeight;
        }

        /// <summary>
        /// 电压
        /// </summary>
        public double Voltage { get; set; } = -1;

        /// <summary>
        /// 时间
        /// </summary>
        public double Time { get; set; } = -1;

        /// <summary>
        /// 时间间隔(MS)
        /// </summary>
        public int TimeInterval { get; set; } = 50;

        //X轴改变
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            switch (comboBox.SelectedIndex)
            {
                case 0:
                    Time = 2000;
                    break;
                case 1:
                    Time = 5000;
                    break;
                case 2:
                    Time = 10000;
                    break;
                case 3:
                    Time = 20000;
                    break;
                default:
                    return;
            }
            
            if (Canvas != null)
            {
                Canvas.Children.Clear();
                Canvas.Children.Add(BackgroudChart);
                Canvas.Children.Add(coordLabel);

                coordLabel.Text = "";

                BackgroudChart.SetValue(Canvas.LeftProperty, 0.0);
                BackgroudChart.SetValue(Canvas.TopProperty, 0.0);
                BackgroudChart.Width = Canvas.ActualWidth;
                BackgroudChart.Height = Canvas.ActualHeight;
            }

            if (SparrowChart != null)
            {
                BackgroudChart.XAxis.MaxValue = $"{Time / 1000.0}";
                BackgroudChart.UpdateLayout();
                BackgroudChart.RefreshLegend();

                SparrowChart.XAxis.MaxValue = $"{Time / 1000.0}";
                SparrowChart.UpdateLayout();
                MainWindowModel.SetData(new Dictionary<int, double>());
            }

        }

        //Y轴改变
        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            switch (comboBox.SelectedIndex)
            {
                case 0:
                    Voltage = 5000;
                    break;
                case 1:
                    Voltage = 12000;
                    break;
                case 2:
                    Voltage = 24000;
                    break;
                default:
                    return;
            }

            if (Canvas != null)
            {
                Canvas.Children.Clear();
                Canvas.Children.Add(BackgroudChart);
                Canvas.Children.Add(coordLabel);

                coordLabel.Text = "";

                BackgroudChart.SetValue(Canvas.LeftProperty, 0.0);
                BackgroudChart.SetValue(Canvas.TopProperty, 0.0);
                BackgroudChart.Width = Canvas.ActualWidth;
                BackgroudChart.Height = Canvas.ActualHeight;
            }

            if (SparrowChart != null)
            {
                SparrowChart.YAxis.MaxValue = $"{Voltage / 1000.0}";
                SparrowChart.UpdateLayout();
                MainWindowModel.SetData(new Dictionary<int, double>());
            }

        }

        /// <summary>
        /// 选择采样间隔
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_SelectionChanged_2(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            switch (comboBox.SelectedIndex)
            {
                case 0:
                    TimeInterval = 10;
                    break;
                case 1:
                    TimeInterval = 50;
                    break;
                case 2:
                    TimeInterval = 100;
                    break;
                default:
                    break;
            }

            if (Canvas != null)
            {
                Canvas.Children.Clear();
                Canvas.Children.Add(BackgroudChart);
                Canvas.Children.Add(coordLabel);

                coordLabel.Text = "";

                BackgroudChart.SetValue(Canvas.LeftProperty, 0.0);
                BackgroudChart.SetValue(Canvas.TopProperty, 0.0);
                BackgroudChart.Width = Canvas.ActualWidth;
                BackgroudChart.Height = Canvas.ActualHeight;
            }
            
        }

        //public class VoltagePoint
        //{
        //    /// <summary>
        //    /// 电压
        //    /// </summary>
        //    public double Voltage { get; set; }

        //    /// <summary>
        //    /// 时间
        //    /// </summary>
        //    public double Time { get; set; }
        //}

        //public List<VoltagePoint> Voltages { get; set; } = new List<VoltagePoint>();

        /// <summary>
        /// 记录的电压值
        /// </summary>
        public Dictionary<int, double> RecordVoltages = new Dictionary<int, double>();

        /// <summary>
        /// 点集合
        /// </summary>
        private List<Point> pointList = new List<Point>();

        private Point BasePoint { get; set; } = new Point(47, 175.76);

        private void Canvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Canvas canvas = sender as Canvas;

            //Voltages.Clear();
            pointList.Clear();
            canvas.Children.Clear();
            canvas.Children.Add(BackgroudChart);
            canvas.Children.Add(coordLabel);

            Point point = e.GetPosition(canvas);
            coordLabel.SetValue(Canvas.LeftProperty, point.X + 5);
            coordLabel.SetValue(Canvas.TopProperty, point.Y - 15);

            if ((Voltage == -1) || (Time == -1))
            {
                MessageBox.Show("未选择有效的XY轴范围");
                return;
            }

            //容量
            RecordVoltages.Clear();
            int capacity = (int)(Time / TimeInterval);

            for (int i = 0; i < capacity; i++)
            {
                RecordVoltages.Add(TimeInterval * i, 0);
            }
        }

        private void Canvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Canvas canvas = sender as Canvas;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // 返回指针相对于Canvas的位置
                Point point = e.GetPosition(canvas);

                //获取控件的大小
                double xRatio = (point.X - BasePoint.X) / (canvas.ActualWidth - BasePoint.X);
                double yRatio = (BasePoint.Y - point.Y) / BasePoint.Y;

                if ((xRatio < 0) || (xRatio > 1) || (yRatio < 0) || (yRatio > 1))
                {
                    return;
                }

                //var voltagePoint = new VoltagePoint();

                //voltagePoint.Voltage = Voltage * yRatio;
                //voltagePoint.Time = Time * xRatio;
                //Voltages.Add(voltagePoint);

                pointList.Add(point);
                coordLabel.SetValue(Canvas.LeftProperty, point.X + 5);
                coordLabel.SetValue(Canvas.TopProperty, point.Y - 15);
                coordLabel.Text = $"{Time * xRatio / 1000.0:F2}S,{Voltage * yRatio / 1000.0:F2}V";

                //记录点
                RecordVoltages[(int)((Time * xRatio) / TimeInterval + 0.5) * TimeInterval] = Voltage * yRatio;

                // 去重复点
                var disList = pointList.Distinct().ToList();
                var count = disList.Count(); // 总点数

                //if (point != this.startPoint && this.startPoint != null)
                {
                    var l = new Line();
                    l.Stroke = Brushes.Red;
                    l.StrokeThickness = 1;

                    if (count < 2)
                        return;
                    l.X1 = disList[count - 2].X;  // count-2  保证 line的起始点为点集合中的倒数第二个点。
                    l.Y1 = disList[count - 2].Y;
                    // 终点X,Y 为当前point的X,Y
                    l.X2 = point.X;
                    l.Y2 = point.Y;
                    canvas.Children.Add(l);
                }

            }

        }

        private void Canvas_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Canvas canvas = sender as Canvas;
        }

        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            //ScopeCHACollection.Clear();
            MainWindowModel.SetData(RecordVoltages);

        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            // 创建一个保存文件式的对话框
            var sfd = new Microsoft.Win32.SaveFileDialog();

            //设置保存的文件的类型，注意过滤器的语法  
            sfd.Filter = "json file|*.json";
            sfd.FileName = "";

            //调用ShowDialog()方法显示该对话框，该方法的返回值代表用户是否点击了确定按钮  
            if (sfd.ShowDialog() == true)
            {
                //此处做你想做的事 ...=sfd.FileName; 
                ExportData exportData = new ExportData
                {
                    MaxVoltage = Voltage,
                    MaxTime = Time,
                    TimeInterval = TimeInterval,
                    RecordVoltages = new Dictionary<int, double>(RecordVoltages),
                };

                if (JsonSerialization.SerializeObjectToFile(exportData, sfd.FileName))
                {
                    MessageBox.Show("导出成功");
                }
                else
                {
                    MessageBox.Show("导出失败");
                }
            }

        }

    }


    /// <summary>
    /// 导出数据
    /// </summary>
    public class ExportData
    {
        //数据列表
        public Dictionary<int, double> RecordVoltages = new Dictionary<int, double>();

        /// <summary>
        /// 最大电压
        /// </summary>
        public double MaxVoltage { get; set; }

        /// <summary>
        /// 最大时间
        /// </summary>
        public double MaxTime { get; set; }

        /// <summary>
        /// 时间间隔(MS)
        /// </summary>
        public int TimeInterval { get; set; } = 50;
    }

    public class MainWindowModel : Screen
    {
        #region 折线图


        private ObservableCollection<Data> scopeChACollection = new ObservableCollection<Data>();

        /// <summary>
        /// 通道A数据
        /// </summary>
        public ObservableCollection<Data> ScopeCHACollection
        {
            get
            {
                return scopeChACollection;
            }
            set
            {
                scopeChACollection = value;
                NotifyOfPropertyChange(() => ScopeCHACollection);
            }
        }

        private ObservableCollection<Data> scopeChAEdgeCollection = new ObservableCollection<Data>();

        /// <summary>
        /// 通道A边沿数据
        /// </summary>
        public ObservableCollection<Data> ScopeCHAEdgeCollection
        {
            get
            {
                return scopeChAEdgeCollection;
            }
            set
            {
                scopeChAEdgeCollection = value;
                NotifyOfPropertyChange(() => ScopeCHAEdgeCollection);
            }
        }


        private ObservableCollection<Data> scopeChBCollection = new ObservableCollection<Data>();

        /// <summary>
        /// 通道A数据
        /// </summary>
        public ObservableCollection<Data> ScopeCHBCollection
        {
            get
            {
                return scopeChBCollection;
            }
            set
            {
                scopeChBCollection = value;
                NotifyOfPropertyChange(() => ScopeCHBCollection);
            }
        }

        private ObservableCollection<Data> scopeChBEdgeCollection = new ObservableCollection<Data>();

        /// <summary>
        /// 通道A边沿数据
        /// </summary>
        public ObservableCollection<Data> ScopeCHBEdgeCollection
        {
            get
            {
                return scopeChBEdgeCollection;
            }
            set
            {
                scopeChBEdgeCollection = value;
                NotifyOfPropertyChange(() => ScopeCHBEdgeCollection);
            }
        }

        private string maxTime;

        public string MaxTime
        {
            get { return maxTime; }
            set { maxTime = value; NotifyOfPropertyChange(() => MaxTime); }
        }

        private string maxVoltage;

        public string MaxVoltage
        {
            get { return maxVoltage; }
            set { maxVoltage = value; NotifyOfPropertyChange(() => MaxVoltage); }
        }

        #endregion

        public void SetData(Dictionary<int, double> data)
        {
            var scope = new ObservableCollection<Data>();

            foreach (var item in data)
            {
                if (item.Value != 0)
                {
                    scope.Add(new Data
                    {
                        Value = item.Key / 1000.0,
                        Value1 = item.Value / 1000.0
                    });
                }
            }

            ScopeCHACollection = new ObservableCollection<Data>(scope);
            ScopeCHAEdgeCollection = new ObservableCollection<Data>(scope);

            ScopeCHBCollection = new ObservableCollection<Data>();
            ScopeCHBEdgeCollection = new ObservableCollection<Data>();
        }
    }
}
