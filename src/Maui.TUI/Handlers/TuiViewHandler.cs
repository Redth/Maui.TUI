#nullable enable
using Microsoft.Maui.Handlers;
using XenoAtom.Terminal.UI;
using XenoAtom.Terminal.UI.Geometry;

namespace Maui.TUI.Handlers;

/// <summary>
/// Base ViewHandler for TUI platform views backed by XenoAtom.Terminal.UI Visuals.
/// Overrides PlatformArrange and GetDesiredSize to work with the cell-based TUI layout.
/// </summary>
public abstract class TuiViewHandler<TVirtualView, TPlatformView> : ViewHandler<TVirtualView, TPlatformView>
	where TVirtualView : class, IView
	where TPlatformView : class
{
	protected TuiViewHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
		: base(mapper, commandMapper)
	{
	}

	public override void PlatformArrange(Microsoft.Maui.Graphics.Rect rect)
	{
		if (PlatformView is Visual visual)
		{
			if (rect.Width < 0 || rect.Height < 0)
				return;

			visual.Arrange(new Rectangle(
				(int)rect.X, (int)rect.Y,
				(int)Math.Ceiling(rect.Width), (int)Math.Ceiling(rect.Height)));
		}
	}

	public override Microsoft.Maui.Graphics.Size GetDesiredSize(double widthConstraint, double heightConstraint)
	{
		if (PlatformView is Visual visual)
		{
			if (widthConstraint < 0 || heightConstraint < 0)
				return Microsoft.Maui.Graphics.Size.Zero;

			var w = double.IsInfinity(widthConstraint) ? int.MaxValue : (int)Math.Ceiling(widthConstraint);
			var h = double.IsInfinity(heightConstraint) ? int.MaxValue : (int)Math.Ceiling(heightConstraint);
			var constraints = new XenoAtom.Terminal.UI.Layout.LayoutConstraints(0, w, 0, h);
			var hints = visual.Measure(in constraints);
			return new Microsoft.Maui.Graphics.Size(hints.Natural.Width, hints.Natural.Height);
		}

		return Microsoft.Maui.Graphics.Size.Zero;
	}
}
