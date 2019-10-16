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
            var collection = HamburgerMenuControl.OptionsItemsSource as HamburgerMenuItemCollection;

            var collection2 = HamburgerMenuControl.OptionsItemsSource as HamburgerMenuItemCollection;
            (collection2[0].Tag as UserControl).DataContext = this.DataContext;
        }
    }
}