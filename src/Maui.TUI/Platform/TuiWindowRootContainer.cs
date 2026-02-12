using XenoAtom.Terminal.UI;
using XenoAtom.Terminal.UI.Controls;
using XenoAtom.Terminal.UI.Layout;

namespace Maui.TUI.Platform;

/// <summary>
/// Root container visual that fills the terminal viewport and hosts MAUI page content.
/// Supports stacking pages for modal navigation (topmost page is visible).
/// </summary>
public class TuiWindowRootContainer : Panel
{
	Visual? _currentPage;
	readonly List<Visual> _modalPages = new();

	public void SetPage(Visual pageView)
	{
		if (_currentPage != null)
		{
			_currentPage.IsVisible = _modalPages.Count == 0;
			Children.Remove(_currentPage);
		}

		_currentPage = pageView;

		if (_currentPage != null)
		{
			// Hide the root page if there are modal pages on top
			_currentPage.IsVisible = _modalPages.Count == 0;
			Children.Insert(0, _currentPage);
		}
	}

	public void RemovePage(Visual pageView)
	{
		if (_currentPage == pageView)
			_currentPage = null;
		Children.Remove(pageView);
	}

	public void PushModal(Visual modalView)
	{
		// Hide whatever is currently on top
		if (_modalPages.Count > 0)
			_modalPages[^1].IsVisible = false;
		else if (_currentPage != null)
			_currentPage.IsVisible = false;

		_modalPages.Add(modalView);
		Children.Add(modalView);
	}

	public void PopModal()
	{
		if (_modalPages.Count == 0)
			return;

		var top = _modalPages[^1];
		_modalPages.RemoveAt(_modalPages.Count - 1);
		Children.Remove(top);

		// Show the next page down
		if (_modalPages.Count > 0)
			_modalPages[^1].IsVisible = true;
		else if (_currentPage != null)
			_currentPage.IsVisible = true;
	}
}
