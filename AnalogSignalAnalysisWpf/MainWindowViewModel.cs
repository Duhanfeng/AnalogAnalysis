using AnalogSignalAnalysisWpf.Core;
using AnalogSignalAnalysisWpf.Hardware;
using AnalogSignalAnalysisWpf.Hardware.Scope;
using AnalogSignalAnalysisWpf.LiveData;
using Caliburn.Micro;
using CsvHelper;
using DataAnalysis;
using Framework.Infrastructure.Serialization;
using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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

        private object measureLock = new object();

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

            Power = new ModbusPower();
            if (SerialPorts.Contains(SystemParamManager.SystemParam.PowerParams.PrimarySerialPortName))
            {
                //配置通信参数
                Power.PrimarySerialPortName = SystemParamManager.SystemParam.PowerParams.PrimarySerialPortName;
                Power.SerialPortBaudRate = SystemParamManager.SystemParam.PowerParams.SerialPortBaudRate;
                Power.SlaveAddress = SystemParamManager.SystemParam.PowerParams.SlaveAddress;
                Power.ReadTimeout = SystemParamManager.SystemParam.PowerParams.ReadTimeout;
                Power.WriteTimeout = SystemParamManager.SystemParam.PowerParams.WriteTimeout;

                //连接设备
                if (Power.Connect())
                {
                    AddRunningMessage("连接Power成功");
                    Power.IsEnableOutput = false;
                    Power.Voltage = SystemParamManager.SystemParam.PowerParams.Voltage;
                    Power.Current = SystemParamManager.SystemParam.PowerParams.Current;
                    //Power.IsEnableOutput = SystemParamManager.SystemParam.PowerParams.IsEnableOutput;
                }
                else
                {
                    AddRunningMessage("连接Power失败");
                }
            }
            else
            {
                AddRunningMessage($"无有效端口[{SystemParamManager.SystemParam.PLCParams.PrimarySerialPortName ?? "Null"}],连接Power失败");
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
                    PLC.PWMSwitch = false;
                    PLC.FlowSwitch = false;
                    AddRunningMessage("连接PLC成功");
                }
                else
                {
                    AddRunningMessage("连接PLC失败");
                }
            }
            else
            {
                AddRunningMessage($"无有效端口[{SystemParamManager.SystemParam.PLCParams.PrimarySerialPortName ?? "Null"}],连接PLC失败");
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

            //创建Power控件实例
            PowerControlView = new PowerControlView();
            PowerControlView.DataContext = this;

            //创建PLC控件实例
            PLCControlView = new PLCControlView();
            PLCControlView.DataContext = this;

            //设置窗口
            SettingsView = new SettingsView();
            SettingsView.DataContext = this;

            //设置测试model实例
            FrequencyMeasurementViewModel = new FrequencyMeasurementViewModel(Scope, Power, PLC, PWM);
            InputOutputMeasurementViewModel = new InputOutputMeasurementViewModel(Scope, Power, PLC, PWM);
            NewIOMeasurementViewModel = new NewIOMeasurementViewModel(Scope, Power, PLC, PWM);
            PNVoltageMeasurementViewModel = new PNVoltageMeasurementViewModel(Scope, Power, PLC, PWM);
            ThroughputMeasurementViewModel = new ThroughputMeasurementViewModel(Scope, Power, PLC, PWM);
            BurnInTestViewModel = new BurnInTestViewModel(Scope, Power, PLC, PWM);
            FlowMeasureViewModel = new FlowMeasureViewModel(Scope, Power, PLC, PWM);

            FrequencyMeasurementViewModel.MeasurementStarted += MeasurementViewModel_MeasurementStarted;
            InputOutputMeasurementViewModel.MeasurementStarted += MeasurementViewModel_MeasurementStarted;
            NewIOMeasurementViewModel.MeasurementStarted += MeasurementViewModel_MeasurementStarted;
            PNVoltageMeasurementViewModel.MeasurementStarted += MeasurementViewModel_MeasurementStarted;
            ThroughputMeasurementViewModel.MeasurementStarted += MeasurementViewModel_MeasurementStarted;
            FlowMeasureViewModel.MeasurementStarted += MeasurementViewModel_MeasurementStarted;

            FrequencyMeasurementViewModel.MeasurementCompleted += FrequencyMeasurementViewModel_MeasurementCompleted;
            InputOutputMeasurementViewModel.MeasurementCompleted += InputOutputMeasurementViewModel_MeasurementCompleted;
            NewIOMeasurementViewModel.MeasurementCompleted += NewIOMeasurementViewModel_MeasurementCompleted;
            PNVoltageMeasurementViewModel.MeasurementCompleted += PNVoltageMeasurementViewModel_MeasurementCompleted;
            ThroughputMeasurementViewModel.MeasurementCompleted += ThroughputMeasurementViewModel_MeasurementCompleted;
            FlowMeasureViewModel.MeasurementCompleted += ThroughputMeasurementViewModel_MeasurementCompleted;

            FrequencyMeasurementViewModel.MessageRaised += FrequencyMeasurementViewModel_MessageRaised;
            InputOutputMeasurementViewModel.MessageRaised += InputOutputMeasurementViewModel_MessageRaised;
            NewIOMeasurementViewModel.MessageRaised += InputOutputMeasurementViewModel_MessageRaised;
            PNVoltageMeasurementViewModel.MessageRaised += PNVoltageMeasurementViewModel_MessageRaised;
            ThroughputMeasurementViewModel.MessageRaised += ThroughputMeasurementViewModel_MessageRaised;
            FlowMeasureViewModel.MessageRaised += ThroughputMeasurementViewModel_MessageRaised;

            //获取用户名
            SystemParamManager.LoadUser();

        }
        
        private void MeasurementViewModel_MeasurementStarted(object sender, EventArgs e)
        {
            lock (measureLock)
            {
                IsMeasuring = true;
            }

            if (sender is FrequencyMeasurementViewModel)
            {
                FrequencyMeasureStatus = "测试中";
            }
            else if (sender is PNVoltageMeasurementViewModel)
            {
                PNMeasureStatus = "测试中";
            }
            else if (sender is InputOutputMeasurementViewModel)
            {
                IOMeasureStatus = "测试中";
            }
            else if (sender is NewIOMeasurementViewModel)
            {
                IOMeasureStatus = "测试中";
            }
            else if (sender is ThroughputMeasurementViewModel)
            {
                FlowMeasureStatus = "测试中";
            }
            else if (sender is FlowMeasureViewModel)
            {
                FlowMeasureStatus = "测试中";
            }

            NotifyOfPropertyChange(() => IsEnableTest);

        }

        private void FrequencyMeasurementViewModel_MeasurementCompleted(object sender, FrequencyMeasurementCompletedEventArgs e)
        {
            lock (measureLock)
            {
                IsMeasuring = false;
            }

            if (e.IsSuccess)
            {
                FrequencyMeasureStatus = "测试完成";
                MaxLimitFrequency = e.MaxLimitFrequency;
            }
            else
            {
                FrequencyMeasureStatus = "测试失败";
                MaxLimitFrequency = -1;
            }

            NotifyOfPropertyChange(() => IsEnableTest);
        }

        private void PNVoltageMeasurementViewModel_MeasurementCompleted(object sender, PNVoltageMeasurementCompletedEventArgs e)
        {
            lock (measureLock)
            {
                IsMeasuring = false;
            }

            if (e.IsSuccess)
            {
                PNMeasureStatus = "测试完成";
                PositiveVoltage = e.PositiveVoltage;
                NegativeVoltage = e.NegativeVoltage;
            }
            else
            {
                PNMeasureStatus = "测试失败";
                PositiveVoltage = -1;
                NegativeVoltage = -1;
            }
            NotifyOfPropertyChange(() => IsEnableTest);
        }

        private void InputOutputMeasurementViewModel_MeasurementCompleted(object sender, InputOutputMeasurementCompletedEventArgs e)
        {
            lock (measureLock)
            {
                IsMeasuring = false;
            }

            new Thread(delegate ()
            {
                ThreadPool.QueueUserWorkItem(delegate
                {
                    System.Threading.SynchronizationContext.SetSynchronizationContext(new System.Windows.Threading.DispatcherSynchronizationContext(System.Windows.Application.Current.Dispatcher));
                    System.Threading.SynchronizationContext.Current.Send(pl =>
                    {
                        if (e.IsSuccess)
                        {
                            IOMeasureStatus = "测试完成";
                            InputOutputInfos = new ObservableCollection<InputOutputMeasurementInfo>(e.Infos);
                        }
                        else
                        {
                            IOMeasureStatus = "测试失败";
                            InputOutputInfos = new ObservableCollection<InputOutputMeasurementInfo>();
                        }
                    }, null);
                });
            }).Start();
            NotifyOfPropertyChange(() => IsEnableTest);
        }

        private void NewIOMeasurementViewModel_MeasurementCompleted(object sender, EventArgs e)
        {
            lock (measureLock)
            {
                IsMeasuring = false;
            }

            new Thread(delegate ()
            {
                ThreadPool.QueueUserWorkItem(delegate
                {
                    System.Threading.SynchronizationContext.SetSynchronizationContext(new System.Windows.Threading.DispatcherSynchronizationContext(System.Windows.Application.Current.Dispatcher));
                    System.Threading.SynchronizationContext.Current.Send(pl =>
                    {
                        //if (e.IsSuccess)
                        {
                            IOMeasureStatus = "测试完成";
                            //InputOutputInfos = new ObservableCollection<InputOutputMeasurementInfo>(e.Infos);
                        }

                    }, null);
                });
            }).Start();
            NotifyOfPropertyChange(() => IsEnableTest);
        }

        private void ThroughputMeasurementViewModel_MeasurementCompleted(object sender, ThroughputMeasurementCompletedEventArgs e)
        {
            lock (measureLock)
            {
                IsMeasuring = false;
            }

            if (e.IsSuccess)
            {
                FlowMeasureStatus = "测试完成";
                Flow = e.Flow;
            }
            else
            {
                FlowMeasureStatus = "测试失败";
                Flow = -1;
            }
            NotifyOfPropertyChange(() => IsEnableTest);
        }

        private void FrequencyMeasurementViewModel_MessageRaised(object sender, MessageRaisedEventArgs e)
        {
            AddRunningMessage(e.Message);
        }

        private void InputOutputMeasurementViewModel_MessageRaised(object sender, MessageRaisedEventArgs e)
        {
            OnMessageRaised(e.MessageLevel, e.Message);
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

        private PowerControlView powerControlView;

        /// <summary>
        /// Power配置控件
        /// </summary>
        public PowerControlView PowerControlView
        {
            get
            {
                return powerControlView;
            }
            set
            {
                powerControlView = value;
                NotifyOfPropertyChange(() => PowerControlView);
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

            NotifyOfPropertyChange(() => IsHardwareValid);
        }

        private readonly int SampleInterval = 1;

        /// <summary>
        /// 读取数据
        /// </summary>
        public void StartReadScopeData()
        {
            if (IsScopeValid)
            {
                ScopeSampleTime = ScopeSampleTime;

                if (Scope.SampleRate == ESampleRate.Sps_96K)
                {
                    Scope.ScopeReadDataCompleted -= Scope_ScopeReadDataCompleted;
                    Scope.ScopeReadDataCompleted += Scope_ScopeReadDataCompleted;

                    //开始连续采集
                    Scope.StartSerialSample(15000);
                }
                else
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
                        ScopeCHBCollection = collection;
                    }
                    else
                    {
                        ScopeAverageCHB = -1;
                        ScopeCHBCollection = new ObservableCollection<Data>();
                    }
                }

            }
        }

        private void Scope_ScopeReadDataCompleted(object sender, ScopeReadDataCompletedEventArgs e)
        {
            double[] globleChannel1;
            double[] globleChannel2;
            double[] currentChannel1;
            double[] currentChannel2;

            e.GetData(out globleChannel1, out globleChannel2, out currentChannel1, out currentChannel2);

            //设置通道A数据
            //数据滤波
            double[] filterData;
            Analysis.MeanFilter(currentChannel1, 11, out filterData);
            ScopeAverageCHA = Analysis.Mean(filterData);

            //显示数据
            ObservableCollection<Data> collectionA;

            if (e.CurrentPacket == 0)
            {
                collectionA = new ObservableCollection<Data>();
            }
            else
            {
                collectionA = new ObservableCollection<Data>(ScopeCHACollection);
            }

            for (int i = 0; i < currentChannel1.Length / SampleInterval; i++)
            {
                collectionA.Add(new Data() { Value1 = filterData[i * SampleInterval], Value = (e.CurrentPacket * currentChannel1.Length + i) * 1000.0 / ((int)Scope.SampleRate) * SampleInterval });
            }

            ObservableCollection<Data> collectionB = new ObservableCollection<Data>();

            if (ScopeCHBEnable)
            {
                Analysis.MeanFilter(currentChannel2, 11, out filterData);
                ScopeAverageCHB = Analysis.Mean(filterData);

                if (e.CurrentPacket == 0)
                {
                    collectionB = new ObservableCollection<Data>();
                }
                else
                {
                    collectionB = new ObservableCollection<Data>(ScopeCHBCollection);
                }

                //显示通道B数据
                for (int i = 0; i < currentChannel2.Length / SampleInterval; i++)
                {
                    collectionB.Add(new Data() { Value1 = filterData[i * SampleInterval], Value = (e.CurrentPacket * currentChannel2.Length + i) * 1000.0 / ((int)Scope.SampleRate) * SampleInterval });
                }
            }
            else
            {
                ScopeAverageCHB = -1;
            }

            //显示图像
            ScopeCHACollection = collectionA;
            ScopeCHBCollection = collectionB;

            if (e.CurrentPacket == (e.TotalPacket - 1))
            {
                new Thread(delegate ()
                {
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        SynchronizationContext.SetSynchronizationContext(new System.Windows.Threading.DispatcherSynchronizationContext(System.Windows.Application.Current.Dispatcher));
                        SynchronizationContext.Current.Send(pl =>
                        {
                            OnMessageRaised(MessageLevel.Message, "采集完成");
                        }, null);
                    });
                }).Start();

            }

        }

        #region 配置参数

        private string scopeSampleButton = "开始采集";

        public string ScopeSampleButton
        {
            get { return scopeSampleButton; }
            set { scopeSampleButton = value; NotifyOfPropertyChange(() => ScopeSampleButton); }
        }

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

                    if (Scope.SampleRate == ESampleRate.Sps_96K)
                    {
                        ScopeSampleButton = "连续采集";
                    }
                    else
                    {
                        ScopeSampleButton = "单次采集";
                    }
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
        public ObservableCollection<Data> ScopeCHBCollection
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

        #region Power

        /// <summary>
        /// Power
        /// </summary>
        public IPower Power { get; set; }

        /// <summary>
        /// Power有效标志
        /// </summary>
        public bool IsPowerValid
        {
            get
            {
                return Power?.IsConnect ?? false;
            }
        }

        private object powerLock = new object();

        #region COM配置

        /// <summary>
        /// 串口号
        /// </summary>
        public string PowerSerialPort
        {
            get
            {
                return Power?.PrimarySerialPortName ?? "Nana";
            }
            set
            {
                if (Power != null)
                {
                    Power.PrimarySerialPortName = value;
                }

                NotifyOfPropertyChange(() => PowerSerialPort);
            }
        }

        /// <summary>
        /// 串口波特率
        /// </summary>
        public int PowerSerialPortBaudRate
        {
            get
            {
                return Power?.SerialPortBaudRate ?? 115200;
            }
            set
            {
                if (Power != null)
                {
                    Power.SerialPortBaudRate = value;
                }

                NotifyOfPropertyChange(() => PowerSerialPortBaudRate);
            }
        }

        /// <summary>
        /// 从站地址
        /// </summary>
        public byte PowerSlaveAddress
        {
            get
            {
                return Power?.SlaveAddress ?? 0xFF;
            }
            set
            {
                if (Power != null)
                {
                    Power.SlaveAddress = value;
                }

                NotifyOfPropertyChange(() => PowerSlaveAddress);
            }
        }

        /// <summary>
        /// 超时
        /// </summary>
        public int PowerTimeout
        {
            get
            {
                return Power?.ReadTimeout ?? -1;
            }
            set
            {
                if (Power != null)
                {
                    Power.ReadTimeout = value;
                    Power.WriteTimeout = value;
                }

                NotifyOfPropertyChange(() => PowerTimeout);
            }
        }

        /// <summary>
        /// 连接到Power
        /// </summary>
        public void ConnectPower()
        {
            Power?.Connect();
            UpdatePowerStatus();

            if (IsPowerValid)
            {
                //将当前通信配置保存到系统参数中
                SystemParamManager.SystemParam.PowerParams.PrimarySerialPortName = PowerSerialPort;
                SystemParamManager.SystemParam.PowerParams.SerialPortBaudRate = PowerSerialPortBaudRate;
                SystemParamManager.SystemParam.PowerParams.SlaveAddress = PowerSlaveAddress;
                SystemParamManager.SystemParam.PowerParams.ReadTimeout = PowerTimeout;
                SystemParamManager.SystemParam.PowerParams.WriteTimeout = PowerTimeout;
                SystemParamManager.SaveParams();

                AddRunningMessage("连接Power成功");
            }
            else
            {
                AddRunningMessage("连接Power失败");
            }

        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void DisconnectPower()
        {
            Power?.Disconnect();
            UpdatePowerStatus();
            AddRunningMessage("断开Power连接");
        }

        /// <summary>
        /// 连接/断开连接
        /// </summary>
        public void ConnectOrDisconnectPower()
        {
            if (IsPowerValid)
            {
                DisconnectPower();
            }
            else
            {
                ConnectPower();
            }

        }

        #endregion

        #region 输出参数控制

        /// <summary>
        /// Power电压
        /// </summary>
        public double PowerVoltage
        {
            get
            {
                if (IsPowerValid)
                {
                    return Power.Voltage;
                }
                return -1;
            }
            set
            {
                if (IsPowerValid == true)
                {
                    Power.Voltage = value;
                    SystemParamManager.SystemParam.PowerParams.Voltage = value;
                    SystemParamManager.SaveParams();
                }

                //NotifyOfPropertyChange(() => PowerVoltage);
                UpdatePowerStatus();
            }
        }

        /// <summary>
        /// Power电流
        /// </summary>
        public double PowerCurrent
        {
            get
            {
                if (IsPowerValid)
                {
                    return Power.Current;
                }
                return -1;
            }
            set
            {
                if (IsPowerValid == true)
                {
                    Power.Current = value;
                    SystemParamManager.SystemParam.PowerParams.Current = value;
                    SystemParamManager.SaveParams();
                }

                //NotifyOfPropertyChange(() => PowerCurrent);
                UpdatePowerStatus();
            }
        }

        /// <summary>
        /// Power使能输出
        /// </summary>
        public bool PowerIsEnableOutput
        {
            get
            {
                if (IsPowerValid)
                {
                    return Power.IsEnableOutput;
                }
                return false;
            }
            set
            {
                if (IsPowerValid == true)
                {
                    Power.IsEnableOutput = value;
                    SystemParamManager.SystemParam.PowerParams.IsEnableOutput = value;
                    SystemParamManager.SaveParams();
                }

                //NotifyOfPropertyChange(() => PowerIsEnableOutput);
                UpdatePowerStatus();
            }
        }

        /// <summary>
        /// 实际电压值
        /// </summary>
        public double PowerRealityVoltage
        {
            get
            {
                if (IsPowerValid)
                {
                    return Power.RealityVoltage;
                }
                return -1;
            }
        }

        /// <summary>
        /// 实际电流值
        /// </summary>
        public double PowerRealityCurrent
        {
            get
            {
                if (IsPowerValid)
                {
                    return Power.RealityCurrent;
                }
                return -1;
            }
        }

        /// <summary>
        /// 实际温度值
        /// </summary>
        public double PowerRealityTemperature
        {
            get
            {
                if (IsPowerValid)
                {
                    return Power.RealityTemperature;
                }
                return -1;
            }
        }

        /// <summary>
        /// 更新Power状态
        /// </summary>
        public void UpdatePowerStatus()
        {
            NotifyOfPropertyChange(() => IsPowerValid);
            NotifyOfPropertyChange(() => PowerVoltage);
            NotifyOfPropertyChange(() => PowerCurrent);
            NotifyOfPropertyChange(() => PowerIsEnableOutput);
            NotifyOfPropertyChange(() => PowerRealityVoltage);
            NotifyOfPropertyChange(() => PowerRealityCurrent);
            NotifyOfPropertyChange(() => PowerRealityTemperature);

            NotifyOfPropertyChange(() => IsHardwareValid);

            //lock (powerLock)
            //{
            //    if (isPowerThreadRunning)
            //    {
            //        return;
            //    }
            //}

            //new Thread(() =>
            //{
            //    lock (powerLock)
            //    {
            //        isPowerThreadRunning = true;
            //    }

            //    for (int i = 0; i < 20; i++)
            //    {
            //        Thread.Sleep(50);
            //        NotifyOfPropertyChange(() => IsPowerValid);
            //        NotifyOfPropertyChange(() => PowerVoltage);
            //        NotifyOfPropertyChange(() => PowerCurrent);
            //        NotifyOfPropertyChange(() => PowerIsEnableOutput);
            //        NotifyOfPropertyChange(() => PowerRealityVoltage);
            //        NotifyOfPropertyChange(() => PowerRealityCurrent);
            //        NotifyOfPropertyChange(() => PowerRealityTemperature);
            //    }

            //    lock (powerLock)
            //    {
            //        isPowerThreadRunning = false;
            //    }

            //}).Start();
        }

        /// <summary>
        /// 使能Power输出
        /// </summary>
        public void EnablePowerOutput()
        {
            if (IsPowerValid)
            {
                PowerIsEnableOutput = !PowerIsEnableOutput;
            }
        }

        #endregion

        #endregion

        #region PLC

        /// <summary>
        /// PLC
        /// </summary>
        IPLC PLC { get; set; }

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
            if (IsPowerValid)
            {
                DisconnectPLC();
            }
            else
            {
                ConnectPLC();
            }

        }

        #endregion

        /// <summary>
        /// 更新Power状态
        /// </summary>
        public void UpdatePLCStatus()
        {
            NotifyOfPropertyChange(() => IsPLCValid);
            NotifyOfPropertyChange(() => PLCPWMSwitch);
            NotifyOfPropertyChange(() => PLCFlowSwitch);
            
            NotifyOfPropertyChange(() => IsHardwareValid);
        }

        #region 输出参数控制

        /// <summary>
        /// PLC开关
        /// </summary>
        public bool PLCPWMSwitch
        {
            get 
            {
                if (IsPLCValid)
                {
                    return PLC.PWMSwitch;
                }
                return false; 
            }
            set 
            {
                if (IsPLCValid)
                {
                    PLC.PWMSwitch = value;
                    SystemParamManager.SystemParam.PLCParams.Switch = value;
                    SystemParamManager.SaveParams();
                }

                UpdatePLCStatus();

            }
        }

        /// <summary>
        /// PLC开关
        /// </summary>
        public bool PLCFlowSwitch
        {
            get
            {
                if (IsPLCValid)
                {
                    return PLC.FlowSwitch;
                }
                return false;
            }
            set
            {
                if (IsPLCValid)
                {
                    PLC.FlowSwitch = value;
                }

                UpdatePLCStatus();

            }
        }

        /// <summary>
        /// 使能PLC开关
        /// </summary>
        public void EnablePLCPWMSwitch()
        {
            PLCPWMSwitch = !PLCPWMSwitch;
        }


        /// <summary>
        /// 使能PLC开关
        /// </summary>
        public void EnablePLCFlowSwitch()
        {
            PLCFlowSwitch = !PLCFlowSwitch;
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
        /// 连接到Power
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
        /// 更新Power状态
        /// </summary>
        public void UpdatePWMStatus()
        {
            NotifyOfPropertyChange(() => IsPWMValid);
            NotifyOfPropertyChange(() => PWMSerialPort);
            NotifyOfPropertyChange(() => PWMFrequency);
            NotifyOfPropertyChange(() => PWMDutyRatio);

            NotifyOfPropertyChange(() => IsHardwareValid);
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

        /// <summary>
        /// 硬件有效标志
        /// </summary>
        public bool IsHardwareValid
        {
            get
            {
                if ((Scope?.IsConnect == true) && (Power?.IsConnect == true) && (PLC?.IsConnect == true) && (PWM?.IsConnect == true))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 更新硬件
        /// </summary>
        public void UpdateHardware()
        {
            if (Scope?.IsConnect != true)
            {
                Scope?.Connect();
            }

            if (Power?.IsConnect != true)
            {
                Power?.Connect();
            }

            if (PLC?.IsConnect != true)
            {
                PLC?.Connect();
            }

            if (PWM?.IsConnect != true)
            {
                PWM?.Connect();
            }

            NotifyOfPropertyChange(() => IsHardwareValid);
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
        /// 输入输出测试Model(新)
        /// </summary>
        public NewIOMeasurementViewModel NewIOMeasurementViewModel { get; set; }

        /// <summary>
        /// 吸合释放电压测试Model
        /// </summary>
        public PNVoltageMeasurementViewModel PNVoltageMeasurementViewModel { get; set; }

        /// <summary>
        /// 通气量测试Model
        /// </summary>
        public ThroughputMeasurementViewModel ThroughputMeasurementViewModel { get; set; }

        /// <summary>
        /// 老化测试
        /// </summary>
        public BurnInTestViewModel BurnInTestViewModel { get; set; }

        /// <summary>
        /// 流量测试
        /// </summary>
        public FlowMeasureViewModel FlowMeasureViewModel { get; set; }

        private int hamburgerMenuIndex;

        /// <summary>
        /// 窗口索引
        /// </summary>
        public int HamburgerMenuIndex
        {
            get 
            { 
                return hamburgerMenuIndex;
            }
            set 
            { 
                hamburgerMenuIndex = value;
                NotifyOfPropertyChange(() => HamburgerMenuIndex);
            }
        }

        private int hamburgerOptionsIndex;

        /// <summary>
        /// 操作索引
        /// </summary>
        public int HamburgerOptionsIndex
        {
            get 
            { 
                return hamburgerOptionsIndex; 
            }
            set 
            { 
                hamburgerOptionsIndex = value;
                NotifyOfPropertyChange(() => HamburgerOptionsIndex);
            }
        }

        /// <summary>
        /// 加载界面
        /// </summary>
        public void Loaded()
        {
            HamburgerMenuIndex = 0;
        }

        /// <summary>
        /// 使能测试
        /// </summary>
        public bool IsEnableTest
        {
            get
            {
                //假如硬件无效,则禁止使能测试
                if (!IsHardwareValid)
                {
                    return false;
                }

                //正在测试中,则禁止使能测试
                lock (measureLock)
                {
                    if (IsMeasuring)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// 测试全部
        /// </summary>
        public void TestAll()
        {
            lock (measureLock)
            {
                if (IsMeasuring)
                {
                    return;
                }
            }
            NotifyOfPropertyChange(() => IsEnableTest);

            var result = ((MetroWindow)Application.Current.MainWindow).ShowModalMessageExternal("测试确认", "是否开始测试?", MessageDialogStyle.AffirmativeAndNegative);
            if (result != MessageDialogResult.Affirmative)
            {
                return;
            }

            new Thread(() =>
            {
                HamburgerMenuIndex = 0;
                PNVoltageMeasurementViewModel.Start();
                Thread.Sleep(1000);

                //等待测试完成
                while (true)
                {
                    lock (measureLock)
                    {
                        if (!IsMeasuring)
                        {
                            break;
                        }
                    }
                    Thread.Sleep(20);
                }

                HamburgerMenuIndex = 1;
                ThroughputMeasurementViewModel.Start();
                Thread.Sleep(1000);

                //等待测试完成
                while (true)
                {
                    lock (measureLock)
                    {
                        if (!IsMeasuring)
                        {
                            break;
                        }
                    }
                    Thread.Sleep(20);
                }

                HamburgerMenuIndex = 2;
                FrequencyMeasurementViewModel.Start();
                Thread.Sleep(1000);

                //等待测试完成
                while (true)
                {
                    lock (measureLock)
                    {
                        if (!IsMeasuring)
                        {
                            break;
                        }
                    }
                    Thread.Sleep(20);
                }

                //InputOutputMeasurementViewModel.Start();
                //Thread.Sleep(1000);

                ////等待测试完成
                //while (true)
                //{
                //    lock (measureLock)
                //    {
                //        if (!IsMeasuring)
                //        {
                //            break;
                //        }
                //    }
                //    Thread.Sleep(20);
                //}


                Thread.Sleep(1000);
                HamburgerOptionsIndex = 0;

                NotifyOfPropertyChange(() => IsEnableTest);

            }).Start();

        }

        #region 测试结果

        private string frequencyMeasureStatus = "就绪";

        /// <summary>
        /// 频率测量状态
        /// </summary>
        public string FrequencyMeasureStatus
        {
            get
            {
                return frequencyMeasureStatus;
            }
            set
            {
                frequencyMeasureStatus = value;
                NotifyOfPropertyChange(() => FrequencyMeasureStatus);
            }
        }

        private int maxLimitFrequency;

        /// <summary>
        /// 极限频率
        /// </summary>
        public int MaxLimitFrequency
        {
            get
            {
                return maxLimitFrequency;
            }
            set
            {
                maxLimitFrequency = value;
                NotifyOfPropertyChange(() => MaxLimitFrequency);
            }
        }

        private string pnMeasureStatus = "就绪";

        /// <summary>
        /// 吸合/释放电压测试状态
        /// </summary>
        public string PNMeasureStatus
        {
            get
            {
                return pnMeasureStatus;
            }
            set
            {
                pnMeasureStatus = value;
                NotifyOfPropertyChange(() => PNMeasureStatus);
            }
        }

        private double positiveVoltage;

        /// <summary>
        /// 有效电压/吸合电压(V)
        /// </summary>
        public double PositiveVoltage
        {
            get
            {
                return positiveVoltage;
            }
            set
            {
                positiveVoltage = value;
                NotifyOfPropertyChange(() => PositiveVoltage);
            }
        }

        private double negativeVoltage;

        /// <summary>
        /// 无效电压/释放电压(V)
        /// </summary>
        public double NegativeVoltage
        {
            get
            {
                return negativeVoltage;
            }
            set
            {
                negativeVoltage = value;
                NotifyOfPropertyChange(() => NegativeVoltage);
            }
        }

        private string ioMeasureStatus = "就绪";

        /// <summary>
        /// 输入输出测试状态
        /// </summary>
        public string IOMeasureStatus
        {
            get
            {
                return ioMeasureStatus;
            }
            set
            {
                ioMeasureStatus = value;
                NotifyOfPropertyChange(() => IOMeasureStatus);
            }
        }

        private ObservableCollection<InputOutputMeasurementInfo> inputOutputInfos;

        /// <summary>
        /// 输入输出信息
        /// </summary>
        public ObservableCollection<InputOutputMeasurementInfo> InputOutputInfos
        {
            get
            {
                return inputOutputInfos;
            }
            set
            {
                inputOutputInfos = value;
                NotifyOfPropertyChange(() => InputOutputInfos);
            }
        }

        private string flowMeasureStatus = "就绪";

        /// <summary>
        /// 通气量测试状态
        /// </summary>
        public string FlowMeasureStatus
        {
            get
            {
                return flowMeasureStatus;
            }
            set
            {
                flowMeasureStatus = value;
                NotifyOfPropertyChange(() => FlowMeasureStatus);
            }
        }

        private double flow;

        /// <summary>
        /// 流量
        /// </summary>
        public double Flow
        {
            get
            {
                return flow;
            }
            set
            {
                flow = value;
                NotifyOfPropertyChange(() => Flow);
            }
        }

        /// <summary>
        /// 导出结果
        /// </summary>
        public void ExportReport()
        {
            try
            {
                var records = new List<object>
                {
                    new { Id = 1, Name = "极限频率", Value = $"{MaxLimitFrequency}" },
                    new { Id = 2, Name = "吸合电压", Value = $"{PositiveVoltage}" },
                    new { Id = 3, Name = "释放电压", Value = $"{NegativeVoltage}" },
                    new { Id = 4, Name = "流量", Value = $"{Flow}" },
                };

                if (!Directory.Exists("Report"))
                {
                    Directory.CreateDirectory("Report");
                }

                using (var writer = new StreamWriter($"Report/{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}.csv"))
                using (var csv = new CsvWriter(writer))
                {
                    csv.WriteRecords(records);
                }

                OnMessageRaised(MessageLevel.Message, "导出报告成功");

            }
            catch (Exception)
            {
                OnMessageRaised(MessageLevel.Message, "导出报告失败");
            }


        }

        #endregion

        #endregion

        #region 管理员权限

        private string operaMsg = "操作员";

        /// <summary>
        /// 操作信息
        /// </summary>
        public string OperaMsg
        {
            get
            {
                return operaMsg;
            }
            set
            {
                operaMsg = value;
                NotifyOfPropertyChange(() => OperaMsg);
            }
        }

        private bool isAdmin = false;

        /// <summary>
        /// 管理员权限
        /// </summary>
        public bool IsAdmin
        {
            get
            {
                return isAdmin;
            }
            set
            {
                if (value)
                {
                    OperaMsg = "管理员";
                }
                else
                {
                    OperaMsg = "操作员";
                }
                isAdmin = value;
                NotifyOfPropertyChange(() => IsAdmin);
            }
        }

        /// <summary>
        /// 登录
        /// </summary>
        public void Login()
        {
            LoginDialogSettings settings = new LoginDialogSettings();
            settings.InitialUsername = "Admin";
            settings.NegativeButtonVisibility = Visibility.Visible;
            settings.EnablePasswordPreview = true;

            var result = ((MetroWindow)Application.Current.MainWindow).ShowModalLoginExternal("登录管理员账户", "", settings);
            if ((result?.Username?.Equals(SystemParamManager.User?.UserName) == true) &&
                (result?.Password?.Equals(SystemParamManager.User?.Password) == true))
            {
                IsAdmin = true;
                //OnMessageRaised(MessageLevel.Message, "登录成功");

                FrequencyMeasurementViewModel.IsAdmin = IsAdmin;
                InputOutputMeasurementViewModel.IsAdmin = IsAdmin;
                NewIOMeasurementViewModel.IsAdmin = IsAdmin;
                PNVoltageMeasurementViewModel.IsAdmin = IsAdmin;
                ThroughputMeasurementViewModel.IsAdmin = IsAdmin;
            }
            else
            {
                IsAdmin = false;

                FrequencyMeasurementViewModel.IsAdmin = IsAdmin;
                InputOutputMeasurementViewModel.IsAdmin = IsAdmin;
                NewIOMeasurementViewModel.IsAdmin = IsAdmin;
                PNVoltageMeasurementViewModel.IsAdmin = IsAdmin;
                ThroughputMeasurementViewModel.IsAdmin = IsAdmin;
                OnMessageRaised(MessageLevel.Err, "登录失败");
            }
        }

        /// <summary>
        /// 注销
        /// </summary>
        public void Logout()
        {
            IsAdmin = false;

            FrequencyMeasurementViewModel.IsAdmin = IsAdmin;
            InputOutputMeasurementViewModel.IsAdmin = IsAdmin;
            NewIOMeasurementViewModel.IsAdmin = IsAdmin;
            PNVoltageMeasurementViewModel.IsAdmin = IsAdmin;
            ThroughputMeasurementViewModel.IsAdmin = IsAdmin;
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        public void MotifyPassword()
        {
            MetroDialogSettings settings = new MetroDialogSettings();
            var result = ((MetroWindow)Application.Current.MainWindow).ShowModalInputExternal("修改管理员密码", "输入新密码", settings);

            if (!string.IsNullOrWhiteSpace(result))
            {
                SystemParamManager.User.Password = result;
                SystemParamManager.SaveUser();
            }

        }

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

        #region 默认全局参数

        public double GlobalPressureZeroVoltage
        {
            get
            {
                return SystemParamManager.SystemParam.GlobalParam.PressureZeroVoltage;
            }
            set
            {
                SystemParamManager.SystemParam.GlobalParam.PressureZeroVoltage = value;
                NotifyOfPropertyChange(() => GlobalPressureZeroVoltage);
                SystemParamManager.SaveParams();
            }
        }


        /// <summary>
        /// 全局气压比例系数(P/V)
        /// </summary>
        public double GlobalPressureK
        {
            get
            {
                return SystemParamManager.SystemParam.GlobalParam.PressureK;
            }
            set
            {
                SystemParamManager.SystemParam.GlobalParam.PressureK = value;
                NotifyOfPropertyChange(() => GlobalPressureK);
                SystemParamManager.SaveParams();
            }
        }

        /// <summary>
        /// 流量系数
        /// </summary>
        public double GlobalFlowK
        {
            get
            {
                return SystemParamManager.SystemParam.GlobalParam.FlowK;
            }
            set
            {
                SystemParamManager.SystemParam.GlobalParam.FlowK = value;
                NotifyOfPropertyChange(() => GlobalFlowK);
                SystemParamManager.SaveParams();
            }
        }

        /// <summary>
        /// 全局电源模块通信延迟(MS)
        /// </summary>
        public int GlobalPowerCommonDelay
        {
            get
            {
                return SystemParamManager.SystemParam.GlobalParam.PowerCommonDelay;
            }
            set
            {
                SystemParamManager.SystemParam.GlobalParam.PowerCommonDelay = value;
                NotifyOfPropertyChange(() => GlobalPowerCommonDelay);
                SystemParamManager.SaveParams();
            }
        }

        /// <summary>
        /// CHA探头衰变
        /// </summary>
        public string GlobalScopeCHAScale
        {
            get
            {
                return EnumHelper.GetDescription(SystemParamManager.SystemParam.GlobalParam.Scale);
            }
            set
            {
                SystemParamManager.SystemParam.GlobalParam.Scale = EnumHelper.GetEnum<EScale>(value);
                NotifyOfPropertyChange(() => GlobalScopeCHAScale);
                SystemParamManager.SaveParams();
            }
        }

        /// <summary>
        /// CHA电压档位
        /// </summary>
        public string GlobalScopeCHAVoltageDIV
        {
            get
            {
                return EnumHelper.GetDescription(SystemParamManager.SystemParam.GlobalParam.VoltageDIV);
            }
            set
            {
                SystemParamManager.SystemParam.GlobalParam.VoltageDIV = EnumHelper.GetEnum<EVoltageDIV>(value);
                NotifyOfPropertyChange(() => GlobalScopeCHAVoltageDIV);
                SystemParamManager.SaveParams();
            }
        }

        /// <summary>
        /// 采样率
        /// </summary>
        public string GlobalScopeSampleRate
        {
            get
            {
                return EnumHelper.GetDescription(SystemParamManager.SystemParam.GlobalParam.SampleRate);
            }
            set
            {
                SystemParamManager.SystemParam.GlobalParam.SampleRate = EnumHelper.GetEnum<ESampleRate>(value);
                NotifyOfPropertyChange(() => GlobalScopeSampleRate);
                SystemParamManager.SaveParams();
            }
        }

        #endregion

        #endregion

    }
}
