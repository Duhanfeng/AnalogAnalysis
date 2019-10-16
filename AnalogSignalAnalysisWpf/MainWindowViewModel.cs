using AnalogSignalAnalysisWpf.Core;
using AnalogSignalAnalysisWpf.Hardware.PLC;
using AnalogSignalAnalysisWpf.Hardware.PWM;
using AnalogSignalAnalysisWpf.Hardware.Scope;
using Caliburn.Micro;
using MahApps.Metro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

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
            Scope = new LOTOA02();
            ScopeControlView = new ScopeControlView2();
            ScopeControlView.DataContext = this;

            ScopeCouplingCollection = new ObservableCollection<string>(EnumHelper.GetAllDescriptions<LOTOA02.ECoupling>());
            ScopeSampleRateCollection = new ObservableCollection<string>(EnumHelper.GetAllDescriptions<LOTOA02.ESampleRate>());
            ScopeTriggerModelCollection = new ObservableCollection<string>(EnumHelper.GetAllDescriptions<LOTOA02.ETriggerModel>());
            ScopeTriggerEdgeCollection = new ObservableCollection<string>(EnumHelper.GetAllDescriptions<LOTOA02.ETriggerEdge>());
            ScopeVoltageDIVCollection = new ObservableCollection<string>(EnumHelper.GetAllDescriptions<LOTOA02.EVoltageDIV>());

            //创建PLC控件实例
            PLC = new ModbusPLC("COM1");
            PLCControlView = new PLCControlView();
            PLCControlView.DataContext = this;

            //创建PWM控件实例
            PWM = new SerialPortPWM();
            PWMControlView = new PWMControlView();
            PWMControlView.DataContext = this;

            //设置窗口
            SettingsView = new SettingsView();
            SettingsView.DataContext = this;

            //更新串口
            SerialPorts = new ObservableCollection<string>(SerialPort.GetPortNames());
            PLC.PrimarySerialPortName = SerialPorts[0] ?? "";

            AddRunningMessage("PWMControlView");
            AddRunningMessage("ObservableCollection");
            AddRunningMessage("SerialPortPWM SerialPortPWM SerialPortPWM SerialPortPWM SerialPortPWM SerialPortPWM SerialPortPWM SerialPortPWM");
            AddRunningMessage("ObservableCollection");

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

        #region 子窗口

        private ScopeControlView2 scopeControlView;

        /// <summary>
        /// 示波器配置控件
        /// </summary>
        public ScopeControlView2 ScopeControlView
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

        private SettingsView settingsView;

        /// <summary>
        /// 设置窗口
        /// </summary>
        public SettingsView SettingsView
        {
            get
            {
                return settingsView;
            }
            set
            {
                settingsView = value;
                NotifyOfPropertyChange(() => SettingsView);
            }
        }


        #endregion

        #region 硬件

        #region Scope

        /// <summary>
        /// 示波器
        /// </summary>
        public LOTOA02 Scope { get; set; }

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

        /// <summary>
        /// 连接到Scope
        /// </summary>
        public void ConnectScope()
        {
            Scope?.Connect();
            UpdateScopeStatus();
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void DisconnectScope()
        {
            Scope?.Disconnect();
            UpdateScopeStatus();
        }

        /// <summary>
        /// 更新示波器状态
        /// </summary>
        public void UpdateScopeStatus()
        {
            NotifyOfPropertyChange(() => ScopeCHAVoltageDIV);
            NotifyOfPropertyChange(() => ScopeCHBVoltageDIV);
            NotifyOfPropertyChange(() => ScopeCHACoupling);
            NotifyOfPropertyChange(() => ScopeCHBCoupling);
            NotifyOfPropertyChange(() => ScopeSampleRate);
            NotifyOfPropertyChange(() => ScopeTriggerModel);
            NotifyOfPropertyChange(() => ScopeTriggerEdge);

        }

        #region 配置参数

        /// <summary>
        /// 电压档位
        /// </summary>
        public ObservableCollection<string> ScopeVoltageDIVCollection { get; set; }

        /// <summary>
        /// 耦合集合
        /// </summary>
        public ObservableCollection<string> ScopeCouplingCollection { get; set; }

        /// <summary>
        /// 采样率
        /// </summary>
        public ObservableCollection<string> ScopeSampleRateCollection { get; set; }

        /// <summary>
        /// 触发模式
        /// </summary>
        public ObservableCollection<string> ScopeTriggerModelCollection { get; set; }

        /// <summary>
        /// 触发边沿
        /// </summary>
        public ObservableCollection<string> ScopeTriggerEdgeCollection { get; set; }

        /// <summary>
        /// 示波器CHA使能标志
        /// </summary>
        public bool ScopeCHAEnable
        {
            get
            {
                return IsScopeValid;
            }
            set
            {
                NotifyOfPropertyChange(() => ScopeCHAEnable);
            }
        }

        /// <summary>
        /// 示波器CHB使能标志
        /// </summary>
        public bool ScopeCHBEnable
        {
            get
            {
                return Scope?.IsCHBEnable ?? false;
            }
            set
            {
                if (IsScopeValid)
                {
                    Scope.IsCHBEnable = value;
                }
                NotifyOfPropertyChange(() => ScopeCHBEnable);
            }
        }

        /// <summary>
        /// CHA电压档位
        /// </summary>
        public string ScopeCHAVoltageDIV
        {
            get
            {
                if (IsScopeValid)
                {
                    return EnumHelper.GetDescription(Scope.CHAVoltageDIV);
                }
                return "";
            }
            set
            {
                if (IsScopeValid)
                {
                    Scope.CHAVoltageDIV = EnumHelper.GetEnum<LOTOA02.EVoltageDIV>(value);
                }
                NotifyOfPropertyChange(() => ScopeCHAVoltageDIV);
            }
        }

        /// <summary>
        /// CHB电压档位
        /// </summary>
        public string ScopeCHBVoltageDIV
        {
            get
            {
                if (IsScopeValid)
                {
                    return EnumHelper.GetDescription(Scope.CHBVoltageDIV);
                }
                return "";
            }
            set
            {
                if (IsScopeValid)
                {
                    Scope.CHBVoltageDIV = EnumHelper.GetEnum<LOTOA02.EVoltageDIV>(value);
                }
                NotifyOfPropertyChange(() => ScopeCHAVoltageDIV);
            }
        }

        /// <summary>
        /// CHA耦合
        /// </summary>
        public string ScopeCHACoupling
        {
            get
            {
                if (IsScopeValid)
                {
                    return EnumHelper.GetDescription(Scope.CHACoupling);
                }
                return "";
            }
            set
            {
                if (IsScopeValid)
                {
                    Scope.CHACoupling = EnumHelper.GetEnum<LOTOA02.ECoupling>(value);
                }
                NotifyOfPropertyChange(() => ScopeCHACoupling);
            }
        }

        /// <summary>
        /// CHB耦合
        /// </summary>
        public string ScopeCHBCoupling
        {
            get
            {
                if (IsScopeValid)
                {
                    return EnumHelper.GetDescription(Scope.CHBCoupling);
                }
                return "";
            }
            set
            {
                if (IsScopeValid)
                {
                    Scope.CHBCoupling = EnumHelper.GetEnum<LOTOA02.ECoupling>(value);
                }
                NotifyOfPropertyChange(() => ScopeCHBCoupling);
            }
        }

        /// <summary>
        /// 采样率
        /// </summary>
        public string ScopeSampleRate
        {
            get
            {
                if (IsScopeValid)
                {
                    return EnumHelper.GetDescription(Scope.SampleRate);
                }
                return "";
            }
            set
            {
                if (IsScopeValid)
                {
                    Scope.SampleRate = EnumHelper.GetEnum<LOTOA02.ESampleRate>(value);
                }
                NotifyOfPropertyChange(() => ScopeSampleRate);
            }
        }

        /// <summary>
        /// 触发模式
        /// </summary>
        public string ScopeTriggerModel
        {
            get
            {
                if (IsScopeValid)
                {
                    return EnumHelper.GetDescription(Scope.TriggerModel);
                }
                return "";
            }
            set
            {
                if (IsScopeValid)
                {
                    Scope.TriggerModel = EnumHelper.GetEnum<LOTOA02.ETriggerModel>(value);
                }
                NotifyOfPropertyChange(() => ScopeTriggerModel);
            }
        }

        /// <summary>
        /// 触发沿
        /// </summary>
        public string ScopeTriggerEdge
        {
            get
            {
                if (IsScopeValid)
                {
                    return EnumHelper.GetDescription(Scope.TriggerEdge);
                }
                return "";
            }
            set
            {
                if (IsScopeValid)
                {
                    Scope.TriggerEdge = EnumHelper.GetEnum<LOTOA02.ETriggerEdge>(value);
                }
                NotifyOfPropertyChange(() => ScopeTriggerEdge);
            }
        }

        #endregion

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

        private object plcLock = new object();

        private bool isPLCThreadRunning = false;

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
            UpdatePLCStatus();
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void DisconnectPLC()
        {
            PLC?.Disconnect();
            UpdatePLCStatus();
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

                //NotifyOfPropertyChange(() => PLCVoltage);
                UpdatePLCStatus();
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

                //NotifyOfPropertyChange(() => PLCCurrent);
                UpdatePLCStatus();
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

                //NotifyOfPropertyChange(() => PLCEnable);
                UpdatePLCStatus();
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
            lock (plcLock)
            {
                if (isPLCThreadRunning)
                {
                    return;
                }
            }

            new Thread(() =>
            {
                lock (plcLock)
                {
                    isPLCThreadRunning = true;
                }

                for (int i = 0; i < 20; i++)
                {
                    Thread.Sleep(50);
                    NotifyOfPropertyChange(() => IsPLCValid);
                    NotifyOfPropertyChange(() => PLCVoltage);
                    NotifyOfPropertyChange(() => PLCCurrent);
                    NotifyOfPropertyChange(() => PLCEnable);
                    NotifyOfPropertyChange(() => PLCRealityVoltage);
                    NotifyOfPropertyChange(() => PLCRealityCurrent);
                    NotifyOfPropertyChange(() => PLCRealityTemperature);
                }

                lock (plcLock)
                {
                    isPLCThreadRunning = false;
                }

            }).Start();
        }

        /// <summary>
        /// 使能PLC输出
        /// </summary>
        public void EnablePLC()
        {
            if (IsPLCValid)
            {
                PLCEnable = !PLCEnable;
            }
        }

        #endregion

        #endregion

        #region PWM

        /// <summary>
        /// PWM
        /// </summary>
        IPWM PWM { get; set; }

        /// <summary>
        /// PWM有效标志
        /// </summary>
        public bool IsPWMValid
        {
            get
            {
                return PWM?.IsConnect ?? false;
            }
        }

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

        /// <summary>
        /// 连接到PLC
        /// </summary>
        public void ConnectPWM()
        {
            PWM?.Connect();
            UpdatePWMStatus();
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void DisconnectPWM()
        {
            PWM?.Disconnect();
            UpdatePWMStatus();
        }


        /// <summary>
        /// 更新PLC状态
        /// </summary>
        public void UpdatePWMStatus()
        {
            NotifyOfPropertyChange(() => IsPWMValid);
            NotifyOfPropertyChange(() => PWMSerialPort);
            NotifyOfPropertyChange(() => PWMFrequency);
            NotifyOfPropertyChange(() => PWMDutyRatio);
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

                UpdatePWMStatus();
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

                UpdatePWMStatus();
            }
        }

        #endregion

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

        private string runningMessage;

        /// <summary>
        /// 运行信息
        /// </summary>
        public string RunningMessage
        {
            get
            {
                return runningMessage;
            }
            set
            {
                runningMessage = value;
                NotifyOfPropertyChange(() => RunningMessage);
            }
        }

        /// <summary>
        /// 增加运行信息
        /// </summary>
        public void AddRunningMessage(string message)
        {
            RunningMessage += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}:\r\n{message}\r\n\r\n";

        }

        /// <summary>
        /// 清除运行信息
        /// </summary>
        public void ClearRunningMessage()
        {
            RunningMessage = "";

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
