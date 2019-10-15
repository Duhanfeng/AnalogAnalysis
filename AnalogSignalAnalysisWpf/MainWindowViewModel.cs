using Caliburn.Micro;
using MahApps.Metro;
using AnalogSignalAnalysisWpf.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using AnalogSignalAnalysisWpf.Event;
using MahApps.Metro.Controls.Dialogs;
using System.Threading;
using MahApps.Metro.Controls;
using AnalogSignalAnalysisWpf.Hardware.Scope;
using AnalogSignalAnalysisWpf.Hardware.PLC;
using System.Collections.ObjectModel;
using System.IO.Ports;
using AnalogSignalAnalysisWpf.Hardware.PWM;

namespace AnalogSignalAnalysisWpf
{

    public class AccentColorMenuData
    {
        public string Name { get; set; }

        public Brush BorderColorBrush { get; set; }

        public Brush ColorBrush { get; set; }

        public AccentColorMenuData()
        {
            this.ChangeAccentCommand = new SimpleCommand(o => true, this.DoChangeTheme);
        }

        public ICommand ChangeAccentCommand { get; }

        protected virtual void DoChangeTheme(object sender)
        {
            ThemeManager.ChangeThemeColorScheme(Application.Current, this.Name);
        }
    }

    public class AppThemeMenuData : AccentColorMenuData
    {
        protected override void DoChangeTheme(object sender)
        {
            ThemeManager.ChangeThemeBaseColor(Application.Current, this.Name);
        }
    }


    public class MainWindowViewModel : Screen, IDisposable
    {
        /// <summary>
        /// 创建MainWindowViewModel新实例
        /// </summary>
        public MainWindowViewModel()
        {
            //配置窗口
            ShowTitleBar = false;
            IgnoreTaskbarOnMaximize = true;
            Topmost = false;
            QuitConfirmationEnabled = true;

            //获取所有颜色的集合
            this.AccentColors = ThemeManager.ColorSchemes
                                            .Select(a => new AccentColorMenuData { Name = a.Name, ColorBrush = a.ShowcaseBrush })
                                            .ToList();

            //获取所有主题的集合
            this.AppThemes = ThemeManager.Themes
                                         .GroupBy(x => x.BaseColorScheme)
                                         .Select(x => x.First())
                                         .Select(a => new AppThemeMenuData() { Name = a.BaseColorScheme, BorderColorBrush = a.Resources["MahApps.Brushes.BlackColor"] as Brush, ColorBrush = a.Resources["MahApps.Brushes.WhiteColor"] as Brush })
                                         .ToList();

            //创建示波器控件实例
            Scope = new Hantek66022BE();
            ScopeControlView = new ScopeControlView();
            ScopeControlView.DataContext = new ScopeControlViewModel(Scope);

            //创建PLC控件实例
            PLC = new ModbusPLC("COM1");
            PLCControlView = new PLCControlView();
            PLCControlView.DataContext = this;

            //创建PWM控件实例
            PWMControlView = new PWMControlView();
            PWMControlView.DataContext = this;

            //更新串口
            SerialPorts = new ObservableCollection<string>(SerialPort.GetPortNames());
            PLC.PrimarySerialPortName = SerialPorts[0] ?? "";

        }

        #region 窗口控制

        private bool showTitleBar;

        /// <summary>
        /// 显示顶部控件
        /// </summary>
        public bool ShowTitleBar
        {
            get
            {
                return showTitleBar;
            }
            set
            {
                showTitleBar = value;
                NotifyOfPropertyChange(() => ShowTitleBar);
            }
        }

        private bool ignoreTaskbarOnMaximize;

        /// <summary>
        /// 最大化时忽略底部任务栏
        /// </summary>
        public bool IgnoreTaskbarOnMaximize
        {
            get
            {
                return ignoreTaskbarOnMaximize;
            }
            set
            {
                ignoreTaskbarOnMaximize = value;
                NotifyOfPropertyChange(() => IgnoreTaskbarOnMaximize);
            }
        }

        private bool topmost;

        /// <summary>
        /// 置顶
        /// </summary>
        public bool Topmost
        {
            get
            {
                return topmost;
            }
            set
            {
                topmost = value;
                NotifyOfPropertyChange(() => Topmost);
            }
        }

        private bool quitConfirmationEnabled;

        /// <summary>
        /// 快速确认使能
        /// </summary>
        public bool QuitConfirmationEnabled
        {
            get
            {
                return quitConfirmationEnabled;
            }
            set
            {
                quitConfirmationEnabled = value;
                NotifyOfPropertyChange(() => QuitConfirmationEnabled);
            }
        }

        /// <summary>
        /// 颜色
        /// </summary>
        public List<AccentColorMenuData> AccentColors { get; set; }

        /// <summary>
        /// 主题
        /// </summary>
        public List<AppThemeMenuData> AppThemes { get; set; }

        #endregion

        #region 串口

        private ObservableCollection<string> serialPorts;

        /// <summary>
        /// 串口列表
        /// </summary>
        public ObservableCollection<string> SerialPorts
        {
            get
            {
                return serialPorts;
            }
            set
            {
                serialPorts = value;
                NotifyOfPropertyChange(() => SerialPorts);
            }
        }

        #endregion

        #region 硬件

        /// <summary>
        /// 示波器
        /// </summary>
        public IScope Scope { get; set; }

        /// <summary>
        /// 示波器有效标志
        /// </summary>
        public bool IsScopeValid
        {
            get
            {
                return Scope?.IsConnect ?? false;
            }
        }

        #endregion

        #region 子窗口

        private ScopeControlView scopeControlView;

        /// <summary>
        /// 示波器配置控件
        /// </summary>
        public ScopeControlView ScopeControlView
        {
            get 
            { 
                return scopeControlView;
            }
            set
            { 
                scopeControlView = value;
                NotifyOfPropertyChange(() => ScopeControlView);
            }
        }

        private PLCControlView plcControlView;

        /// <summary>
        /// PLC配置控件
        /// </summary>
        public PLCControlView PLCControlView
        {
            get
            {
                return plcControlView;
            }
            set
            {
                plcControlView = value;
                NotifyOfPropertyChange(() => PLCControlView);
            }
        }

        private PWMControlView pwmControlView;

        /// <summary>
        /// PWM配置控件
        /// </summary>
        public PWMControlView PWMControlView
        {
            get
            {
                return pwmControlView;
            }
            set
            {
                pwmControlView = value;
                NotifyOfPropertyChange(() => PWMControlView);
            }
        }

        #endregion

        #region PLC

        /// <summary>
        /// PLC
        /// </summary>
        public IPLC PLC { get; set; }

        /// <summary>
        /// PLC有效标志
        /// </summary>
        public bool IsPLCValid
        {
            get
            {
                return PLC?.IsConnect ?? false;
            }
        }

        #region COM配置

        /// <summary>
        /// 串口号
        /// </summary>
        public string PLCSerialPort
        {
            get
            {
                return PLC?.PrimarySerialPortName ?? "Nana";
            }
            set
            {
                if (PLC != null)
                {
                    PLC.PrimarySerialPortName = value;
                }

                NotifyOfPropertyChange(() => PLCSerialPort);
            }
        }

        /// <summary>
        /// 串口波特率
        /// </summary>
        public int PLCSerialPortBaudRate 
        { 
            get
            {
                return PLC?.SerialPortBaudRate ?? 115200;
            }
            set
            {
                if (PLC != null)
                {
                    PLC.SerialPortBaudRate = value;
                }

                NotifyOfPropertyChange(() => PLCSerialPortBaudRate);
            }
        }

        /// <summary>
        /// 从站地址
        /// </summary>
        public byte PLCSlaveAddress
        {
            get
            {
                return PLC?.SlaveAddress ?? 0xFF;
            }
            set
            {
                if (PLC != null)
                {
                    PLC.SlaveAddress = value;
                }

                NotifyOfPropertyChange(() => PLCSlaveAddress);
            }
        }

        /// <summary>
        /// 超时
        /// </summary>
        public int PLCTimeout
        {
            get
            {
                return PLC?.ReadTimeout ?? -1;
            }
            set
            {
                if (PLC != null)
                {
                    PLC.ReadTimeout = value;
                    PLC.WriteTimeout = value;
                }

                NotifyOfPropertyChange(() => PLCTimeout);
            }
        }

        /// <summary>
        /// 连接到PLC
        /// </summary>
        public void ConnectPLC()
        {
            PLC?.Connect();
            NotifyOfPropertyChange(() => IsPLCValid);
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void DisconnectPLC()
        {
            PLC?.Disconnect();
            NotifyOfPropertyChange(() => IsPLCValid);
        }

        #endregion

        #region 输出参数控制

        /// <summary>
        /// PLC电压
        /// </summary>
        public double PLCVoltage
        {
            get
            {
                if (IsPLCValid)
                {
                    return PLC.Voltage;
                }
                return -1;
            }
            set
            {
                if (IsPLCValid == true)
                {
                    PLC.Voltage = value;
                }

                NotifyOfPropertyChange(() => PLCVoltage);
            }
        }

        /// <summary>
        /// PLC电流
        /// </summary>
        public double PLCCurrent
        {
            get
            {
                if (IsPLCValid)
                {
                    return PLC.Current;
                }
                return -1;
            }
            set
            {
                if (IsPLCValid == true)
                {
                    PLC.Current = value;
                }
                
                NotifyOfPropertyChange(() => PLCCurrent);
            }
        }

        /// <summary>
        /// PLC电流
        /// </summary>
        public bool PLCEnable
        {
            get
            {
                if (IsPLCValid)
                {
                    return PLC.Enable;
                }
                return false;
            }
            set
            {
                if (IsPLCValid == true)
                {
                    PLC.Enable = value;
                }

                NotifyOfPropertyChange(() => PLCEnable);
            }
        }

        /// <summary>
        /// 实际电压值
        /// </summary>
        public double PLCRealityVoltage
        {
            get
            {
                if (IsPLCValid)
                {
                    return PLC.RealityVoltage;
                }
                return -1;
            }
        }

        /// <summary>
        /// 实际电流值
        /// </summary>
        public double PLCRealityCurrent
        {
            get
            {
                if (IsPLCValid)
                {
                    return PLC.RealityCurrent;
                }
                return -1;
            }
        }

        /// <summary>
        /// 实际温度值
        /// </summary>
        public double PLCRealityTemperature
        {
            get
            {
                if (IsPLCValid)
                {
                    return PLC.RealityTemperature;
                }
                return -1;
            }
        }

        /// <summary>
        /// 更新PLC状态
        /// </summary>
        public void UpdatePLCStatus()
        {
            NotifyOfPropertyChange(() => PLCRealityVoltage);
            NotifyOfPropertyChange(() => PLCRealityCurrent);
            NotifyOfPropertyChange(() => PLCRealityTemperature);

        }

        /// <summary>
        /// 使能PLC输出
        /// </summary>
        public void EnablePLC()
        {
            if (IsPLCValid)
            {
                PLC.Enable = !PLC.Enable;
            }
        }

        #endregion

        #endregion

        #region PWM

        /// <summary>
        /// PWM
        /// </summary>
        IPWM PWM { get; set; }

        #region COM配置

        /// <summary>
        /// 串口号
        /// </summary>
        public string PWMSerialPort
        {
            get
            {
                return PWM?.PrimarySerialPortName ?? "Nana";
            }
            set
            {
                if (PWM != null)
                {
                    PWM.PrimarySerialPortName = value;
                }

                NotifyOfPropertyChange(() => PWMSerialPort);
            }
        }


        #endregion

        #region 输出参数控制

        /// <summary>
        /// PWM频率
        /// </summary>
        public int PWMFrequency
        {
            get
            {
                if (PWM != null)
                {
                    return PWM.Frequency;
                }
                return -1;
            }
            set
            {
                if (PWM != null)
                {
                    PWM.Frequency = value;
                }

                NotifyOfPropertyChange(() => PWMFrequency);
            }
        }

        /// <summary>
        /// PWM占空比
        /// </summary>
        public double PWMDutyRatio
        {
            get
            {
                if (PWM != null)
                {
                    return PWM.DutyRatio;
                }
                return -1;
            }
            set
            {
                if (PWM != null)
                {
                    PWM.DutyRatio = value;
                }

                NotifyOfPropertyChange(() => PWMDutyRatio);
            }
        }

        #endregion

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

        #region 测试

        /// <summary>
        /// 抛异常
        /// </summary>
        public void ThrowException()
        {
            try
            {
                throw new InvalidOperationException("手动触发异常");
            }
            catch (Exception ex)
            {
                OnMessageRaised(MessageLevel.Err, ex.Message, ex);
            }
        }

        public void ThrowWarning()
        {
            OnMessageRaised(MessageLevel.Warning, "警告信息");
        }

        public void ThrowMessage()
        {
            OnMessageRaised(MessageLevel.Message, "提示信息");
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~MainWindowViewModel()
        // {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
