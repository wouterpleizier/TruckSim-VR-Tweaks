using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace TruckSimVRTweaks
{
    public partial class SteamShortcutViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial string LaunchOptionsText { get; private set; }

        public SteamShortcutViewModel(string gameArguments)
        {
            LaunchOptionsText = string.Join(' ',
                $"\"{Environment.ProcessPath}\"",
                $"-waitforexit",
                $"-gamepath %command%",
                $"-gameargs \"{gameArguments}\"");
        }

        [RelayCommand]
        private void CopyLaunchOptionsText()
        {
            Clipboard.SetText(LaunchOptionsText);
        }
    }
}
