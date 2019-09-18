using AnalogDataAnalysisWpf.Hantek66022BE;
using AnalogDataAnalysisWpf.LiveData;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
                VirtualOscilloscope = new VirtualOscilloscope(0, 10240);
            }
            catch (Exception ex)
            {
                OnMessageRaised( MessageLevel.Err, ex.Message, ex);
            }
        }

        public VirtualOscilloscope VirtualOscilloscope { get; set; }

        //public LiveDataViewModel

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

                if (value is LiveDataViewModel)
                {
                    liveDataViewModel1 = value as LiveDataViewModel;
                }

                NotifyOfPropertyChange(() => LiveDataViewModel1);
            }
        }

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
                ushort[] channel1Array;
                ushort[] channel2Array;
                uint triggerPointIndex;

                VirtualOscilloscope.ReadDeviceData(out channel1Array, out channel2Array, out triggerPointIndex);

                var collection = new System.Collections.ObjectModel.ObservableCollection<Data>();
                for (int i = 0; i < channel1Array.Length; i++)
                {
                    collection.Add(new Data() { Value1 = channel1Array[i], Value = i });
                }

                LiveDataViewModel1.Collection = collection;
            }
            catch (Exception ex)
            {
                OnMessageRaised(MessageLevel.Err, ex.Message, ex);
            }

        }

        /// <summary>
        /// 打开窗口配置窗口
        /// </summary>
        public void OpenDeviceConfigWindow()
        {
            //ScenesListView.SelectedItem
            var view = new DeviceConfigView(VirtualOscilloscope)
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            //将控件嵌入窗口之中
            var window = new Window();
            window.MinWidth = view.MinWidth + 50;
            window.MinHeight = view.MinHeight + 50;
            window.MaxWidth = view.MaxWidth;
            window.MaxHeight = view.MaxHeight;
            window.Width = view.MinWidth + 50;
            window.Height = view.MinHeight + 50;
            window.Content = view;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            //window.Owner = Window.GetWindow(this);
            window.Title = "设备配置窗口";
            window.Show();
        }

    }
}
