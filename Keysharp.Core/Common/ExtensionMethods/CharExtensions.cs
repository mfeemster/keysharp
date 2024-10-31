namespace System
{
	/// <summary>
	/// Extension methods for the char type.
	/// </summary>
	public static class CharExtensions
	{
		/// <summary>
		/// Checks if is hex.
		/// </summary>
		/// <param name="c">The C.</param>
		/// <returns>A bool</returns>
		internal static bool IsHex(this char c)
		{
			return (c >= '0' && c <= '9') ||
				   (c >= 'a' && c <= 'f') ||
				   (c >= 'A' && c <= 'F');
		}

		/// <summary>
		/// Checks if is identifier char.
		/// </summary>
		/// <param name="c">The C.</param>
		/// <returns>A bool</returns>
		internal static bool IsIdentifierChar(this char c) => (uint)c > 0x7F || char.IsAsciiLetterOrDigit(c) || c == '_';

		/// <summary>
		/// Checks if is leading identifier char.
		/// </summary>
		/// <param name="c">The C.</param>
		/// <returns>A bool</returns>
		internal static bool IsLeadingIdentifierChar(this char c) => (uint)c > 0x7F || char.IsLetter(c) || c == '_';
	}
}