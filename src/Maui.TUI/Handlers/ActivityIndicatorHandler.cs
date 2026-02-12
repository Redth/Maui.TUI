#nullable enable
using XenoAtom.Terminal.UI.Controls;

namespace Maui.TUI.Handlers;

public partial class ActivityIndicatorHandler : TuiViewHandler<IActivityIndicator, Spinner>
{
	public static IPropertyMapper<IActivityIndicator, ActivityIndicatorHandler> Mapper =
		new PropertyMapper<IActivityIndicator, ActivityIndicatorHandler>(ViewMapper)
		{
			[nameof(IActivityIndicator.IsRunning)] = MapIsRunning,
		};

	public static CommandMapper<IActivityIndicator, ActivityIndicatorHandler> CommandMapper = new(ViewCommandMapper);

	public ActivityIndicatorHandler() : base(Mapper, CommandMapper) { }
	public ActivityIndicatorHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
		: base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

	protected override Spinner CreatePlatformView() => new Spinner();

	public static void MapIsRunning(ActivityIndicatorHandler handler, IActivityIndicator indicator) =>
		handler.PlatformView.IsActive = indicator.IsRunning;
}
