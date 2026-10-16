namespace Keysharp.Main;

/// <summary>
/// Keys the daemon to launcher/runtime MVIDs and compiler payload. Component assemblies contribute their
/// MVIDs; other payload files contribute their names and sizes.
/// </summary>
internal static class KeysharpFingerprint
{
	// Null when the compiler component is missing or could not be read, as while an update replaces it: no daemon is
	// used then, so a launch reports a missing compiler at once rather than after waiting for a daemon.
	internal static string Value { get; } = Compute();

	// FNV-1a identifies builds and users without loading the platform's crypto library on every launch.
	internal static ulong Hash(ReadOnlySpan<byte> data, ulong hash = 14695981039346656037)
	{
		foreach (var b in data)
			hash = unchecked((hash ^ b) * 1099511628211);

		return hash;
	}

	private static string Compute()
	{
		Span<byte> buf = stackalloc byte[16];

		// typeof(KeysharpFingerprint) lives in Keysharp.dll/exe; typeof(Script) lives in Keysharp.Core.dll.
		_ = typeof(KeysharpFingerprint).Module.ModuleVersionId.TryWriteBytes(buf);
		var hash = Hash(buf);
		_ = typeof(Script).Module.ModuleVersionId.TryWriteBytes(buf);
		hash = Hash(buf, hash);

		try
		{
			var directory = Path.Combine(AppContext.BaseDirectory, ScriptingComponentRegistry.RelativeDirectory(ScriptingComponentIds.Compiler));
			var files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
			System.Array.Sort(files, StringComparer.Ordinal);

			foreach (var path in files)
			{
				var name = Path.GetFileName(path);
				var identity = name.StartsWith("Keysharp.", StringComparison.OrdinalIgnoreCase) && name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
							   ? ReadMvid(path).ToString() : new FileInfo(path).Length.ToString();
				hash = Hash(Encoding.UTF8.GetBytes($"{Path.GetRelativePath(directory, path)}|{identity}\n"), hash);
			}
		}
		catch
		{
			return null;
		}

		return hash.ToString("X16");
	}

	// Reading just the MVID headers avoids loading and initializing System.Reflection.Metadata
	// on the script launch path, where its first-use cost matters.
	internal static Guid ReadMvid(string path)
	{
		using var reader = new BinaryReader(File.OpenRead(path));
		var stream = reader.BaseStream;

		if (U16(0) != 0x5A4D)
			throw new BadImageFormatException("No DOS signature.", path);

		var pe = (long)U32(0x3C);
		var sectionCount = U16(pe + 6);
		var optionalHeader = pe + 24;
		var sectionTable = optionalHeader + U16(pe + 20);
		var magic = U16(optionalHeader);

		if (U32(pe) != 0x00004550 || magic is not (0x10B or 0x20B))
			throw new BadImageFormatException("No PE signature or supported optional header.", path);

		// Data directory 14 is the CLI header; the directories start later in a PE32+ optional header.
		var cliHeader = Offset(U32(optionalHeader + (magic == 0x20B ? 224 : 208)));
		var root = Offset(U32(cliHeader + 8));

		if (U32(root) != 0x424A5342)
			throw new BadImageFormatException("No CLI metadata signature.", path);

		var versionLength = U32(root + 12);
		var streamCount = U16(root + 18 + versionLength);
		long tables = -1, guids = -1, guidSize = 0;
		Span<byte> name = stackalloc byte[32];
		stream.Position = root + 20 + versionLength;

		for (var i = 0; i < streamCount; i++)
		{
			var offset = root + reader.ReadUInt32();
			var size = reader.ReadUInt32();
			var nameStart = stream.Position;
			var length = 0;

			for (byte b; (b = reader.ReadByte()) != 0;)
			{
				if (length == name.Length)
					throw new BadImageFormatException("A metadata stream name is too long.", path);

				name[length++] = b;
			}

			stream.Position = nameStart + ((length + 4) & ~3);

			if (name[..length].SequenceEqual("#~"u8) || name[..length].SequenceEqual("#-"u8))
				tables = offset;
			else if (name[..length].SequenceEqual("#GUID"u8))
			{
				guids = offset;
				guidSize = size;
			}
		}

		if (tables < 0 || guids < 0)
			throw new BadImageFormatException("No metadata tables or GUID heap.", path);

		// The Module table comes first after the row counts: Generation, then a string heap index, then the MVID's
		// GUID heap index, each index two or four bytes wide as the heap size flags say.
		stream.Position = tables + 6;
		var heapSizes = reader.ReadByte();
		stream.Position = tables + 8;
		var validTables = reader.ReadUInt64();

		if ((validTables & 1) == 0 || U32(tables + 24) != 1)
			throw new BadImageFormatException("No unique Module row.", path);

		var moduleRow = tables + 24 + 4 * System.Numerics.BitOperations.PopCount(validTables)
			+ ((heapSizes & 0x40) != 0 ? 4 : 0);
		var mvidAt = moduleRow + 2 + ((heapSizes & 1) != 0 ? 4 : 2);
		var mvidIndex = (heapSizes & 2) != 0 ? U32(mvidAt) : U16(mvidAt);
		var guidOffset = ((long)mvidIndex - 1) * 16;

		if (mvidIndex == 0 || guidOffset > guidSize - 16)
			throw new BadImageFormatException("The MVID lies outside the GUID heap.", path);

		stream.Position = guids + guidOffset;
		return new Guid(reader.ReadBytes(16));

		uint U32(long at)
		{
			stream.Position = at;
			return reader.ReadUInt32();
		}

		ushort U16(long at)
		{
			stream.Position = at;
			return reader.ReadUInt16();
		}

		long Offset(uint rva)
		{
			for (var i = 0; i < sectionCount; i++)
			{
				var section = sectionTable + i * 40;
				var start = U32(section + 12);

				if (rva >= start && (long)rva - start < U32(section + 16))
					return (long)rva - start + U32(section + 20);
			}

			throw new BadImageFormatException("An RVA lies outside every section.", path);
		}
	}
}
