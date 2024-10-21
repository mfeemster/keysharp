namespace System
{
	public static class CharExtensions
	{
		internal static bool IsHex(this char c)
		{
			return (c >= '0' && c <= '9') ||
				   (c >= 'a' && c <= 'f') ||
				   (c >= 'A' && c <= 'F');
		}

		internal static bool IsIdentifierChar(this char c) => (uint)c > 0x7F || char.IsAsciiLetterOrDigit(c) || c == '_';

		internal static bool IsLeadingIdentifierChar(this char c) => (uint)c > 0x7F || char.IsLetter(c) || c == '_';
	}
}