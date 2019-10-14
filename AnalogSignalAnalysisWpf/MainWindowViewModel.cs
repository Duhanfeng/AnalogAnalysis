using Caliburn.Micro;
using MahApps.Metro;
using AnalogSignalAnalysisWpf.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using AnalogSignalAnalysisWpf.Event;
using MahApps.Metro.Controls.Dialogs;
using System.Threading;
using MahApps.Metro.Controls;
using AnalogSignalAnalysisWpf.Hardware.Scope;
using AnalogSignalAnalysisWpf.Hardware.PLC;

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
            Scope = new Hantek66022BE();
            ScopeControlView = new ScopeControlView();
            ScopeControlView.DataContext = new ScopeControlViewModel(Scope);

            //创建PLC控件实例
            PLC = new ModbusPLC("COM1");
            PLCControlView = new PLCControlView();
            PLCControlView.DataContext = new PLCControlViewModel(PLC);

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

        #region 硬件

        /// <summary>
        /// 示波器
        /// </summary>
        public IScope Scope { get; set; }

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

        #endregion

        #region 子窗口

        private ScopeControlView scopeControlView;

        /// <summary>
        /// 示波器配置控件
        /// </summary>
        public ScopeControlView ScopeControlView
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
