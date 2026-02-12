using Microsoft.Maui.Controls;

namespace Maui.TUI.Sample;

class App : Application
{
	protected override Window CreateWindow(IActivationState? activationState)
	{
		var tabbedPage = new TabbedPage
		{
			Title = "MAUI TUI Sample",
			Children =
			{
				new ControlsPage(),
				new CollectionViewPage(),
				new NavigationPage(new NavigationDemoPage()) { Title = "Navigation" },
				new ModalDemoPage(),
				new FormDemoPage(),
			}
		};

		return new Window(tabbedPage);
	}
}
