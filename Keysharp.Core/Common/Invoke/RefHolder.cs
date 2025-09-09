namespace Keysharp.Core.Common.Invoke
{
	[PublicForTestOnly]
	public class RefHolder
	{
		internal int index;
		internal Action<object> reassign;
		internal object val;

		public RefHolder(int i, object o, Action<object> r)
		{
			index = i;
			val = o ?? "";
			reassign = r;
		}
	}
}