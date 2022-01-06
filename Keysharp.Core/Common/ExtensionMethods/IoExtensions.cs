using System.Text;

namespace System.IO
{
	public static class SystemIoExtensions
	{
		public static string ReadLine(this BinaryReader reader)
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
						if (reader.PeekChar() == '\n') reader.ReadChar();

						foundEndOfLine = true;
						break;

					case '\n':
						foundEndOfLine = true;
						break;

					default:
						result.Append(ch);
						break;
				}
			}

			return result.ToString();
		}
	}
}