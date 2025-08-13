using System.Text;
using System.Windows;

namespace TruckSimVRTweaks
{
    public static class MessageBoxUtil
    {
        public static MessageBoxResult ShowError(string message)
        {
            return MessageBox.Show(message, App.Title, MessageBoxButton.OK, MessageBoxImage.Error);
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

            return MessageBox.Show(messageBuilder.ToString(), App.Title, buttonType, MessageBoxImage.Error);
        }

        public static bool ShowWarning(string message)
        {
            StringBuilder messageBuilder = new(message);
            messageBuilder.AppendLine();
            messageBuilder.AppendLine();
            messageBuilder.Append($"{App.Title} will likely not work as expected. Continue anyway?");

            return MessageBox.Show(messageBuilder.ToString(), App.Title, MessageBoxButton.YesNo)
                == MessageBoxResult.Yes;
        }
    }
}
