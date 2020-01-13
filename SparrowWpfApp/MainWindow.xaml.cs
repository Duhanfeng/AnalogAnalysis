using Caliburn.Micro;
using Sparrow.Chart;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace SparrowWpfApp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainWindowModel();
        }
    }

    /// <summary>
    /// 示波器数据读取完成事件
    /// </summary>
    public class ScopeReadDataCompletedEventArgs : EventArgs
    {
        private double[] _globalChannel1 = new double[0];
        private double[] _globalChannel2 = new double[0];

        private double[] _currentCHannel1 = new double[0];
        private double[] _currentCHannel2 = new double[0];

        /// <summary>
        /// 总共的数据包
        /// </summary>
        public int TotalPacket { get; }

        /// <summary>
        /// 当前的数据包
        /// </summary>
        public int CurrentPacket { get; }

        /// <summary>
        /// 创建ScopeReadDataCompletedEventArgs新实例
        /// </summary>
        /// <param name="ch1"></param>
        /// <param name="ch2"></param>
        public ScopeReadDataCompletedEventArgs(double[] globalChannel1, double[] globalChannel2)
        {
            _globalChannel1 = globalChannel1;
            _globalChannel2 = globalChannel2;
        }

        public ScopeReadDataCompletedEventArgs(double[] globalChannel1, double[] globalChannel2, double[] currentChannel1, double[] currentChannel2, int totalPacket, int currentPacket)
        {
            _globalChannel1 = globalChannel1;
            _globalChannel2 = globalChannel2;

            _currentCHannel1 = currentChannel1;
            _currentCHannel2 = currentChannel2;

            TotalPacket = totalPacket;
            CurrentPacket = currentPacket;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="ch1"></param>
        /// <param name="ch2"></param>
        public void GetData(out double[] globalChannel1, out double[] globalChannel2)
        {
            globalChannel1 = _globalChannel1;
            globalChannel2 = _globalChannel2;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="ch1"></param>
        /// <param name="ch2"></param>
        public void GetData(out double[] globalChannel1, out double[] globalChannel2, out double[] currentCHannel1, out double[] currentCHannel2)
        {
            GetData(out globalChannel1, out globalChannel2);

            currentCHannel1 = _currentCHannel1;
            currentCHannel2 = _currentCHannel2;
        }
    }

    public class Sample
    {
        private Random randomNumber = new Random();

        public event EventHandler<ScopeReadDataCompletedEventArgs> ScopeReadDataCompleted;

        /// <summary>
        /// 开始连续采集
        /// </summary>
        public void StartSerialSampple()
        {
            var thread = new Thread(() =>
            {
                int sampleCount = 10 * 96 * 1000;
                int packetDataLength = 96 * 1000 / 20;

                double[] channelData1 = new double[sampleCount];
                double[] channelData2 = new double[sampleCount];

                double[] currentChannel1 = new double[packetDataLength];
                double[] currentChannel2 = new double[packetDataLength];

                int totalPacket = sampleCount / packetDataLength;
                int currentPacket = 0;

                double value1 = 0.5;
                double value2 = 0.5;

                while (true)
                {
                    for (int i = 0; i < currentChannel1.Length; i++)
                    {
                        value1 += (randomNumber.NextDouble() / 1000) - 0.0005;
                        if (value1 > 1)
                        {
                            value1 = 1;
                        }
                        else if (value1 < 0)
                        {
                            value1 = 0;
                        }

                        currentChannel1[i] = value1;
                    }

                    for (int i = 0; i < currentChannel2.Length; i++)
                    {
                        value2 += (randomNumber.NextDouble() / 1000) - 0.0005;
                        if (value2 > 1)
                        {
                            value2 = 1;
                        }
                        else if (value2 < 0)
                        {
                            value2 = 0;
                        }

                        currentChannel2[i] = value2;
                    }

                    ScopeReadDataCompleted?.Invoke(this, new ScopeReadDataCompletedEventArgs(channelData1, channelData2, currentChannel1, currentChannel2, totalPacket, currentPacket % totalPacket));
                    Thread.Sleep(1000 / 20);
                    currentPacket++;
                }

            });
            thread.IsBackground = true;
            thread.Start();
        }
    }

    public class MainWindowModel : Screen
    {
        
        public MainWindowModel()
        {
            Sample.ScopeReadDataCompleted += Sample_ScopeReadDataCompleted;
        }

        public Sample Sample = new Sample();

        public void Start()
        {
            Sample.StartSerialSampple();

        }

        private void Sample_ScopeReadDataCompleted(object sender, ScopeReadDataCompletedEventArgs e)
        {
            //获取示波器数据
            e.GetData(out double[] globalChannel1, out double[] globalChannel2, out double[] currentCHannel1, out double[] currentCHannel2);

            AppendScopeData(currentCHannel1, currentCHannel2, true);
        }

        #region 数据模型

        #region 配置参数

        /// <summary>
        /// 采样率
        /// </summary>
        public int SampleRate { get; set; } = 96 * 1000;

        /// <summary>
        /// 显示率(点/S)
        /// </summary>
        public int DisplayRate { get; set; } = 100;

        /// <summary>
        /// 显示间隔(忽略此间隔中的其他数)
        /// </summary>
        public int DisplayInterval
        {
            get
            {
                return SampleRate / DisplayRate;
            }
        }

        /// <summary>
        /// 时间间隔
        /// </summary>
        public double TimeInterval
        {
            get
            {
                return 1.0 / DisplayRate;
            }
        }

        /// <summary>
        /// 总时间(S)
        /// </summary>
        public int TotalTime { get; set; } = 10;

        /// <summary>
        /// 总数量
        /// </summary>
        public int TotalCount
        {
            get
            {

                return IsShowLastValue ? TotalTime * DisplayRate * 2 : TotalTime * DisplayRate;
            }
        }

        /// <summary>
        /// 显示上一次的数据
        /// </summary>
        public bool IsShowLastValue { get; set; } = false;

        #endregion

        #region 示波器通道

        #region 通道A

        private ObservableCollection<DoublePoint> scopeChACollection = new ObservableCollection<DoublePoint>();

        /// <summary>
        /// 通道A数据
        /// </summary>
        public ObservableCollection<DoublePoint> ScopeCHACollection
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

        #endregion

        #region 通道B

        private ObservableCollection<DoublePoint> scopeChBCollection = new ObservableCollection<DoublePoint>();

        /// <summary>
        /// 通道B数据
        /// </summary>
        public ObservableCollection<DoublePoint> ScopeCHBCollection
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

        #endregion

        /// <summary>
        /// 清除示波器数据
        /// </summary>
        public void ClearScopeData()
        {
            ScopeCHACollection = new ObservableCollection<DoublePoint>();
            ScopeCHBCollection = new ObservableCollection<DoublePoint>();
        }

        /// <summary>
        /// 追加示波器数据
        /// </summary>
        /// <param name="channel1">通道1数据</param>
        /// <param name="channel2">通道2数据</param>
        public void AppendScopeData(double[] channel1, double[] channel2, bool isLimit = true)
        {
            ObservableCollection<DoublePoint> scopeCHACollection;
            ObservableCollection<DoublePoint> scopeCHBCollection;

            {
                double time = 0;
                double lastValue = 0;
                var tempCHA = new ObservableCollection<DoublePoint>();
                var tempCHA2 = new ObservableCollection<DoublePoint>();

                //将之前的数据转移到临时变量中
                foreach (var item in ScopeCHACollection)
                {
                    tempCHA.Add(item);
                }

                if (tempCHA.Count > 0)
                {
                    time = tempCHA[tempCHA.Count - 1].Data;
                    lastValue = tempCHA[tempCHA.Count - 1].Value;
                }

                //将数据追加在结尾
                for (int i = 0; i < channel1.Length / DisplayInterval; i++)
                {
                    if (IsShowLastValue)
                    {
                        tempCHA.Add(new DoublePoint() { Data = time + (i + 1) * TimeInterval, Value = lastValue });
                    }
                    lastValue = channel1[i * DisplayInterval];
                    tempCHA.Add(new DoublePoint() { Data = time + (i + 1) * TimeInterval, Value = lastValue });
                }

                //如果数据过长,则截掉前面的
                if ((isLimit) && (tempCHA.Count > TotalCount))
                {
                    int difference = tempCHA.Count - TotalCount;

                    for (int i = difference; i < tempCHA.Count; i++)
                    {
                        tempCHA2.Add(tempCHA[i]);
                    }
                    scopeCHACollection = tempCHA2;
                }
                else
                {
                    scopeCHACollection = tempCHA;
                }
            }

            {
                double time = 0;
                double lastValue = 0;
                var tempCHB = new ObservableCollection<DoublePoint>();
                var tempCHB2 = new ObservableCollection<DoublePoint>();

                //将之前的数据转移到临时变量中
                foreach (var item in ScopeCHBCollection)
                {
                    tempCHB.Add(item);
                }

                if (tempCHB.Count > 0)
                {
                    time = tempCHB[tempCHB.Count - 1].Data;
                    lastValue = tempCHB[tempCHB.Count - 1].Value;
                }

                //将数据追加在结尾
                for (int i = 0; i < channel2.Length / DisplayInterval; i++)
                {
                    if (IsShowLastValue)
                    {
                        tempCHB.Add(new DoublePoint() { Data = time + (i + 1) * TimeInterval, Value = lastValue });
                    }
                    lastValue = channel2[i * DisplayInterval];
                    tempCHB.Add(new DoublePoint() { Data = time + (i + 1) * TimeInterval, Value = lastValue });
                }

                //如果数据过长,则截掉前面的
                if ((isLimit) && (tempCHB.Count > TotalCount))
                {
                    int difference = tempCHB.Count - TotalCount;
                    for (int i = difference; i < tempCHB.Count; i++)
                    {
                        tempCHB2.Add(tempCHB[i]);
                    }
                    scopeCHBCollection = tempCHB2;
                }
                else
                {
                    scopeCHBCollection = tempCHB;
                }
            }

            //最后才将数据赋值给绑定的变量
            ScopeCHACollection = scopeCHACollection;
            ScopeCHBCollection = scopeCHBCollection;
        }

        #endregion

        #region 模板

        /// <summary>
        /// 记录的模板
        /// </summary>
        private ObservableCollection<DoublePoint> recordTemplateCollection = new ObservableCollection<DoublePoint>();

        private ObservableCollection<DoublePoint> templateCollection = new ObservableCollection<DoublePoint>();

        /// <summary>
        /// 模板
        /// </summary>
        public ObservableCollection<DoublePoint> TemplateCollection
        {
            get
            {
                return templateCollection;
            }
            set
            {
                templateCollection = value;
                NotifyOfPropertyChange(() => TemplateCollection);
            }
        }


        /// <summary>
        /// 显示模板
        /// </summary>
        /// <param name="baseTime"></param>
        public void ShowTemplate(double baseTime = 0)
        {
            if (recordTemplateCollection?.Count > 0)
            {
                var tempTemplate = new ObservableCollection<DoublePoint>();
                foreach (var item in recordTemplateCollection)
                {
                    var temp = new DoublePoint();
                    temp.Data = item.Data + baseTime;
                    temp.Value = item.Value;
                    tempTemplate.Add(temp);
                }
                TemplateCollection = tempTemplate;
            }

        }

        /// <summary>
        /// 清除模板
        /// </summary>
        public void ClearTemplate()
        {
            TemplateCollection = new ObservableCollection<DoublePoint>();
        }

        #endregion

        #region 电源模块输出数据

        private ObservableCollection<DoublePoint> powerCollection = new ObservableCollection<DoublePoint>();

        /// <summary>
        /// 电源模块输出信息
        /// </summary>
        public ObservableCollection<DoublePoint> PowerCollection
        {
            get
            {
                return powerCollection;
            }
            set
            {
                powerCollection = value;
                NotifyOfPropertyChange(() => PowerCollection);
            }
        }

        /// <summary>
        /// 清除电源数据
        /// </summary>
        public void ClearPowerData()
        {
            PowerCollection = new ObservableCollection<DoublePoint>();
        }

        /// <summary>
        /// 追加电源数据
        /// </summary>
        public void AppendPowerData(ObservableCollection<DoublePoint> datas)
        {
            //将之前的数据转移到临时变量中
            var tempPower = new ObservableCollection<DoublePoint>();
            foreach (var item in PowerCollection)
            {
                tempPower.Add(item);
            }

            //追加数据到后面
            foreach (var item in datas)
            {
                tempPower.Add(item);
            }

            PowerCollection = tempPower;
        }

        #endregion

        #endregion

    }
}
