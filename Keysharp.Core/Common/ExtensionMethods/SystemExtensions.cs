namespace System
{
	/// <summary>
	/// Extension methods for various System classes.
	/// </summary>
	public static class SystemExtensions
	{
		/// <summary>
		/// Returns the next double value from a <see cref="Random"/> object, within a specified range.
		/// </summary>
		/// <param name="random">The <see cref="Random"/> object used to create the double.</param>
		/// <param name="minValue">The minimum value the double can be, inclusive.</param>
		/// <param name="maxValue">The maximum value the double can be, exclusive.</param>
		/// <returns>The next double within the range minValue inclusive to maxValue exclusive.</returns>
		internal static double NextDouble(this Random random, double minValue, double maxValue) => random.NextDouble()* (maxValue - minValue) + minValue;
	}
}