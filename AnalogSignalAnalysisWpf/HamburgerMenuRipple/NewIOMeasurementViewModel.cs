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
using Framework.Infrastructure.Serialization;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.Windows;

namespace AnalogSignalAnalysisWpf
{
    /// <summary>
    /// 外部记录的数据
    /// </summary>
    public class ExternRecordData
    {
        //数据列表
        public Dictionary<int, double> RecordVoltages = new Dictionary<int, double>();

        /// <summary>
        /// 最大电压
        /// </summary>
        public double MaxVoltage { get; set; }

        /// <summary>
        /// 最大时间
        /// </summary>
        public double MaxTime { get; set; }

        /// <summary>
        /// 时间间隔(MS)
        /// </summary>
        public int TimeInterval { get; set; } = 50;
    }

    public class NewIOMeasurementViewModel : Screen
    {

        /// <summary>
        /// 内部锁
        /// </summary>
        private object lockObject = new object();

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
        public NewIOMeasurementViewModel()
        {
            //恢复配置参数
            SystemParamManager = SystemParamManager.GetInstance();

            //更新硬件状态
            UpdateHardware();
        }

        /// <summary>
        /// 创建InputOutputMeasurementViewModel新实例
        /// </summary>
        /// <param name="scope">示波器</param>
        /// <param name="power">Power</param>
        public NewIOMeasurementViewModel(IScopeBase scope, IPower power, IPLC plc, IPWM pwm) : this()
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

        #region 运行状态

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
        /// 当前输入电压(电源输出电压)
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

        #region 输入参数

        private string importFile;

        /// <summary>
        /// 导入的配置文件
        /// </summary>
        public string ImportFile
        {
            get { return importFile; }
            set { importFile = value; NotifyOfPropertyChange(() => ImportFile); }
        }

        /// <summary>
        /// 模板数据
        /// </summary>
        public ObservableCollection<Data> Template { get; set; } = new ObservableCollection<Data>();

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
#if true
                if (Scope?.IsConnect == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
#else
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
#endif

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

        #region 显示参数

        private ObservableCollection<Data> powerCollection;

        /// <summary>
        /// 输出参数
        /// </summary>
        public ObservableCollection<Data> PowerCollection
        {
            get
            {
                return powerCollection;
            }
            set
            {
                powerCollection = value;
                NotifyOfPropertyChange(() => PowerCollection);
            }
        }

        private ObservableCollection<Data> powerCollection2;

        /// <summary>
        /// 输出参数
        /// </summary>
        public ObservableCollection<Data> PowerCollection2
        {
            get
            {
                return powerCollection2;
            }
            set
            {
                powerCollection2 = value;
                NotifyOfPropertyChange(() => PowerCollection2);
            }
        }

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

        private ObservableCollection<Data> templateDataCollection;

        /// <summary>
        /// 模板数据集合
        /// </summary>
        public ObservableCollection<Data> TemplateDataCollection
        {
            get { return templateDataCollection; }
            set { templateDataCollection = value; NotifyOfPropertyChange(() => TemplateDataCollection); }
        }

        private ObservableCollection<Data> tempPowerCollection = new ObservableCollection<Data>();
        private ObservableCollection<Data> tempPowerCollection2 = new ObservableCollection<Data>();
        private ObservableCollection<Data> tempCHACollection = new ObservableCollection<Data>();
        private ObservableCollection<Data> tempCHBCollection = new ObservableCollection<Data>();

        /// <summary>
        /// 追加电源数据
        /// </summary>
        /// <param name="datas"></param>
        private void AppendPowerCollection(IEnumerable<Data> datas)
        {
            foreach (var item in datas)
            {
                tempPowerCollection.Add(item);
            }

            PowerCollection = PowerCollection = new ObservableCollection<Data>(tempPowerCollection);
        }

        /// <summary>
        /// 追加电源数据
        /// </summary>
        /// <param name="datas"></param>
        private void AppendPowerCollection(Data data)
        {
            tempPowerCollection.Add(data);

            PowerCollection = new ObservableCollection<Data>(tempPowerCollection);

        }

        /// <summary>
        /// 清除电源集合
        /// </summary>
        private void ClearPowerCollection()
        {
            tempPowerCollection  = new ObservableCollection<Data>();
            PowerCollection = new ObservableCollection<Data>();
        }

        /// <summary>
        /// 追加电源数据
        /// </summary>
        /// <param name="datas"></param>
        private void AppendPowerCollection2(IEnumerable<Data> datas)
        {
            foreach (var item in datas)
            {
                tempPowerCollection2.Add(item);
            }

            PowerCollection2 = new ObservableCollection<Data>(tempPowerCollection2);
        }

        /// <summary>
        /// 追加电源数据
        /// </summary>
        /// <param name="datas"></param>
        private void AppendPowerCollection2(Data data)
        {
            tempPowerCollection2.Add(data);

            PowerCollection2 = new ObservableCollection<Data>(tempPowerCollection2);

        }

        /// <summary>
        /// 清除电源集合
        /// </summary>
        private void ClearPowerCollection2()
        {
            tempPowerCollection2 = new ObservableCollection<Data>();
            PowerCollection2 = new ObservableCollection<Data>();
        }

        /// <summary>
        /// 追加CHA
        /// </summary>
        /// <param name="datas"></param>
        private void AppendScopeCHACollection(IEnumerable<Data> datas)
        {
            foreach (var item in datas)
            {
                tempCHACollection.Add(item);
            }

            ScopeCHACollection = new ObservableCollection<Data>(tempCHACollection);

        }
        
        /// <summary>
        /// 清除CHA集合
        /// </summary>
        private void ClearScopeCHACollection()
        {
            tempCHACollection = new ObservableCollection<Data>();
            ScopeCHACollection = new ObservableCollection<Data>();
        }

        /// <summary>
        /// 追加CHB
        /// </summary>
        /// <param name="datas"></param>
        private void AppendScopeCHBCollection(IEnumerable<Data> datas)
        {
            foreach (var item in datas)
            {
                tempCHBCollection.Add(item);
            }

            ScopeCHBCollection = new ObservableCollection<Data>(tempCHBCollection);

        }

        /// <summary>
        /// 清除CHB集合
        /// </summary>
        private void ClearScopeCHBCollection()
        {
            tempCHBCollection = new ObservableCollection<Data>();
            ScopeCHBCollection = new ObservableCollection<Data>();
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
        public event EventHandler<EventArgs> MeasurementCompleted;

        /// <summary>
        /// 测量完成事件
        /// </summary>
        /// <param name="e"></param>
        protected void OnMeasurementCompleted()
        {
            CanMeasure = true;

            RunningStatus = "成功";

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
            MeasurementCompleted?.Invoke(this, new EventArgs());
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

        public event EventHandler<EventArgs> Compared;

        protected void OnCompared()
        {
            Compared?.Invoke(this, new EventArgs());
        }

        #endregion

        #region 应用

        /// <summary>
        /// 外部记录的数据
        /// </summary>
        public ExternRecordData ExternRecordData { get; set; }

        /// <summary>
        /// 测量线程
        /// </summary>
        private Thread measureThread;

        /// <summary>
        /// 采用间隔(界面)
        /// </summary>
        private readonly int SampleInterval = 10;

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

            //配置文件校验
            if (string.IsNullOrWhiteSpace(ImportFile) || !File.Exists(ImportFile))
            {
                OnMessageRaised(MessageLevel.Err, "配置文件无效");
                return;
            }

            //加载文件
            ExternRecordData = JsonSerialization.DeserializeObjectFromFile<ExternRecordData>(ImportFile);

            if (!(ExternRecordData?.RecordVoltages?.Values?.Count > 0))
            {
                OnMessageRaised(MessageLevel.Err, "配置文件无效");
                return;
            }

            int totalTime = (ExternRecordData.RecordVoltages.Values.Count) * ExternRecordData.TimeInterval + 3000;

            RunningStatus = "运行中";

            //复位示波器设置
            Scope.Disconnect();
            Scope.Connect();
            Scope.CHAScale = SystemParamManager.SystemParam.GlobalParam.Scale;
            Scope.CHAVoltageDIV = SystemParamManager.SystemParam.GlobalParam.VoltageDIV;
            Scope.IsCHBEnable = true;
            Scope.CHBScale = SystemParamManager.SystemParam.GlobalParam.Scale;
            Scope.CHBVoltageDIV = SystemParamManager.SystemParam.GlobalParam.VoltageDIV;
            Scope.SampleRate = ESampleRate.Sps_96K;

            //设置电源模块直通
            PLC.PWMSwitch = false;
            PLC.FlowSwitch = true;

            //开启测量线程
            measureThread = new System.Threading.Thread(() =>
            {
                lock (lockObject)
                {
                    IsMeasuring = true;
                }

                OnMeasurementStarted();

                Stopwatch stopwatch = new Stopwatch();
                Stopwatch totalStopwatch = new Stopwatch();

                //设置示波器采集
                Scope.ScopeReadDataCompleted -= Scope_ScopeReadDataCompleted;
                Scope.ScopeReadDataCompleted += Scope_ScopeReadDataCompleted;

                //清除界面数据
                ClearPowerCollection();
                ClearPowerCollection2();
                ClearScopeCHACollection();
                ClearScopeCHBCollection();

                Power.Voltage = 0;
                Power.IsEnableOutput = true;

                //开始连续采集
                Scope.StartSerialSampple(totalTime);

                var collection = new ObservableCollection<Data>();

                int totalCount = ExternRecordData.RecordVoltages.Values.Count;

                int index = 0;
                double lastVol = 0;
                totalStopwatch.Start();
                foreach (var item in ExternRecordData.RecordVoltages.Values)
                {
                    stopwatch.Start();
                    while (true)
                    {
                        if (stopwatch.Elapsed.TotalMilliseconds >= ExternRecordData.TimeInterval)
                        {
                            stopwatch.Restart();

                            if (item > 0)
                            {
                                //设置当前电压
                                Power.Voltage = item / 1000;
                                Console.WriteLine($"[{stopwatch.Elapsed.TotalMilliseconds:F2}:{index}]: vol: {item:F3}");
                                lastVol = item;

                                collection.Add(new Data() { Value1 = item / 1000, Value = totalStopwatch.Elapsed.TotalMilliseconds });
                            }
                            else
                            {
                                //设置当前电压
                                Power.Voltage = lastVol / 1000;
                                Console.WriteLine($"[{stopwatch.Elapsed.TotalMilliseconds:F2}:{index}]: vol: {lastVol:F3}");
                            }

                            index++;
                            break;
                        }
                        Thread.Sleep(2);
                    }

                    if ((index % (totalCount / 20)) == 0)
                    {
                        AppendPowerCollection(collection);
                        collection = new ObservableCollection<Data>();
                    }
                }

                //AppendPowerCollection(new Data() { Value1 = lastVol / 1000, Value = index * ExternRecordData.TimeInterval });

                //输出完成电压后,设置相关标志位
                OnMeasurementCompleted();

            });

            measureThread.Start();
        }

        /// <summary>
        /// 比较
        /// </summary>
        public void Compare()
        {
            ClearPowerCollection2();
            AppendPowerCollection2(tempPowerCollection);

            new Thread(() => { Thread.Sleep(1000); OnCompared(); }).Start();

        }

        /// <summary>
        /// 显示完整的图表
        /// </summary>
        /// <param name="globleChannel1"></param>
        /// <param name="globleChannel2"></param>
        public void ShowCompleteGraph(double[] globleChannel1, double[] globleChannel2)
        {
            ObservableCollection<Data> collectionA = new ObservableCollection<Data>();
            ObservableCollection<Data> collectionB = new ObservableCollection<Data>();

            double[] pressureChA = globleChannel1.ToList().ConvertAll(x => VoltageToPressure(x)).ToArray();
            double[] pressureChB = globleChannel2.ToList().ConvertAll(x => VoltageToPressure(x)).ToArray();

            double[] filterData;
            Analysis.MeanFilter(pressureChA, 11, out filterData);

            for (int i = 0; i < pressureChA.Length; i++)
            {
                collectionA.Add(new Data() { Value1 = filterData[i], Value = i * 1000.0 / ((int)Scope.SampleRate) });
            }

            Analysis.MeanFilter(pressureChB, 11, out filterData);

            for (int i = 0; i < pressureChB.Length; i++)
            {
                collectionB.Add(new Data() { Value1 = filterData[i], Value = i * 1000.0 / ((int)Scope.SampleRate) });
            }

            ClearScopeCHACollection();
            ClearScopeCHBCollection();

            AppendScopeCHACollection(collectionA);
            AppendScopeCHACollection(collectionB);

            //显示模板数据
            TemplateDataCollection = new ObservableCollection<Data>(Template);

            //Compare();
        }

        /// <summary>
        /// 示波器读取数据完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Scope_ScopeReadDataCompleted(object sender, ScopeReadDataCompletedEventArgs e)
        {

#if true
            double[] globleChannel1;
            double[] globleChannel2;
            double[] currentChannel1;
            double[] currentChannel2;

            e.GetData(out globleChannel1, out globleChannel2, out currentChannel1, out currentChannel2);

            //设置通道A数据
            //数据滤波
            double[] filterData;
            Analysis.MeanFilter(currentChannel1, 11, out filterData);

            //显示数据
            ObservableCollection<Data> collectionA = new ObservableCollection<Data>();

            for (int i = 0; i < currentChannel1.Length / SampleInterval; i++)
            {
                collectionA.Add(new Data() { Value1 = filterData[i * SampleInterval], Value = (e.CurrentPacket * currentChannel1.Length + i) * 1000.0 / ((int)Scope.SampleRate) });
            }

            ObservableCollection<Data> collectionB = new ObservableCollection<Data>();

            Analysis.MeanFilter(currentChannel2, 11, out filterData);

            //显示通道B数据
            for (int i = 0; i < currentChannel2.Length / SampleInterval; i++)
            {
                collectionB.Add(new Data() { Value1 = filterData[i * SampleInterval], Value = (e.CurrentPacket * currentChannel2.Length + i) * 1000.0 / ((int)Scope.SampleRate) });
            }

            //显示图像
            AppendScopeCHACollection(collectionA);
            AppendScopeCHBCollection(collectionB);
#endif

            //当最后一包数据,则注销事件,并在2S后启动下一次测量任务
            if (e.CurrentPacket == (e.TotalPacket - 1))
            {
                var scope = sender as IScopeBase;
                scope.ScopeReadDataCompleted -= Scope_ScopeReadDataCompleted;

                ShowCompleteGraph(globleChannel1, globleChannel2);

                //继续老化(若有)
                new Thread(() =>
                {
                    if ((currentBurnInTime % BackupInterval) == 0)
                    {
                        //备份截图
                        new Thread(() => { Thread.Sleep(1000); OnCompared(); }).Start();
                    }

                    currentBurnInTime++;
                    if (currentBurnInTime < TestTime)
                    {
                        Thread.Sleep(2000);
                        Start();
                    }
                }).Start();
                
            }

        }

        /// <summary>
        /// 导入配置信息文件
        /// </summary>
        /// <param name="file"></param>
        public void ImportConfigFile(string file)
        {
            ImportFile = file;
            ExternRecordData = JsonSerialization.DeserializeObjectFromFile<ExternRecordData>(file);
        }

        /// <summary>
        /// 导入模板文件
        /// </summary>
        /// <param name="file"></param>
        public void ImportTemplate(string file)
        {
            //导入模板
            var temp = JsonSerialization.DeserializeObjectFromFile<ObservableCollection<Data>>(file);

            if (temp != null)
            {
                Template = new ObservableCollection<Data>(temp);
            }
            else
            {
                OnMessageRaised(MessageLevel.Err, "文件无效");
            }
        }

        /// <summary>
        /// 导出模板文件
        /// </summary>
        /// <param name="file"></param>
        public void ExportTemplate(string file)
        {
            JsonSerialization.SerializeObjectToFile(Template, file);
            OnMessageRaised(MessageLevel.Message, "导出成功");
        }

        /// <summary>
        /// 设置当前为模板
        /// </summary>
        public void SetTemplate()
        {
            if (ScopeCHACollection != null)
            {
                //设置当前结果为模板
                Template = new ObservableCollection<Data>(ScopeCHACollection);
            }
            else
            {
                //Template = new ObservableCollection<Data>();
                OnMessageRaised(MessageLevel.Err, "设置模板失败");
            }
        }

        private int backupInterval = 5;

        /// <summary>
        /// 备份间隔
        /// </summary>
        public int BackupInterval
        {
            get { return backupInterval; }
            set { backupInterval = value; }
        }

        private int testTime;

        /// <summary>
        /// 测试次数
        /// </summary>
        public int TestTime
        {
            get { return testTime; }
            set { testTime = value; NotifyOfPropertyChange(() => TestTime); }
        }

        /// <summary>
        /// 当前老化测试次数
        /// </summary>
        private int currentBurnInTime = 0;

        /// <summary>
        /// 老化测试
        /// </summary>
        public void BurnIn()
        {
            currentBurnInTime = 0;
            Start();

            new Thread(() => 
            { 


            }).Start();

        }

        #endregion
    }
}
