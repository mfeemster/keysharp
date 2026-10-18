using Keysharp.Internals;

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
				=> Errors.ErrorOccurred("Input instances are produced by Input.Devices and A_EventInfo.Device.");

			public long Id => device.Id;
			public string Kind => device.Kind;
			public string Name => device.Name;
			public long VendorId => device.VendorId;
			public long ProductId => device.ProductId;
			public string HardwareId => device.HardwareId;

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
