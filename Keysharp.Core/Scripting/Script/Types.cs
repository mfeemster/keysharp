namespace Keysharp.Scripting
{
	/// <summary>
	/// This was needed to implement this note in the documentation for && and ||:
	///     In an expression where all operands resolve to True, the last operand that resolved to True is returned. Otherwise, the first operand that resolves to False is returned.
	///     In an expression where at least one operand resolves to True, the first operand that resolved to True is returned. Otherwise, the last operand that resolves to False is returned.
	/// </summary>
	public class BoolResult// : Keysharp.Core.KeysharpObject
	{
		//Should this class be a struct instead?
		internal bool b;

		internal object o;

		public BoolResult(bool _b, object _o)
		{
			b = _b;
			o = GetNestedObj(_o);
		}

		public static implicit operator bool(BoolResult r) => r.b;

		public static BoolResult operator &(BoolResult obj1, BoolResult obj2) => !obj1.b ? obj1 : obj2;

		public static bool operator false(BoolResult obj) => !obj.b;

		public static bool operator true(BoolResult obj) => obj.b;

		public override string ToString() => o.ToString();

		public static BoolResult operator |(BoolResult obj1, BoolResult obj2) => obj1.b ? obj1 : obj2;

		private object GetNestedObj(object obj) => obj is BoolResult br ? GetNestedObj(br.o) : obj;//Could potentially be very slow.
	}

	public partial class Script
	{

		private static Type MatchTypes(ref object left, ref object right)
		{
			if (left is string || right is string)
			{
				left = ForceString(left);
				right = ForceString(right);
				return typeof(string);
			}
			else if (left is bool || right is bool)//bool takes precedence, because if one is a bool, we assume they want to do a boolean style comparison.
			{
				left = ForceBool(left);
				right = ForceBool(right);
				return typeof(bool);
			}
			else if (left is double || right is double)
			{
				left = ForceDouble(left);
				right = ForceDouble(right);
				return typeof(double);
			}
			else if (left is long || right is long)
			{
				left = ForceLong(left);
				right = ForceLong(right);
				return typeof(long);
			}
			//Anything below here is highly unlikely to occur.
			else if (left is int || right is int)
			{
				left = ForceInt(left);
				right = ForceInt(right);
				return typeof(int);
			}
			else if (left is uint || right is uint)
			{
				left = (uint)ForceLong(left);
				right = (uint)ForceLong(right);
				return typeof(uint);
			}
			else if (left is IntPtr || right is IntPtr)
			{
				left = ForceLong(left);
				right = ForceLong(right);
				return typeof(long);
			}
			else
			{
				return null;
			}
		}
	}
}