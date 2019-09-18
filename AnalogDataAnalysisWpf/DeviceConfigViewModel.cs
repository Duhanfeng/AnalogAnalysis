using AnalogDataAnalysisWpf.Hantek66022BE;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalogDataAnalysisWpf
{
    
    public class DeviceConfigViewModel : Screen
    {
        #region 构造函数

        public DeviceConfigViewModel(VirtualOscilloscope virtualOscilloscope)
        {
            VirtualOscilloscope = virtualOscilloscope;
            VoltageDIVCollection = new ObservableCollection<string>(EnumHelper.GetAllDescriptions<EVoltageDIV>());
            TimeDIVCollection = new ObservableCollection<string>(EnumHelper.GetAllDescriptions<ETimeDIV>());
            TriggerSweepCollection = new ObservableCollection<string>(EnumHelper.GetAllDescriptions<ETriggerSweep>());
            TriggerSourceCollection = new ObservableCollection<string>(EnumHelper.GetAllDescriptions<ETriggerSource>());
            TriggerSlopeCollectione = new ObservableCollection<string>(EnumHelper.GetAllDescriptions<ETriggerSlope>());
            InsertModeCollection = new ObservableCollection<string>(EnumHelper.GetAllDescriptions<EInsertMode>());

        }

        #endregion

        #region 硬件实例
        public VirtualOscilloscope VirtualOscilloscope { get; set; }

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
                return EnumHelper.GetDescription(VirtualOscilloscope.CH1VoltageDIV);
            }
            set
            {
                VirtualOscilloscope.CH1VoltageDIV = EnumHelper.GetEnum<EVoltageDIV>(value);
                NotifyOfPropertyChange(() => CH1VoltageDIV);
            }
        }

        public string CH2VoltageDIV
        {
            get
            {
                return EnumHelper.GetDescription(VirtualOscilloscope.CH2VoltageDIV);
            }
            set
            {
                VirtualOscilloscope.CH2VoltageDIV = EnumHelper.GetEnum<EVoltageDIV>(value);
                NotifyOfPropertyChange(() => CH2VoltageDIV);
            }
        }

        public string TimeDIV
        {
            get
            {
                return EnumHelper.GetDescription(VirtualOscilloscope.TimeDIV);
            }
            set
            {
                VirtualOscilloscope.TimeDIV = EnumHelper.GetEnum<ETimeDIV>(value);
                NotifyOfPropertyChange(() => TimeDIV);
            }
        }

        public string TriggerSweep
        {
            get
            {
                return EnumHelper.GetDescription(VirtualOscilloscope.TriggerSweep);
            }
            set
            {
                VirtualOscilloscope.TriggerSweep = EnumHelper.GetEnum<ETriggerSweep>(value);
                NotifyOfPropertyChange(() => TriggerSweep);
            }
        }

        public string TriggerSource
        {
            get
            {
                return EnumHelper.GetDescription(VirtualOscilloscope.TriggerSource);
            }
            set
            {
                VirtualOscilloscope.TriggerSource = EnumHelper.GetEnum<ETriggerSource>(value);
                NotifyOfPropertyChange(() => TriggerSource);
            }
        }

        public string TriggerSlope
        {
            get
            {
                return EnumHelper.GetDescription(VirtualOscilloscope.TriggerSlope);
            }
            set
            {
                VirtualOscilloscope.TriggerSlope = EnumHelper.GetEnum<ETriggerSlope>(value);
                NotifyOfPropertyChange(() => TriggerSlope);
            }
        }

        public string InsertMode
        {
            get
            {
                return EnumHelper.GetDescription(VirtualOscilloscope.InsertMode);
            }
            set
            {
                VirtualOscilloscope.InsertMode = EnumHelper.GetEnum<EInsertMode>(value);
                NotifyOfPropertyChange(() => InsertMode);
            }
        }

        public short TriggerLevel
        {
            get
            {
                return VirtualOscilloscope.TriggerLevel;
            }
            set
            {
                VirtualOscilloscope.TriggerLevel = value;
                NotifyOfPropertyChange(() => TriggerLevel);
            }
        }

        public short HorizontalTriggerPosition
        {
            get
            {
                return VirtualOscilloscope.HorizontalTriggerPosition;
            }
            set
            {
                VirtualOscilloscope.HorizontalTriggerPosition = value;
                NotifyOfPropertyChange(() => HorizontalTriggerPosition);
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
    }
}
