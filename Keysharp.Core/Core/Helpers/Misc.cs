namespace Keysharp.Core
{
	public static class Misc
	{
		public static void Collect() => GC.Collect();

		public static RefHolder Mrh(int i, object o, Action<object> r) => new RefHolder(i, o, r);

		internal static string PrintProps(object obj, string name, StringBuffer sbuf, ref int tabLevel)
		{
			var sb = sbuf.sb;
			var indent = new string('\t', tabLevel);
			var fieldType = obj != null ? obj.GetType().Name : "";

			if (obj is KeysharpObject kso)
			{
				kso.PrintProps(name, sbuf, ref tabLevel);
			}
			else if (obj != null)
			{
				if (obj is string vs)
				{
					var str = "\"" + vs + "\"";//Can't use interpolated string here because the AStyle formatter misinterprets it.
					_ = sb.AppendLine($"{indent}{name}: {str} ({fieldType})");
				}
				else
					_ = sb.AppendLine($"{indent}{name}: {obj} ({fieldType})");
			}
			else
				_ = sb.AppendLine($"{indent}{name}: null");

			return sb.ToString();
		}
	}
}