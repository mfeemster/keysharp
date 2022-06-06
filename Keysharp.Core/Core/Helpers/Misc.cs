namespace Keysharp.Core
{
	public class Misc
	{
		public delegate void SimpleDelegate();

		public delegate void VariadicAction(params object[] o);

		public delegate object VariadicFunction(params object[] args);
	}
}