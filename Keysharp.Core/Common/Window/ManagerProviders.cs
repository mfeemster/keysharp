namespace Keysharp.Core.Common.Window
{
	/// <summary>
	/// Singleton Facade for easy accessing current Platform's ControlManager
	/// </summary>
	internal class ControlManagerProvider
	{
		public static ControlManagerBase Instance { get; } = ControlProvider.CreateControlManager();

		// Explicit static constructor to tell C# compiler
		// not to mark type as beforefieldinit
		static ControlManagerProvider() { }

		// private constructor
		private ControlManagerProvider()
		{ }
	}

	/// <summary>
	/// Singleton Facade for easy accessing current Platform's WindowManager
	/// </summary>
	internal class WindowManagerProvider
	{
		public static WindowManagerBase Instance { get; } = WindowProvider.CreateWindowManager();

		// Explicit static constructor to tell C# compiler
		// not to mark type as beforefieldinit
		static WindowManagerProvider() { }

		// private constructor
		private WindowManagerProvider()
		{ }
	}
}