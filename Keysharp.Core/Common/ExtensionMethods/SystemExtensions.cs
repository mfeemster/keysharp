namespace System
{
	public static class SystemExtensions
	{
		internal static double NextDouble(this Random random, double minValue, double maxValue) => random.NextDouble()* (maxValue - minValue) + minValue;
	}
}