namespace Keysharp.Core.Common.Strings
{
	public class StringBuffer : KeysharpObject
	{
		internal StringBuilder sb;

		public StringBuffer(params object[] obj) => __New(obj);

		public static implicit operator string(StringBuffer s) => s.sb.ToString();

		public object __New(params object[] obj)
		{
			var str = obj.Length > 0 ? obj[0].ToString() : "";
			var capacity = obj.Length > 1 ? obj[1].Ai() : 256;
			sb = new StringBuilder(str, capacity);
			return "";
		}

		public override void PrintProps(string name, StringBuffer sbuf, ref int tabLevel)
		{
			var indent = new string('\t', tabLevel);
			var fieldType = GetType().Name;
			var str = sb.ToString();

			if (name.Length == 0)
				_ = sbuf.sb.AppendLine($"{indent}{str} ({fieldType})");
			else
				_ = sbuf.sb.AppendLine($"{indent}{name}: {str} ({fieldType})");
		}

		public override string ToString() => (string)this;
	}
}