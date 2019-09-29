using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AnalogSignalAnalysisWpf
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private bool _shutdown = false;
        private readonly MainWindowViewModel _viewModel;

        public MainWindow()
        {
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;
            _viewModel.MessageRaised += ViewModel_MessageRaised;

            InitializeComponent();
        }

        private void ViewModel_MessageRaised(object sender, Event.MessageRaisedEventArgs e)
        {
            ((MetroWindow)Application.Current.MainWindow).ShowMessageAsync(EnumHelper.GetDescription(e.MessageLevel), e.Message);

        }

        /// <summary>
        /// 确认关闭
        /// </summary>
        /// <returns></returns>
        private async Task ConfirmShutdown()
        {
            var mySettings = new MetroDialogSettings
            {
                AffirmativeButtonText = "退出",
                NegativeButtonText = "取消",
                AnimateShow = true,
                AnimateHide = false
            };

            var result = await this.ShowMessageAsync("退出应用?",
                                                     "是否确认退出应用?",
                                                     MessageDialogStyle.AffirmativeAndNegative, mySettings);

            _shutdown = result == MessageDialogResult.Affirmative;

            if (_shutdown)
            {
                _viewModel.Dispose();
                Application.Current.Shutdown();
            }
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (e.Cancel)
            {
                return;
            }

            if (_viewModel.QuitConfirmationEnabled
                && _shutdown == false)
            {
                e.Cancel = true;

                // We have to delay the execution through BeginInvoke to prevent potential re-entrancy
                Dispatcher.BeginInvoke(new Action(async () => await this.ConfirmShutdown()));
            }
            else
            {
                _viewModel.Dispose();
            }
        }

        private void ShowInverse(object sender, RoutedEventArgs e)
        {
            var flyout = Flyouts.Items[0] as Flyout;
            if (flyout == null)
            {
                return;
            }

            flyout.IsOpen = !flyout.IsOpen;
        }

        /// <summary>
        /// 示波器配置界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScopeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var flyout = Flyouts.Items[0] as Flyout;
            if (flyout == null)
            {
                return;
            }

            flyout.IsOpen = !flyout.IsOpen;
        }

        /// <summary>
        /// PLC配置界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PLCMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var flyout = Flyouts.Items[1] as Flyout;
            if (flyout == null)
            {
                return;
            }

            flyout.IsOpen = !flyout.IsOpen;
        }

        private void ShowDynamicFlyout(object sender, RoutedEventArgs e)
        {
            var flyout = new Flyout
            {
                Header = "Dynamic flyout"
            };

            // when the flyout is closed, remove it from the hosting FlyoutsControl
            RoutedEventHandler closingFinishedHandler = null;
            closingFinishedHandler = (o, args) =>
            {
                flyout.ClosingFinished -= closingFinishedHandler;
                flyoutsControl.Items.Remove(flyout);
            };
            flyout.ClosingFinished += closingFinishedHandler;

            flyoutsControl.Items.Add(flyout);

            flyout.IsOpen = true;
        }
    }
}
