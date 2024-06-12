namespace Keysharp.Core.Common.Platform
{
	/// <summary>
	/// Singleton Facade for easy accessing current platform's ControlManager.
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
	/// Singleton Facade for easy accessing current platform's WindowManager.
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

	/// <summary>
	/// Singleton Facade for easy accessing current platform's PlatformManager.
	/// </summary>
	internal class PlatformManagerProvider
	{
		public static PlatformManagerBase Instance { get; } = PlatformProvider.CreatePlatformManager();

		// Explicit static constructor to tell C# compiler
		// not to mark type as beforefieldinit
		static PlatformManagerProvider() { }

		// private constructor
		private PlatformManagerProvider()
		{ }
	}
}