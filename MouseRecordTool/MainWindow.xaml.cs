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

            GraphTypeComboBox_SelectionChanged(GraphTypeComboBox, null);

            MaxFrequencyTextBlock.Text = "<=10";
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
        /// 电压(V)
        /// </summary>
        public double Voltage { get; set; } = -1;

        /// <summary>
        /// 时间(MS)
        /// </summary>
        public int Time { get; set; } = -1;

        /// <summary>
        /// 时间间隔(MS)
        /// </summary>
        public int TimeInterval { get; set; } = 50;

        /// <summary>
        /// 频率(Hz)
        /// </summary>
        public int Frequency { get; set; } = -1;

        /// <summary>
        /// 占空比
        /// </summary>
        public int DutyRatio { get; set; } = 50;

        /// <summary>
        /// 记录的电压值
        /// </summary>
        public Dictionary<int, double> RecordVoltages = new Dictionary<int, double>();

        /// <summary>
        /// 点集合
        /// </summary>
        private List<Point> pointList = new List<Point>();

        /// <summary>
        /// 基础点(表格中原点的位置)
        /// </summary>
        private Point BasePoint { get; set; } = new Point(47, 175.76);

        /// <summary>
        /// 图表类型
        /// </summary>
        public int GraphType { get; set; } = 0;

        private void Canvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (GraphType == 0)
            {
                //读取数据
                try
                {
                    Time = (int)(double.Parse(SampleTimeTextBox.Text) * 1000);
                    Voltage = int.Parse(MaxVoltageTextBox.Text) * 1000;
                    TimeInterval = int.Parse(SampleIntervalTextBox.Text);
                }
                catch (Exception)
                {

                    MessageBox.Show("数据异常");
                }

                if (SparrowChart != null)
                {
                    SparrowChart.XAxis.MaxValue = $"{Time / 1000.0}";
                    SparrowChart.YAxis.MaxValue = $"{Voltage / 1000.0}";
                    SparrowChart.UpdateLayout();
                    MainWindowModel.SetData(new Dictionary<int, double>());
                }

                Canvas canvas = sender as Canvas;

                //Voltages.Clear();
                pointList.Clear();
                canvas.Children.Clear();
                canvas.Children.Add(BackgroudChart);
                canvas.Children.Add(coordLabel);

                Point point = e.GetPosition(canvas);
                coordLabel.SetValue(Canvas.LeftProperty, point.X + 5);
                coordLabel.SetValue(Canvas.TopProperty, point.Y - 15);

                if ((Voltage <= 0) || (Time <= 0))
                {
                    MessageBox.Show("未选择有效的XY轴范围");
                    return;
                }

                //容量
                RecordVoltages.Clear();
                int capacity = (Time / TimeInterval);

                for (int i = 0; i < capacity; i++)
                {
                    RecordVoltages.Add(TimeInterval * i, 0);
                }
            }
            
        }

        private void Canvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (GraphType == 0)
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
            
        }

        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            if (GraphType == 0)
            {
                var points = pointList.Distinct().ToList();
                var validPoints = new List<Point>();

                if (points?.Count > 0)
                {
                    var xLength = Canvas.ActualWidth - BasePoint.X;
                    int pointCount = Time / TimeInterval;
                    double xInterval = xLength / pointCount;
                    double currentX = BasePoint.X;
                    int currentTime = 0;

                    RecordVoltages.Clear();
                    for (int i = 0; i < pointCount; i++)
                    {
                        RecordVoltages.Add(i * TimeInterval, 0);
                    }

                    for (int i = 1; i < points.Count; i++)
                    {
                        while (true)
                        {
                            if (currentX > points[i].X)
                            {
                                //到下一个点
                                break;
                            }
                            else if ((currentX > points[i - 1].X) && (currentX < points[i].X))
                            {
                                //计算线公式
                                double a = (points[i].Y - points[i - 1].Y) / (points[i].X - points[i - 1].X);
                                double b1 = points[i].Y - a * points[i].X;
                                double b2 = points[i - 1].Y - a * points[i - 1].X;

                                //计算当前点
                                double y = currentX * a + b1;
                                Point point = new Point(currentX, y);
                                validPoints.Add(point);

                                double yRatio = (BasePoint.Y - y) / BasePoint.Y;
                                RecordVoltages[currentTime] = Voltage * yRatio;

                                //递进
                                currentX += xInterval;
                                currentTime += TimeInterval;
                            }
                            else
                            {
                                //当前点小于有效点,递进
                                currentX += xInterval;
                                currentTime += TimeInterval;
                            }
                        }

                    }

                    MainWindowModel.SetPulseData(RecordVoltages);

                }

            }
            else
            {
                //数据校验
                try
                {
                    Time = (int)(double.Parse(SampleTimeTextBox.Text) * 1000);
                    Voltage = double.Parse(MaxVoltageTextBox.Text) * 1000;
                    TimeInterval = int.Parse(SampleIntervalTextBox.Text);
                    Frequency = int.Parse(FrequencyTextBox.Text);
                    DutyRatio = int.Parse(DutyRatioTextBox.Text);

                    if ((Time <= 0) || (Voltage <= 0) || (TimeInterval <= 0) || (Frequency <= 0))
                    {
                        MessageBox.Show("数据必须大于0!");
                        return;
                    }

                    double sampleFrequency = 1000.0 / TimeInterval;

                    if (sampleFrequency < (Frequency * 2))
                    {
                        MessageBox.Show($"采样频率({sampleFrequency.ToString("F2")})必须大于输出频率的2倍({Frequency.ToString("F2")} * 2)");
                        return;
                    }
                }
                catch (Exception)
                {

                    MessageBox.Show("数据异常");
                    return;
                }

                if (SparrowChart != null)
                {
                    SparrowChart.XAxis.MaxValue = $"{Time * 1.2 / 1000.0}";
                    SparrowChart.YAxis.MaxValue = $"{Voltage * 1.2 / 1000.0}";
                    SparrowChart.UpdateLayout();
                    MainWindowModel.SetData(new Dictionary<int, double>());
                }

                //Voltages.Clear();
                pointList.Clear();
                Canvas.Children.Clear();
                Canvas.Children.Add(BackgroudChart);

                //容量
                RecordVoltages.Clear();
                int capacity = (int)(Time / TimeInterval);

                for (int i = 0; i < capacity; i++)
                {
                    RecordVoltages.Add(TimeInterval * i, 0);
                }

                if (GraphType == 1)
                {
                    //(周期,单位MS)
                    double cycleTime = 1 * 1000.0 / Frequency;
                    double positiveTime = cycleTime * (DutyRatio * 0.01);
                    for (int i = 0; i < RecordVoltages.Count; i++)
                    {
                        //当前在周期中的时间
                        double currentTime = (TimeInterval * i) % cycleTime;

                        //判断是高电平还是低电平
                        if (currentTime < positiveTime)
                        {
                            RecordVoltages[TimeInterval * i] = Voltage;
                        }
                        else
                        {
                            RecordVoltages[TimeInterval * i] = 0;
                        }
                    }

                }

                MainWindowModel.SetPulseData(RecordVoltages);
            }

        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            //数据校验
            try
            {
                Time = (int)(double.Parse(SampleTimeTextBox.Text) * 1000);
                Voltage = double.Parse(MaxVoltageTextBox.Text) * 1000;
                TimeInterval = int.Parse(SampleIntervalTextBox.Text);
                Frequency = int.Parse(FrequencyTextBox.Text);
                DutyRatio = int.Parse(DutyRatioTextBox.Text);

                if ((Time <= 0) || (Voltage <= 0) || (TimeInterval <= 0) || (Frequency <= 0))
                {
                    MessageBox.Show("数据必须大于0!");
                    return;
                }
            }
            catch (Exception)
            {

                MessageBox.Show("数据异常");
                return;
            }

            // 创建一个保存文件式的对话框
            var sfd = new Microsoft.Win32.SaveFileDialog();

            //设置保存的文件的类型，注意过滤器的语法  
            sfd.Filter = "json file|*.json";
            sfd.FileName = NameTextBox.Text + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss")
;

            //调用ShowDialog()方法显示该对话框，该方法的返回值代表用户是否点击了确定按钮  
            if (sfd.ShowDialog() == true)
            {
                //此处做你想做的事 ...=sfd.FileName; 
                ExportData exportData = new ExportData
                {
                    DutyRatio = DutyRatio,
                    MaxVoltage = Voltage,
                    MaxTime = Time,
                    TimeInterval = TimeInterval,
                    GraphType = GraphType,
                    Frequency = Frequency,
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

        /// <summary>
        /// 图表类型
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GraphTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                int index = (sender as ComboBox).SelectedIndex;

                if (index == 0)
                {
                    CheckButton.IsEnabled = true;
                    SinewaveStackPanel.IsEnabled = false;
                }
                else if ((index == 1) || (index == 2))
                {
                    CheckButton.IsEnabled = true;
                    SinewaveStackPanel.IsEnabled = true;
                }
                else
                {
                    CheckButton.IsEnabled = false;
                    SinewaveStackPanel.IsEnabled = false;
                }
                GraphType = index;

                //清空图表
                try
                {
                    Time = int.Parse(SampleTimeTextBox.Text) * 1000;
                    Voltage = int.Parse(MaxVoltageTextBox.Text) * 1000;
                    TimeInterval = int.Parse(SampleIntervalTextBox.Text);
                }
                catch (Exception)
                {

                    MessageBox.Show("数据异常");
                }

                if (SparrowChart != null)
                {
                    SparrowChart.XAxis.MaxValue = $"{Time / 1000.0}";
                    SparrowChart.YAxis.MaxValue = $"{Voltage / 1000.0}";
                    SparrowChart.UpdateLayout();
                    MainWindowModel.SetData(new Dictionary<int, double>());
                }

                //Voltages.Clear();
                pointList.Clear();
                Canvas.Children.Clear();
                Canvas.Children.Add(BackgroudChart);
                //Canvas.Children.Add(coordLabel);

                if ((Voltage <= 0) || (Time <= 0))
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
            
        }

        /// <summary>
        /// 产生波形
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {

            
        }

        private void SampleIntervalTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            //数据校验
            try
            {
                if (e.Key == Key.Enter)
                {
                    TimeInterval = int.Parse(SampleIntervalTextBox.Text);
                    SampleFrequencyTextBox.Text = (1000.0 / TimeInterval).ToString("F2");
                    MaxFrequencyTextBlock.Text = "<=" + (1000.0 / TimeInterval / 2).ToString("F2");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("数据异常");
                return;
            }
            
        }

        private void SampleIntervalTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            //数据校验
            try
            {
                TimeInterval = int.Parse(SampleIntervalTextBox.Text);
                SampleFrequencyTextBox.Text = (1000.0 / TimeInterval).ToString("F2");
                MaxFrequencyTextBlock.Text = "<=" + (1000.0 / TimeInterval / 2).ToString("F2");
            }
            catch (Exception)
            {
                MessageBox.Show("数据异常");
                return;
            }
        }

        private void SparrowChart_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                SparrowChart.Width = SparrowChart.ActualWidth + 50;
            }
            else
            {
                if (SparrowChartScrollViewer.ActualWidth < (SparrowChart.ActualWidth))
                {
                    double width = SparrowChart.ActualWidth - 50;
                    if (width > SparrowChartScrollViewer.ActualWidth - 20)
                    {
                        SparrowChart.Width = width;
                    }
                }
                
            }
                        
        }
    }


    /// <summary>
    /// 导出数据
    /// </summary>
    public class ExportData
    {
        
        /// <summary>
        /// 最大电压
        /// </summary>
        public double MaxVoltage { get; set; }

        /// <summary>
        /// 最大时间(MS)
        /// </summary>
        public int MaxTime { get; set; }

        /// <summary>
        /// 时间间隔(MS)
        /// </summary>
        public int TimeInterval { get; set; } = 50;

        /// <summary>
        /// 图表类型
        /// 0-自定义
        /// 1-脉冲
        /// 2-正弦波
        /// </summary>
        public int GraphType { get; set; }

        /// <summary>
        /// 数据列表
        /// </summary>
        public Dictionary<int, double> RecordVoltages = new Dictionary<int, double>();

        /// <summary>
        /// 频率(Hz)
        /// </summary>
        public int Frequency { get; set; }

        /// <summary>
        /// 占空比
        /// </summary>
        public int DutyRatio { get; set; }
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
                if (item.Value >= 0)
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

        
        public void SetPulseData(Dictionary<int, double> data)
        {
            double lastValue = 0;

            var scope = new ObservableCollection<Data>();

            foreach (var item in data)
            {
                if (item.Value >= 0)
                {
                    if (lastValue != item.Value)
                    {
                        scope.Add(new Data
                        {
                            Value = item.Key / 1000.0,
                            Value1 = lastValue / 1000.0
                        });
                    }
                    scope.Add(new Data
                    {
                        Value = item.Key / 1000.0,
                        Value1 = item.Value / 1000.0
                    });

                    lastValue = item.Value;
                }
            }

            ScopeCHACollection = new ObservableCollection<Data>(scope);

        }
    }
}
