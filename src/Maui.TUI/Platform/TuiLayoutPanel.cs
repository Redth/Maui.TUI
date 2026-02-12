using XenoAtom.Terminal.UI;
using XenoAtom.Terminal.UI.Controls;
using XenoAtom.Terminal.UI.Layout;
using XenoAtom.Terminal.UI.Geometry;

namespace Maui.TUI.Platform;

/// <summary>
/// TUI visual that delegates measure/arrange to MAUI's cross-platform layout system.
/// Used as the platform view for Layout handlers (VerticalStackLayout, HorizontalStackLayout, etc.).
/// </summary>
public class TuiLayoutPanel : Panel
{
	internal Func<double, double, Microsoft.Maui.Graphics.Size>? CrossPlatformMeasure { get; set; }
	internal Func<Microsoft.Maui.Graphics.Rect, Microsoft.Maui.Graphics.Size>? CrossPlatformArrange { get; set; }

	protected override SizeHints MeasureCore(in LayoutConstraints constraints)
	{
		if (CrossPlatformMeasure is null)
			return base.MeasureCore(in constraints);

		var width = constraints.IsWidthBounded ? constraints.MaxWidth : 1000;
		var height = constraints.IsHeightBounded ? constraints.MaxHeight : 1000;
		var measure = CrossPlatformMeasure(width, height);

		var size = new XenoAtom.Terminal.UI.Geometry.Size((int)Math.Ceiling(measure.Width), (int)Math.Ceiling(measure.Height));
		return SizeHints.Fixed(size);
	}

	protected override void ArrangeCore(in Rectangle rect)
	{
		if (CrossPlatformArrange is not null)
		{
			// MAUI's CrossPlatformArrange calls PlatformArrange on each child handler,
			// which positions the TUI visuals at their correct offsets.
			CrossPlatformArrange(new Microsoft.Maui.Graphics.Rect(rect.X, rect.Y, rect.Width, rect.Height));
		}
	}
}
