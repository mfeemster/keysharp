namespace System.IO
{
	/// <summary>
	/// Extension methods for various System.IO classes.
	/// </summary>
	public static class SystemIoExtensions
	{
		/// <summary>
		/// Reads a line from a BinaryReader at the current position.
		/// Because this examines one character at a time, it will be slow.
		/// </summary>
		/// <param name="reader">The BinaryReader to read a line from.</param>
		/// <returns>The line read from reader.</returns>
		internal static string ReadLine(this BinaryReader reader)
		{
			var result = new StringBuilder();
			var foundEndOfLine = false;
			char ch;

			while (!foundEndOfLine)
			{
				try
				{
					ch = reader.ReadChar();
				}
				catch (EndOfStreamException)
				{
					if (result.Length == 0) return null;
					else break;
				}

				switch (ch)
				{
					case '\r':
						if (reader.PeekChar() == '\n')
							_ = reader.ReadChar();

						foundEndOfLine = true;
						break;

					case '\n':
						foundEndOfLine = true;
						break;

					default:
						_ = result.Append(ch);
						break;
				}
			}

			return result.ToString();
		}
	}
}