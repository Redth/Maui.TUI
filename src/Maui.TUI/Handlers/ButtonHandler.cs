#nullable enable
using XenoAtom.Terminal.UI.Input;
using TuiButton = XenoAtom.Terminal.UI.Controls.Button;
using TuiTextBlock = XenoAtom.Terminal.UI.Controls.TextBlock;

namespace Maui.TUI.Handlers;

public partial class ButtonHandler : TuiViewHandler<IButton, TuiButton>
{
	public static IPropertyMapper<IButton, ButtonHandler> Mapper =
		new PropertyMapper<IButton, ButtonHandler>(ViewMapper)
		{
			[nameof(IText.Text)] = MapText,
		};

	public static CommandMapper<IButton, ButtonHandler> CommandMapper =
		new(ViewCommandMapper);

	public ButtonHandler() : base(Mapper, CommandMapper)
	{
	}

	public ButtonHandler(IPropertyMapper? mapper)
		: base(mapper ?? Mapper, CommandMapper)
	{
	}

	public ButtonHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
		: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
	{
	}

	protected override TuiButton CreatePlatformView() => new TuiButton();

	protected override void ConnectHandler(TuiButton platformView)
	{
		base.ConnectHandler(platformView);
		platformView.ClickRouted += OnClicked;
	}

	protected override void DisconnectHandler(TuiButton platformView)
	{
		platformView.ClickRouted -= OnClicked;
		base.DisconnectHandler(platformView);
	}

	void OnClicked(object? sender, ClickEventArgs e)
	{
		VirtualView?.Clicked();
	}

	public static void MapText(ButtonHandler handler, IButton button)
	{
		var text = (button as ITextButton)?.Text ?? (button as IText)?.Text ?? string.Empty;
		handler.PlatformView.Content = new TuiTextBlock(text);
	}
}
