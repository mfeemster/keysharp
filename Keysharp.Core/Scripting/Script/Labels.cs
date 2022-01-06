namespace Keysharp.Scripting
{
	partial class Script
	{
		//public static object LabelCall(string name) => FunctionCall(LabelMethodName(name), new object[] { });

		internal static string LabelMethodName(string raw)
		{
			foreach (var sym in raw)
			{
				if (!char.IsLetterOrDigit(sym))
					return string.Concat("label_", raw.GetHashCode().ToString("X"));
			}

			return raw;
		}
	}
}