namespace System
{
	/// <summary>
	/// Extension methods for various numerical types.
	/// </summary>
	public static class NumericExtensions
	{
		/// <summary>
		/// Checks if is almost equal.
		/// </summary>
		/// <param name="d">The D.</param>
		/// <param name="comparison">The comparison.</param>
		/// <returns>A bool</returns>
		public static bool IsAlmostEqual(this double d, double comparison) => d.IsAlmostEqual(comparison, 0.00001);

		/// <summary>
		/// Checks if is almost equal.
		/// </summary>
		/// <param name="d">The D.</param>
		/// <param name="comparison">The comparison.</param>
		/// <param name="tolerance">The tolerance.</param>
		/// <returns>A bool</returns>
		public static bool IsAlmostEqual(this double d, double comparison, double tolerance) => (d - comparison).IsAlmostZero(tolerance);

		/// <summary>
		/// Checks if is almost zero.
		/// </summary>
		/// <param name="d">The D.</param>
		/// <param name="tolerance">The tolerance.</param>
		/// <returns>A bool</returns>
		public static bool IsAlmostZero(this double d, double tolerance) => d > -tolerance&& d < tolerance;
	}
}