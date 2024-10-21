#if WINDOWS
namespace Keysharp.Core.COM
{
	public class ComMethodPropertyHolder : MethodPropertyHolder
	{
		public string Name { get; private set; }

		public ComMethodPropertyHolder(string name)
			: base(null, null)
		{
			Name = name;
			callFunc = (inst, obj) =>
			{
				var t = inst.GetType();
				var m = t.GetMember(Name);
				return inst.GetType().InvokeMember(Name, BindingFlags.InvokeMethod, null, inst, obj);
			};
		}
	}
}

#endif