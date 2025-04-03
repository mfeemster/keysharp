namespace Keysharp.Scripting
{
	internal partial class Parser
	{
		public static int TypeDistance(Type t1, Type t2)
		{
			var distance = 0;

			while (t1 != null && t1 != t2)
			{
				distance++;
				t1 = t1.BaseType;
			}

			return distance;
		}
	}
}