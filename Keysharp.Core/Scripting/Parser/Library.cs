using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Keysharp.Core;

namespace Keysharp.Scripting
{
	public partial class Parser
	{
		internal static Dictionary<string, MethodInfo> libMethods;
		internal static Dictionary<string, PropertyInfo> libProperties;

		private static void ScanLibrary()
		{
			if (libMethods == null)
				libMethods = new Dictionary<string, MethodInfo>();
			else
				libMethods.Clear();

			if (libProperties == null)
				libProperties = new Dictionary<string, PropertyInfo>();
			else
				libProperties.Clear();

			var ignore = new List<string>();
			var types = Reflections.loadedAssemblies.Values.Where(asm => asm.FullName.StartsWith("Keysharp.Core,"))
						.SelectMany(t => t.GetTypes())
						.Where(t => t.Namespace == "Keysharp.Core" && t.IsClass && t.IsPublic/* && t.IsAbstract && t.IsSealed*/);
			var methods = types
						  .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
						  .Where(m => !m.IsSpecialName)
						  ;
			var props = types
						.SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Static))
						.Where(p => p.Name.StartsWith("A_"))
						;

			foreach (var method in methods)
			{
				//if (!method.IsPublic || !method.IsStatic)
				//continue;
				var name = method.Name.ToLowerInvariant();

				//if (ignore.Contains(name))
				//continue;

				//var param = method.GetParameters();

				if (!libMethods.ContainsKey(name))//If we ever want to support duplicates, we'll likely need a multimap.
					//{
					//  _ = libMethods.Remove(name);
					//  ignore.Add(name);
					//}
					//else
					libMethods.Add(name, method);

				//else
				//Console.WriteLine($"dupe: {name}");
			}

			var list = libMethods.Keys.ToList();
			list.Sort();
			//File.WriteAllText("methodskeysharp.txt", string.Join("\n", list.ToArray()));

			foreach (var property in props)
				libProperties.Add(property.Name.ToLowerInvariant(), property);
		}
	}
}