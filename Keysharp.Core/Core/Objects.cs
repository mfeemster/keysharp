namespace Keysharp.Core
{
	/// <summary>
	/// Public interface for Obj*() functions.
	/// </summary>
	public static class Objects
	{
		/// <summary>
		/// Creates a new <see cref="Map"/> object.
		/// </summary>
		/// <param name="obj">The optional data to initialize the <see cref="Map"/> with. This can be:
		///     An existing <see cref="Map"/> object.
		///     An <see cref="Array"/> of key,value pairs.
		///     An existing <see cref="Dictionary{string, object}"/> object.
		///     An object[] of key,value pairs.
		/// </param>
		/// <returns>A new <see cref="Map"/> object.</returns>
		public static Map Object(params object[] obj)
		{
			var dkt = new Map();

			if (obj.Length != 0)
				dkt.Set(obj);

			return dkt;
		}

		/// <summary>
		/// Throws an <see cref="Error"/> exception because this function has no meaning in Keysharp.
		/// </summary>
		/// <param name="obj">Ignored.</param>
		/// <returns>None</returns>
		/// <exception cref="Error">Throws an <see cref="Error"/> exception because this function has no meaning in Keysharp.</exception>
		public static object ObjGetCapacity(object obj) => obj is KeysharpObject kso ? kso.GetCapacity() : throw new Error($"Object of type {obj.GetType()} was not of type KeysharpObject.");

		/// <summary>
		/// Returns whether an object contains an OwnProp by the specified name.
		/// </summary>
		/// <param name="obj">The obj to search for an OwnProp on.</param>
		/// <param name="name">The OwnProp name to search for.</param>
		/// <returns>Returns 1 if an object owns a property by the specified name, otherwise 0.</returns>
		/// <exception cref="Error">Throws an <see cref="Error"/> exception if obj was not of type <see cref="KeysharpObject"/>.</exception>
		public static long ObjHasOwnProp(object obj, object name) => obj is KeysharpObject kso ? kso.HasOwnProp(name) : 0L;

		/// <summary>
		/// Returns the number of properties owned by an object.
		/// </summary>
		/// <param name="obj">The object to get the OwnProps count for.</param>
		/// <returns>The number of properties owned by an object.</returns>
		/// <exception cref="Error">Throws an <see cref="Error"/> exception if obj was not of type <see cref="KeysharpObject"/>.</exception>
		public static long ObjOwnPropCount(object obj) => obj is KeysharpObject kso ? kso.OwnPropCount() : throw new Error($"Object of type {obj.GetType()} was not of type KeysharpObject.");

		/// <summary>
		/// Returns an OwnProps iterator for the given object.
		/// </summary>
		/// <param name="obj">The object whose OwnProps will be retrieved.</param>
		/// <param name="userOnly">Optionally pass true to specify only user props, else false return all. Default: true.</param>
		/// <returns>An <see cref="OwnPropsIterator"/> object for obj.</returns>
		/// <exception cref="Error">Throws an <see cref="Error"/> exception if obj was not of type <see cref="KeysharpObject"/>.</exception>
		public static object ObjOwnProps(object obj, object userOnly = null) => obj is KeysharpObject kso ? kso.OwnProps(userOnly) : throw new Error($"Object of type {obj.GetType()} was not of type KeysharpObject.");

		/// <summary>
		/// Throws an <see cref="Error"/> exception because this function has no meaning in Keysharp.
		/// </summary>
		/// <param name="obj">Ignored.</param>
		/// <exception cref="Error">Throws an <see cref="Error"/> exception because this function has no meaning in Keysharp.</exception>
		public static void ObjSetBase(params object[] obj) => throw new Error(Any.BaseExc);

		/// <summary>
		/// Throws an <see cref="Error"/> exception because this function has no meaning in Keysharp.
		/// </summary>
		/// <param name="obj0">Ignored.</param>
		/// <param name="obj1">Ignored.</param>
		/// <returns>None</returns>
		/// <exception cref="Error">Throws an <see cref="Error"/> exception because this function has no meaning in Keysharp.</exception>
		public static object ObjSetCapacity(object obj0, object obj1) => obj0 is KeysharpObject kso ? kso.SetCapacity(obj1) : throw new Error($"Object of type {obj0.GetType()} was not of type KeysharpObject.");
	}
}