using System;

namespace Keysharp.Scripting
{
	public partial class Script
	{
		private static object ForceType(Type requested, object value)
		{
			if (requested == typeof(object) || requested.IsAssignableFrom(value.GetType()))
				return value;

			if (requested == typeof(decimal))
				return ForceDecimal(value);

			if (requested == typeof(double))
				return ForceDouble(value);

			//if (requested == typeof(int))
			//  return ForceInt(value);

			if (requested == typeof(long))
				return ForceLong(value);

			return requested == typeof(string) ? ForceString(value) : value;
		}

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