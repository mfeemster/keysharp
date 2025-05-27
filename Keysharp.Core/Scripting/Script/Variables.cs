namespace Keysharp.Scripting
{
	public class Variables
	{
		internal List<(string, bool)> preloadedDlls = [];
		internal DateTime startTime = DateTime.UtcNow;
		private readonly Dictionary<string, MemberInfo> globalVars = new (StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Will be a generated call within Main which calls into this class to add DLLs.
		/// </summary>
		/// <param name="p"></param>
		/// <param name="s"></param>
		public void AddPreLoadedDll(string p, bool s) => preloadedDlls.Add((p, s));

		public Variables()
		{
			var stack = new StackTrace(false).GetFrames();

			for (var i = stack.Length - 1; i >= 0; i--)
			{
				var type = stack[i].GetMethod().DeclaringType;

				if (type != null && type.FullName.StartsWith("Keysharp.CompiledMain", StringComparison.OrdinalIgnoreCase))
				{
					var fields = type.GetFields(BindingFlags.Static |
												BindingFlags.NonPublic |
												BindingFlags.Public);
					var props = type.GetProperties(BindingFlags.Static |
												   BindingFlags.NonPublic |
												   BindingFlags.Public);
					_ = globalVars.EnsureCapacity(fields.Length + props.Length);

					foreach (var field in fields)
						globalVars[field.Name] = field;

					foreach (var prop in props)
						globalVars[prop.Name] = prop;

					break;
				}
			}
		}

		public object GetVariable(string key)
		{
			if (globalVars.TryGetValue(key, out var field))
			{
				if (field is PropertyInfo pi)
					return pi.GetValue(null);
				else if (field is FieldInfo fi)
					return fi.GetValue(null);
			}

			return GetReservedVariable(key);//Last, try reserved variable.
		}

		public object SetVariable(string key, object value)
		{
			if (globalVars.TryGetValue(key, out var field))
			{
				if (field is PropertyInfo pi)
					pi.SetValue(null, value);
				else if (field is FieldInfo fi)
					fi.SetValue(null, value);
			}
			else
				_ = SetReservedVariable(key, value);

			return value;
		}

		private PropertyInfo FindReservedVariable(string name)
		{
			_ = Script.TheScript.ReflectionsData.flatPublicStaticProperties.TryGetValue(name, out var prop);
			return prop;
		}

		private object GetReservedVariable(string name)
		{
			var prop = FindReservedVariable(name);
			return prop == null || !prop.CanRead ? null : prop.GetValue(null);
		}

		private bool SetReservedVariable(string name, object value)
		{
			var prop = FindReservedVariable(name);
			var set = prop != null && prop.CanWrite;

			if (set)
			{
				value = Script.ForceType(prop.PropertyType, value);
				prop.SetValue(null, value);
			}

			return set;
		}

		public object this[object key]
		{
			get => GetVariable(key.ToString()) ?? "";
			set => _ = SetVariable(key.ToString(), value);
		}
	}
}