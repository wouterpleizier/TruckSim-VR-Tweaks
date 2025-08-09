#include "pch.h"

#include "inputtweaks.h"
#include "layer.h"
#include <log.h>
#include <WinUser.h>
#include <fstream>

void openxr_api_layer::InputTweaks::initialize() {
    if (!m_isInitialized) {
        try {
            std::ifstream stream(openxr_api_layer::dllHome / "TruckSimVRTweaks.json");
            nlohmann::json jsonSettings = nlohmann::json::parse(stream);
            m_settings = jsonSettings.template get<Settings>();
            log::Log(fmt::format("Loaded settings: {}\n", jsonSettings.dump(4)));
        } catch (nlohmann::json::parse_error& ex) {
            log::Log(fmt::format("Unable to parse settings at byte {}: {}\n", ex.byte, ex.what()));
            return;
        } catch (std::exception& ex) {
            log::Log(fmt::format("Unable to parse settings: {}\n", ex.what()));
            return;
        }

        HRESULT result;
        if (FAILED(result = DirectInput8Create(GetModuleHandle(nullptr),
                                               DIRECTINPUT_VERSION,
                                               IID_IDirectInput8,
                                               (VOID**)&m_directInput,
                                               nullptr))) {
            log::Log(fmt::format("DirectInput8Create failed: {}\n", result));
            return;
        }

        GUID deviceGuid;
        if (FAILED(result = CLSIDFromString(
                       std::wstring(m_settings.InputDeviceGuid.begin(), m_settings.InputDeviceGuid.end()).c_str(),
                       &deviceGuid))) {
            log::Log(fmt::format("CLSIDFromString failed: {}\n", result));
            return;
        }

        if (FAILED(result = m_directInput->CreateDevice(deviceGuid, &m_inputDevice, nullptr))) {
            log::Log(fmt::format("m_directInput->CreateDevice failed: {}\n", result));
            
            log::Log(fmt::format("Attempting to find device named {}\n", m_settings.InputDeviceName));
            if (FAILED(result = m_directInput->EnumDevices(
                           DI8DEVCLASS_ALL, enumDevicesCallback, this, DIEDFL_ATTACHEDONLY))) {
                log::Log(fmt::format("m_directInput->EnumDevices failed: {}\n", result));
                return;
            }

            if (m_inputDevice) {
                log::Log("Found device\n");
            } else {
                log::Log("Failed to find device\n");
                return;
            }
        }

        if (FAILED(result = m_inputDevice->SetCooperativeLevel(nullptr, DISCL_NONEXCLUSIVE | DISCL_BACKGROUND))) {
            log::Log(fmt::format("m_inputDevice->SetCooperativeLevel failed: {}\n", result));
            return;
        }

        if (FAILED(result = m_inputDevice->SetDataFormat(&c_dfDIJoystick2))) {
            log::Log(fmt::format("m_inputDevice->SetDataFormat failed: {}\n", result));
            return;
        }

        if (FAILED(result = m_inputDevice->Acquire())) {
            log::Log(fmt::format("m_inputDevice->Acquire failed: {}\n", result));
            return;
        }

        DIJOYSTATE2 state;
        if (FAILED(result = m_inputDevice->GetDeviceState(sizeof(DIJOYSTATE2), &state))) {
            log::Log(fmt::format("m_inputDevice->GetDeviceState failed: {}\n", result));
            return;
        }

        m_isInitialized = true;
    }
}

void openxr_api_layer::InputTweaks::update(double pitch, double yaw) {
    m_lastPitch = m_currentPitch;
    m_lastYaw = m_currentYaw;
    m_currentPitch = pitch;
    m_currentYaw = yaw;

    if (!m_isInitialized) {
        return;
    }

    HRESULT result;
    DIJOYSTATE2 state;
    if (FAILED(result = m_inputDevice->GetDeviceState(sizeof(DIJOYSTATE2), &state))) {
        log::Log(fmt::format("m_inputDevice->GetDeviceState failed: {}\n", result));
        return;
    }

    m_settings.InputBindings.ToggleMouse.update(state);
    m_settings.InputBindings.MouseLeftClick.update(state);
    m_settings.InputBindings.MouseRightClick.update(state);
    m_settings.InputBindings.MouseScrollUp.update(state);
    m_settings.InputBindings.MouseScrollDown.update(state);
    m_settings.InputBindings.Escape.update(state);

    switch (m_settings.SimulatedMouseTrigger) {
    case SimulatedMouseTrigger::AlwaysDisabled:
        m_simulatedMouseIsActive = false;
        break;

    case SimulatedMouseTrigger::AlwaysEnabled:
        m_simulatedMouseIsActive = isGameOnForeground();
        break;

    case SimulatedMouseTrigger::HoldToEnable:
        m_simulatedMouseIsActive = m_settings.InputBindings.ToggleMouse.isHeld();
        break;

    case SimulatedMouseTrigger::PressToToggle:
        if (m_settings.InputBindings.ToggleMouse.isPressed()) {
            m_simulatedMouseIsActive = !m_simulatedMouseIsActive;
        }
        break;
    }

    std::vector<INPUT> inputs;
    INPUT mouseInput{};
    mouseInput.type = INPUT_MOUSE;
    if (m_simulatedMouseIsActive) {
        if (m_settings.InputBindings.MouseLeftClick.isPressed()) {
            mouseInput.mi.dwFlags |= MOUSEEVENTF_LEFTDOWN;
            m_simulatedMouseLeftButtonIsDown = true;
        }

        if (m_settings.InputBindings.MouseRightClick.isPressed()) {
            mouseInput.mi.dwFlags |= MOUSEEVENTF_RIGHTDOWN;
            m_simulatedMouseRightButtonIsDown = true;
        }

        if (m_settings.InputBindings.MouseScrollUp.isPressed()) {
            mouseInput.mi.dwFlags |= MOUSEEVENTF_WHEEL;
            mouseInput.mi.mouseData = WHEEL_DELTA;
        }

        if (m_settings.InputBindings.MouseScrollDown.isPressed()) {
            mouseInput.mi.dwFlags |= MOUSEEVENTF_WHEEL;
            mouseInput.mi.mouseData = -WHEEL_DELTA;
        }

        if (!std::isnan(m_lastPitch) && !std::isnan(m_currentPitch) && !std::isnan(m_lastYaw) &&
            !std::isnan(m_currentYaw)) {
            double sensitivity = m_settings.SimulatedMouseSensitivity;
            mouseInput.mi.dwFlags |= MOUSEEVENTF_MOVE;
            mouseInput.mi.dx = static_cast<DWORD>(std::lround((m_lastYaw - m_currentYaw) * sensitivity));
            mouseInput.mi.dy = static_cast<DWORD>(std::lround((m_lastPitch - m_currentPitch) * sensitivity));
        }
    }

    if (m_simulatedMouseLeftButtonIsDown && !m_settings.InputBindings.MouseLeftClick.isHeld()) {
        mouseInput.mi.dwFlags |= MOUSEEVENTF_LEFTUP;
        m_simulatedMouseLeftButtonIsDown = false;
    }

    if (m_simulatedMouseRightButtonIsDown && !m_settings.InputBindings.MouseRightClick.isHeld()) {
        mouseInput.mi.dwFlags |= MOUSEEVENTF_RIGHTUP;
        m_simulatedMouseRightButtonIsDown = false;
    }

    if (mouseInput.mi.dx != 0 || mouseInput.mi.dy != 0 || mouseInput.mi.dwFlags != 0) {
        inputs.push_back(mouseInput);
    }

    INPUT keyboardInput{};
    keyboardInput.type = INPUT_KEYBOARD;
    if (m_settings.InputBindings.Escape.isPressed()) {
        keyboardInput.ki.wScan = MapVirtualKey(VK_ESCAPE, MAPVK_VK_TO_VSC);
        keyboardInput.ki.dwFlags |= KEYEVENTF_SCANCODE;
        m_simulatedEscapeKeyIsDown = true;
    } else if (m_simulatedEscapeKeyIsDown && !m_settings.InputBindings.Escape.isHeld()) {
        keyboardInput.ki.wScan = MapVirtualKey(VK_ESCAPE, MAPVK_VK_TO_VSC);
        keyboardInput.ki.dwFlags |= KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP;
        m_simulatedEscapeKeyIsDown = false;
    }

    if (keyboardInput.ki.wScan != 0 || keyboardInput.ki.dwFlags != 0) {
        inputs.push_back(keyboardInput);
    }

    if (inputs.size() > 0) {
        UINT sentInputs = SendInput(static_cast<UINT>(inputs.size()), &inputs[0], sizeof(INPUT));
        if (sentInputs != inputs.size()) {
            log::Log(fmt::format("Failed to send inputs: {}\n", HRESULT_FROM_WIN32(GetLastError())));
        }
    }
}

BOOL openxr_api_layer::InputTweaks::enumDevicesCallback(LPCDIDEVICEINSTANCE lpddi, LPVOID pvRef) {
    InputTweaks* self = static_cast<InputTweaks*>(pvRef);

    std::wstring deviceNameW(lpddi->tszInstanceName);
    std::string deviceName(deviceNameW.begin(), deviceNameW.end());

    if (self->m_settings.InputDeviceName == deviceName) {
        HRESULT result;
        if (FAILED(result = self->m_directInput->CreateDevice(lpddi->guidInstance, &self->m_inputDevice, nullptr))) {
            log::Log(fmt::format("m_directInput->CreateDevice failed: {}\n", result));
            return DIENUM_CONTINUE;
        }

        return DIENUM_STOP;
    }

    return DIENUM_CONTINUE;
}

bool openxr_api_layer::InputTweaks::isGameOnForeground() {
    DWORD currentProcessId = GetCurrentProcessId();
    HWND foregroundWindowHandle = GetForegroundWindow();
    DWORD foregroundWindowProcessId;
    DWORD foregroundWindowThreadId = GetWindowThreadProcessId(foregroundWindowHandle, &foregroundWindowProcessId);

    return currentProcessId && foregroundWindowProcessId && currentProcessId == foregroundWindowProcessId;
}
