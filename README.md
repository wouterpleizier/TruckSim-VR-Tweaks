# TruckSim VR Tweaks

VR / steering wheel improvements for Euro Truck Simulator 2 and American Truck Simulator. May also work for other games to some degree.

## Features
When playing ETS2/ATS with a steering wheel, the game still relies on mouse and keyboard input for menu navigation and certain other functions (e.g. skipping cutscenes). This can be annoying in VR when your mouse/keyboard is out of reach or difficult to see.

TruckSim VR Tweaks aims to fix this by letting you move the mouse cursor using your VR headset's orientation. Additionally, you can simulate mouse clicks, scrolling and keypresses using buttons on your steering wheel.

| Configuration screenshot | In-game video |
| ------------------------ | ------------- |
| <img width="673" height="463" alt="screenshot" src="https://github.com/user-attachments/assets/692ddd31-e14a-4b96-8e54-0bcce42bb77a" /> | <video src="https://github.com/user-attachments/assets/2f7e6523-9e92-4717-b08e-0ce8f5462d9f" /> |

## Requirements
- A DirectInput-compatible input device (most steering wheels for Windows should work)
- An OpenXR-compatible VR headset (Oculus/Meta, HTC, Valve, WMR, Varjo, Pimax, Pico, etc)
- Euro Truck Simulator 2 (1.50 or higher), American Truck Simulator (1.50 or higher) or some other OpenXR-supported game.

## Download
> [!CAUTION]
> TruckSim VR Tweaks may be flagged as a cheat when used in games with anti-cheat software. Use at your own risk.

Download the latest version here: https://github.com/wouterpleizier/TruckSim-VR-Tweaks/releases/latest

Extract the downloaded zip's contents to an empty folder somewhere, then run `TruckSimVRTweaks.exe` to get started.

## Instructions / tips
Most settings are hopefully fairly self-explanatory, but here's an overview just in case.

### General settings
| Setting            | Description |
| ------------------ | ----------- |
| Input device       | The device you want to use for simulating mouse/keyboard inputs. You'll probably want to set this to the same steering wheel that you're using in-game. |
| Mouse simulation   | Determines when mouse movement, clicking and scrolling is simulated. When using `Hold to enable` or `Press to toggle`, the desired hold/toggle button must be assigned to `Simulate mouse` under [Input bindings](#input-bindings). |
| Sensitivity        | How fast/far the mouse cursor moves (in pixels) based on your VR headset's pitch/yaw (in degrees). `50` is a good starting point; use a smaller value if you need more precision, or a bigger value for more speed. |

### Input bindings
After choosing an input device in the previous section, you can assign its buttons and D-pad/POV directions to the specified actions. For reference, I prefer the following input bindings when using my Thrustmaster TMX Force Feedback steering wheel:
- Escape: Start/Menu button
- Simulate mouse: RB button
- Left/right click: D-pad left/right
- Scroll up/down: D-pad up/down

Note that when a button is assigned to multiple actions (in TruckSim VR Tweaks, in-game or both), pressing the button may trigger each of those actions simultaneously and lead to unexpected behavior. For this reason, you'll probably want to ensure that the `Escape` and `Simulate mouse` buttons are not bound to anything else.

For the click and scroll buttons, however, you usually don't need to worry about overlapping/conflicting with in-game input bindings. For example, when `D-pad left` is assigned to `Left click` in TruckSim VR Tweaks and to `Left-Turn Indicator` in-game, both of these actions may technically be triggered simultaneously, but the former will only function in menus and the latter will only function while driving.

### Game
| Setting                 | Description |
| ----------------------- | ----------- |
| Game path               | The path to the game's executable file. This depends entirely on the game and where you've installed it, but it'll probably look similar to this (assuming ETS2): `C:\Steam\steamapps\common\Euro Truck Simulator 2\bin\win_x64\eurotrucks2.exe` |
| Arguments               | The command line options that you want to pass along to the game. For ETS2/ATS, you'll want to at least include `-openxr` (and `-experimental_vr` when using the regular branch in 1.55 and up), as TruckSim VR Tweaks only works in OpenXR mode. |

**`Play` button:**<br>Play the game using the current settings. (Note: the TruckSim VR Tweaks window will remain open, but any settings that are changed during gameplay will not be applied until after you close the game and click `Play` again)

**`Create desktop shortcut` button:**<br>Create a desktop shortcut that will start the game and automatically run TruckSim VR Tweaks in the background.

**`Create Steam shortcut` button:**<br>Displays instructions on changing the game's launch options in Steam in order to automatically run TruckSim VR Tweaks in the background whenever you play it.

> [!WARNING]
> Many Steam games (including ETS2/ATS) will forcibly launch via Steam, even when the game's executable is launched directly. In most cases, TruckSim VR Tweaks should still work when clicking `Play` or using a desktop shortcut, though Steam may ask you to confirm whether you want to launch the game with the specified launch options. If TruckSim VR Tweaks doesn't work or if you want to skip the confirmation, you should click the `Create Steam shortcut` button and follow the shown instructions. You can then close the TruckSim VR Tweaks window and use Steam to launch the game or create a desktop shortcut.

> [!TIP]
> Unlike desktop shortcuts, Steam shortcuts specify the game path as `%command%`, which Steam automatically replaces with the game path that it would normally use when playing the game. If you want to use TruckSim VR Tweaks in multiple Steam games (e.g. both ETS2 and ATS), you can copy and paste the launch options without making any changes (aside from the arguments, if necessary).

## Troubleshooting
### TruckSim VR Tweaks doesn't start/work
If nothing happens or an error occurs when either `TruckSimVRTweaks.exe` or the game is launched, you may need to download and install the following redistributables from Microsoft:
- https://builds.dotnet.microsoft.com/dotnet/WindowsDesktop/9.0.8/windowsdesktop-runtime-9.0.8-win-x64.exe
- https://aka.ms/vs/17/release/vc_redist.x64.exe

If this doesn't fix the problem, additional information may be provided in a file named `XR_APILAYER_NOVENDOR_trucksim_vr_tweaks.log` in your TruckSim VR Tweaks directory. If the file doesn't exist, look in the following directory instead:<br>`%localappdata%\XR_APILAYER_NOVENDOR_trucksim_vr_tweaks`

### Other OpenXR applications
TruckSim VR Tweaks uses its own OpenXR API layer, similar to tools like OpenXR Toolkit. As TruckSim VR Tweaks is intended to be used for specific games only, the API layer is automatically disabled after the game and/or the TruckSim VR Tweaks window closes. However, if the process ends unexpectedly (e.g. Windows crashes or your PC suddenly loses power), the API layer may remain enabled and affect other OpenXR applications as well.

To fix this, simply run `TruckSimVRTweaks.exe` and then close it again.

(If the above doesn't help, try removing or disabling the `XR_APILAYER_NOVENDOR_trucksim_vr_tweaks` layer using [OpenXR API Layers GUI](https://github.com/fredemmott/OpenXR-API-Layers-GUI), or by removing it from your registry under `HKEY_CURRENT_USER\Software\Khronos\OpenXR\1\ApiLayers\Implicit`)

## Credits/acknowledgments
- Created by Wouter Pleizier a.k.a. Blueberry_pie
- Thanks to [Matthieu Bucchianeri](https://github.com/mbucchia) for the OpenXR API Layer template/tutorial/docs
- Thanks to [Rectus](https://github.com/Rectus) for advice and an additional code reference

Euro Truck Simulator 2 and American Truck Simulator are Copyright (c) 2012-2025 SCS Software. TruckSim VR Tweaks is not affiliated with SCS Software.
