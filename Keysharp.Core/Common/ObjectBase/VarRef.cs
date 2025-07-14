namespace Keysharp.Core.Common.ObjectBase
{
	public class VarRef : KeysharpObject
	{
		private readonly Func<object> Get;
		private readonly Action<object> Set;

		public static VarRef Empty = new VarRef(() => null, x => x = null);

		public VarRef(object x) : base(skipLogic: true)
		{
			Get = () => x;
			Set = (value) => x = value;
		}

		public VarRef(Func<object> getter, Action<object> setter) : base(skipLogic: true)
		{
			Get = getter;
			Set = setter;
		}

		public object __Value
		{
			get => Get();
			set => Set(value);
		}
	}
}
