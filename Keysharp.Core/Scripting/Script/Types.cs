namespace Keysharp.Scripting
{
	public partial class Script
	{
		private static Type MatchTypes(ref object left, ref object right)
		{
			if (left is bool bl)
				left = bl ? 1L : 0L;
			if (right is bool br)
				right = br ? 1L : 0L;
			if (left is StringBuffer sbl)
				left = sbl.ToString();
			if (right is StringBuffer sbr)
				right = sbr.ToString();

			var lt = left.GetType();
			var rt = right.GetType();
			if (lt == rt)
				return lt;

			if (left is Any)
				return lt;
			else if (right is Any)
				return rt;

			if (ParseNumericArgs(left, right, "value compare", out bool leftIsDouble, out bool rightIsDouble, out double leftd, out long leftl, out double rightd, out long rightl, false))
			{
				if (leftIsDouble && rightIsDouble)
				{
					left = leftd; right = rightd;
					return typeof(double);
				}
				else if (!leftIsDouble && !rightIsDouble)
				{
					left = leftl; right = rightl;
					return typeof(long);
				}
				else if (!leftIsDouble)
				{
					left = leftl.Ad(); right = rightd;
					return typeof(double);
				}
				else if (!rightIsDouble)
				{
					left = leftd; right = rightl.Ad();
					return typeof(double);
				}
			}

			if (left is string || right is string)
			{
				left = ForceString(left);
				right = ForceString(right);
				return typeof(string);
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
			else
			{
				return null;
			}
		}
	}
}