namespace Keysharp.Core.Common.Strings
{
	public class StringBuffer : KeysharpObject
	{
		internal StringBuilder sb;

		public new (Type, object) super => (typeof(KeysharpObject), this);

		public StringBuffer(params object[] args) => _ = __New(args);

		public static implicit operator string(StringBuffer s) => s.sb.ToString();

		public new object __New(params object[] args)
		{
			var str = args.Length > 0 ? args[0].ToString() : "";
			var capacity = args.Length > 1 ? args[1].Ai() : 256;
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