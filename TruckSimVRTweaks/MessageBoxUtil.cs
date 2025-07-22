using System.Windows;

namespace TruckSimVRTweaks
{
    public static class MessageBoxUtil
    {
        public static MessageBoxResult ShowError(string message, Exception? exception = null)
        {
            if (exception != null)
            {
                return MessageBox.Show($"{message}{Environment.NewLine}{Environment.NewLine}{exception}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                return MessageBox.Show(message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
