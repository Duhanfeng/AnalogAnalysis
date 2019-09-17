using AnalogAnalysis;
using AnalogAnalysisWpf.Performance;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AnalogAnalysisWpf
{
    class MainWindowViewModel : Screen
    {



        private IntPtr calData = Marshal.AllocHGlobal(2 * 32);

        #region 事件

        /// <summary>
        /// 消息触发事件
        /// </summary>
        internal event EventHandler<MessageRaisedEventArgs> MessageRaised;

        #endregion

        #region 属性

        private LiveDataViewModel liveDataViewModel = new LiveDataViewModel();

        public LiveDataViewModel LiveDataViewModel
        {
            get
            {
                return liveDataViewModel;
            }
            set
            {
                if (value is LiveDataViewModel)
                {
                    liveDataViewModel = value as LiveDataViewModel;
                }

                NotifyOfPropertyChange(() => LiveDataViewModel);
            }
        }

        #endregion

        public MainWindowViewModel(PerformanceView performanceView)
        {
            PerformanceView = performanceView;
            //LiveDataViewModel.Collection.Clear();
            //LiveDataViewModel.Collection.Add(new Data() { Value = 1, Value1 = 1 });
            //LiveDataViewModel.Collection.Add(new Data() { Value = 2, Value1 = 2 });
            //LiveDataViewModel.Collection.Add(new Data() { Value = 3, Value1 = 3 });
            //LiveDataViewModel.Collection.Add(new Data() { Value = 4, Value1 = 4 });
        }

        public PerformanceView PerformanceView { get; set; }

        /// <summary>
        /// 初始化系统
        /// </summary>
        public void Init()
        {
            if (Hantek66022BE.dsoOpenDevice(0) != 0)
            {
                unsafe
                {
                    Hantek66022BE.dsoGetCalLevel(0, calData, 32);
                    Hantek66022BE.dsoSetVoltDIV(0, 0, 5);
                    Hantek66022BE.dsoSetVoltDIV(0, 1, 5);
                    Hantek66022BE.dsoSetTimeDIV(0, 14);
                }
            }
            else
            {
                MessageRaised?.Invoke(this, new MessageRaisedEventArgs(MessageLevel.Err, "打开设备失败!"));
            }
        }

        public void ReadData()
        {
            unsafe
            {
                uint dataLength = 10240;
                //uint dataLength = 1000;
                IntPtr channel1Data = Marshal.AllocHGlobal(2 * (int)dataLength);
                IntPtr channel2Data = Marshal.AllocHGlobal(2 * (int)dataLength);
                uint trigPointIndex = 0;

                if (Hantek66022BE.dsoReadHardData(0, channel1Data, channel2Data, dataLength, calData, 5, 5, 0, 0, 64, 0, 14, 50, (uint)dataLength, ref trigPointIndex, 0) != -1)
                {
                    //显示数据
                    ushort* channel1 = (ushort*)channel1Data.ToPointer();
                    ushort* channel2 = (ushort*)channel2Data.ToPointer();

                    
                    short[] channel1Buff = new short[dataLength];
                    Marshal.Copy(channel1Data, channel1Buff, 0, (int)dataLength);

                    PerformanceView.SetData(channel1Buff);


                    var collection = new System.Collections.ObjectModel.ObservableCollection<Data>();
                    for (int i = 0; i < dataLength ; i++)
                    {
                        //LiveDataViewModel.Collection.Add(channel1Buff[i]);
                        collection.Add(new Data() { Value1 = channel1Buff[i], Value = i });
                    }

                    LiveDataViewModel.Collection = collection;

                    //
                    //SeriesCollection[0].Values = new ChartValues<short>(channel1Buff);
                    //NotifyOfPropertyChange(() => SeriesCollection);
                }
                else
                {
                    MessageRaised?.Invoke(this, new MessageRaisedEventArgs(MessageLevel.Err, "读取数据失败!"));
                }

                Marshal.FreeHGlobal(channel1Data);
                Marshal.FreeHGlobal(channel2Data);
            }
        }
    }
}
