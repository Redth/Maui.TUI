#nullable enable
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using XenoAtom.Terminal.UI;
using XenoAtom.Terminal.UI.Controls;
using XenoAtom.Terminal.UI.Layout;

namespace Maui.TUI.Platform;

/// <summary>
/// Handles MAUI alert/dialog/action sheet/prompt requests by showing TUI Dialog modals.
/// Registers as IAlertManagerSubscription via DI so AlertManager.Subscribe() picks it up automatically.
/// </summary>
public static class TuiAlertManager
{
	/// <summary>
	/// Registers the TUI alert subscription in the DI container.
	/// AlertManager.Subscribe() checks DI first: context.Services.GetService&lt;IAlertManagerSubscription&gt;()
	/// </summary>
	public static void RegisterAlertSubscription(IServiceCollection services)
	{
		try
		{
			// Get the internal IAlertManagerSubscription type
			var amType = typeof(Microsoft.Maui.Controls.Window).Assembly
				.GetType("Microsoft.Maui.Controls.Platform.AlertManager");
			var iamsType = amType?.GetNestedType("IAlertManagerSubscription", BindingFlags.Public | BindingFlags.NonPublic);
			if (iamsType is null)
			{
				Console.Error.WriteLine("[TuiAlertManager] IAlertManagerSubscription type not found");
				return;
			}

			// Create a DispatchProxy that implements IAlertManagerSubscription
			var proxyType = typeof(AlertSubscriptionProxy<>).MakeGenericType(iamsType);
			var createMethod = typeof(DispatchProxy)
				.GetMethods(BindingFlags.Public | BindingFlags.Static)
				.First(m => m.Name == "Create" && m.GetGenericArguments().Length == 2)
				.MakeGenericMethod(iamsType, proxyType);
			var proxy = createMethod.Invoke(null, null);

			if (proxy is null)
			{
				Console.Error.WriteLine("[TuiAlertManager] Failed to create proxy");
				return;
			}

			// Register as the internal interface type so AlertManager.Subscribe() finds it via DI
			services.AddSingleton(iamsType, proxy);
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine($"[TuiAlertManager] Failed to register: {ex.Message}");
		}
	}

	internal static void HandleAlert(Page sender, object arguments)
	{
		var argsType = arguments.GetType();
		var title = argsType.GetProperty("Title")?.GetValue(arguments) as string;
		var message = argsType.GetProperty("Message")?.GetValue(arguments) as string;
		var accept = argsType.GetProperty("Accept")?.GetValue(arguments) as string;
		var cancel = argsType.GetProperty("Cancel")?.GetValue(arguments) as string;
		var result = argsType.GetProperty("Result")?.GetValue(arguments);

		var app = GetTerminalApp(sender);
		if (app is null)
		{
			SetResult(result, false);
			return;
		}

		var content = new VStack();
		if (!string.IsNullOrEmpty(message))
			content.Children.Add(new TextBlock(message));

		var buttons = new HStack();

		if (!string.IsNullOrEmpty(accept))
		{
			var acceptBtn = new XenoAtom.Terminal.UI.Controls.Button { Content = new TextBlock(accept) };
			acceptBtn.ClickRouted += (s, e) =>
			{
				SetResult(result, true);
				CloseParentDialog(acceptBtn);
			};
			buttons.Children.Add(acceptBtn);
		}

		if (!string.IsNullOrEmpty(cancel))
		{
			var cancelBtn = new XenoAtom.Terminal.UI.Controls.Button { Content = new TextBlock(cancel) };
			cancelBtn.ClickRouted += (s, e) =>
			{
				SetResult(result, false);
				CloseParentDialog(cancelBtn);
			};
			buttons.Children.Add(cancelBtn);
		}

		content.Children.Add(buttons);

		var dialog = new Dialog
		{
			Title = new TextBlock(title ?? string.Empty),
			Content = content,
			IsModal = true,
		};

		app.Post(() => dialog.Show());
	}

	internal static void HandleActionSheet(Page sender, object arguments)
	{
		var argsType = arguments.GetType();
		var title = argsType.GetProperty("Title")?.GetValue(arguments) as string;
		var buttonsEnum = argsType.GetProperty("Buttons")?.GetValue(arguments) as IEnumerable<string>;
		var destruction = argsType.GetProperty("Destruction")?.GetValue(arguments) as string;
		var cancel = argsType.GetProperty("Cancel")?.GetValue(arguments) as string;
		var result = argsType.GetProperty("Result")?.GetValue(arguments);

		var app = GetTerminalApp(sender);
		if (app is null)
		{
			SetStringResult(result, null);
			return;
		}

		var content = new VStack();

		if (buttonsEnum is not null)
		{
			foreach (var buttonText in buttonsEnum)
			{
				var btn = new XenoAtom.Terminal.UI.Controls.Button { Content = new TextBlock(buttonText) };
				btn.ClickRouted += (s, e) =>
				{
					SetStringResult(result, buttonText);
					CloseParentDialog(btn);
				};
				content.Children.Add(btn);
			}
		}

		if (!string.IsNullOrEmpty(destruction))
		{
			var destructBtn = new XenoAtom.Terminal.UI.Controls.Button { Content = new TextBlock(destruction) };
			destructBtn.ClickRouted += (s, e) =>
			{
				SetStringResult(result, destruction);
				CloseParentDialog(destructBtn);
			};
			content.Children.Add(destructBtn);
		}

		if (!string.IsNullOrEmpty(cancel))
		{
			var cancelBtn = new XenoAtom.Terminal.UI.Controls.Button { Content = new TextBlock(cancel) };
			cancelBtn.ClickRouted += (s, e) =>
			{
				SetStringResult(result, cancel);
				CloseParentDialog(cancelBtn);
			};
			content.Children.Add(cancelBtn);
		}

		var dialog = new Dialog
		{
			Title = new TextBlock(title ?? string.Empty),
			Content = content,
			IsModal = true,
		};

		app.Post(() => dialog.Show());
	}

	internal static void HandlePrompt(Page sender, object arguments)
	{
		var argsType = arguments.GetType();
		var title = argsType.GetProperty("Title")?.GetValue(arguments) as string;
		var message = argsType.GetProperty("Message")?.GetValue(arguments) as string;
		var accept = argsType.GetProperty("Accept")?.GetValue(arguments) as string ?? "OK";
		var cancel = argsType.GetProperty("Cancel")?.GetValue(arguments) as string ?? "Cancel";
		var initialValue = argsType.GetProperty("InitialValue")?.GetValue(arguments) as string ?? "";
		var placeholder = argsType.GetProperty("Placeholder")?.GetValue(arguments) as string ?? "";
		var result = argsType.GetProperty("Result")?.GetValue(arguments);

		var app = GetTerminalApp(sender);
		if (app is null)
		{
			SetStringResult(result, null);
			return;
		}

		var textBox = new TextBox { Text = initialValue, Placeholder = placeholder };

		var content = new VStack();
		if (!string.IsNullOrEmpty(message))
			content.Children.Add(new TextBlock(message));
		content.Children.Add(textBox);

		var buttons = new HStack();
		var acceptBtn = new XenoAtom.Terminal.UI.Controls.Button { Content = new TextBlock(accept) };
		acceptBtn.ClickRouted += (s, e) =>
		{
			SetStringResult(result, textBox.Text);
			CloseParentDialog(acceptBtn);
		};
		buttons.Children.Add(acceptBtn);

		var cancelBtn = new XenoAtom.Terminal.UI.Controls.Button { Content = new TextBlock(cancel) };
		cancelBtn.ClickRouted += (s, e) =>
		{
			SetStringResult(result, null);
			CloseParentDialog(cancelBtn);
		};
		buttons.Children.Add(cancelBtn);
		content.Children.Add(buttons);

		var dialog = new Dialog
		{
			Title = new TextBlock(title ?? string.Empty),
			Content = content,
			IsModal = true,
		};

		app.Post(() => dialog.Show());
	}

	static void SetResult(object? tcs, bool value)
	{
		if (tcs is TaskCompletionSource<bool> boolTcs)
			boolTcs.TrySetResult(value);
	}

	static void SetStringResult(object? tcs, string? value)
	{
		if (tcs is TaskCompletionSource<string?> stringTcs)
			stringTcs.TrySetResult(value);
		else
			tcs?.GetType().GetMethod("TrySetResult")?.Invoke(tcs, [value]);
	}

	static TerminalApp? GetTerminalApp(Page sender)
	{
		var element = sender?.Handler?.PlatformView;
		if (element is Visual visual)
			return visual.App;
		return null;
	}

	static void CloseParentDialog(Visual visual)
	{
		Visual? current = visual;
		while (current is not null)
		{
			if (current is Dialog dialog)
			{
				dialog.Close();
				return;
			}
			current = current.Parent;
		}
	}
}

/// <summary>
/// DispatchProxy implementation that intercepts IAlertManagerSubscription calls.
/// </summary>
public class AlertSubscriptionProxy<T> : DispatchProxy
{
	protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
	{
		if (targetMethod is null) return null;

		switch (targetMethod.Name)
		{
			case "OnAlertRequested" when args?.Length == 2:
				TuiAlertManager.HandleAlert((Page)args[0]!, args[1]!);
				break;
			case "OnActionSheetRequested" when args?.Length == 2:
				TuiAlertManager.HandleActionSheet((Page)args[0]!, args[1]!);
				break;
			case "OnPromptRequested" when args?.Length == 2:
				TuiAlertManager.HandlePrompt((Page)args[0]!, args[1]!);
				break;
			case "OnPageBusy":
				break;
		}

		return null;
	}
}
