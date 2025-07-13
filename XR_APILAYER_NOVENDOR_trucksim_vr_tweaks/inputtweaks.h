#pragma once

#include "settings.h"
#include <nlohmann/json.hpp>
#include <dinput.h>

namespace openxr_api_layer {
    class InputTweaks {
      public:
        void initialize();
        void update(double pitch, double yaw);

      private:
        static BOOL enumDevicesCallback(LPCDIDEVICEINSTANCE lpddi, LPVOID pvRef);
        static bool isGameOnForeground();

        bool m_isInitialized{false};
        Settings m_settings{};
        
        LPDIRECTINPUT8 m_directInput{nullptr};
        LPDIRECTINPUTDEVICE8 m_inputDevice{nullptr};

        bool m_simulatedMouseIsActive{false};
        bool m_simulatedMouseLeftButtonIsDown{false};
        bool m_simulatedMouseRightButtonIsDown{false};

        bool m_simulatedEscapeKeyIsDown{false};
        
        double m_currentPitch{std::numeric_limits<double>::quiet_NaN()};
        double m_currentYaw{std::numeric_limits<double>::quiet_NaN()};
        double m_lastPitch{std::numeric_limits<double>::quiet_NaN()};
        double m_lastYaw{std::numeric_limits<double>::quiet_NaN()};
    };
} // namespace openxr_api_layer