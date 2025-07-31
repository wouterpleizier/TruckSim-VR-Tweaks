using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace TruckSimVRTweaks
{
    public partial class App : Application
    {
        private DispatcherTimer? _timer;

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;

            base.OnStartup(e);

            string? gamePath = null;
            string? gameArgs = null;
            bool waitForExit = false;

            Queue<string> arguments = new(e.Args);
            while (arguments.TryDequeue(out string? argument))
            {
                switch (argument.ToLowerInvariant())
                {
                    case "-gamepath":
                        if (!arguments.TryDequeue(out gamePath)) throw new Exception("No value specified for -gamepath");
                        break;

                    case "-gameargs":
                        if (!arguments.TryDequeue(out gameArgs)) throw new Exception("No value specified for -gameargs");
                        break;

                    case "-waitforexit":
                        waitForExit = true;
                        break;

                    default:
                        throw new Exception($"Unknown argument: {argument}");
                }
            }

            if (string.IsNullOrEmpty(gamePath))
            {
                StartupUri = new Uri($"{nameof(SettingsView)}.xaml", UriKind.Relative);
            }
            else
            {
                StartSilently(gamePath, gameArgs ?? string.Empty, waitForExit);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Just to be safe, don't touch the API layer if other instances of this program still happen to be running.
            if (GetAppInstanceCount() <= 1)
            {
                ApiLayerManager.DisableApiLayer();
            }

            base.OnExit(e);
        }

        private void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                MessageBoxResult result = MessageBoxUtil.ShowError(
                    "An unexpected error occurred",
                    (Exception)e.ExceptionObject,
                    "The program will now close. Would you like to copy diagnostic information to your clipboard?",
                    MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    Clipboard.SetText(e.ExceptionObject.ToString());
                }
            }
            finally
            {
                Environment.FailFast(null, e.ExceptionObject as Exception);
            }
        }

        private void StartSilently(string gamePath, string gameArgs, bool waitForExit)
        {
            string normalizedGamePath = Path.GetFullPath(gamePath);
            string gameProcessName = Path.GetFileName(normalizedGamePath);
            string gameProcessNameWithoutExtension = Path.GetFileNameWithoutExtension(normalizedGamePath);

            ApiLayerManager.EnableApiLayer();
            Process.Start(normalizedGamePath, gameArgs);

            // When running the executable directly, some games may choose to immediately exit and then restart via
            // Steam or some other launcher. So let's just wait a while and find the process ourselves.
            TimeSpan interval = TimeSpan.FromSeconds(10);
            _timer = new DispatcherTimer(
                interval,
                DispatcherPriority.Normal,
                (sender, _) =>
                {
                    (sender as DispatcherTimer)?.Stop();

                    if (FindProcess(gameProcessNameWithoutExtension, normalizedGamePath) is Process process)
                    {
                        if (waitForExit)
                        {
                            process.EnableRaisingEvents = true;
                            process.Exited += (_, _) =>
                            {
                                process.Dispose();
                                Dispatcher.Invoke(Shutdown);
                            };
                        }
                        else
                        {
                            process.Dispose();
                            Shutdown();
                        }
                    }
                    else
                    {
                        MessageBoxResult result = MessageBox.Show(
                            $"{nameof(TruckSimVRTweaks)} is waiting for {gameProcessName} to start, but it looks " +
                            $"like that hasn't happened yet (or the game has already exited). Would you like to wait " +
                            $"an additional {(int)interval.TotalSeconds} seconds?",
                            MessageBoxUtil.Caption,
                            MessageBoxButton.YesNo);

                        if (result == MessageBoxResult.Yes)
                        {
                            (sender as DispatcherTimer)?.Start();
                        }
                        else
                        {
                            Shutdown();
                        }
                    }
                },
                Dispatcher)
            {
                IsEnabled = true
            };
        }

        private static Process? FindProcess(string name, string preferredPath)
        {
            Process? result = null;
            Process[] processes = [];

            try
            {
                processes = Process.GetProcessesByName(name);
                if (processes.Length > 0)
                {
                    result = processes.FirstOrDefault(
                        predicate: process =>
                            process.MainModule?.FileName is string path
                            && string.Equals(path, preferredPath, StringComparison.OrdinalIgnoreCase),
                        defaultValue: processes[0]);
                }

                return result;
            }
            finally
            {
                foreach (Process process in processes)
                {
                    if (process != result)
                    {
                        process.Dispose();
                    }
                }
            }
        }

        private static int GetAppInstanceCount()
        {
            using Process currentProcess = Process.GetCurrentProcess();
            Process[] processes = [];

            try
            {
                processes = Process.GetProcessesByName(currentProcess.ProcessName);
                return processes.Count(process => string.Equals(
                    process.MainModule?.FileName,
                    currentProcess.MainModule?.FileName,
                    StringComparison.OrdinalIgnoreCase));
            }
            finally
            {
                foreach (Process process in processes)
                {
                    process.Dispose();
                }
            }
        }
    }
}