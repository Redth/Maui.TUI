#nullable enable
using XenoAtom.Terminal.UI.Geometry;
using XenoAtom.Terminal.UI.Layout;
using TuiPanel = XenoAtom.Terminal.UI.Controls.Panel;

namespace Maui.TUI.Platform;

/// <summary>
/// A simple container panel for NavigationPage that displays a single child
/// (the current page) stretched to fill the available space.
/// </summary>
public class TuiNavigationContainer : TuiPanel
{
	protected override SizeHints MeasureCore(in LayoutConstraints constraints)
	{
		if (Children.Count > 0)
			return Children[0].Measure(in constraints);

		return default;
	}

	protected override void ArrangeCore(in Rectangle rect)
	{
		if (Children.Count > 0)
			Children[0].Arrange(rect);
	}
}
