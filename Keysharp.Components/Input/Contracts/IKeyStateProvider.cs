namespace Keysharp.Components.Input
{
	/// <summary>Supplies keyboard and mouse-button state to GetKeyState and KeyWait.</summary>
	/// <remarks>
	/// Queries may arrive concurrently. Implement System.IDisposable to release resources after the last
	/// admitted query returns after removal or script shutdown. Hook processing, input capture, sending and
	/// device enumeration use native services.
	/// </remarks>
	public interface IKeyStateProvider
	{
		/// <summary>Returns true when the provider supplies State; false requests native state instead.</summary>
		/// <param name="VirtualKey">The virtual-key code, including mouse-button codes.</param>
		/// <param name="Mode">L for logical state, P for physical state, or T for toggle state.</param>
		/// <param name="State">Whether the requested key state is active.</param>
		/// <remarks>An unexpected managed exception reports a script error instead of requesting native fallback.</remarks>
		bool TryGetKeyState(uint VirtualKey, string Mode, out bool State);
	}
}
