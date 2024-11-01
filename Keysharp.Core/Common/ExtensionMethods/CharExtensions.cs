namespace System
{
	/// <summary>
	/// Extension methods for the char type.
	/// </summary>
	public static class CharExtensions
	{
		/// <summary>
		/// Checks if a character is hex.
		/// </summary>
		/// <param name="c">The character to examine.</param>
		/// <returns>True if hex, else false.</returns>
		internal static bool IsHex(this char c)
		{
			return (c >= '0' && c <= '9') ||
				   (c >= 'a' && c <= 'f') ||
				   (c >= 'A' && c <= 'F');
		}

		/// <summary>
		/// Checks if a character can be used in an identifier.
		/// </summary>
		/// <param name="c">The character to examine.</param>
		/// <returns>True if c is a valid identifier character, else false.</returns>
		internal static bool IsIdentifierChar(this char c) => (uint)c > 0x7F || char.IsAsciiLetterOrDigit(c) || c == '_';

		/// <summary>
		/// Checks if a character is one that an identifier can start with.
		/// </summary>
		/// <param name="c">The character to examine.</param>
		/// <returns>True if c is a character that an identifier can start with.</returns>
		internal static bool IsLeadingIdentifierChar(this char c) => (uint)c > 0x7F || char.IsLetter(c) || c == '_';
	}
}