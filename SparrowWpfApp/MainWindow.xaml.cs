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
            new Thread(() =>
            {
                int sampleCount = 10 * 96 * 1000;
                int packetDataLength = 96 * 1000 / 20;

                double[] channelData1 = new double[sampleCount];
                double[] channelData2 = new double[sampleCount];

                double[] currentChannel1 = new double[packetDataLength];
                double[] currentChannel2 = new double[packetDataLength];

                int totalPacket = sampleCount / packetDataLength;
                int currentPacket = 0;

                double value = 0.5;

                while (true)
                {
                    for (int i = 0; i < currentChannel1.Length; i++)
                    {
                        value += (randomNumber.NextDouble() / 1000) - 0.0005;
                        if (value > 1)
                        {
                            value = 1;
                        }
                        else if (value < 0)
                        {
                            value = 0;
                        }

                        currentChannel1[i] = value;
                    }
                    
                    ScopeReadDataCompleted?.Invoke(this, new ScopeReadDataCompletedEventArgs(channelData1, channelData2, currentChannel1, currentChannel2, totalPacket, currentPacket % totalPacket));
                    Thread.Sleep(1000 / 20);
                    currentPacket++;
                }

            }).Start();
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

        int currentCount = 0;

        private void Sample_ScopeReadDataCompleted(object sender, ScopeReadDataCompletedEventArgs e)
        {
            int interval = 96;

            e.GetData(out double[] globalChannel1, out double[] globalChannel2, out double[] currentCHannel1, out double[] currentCHannel2);

            var points = new ObservableCollection<DoublePoint>();

            foreach (var item in CHAPointsCollection)
            {
                points.Add(item);
            }

            for (int i = 0; i < currentCHannel1.Length; i++)
            {
                if ((i % interval) == 0)
                {
                    points.Add(new DoublePoint() { Data = currentCount / 96000.0, Value = currentCHannel1[i] });
                }
                currentCount++;
            }

            //如果数据过长,则裁剪一部分
            int totalCount = e.TotalPacket * currentCHannel1.Length / interval;
            int invalidCount = currentCHannel1.Length / interval;

            if (points.Count > totalCount)
            {
                var points2 = new ObservableCollection<DoublePoint>();

                for (int i = invalidCount; i < totalCount; i++)
                {
                    points2.Add(points[i]);
                }
                CHAPointsCollection = points2;
            }
            else
            {
                CHAPointsCollection = points;
            }

        }

        private ObservableCollection<DoublePoint> chAPointsCollection = new ObservableCollection<DoublePoint>();

        public ObservableCollection<DoublePoint> CHAPointsCollection
        {
            get { return chAPointsCollection; }
            set { chAPointsCollection = value; NotifyOfPropertyChange(() => CHAPointsCollection); }
        }

    }
}
