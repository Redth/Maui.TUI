using Microsoft.Maui.Controls;

namespace Maui.TUI.Sample;

class FlyoutDemoPage : FlyoutPage
{
	public FlyoutDemoPage()
	{
		Title = "Flyout";

		Flyout = new ContentPage
		{
			Title = "Menu",
			Content = new VerticalStackLayout
			{
				Spacing = 1,
				Children =
				{
					new Label { Text = "=== Menu ===" },
					CreateMenuItem("Home"),
					CreateMenuItem("Settings"),
					CreateMenuItem("About"),
				}
			}
		};

		Detail = CreateDetailPage("Home");
	}

	Button CreateMenuItem(string title)
	{
		var btn = new Button { Text = title };
		btn.Clicked += (s, e) =>
		{
			Detail = CreateDetailPage(title);
		};
		return btn;
	}

	ContentPage CreateDetailPage(string title)
	{
		var dismissButton = new Button { Text = "Dismiss FlyoutPage" };
		dismissButton.Clicked += async (s, e) =>
		{
			await Navigation.PopModalAsync();
		};

		return new ContentPage
		{
			Title = title,
			Content = new VerticalStackLayout
			{
				Spacing = 1,
				Children =
				{
					new Label { Text = $"=== {title} ===" },
					new Label { Text = $"You selected: {title}" },
					dismissButton,
				}
			}
		};
	}
}
