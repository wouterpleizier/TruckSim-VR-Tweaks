// MIT License
//
// Copyright(c) 2022-2023 Matthieu Bucchianeri
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this softwareand associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and /or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright noticeand this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#include "pch.h"

namespace {
    constexpr uint32_t k_maxLoggedErrors = 100;
    uint32_t g_globalErrorCount = 0;
} // namespace

namespace openxr_api_layer::log {
    extern std::ofstream logStream;

    // {063aa5bf-9491-5d83-474a-ce1a8dede5a8}
    TRACELOGGING_DEFINE_PROVIDER(g_traceProvider,
                                 "TruckSimVRTweaks",
                                 (0x063aa5bf, 0x9491, 0x5d83, 0x47, 0x4a, 0xce, 0x1a, 0x8d, 0xed, 0xe5, 0xa8));

    TraceLoggingActivity<g_traceProvider> g_traceActivity;

    namespace {

        // Utility logging function.
        void InternalLog(const char* fmt, va_list va) {
            const std::time_t now = std::time(nullptr);

            char buf[1024];
            size_t offset = std::strftime(buf, sizeof(buf), "%Y-%m-%d %H:%M:%S %z: ", std::localtime(&now));
            vsnprintf_s(buf + offset, sizeof(buf) - offset, _TRUNCATE, fmt, va);
            OutputDebugStringA(buf);
            if (logStream.is_open()) {
                logStream << buf;
                logStream.flush();
            }
        }
    } // namespace

    void Log(const char* fmt, ...) {
        va_list va;
        va_start(va, fmt);
        InternalLog(fmt, va);
        va_end(va);
    }

    void ErrorLog(const char* fmt, ...) {
        if (g_globalErrorCount++ < k_maxLoggedErrors) {
            va_list va;
            va_start(va, fmt);
            InternalLog(fmt, va);
            va_end(va);
            if (g_globalErrorCount == k_maxLoggedErrors) {
                Log("Maximum number of errors logged. Going silent.");
            }
        }
    }

    void DebugLog(const char* fmt, ...) {
#ifdef _DEBUG
        va_list va;
        va_start(va, fmt);
        InternalLog(fmt, va);
        va_end(va);
#endif
    }

} // namespace openxr_api_layer::log
