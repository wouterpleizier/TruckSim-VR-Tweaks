using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace TruckSimVRTweaks
{
    public partial class SettingsViewModel : ObservableObject
    {
        private static string SettingsPath { get; } = Path.Combine(AppContext.BaseDirectory, "TruckSimVRTweaks.json");

        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            IgnoreReadOnlyProperties = true,
            WriteIndented = true,
            Converters =
            {
                new JsonStringEnumConverter(namingPolicy: null, allowIntegerValues: false),
                new JsonStringGuidConverter()
            }
        };

        [ObservableProperty]
        public partial Settings Settings { get; private set; }

        public Guid InputDeviceGuid
        {
            get { return Settings.InputDeviceGuid; }
            set
            {
                Guid previousValue = Settings.InputDeviceGuid;

                if (value != Guid.Empty && InputPoller.Instance.InputDeviceNames.TryGetValue(value, out var name))
                {
                    InputPoller.Instance.InputDeviceGuid = value;
                    Settings.InputDeviceGuid = value;
                    Settings.InputDeviceName = name;
                }
                else
                {
                    InputPoller.Instance.InputDeviceGuid = Guid.Empty;
                    Settings.InputDeviceGuid = Guid.Empty;
                    Settings.InputDeviceName = string.Empty;
                }

                if (previousValue != value)
                {
                    OnPropertyChanged();
                }
            }
        }

        public Dictionary<SimulatedMouseTrigger, string> SimulatedMouseTriggers { get; } = new()
        {
            { SimulatedMouseTrigger.AlwaysDisabled, "Disabled" },
            { SimulatedMouseTrigger.AlwaysEnabled, "Always enabled" },
            { SimulatedMouseTrigger.HoldToEnable, "Hold button to enable" },
            { SimulatedMouseTrigger.PressToToggle, "Press button to toggle" },
        };

        public SettingsViewModel()
        {
            Settings? settings = null;

            if (File.Exists(SettingsPath))
            {
                try
                {
                    using FileStream stream = File.OpenRead(SettingsPath);
                    settings = JsonSerializer.Deserialize<Settings>(stream, _jsonSerializerOptions);
                }
                catch (Exception exception)
                {
                    MessageBoxUtil.ShowError("Failed to load settings", exception, $"Default settings will be used.");
                }
            }

            Settings = settings ?? new();
            InputDeviceGuid = Settings.InputDeviceGuid;
        }

        public void HandleClosing(object? sender, CancelEventArgs e)
        {
            SaveSettings();
        }

        private void SaveSettings()
        {
            using FileStream stream = File.Create(SettingsPath);
            JsonSerializer.Serialize(stream, Settings, _jsonSerializerOptions);
        }

        [RelayCommand]
        private void BrowseGamePath()
        {
            string? defaultDirectory = Path.GetDirectoryName(Settings.GamePath);
            if (!Path.Exists(defaultDirectory))
            {
                defaultDirectory = AppContext.BaseDirectory;
            }

            Microsoft.Win32.OpenFileDialog dialog = new()
            {
                DefaultDirectory = defaultDirectory,
                Filter = "Euro Truck Simulator 2 / American Truck Simulator|eurotrucks2.exe;amtrucks.exe|All Executables|*.exe|All Files|*.*",
                DefaultExt = ".exe"
            };

            if (dialog.ShowDialog() is true)
            {
                Settings.GamePath = dialog.FileName;
            }
        }

        [RelayCommand]
        private void PlayGame()
        {
            SaveSettings();
            ApiLayerManager.EnableApiLayer();
            Process.Start(Settings.GamePath, Settings.GameArguments);
        }

        [RelayCommand]
        private void CreateDesktopShortcut()
        {
            try
            {
                IWshRuntimeLibrary.WshShell shell = new();

                string shortcutPath = Path.Combine(
                    shell.SpecialFolders.Item("Desktop"),
                    $"{App.Title} - {Path.GetFileNameWithoutExtension(Settings.GamePath)}.lnk");

                if (File.Exists(shortcutPath) && MessageBox.Show("Shortcut already exists. Overwrite?", App.Title,
                    MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    return;
                }

                IWshRuntimeLibrary.IWshShortcut shortcut = shell.CreateShortcut(shortcutPath);
                shortcut.TargetPath = Environment.ProcessPath;
                shortcut.Arguments = $"-gamepath \"{Settings.GamePath}\" -gameargs \"{Settings.GameArguments}\"";
                shortcut.IconLocation = Settings.GamePath;
                shortcut.Save();
            }
            catch (Exception exception)
            {
                MessageBoxUtil.ShowError("Failed to create desktop shortcut", exception);
            }
        }

        [RelayCommand]
        private void CreateSteamShortcut()
        {
            // TODO
        }
    }
}
