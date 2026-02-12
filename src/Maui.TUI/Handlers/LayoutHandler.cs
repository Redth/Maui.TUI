#nullable enable
using Maui.TUI.Platform;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using XenoAtom.Terminal.UI;

namespace Maui.TUI.Handlers;

public partial class LayoutHandler : TuiViewHandler<Layout, TuiLayoutPanel>
{
	public static IPropertyMapper<Layout, LayoutHandler> Mapper =
		new PropertyMapper<Layout, LayoutHandler>(ViewMapper)
		{
		};

	public static CommandMapper<Layout, LayoutHandler> CommandMapper =
		new(ViewCommandMapper)
		{
			[nameof(ILayoutHandler.Add)] = MapAdd,
			[nameof(ILayoutHandler.Remove)] = MapRemove,
			[nameof(ILayoutHandler.Clear)] = MapClear,
			[nameof(ILayoutHandler.Insert)] = MapInsert,
			[nameof(ILayoutHandler.Update)] = MapUpdate,
			[nameof(ILayoutHandler.UpdateZIndex)] = MapUpdateZIndex,
		};

	public LayoutHandler() : base(Mapper, CommandMapper)
	{
	}

	public LayoutHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
		: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
	{
	}

	protected override TuiLayoutPanel CreatePlatformView()
	{
		_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} must be set.");
		return new TuiLayoutPanel
		{
			CrossPlatformMeasure = VirtualView.CrossPlatformMeasure,
			CrossPlatformArrange = VirtualView.CrossPlatformArrange,
		};
	}

	public override void SetVirtualView(IView view)
	{
		base.SetVirtualView(view);

		_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set.");
		_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set.");
		_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set.");

		PlatformView.CrossPlatformMeasure = VirtualView.CrossPlatformMeasure;
		PlatformView.CrossPlatformArrange = VirtualView.CrossPlatformArrange;

		PlatformView.Children.Clear();
		foreach (var child in VirtualView)
		{
			var platformChild = child.ToPlatform(MauiContext);
			if (platformChild is Visual visual)
				PlatformView.Children.Add(visual);
		}
	}

	public void Add(IView child)
	{
		_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set.");
		_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set.");
		_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set.");

		var targetIndex = VirtualView.IndexOf(child);
		var platformChild = child.ToPlatform(MauiContext);
		if (platformChild is Visual visual)
			PlatformView.Children.Insert(targetIndex, visual);
	}

	public void Remove(IView child)
	{
		if ((child.Handler?.ContainerView ?? child.Handler?.PlatformView) is Visual visual)
			PlatformView?.Children.Remove(visual);
	}

	public void Clear()
	{
		PlatformView?.Children.Clear();
	}

	public void Insert(int index, IView child)
	{
		_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set.");
		_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set.");
		_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set.");

		var targetIndex = VirtualView.IndexOf(child);
		var platformChild = child.ToPlatform(MauiContext);
		if (platformChild is Visual visual)
			PlatformView.Children.Insert(targetIndex, visual);
	}

	public void Update(int index, IView child)
	{
		_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set.");
		_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set.");

		var platformChild = child.ToPlatform(MauiContext);
		if (platformChild is Visual visual)
			PlatformView.Children[index] = visual;
	}

	public void UpdateZIndex(IView child)
	{
		// Z-index reordering not needed for MVP TUI
	}

	protected override void DisconnectHandler(TuiLayoutPanel platformView)
	{
		platformView.Children.Clear();
		base.DisconnectHandler(platformView);
	}

	public static void MapAdd(LayoutHandler handler, Layout layout, object? arg)
	{
		if (arg is LayoutHandlerUpdate args)
			handler.Add(args.View);
	}

	public static void MapRemove(LayoutHandler handler, Layout layout, object? arg)
	{
		if (arg is LayoutHandlerUpdate args)
			handler.Remove(args.View);
	}

	public static void MapInsert(LayoutHandler handler, Layout layout, object? arg)
	{
		if (arg is LayoutHandlerUpdate args)
			handler.Insert(args.Index, args.View);
	}

	public static void MapClear(LayoutHandler handler, Layout layout, object? arg)
	{
		handler.Clear();
	}

	static void MapUpdate(LayoutHandler handler, Layout layout, object? arg)
	{
		if (arg is LayoutHandlerUpdate args)
			handler.Update(args.Index, args.View);
	}

	static void MapUpdateZIndex(LayoutHandler handler, Layout layout, object? arg)
	{
		if (arg is IView view)
			handler.UpdateZIndex(view);
	}
}
