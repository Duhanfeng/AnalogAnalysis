﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalogDataAnalysisWpf
{
    public enum MessageLevel
    {
        /// <summary>
        /// 消息
        /// </summary>
        Message,

        /// <summary>
        /// 警告
        /// </summary>
        Warning,

        /// <summary>
        /// 错误
        /// </summary>
        Err,
    }

    public class MessageRaisedEventArgs : EventArgs
    {
        public MessageRaisedEventArgs(MessageLevel messageLevel, string message, Exception exception = null)
        {
            MessageLevel = messageLevel;
            Message = message;
            Exception = exception;
        }

        public MessageLevel MessageLevel { get; private set; }

        public string Message { get; private set; }

        public Exception Exception { get; private set; }
    }
}
