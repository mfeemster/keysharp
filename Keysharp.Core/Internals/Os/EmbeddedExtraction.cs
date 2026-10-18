namespace Keysharp.Internals.Os
{
	internal static class EmbeddedExtraction
	{
		// Keep manifest access inside the error boundary, including failures in the collection getter.
		internal static string CopyAssets(Func<IEnumerable<(string Source, string Deployed, string Hash)>> assets,
			string destination, string assetDescription, string payloadDescription)
		{
			if (string.IsNullOrWhiteSpace(destination)) return null;
			try
			{
				foreach (var asset in assets())
				{
					if (!MatchesHash(asset.Source, asset.Hash))
						return $"{assetDescription} changed or disappeared while compiling: {asset.Source}";
					var target = SafePath(destination, asset.Deployed);
					if (Path.GetFullPath(asset.Source).Equals(target, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
						continue;
					Directory.CreateDirectory(Path.GetDirectoryName(target));
					File.Copy(asset.Source, target, true);
				}
			}
			catch (Exception error) { return $"Copying {payloadDescription} to {destination} failed: {error.Message}"; }
			return null;
		}

		internal static void ExtractResource(Assembly assembly, string resourceName, string root, string relative,
			string hash, string description)
		{
			ExtractionCache.Use(root);
			var target = SafePath(root, relative);
			if (File.Exists(target) && MatchesHash(target, hash)) return;
			using var source = assembly.GetManifestResourceStream(resourceName)
				?? throw new InvalidDataException($"{description} '{relative}' is missing.");
			_ = Extract(source, root, relative, hash);
		}

		internal static string SafePath(string root, string relative)
		{
			if (string.IsNullOrEmpty(relative) || Path.IsPathRooted(relative) || OperatingSystem.IsWindows() && relative.Contains(':'))
				throw new InvalidDataException("An embedded payload contains an invalid deployment path.");
			var fullRoot = Path.GetFullPath(root);
			var path = Path.GetFullPath(Path.Combine(fullRoot, relative));
			var prefix = Path.TrimEndingDirectorySeparator(fullRoot) + Path.DirectorySeparatorChar;
			if (!path.StartsWith(prefix, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
				throw new InvalidDataException("An embedded payload contains an invalid deployment path.");
			return path;
		}

		internal static string Extract(Stream source, string root, string relative, string expectedHash = null)
		{
			var target = SafePath(root, relative);
			ExtractionCache.Use(root);
			// Serialize repair so another extractor cannot replace a file already returned to a reader.
			var identity = OperatingSystem.IsWindows() ? target.ToUpperInvariant() : target;
			using var mutex = new Mutex(false, @"Global\Keysharp.Extract." + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(identity))));
			try { mutex.WaitOne(); } catch (AbandonedMutexException) { }
			try
			{
				if (File.Exists(target) && (expectedHash == null || MatchesHash(target, expectedHash))) return target;
				Directory.CreateDirectory(Path.GetDirectoryName(target));
				var temporary = target + "." + Environment.ProcessId + "." + Guid.NewGuid().ToString("N") + ".tmp";
				try
				{
					using (var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
					{
						source.CopyTo(output);
						output.Flush(true);
					}
					if (expectedHash != null && !MatchesHash(temporary, expectedHash))
						throw new InvalidDataException($"Embedded payload '{relative}' failed its SHA-256 check.");
					if (File.Exists(target)) File.Replace(temporary, target, null);
					else File.Move(temporary, target);
				}
				finally { try { File.Delete(temporary); } catch { } }
				return target;
			}
			finally { mutex.ReleaseMutex(); }
		}

		internal static bool MatchesHash(string path, string expected)
		{
			try { return HashFile(path).Equals(expected, StringComparison.OrdinalIgnoreCase); }
			catch { return false; }
		}

		internal static string HashFile(string path)
		{
			using var stream = File.OpenRead(path);
			return Convert.ToHexString(SHA256.HashData(stream));
		}

		internal static bool IsSha256(string hash) => hash is { Length: 64 } && hash.All(Uri.IsHexDigit);
	}
}
