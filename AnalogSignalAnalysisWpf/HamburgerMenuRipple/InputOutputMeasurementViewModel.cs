using AnalogSignalAnalysisWpf.Hardware;
using AnalogSignalAnalysisWpf.Hardware.Scope;
using AnalogSignalAnalysisWpf.LiveData;
using Caliburn.Micro;
using DataAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace AnalogSignalAnalysisWpf
{

    public class InputOutputMeasurementInfo
    {
        /// <summary>
        /// 时间
        /// </summary>
        public string DateTime { get; set; }

        /// <summary>
        /// 输入电压
        /// </summary>
        public double Input { get; set; }

        /// <summary>
        /// 输出电压(检测电压)
        /// </summary>
        public double Output { get; set; }

        /// <summary>
        /// 公斤力
        /// </summary>
        public double Output2 { get; set; }

        /// <summary>
        /// 创建InputOutputMeasurementInfo新实例
        /// </summary>
        public InputOutputMeasurementInfo()
        {

        }

        /// <summary>
        /// 创建InputOutputMeasurementInfo新实例
        /// </summary>
        /// <param name="input">输入电压</param>
        /// <param name="output">输出电压</param>
        public InputOutputMeasurementInfo(double input, double output)
        {
            DateTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Input = input;
            Output = output;
            Output2 = Output * 10.197;
        }
    }

    /// <summary>
    /// 电压表
    /// </summary>
    public class VoltageTable
    {
        /// <summary>
        /// 电压值
        /// </summary>
        public double Voltage { get; set; }
    }

    public class InputOutputMeasurementViewModel : Screen
    {
        #region 构造函数

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
                isAdmin = value;
                NotifyOfPropertyChange(() => IsAdmin);
            }
        }

        /// <summary>
        /// 系统参数管理器
        /// </summary>
        public SystemParamManager SystemParamManager { get; private set; }

        /// <summary>
        /// 创建InputOutputMeasurementViewModel新实例
        /// </summary>
        public InputOutputMeasurementViewModel()
        {
            //恢复配置参数
            SystemParamManager = SystemParamManager.GetInstance();

            VoltageTable = new ObservableCollection<VoltageTable>(SystemParamManager.SystemParam.InputOutputMeasureParams.VoltageTable);
            OutputType = SystemParamManager.SystemParam.InputOutputMeasureParams.OutputType;
            VoltageInterval = SystemParamManager.SystemParam.InputOutputMeasureParams.VoltageInterval;
            MinVoltage = SystemParamManager.SystemParam.InputOutputMeasureParams.MinVoltage;
            MaxVoltage = SystemParamManager.SystemParam.InputOutputMeasureParams.MaxVoltage;
            SampleTime = SystemParamManager.SystemParam.InputOutputMeasureParams.SampleTime;

            //更新硬件状态
            UpdateHardware();
        }

        /// <summary>
        /// 创建InputOutputMeasurementViewModel新实例
        /// </summary>
        /// <param name="scope">示波器</param>
        /// <param name="power">Power</param>
        public InputOutputMeasurementViewModel(IScopeBase scope, IPower power, IPLC plc, IPWM pwm) : this()
        {
            if (scope == null)
            {
                throw new ArgumentException("scope invalid");
            }

            if (power == null)
            {
                throw new ArgumentException("power invalid");
            }

            if (plc == null)
            {
                throw new ArgumentException("plc invalid");
            }

            if (pwm == null)
            {
                throw new ArgumentException("pwm invalid");
            }

            Scope = scope;
            Power = power;
            PLC = plc;
            PWM = pwm;

            if (!IsHardwareValid)
            {
                RunningStatus = "硬件无效";
            }
            else
            {
                RunningStatus = "就绪";
            }
        }

        #endregion

        #region 硬件接口

        /// <summary>
        /// 示波器接口
        /// </summary>
        public IScopeBase Scope { get; set; }

        /// <summary>
        /// Power接口
        /// </summary>
        public IPower Power { get; set; }

        /// <summary>
        /// PLC接口
        /// </summary>
        public IPLC PLC { get; set; }

        /// <summary>
        /// PWM接口
        /// </summary>
        public IPWM PWM { get; set; }

        /// <summary>
        /// 硬件有效标志
        /// </summary>
        public bool IsHardwareValid
        {
            get
            {
                if ((Scope?.IsConnect == true) &&
                    (Power?.IsConnect == true) &&
                    (PLC?.IsConnect == true) &&
                    (PWM?.IsConnect == true))
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
            NotifyOfPropertyChange(() => CanMeasure);
        }

        #endregion

        #region 配置参数

        private ObservableCollection<VoltageTable> voltageTable;

        /// <summary>
        /// 电压表
        /// </summary>
        public ObservableCollection<VoltageTable> VoltageTable
        {
            get 
            { 
                return voltageTable; 
            }
            set 
            {
                voltageTable = value;
                NotifyOfPropertyChange(() => VoltageTable);

                SystemParamManager.SystemParam.InputOutputMeasureParams.VoltageTable = new ObservableCollection<VoltageTable>(value);
                SystemParamManager.SaveParams();
            }
        }

        private int outputType;

        /// <summary>
        /// 输出类型
        /// </summary>
        public int OutputType
        {
            get 
            { 
                return outputType; 
            }
            set 
            { 
                outputType = value;
                NotifyOfPropertyChange(() => OutputType);

                SystemParamManager.SystemParam.InputOutputMeasureParams.OutputType = value;
                SystemParamManager.SaveParams();

                IsEnableLoopupTableOutput = value == 1;
                IsEnableNormalOutput = !IsEnableLoopupTableOutput;
            }
        }

        private double voltageInterval = 0.25;

        /// <summary>
        /// 电压间隔(V)
        /// </summary>
        public double VoltageInterval
        {
            get
            {
                return voltageInterval;
            }
            set
            {
                voltageInterval = value;
                NotifyOfPropertyChange(() => VoltageInterval);

                SystemParamManager.SystemParam.InputOutputMeasureParams.VoltageInterval = value;
                SystemParamManager.SaveParams();
            }
        }

        private double minVoltage = 1;

        /// <summary>
        /// 最小电压(单位:V)
        /// </summary>
        public double MinVoltage
        {
            get
            {
                return minVoltage;
            }
            set
            {
                minVoltage = value;
                NotifyOfPropertyChange(() => MinVoltage);

                SystemParamManager.SystemParam.InputOutputMeasureParams.MinVoltage = value;
                SystemParamManager.SaveParams();
            }
        }

        private double maxVoltage = 15.0;

        /// <summary>
        /// 最大电压(单位:V)
        /// </summary>
        public double MaxVoltage
        {
            get
            {
                return maxVoltage;
            }
            set
            {
                maxVoltage = value;
                NotifyOfPropertyChange(() => MaxVoltage);

                SystemParamManager.SystemParam.InputOutputMeasureParams.MaxVoltage = value;
                SystemParamManager.SaveParams();
            }
        }

        private int sampleTime = 500;

        /// <summary>
        /// 采样时间(MS)
        /// </summary>
        public int SampleTime
        {
            get
            {
                return sampleTime;
            }
            set
            {
                sampleTime = value;
                NotifyOfPropertyChange(() => SampleTime);

                SystemParamManager.SystemParam.InputOutputMeasureParams.SampleTime = value;
                SystemParamManager.SaveParams();
            }
        }

        #endregion

        #region 测量信息

        private ObservableCollection<InputOutputMeasurementInfo> measurementInfos;

        /// <summary>
        /// 测量信息
        /// </summary>
        public ObservableCollection<InputOutputMeasurementInfo> MeasurementInfos
        {
            get
            {
                return measurementInfos;
            }
            set
            {
                measurementInfos = value;
                NotifyOfPropertyChange(() => MeasurementInfos);
            }
        }

        private bool isEnableLoopupTableOutput;

        /// <summary>
        /// 使能查表法
        /// </summary>
        public bool IsEnableLoopupTableOutput
        {
            get 
            { 
                return isEnableLoopupTableOutput; 
            }
            set 
            {
                isEnableLoopupTableOutput = value;
                NotifyOfPropertyChange(() => IsEnableLoopupTableOutput);
            }
        }

        private bool isEnableNormalOutput;

        /// <summary>
        /// 使能正常输出模式
        /// </summary>
        public bool IsEnableNormalOutput
        {
            get 
            { 
                return isEnableNormalOutput; 
            }
            set 
            { 
                isEnableNormalOutput = value;
                NotifyOfPropertyChange(() => IsEnableNormalOutput);
            }
        }


        private bool isMeasuring;

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

        private bool canMeasure = true;

        /// <summary>
        /// 允许测量
        /// </summary>
        public bool CanMeasure
        {
            get
            {
                if (IsHardwareValid)
                {
                    return canMeasure;
                }

                return false;
            }
            set
            {
                canMeasure = value;
                NotifyOfPropertyChange(() => CanMeasure);
            }
        }

        private string runningStatus = "就绪";

        /// <summary>
        /// 运行状态
        /// </summary>
        public string RunningStatus
        {
            get
            {
                return runningStatus;
            }
            set
            {
                runningStatus = value;
                NotifyOfPropertyChange(() => RunningStatus);
            }
        }

        private double currentInput;

        /// <summary>
        /// 当前输入电压
        /// </summary>
        public double CurrentInput
        {
            get
            {
                return currentInput;
            }
            set
            {
                currentInput = value;
                NotifyOfPropertyChange(() => CurrentInput);
            }
        }

        private double currentOutput;

        /// <summary>
        /// 当前输出电压(测量电压)
        /// </summary>
        public double CurrentOutput
        {
            get
            {
                return currentOutput;
            }
            set
            {
                currentOutput = value;
                NotifyOfPropertyChange(() => CurrentOutput);
            }
        }

        #endregion

        #region 折线图

        private ObservableCollection<Data> scopeCHACollection;

        /// <summary>
        /// 通道A数据
        /// </summary>
        public ObservableCollection<Data> ScopeCHACollection
        {
            get
            {
                return scopeCHACollection;
            }
            set
            {
                scopeCHACollection = value;
                NotifyOfPropertyChange(() => ScopeCHACollection);
            }
        }

        private ObservableCollection<Data> scopeCHBCollection;

        /// <summary>
        /// 通道A数据
        /// </summary>
        public ObservableCollection<Data> ScopeCHBCollection
        {
            get
            {
                return scopeCHBCollection;
            }
            set
            {
                scopeCHBCollection = value;
                NotifyOfPropertyChange(() => ScopeCHBCollection);
            }
        }

        private ObservableCollection<Data> aVoltageEdgeCollection = new ObservableCollection<Data>();

        /// <summary>
        /// 通道A数据
        /// </summary>
        public ObservableCollection<Data> AVoltageEdgeCollection
        {
            get
            {
                return aVoltageEdgeCollection;
            }
            set
            {
                aVoltageEdgeCollection = value;
                NotifyOfPropertyChange(() => AVoltageEdgeCollection);
            }
        }


        private ObservableCollection<Data> bVoltageEdgeCollection = new ObservableCollection<Data>();

        /// <summary>
        /// 通道A数据
        /// </summary>
        public ObservableCollection<Data> BVoltageEdgeCollection
        {
            get
            {
                return bVoltageEdgeCollection;
            }
            set
            {
                bVoltageEdgeCollection = value;
                NotifyOfPropertyChange(() => BVoltageEdgeCollection);
            }
        }


        #endregion

        #region 事件

        /// <summary>
        /// 测量开始事件
        /// </summary>
        public event EventHandler<EventArgs> MeasurementStarted;

        /// <summary>
        /// 测量开始事件
        /// </summary>
        protected void OnMeasurementStarted()
        {
            CanMeasure = false;
            MeasurementStarted?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// 测量完成事件
        /// </summary>
        public event EventHandler<InputOutputMeasurementCompletedEventArgs> MeasurementCompleted;

        /// <summary>
        /// 测量完成事件
        /// </summary>
        /// <param name="e"></param>
        protected void OnMeasurementCompleted(InputOutputMeasurementCompletedEventArgs e)
        {
            CanMeasure = true;

            RunningStatus = e.IsSuccess ? "成功" : "失败";

            PLC.PWMSwitch = false;
            PLC.FlowSwitch = false;

            if (Power?.IsConnect == true)
            {
                Power.IsEnableOutput = false;
            }

            lock (lockObject)
            {
                IsMeasuring = false;
            }
            MeasurementCompleted?.Invoke(this, e);
        }

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

        private object lockObject = new object();

        /// <summary>
        /// 测量线程
        /// </summary>
        private Thread measureThread;

        /// <summary>
        /// 显示输入输出数据
        /// </summary>
        /// <param name="time">当前时间</param>
        /// <param name="currentInput">当前输入</param>
        /// <param name="currentOutput">当前输出</param>
        /// <param name="isSuccess">执行结果</param>
        private void ShowIOData(double time, double currentInput, double currentOutput)
        {
            new Thread(delegate ()
            {
                ThreadPool.QueueUserWorkItem(delegate
                {
                    SynchronizationContext.SetSynchronizationContext(new System.Windows.Threading.DispatcherSynchronizationContext(System.Windows.Application.Current.Dispatcher));
                    SynchronizationContext.Current.Send(pl =>
                    {
                        AVoltageEdgeCollection.Add(new Data
                        {
                            Value1 = currentInput,
                            Value = time
                        });

                        ScopeCHACollection.Add(new Data
                        {
                            Value1 = currentInput,
                            Value = time
                        });

                        BVoltageEdgeCollection.Add(new Data
                        {
                            Value1 = currentOutput,
                            Value = time
                        });

                        ScopeCHBCollection.Add(new Data
                        {
                            Value1 = currentOutput,
                            Value = time
                        });

                        MeasurementInfos.Insert(0, new InputOutputMeasurementInfo(currentInput, currentOutput));
                    }, null);
                });
            }).Start();
        }

        /// <summary>
        /// 电压转气压
        /// </summary>
        /// <param name="voltage"></param>
        /// <returns></returns>
        private double VoltageToPressure(double voltage)
        {

            return (voltage - SystemParamManager.SystemParam.GlobalParam.PressureZeroVoltage) * SystemParamManager.SystemParam.GlobalParam.PressureK;
        }

        /// <summary>
        /// 启动
        /// </summary>
        public void Start()
        {
            lock (lockObject)
            {
                if (IsMeasuring)
                {
                    return;
                }
            }

            if (!IsHardwareValid)
            {
                RunningStatus = "硬件无效";
                return;
            }

            RunningStatus = "运行中";

            //复位示波器设置
            Scope.Disconnect();
            Scope.Connect();
            Scope.CHAScale = SystemParamManager.SystemParam.GlobalParam.Scale;
            Scope.SampleRate = SystemParamManager.SystemParam.GlobalParam.SampleRate;
            Scope.CHAVoltageDIV = SystemParamManager.SystemParam.GlobalParam.VoltageDIV;

            //设置电源模块直通
            PLC.PWMSwitch = false;
            PLC.FlowSwitch = true;

            ScopeCHACollection = new ObservableCollection<Data>();
            ScopeCHBCollection = new ObservableCollection<Data>();
            AVoltageEdgeCollection = new ObservableCollection<Data>();
            BVoltageEdgeCollection = new ObservableCollection<Data>();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            OnMeasurementStarted();

            measureThread = new System.Threading.Thread(() =>
            {
                lock (lockObject)
                {
                    IsMeasuring = true;
                }

                var infos = new List<InputOutputMeasurementInfo>();
                MeasurementInfos = new ObservableCollection<InputOutputMeasurementInfo>();

                Scope.SampleTime = 500;

                //查表法
                if (IsEnableLoopupTableOutput)
                {
                    if (VoltageTable?.Count >= 0)
                    {
                        double currentVoltage = 0;
                        Power.Voltage = 0;
                        Power.IsEnableOutput = true;

                        foreach (var item in VoltageTable)
                        {
                            currentVoltage = item.Voltage;

                            //设置当前电压
                            Power.Voltage = currentVoltage;
                            Thread.Sleep(SampleTime);

                            //读取Scope数据
                            double[] originalData;
                            Scope.ReadDataBlock(0, out originalData);

                            //数据滤波
                            double[] filterData;
                            Analysis.MeanFilter(originalData, 7, out filterData);

                            //电压转气压
                            double[] pressureData = filterData.ToList().ConvertAll(x => VoltageToPressure(x)).ToArray();

                            //获取中值
                            var medianData = Analysis.Median(pressureData);
                            if (double.IsNaN(medianData))
                            {
                                OnMeasurementCompleted(new InputOutputMeasurementCompletedEventArgs());
                            }

                            CurrentInput = currentVoltage;
                            CurrentOutput = medianData;

                            if (infos.Count >= 2)
                            {
                                CurrentOutput = infos[infos.Count - 1].Output * 0.5 + infos[infos.Count - 2].Output * 0.2 + medianData * 0.3;
                            }

                            infos.Add(new InputOutputMeasurementInfo(CurrentInput, CurrentOutput));

                            stopwatch.Stop();
                            ShowIOData(stopwatch.Elapsed.TotalMilliseconds, currentVoltage, medianData);
                            stopwatch.Start();
                        }

                    }
                    else
                    {
                        OnMeasurementCompleted(new InputOutputMeasurementCompletedEventArgs());
                    }

                    //输出结果
                    OnMeasurementCompleted(new InputOutputMeasurementCompletedEventArgs(true, infos));
                }
                else
                {
                    if (MinVoltage > MaxVoltage)
                    {
                        OnMeasurementCompleted(new InputOutputMeasurementCompletedEventArgs());
                    }
                    
                    double currentVoltage = MinVoltage;
                    Power.Voltage = currentVoltage;
                    Power.IsEnableOutput = true;

                    int count = 0;
                    while (currentVoltage <= MaxVoltage)
                    {
                        //设置当前电压
                        Power.Voltage = currentVoltage;
                        Thread.Sleep(SampleTime);

                        //读取Scope数据
                        double[] originalData;
                        Scope.ReadDataBlock(0, out originalData);

                        //数据滤波
                        double[] filterData;
                        Analysis.MeanFilter(originalData, 7, out filterData);

                        //电压转气压
                        double[] pressureData = filterData.ToList().ConvertAll(x => VoltageToPressure(x)).ToArray();

                        //获取中值
                        var medianData = Analysis.Median(pressureData);
                        if (double.IsNaN(medianData))
                        {
                            OnMeasurementCompleted(new InputOutputMeasurementCompletedEventArgs());
                        }

                        CurrentInput = currentVoltage;
                        CurrentOutput = medianData;

                        if (infos.Count >= 2)
                        {
                            CurrentOutput = infos[infos.Count - 1].Output * 0.5 + infos[infos.Count - 2].Output * 0.2 + medianData * 0.3;
                        }

                        infos.Add(new InputOutputMeasurementInfo(CurrentInput, CurrentOutput));

                        stopwatch.Stop();
                        ShowIOData(stopwatch.Elapsed.TotalMilliseconds, currentVoltage, medianData);
                        stopwatch.Start();

                        currentVoltage += VoltageInterval;
                        count++;
                    }

                    //输出结果
                    OnMeasurementCompleted(new InputOutputMeasurementCompletedEventArgs(true, infos));
                }
            });

            measureThread.Start();
        }

        private int talbleIndex;

        /// <summary>
        /// 列表索引
        /// </summary>
        public int TalbleIndex
        {
            get 
            { 
                return talbleIndex; 
            }
            set 
            { 
                talbleIndex = value;
                NotifyOfPropertyChange(() => TalbleIndex);
            }
        }

        /// <summary>
        /// 插入测试数据
        /// </summary>
        public void InsertTableItem()
        {
            lock (lockObject)
            {
                if (IsMeasuring)
                {
                    return;
                }
            }

            if (TalbleIndex < 0)
            {
                return;
            }

            VoltageTable.Insert(TalbleIndex, new AnalogSignalAnalysisWpf.VoltageTable());

            SystemParamManager.SystemParam.InputOutputMeasureParams.VoltageTable = new ObservableCollection<VoltageTable>(VoltageTable);
            SystemParamManager.SaveParams();
        }

        /// <summary>
        /// 删除测试数据
        /// </summary>
        public void DeleteTableItem()
        {
            lock (lockObject)
            {
                if (IsMeasuring)
                {
                    return;
                }
            }

            if (TalbleIndex < 0)
            {
                return;
            }

            VoltageTable.RemoveAt(TalbleIndex);

            SystemParamManager.SystemParam.InputOutputMeasureParams.VoltageTable = new ObservableCollection<VoltageTable>(VoltageTable);
            SystemParamManager.SaveParams();
        }

        /// <summary>
        /// 清空测试数据
        /// </summary>
        public void ClearTable()
        {
            VoltageTable = new ObservableCollection<VoltageTable>();
            SystemParamManager.SystemParam.InputOutputMeasureParams.VoltageTable = new ObservableCollection<VoltageTable>(VoltageTable);
            SystemParamManager.SaveParams();
        }

        /// <summary>
        /// 保存测试数据
        /// </summary>
        public void SaveTable()
        {
            SystemParamManager.SystemParam.InputOutputMeasureParams.VoltageTable = new ObservableCollection<VoltageTable>(VoltageTable);
            SystemParamManager.SaveParams();
        }

        #endregion

    }
}
