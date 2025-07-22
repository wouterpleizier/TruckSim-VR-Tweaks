using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace TruckSimVRTweaks
{
    public partial class InputBindingControl : UserControl
    {
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            nameof(Label), typeof(string), typeof(InputBindingControl), new PropertyMetadata());

        public TextBlock? AssignButtonContent
        {
            get { return (TextBlock?)GetValue(AssignButtonContentProperty); }
            set { SetValue(AssignButtonContentProperty, value); }
        }

        public static readonly DependencyProperty AssignButtonContentProperty = DependencyProperty.Register(
            nameof(AssignButtonContent), typeof(TextBlock), typeof(InputBindingControl), new PropertyMetadata());

        public InputBinding InputBinding
        {
            get { return (InputBinding)GetValue(InputBindingProperty); }
            set { SetValue(InputBindingProperty, value); }
        }

        public static readonly DependencyProperty InputBindingProperty = DependencyProperty.Register(
            nameof(InputBinding), typeof(InputBinding), typeof(InputBindingControl), new PropertyMetadata());

        public InputBindingControl()
        {
            InitializeComponent();

            Loaded += HandleLoaded;
        }

        private void HandleLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= HandleLoaded;
            UpdateAssignButtonContent();
        }

        private void UpdateAssignButtonContent(TimeSpan? timeRemaining = null)
        {
            if (InputBinding == null)
            {
                AssignButtonContent = null;
                return;
            }

            AssignButtonContent ??= new TextBlock();
            if (timeRemaining.HasValue)
            {
                double secondsRemaining = Math.Ceiling(timeRemaining.Value.TotalSeconds);
                AssignButtonContent.Text = FormattableString.Invariant($"Waiting for input... ({secondsRemaining:0})");
                AssignButtonContent.FontWeight = FontWeights.Bold;
                AssignButtonContent.Opacity = 1.0;
            }
            else if (InputBinding.Type == InputType.Unset)
            {
                AssignButtonContent.Text = "<unassigned>";
                AssignButtonContent.FontWeight = FontWeights.Regular;
                AssignButtonContent.Opacity = 2.0 / 3.0;
            }
            else
            {
                StringBuilder text = new StringBuilder()
                    .AppendFormat(CultureInfo.InvariantCulture, "{0} {1}", InputBinding.Type, InputBinding.Value);

                if (InputBinding.Type is InputType.POV0 or InputType.POV1 or InputType.POV2 or InputType.POV3)
                {
                    string? direction = InputBinding.Value switch
                    {
                        0 => "Up",
                        9000 => "Right",
                        18000 => "Down",
                        27000 => "Left",
                        _ => null
                    };

                    if (direction != null)
                    {
                        text.AppendFormat(" ({0})", direction);
                    }
                }

                AssignButtonContent.Text = text.ToString();
                AssignButtonContent.FontWeight = FontWeights.Regular;
                AssignButtonContent.Opacity = 1.0;
            }
        }

        private void HandleClearButtonClicked(object sender, RoutedEventArgs e)
        {
            if (InputPoller.Instance.IsPolling(InputBinding))
            {
                InputPoller.Instance.StopPolling(InputBinding);
            }

            InputBinding.Type = InputType.Unset;
            InputBinding.Value = -1;
            UpdateAssignButtonContent();
        }

        private void HandleAssignButtonClicked(object sender, RoutedEventArgs e)
        {
            if (InputPoller.Instance.IsPolling(InputBinding))
            {
                InputPoller.Instance.StopPolling(InputBinding);
                UpdateAssignButtonContent();
            }
            else
            {
                InputPoller.Instance.StartPolling(InputBinding,
                    progressHandler: (timeRemaining) =>
                    {
                        UpdateAssignButtonContent(timeRemaining);
                    },
                    completedHandler: (result) =>
                    {
                        if (result.HasValue)
                        {
                            InputBinding.Type = result.Value.Type;
                            InputBinding.Value = result.Value.Value;
                        }

                        UpdateAssignButtonContent();
                    });
            }
        }
    }
}
