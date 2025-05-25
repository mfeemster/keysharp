namespace Keysharp.Core
{
	internal static class EmbeddedDependencyLoader
	{
		internal static Dictionary<string, Assembly> assemblyResources = new(StringComparer.OrdinalIgnoreCase);
		internal static string dllExt = 
				OperatingSystem.IsWindows() ? ".dll"
				: OperatingSystem.IsMacOS() ? ".dylib"
				: ".so";

		static EmbeddedDependencyLoader()
		{
			var asm = Assembly.GetExecutingAssembly();
			foreach (var name in asm.GetManifestResourceNames())
				if (name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".so", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".dylib", StringComparison.OrdinalIgnoreCase))
					assemblyResources[name] = asm;

			asm = Assembly.GetEntryAssembly();
			foreach (var name in asm.GetManifestResourceNames())
				if (name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".so", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".dylib", StringComparison.OrdinalIgnoreCase))
					assemblyResources[name] = asm;
		}

		[ModuleInitializer]
		public static void Initialize()
		{
			if (assemblyResources.Count > 0)
				AppDomain.CurrentDomain.AssemblyResolve += ResolveFromResources;
		}

		private static nint ResolveNativeFromResources(string libName, Assembly asm, DllImportSearchPath? path) 
		{
			Assembly resourceAsm;
			var resourceName = $"{libName}{dllExt}";

			Stream rs = null;
			if (assemblyResources.TryGetValue(resourceName, out resourceAsm))
				rs = resourceAsm.GetManifestResourceStream(resourceName);
			else if (assemblyResources.TryGetValue("Deps." + resourceName, out resourceAsm))
				rs = resourceAsm.GetManifestResourceStream("Deps." + resourceName);

			if (rs == null) return IntPtr.Zero;

			var tmp = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath), resourceName);

			if (File.Exists(tmp)) return NativeLibrary.Load(tmp);

			using var fs = File.Create(tmp);
			rs.CopyTo(fs);
			rs.Close();
			fs.Close();
			return NativeLibrary.Load(tmp);
		}

		private static Assembly ResolveFromResources(object sender, ResolveEventArgs args)
		{
			var name = new AssemblyName(args.Name).Name + ".dll";
			var resourceName = "Deps." + name; // match the <LogicalName> used in the manifest

			Stream stream = null;
			Assembly asm;
			if (assemblyResources.TryGetValue(name, out asm))
				stream = asm.GetManifestResourceStream(name);
			else if (assemblyResources.TryGetValue(resourceName, out asm))
				stream = asm.GetManifestResourceStream(resourceName);
			if (stream == null) return null;

			byte[] data = new byte[stream.Length];
			_ = stream.Read(data, 0, data.Length);
			stream.Close();

			asm = Assembly.Load(data);

			// Native P/Invoke resolver
			NativeLibrary.SetDllImportResolver(asm, ResolveNativeFromResources);

			return asm;
		}
	}
}
