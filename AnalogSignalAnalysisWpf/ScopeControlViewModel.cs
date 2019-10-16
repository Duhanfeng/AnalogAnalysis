using AnalogSignalAnalysisWpf.Hardware;
using AnalogSignalAnalysisWpf.Hardware.Scope;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalogSignalAnalysisWpf
{
    class ScopeControlViewModel : Screen
    {
        #region 构造函数

        public ScopeControlViewModel(IScope scope)
        {
            Scope = scope;
            VoltageDIVCollection = new ObservableCollection<string>(EnumHelper.GetAllDescriptions<EVoltageDIV>());
            TimeDIVCollection = new ObservableCollection<string>(EnumHelper.GetAllDescriptions<ESampleRate>());
            TriggerSweepCollection = new ObservableCollection<string>(EnumHelper.GetAllDescriptions<ETriggerSweep>());
            TriggerSourceCollection = new ObservableCollection<string>(EnumHelper.GetAllDescriptions<ETriggerSource>());
            TriggerSlopeCollectione = new ObservableCollection<string>(EnumHelper.GetAllDescriptions<ETriggerSlope>());
            InsertModeCollection = new ObservableCollection<string>(EnumHelper.GetAllDescriptions<EInsertMode>());

        }

        #endregion

        #region 硬件实例

        /// <summary>
        /// 示波器
        /// </summary>
        public IScope Scope { get; set; }

        /// <summary>
        /// 连接标志位
        /// </summary>
        public bool IsConnect 
        { 
            get
            {
                return Scope?.IsConnect ?? false;
            }
        }

        #endregion

        #region 配置属性

        public ObservableCollection<string> VoltageDIVCollection { get; set; }

        public ObservableCollection<string> TimeDIVCollection { get; set; }

        public ObservableCollection<string> TriggerSweepCollection { get; set; }

        public ObservableCollection<string> TriggerSourceCollection { get; set; }

        public ObservableCollection<string> TriggerSlopeCollectione { get; set; }

        public ObservableCollection<string> InsertModeCollection { get; set; }

        public string CH1VoltageDIV
        {
            get
            {
                return EnumHelper.GetDescription(Scope.CH1VoltageDIV);
            }
            set
            {
                Scope.CH1VoltageDIV = EnumHelper.GetEnum<EVoltageDIV>(value);
                NotifyOfPropertyChange(() => CH1VoltageDIV);
            }
        }

        public string CH2VoltageDIV
        {
            get
            {
                return EnumHelper.GetDescription(Scope.CH2VoltageDIV);
            }
            set
            {
                Scope.CH2VoltageDIV = EnumHelper.GetEnum<EVoltageDIV>(value);
                NotifyOfPropertyChange(() => CH2VoltageDIV);
            }
        }

        public string SampleRate
        {
            get
            {
                return EnumHelper.GetDescription(Scope.SampleRate);
            }
            set
            {
                Scope.SampleRate = EnumHelper.GetEnum<ESampleRate>(value);
                NotifyOfPropertyChange(() => SampleRate);
            }
        }

        public string TriggerSweep
        {
            get
            {
                return EnumHelper.GetDescription(Scope.TriggerSweep);
            }
            set
            {
                Scope.TriggerSweep = EnumHelper.GetEnum<ETriggerSweep>(value);
                NotifyOfPropertyChange(() => TriggerSweep);
            }
        }

        public string TriggerSource
        {
            get
            {
                return EnumHelper.GetDescription(Scope.TriggerSource);
            }
            set
            {
                Scope.TriggerSource = EnumHelper.GetEnum<ETriggerSource>(value);
                NotifyOfPropertyChange(() => TriggerSource);
            }
        }

        public string TriggerSlope
        {
            get
            {
                return EnumHelper.GetDescription(Scope.TriggerSlope);
            }
            set
            {
                Scope.TriggerSlope = EnumHelper.GetEnum<ETriggerSlope>(value);
                NotifyOfPropertyChange(() => TriggerSlope);
            }
        }

        public string InsertMode
        {
            get
            {
                return EnumHelper.GetDescription(Scope.InsertMode);
            }
            set
            {
                Scope.InsertMode = EnumHelper.GetEnum<EInsertMode>(value);
                NotifyOfPropertyChange(() => InsertMode);
            }
        }

        public int TriggerLevel
        {
            get
            {
                return Scope.TriggerLevel;
            }
            set
            {
                Scope.TriggerLevel = value;
                NotifyOfPropertyChange(() => TriggerLevel);
            }
        }

        public int SampleTime
        {
            get
            {
                return Scope.SampleTime;
            }
            set
            {
                Scope.SampleTime = value;
                NotifyOfPropertyChange(() => SampleTime);
            }
        }

        #endregion

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

        #region 方法

        /// <summary>
        /// 连接设备
        /// </summary>
        public void Connect()
        {
            Scope?.Connect(0);
            NotifyOfPropertyChange(() => IsConnect);
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            Scope?.Disconnect();
            NotifyOfPropertyChange(() => IsConnect);
        }

        #endregion

    }
}
