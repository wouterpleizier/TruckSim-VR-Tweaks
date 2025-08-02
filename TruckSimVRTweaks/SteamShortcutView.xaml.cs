using System.Diagnostics;
using System.Windows;

namespace TruckSimVRTweaks
{
    public partial class SteamShortcutView : Window
    {
        public SteamShortcutView()
        {
            InitializeComponent();
        }

        private void HandleHyperlinkClicked(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
