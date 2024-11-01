namespace System
{
	/// <summary>
	/// Extension methods for the bool type.
	/// </summary>
	public static class BoolExtensions
	{
		/// <summary>
		/// Returns whether a nullable bool has a value in it and the value is false.
		/// </summary>
		/// <param name="b">The nullable bool to examine.</param>
		/// <returns>True if b has a value and it's false, else false.</returns>
		public static bool IsFalse(this bool? b) => b.HasValue&& !b.Value;

		/// <summary>
		/// Returns whether a nullable bool has a value in it and the value is true.
		/// </summary>
		/// <param name="b">The nullable bool to examine.</param>
		/// <returns>True if b has a value and it's true, else false.</returns>
		public static bool IsTrue(this bool? b) => b.HasValue&& b.Value;
	}
}