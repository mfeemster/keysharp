using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Keysharp.Core;

namespace Keysharp.Scripting
{
	public partial class Parser
	{
		internal static Dictionary<string, MethodInfo> libMethods;//Changed to contain the entire method info instead of just its params.//MATT
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
			//Needed a more detailed search since we've refactored into different classes, so linq is better here.//MATT
			var types = Reflections.loadedAssemblies.Values.Where(asm => asm.FullName.StartsWith("Keysharp.Core,"))
						.SelectMany(t => t.GetTypes())
						.Where(t => t.Namespace == "Keysharp.Core" && t.IsClass && t.IsPublic/* && t.IsAbstract && t.IsSealed*/);
			var methods = types
						  .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
						  .Where(m => !m.IsSpecialName)//Original included all set_ and get_ methods which properties generate. Unsure if we want those. Exclude for now, as well as the applicationexit event handler.//MATT
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

				//if (name == "msgbox")//MATT
				//  Console.WriteLine(name);

				//if (ignore.Contains(name))
				//continue;

				//var param = method.GetParameters();

				if (!libMethods.ContainsKey(name))//Quick hack to get it to work, figure out dupes later. Will likely need a multimap.//MATT
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