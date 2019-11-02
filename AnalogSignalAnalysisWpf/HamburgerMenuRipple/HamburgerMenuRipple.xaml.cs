using System.Windows.Controls;

namespace AnalogSignalAnalysisWpf
{
    using MahApps.Metro.Controls;
    using MahApps.Metro.IconPacks;

    public sealed partial class HamburgerMenuRipple : UserControl
    {
        public HamburgerMenuRipple()
        {
            this.InitializeComponent();
            
        }

        private void HamburgerMenuControl_OnItemInvoked(object sender, HamburgerMenuItemInvokedEventArgs e)
        {
            HamburgerMenuControl.Content = e.InvokedItem;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //主动设置绑定源
            if (DataContext is MainWindowViewModel)
            {
                var collection = HamburgerMenuControl.ItemsSource as HamburgerMenuItemCollection;

                foreach (var item in collection)
                {
                    if (item.Tag is FrequencyMeasurementView)
                    {
                        (item.Tag as FrequencyMeasurementView).DataContext = (DataContext as MainWindowViewModel).FrequencyMeasurementViewModel;
                    }
                    else if (item.Tag is PNVoltageMeasurementView)
                    {
                        (item.Tag as PNVoltageMeasurementView).DataContext = (DataContext as MainWindowViewModel).PNVoltageMeasurementViewModel;
                    }
                    else if (item.Tag is InputOutputMeasurementView)
                    {
                        (item.Tag as InputOutputMeasurementView).DataContext = (DataContext as MainWindowViewModel).InputOutputMeasurementViewModel;
                    }
                    else if (item.Tag is ThroughputMeasurementView)
                    {
                        (item.Tag as ThroughputMeasurementView).DataContext = (DataContext as MainWindowViewModel).ThroughputMeasurementViewModel;
                    }
                    else if (item.Tag is BurnInTestView)
                    {
                        (item.Tag as BurnInTestView).DataContext = (DataContext as MainWindowViewModel).BurnInTestViewModel;
                    }
                }

            }
            
            var collection2 = HamburgerMenuControl.OptionsItemsSource as HamburgerMenuItemCollection;
            foreach (var item in collection2)
            {
                (item.Tag as UserControl).DataContext = DataContext;
            }
        }
    }
}