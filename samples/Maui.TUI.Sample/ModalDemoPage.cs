using Microsoft.Maui.Controls;

namespace Maui.TUI.Sample;

class ModalDemoPage : ContentPage
{
	readonly Label _statusLabel;

	public ModalDemoPage()
	{
		Title = "Modal Demo";

		_statusLabel = new Label { Text = "Press the button to show a modal page" };

		var pushModalButton = new Button { Text = "Push Modal Page" };
		pushModalButton.Clicked += async (s, e) =>
		{
			var modalPage = new ModalContentPage(1);
			await Navigation.PushModalAsync(modalPage);
			_statusLabel.Text = $"Modal returned: {modalPage.ResultText}";
		};

		var showFlyoutButton = new Button { Text = "Show FlyoutPage (Modal)" };
		showFlyoutButton.Clicked += async (s, e) =>
		{
			await Navigation.PushModalAsync(new FlyoutDemoPage());
		};

		Content = new VerticalStackLayout
		{
			Spacing = 1,
			Children =
			{
				new Label { Text = "=== Modal Page Demo ===" },
				_statusLabel,
				pushModalButton,
				showFlyoutButton,
			}
		};
	}
}

class ModalContentPage : ContentPage
{
	public string ResultText { get; private set; } = "dismissed";
	readonly int _depth;

	public ModalContentPage(int depth)
	{
		_depth = depth;
		Title = $"Modal Page {depth}";

		var depthLabel = new Label { Text = $"This is modal page {depth}" };

		var pushAnotherButton = new Button { Text = $"Push Another Modal ({depth + 1})" };
		pushAnotherButton.Clicked += async (s, e) =>
		{
			await Navigation.PushModalAsync(new ModalContentPage(depth + 1));
		};

		var dismissButton = new Button { Text = "Dismiss This Modal" };
		dismissButton.Clicked += async (s, e) =>
		{
			ResultText = $"closed from depth {depth}";
			await Navigation.PopModalAsync();
		};

		var alertButton = new Button { Text = "Show Alert on Modal" };
		alertButton.Clicked += async (s, e) =>
		{
			await DisplayAlertAsync("Modal Alert", $"Hello from modal {depth}!", "OK");
		};

		Content = new VerticalStackLayout
		{
			Spacing = 1,
			Children =
			{
				new Label { Text = $"=== Modal Page {depth} ===" },
				depthLabel,
				pushAnotherButton,
				dismissButton,
				alertButton,
			}
		};
	}
}
