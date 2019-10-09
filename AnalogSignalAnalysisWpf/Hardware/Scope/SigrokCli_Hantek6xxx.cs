using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalogSignalAnalysisWpf.Hardware.Scope
{
    public class SigrokCli_Hantek6xxx : IScope
    {
        #region Sigrok-cli交互

        /// <summary>
        /// 程序路径
        /// </summary>
        private static string Execute = @"C:\Program Files (x86)\sigrok\sigrok-cli\sigrok-cli.exe";

        /// <summary>
        /// 执行指令
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private static string ExecuteCmd(string cmd)
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = Execute;
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.StartInfo.Arguments = cmd;
            p.Start();//启动程序

            ////拼接指令
            //string str = string.Format(@"""{0}"" {1} {2}", Execute, cmd, "&exit");

            ////向cmd窗口发送输入信息
            //p.StandardInput.WriteLine(str);

            //p.StandardInput.AutoFlush = true;
            ////p.StandardInput.WriteLine("exit");
            ////向标准输入写入要执行的命令。这里使用&是批处理命令的符号，表示前面一个命令不管是否执行成功都执行后面(exit)命令，如果不执行exit命令，后面调用ReadToEnd()方法会假死
            ////同类的符号还有&&和||前者表示必须前一个命令执行成功才会执行后面的命令，后者表示必须前一个命令执行失败才会执行后面的命令

            //获取cmd窗口的输出信息
            string output = p.StandardOutput.ReadToEnd();

            //StreamReader reader = p.StandardOutput;
            //string line=reader.ReadLine();
            //while (!reader.EndOfStream)
            //{
            //    str += line + "  ";
            //    line = reader.ReadLine();
            //}

            p.WaitForExit();//等待程序执行完退出进程
            p.Close();

            return output;
        }

        private class CHData
        {
            public string CH { get; set; }
        }

        private static void GetData(string file, out double[] Data)
        {
            if (!File.Exists(file))
            {
                throw new FileNotFoundException($"{nameof(file)}:{file} not found!");
            }

            try
            {
                using (var reader = new StreamReader(file))
                {
                    using (var csv = new CsvReader(reader))
                    {
                        csv.Configuration.HasHeaderRecord = false;
                        csv.Configuration.Comment = ';';
                        csv.Configuration.AllowComments = true;
                        //csv.Configuration.RegisterClassMap<FooMap>();
                        var records = csv.GetRecords<CHData>();
                        //deviceData = records.ToArray();
                        var str = records.ToList().ConvertAll(x => x.CH);

                        Data = (
                        from val in str
                        where !val.Contains("CH")
                        select val).ToList().ConvertAll(x => double.Parse(x)).ToArray();
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
           
        }


        #endregion
        /// <summary>
        /// 设备连接标志
        /// </summary>
        public bool IsConnect { get; private set; }

        /// <summary>
        /// 连接设备
        /// </summary>
        /// <param name="devIndex">设备索引</param>
        /// <returns>执行结果</returns>
        public bool Connect(int devIndex)
        {
            //搜索设备
            string result = ExecuteCmd("--scan");

            IsConnect = result.Contains("hantek-6xxx");

            return IsConnect;
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            IsConnect = false;
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="channelIndex">通道索引</param>
        /// <param name="channelData">通道数据</param>
        public void ReadData(int channelIndex, out double[] channelData)
        {
            if (!IsConnect)
            {
                throw new Exception("示波器未连接!");
            }
            if (channelIndex >= 2)
            {
                throw new ArgumentException("通道超限");
            }

            string channel = (channelIndex == 0) ? "CH1" : "CH2";

            string result = ExecuteCmd($"-d hantek-6xxx --time {SampleTime} -C {channel} -o d:\\example.csv -O csv:dedup:header=true");
            Console.WriteLine(result);

            throw new InvalidOperationException();
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="channelData1">通道1数据</param>
        /// <param name="channelData2">通道2数据</param>
        public void ReadData(out double[] channelData1, out double channelData2)
        {
            throw new InvalidOperationException();
        }

        #region 设备参数设置

        /// <summary>
        /// 通道1电压档位
        /// </summary>
        public EVoltageDIV CH1VoltageDIV { get; set; }

        /// <summary>
        /// 通道2电压档位
        /// </summary>
        public EVoltageDIV CH2VoltageDIV { get; set; }

        /// <summary>
        /// 采样率
        /// </summary>
        public ESampleRate SampleRate { get; set; }

        /// <summary>
        /// 扫频模式
        /// </summary>
        public ETriggerSweep TriggerSweep { get; set; }

        /// <summary>
        /// 触发源
        /// </summary>
        public ETriggerSource TriggerSource { get; set; }

        /// <summary>
        /// 触发电平
        /// </summary>
        public int TriggerLevel { get; set; }

        /// <summary>
        /// 触发边沿
        /// </summary>
        public ETriggerSlope TriggerSlope { get; set; }

        /// <summary>
        /// 差值方式
        /// </summary>
        public EInsertMode InsertMode { get; set; }

        /// <summary>
        /// 采集时长(MS)
        /// </summary>
        public int SampleTime { get; set; } = 200;

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
        // ~Hantek66022BE()
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
