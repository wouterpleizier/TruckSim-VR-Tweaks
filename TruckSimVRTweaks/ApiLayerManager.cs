using Microsoft.Win32;
using System.IO;

namespace TruckSimVRTweaks
{
    public static class ApiLayerManager
    {
        private const string LayerName = "XR_APILAYER_NOVENDOR_trucksim_vr_tweaks";
        private const string RegistrySubKey = "Software\\Khronos\\OpenXR\\1\\ApiLayers\\Implicit";

        private readonly static string _dllPath = Path.Combine(AppContext.BaseDirectory, Path.ChangeExtension(LayerName, "dll"));
        private readonly static string _jsonPath = Path.Combine(AppContext.BaseDirectory, Path.ChangeExtension(LayerName, "json"));

        public static void EnableApiLayer()
        {
            if (!File.Exists(_dllPath)) throw new FileNotFoundException(null, _dllPath);
            if (!File.Exists(_jsonPath)) throw new FileNotFoundException(null, _jsonPath);

            SetApiLayerActive(true);
        }

        public static void DisableApiLayer()
        {
            SetApiLayerActive(false);
        }

        private static void SetApiLayerActive(bool enable)
        {
            RegistryKey subKey = Registry.CurrentUser.CreateSubKey(RegistrySubKey);

            if (enable)
            {
                subKey.SetValue(_jsonPath, 0, RegistryValueKind.DWord);
            }
            else
            {
                subKey.DeleteValue(_jsonPath, throwOnMissingValue: false);
            }
        }
    }
}
