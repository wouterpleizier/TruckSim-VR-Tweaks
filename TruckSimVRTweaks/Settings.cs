using CommunityToolkit.Mvvm.ComponentModel;

namespace TruckSimVRTweaks
{
    public partial class Settings : ObservableObject
    {
        [NotifyPropertyChangedFor(nameof(IsSimulatedMouseEnabled), nameof(IsSimulatedMouseEnabledWithInputBinding))]
        [ObservableProperty] public partial SimulatedMouseTrigger SimulatedMouseTrigger { get; set; } = SimulatedMouseTrigger.AlwaysDisabled;
        [ObservableProperty] public partial double SimulatedMouseSensitivity { get; set; } = 50.0;
        
        [ObservableProperty] public partial string InputDeviceName { get; set; } = string.Empty;
        [ObservableProperty] public partial Guid InputDeviceGuid { get; set; } = Guid.Empty;
        [ObservableProperty] public partial InputBindings InputBindings { get; set; } = new();
        
        [ObservableProperty] public partial string GamePath { get; set; } = string.Empty;
        [ObservableProperty] public partial string GameArguments { get; set; } = string.Empty;

        public bool IsSimulatedMouseEnabled => SimulatedMouseTrigger
            is SimulatedMouseTrigger.AlwaysEnabled
            or SimulatedMouseTrigger.HoldToEnable
            or SimulatedMouseTrigger.PressToToggle;

        public bool IsSimulatedMouseEnabledWithInputBinding => SimulatedMouseTrigger
            is SimulatedMouseTrigger.HoldToEnable
            or SimulatedMouseTrigger.PressToToggle;
    }

    public partial class InputBindings : ObservableObject
    {
        [ObservableProperty] public partial InputBinding ToggleMouse { get; set; } = new();
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

    public enum SimulatedMouseTrigger
    {
        AlwaysDisabled,
        AlwaysEnabled,
        HoldToEnable,
        PressToToggle,
    }
}
