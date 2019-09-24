using AnalogDataAnalysisWpf.Hantek66022BE;
using AnalogDataAnalysisWpf.LiveData;
using Caliburn.Micro;
using DataAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static DataAnalysis.Analysis;

namespace AnalogDataAnalysisWpf
{
    public class MainWindowViewModel : Screen
    {
        /// <summary>
        /// 创建MainWindowViewModel新实例
        /// </summary>
        public MainWindowViewModel()
        {
            try
            {
                VirtualOscilloscope = new VirtualOscilloscope(0, 100);
                deviceConfigView = new DeviceConfigView(VirtualOscilloscope);
            }
            catch (Exception ex)
            {
                OnMessageRaised( MessageLevel.Err, ex.Message, ex);
            }
        }

        public VirtualOscilloscope VirtualOscilloscope { get; set; }

        private LiveDataViewModel liveDataViewModel1 = new LiveDataViewModel();

        /// <summary>
        /// LiveData显示模型
        /// </summary>
        public LiveDataViewModel LiveDataViewModel1
        {
            get
            {
                return liveDataViewModel1;
            }
            set
            {
                liveDataViewModel1 = value;
                NotifyOfPropertyChange(() => LiveDataViewModel1);
            }
        }


        private LiveDataViewModel liveDataViewModel2 = new LiveDataViewModel();

        /// <summary>
        /// LiveData显示模型
        /// </summary>
        public LiveDataViewModel LiveDataViewModel2
        {
            get
            {
                return liveDataViewModel2;
            }
            set
            {
                liveDataViewModel2 = value;
                NotifyOfPropertyChange(() => LiveDataViewModel2);
            }
        }

        private DeviceConfigView deviceConfigView;

        public DeviceConfigView DeviceConfigView
        {
            get
            {
                return deviceConfigView;
            }
            set
            {
                deviceConfigView = value;
                NotifyOfPropertyChange(() => DeviceConfigView);
            }
        }

        /// <summary>
        /// 滤波数据
        /// </summary>
        public double[] FilterData;

        /// <summary>
        /// 数据采样间隔
        /// </summary>
        public int SampleInterval = 1;

        public List<int> EdgeIndexs = new List<int>();

        public DigitEdgeType DigitEdgeType = DigitEdgeType.CriticalLevel;


        #region 事件

        /// <summary>
        /// 消息触发事件
        /// </summary>
        public event EventHandler<MessageRaisedEventArgs> MessageRaised;

        /// <summary>
        /// 触发消息触发事件
        /// </summary>
        /// <param name="messageLevel"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        protected void OnMessageRaised(MessageLevel messageLevel, string message, Exception exception = null)
        {
            MessageRaised?.Invoke(this, new MessageRaisedEventArgs(messageLevel, message, exception));
        }

        #endregion

        /// <summary>
        /// 读取设备数据
        /// </summary>
        public void ReadDeviceData()
        {
            
            try
            {
                double[] source;
                VirtualOscilloscope.ReadDeviceData(out source);
                int sampleRate = VirtualOscilloscope.SampleRate;

                //数据滤波
                Analysis.MeanFilter(source, out FilterData, 21);

                //显示数据
                var collection = new System.Collections.ObjectModel.ObservableCollection<Data>();
                for (int i = 0; i < source.Length / SampleInterval; i++)
                {
                    collection.Add(new Data() { Value1 = FilterData[i * SampleInterval], Value = i * 1000.0 / sampleRate * SampleInterval });
                }

                LiveDataViewModel1.Collection = collection;

                //提取边沿
                if (FilterData?.Length > 0)
                {

                    Analysis.FindEdgeByThreshold(FilterData, 0.4, 1.6, out EdgeIndexs, out DigitEdgeType);

                    if ((DigitEdgeType == DigitEdgeType.FirstFillingEdge) || (DigitEdgeType == DigitEdgeType.FirstRisingEdge))
                    {
                        collection = new ObservableCollection<Data>();

                        foreach (var item in EdgeIndexs)
                        {
                            collection.Add(new Data() { Value1 = LiveDataViewModel1.Collection[item / SampleInterval].Value1, Value = item * 1000.0 / sampleRate * SampleInterval });

                        }
                        LiveDataViewModel1.Collection2 = collection;
                    }

                }

            }
            catch (Exception ex)
            {
                OnMessageRaised(MessageLevel.Err, ex.Message, ex);
            }

        }

        ///// <summary>
        ///// 打开窗口配置窗口
        ///// </summary>
        //public void OpenDeviceConfigWindow()
        //{
        //    //ScenesListView.SelectedItem
        //    var view = new DeviceConfigView(VirtualOscilloscope)
        //    {
        //        HorizontalAlignment = HorizontalAlignment.Stretch,
        //        VerticalAlignment = VerticalAlignment.Stretch
        //    };

        //    //将控件嵌入窗口之中
        //    var window = new MahApps.Metro.Controls.MetroWindow();
        //    window.MinWidth = view.MinWidth + 50;
        //    window.MinHeight = view.MinHeight + 50;
        //    window.MaxWidth = view.MaxWidth;
        //    window.MaxHeight = view.MaxHeight;
        //    window.Width = view.MinWidth + 50;
        //    window.Height = view.MinHeight + 50;
        //    window.Content = view;
        //    window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        //    //window.Owner = Window.GetWindow(this);
        //    window.Title = "设备配置窗口";
        //    window.Show();
        //}

    }
}
