using Keysharp.Internals;

namespace Keysharp.Builtins
{
	public partial class Ks
	{
		public class Input : KeysharpObject
		{
			private readonly InputDeviceInfo device;

			public Input(params object[] Arguments) : base(Arguments) { }
			internal Input(InputDeviceInfo device) : base(null) => this.device = device;

			public override object __New(params object[] Arguments)
				=> Errors.ErrorOccurred("Input instances are produced by Input.Devices.");

			public long Id => device.Id;
			public string Kind => device.Kind;
			public string Name => device.Name;
			public long VendorId => device.VendorId;
			public long ProductId => device.ProductId;
			public string HardwareId => device.HardwareId;

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
