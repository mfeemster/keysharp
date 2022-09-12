namespace System
{
	public static class SystemExtensions
	{
		public static double NextDouble(this Random random, double minValue, double maxValue) => random.NextDouble()* (maxValue - minValue) + minValue;
	}
}