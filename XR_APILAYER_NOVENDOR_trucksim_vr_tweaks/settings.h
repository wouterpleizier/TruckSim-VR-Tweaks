#pragma once

#include <dinput.h>
#include <nlohmann/json.hpp>

namespace openxr_api_layer {
    enum class MouseSimulationMode { AlwaysDisabled, AlwaysEnabled, HoldToEnable, PressToToggle };
    
    NLOHMANN_JSON_SERIALIZE_ENUM(MouseSimulationMode,
                                 {
                                     {MouseSimulationMode::AlwaysDisabled, "AlwaysDisabled"},
                                     {MouseSimulationMode::AlwaysEnabled, "AlwaysEnabled"},
                                     {MouseSimulationMode::HoldToEnable, "HoldToEnable"},
                                     {MouseSimulationMode::PressToToggle, "PressToToggle"},
                                 });

    enum class InputType { Unset, Button, POV0, POV1, POV2, POV3 };
    
    NLOHMANN_JSON_SERIALIZE_ENUM(InputType,
                                 {
                                     {InputType::Unset, ""},
                                     {InputType::Button, "Button"},
                                     {InputType::POV0, "POV0"},
                                     {InputType::POV1, "POV1"},
                                     {InputType::POV2, "POV2"},
                                     {InputType::POV3, "POV3"},
                                 });

    struct InputBinding {
      public:
        InputType Type{InputType::Unset};
        int Value{-1};

        void update(DIJOYSTATE2& state) {
            m_wasDown = m_isDown;

            if (!isValid()) {
                m_isDown = false;
            } else {
                switch (Type) {
                case InputType::Button:
                    m_isDown = state.rgbButtons[Value];
                    break;

                case InputType::POV0:
                case InputType::POV1:
                case InputType::POV2:
                case InputType::POV3:
                    m_isDown = state.rgdwPOV[static_cast<int>(Type) - static_cast<int>(InputType::POV0)] == Value;
                    break;

                default:
                    m_isDown = false;
                    break;
                }
            }
        }

        bool isValid() const {
            switch (Type) {
            case InputType::Button:
                return Value >= 0 && Value < 128;

            case InputType::POV0:
            case InputType::POV1:
            case InputType::POV2:
            case InputType::POV3:
                return Value >= 0 && Value < 36000;

            default:
                return false;
            }
        }

        bool isHeld() const {
            return m_isDown;
        }

        bool isPressed() const {
            return !m_wasDown && m_isDown;
        }

      private:
        bool m_isDown{false};
        bool m_wasDown{false};
    };

    NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE_WITH_DEFAULT(InputBinding, Type, Value);

    struct InputBindings {
        InputBinding SimulateMouse{};
        InputBinding MouseLeftClick{};
        InputBinding MouseRightClick{};
        InputBinding MouseScrollUp{};
        InputBinding MouseScrollDown{};
        InputBinding Escape{};
    };

    NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE_WITH_DEFAULT(
        InputBindings, SimulateMouse, MouseLeftClick, MouseRightClick, MouseScrollUp, MouseScrollDown, Escape);

    struct Settings {
        MouseSimulationMode MouseSimulationMode{MouseSimulationMode::HoldToEnable};
        double MouseSimulationSensitivity{50.0};
        std::string InputDeviceName{};
        std::string InputDeviceGuid{};
        InputBindings InputBindings{};
    };

    NLOHMANN_DEFINE_TYPE_NON_INTRUSIVE_WITH_DEFAULT(
        Settings, MouseSimulationMode, MouseSimulationSensitivity, InputDeviceName, InputDeviceGuid, InputBindings);
} // namespace openxr_api_layer