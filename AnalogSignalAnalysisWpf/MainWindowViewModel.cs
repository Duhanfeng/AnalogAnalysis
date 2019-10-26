using AnalogSignalAnalysisWpf.Core;
using AnalogSignalAnalysisWpf.Hardware.PLC;
using AnalogSignalAnalysisWpf.Hardware.PWM;
using AnalogSignalAnalysisWpf.Hardware.Scope;
using AnalogSignalAnalysisWpf.LiveData;
using Caliburn.Micro;
using DataAnalysis;
using Framework.Infrastructure.Serialization;
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
        #region 构造、析构接口

        /// <summary>
        /// 创建MainWindowViewModel新实例
        /// </summary>
        public MainWindowViewModel()
        {
            //throw new Exception();
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

            //获取系统参数
            SystemParamManager = SystemParamManager.GetInstance();
            if (SystemParamManager.LoadParams())
            {
                AddRunningMessage("加载参数成功");
            }
            else
            {
                AddRunningMessage("无有效配置参数，已创建默认配置文件");
            }

            //更新串口
            SerialPorts = new ObservableCollection<string>(SerialPort.GetPortNames());

            //恢复硬件参数
            Scope = new LOTOA02();
            if (Scope.Connect())
            {
                AddRunningMessage("打开示波器成功");
                Scope.IsCHBEnable = SystemParamManager.SystemParam.ScopeParams.IsCHBEnable;
                Scope.CHAScale = SystemParamManager.SystemParam.ScopeParams.CHAScale;
                Scope.CHBScale = SystemParamManager.SystemParam.ScopeParams.CHBScale;
                Scope.CHAVoltageDIV = SystemParamManager.SystemParam.ScopeParams.CHAVoltageDIV;
                Scope.CHBVoltageDIV = SystemParamManager.SystemParam.ScopeParams.CHBVoltageDIV;
                Scope.CHACoupling = SystemParamManager.SystemParam.ScopeParams.CHACoupling;
                Scope.CHBCoupling = SystemParamManager.SystemParam.ScopeParams.CHBCoupling;
                Scope.SampleRate = SystemParamManager.SystemParam.ScopeParams.SampleRate;
                Scope.SampleTime = SystemParamManager.SystemParam.ScopeParams.SampleTime;
                Scope.TriggerModel = SystemParamManager.SystemParam.ScopeParams.TriggerModel;
                Scope.TriggerEdge = SystemParamManager.SystemParam.ScopeParams.TriggerEdge;
                //Scope.IsCHAEnable = SystemParamManager.SystemParam.ScopeParams.IsCHAEnable;
            }
            else
            {
                AddRunningMessage("打开示波器失败");
            }

            PLC = new ModbusPLC();
            if (SerialPorts.Contains(SystemParamManager.SystemParam.PLCParams.PrimarySerialPortName))
            {
                //配置通信参数
                PLC.PrimarySerialPortName = SystemParamManager.SystemParam.PLCParams.PrimarySerialPortName;
                PLC.SerialPortBaudRate = SystemParamManager.SystemParam.PLCParams.SerialPortBaudRate;
                PLC.SlaveAddress = SystemParamManager.SystemParam.PLCParams.SlaveAddress;
                PLC.ReadTimeout = SystemParamManager.SystemParam.PLCParams.ReadTimeout;
                PLC.WriteTimeout = SystemParamManager.SystemParam.PLCParams.WriteTimeout;

                //连接设备
                if (PLC.Connect())
                {
                    AddRunningMessage("连接PLC成功");
                    PLC.Voltage = SystemParamManager.SystemParam.PLCParams.Voltage;
                    PLC.Current = SystemParamManager.SystemParam.PLCParams.Current;
                    PLC.EnableOutput = SystemParamManager.SystemParam.PLCParams.EnableOutput;
                }
                else
                {
                    AddRunningMessage("连接PLC失败");
                }
            }
            else
            {
                AddRunningMessage($"无有效端口[{SystemParamManager.SystemParam.PWMParams.PrimarySerialPortName ?? "Null"}],连接PLC失败");
            }

            PWM = new SerialPortPWM();
            if (SerialPorts.Contains(SystemParamManager.SystemParam.PWMParams.PrimarySerialPortName))
            {
                //配置通信参数
                PWM.PrimarySerialPortName = SystemParamManager.SystemParam.PWMParams.PrimarySerialPortName;

                //连接设备
                if (PWM.Connect())
                {
                    AddRunningMessage("连接PWM成功");
                    PWM.Frequency = SystemParamManager.SystemParam.PWMParams.Frequency;
                    PWM.DutyRatio = SystemParamManager.SystemParam.PWMParams.DutyRatio;
                }
                else
                {
                    AddRunningMessage("连接PWM失败");
                }
            }
            else
            {
                AddRunningMessage($"无有效端口[{SystemParamManager.SystemParam.PWMParams.PrimarySerialPortName ?? "Null"}],连接PWM失败");
            }

            //创建示波器控件实例
            ScopeControlView = new ScopeControlView2();
            ScopeControlView.DataContext = this;

            ScopeScale = new ObservableCollection<string>(EnumHelper.GetAllDescriptions<EScale>());
            ScopeCouplingCollection = new ObservableCollection<string>(EnumHelper.GetAllDescriptions<ECoupling>());
            ScopeSampleRateCollection = new ObservableCollection<string>(EnumHelper.GetAllDescriptions<ESampleRate>());
            ScopeTriggerModelCollection = new ObservableCollection<string>(EnumHelper.GetAllDescriptions<ETriggerModel>());
            ScopeTriggerEdgeCollection = new ObservableCollection<string>(EnumHelper.GetAllDescriptions<ETriggerEdge>());
            ScopeVoltageDIVCollection = new ObservableCollection<string>(EnumHelper.GetAllDescriptions<EVoltageDIV>());

            //创建PLC控件实例
            PLCControlView = new PLCControlView();
            PLCControlView.DataContext = this;

            //创建PWM控件实例
            PWMControlView = new PWMControlView();
            PWMControlView.DataContext = this;

            //设置窗口
            SettingsView = new SettingsView();
            SettingsView.DataContext = this;

            //设置测试model实例
            FrequencyMeasurementViewModel = new FrequencyMeasurementViewModel(Scope, PLC, PWM);
            InputOutputMeasurementViewModel = new InputOutputMeasurementViewModel(Scope, PLC, PWM);
            PNVoltageMeasurementViewModel = new PNVoltageMeasurementViewModel(Scope, PLC, PWM);
            ThroughputMeasurementViewModel = new ThroughputMeasurementViewModel(Scope, PLC, PWM);

            FrequencyMeasurementViewModel.MeasurementStarted += MeasurementViewModel_MeasurementStarted;
            InputOutputMeasurementViewModel.MeasurementStarted += MeasurementViewModel_MeasurementStarted;
            PNVoltageMeasurementViewModel.MeasurementStarted += MeasurementViewModel_MeasurementStarted;
            ThroughputMeasurementViewModel.MeasurementStarted += MeasurementViewModel_MeasurementStarted;

            FrequencyMeasurementViewModel.MeasurementCompleted += FrequencyMeasurementViewModel_MeasurementCompleted;
            InputOutputMeasurementViewModel.MeasurementCompleted += InputOutputMeasurementViewModel_MeasurementCompleted;
            PNVoltageMeasurementViewModel.MeasurementCompleted += PNVoltageMeasurementViewModel_MeasurementCompleted;
            ThroughputMeasurementViewModel.MeasurementCompleted += ThroughputMeasurementViewModel_MeasurementCompleted;

            FrequencyMeasurementViewModel.MessageRaised += FrequencyMeasurementViewModel_MessageRaised;
            InputOutputMeasurementViewModel.MessageRaised += InputOutputMeasurementViewModel_MessageRaised;
            PNVoltageMeasurementViewModel.MessageRaised += PNVoltageMeasurementViewModel_MessageRaised;
            ThroughputMeasurementViewModel.MessageRaised += ThroughputMeasurementViewModel_MessageRaised;

        }

        private void ThroughputMeasurementViewModel_MeasurementCompleted(object sender, ThroughputMeasurementCompletedEventArgs e)
        {
            IsMeasuring = false;
        }

        private void PNVoltageMeasurementViewModel_MeasurementCompleted(object sender, PNVoltageMeasurementCompletedEventArgs e)
        {
            IsMeasuring = false;
        }

        private void InputOutputMeasurementViewModel_MeasurementCompleted(object sender, InputOutputMeasurementCompletedEventArgs e)
        {
            IsMeasuring = false;
        }

        private void FrequencyMeasurementViewModel_MeasurementCompleted(object sender, FrequencyMeasurementCompletedEventArgs e)
        {
            IsMeasuring = false;
        }

        private void MeasurementViewModel_MeasurementStarted(object sender, EventArgs e)
        {
            IsMeasuring = true;
        }

        private void FrequencyMeasurementViewModel_MessageRaised(object sender, MessageRaisedEventArgs e)
        {
            AddRunningMessage(e.Message);
        }

        private void InputOutputMeasurementViewModel_MessageRaised(object sender, MessageRaisedEventArgs e)
        {
            AddRunningMessage(e.Message);
        }

        private void PNVoltageMeasurementViewModel_MessageRaised(object sender, MessageRaisedEventArgs e)
        {
            AddRunningMessage(e.Message);
        }

        private void ThroughputMeasurementViewModel_MessageRaised(object sender, MessageRaisedEventArgs e)
        {
            AddRunningMessage(e.Message);
        }

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

        #endregion

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
        public IScopeBase Scope { get; set; }

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
            
            if (IsScopeValid)
            {
                //还原配置
                Scope.CHACoupling = SystemParamManager.SystemParam.ScopeParams.CHACoupling;
                Scope.CHBCoupling = SystemParamManager.SystemParam.ScopeParams.CHBCoupling;
                Scope.CHAVoltageDIV = SystemParamManager.SystemParam.ScopeParams.CHAVoltageDIV;
                Scope.CHBVoltageDIV = SystemParamManager.SystemParam.ScopeParams.CHBVoltageDIV;
                Scope.IsCHBEnable = SystemParamManager.SystemParam.ScopeParams.IsCHBEnable;
                Scope.SampleRate = SystemParamManager.SystemParam.ScopeParams.SampleRate;
                Scope.TriggerModel = SystemParamManager.SystemParam.ScopeParams.TriggerModel;
                Scope.TriggerEdge = SystemParamManager.SystemParam.ScopeParams.TriggerEdge;

                AddRunningMessage("连接示波器成功");
            }
            else
            {
                AddRunningMessage("连接示波器失败");
            }

            UpdateScopeStatus();
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void DisconnectScope()
        {
            Scope?.Disconnect();
            UpdateScopeStatus();
            AddRunningMessage("断开示波器连接");
        }

        /// <summary>
        /// 连接/断开连接
        /// </summary>
        public void ConnectOrDisconnectScope()
        {
            if (IsScopeValid)
            {
                DisconnectScope();
            }
            else
            {
                ConnectScope();
            }

        }

        /// <summary>
        /// 更新示波器状态
        /// </summary>
        public void UpdateScopeStatus()
        {
            NotifyOfPropertyChange(() => IsScopeValid);
            NotifyOfPropertyChange(() => ScopeCHAVoltageDIV);
            NotifyOfPropertyChange(() => ScopeCHBVoltageDIV);
            NotifyOfPropertyChange(() => ScopeCHACoupling);
            NotifyOfPropertyChange(() => ScopeCHBCoupling);
            NotifyOfPropertyChange(() => ScopeSampleRate);
            NotifyOfPropertyChange(() => ScopeTriggerModel);
            NotifyOfPropertyChange(() => ScopeTriggerEdge);
            NotifyOfPropertyChange(() => ScopeCHAEnable);
            NotifyOfPropertyChange(() => ScopeCHBEnable);

        }

        private readonly int SampleInterval = 1;

        /// <summary>
        /// 读取数据
        /// </summary>
        public void StartReadScopeData()
        {
            if (IsScopeValid)
            {
                double[] channelAData;
                double[] channelBData;
                Scope.ReadDataBlock(out channelAData, out channelBData);

                //设置通道A数据
                //数据滤波
                double[] filterData;
                Analysis.MeanFilter(channelAData, 11, out filterData);
                ScopeAverageCHA = Analysis.Mean(filterData);

                //显示数据
                var collection = new ObservableCollection<Data>();
                for (int i = 0; i < channelAData.Length / SampleInterval; i++)
                {
                    collection.Add(new Data() { Value1 = filterData[i * SampleInterval], Value = i * 1000.0 / ((int)Scope.SampleRate) * SampleInterval });
                }
                ScopeCHACollection = collection;

                if (ScopeCHBEnable)
                {
                    Analysis.MeanFilter(channelBData, 11, out filterData);
                    ScopeAverageCHB = Analysis.Mean(filterData);

                    //显示通道B数据
                    collection = new ObservableCollection<Data>();
                    for (int i = 0; i < channelBData.Length / SampleInterval; i++)
                    {
                        collection.Add(new Data() { Value1 = filterData[i * SampleInterval], Value = i * 1000.0 / ((int)Scope.SampleRate) * SampleInterval });
                    }
                    ScopeChBCollection = collection;
                }
                else
                {
                    ScopeAverageCHB = -1;
                    ScopeChBCollection = new ObservableCollection<Data>();
                }

            }
        }

        #region 配置参数

        /// <summary>
        /// 放大倍数
        /// </summary>
        public ObservableCollection<string> ScopeScale { get; set; }

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
                if (IsScopeValid)
                {
                    return Scope.IsCHBEnable;
                }
                return false;
            }
            set
            {
                if (IsScopeValid)
                {
                    Scope.IsCHBEnable = value;
                    SystemParamManager.SystemParam.ScopeParams.IsCHBEnable = Scope.IsCHBEnable;
                    SystemParamManager.SaveParams();
                }
                NotifyOfPropertyChange(() => ScopeCHBEnable);
            }
        }

        /// <summary>
        /// CHA档位
        /// </summary>
        public string ScopeCHAScale
        {
            get
            {
                if (IsScopeValid)
                {
                    return EnumHelper.GetDescription(Scope.CHAScale);
                }
                return "";
            }
            set
            {
                if (IsScopeValid)
                {
                    Scope.CHAScale = EnumHelper.GetEnum<EScale>(value);
                    SystemParamManager.SystemParam.ScopeParams.CHAScale = Scope.CHAScale;
                    SystemParamManager.SaveParams();
                }
                NotifyOfPropertyChange(() => ScopeCHAScale);
            }
        }

        /// <summary>
        /// 示波器CHB倍数
        /// </summary>
        public string ScopeCHBScale
        {
            get
            {
                if (IsScopeValid)
                {
                    return EnumHelper.GetDescription(Scope.CHBScale);
                }
                return "";
            }
            set
            {
                if (IsScopeValid)
                {
                    Scope.CHBScale = EnumHelper.GetEnum<EScale>(value);
                    SystemParamManager.SystemParam.ScopeParams.CHBScale = Scope.CHBScale;
                    SystemParamManager.SaveParams();
                }
                NotifyOfPropertyChange(() => ScopeCHBScale);
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
                    Scope.CHAVoltageDIV = EnumHelper.GetEnum<EVoltageDIV>(value);
                    SystemParamManager.SystemParam.ScopeParams.CHAVoltageDIV = Scope.CHAVoltageDIV;
                    SystemParamManager.SaveParams();
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
                    Scope.CHBVoltageDIV = EnumHelper.GetEnum<EVoltageDIV>(value);
                    SystemParamManager.SystemParam.ScopeParams.CHBVoltageDIV = Scope.CHBVoltageDIV;
                    SystemParamManager.SaveParams();
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
                    Scope.CHACoupling = EnumHelper.GetEnum<ECoupling>(value);
                    SystemParamManager.SystemParam.ScopeParams.CHACoupling = Scope.CHACoupling;
                    SystemParamManager.SaveParams();
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
                    Scope.CHBCoupling = EnumHelper.GetEnum<ECoupling>(value);
                    SystemParamManager.SystemParam.ScopeParams.CHBCoupling = Scope.CHBCoupling;
                    SystemParamManager.SaveParams();
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
                    Scope.SampleRate = EnumHelper.GetEnum<ESampleRate>(value);
                    SystemParamManager.SystemParam.ScopeParams.SampleRate = Scope.SampleRate;
                    SystemParamManager.SaveParams();
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
                    Scope.TriggerModel = EnumHelper.GetEnum<ETriggerModel>(value);
                    SystemParamManager.SystemParam.ScopeParams.TriggerModel = Scope.TriggerModel;
                    SystemParamManager.SaveParams();
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
                    Scope.TriggerEdge = EnumHelper.GetEnum<ETriggerEdge>(value);
                    SystemParamManager.SystemParam.ScopeParams.TriggerEdge = Scope.TriggerEdge;
                    SystemParamManager.SaveParams();
                }
                NotifyOfPropertyChange(() => ScopeTriggerEdge);
            }
        }

        /// <summary>
        /// 采样时间
        /// </summary>
        public int ScopeSampleTime
        {
            get
            {
                if (IsScopeValid)
                {
                    return Scope.SampleTime;
                }
                return -1;
            }
            set
            {
                if (IsScopeValid)
                {
                    Scope.SampleTime = value;
                    SystemParamManager.SystemParam.ScopeParams.SampleTime = Scope.SampleTime;
                    SystemParamManager.SaveParams();
                }
                NotifyOfPropertyChange(() => ScopeSampleTime);
            }
        }

        #endregion

        #region 显示参数

        private ObservableCollection<Data> scopeChACollection;

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

        private ObservableCollection<Data> scopeChBCollection;

        /// <summary>
        /// 通道B数据
        /// </summary>
        public ObservableCollection<Data> ScopeChBCollection
        {
            get
            {
                return scopeChBCollection;
            }
            set
            {
                scopeChBCollection = value;
                NotifyOfPropertyChange(() => ScopeChBCollection);
            }
        }

        private double scopeAverageCHA;

        /// <summary>
        /// 通道A平均值
        /// </summary>
        public double ScopeAverageCHA
        {
            get
            {
                return scopeAverageCHA;
            }
            set
            {
                scopeAverageCHA = value;
                NotifyOfPropertyChange(() => ScopeAverageCHA);
            }
        }

        private double scopeAverageCHB;

        /// <summary>
        /// 通道B平均值
        /// </summary>
        public double ScopeAverageCHB
        {
            get
            {
                return scopeAverageCHB;
            }
            set
            {
                scopeAverageCHB = value;
                NotifyOfPropertyChange(() => ScopeAverageCHB);
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

            if (IsPLCValid)
            {
                //将当前通信配置保存到系统参数中
                SystemParamManager.SystemParam.PLCParams.PrimarySerialPortName = PLCSerialPort;
                SystemParamManager.SystemParam.PLCParams.SerialPortBaudRate = PLCSerialPortBaudRate;
                SystemParamManager.SystemParam.PLCParams.SlaveAddress = PLCSlaveAddress;
                SystemParamManager.SystemParam.PLCParams.ReadTimeout = PLCTimeout;
                SystemParamManager.SystemParam.PLCParams.WriteTimeout = PLCTimeout;
                SystemParamManager.SaveParams();

                AddRunningMessage("连接PLC成功");
            }
            else
            {
                AddRunningMessage("连接PLC失败");
            }

        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void DisconnectPLC()
        {
            PLC?.Disconnect();
            UpdatePLCStatus();
            AddRunningMessage("断开PLC连接");
        }

        /// <summary>
        /// 连接/断开连接
        /// </summary>
        public void ConnectOrDisconnectPLC()
        {
            if (IsPLCValid)
            {
                DisconnectPLC();
            }
            else
            {
                ConnectPLC();
            }

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
                    SystemParamManager.SystemParam.PLCParams.Voltage = value;
                    SystemParamManager.SaveParams();
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
                    SystemParamManager.SystemParam.PLCParams.Current = value;
                    SystemParamManager.SaveParams();
                }

                //NotifyOfPropertyChange(() => PLCCurrent);
                UpdatePLCStatus();
            }
        }

        /// <summary>
        /// PLC使能输出
        /// </summary>
        public bool PLCEnableOutput
        {
            get
            {
                if (IsPLCValid)
                {
                    return PLC.EnableOutput;
                }
                return false;
            }
            set
            {
                if (IsPLCValid == true)
                {
                    PLC.EnableOutput = value;
                    SystemParamManager.SystemParam.PLCParams.EnableOutput = value;
                    SystemParamManager.SaveParams();
                }

                //NotifyOfPropertyChange(() => PLCEnableOutput);
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
            NotifyOfPropertyChange(() => IsPLCValid);
            NotifyOfPropertyChange(() => PLCVoltage);
            NotifyOfPropertyChange(() => PLCCurrent);
            NotifyOfPropertyChange(() => PLCEnableOutput);
            NotifyOfPropertyChange(() => PLCRealityVoltage);
            NotifyOfPropertyChange(() => PLCRealityCurrent);
            NotifyOfPropertyChange(() => PLCRealityTemperature);

            //lock (plcLock)
            //{
            //    if (isPLCThreadRunning)
            //    {
            //        return;
            //    }
            //}

            //new Thread(() =>
            //{
            //    lock (plcLock)
            //    {
            //        isPLCThreadRunning = true;
            //    }

            //    for (int i = 0; i < 20; i++)
            //    {
            //        Thread.Sleep(50);
            //        NotifyOfPropertyChange(() => IsPLCValid);
            //        NotifyOfPropertyChange(() => PLCVoltage);
            //        NotifyOfPropertyChange(() => PLCCurrent);
            //        NotifyOfPropertyChange(() => PLCEnableOutput);
            //        NotifyOfPropertyChange(() => PLCRealityVoltage);
            //        NotifyOfPropertyChange(() => PLCRealityCurrent);
            //        NotifyOfPropertyChange(() => PLCRealityTemperature);
            //    }

            //    lock (plcLock)
            //    {
            //        isPLCThreadRunning = false;
            //    }

            //}).Start();
        }

        /// <summary>
        /// 使能PLC输出
        /// </summary>
        public void EnablePLCOutput()
        {
            if (IsPLCValid)
            {
                PLCEnableOutput = !PLCEnableOutput;
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

            if (IsPWMValid)
            {
                //将当前通信配置保存到系统参数中
                SystemParamManager.SystemParam.PWMParams.PrimarySerialPortName = PWMSerialPort;
                SystemParamManager.SaveParams();

                AddRunningMessage("连接PWM成功");
            }
            else
            {
                AddRunningMessage("连接PWM失败");
            }
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void DisconnectPWM()
        {
            PWM?.Disconnect();
            UpdatePWMStatus();
            AddRunningMessage("断开PWM连接");
        }

        /// <summary>
        /// 连接/断开连接
        /// </summary>
        public void ConnectOrDisconnectPWM()
        {
            if (IsPWMValid)
            {
                DisconnectPWM();
            }
            else
            {
                ConnectPWM();
            }

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
                if (IsPWMValid)
                {
                    return PWM.Frequency;
                }
                return -1;
            }
            set
            {
                if (IsPWMValid)
                {
                    PWM.Frequency = value;
                    SystemParamManager.SystemParam.PWMParams.Frequency = value;
                    SystemParamManager.SaveParams();
                }

                //NotifyOfPropertyChange(() => PWMFrequency);
                UpdatePWMStatus();
            }
        }

        /// <summary>
        /// PWM占空比
        /// </summary>
        public int PWMDutyRatio
        {
            get
            {
                if (IsPWMValid)
                {
                    return PWM.DutyRatio;
                }
                return -1;
            }
            set
            {
                if (IsPWMValid)
                {
                    PWM.DutyRatio = value;
                    SystemParamManager.SystemParam.PWMParams.DutyRatio = value;
                    SystemParamManager.SaveParams();
                }

                //NotifyOfPropertyChange(() => PWMDutyRatio);
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

        private bool isMeasuring = false;

        /// <summary>
        /// 测量标志
        /// </summary>
        public bool IsMeasuring
        {
            get
            {
                return isMeasuring;
            }
            set
            {
                isMeasuring = value;
                NotifyOfPropertyChange(() => IsMeasuring);
            }
        }

        /// <summary>
        /// 频率测试Model
        /// </summary>
        public FrequencyMeasurementViewModel FrequencyMeasurementViewModel { get; set; }

        /// <summary>
        /// 输入输出测试Model
        /// </summary>
        public InputOutputMeasurementViewModel InputOutputMeasurementViewModel { get; set; }

        /// <summary>
        /// 吸合释放电压测试Model
        /// </summary>
        public PNVoltageMeasurementViewModel PNVoltageMeasurementViewModel { get; set; }

        /// <summary>
        /// 通气量测试Model
        /// </summary>
        public ThroughputMeasurementViewModel ThroughputMeasurementViewModel { get; set; }

        #endregion

        #region 参数

        /// <summary>
        /// 系统参数管理器
        /// </summary>
        public SystemParamManager SystemParamManager { get; private set; }

        /// <summary>
        /// 保存系统参数
        /// </summary>
        public void SaveSystemParam()
        {
            SystemParamManager.SaveParams();
            var serial = JsonSerialization.SerializeObject(SystemParamManager.SystemParam);
            AddRunningMessage($"保存配置文件成功:\r\n{serial}");
        }

        /// <summary>
        /// 加载系统参数
        /// </summary>
        /// <param name="file"></param>
        public void LoadSystemParam(string file)
        {
            if (SystemParamManager.LoadParams(file))
            {
                var serial = JsonSerialization.SerializeObject(SystemParamManager.SystemParam);
                AddRunningMessage($"加载配置文件({file})成功:\r\n{serial}");
            }
            else
            {
                AddRunningMessage($"加载配置文件({file})失败,已还原为默认参数");
            }

        }

        #endregion

    }
}
