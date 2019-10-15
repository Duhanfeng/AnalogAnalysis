using NModbus;
using NModbus.Serial;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalogSignalAnalysisWpf.Hardware.PLC
{
    class ModbusPLC : IPLC
    {
        #region 构造函数

        /// <summary>
        /// 创建ModbusPLC新实例
        /// </summary>
        /// <param name="portName"></param>
        public ModbusPLC(string portName, int baudRate = 115200)
        {
            PrimarySerialPortName = portName;
            SerialPortBaudRate = baudRate;
        }

        #endregion

        #region Modbus接口

        /// <summary>
        /// 写单个寄存器
        /// </summary>
        /// <param name="slaveAddress">从站地址</param>
        /// <param name="registerAddress">寄存器地址</param>
        /// <param name="value">数据</param>
        private void ModbusSerialRtuMasterWriteRegister(byte slaveAddress, ushort registerAddress, ushort value)
        {
            using (SerialPort port = new SerialPort(PrimarySerialPortName))
            {
                //配置串口
                port.BaudRate = SerialPortBaudRate;
                port.DataBits = 8;
                port.Parity = Parity.None;
                port.StopBits = StopBits.One;
                port.Open();

                //创建Modbus主机
                var adapter = new SerialPortAdapter(port);
                adapter.WriteTimeout = 500;
                adapter.ReadTimeout = 500;
                var factory = new ModbusFactory();
                IModbusMaster master = factory.CreateRtuMaster(adapter);

                //写到寄存器
                master.WriteSingleRegister(slaveAddress, registerAddress, value);
            }
        }

        /// <summary>
        /// 写多个寄存器
        /// </summary>
        /// <param name="slaveAddress">从站地址</param>
        /// <param name="registerAddress">寄存器地址</param>
        /// <param name="data">数据</param>
        private void ModbusSerialRtuMasterWriteRegister(byte slaveAddress, ushort registerAddress, ushort[] data)
        {
            using (SerialPort port = new SerialPort(PrimarySerialPortName))
            {
                //配置串口
                port.BaudRate = SerialPortBaudRate;
                port.DataBits = 8;
                port.Parity = Parity.None;
                port.StopBits = StopBits.One;
                port.Open();

                //创建Modbus主机
                var adapter = new SerialPortAdapter(port);
                adapter.ReadTimeout = ReadTimeout;
                adapter.WriteTimeout = WriteTimeout;
                var factory = new ModbusFactory();
                IModbusMaster master = factory.CreateRtuMaster(adapter);

                //写到寄存器
                master.WriteMultipleRegisters(slaveAddress, registerAddress, data);
            }
        }

        /// <summary>
        /// 读单个寄存器
        /// </summary>
        /// <param name="slaveAddress">从站地址</param>
        /// <param name="registerAddress">寄存器地址</param>
        /// <param name="value">数据</param>
        private void ModbusSerialRtuMasterReadRegister(byte slaveAddress, ushort registerAddress, out ushort value)
        {
            value = 0xFFFF;
            using (SerialPort port = new SerialPort(PrimarySerialPortName))
            {
                //配置串口
                port.BaudRate = SerialPortBaudRate;
                port.DataBits = 8;
                port.Parity = Parity.None;
                port.StopBits = StopBits.One;
                port.Open();

                //创建Modbus主机
                var adapter = new SerialPortAdapter(port);
                adapter.WriteTimeout = 500;
                adapter.ReadTimeout = 500;
                var factory = new ModbusFactory();
                IModbusMaster master = factory.CreateRtuMaster(adapter);

                //读寄存器
                var values = master.ReadInputRegisters(slaveAddress, registerAddress, 1);
                if (values?.Length >= 1)
                {
                    value = values[0];
                }

            }
        }

        /// <summary>
        /// 读多个寄存器
        /// </summary>
        /// <param name="slaveAddress">从站地址</param>
        /// <param name="registerAddress">寄存器地址</param>
        /// <param name="value">数据</param>
        private void ModbusSerialRtuMasterReadRegister(byte slaveAddress, ushort registerAddress, ushort numberOfPoints, out ushort[] data)
        {
            data = null;
            using (SerialPort port = new SerialPort(PrimarySerialPortName))
            {
                //配置串口
                port.BaudRate = SerialPortBaudRate;
                port.DataBits = 8;
                port.Parity = Parity.None;
                port.StopBits = StopBits.One;
                port.Open();

                //创建Modbus主机
                var adapter = new SerialPortAdapter(port);
                adapter.ReadTimeout = ReadTimeout;
                adapter.WriteTimeout = WriteTimeout;
                var factory = new ModbusFactory();
                IModbusMaster master = factory.CreateRtuMaster(adapter);

                //读寄存器
                data = master.ReadInputRegisters(slaveAddress, registerAddress, numberOfPoints);
            }
        }

        #endregion

        #region Modbus配置参数

        /// <summary>
        /// 串口号
        /// </summary>
        public string PrimarySerialPortName { get; set; } = "COM1";

        /// <summary>
        /// 串口波特率
        /// </summary>
        public int SerialPortBaudRate { get; set; } = 115200;

        /// <summary>
        /// 从站地址
        /// </summary>
        public byte SlaveAddress { get; set; } = 0x01;

        /// <summary>
        /// 写超时
        /// </summary>
        public int WriteTimeout { get; set; } = 500;

        /// <summary>
        /// 读超时
        /// </summary>
        public int ReadTimeout { get; set; } = 500;

        #endregion

        #region Modbus控制接口

        /// <summary>
        /// 设备连接标志
        /// </summary>
        public bool IsConnect { get; private set; } = false;

        /// <summary>
        /// 连接设备
        /// </summary>
        /// <returns>执行结果</returns>
        public bool Connect()
        {
            ushort data;
            if (Read(VoltageAddress, out data))
            {
                IsConnect = true;
                return true;
            }
            else
            {
                IsConnect = false;
                return false;
            }

        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            IsConnect = false;
        }

        /// <summary>
        /// 写参数
        /// </summary>
        /// <param name="register">寄存器位置</param>
        /// <param name="value">数值</param>
        /// <returns>执行结果</returns>
        public bool Write(ushort register, ushort value)
        {
            try
            {
                ModbusSerialRtuMasterWriteRegister(SlaveAddress, register, value);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 写参数
        /// </summary>
        /// <param name="register">寄存器位置</param>
        /// <param name="values">数值数值</param>
        /// <returns>执行结果</returns>
        public bool Write(ushort register, ushort[] values)
        {
            try
            {
                ModbusSerialRtuMasterWriteRegister(SlaveAddress, register, values);
            }
            catch (Exception)
            {
                return false;
            }
            return true;

        }

        /// <summary>
        /// 读参数
        /// </summary>
        /// <param name="register">寄存器位置</param>
        /// <param name="value">数值</param>
        /// <returns>执行结果</returns>
        public bool Read(ushort register, out ushort value)
        {
            value = 0xFFFF;

            try
            {
                ModbusSerialRtuMasterReadRegister(SlaveAddress, register, out value);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 读参数
        /// </summary>
        /// <param name="register">寄存器位置</param>
        /// <param name="values">数值数值</param>
        /// <returns>执行结果</returns>
        public bool Read(ushort register, ushort count, out ushort[] values)
        {
            values = new ushort[0];

            try
            {
                ModbusSerialRtuMasterReadRegister(SlaveAddress, register, count, out values);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
            
        }

        #endregion

        #region 控制接口

        #region 地址定义

        /// <summary>
        /// 电压地址
        /// </summary>
        private readonly ushort VoltageAddress = 0x00;

        /// <summary>
        /// 电流地址
        /// </summary>
        private readonly ushort CurrentAddress = 0x01;

        /// <summary>
        /// 开关地址
        /// </summary>
        private readonly ushort SwitchAddress = 0x02;

        /// <summary>
        /// 开关状态地址
        /// </summary>
        private readonly ushort SwitchStatusAddress = 0x1000;

        /// <summary>
        /// 实际电压地址
        /// </summary>
        private readonly ushort RealityVoltageAddress = 0x1001;

        /// <summary>
        /// 实际电流地址
        /// </summary>
        private readonly ushort RealityCurrentAddress = 0x1002;

        /// <summary>
        /// 实际温度地址
        /// </summary>
        private readonly ushort RealityTemperatureAddress = 0x1003;

        #endregion

        /// <summary>
        /// 电压比例系数
        /// </summary>
        public readonly int VoltageScale = 100;

        /// <summary>
        /// 电流比例系数
        /// </summary>
        public readonly int CurrentScale = 100;

        /// <summary>
        /// 温度比例系数
        /// </summary>
        public readonly int TemperatureScale = 100;

        /// <summary>
        /// 电压值
        /// </summary>
        public double Voltage
        { 
            get
            {
                if (IsConnect)
                {
                    ushort data;
                    Read(VoltageAddress, out data);
                    return (double)data / VoltageScale;
                }
                return -1;
            }
            set
            {
                if (IsConnect)
                {
                    if (value >= 0)
                    {
                        ushort data = (ushort)(value * VoltageScale);
                        Write(VoltageAddress, data);
                    }
                }
            }
        }

        /// <summary>
        /// 电流值
        /// </summary>
        public double Current
        {
            get
            {
                if (IsConnect)
                {
                    ushort data;
                    Read(CurrentAddress, out data);
                    return (double)data / CurrentScale;
                }
                return -1;
            }
            set
            {
                if (IsConnect)
                {
                    if (value >= 0)
                    {
                        ushort data = (ushort)(value * CurrentScale);
                        Write(CurrentAddress, data);
                    }
                }
            }
        }

        /// <summary>
        /// 使能
        /// </summary>
        public bool Enable
        {
            get
            {
                if (IsConnect)
                {
                    ushort data;
                    Read(SwitchStatusAddress, out data);
                    return (data != 0) ? true : false;
                }
                return false;
            }
            set
            {
                if (IsConnect)
                {
                    Write(SwitchAddress, (ushort)(value ? 1 : 0));
                }
            }
        }

        /// <summary>
        /// 开关频率
        /// </summary>
        public int Frequency { get; set; }

        /// <summary>
        /// 实际电压值
        /// </summary>
        public double RealityVoltage
        {
            get
            {
                if (IsConnect)
                {
                    ushort data;
                    Read(RealityVoltageAddress, out data);
                    return (double)data / VoltageScale;
                }
                return -1;
            }
        }

        /// <summary>
        /// 实际电压值
        /// </summary>
        public double RealityCurrent
        {
            get
            {
                if (IsConnect)
                {
                    ushort data;
                    Read(RealityCurrentAddress, out data);
                    return (double)data / CurrentScale;
                }
                return -1;
            }
        }

        /// <summary>
        /// 实际电压值
        /// </summary>
        public double RealityTemperature
        {
            get
            {
                if (IsConnect)
                {
                    ushort data;
                    Read(RealityTemperatureAddress, out data);
                    return (double)data / TemperatureScale;
                }
                return -1;
            }
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
        // ~PLC()
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
