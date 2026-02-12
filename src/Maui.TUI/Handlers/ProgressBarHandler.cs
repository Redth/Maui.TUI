#nullable enable
using TuiProgressBar = XenoAtom.Terminal.UI.Controls.ProgressBar;

namespace Maui.TUI.Handlers;

public partial class ProgressBarHandler : TuiViewHandler<IProgress, TuiProgressBar>
{
	public static IPropertyMapper<IProgress, ProgressBarHandler> Mapper =
		new PropertyMapper<IProgress, ProgressBarHandler>(ViewMapper)
		{
			[nameof(IProgress.Progress)] = MapProgress,
		};

	public static CommandMapper<IProgress, ProgressBarHandler> CommandMapper = new(ViewCommandMapper);

	public ProgressBarHandler() : base(Mapper, CommandMapper) { }
	public ProgressBarHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
		: base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

	protected override TuiProgressBar CreatePlatformView() => new TuiProgressBar();

	public static void MapProgress(ProgressBarHandler handler, IProgress progress) =>
		handler.PlatformView.Value = progress.Progress;
}
