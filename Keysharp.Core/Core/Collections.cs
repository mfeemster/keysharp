namespace Keysharp.Core
{
	/// <summary>
	/// Public interface for collection-related functions.
	/// </summary>
	public static class Collections
	{
		/// <summary>
		/// Creates an <see cref="Array"/> object using the passed in object.
		/// </summary>
		/// <param name="obj">Optionally empty, else pass to the <see cref="Array"/> constructor.</param>
		/// <returns>A new <see cref="Array"/> object.</returns>
		public static Array Array(params object[] obj)
		{
			if (obj == null || obj.Length == 0)
			{
				var arr = new Array
				{
					Capacity = 64
				};
				return arr;
			}
			else
				return new Array(obj);
		}

		/// <summary>
		/// Creates a new <see cref="Buffer"/> object.
		/// </summary>
		/// <param name="byteCount">The number of bytes to allocate. Corresponds to <see cref="Buffer.Size"/>.<br/>
		/// If omitted, the <see cref="Buffer"/> is created with a null (zero) Ptr and zero Size.<br/>
		/// This can optionally be a byte[] or an <see cref="Array"/> object.<br/>
		/// </param>
		/// <param name="fillByte">Specify a number between 0 and 255 to set each byte in the buffer to that number.<br/>
		/// This should generally be omitted in cases where the buffer will be written into without first being read,<br/>
		/// as it has a time-cost proportionate to the number of bytes.<br/>
		/// If omitted, the memory of the buffer is not initialized; the value of each byte is arbitrary.
		/// </param>
		/// <returns>A new <see cref="Buffer"/> object.</returns>
		public static Buffer Buffer(object byteCount = null, object fillByte = null) => new (byteCount, fillByte);

		/// <summary>
		/// Creates a new <see cref="Map"/> object.
		/// </summary>
		/// <param name="obj">The optional data to initialize the <see cref="Map"/> with. This can be:<br/>
		///     An existing <see cref="Map"/> object.<br/>
		///     An <see cref="Array"/> of key,value pairs.<br/>
		///     An existing <see cref="Dictionary{string, object}"/> object.<br/>
		///     An object[] of key,value pairs.
		/// </param>
		/// <returns>A new <see cref="Map"/> object.</returns>
		public static Map Map(params object[] obj) => new Map(obj);
	}
}