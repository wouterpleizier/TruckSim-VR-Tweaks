using CommunityToolkit.Mvvm.ComponentModel;

namespace TruckSimVRTweaks
{
    public partial class Settings : ObservableObject
    {
        [NotifyPropertyChangedFor(nameof(IsMouseSimulationEnabled), nameof(IsMouseSimulationEnabledWithInputBinding))]
        [ObservableProperty] public partial MouseSimulationMode MouseSimulationMode { get; set; } = MouseSimulationMode.AlwaysDisabled;
        [ObservableProperty] public partial double MouseSimulationSensitivity { get; set; } = 50.0;
        
        [ObservableProperty] public partial string InputDeviceName { get; set; } = string.Empty;
        [ObservableProperty] public partial Guid InputDeviceGuid { get; set; } = Guid.Empty;
        [ObservableProperty] public partial InputBindings InputBindings { get; set; } = new();
        
        [ObservableProperty] public partial string GamePath { get; set; } = string.Empty;
        [ObservableProperty] public partial string GameArguments { get; set; } = string.Empty;

        public bool IsMouseSimulationEnabled => MouseSimulationMode
            is MouseSimulationMode.AlwaysEnabled
            or MouseSimulationMode.HoldToEnable
            or MouseSimulationMode.PressToToggle;

        public bool IsMouseSimulationEnabledWithInputBinding => MouseSimulationMode
            is MouseSimulationMode.HoldToEnable
            or MouseSimulationMode.PressToToggle;
    }

    public partial class InputBindings : ObservableObject
    {
        [ObservableProperty] public partial InputBinding SimulateMouse { get; set; } = new();
        [ObservableProperty] public partial InputBinding MouseLeftClick { get; set; } = new();
        [ObservableProperty] public partial InputBinding MouseRightClick { get; set; } = new();
        [ObservableProperty] public partial InputBinding MouseScrollUp { get; set; } = new();
        [ObservableProperty] public partial InputBinding MouseScrollDown { get; set; } = new();
        [ObservableProperty] public partial InputBinding Escape { get; set; } = new();
    }

    public partial class InputBinding : ObservableObject
    {
        [ObservableProperty] public partial InputType Type { get; set; } = InputType.Unset;
        [ObservableProperty] public partial int Value { get; set; } = -1;
    }

    public enum InputType
    {
        Unset,
        Button,
        POV0,
        POV1,
        POV2,
        POV3,
    }

    public enum MouseSimulationMode
    {
        AlwaysDisabled,
        AlwaysEnabled,
        HoldToEnable,
        PressToToggle,
    }
}
