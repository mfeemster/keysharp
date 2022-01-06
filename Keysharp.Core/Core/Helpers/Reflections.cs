using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Keysharp.Core
{
	public static class Reflections
	{
		private static readonly Dictionary<Type, Dictionary<string, MethodInfo>> Methods = new Dictionary<Type, Dictionary<string, MethodInfo>>();
		private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> Properties = new Dictionary<Type, Dictionary<string, PropertyInfo>>();
		private static List<Type> AssTypes = new List<Type>();
		private static Dictionary<Guid, Dictionary<string, MethodInfo>> ExtensionMethods = new Dictionary<Guid, Dictionary<string, MethodInfo>>();

		static Reflections()
		{
			foreach (var item in AppDomain.CurrentDomain.GetAssemblies())//This is probably no longer needed, and throws an exception on System.ServiceModel.dll anyway.
			{
				try
				{
					AssTypes.AddRange(item.GetTypes());
				}
				catch// (Exception e)
				{
					//Keysharp.Core.Dialogs.MsgBox(e.Message);
				}
			}

			foreach (var t in new Type[]
		{
			typeof(object),
				typeof(object[]),
				typeof(ArrayList),
				typeof(IDictionary),
				typeof(Dictionary<object, object>),
			})
			{
				var meths = t.GetExtensionMethods();
				var sel = meths.Select((meth) => new KeyValuePair<string, MethodInfo>(meth.Name, meth)).ToList();
				ExtensionMethods.Add(t.GUID, new Dictionary<string, MethodInfo>(sel, StringComparer.OrdinalIgnoreCase));
				//t.GetExtensionMethods().Select((meth) => new KeyValuePair<string, MethodInfo>(meth.Name, meth))));
				Properties.Add(t, new Dictionary<string, PropertyInfo>(
								   t.GetProperties().Select((prop) => new KeyValuePair<string, PropertyInfo>(prop.Name, prop)), StringComparer.OrdinalIgnoreCase));
			}
		}

		public static void CacheAllMethods()
		{
			foreach (var item in AppDomain.CurrentDomain.GetAssemblies().Where(assy => assy.FullName.StartsWith("Keysharp.Core,")))
				foreach (var type in item.GetTypes())
					if (type.IsClass && type.IsPublic && type.Namespace.StartsWith("Keysharp.Core"))
						_ = FindMethod(type, "");
		}

		public static void CacheAllProperties()
		{
			foreach (var item in AppDomain.CurrentDomain.GetAssemblies().Where(assy => assy.FullName.StartsWith("Keysharp.Core,")))
				foreach (var type in item.GetTypes())
					if (type.IsClass && type.IsPublic && type.Namespace.StartsWith("Keysharp.Core"))
						_ = FindProperty(type, "");
		}

		internal static MethodInfo FindBuiltInMethod(string name)
		{
			foreach (var item in AppDomain.CurrentDomain.GetAssemblies().Where(assy => assy.FullName.StartsWith("Keysharp.Core,")))
				foreach (var type in item.GetTypes())
					if (type.IsClass && type.IsPublic && type.Namespace.StartsWith("Keysharp.Core"))
						if (FindMethod(type, name) is MethodInfo mi)
							return mi;

			return null;
		}

		internal static MethodInfo FindLocalMethod(string name)
		{
			var stack = new StackTrace(false).GetFrames();

			for (var i = 0; i < stack.Length; i++)
			{
				var type = stack[i].GetMethod().DeclaringType;

				if (type.FullName.StartsWith("Keysharp.Main", StringComparison.OrdinalIgnoreCase))
					return FindMethod(type, name);
			}

			return null;
		}

		internal static string GetVariableInfo()
		{
			var sb = new StringBuilder(2048);
			var stack = new StackTrace(false).GetFrames();

			for (var i = stack.Length - 1; i >= 0; i--)
			{
				if (stack[i] != null &&
						stack[i].GetMethod() != null &&
						stack[i].GetMethod().DeclaringType.Attributes.HasFlag(TypeAttributes.Public))//Public is the script, everything else should be hidden.
				{
					if (stack[i].GetMethod().DeclaringType.Namespace != null &&
							stack[i].GetMethod().DeclaringType.Namespace.StartsWith("Keysharp"))
					{
						var meth = stack[i].GetMethod();
						_ = sb.AppendLine($"Class: {meth.ReflectedType.Name}");

						foreach (var v in meth.ReflectedType.GetFields(BindingFlags.Public | BindingFlags.Static))
						{
							var val = v.GetValue(null);
							var varstr = $"\t{v.Name}: {val?.GetType()}: ";

							if (val is string s)
								varstr += $"[{s.Length}] {s.Substring(0, Math.Min(s.Length, 60))}";
							else if (val is Keysharp.Core.Array arr)
							{
								var ct = Math.Min(100, arr.Count);
								var tempsb = new StringBuilder(ct * 100);

								for (var a = 1; a <= ct; a++)
								{
									var tempstr = arr[a].ToString();
									_ = tempsb.Append(tempstr.Substring(0, Math.Min(tempstr.Length, 60)));

									if (a < ct)
										_ = tempsb.Append(", ");
								}

								varstr += tempsb.ToString();
							}
							else if (val is Keysharp.Core.Map map)
							{
								var ct = Math.Min(100, map.Count);
								var a = 0;
								var tempsb = new StringBuilder(ct * 100);
								_ = tempsb.Append('{');

								foreach (var kv in map.map)
								{
									var tempstr = kv.Value.ToString();
									_ = tempsb.Append($"{kv.Key} : {tempstr.Substring(0, Math.Min(tempstr.Length, 60))}");

									if (++a < ct)
										_ = tempsb.Append(", ");
								}

								_ = tempsb.Append('}');
								varstr += tempsb.ToString();
							}
							else
								varstr += val.ToString();

							_ = sb.AppendLine(varstr);
						}

						_ = sb.AppendLine("");
						_ = sb.AppendLine($"Method: {meth.Name}");
						var mb = stack[i].GetMethod().GetMethodBody();

						foreach (var lvi in mb.LocalVariables)
							_ = sb.AppendLine($"\t{lvi.LocalType}");

						_ = sb.AppendLine("--------------------------------------------------");
					}
				}
			}

			return sb.ToString();
		}

		internal static MethodInfo FindLocalRoutine(string name) => FindLocalMethod(LabelMethodName(name));

		internal static MethodInfo FindMethod(Type t, string name)
		{
			try
			{
				//while (t.Assembly == typeof(Any).Assembly)
				do
				{
					if (Methods.TryGetValue(t, out var dkt))
					{
					}
					else
					{
						foreach (var meth in (MethodInfo[])t.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
							Methods.GetOrAdd(meth.DeclaringType, () => new Dictionary<string, MethodInfo>(StringComparer.OrdinalIgnoreCase)).Add(meth.Name, meth);
					}

					if (dkt == null && !Methods.TryGetValue(t, out dkt))
					{
						t = t.BaseType;
						continue;
					}

					if (dkt.TryGetValue(name, out var mi))//Case sensitive match.
						return mi;

					//foreach (var kv in dkt)//Case insensitive match.
					//  if (kv.Value.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
					//      return kv.Value;
					t = t.BaseType;
				} while (t.Assembly == typeof(Any).Assembly);//Traverse down to the base, but only do it for types that are part of this library. Once a base crosses the library boundary, the loop stops.
			}
			catch (Exception)// e)
			{
				throw;
			}

			return null;
		}

		internal static MethodInfo FindMethod(string name)
		{
			if (FindLocalMethod(name) is MethodInfo mil)
				return mil;

			return FindBuiltInMethod(name);
		}

		internal static MethodInfo FindExtensionMethod(Type t, string meth)
		{
			//if (typeof(IDictionary).IsAssignableFrom(t))
			//  if (ExtensionMethods.TryGetValue(typeof(IDictionary).GUID, out var idkt))
			//      if (idkt.TryGetValue(meth, out var mi))
			//          return mi;
			if (ExtensionMethods.TryGetValue(t.GUID, out var dkt))
				if (dkt.TryGetValue(meth, out var mi))
					return mi;

			return null;
		}
		/*
		    public MethodInfo BestMatch(string name, int length)
		    {
		    MethodInfo result = null;
		    var last = int.MaxValue;

		    foreach (var writer in this)
		    {
		        // find method with same name (case insensitive)
		        if (!name.Equals(writer.Name, StringComparison.OrdinalIgnoreCase))
		            continue;

		        var param = writer.GetParameters().Length;

		        if (param == length) // perfect match when parameter count is the same
		        {
		            return writer;
		        }
		        else if (param > length && param < last) // otherwise find a method with the next highest number of parameters
		        {
		            result = writer;
		            last = param;
		        }
		        else if (result == null) // return the first method with excess parameters as a last resort
		            result = writer;
		    }

		    return result;
		    }
		*/
		internal static PropertyInfo FindProperty(Type t, string name)
		{
			try
			{
				//while (t.Assembly == typeof(Any).Assembly)//Traverse down to the base, but only do it for types that are part of this library. Once a base crosses the library boundary, the loop stops.
				//while (t != typeof(object))//Traverse down to object becase properties of native objects are needed, such as for string.
				do
				{
					if (Properties.TryGetValue(t, out var dkt))
					{
					}
					else//Property on this type has not been used yet, so get all properties and cache.
					{
						foreach (var prop in t.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
							Properties.GetOrAdd(prop.DeclaringType, () => new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase)).Add(prop.Name, prop);
					}

					if (dkt == null && !Properties.TryGetValue(t, out dkt))
					{
						t = t.BaseType;
						continue;
					}

					if (dkt.TryGetValue(name, out var pi))
						return pi;

					//foreach (var kv in dkt)//Case insensitive match.
					//  if (kv.Value.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
					//      return kv.Value;
					t = t.BaseType;
				} while (t != typeof(object));
			}
			catch (Exception)// e)
			{
				throw;
			}

			return null;
		}

		internal static string LabelMethodName(string raw)
		{
			foreach (var sym in raw)
			{
				if (!char.IsLetterOrDigit(sym))
					return string.Concat("label_", raw.GetHashCode().ToString("X"));
			}

			return raw;
		}

		internal static T SafeGetProperty<T>(object item, string name) => (T)item.GetType().GetProperty(name, typeof(T))?.GetValue(item);

		internal static object SafeInvoke(string name, params object[] args)
		{
			var method = FindLocalRoutine(name);

			if (method == null)
				return null;

			try
			{
				return method.Invoke(null, new object[] { args });
			}
			catch { }

			return null;
		}

		internal static void SafeSetProperty(object item, string name, object value) => item.GetType().GetProperty(name, value.GetType())?.SetValue(item, value, null);

		/// <summary>
		/// This Methode extends the System.Type-type to get all extended methods. It searches hereby in all assemblies which are known by the current AppDomain.
		/// </summary>
		/// <remarks>
		/// Insired by Jon Skeet from his answer on http://stackoverflow.com/questions/299515/c-sharp-reflection-to-identify-extension-methods
		/// </remarks>
		/// <returns>returns MethodInfo[] with the extended Method</returns>
		private static List<MethodInfo> GetExtensionMethods(this Type t)
		{
			var query = from type in AssTypes
						where type.IsSealed && /*!type.IsGenericType &&*/ !type.IsNested
						from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
						where method.IsDefined(typeof(ExtensionAttribute), false)
						where method.GetParameters().Length > 0 && method.GetParameters()[0].ParameterType.Name == t.Name
						select method;
			return query.Select(m => m.IsGenericMethod ? m.MakeGenericMethod(t.GenericTypeArguments) : m).ToList();
		}
	}
}