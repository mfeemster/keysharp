namespace Keysharp.Scripting
{
	partial class Script
	{
		public static void Parameters(string[] names, object[] values, object[] defaults)
		{
			for (var i = 0; i < names.Length; i++)
			{
				var init = i < values.Length ? values[i] : i < defaults.Length ? defaults[i] : null;
				Vars[names[i]] = init;
			}
		}

		public static object Parameter(object[] values, object def, int index) => index < values.Length ? values[index] : def;
	}
}