#if LINUX
namespace Keysharp.Core.Linux.Devices
{
	public enum EventType
	{
		/// <summary>
		/// Used as markers to separate events. Events may be separated in time or in space, such as with the multitouch protocol.
		/// </summary>
		EV_SYN,

		/// <summary>
		/// Used to describe state changes of keyboards, buttons, or other key-like devices.
		/// </summary>
		EV_KEY,

		/// <summary>
		/// Used to describe relative axis value changes, e.g. moving the mouse 5 units to the left.
		/// </summary>
		EV_REL,

		/// <summary>
		/// Used to describe absolute axis value changes, e.g. describing the coordinates of a touch on a touchscreen.
		/// </summary>
		EV_ABS,

		/// <summary>
		/// Used to describe miscellaneous input data that do not fit into other types.
		/// </summary>
		EV_MSC,

		/// <summary>
		/// Used to describe binary state input switches.
		/// </summary>
		EV_SW,

		/// <summary>
		/// Used to turn LEDs on devices on and off.
		/// </summary>
		EV_LED,

		/// <summary>
		/// Used to output sound to devices.
		/// </summary>
		EV_SND,

		/// <summary>
		/// Used for autorepeating devices.
		/// </summary>
		EV_REP,

		/// <summary>
		/// Used to send force feedback commands to an input device.
		/// </summary>
		EV_FF,

		/// <summary>
		/// A special type for power button and switch input.
		/// </summary>
		EV_PWR,

		/// <summary>
		/// Used to receive force feedback device status.
		/// </summary>
		EV_FF_STATUS,
	}
}
#endif