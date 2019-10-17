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
                if (collection?.Count == 4)
                {
                    (collection[0].Tag as UserControl).DataContext = (DataContext as MainWindowViewModel).FrequencyMeasurementViewModel;
                    (collection[1].Tag as UserControl).DataContext = (DataContext as MainWindowViewModel).PNVoltageMeasurementViewModel;
                    (collection[2].Tag as UserControl).DataContext = (DataContext as MainWindowViewModel).ThroughputMeasurementViewModel;
                    (collection[3].Tag as UserControl).DataContext = (DataContext as MainWindowViewModel).InputOutputMeasurementViewModel;
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