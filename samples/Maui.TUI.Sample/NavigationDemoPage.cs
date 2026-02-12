using Microsoft.Maui.Controls;

namespace Maui.TUI.Sample;

class NavigationDemoPage : ContentPage
{
	readonly Label _statusLabel;
	int _pageDepth;

	public NavigationDemoPage() : this(1) { }

	NavigationDemoPage(int depth)
	{
		_pageDepth = depth;
		Title = "Navigation";

		// Toolbar items on navigation pages
		ToolbarItems.Add(new ToolbarItem("Info", null, () =>
		{
			_statusLabel.Text = $"Page {_pageDepth}, Stack depth: {Navigation?.NavigationStack?.Count ?? 0}";
		}));

		_statusLabel = new Label { Text = $"You are on page {_pageDepth}" };

		var pushButton = new Button { Text = $"Push Page {_pageDepth + 1}" };
		pushButton.Clicked += async (s, e) =>
		{
			await Navigation.PushAsync(new NavigationDemoPage(_pageDepth + 1));
		};

		var popButton = new Button { Text = "Go Back" };
		popButton.Clicked += async (s, e) =>
		{
			if (Navigation.NavigationStack.Count > 1)
				await Navigation.PopAsync();
			else
				_statusLabel.Text = "Already at root — can't go back";
		};

		var alertButton = new Button { Text = "Show Alert" };
		alertButton.Clicked += async (s, e) =>
		{
			var result = await DisplayAlertAsync("Navigation Demo", $"Hello from page {_pageDepth}!", "OK", "Cancel");
			_statusLabel.Text = $"Alert result: {result}";
		};

		Content = new VerticalStackLayout
		{
			Spacing = 1,
			Children =
			{
				new Label { Text = $"=== Navigation Page {_pageDepth} ===" },
				_statusLabel,
				pushButton,
				popButton,
				alertButton,
			}
		};
	}
}
