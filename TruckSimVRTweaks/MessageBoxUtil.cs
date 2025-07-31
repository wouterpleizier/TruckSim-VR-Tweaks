using System.Text;
using System.Windows;

namespace TruckSimVRTweaks
{
    public static class MessageBoxUtil
    {
        public const string Caption = nameof(TruckSimVRTweaks);

        public static MessageBoxResult ShowError(string message)
        {
            return MessageBox.Show(message, Caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static MessageBoxResult ShowError(string message, Exception exception,
            string? additionalMessage = null, MessageBoxButton buttonType = MessageBoxButton.OK)
        {
            StringBuilder messageBuilder = new(message);
            messageBuilder.Append(": ");
            messageBuilder.Append(exception.Message);

            if (additionalMessage != null)
            {
                messageBuilder.AppendLine();
                messageBuilder.AppendLine();
                messageBuilder.Append(additionalMessage);
            }

            return MessageBox.Show(messageBuilder.ToString(), Caption, buttonType, MessageBoxImage.Error);
        }
    }
}
