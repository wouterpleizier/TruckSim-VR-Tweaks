using System.Windows.Threading;
using Vortice.DirectInput;

namespace TruckSimVRTweaks
{
    public class InputPoller
    {
        public delegate void ProgressHandler(TimeSpan timeUntilCanceled);
        public delegate void CompletedHandler((InputType Type, int Value)? result);

        public static InputPoller Instance => _instance.Value;

        public Guid InputDeviceGuid { get; set; } = Guid.Empty;
        public Dictionary<Guid, string> InputDeviceNames { get; } = new Dictionary<Guid, string>()
        {
            { Guid.Empty, "<none>" }
        };

        private static readonly TimeSpan _interval = TimeSpan.FromSeconds(1.0 / 60.0);
        private static readonly TimeSpan _maxDuration = TimeSpan.FromSeconds(5.0);
        private static readonly Lazy<InputPoller> _instance = new(() => new InputPoller());

        private readonly DispatcherTimer _timer = new(DispatcherPriority.Normal) { Interval = _interval };
        private DateTime _startTime = DateTime.MinValue;
        private InputBinding? _target = null;
        private ProgressHandler? _progressHandler = null;
        private CompletedHandler? _completedHandler = null;

        private readonly IDirectInput8? _directInput;
        private IDirectInputDevice8? _directInputDevice = null;

        private InputPoller()
        {
            try
            {
                _directInput = DInput.DirectInput8Create();

                foreach (DeviceInstance inputDevice in _directInput.GetDevices(
                    DeviceClass.GameControl,
                    DeviceEnumerationFlags.AttachedOnly))
                {
                    InputDeviceNames[inputDevice.InstanceGuid] = inputDevice.InstanceName;
                }
            }
            catch (Exception exception)
            {
                _directInput = null;
                MessageBoxUtil.ShowError("Failed to initialize DirectInput", exception);
            }

            _timer.Tick += HandleTimerTick;
        }

        public bool IsPolling(InputBinding inputBinding)
        {
            return _target == inputBinding && _timer.IsEnabled;
        }

        public void StartPolling(InputBinding inputBinding, ProgressHandler? progressHandler, CompletedHandler? completedHandler)
        {
            if (_target != null && _target != inputBinding)
            {
                StopPolling(result: null);
            }

            if (TryAcquireInputDevice())
            {
                _startTime = DateTime.UtcNow;
                _target = inputBinding;
                _progressHandler = progressHandler;
                _completedHandler = completedHandler;
                _timer.Start();
            }
            else
            {
                StopPolling(result: null);
            }
        }

        public void StopPolling(InputBinding inputBinding)
        {
            if (_target != null && _target == inputBinding)
            {
                StopPolling(result: null);
            }
        }

        private void StopPolling((InputType Type, int Value)? result)
        {
            CompletedHandler? completedHandler = _completedHandler;

            _target = null;
            _progressHandler = null;
            _completedHandler = null;
            _timer.Stop();

            completedHandler?.Invoke(result);
        }

        private bool TryAcquireInputDevice()
        {
            if (_directInput == null)
            {
                return false;
            }

            if (_directInputDevice != null && _directInputDevice.DeviceInfo.InstanceGuid != InputDeviceGuid)
            {
                _directInputDevice.Unacquire();
                _directInputDevice.Dispose();
                _directInputDevice = null;
            }

            if (_directInputDevice == null)
            {
                try
                {
                    _directInputDevice = _directInput.CreateDevice(InputDeviceGuid);
                    _directInputDevice.SetCooperativeLevel(0, CooperativeLevel.NonExclusive | CooperativeLevel.Background);
                    _directInputDevice.SetDataFormat<RawJoystickState>();
                }
                catch (Exception exception)
                {
                    _directInputDevice?.Dispose();
                    _directInputDevice = null;

                    MessageBoxUtil.ShowError($"Failed to initialize DirectInput device {{{InputDeviceGuid}}}", exception);
                    return false;
                }
            }

            if (_directInputDevice != null)
            {
                try
                {
                    _directInputDevice.Acquire();
                }
                catch (Exception exception)
                {
                    MessageBoxUtil.ShowError($"Failed to acquire DirectInput device {{{InputDeviceGuid}}}", exception);
                    return false;
                }
            }

            return _directInputDevice?.DeviceInfo.InstanceGuid == InputDeviceGuid;
        }

        private void HandleTimerTick(object? sender, EventArgs e)
        {
            (InputType Type, int Value)? result = null;
            if (_directInput != null && _directInputDevice != null)
            {
                try
                {
                    JoystickState state = _directInputDevice.GetCurrentJoystickState();

                    for (int i = 0; i < state.PointOfViewControllers.Length; i++)
                    {
                        int povValue = state.PointOfViewControllers[i];
                        if (povValue >= 0)
                        {
                            result = (InputType.POV0 + i, povValue);
                            break;
                        }
                    }

                    if (!result.HasValue)
                    {
                        for (int i = 0; i < state.Buttons.Length; i++)
                        {
                            if (state.Buttons[i])
                            {
                                result = (InputType.Button, i);
                                break;
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    MessageBoxUtil.ShowError($"Failed to read button state of DirectInput device {{{_directInputDevice.DeviceInfo.InstanceGuid}}}", exception);
                    StopPolling(result: null);
                }

                if (result.HasValue)
                {
                    StopPolling(result);
                }
                else
                {
                    TimeSpan timeRemaining = _startTime + _maxDuration - DateTime.UtcNow;
                    if (timeRemaining > TimeSpan.Zero)
                    {
                        _progressHandler?.Invoke(timeRemaining);
                    }
                    else
                    {
                        StopPolling(result: null);
                    }
                }
            }
        }
    }
}
