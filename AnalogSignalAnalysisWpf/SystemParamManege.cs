﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AnalogSignalAnalysisWpf.Hardware.Scope.LOTOA02;

namespace AnalogSignalAnalysisWpf
{
    #region 测量参数

    /// <summary>
    /// 频率测量参数
    /// </summary>
    public class FrequencyMeasureParams
    {
        /// <summary>
        /// 最小电压阈值(单位:V)
        /// </summary>
        public double MinVoltageThreshold { get; set; } = 1.5;

        /// <summary>
        /// 最大电压阈值(单位:V)
        /// </summary>
        public double MaxVoltageThreshold { get; set; } = 8.0;

        /// <summary>
        /// 频率误差
        /// </summary>
        public double FrequencyErrLimit { get; set; } = 0.2;

        /// <summary>
        /// 通信延迟(MS)
        /// </summary>
        public int ComDelay { get; set; } = 200;
    }

    /// <summary>
    /// 输入输出测量参数
    /// </summary>
    public class InputOutputMeasureParams
    {
        /// <summary>
        /// 电压间隔(V)
        /// </summary>
        public double VoltageInterval { get; set; } = 0.25;

        /// <summary>
        /// 最小电压(V)
        /// </summary>
        public double MinVoltage { get; set; } = 1.0;

        /// <summary>
        /// 最大电压(V)
        /// </summary>
        public double MaxVoltage { get; set; } = 15.0;

        /// <summary>
        /// 采样时间(MS)
        /// </summary>
        public int SampleTime { get; set; } = 200;

        /// <summary>
        /// 通信延迟(MS)
        /// </summary>
        public int ComDelay { get; set; } = 200;
    }

    /// <summary>
    /// 吸合释放电压测量参数
    /// </summary>
    public class PNVoltageMeasureParams
    {
        /// <summary>
        /// 最小电压阈值(单位:V)
        /// </summary>
        public double MinVoltageThreshold { get; set; } = 1.5;

        /// <summary>
        /// 最大电压阈值(单位:V)
        /// </summary>
        public double MaxVoltageThreshold { get; set; } = 8.0;

        /// <summary>
        /// 频率误差
        /// </summary>
        public double FrequencyErrLimit { get; set; } = 0.2;

        /// <summary>
        /// 电压间隔(V)
        /// </summary>
        public double VoltageInterval { get; set; } = 0.25;

        /// <summary>
        /// 最小电压(V)
        /// </summary>
        public double MinVoltage { get; set; } = 1.0;

        /// <summary>
        /// 最大电压(V)
        /// </summary>
        public double MaxVoltage { get; set; } = 15.0;

        /// <summary>
        /// 采样时间(MS)
        /// </summary>
        public int SampleTime { get; set; } = 500;

        /// <summary>
        /// 通信延迟(MS)
        /// </summary>
        public int ComDelay { get; set; } = 200;

    }

    /// <summary>
    /// 通气量测量参数
    /// </summary>
    public class ThroughputMeasureParams
    {
        /// <summary>
        /// 最小电压阈值(单位:V)
        /// </summary>
        public double MinVoltageThreshold { get; set; } = 1.5;

        /// <summary>
        /// 最大电压阈值(单位:V)
        /// </summary>
        public double MaxVoltageThreshold { get; set; } = 8.0;

        /// <summary>
        /// 输出电压(V)
        /// </summary>
        public double OutputVoltage { get; set; } = 12.0;

        /// <summary>
        /// 输出延时(MS)
        /// </summary>
        public int OutputDelay { get; set; } = 300;

        /// <summary>
        /// 采样时间(MS)
        /// </summary>
        public int SampleTime { get; set; } = 1000;

    }

    #endregion

    #region 硬件参数

    /// <summary>
    /// 示波器参数
    /// </summary>
    public class ScopeParams
    {
        /// <summary>
        /// CHA耦合
        /// </summary>
        public ECoupling CHACoupling { get; set; }

        /// <summary>
        /// CHB耦合
        /// </summary>
        public ECoupling CHBCoupling { get; set; }

        /// <summary>
        /// 采样率
        /// </summary>
        public ESampleRate SampleRate { get; set; }

        /// <summary>
        /// 触发模式
        /// </summary>
        public ETriggerModel TriggerModel { get; set; }

        /// <summary>
        /// 触发边沿
        /// </summary>
        public ETriggerEdge TriggerEdge { get; set; }

        /// <summary>
        /// CHA电压档位
        /// </summary>
        public EVoltageDIV CHAVoltageDIV { get; set; }

        /// <summary>
        /// CHB电压档位
        /// </summary>
        public EVoltageDIV CHBVoltageDIV { get; set; }

        /// <summary>
        /// 使能CHA通道
        /// </summary>
        public bool IsCHAEnable { get; set; }

        /// <summary>
        /// 使能CHB通道
        /// </summary>
        public bool IsCHBEnable { get; set; }

    }

    public class PWMParams
    {
        /// <summary>
        /// 串口
        /// </summary>
        public string PrimarySerialPortName { get; set; }

        /// <summary>
        /// 频率(Hz)
        /// </summary>
        public int Frequency { get; set; }

        /// <summary>
        /// 占空比
        /// </summary>
        public double DutyRatio { get; set; }
    }

    /// <summary>
    /// PWM参数
    /// </summary>
    public class PLCParams
    {
        #region Modbus配置参数

        /// <summary>
        /// 串口号
        /// </summary>
        public string PrimarySerialPortName { get; set; }

        /// <summary>
        /// 串口波特率
        /// </summary>
        public int SerialPortBaudRate { get; set; }

        /// <summary>
        /// 从站地址
        /// </summary>
        public byte SlaveAddress { get; set; }

        /// <summary>
        /// 写超时
        /// </summary>
        public int WriteTimeout { get; set; }

        /// <summary>
        /// 读超时
        /// </summary>
        public int ReadTimeout { get; set; }

        #endregion

        #region 控制接口

        /// <summary>
        /// 电压值
        /// </summary>
        public double Voltage { get; set; }

        /// <summary>
        /// 电流值
        /// </summary>
        public double Current { get; set; }

        /// <summary>
        /// 使能
        /// </summary>
        public bool Enable { get; set; }

        #endregion

    }

    #endregion

    public class SystemParam
    {
        #region 硬件参数

        /// <summary>
        /// 示波器参数
        /// </summary>
        public ScopeParams ScopeParams { get; set; }

        /// <summary>
        /// PWM参数
        /// </summary>
        public PWMParams PWMParams { get; set; }

        /// <summary>
        /// PLC参数
        /// </summary>
        public PLCParams PLCParams { get; set; }

        #endregion

        #region 测量参数

        /// <summary>
        /// 频率测量参数
        /// </summary>
        public FrequencyMeasureParams FrequencyMeasureParams { get; set; }

        /// <summary>
        /// 输入输出测量参数
        /// </summary>
        public InputOutputMeasureParams InputOutputMeasureParams { get; set; }

        /// <summary>
        /// 吸合释放电压测量参数
        /// </summary>
        public PNVoltageMeasureParams PNVoltageMeasureParams { get; set; }

        /// <summary>
        /// 通气量测量参数
        /// </summary>
        public ThroughputMeasureParams ThroughputMeasureParams { get; set; }

        #endregion
    }

    public class SystemParamManege
    {


    }
}
