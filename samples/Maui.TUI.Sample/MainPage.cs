using Microsoft.Maui.Controls;

namespace Maui.TUI.Sample;

class ControlsPage : ContentPage
{
	int _count;
	readonly Label _counterLabel;
	readonly Label _statusLabel;
	readonly ProgressBar _progressBar;

	public ControlsPage()
	{
		Title = "Controls";

		_counterLabel = new Label { Text = "Hello, .NET MAUI TUI!" };
		_statusLabel = new Label { Text = "Interact with the controls below" };

		var button = new Button { Text = "Click me" };
		button.Clicked += OnButtonClicked;

		var alertButton = new Button { Text = "Show Alert" };
		alertButton.Clicked += async (s, e) =>
		{
			var result = await DisplayAlertAsync("Hello", "This is a TUI alert!", "OK", "Cancel");
			_statusLabel.Text = $"Alert result: {result}";
		};

		var entry = new Entry { Placeholder = "Type here..." };
		entry.TextChanged += (s, e) => _statusLabel.Text = $"Entry: {e.NewTextValue}";

		var editor = new Editor { Placeholder = "Multi-line editor..." };
		editor.TextChanged += (s, e) => _statusLabel.Text = $"Editor: {e.NewTextValue}";

		var checkBox = new CheckBox { IsChecked = false };
		checkBox.CheckedChanged += (s, e) => _statusLabel.Text = $"CheckBox: {e.Value}";

		var toggleSwitch = new Switch { IsToggled = false };
		toggleSwitch.Toggled += (s, e) => _statusLabel.Text = $"Switch: {e.Value}";

		var slider = new Slider(0, 100, 50);
		slider.ValueChanged += (s, e) =>
		{
			_statusLabel.Text = $"Slider: {e.NewValue:F0}";
			_progressBar!.Progress = e.NewValue / 100.0;
		};

		_progressBar = new ProgressBar { Progress = 0.5 };

		var spinner = new ActivityIndicator { IsRunning = true };

		var picker = new Picker { Title = "Pick a color" };
		picker.Items.Add("Red");
		picker.Items.Add("Green");
		picker.Items.Add("Blue");
		picker.Items.Add("Yellow");
		picker.SelectedIndexChanged += (s, e) =>
		{
			if (picker.SelectedIndex >= 0 && picker.SelectedIndex < picker.Items.Count)
				_statusLabel.Text = $"Picker: {picker.Items[picker.SelectedIndex]}";
		};

		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Spacing = 1,
				Children =
				{
					_counterLabel,
					button,
					alertButton,
					new Label { Text = "Entry:" },
					entry,
					new Label { Text = "Editor:" },
					editor,
					new Label { Text = "CheckBox:" },
					checkBox,
					new Label { Text = "Switch:" },
					toggleSwitch,
					new Label { Text = "Slider (0-100):" },
					slider,
					new Label { Text = "ProgressBar:" },
					_progressBar,
					new Label { Text = "ActivityIndicator:" },
					spinner,
					new Label { Text = "Picker:" },
					picker,
					new Label { Text = "--- Status ---" },
					_statusLabel,
				}
			}
		};
	}

	void OnButtonClicked(object? sender, EventArgs e)
	{
		_count++;
		_counterLabel.Text = $"Button clicked {_count} time(s)";
	}
}
