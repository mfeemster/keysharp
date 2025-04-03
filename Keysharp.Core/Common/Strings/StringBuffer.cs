using System.Configuration;

namespace Keysharp.Core.Common.Strings
{
	public class StringBuffer : KeysharpObject
	{
		internal StringBuilder sb;

		public new (Type, object) super => (typeof(KeysharpObject), this);

		public StringBuffer(params object[] args) : base(args) { }

        public static implicit operator string(StringBuffer s) => s.sb.ToString();

		public object Capacity
		{
			get => (long)sb.Capacity;
			set => sb.Capacity = value.Ai();
		}

		public object EntangledString { get; set; }

		public object UpdateEntangledStringFromBuffer() => EntangledString != null ? Script.SetPropertyValue(EntangledString, "__Value", sb.ToString()) : null;
		public object UpdateBufferFromEntangledString() {
			if (EntangledString == null)
				return null;
			var str = Script.GetPropertyValue(EntangledString, "__Value") as string;
			str ??= "";
			var requiredCapacity = Math.Max(sb.Capacity, str.Length);
			sb.EnsureCapacity(requiredCapacity);
			sb.Clear();
			sb.Append(str);
			return str;
		}

        public override object __New(params object[] args)
		{
			var str = args.Length > 0 ? args[0].ToString() : "";
			var capacity = args.Length > 1 ? args[1].Ai() : Math.Max(str.Length, 256);
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