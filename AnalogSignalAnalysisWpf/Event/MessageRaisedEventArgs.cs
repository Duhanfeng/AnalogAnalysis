using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalogSignalAnalysisWpf.Event
{
    public enum MessageLevel
    {
        /// <summary>
        /// 消息
        /// </summary>
        [Description("消息")] 
        Message,

        /// <summary>
        /// 警告
        /// </summary>
        [Description("警告")] 
        Warning,

        /// <summary>
        /// 错误
        /// </summary>
        [Description("错误")] 
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
