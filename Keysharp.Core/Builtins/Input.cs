using Keysharp.Internals;
using Keysharp.Internals.Input;

namespace Keysharp.Builtins
{
	public partial class Ks
	{
		public class Input : KeysharpObject
		{
			private readonly InputDeviceInfo device;
			private readonly Script owner;

			public Input(params object[] Arguments) : base(Arguments) { }
			internal Input(InputDeviceInfo device, Script owner = null) : base(null)
			{
				this.device = device;
				this.owner = owner ?? Script.TheScript;
			}

			public override object __New(params object[] Arguments)
				=> Errors.ErrorOccurred("Input instances cannot be constructed directly.");

			public long Id => device.Id;
			public string Kind => device.Kind;
			public string Name => device.Name;
			public long VendorId => device.VendorId;
			public long ProductId => device.ProductId;
			public string HardwareId => device.HardwareId;
			public static object staticget_Hooks(object @this) => Script.TheScript.InputEventManager.Hooks();

			[Static]
			public static object OnDeviceChange(object @this, object Callback)
			{
				if (Functions.CheckedCallback(Callback, 3) is not { } callback)
					return DefaultObject;

				var hook = new DeviceHook(callback);
				_ = hook.Start();
				return hook.InProgress ? hook : Errors.OSErrorOccurredWithMessage("Input device change notifications are unavailable.");
			}

			public sealed class DeviceHook : EventHook
			{
				internal DeviceHook(object callback) : base(callback) { }
				private protected override EventSubscriptionBase NewRun(ScriptEventScheduler owner)
					=> new InputEventRegistration(callback, owner, Script.TheScript.InputEventManager);
			}

			public object GetKeyState(object KeyName)
			{
				if (!KeyName.CoerceString(out var name))
					return DefaultObject;

				if (owner.IsDisposed)
					return Errors.OSErrorOccurredWithMessage("The input device's script has ended.");

				uint? modifiers = null;
				var virtualKey = owner.HookThread.TextToVK(name, ref modifiers, layout: null);

				if (virtualKey == 0 || MouseUtils.IsWheelVK(virtualKey))
					return Errors.ValueErrorOccurred("Expected a keyboard key or mouse button.", KeyName);

#if LINUX
				if (owner.LinuxServices.Input.TryGetDeviceKeyState(device, virtualKey, out var down))
					return down;
#endif
				return Errors.OSErrorOccurredWithMessage("Physical key state is unavailable for this input source or the device is disconnected.");
			}

			public static object staticget_Devices(object @this)
			{
				try
				{
					var devices = Platform.GetInputDevices(Script.TheScript);
					var result = new Array(null);
					_ = result.array.EnsureCapacity(devices.Length);

					foreach (var device in devices)
						result.array.Add(new Input(device));

					return result;
				}
				catch (Exception ex) when (ex is not KeysharpException and not Flow.UserRequestedExitException)
				{
					return Errors.OSErrorOccurredWithMessage(ex.Message);
				}
			}
		}
	}
}
