namespace Keysharp.Main;

/// <summary>
/// Keys the pipe to the launcher's and runtime's MVIDs and the compiler component's files, keeping
/// clients and daemons on compatible wire formats and references. Keysharp component assemblies contribute
/// their MVIDs; other payload files contribute their names and sizes.
/// </summary>
internal static class KeysharpFingerprint
{
	// Null when the compiler component is missing or could not be read, as while an update replaces it: no daemon is
	// used then, so a launch reports a missing compiler at once rather than after waiting for a daemon.
	internal static string Value { get; } = Compute();

	private static string Compute()
	{
		using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
		Span<byte> buf = stackalloc byte[16];

		// typeof(KeysharpFingerprint) lives in Keysharp.dll/exe; typeof(Script) lives in Keysharp.Core.dll.
		_ = typeof(KeysharpFingerprint).Module.ModuleVersionId.TryWriteBytes(buf);
		hash.AppendData(buf);
		_ = typeof(Script).Module.ModuleVersionId.TryWriteBytes(buf);
		hash.AppendData(buf);

		try
		{
			var directory = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, ScriptingComponentRegistry.RelativeDirectory(ScriptingComponentIds.Compiler)));

			foreach (var file in directory.EnumerateFiles("*", SearchOption.AllDirectories).OrderBy(file => file.FullName, StringComparer.Ordinal))
			{
				var identity = file.Name.StartsWith("Keysharp.", StringComparison.OrdinalIgnoreCase) && file.Extension.Equals(".dll", StringComparison.OrdinalIgnoreCase)
							   ? ReadMvid(file).ToString() : file.Length.ToString();
				hash.AppendData(Encoding.UTF8.GetBytes($"{Path.GetRelativePath(directory.FullName, file.FullName)}|{identity}\n"));
			}
		}
		catch
		{
			return null;
		}

		return Convert.ToHexString(hash.GetHashAndReset().AsSpan(0, 8)); // 16 hex chars is plenty to avoid collisions.
	}

	private static Guid ReadMvid(FileInfo file)
	{
		using var pe = new PEReader(file.OpenRead());
		var metadata = pe.GetMetadataReader();
		return metadata.GetGuid(metadata.GetModuleDefinition().Mvid);
	}
}