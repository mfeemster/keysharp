namespace Keysharp.Core
{
	/// <summary>
	/// Public interface for Ini-related functions.
	/// Although the Windows API does provide functions for manipulating INI files, they are not cross platform.
	/// So the code here provides cross platform INI manipulation functions in pure C#.
	/// </summary>
	public static class Ini
	{
		/// <summary>
		/// Deletes a value from a standard format .ini file.
		/// </summary>
		/// <param name="filename">The name of the .ini file, which is assumed to be in A_WorkingDir if an absolute path isn't specified.</param>
		/// <param name="section">The section name in the .ini file, which is the heading phrase that appears in square brackets (do not include the brackets in this parameter).</param>
		/// <param name="key">If omitted, the entire section will be deleted. Otherwise, specify the key name in the .ini file.</param>
		/// <exception cref="Error">An Error exception is thrown if any file errors occur.</exception>
		public static void IniDelete(object filename, object section, object key = null)
		{
			var file = filename.As();
			var s = section.As();
			var k = key.As();

			if (!File.Exists(file))
				return;

			if (s != "")
				s = string.Format(Keyword_IniSectionOpen + "{0}]", s);

			try
			{
				var haskey = !string.IsNullOrEmpty(k);
				var hassec = !string.IsNullOrEmpty(s);
				var sb = new StringBuilder(1024);
				var writer = new StringWriter();
				var inidkt = IniLoad(file);

				if (hassec && haskey)
				{
					if (inidkt.Contains(s))
					{
						var secdkt = inidkt[s] as OrderedDictionary;
						secdkt.Remove(k);
					}
				}
				else if (hassec)
					inidkt.Remove(s);

				foreach (DictionaryEntry kv in inidkt)
				{
					writer.WriteLine(kv.Key);

					foreach (DictionaryEntry kv2 in (OrderedDictionary)kv.Value)
						if (((string)kv2.Key)[0] != '#')
							writer.WriteLine($"{kv2.Key}={kv2.Value}");
						else
							writer.WriteLine($"{kv2.Key}");

					writer.WriteLine();
				}

				writer.Flush();
				var text = writer.ToString();

				if (File.Exists(file))
					File.Delete(file);

				Files.FileAppend("", file, "unicode");
				File.WriteAllText(file, text);
			}
			catch (Exception ex)
			{
				throw new Error(ex.Message);
			}
		}

		/// <summary>
		/// Reads a value from a standard format .ini file.
		/// </summary>
		/// <param name="filename">The name of the .ini file, which is assumed to be in A_WorkingDir if an absolute path isn't specified.</param>
		/// <param name="section">The section name in the .ini file, which is the heading phrase that appears in square brackets (do not include the brackets in this parameter).</param>
		/// <param name="key">The key name in the .ini file.</param>
		/// <param name="default">If omitted, an OSError is thrown on failure. Otherwise, specify the value to return on failure, such as if the requested key, section or file is not found.</param>
		/// <exception cref="OSError">An OSError exception is thrown if the key can't be found and no default is supplied.</exception>
		public static string IniRead(object filename, object section = null, object key = null, object @default = null)
		{
			var file = filename.As();
			var s = section.As();
			var k = key.As();
			var def = @default.As();
			var result = "";

			if (!File.Exists(file))
				return "";

			if (s != "")
				s = $"[{s}]";

			var haskey = !string.IsNullOrEmpty(k);
			var hassec = !string.IsNullOrEmpty(s);
			var sb = new StringBuilder(1024);
			var inidkt = IniLoad(file);

			if (!haskey && !hassec)
			{
				foreach (DictionaryEntry kv in inidkt)
					_ = sb.AppendLine(((string)kv.Key).Trim(TrimSec));
			}
			else if (haskey && hassec)
			{
				var secdkt = inidkt.GetOrAdd<string, OrderedDictionary, IEqualityComparer>(s, StringComparer.CurrentCultureIgnoreCase);

				if (secdkt.Contains(k))
				{
					var val = secdkt.GetOrAdd<string, string, string>(k, def);
					_ = sb.Append(val);
				}
				else if (def.Length > 0)
					_ = sb.Append(def);
				else
					throw new OSError("", $"Failed to find key {k} in section {s} in INI file {file}.");
			}
			else if (hassec)
			{
				var secdkt = inidkt.GetOrAdd<string, OrderedDictionary, IEqualityComparer>(s, StringComparer.CurrentCultureIgnoreCase);

				foreach (DictionaryEntry kv in secdkt)
					if (((string)kv.Key)[0] != '#')
						_ = sb.AppendLine($"{kv.Key}={kv.Value}");
			}

			result = sb.ToString();
			return result;
		}

		/// <summary>
		/// Writes a value to a standard format .ini file.
		/// </summary>
		/// <param name="value">The string or number that will be written to the right of <paramref name="key"/>'s equal sign (=).
		/// or
		/// The complete content of a section to write to the .ini file, excluding the [SectionName] header.
		/// Key must be omitted. Pairs must not contain any blank lines. If the section already exists, everything up to the last key=value pair is overwritten. Pairs can contain lines without an equal sign (=), but this may produce inconsistent results. Comments can be written to the file but are stripped out when they are read back by IniRead.
		/// </param>
		/// <param name="filename">The name of the .ini file, which is assumed to be in A_WorkingDir if an absolute path isn't specified.</param>
		/// <param name="section">The section name in the .ini file, which is the heading phrase that appears in square brackets (do not include the brackets in this parameter).</param>
		/// <param name="key">The key name in the .ini file.</param>
		/// <exception cref="OSError">An OSError exception is thrown on failure.</exception>
		public static void IniWrite(object value, object filename, object section, object key = null)
		{
			var v = value.As();
			var file = filename.As();
			var s = section.As();
			var k = key.As();
			var within = string.IsNullOrEmpty(s);
			s = string.Format("[{0}]", s ?? string.Empty);
			var haskey = !string.IsNullOrEmpty(k);
			var writer = new StringWriter();

			try
			{
				if (!File.Exists(file))
				{
					writer.WriteLine(s);

					if (haskey)
						writer.WriteLine($"{k}={v}");
					else
						writer.WriteLine(v);
				}
				else
				{
					var inidkt = IniLoad(file);

					if (s != "")
					{
						var kvdkt = inidkt.GetOrAdd<string, OrderedDictionary, IEqualityComparer>(s, StringComparer.CurrentCultureIgnoreCase);

						if (haskey)
						{
							kvdkt[k] = v;
						}
						else
						{
							kvdkt.Clear();//Documentation seems to suggest it should overwrite all in the specified section.

							foreach (Range r in v.AsSpan().SplitAny(TrimLine))
							{
								var pair = v.AsSpan(r).Trim();

								if (pair.Length > 0)
								{
									var equalsIndex = pair.IndexOf('=');

									if (equalsIndex != -1 && equalsIndex != pair.Length - 1)
									{
										var first = pair.Slice(0, equalsIndex);
										var second = pair.Slice(equalsIndex + 1);
										kvdkt[first.Trim(TrimLine).ToString()] = second.Trim(TrimLine).ToString();
									}
								}
							}
						}
					}

					foreach (DictionaryEntry kv in inidkt)
					{
						writer.WriteLine(kv.Key);

						foreach (DictionaryEntry kv2 in (OrderedDictionary)kv.Value)
							if (((string)kv2.Key)[0] != '#')
								writer.WriteLine($"{kv2.Key}={kv2.Value}");
							else
								writer.WriteLine($"{kv2.Key}");

						writer.WriteLine();
					}
				}

				writer.Flush();
				var text = writer.ToString();

				if (File.Exists(file))
					File.Delete(file);

				Files.FileAppend("", file, "unicode");
				File.WriteAllText(file, text);
			}
			catch (Exception ex)
			{
				throw new OSError(ex, $"Error writing key {k} with value {v} in section {s} to INI file {file}.");
			}
		}

		/// <summary>
		/// Private helper to load an Ini file.
		/// </summary>
		/// <param name="filename">The name of the .ini file, which is assumed to be in A_WorkingDir if an absolute path isn't specified.</param>
		/// <returns>An OrderedDictionary with all of the file data in it.</returns>
		private static OrderedDictionary IniLoad(object filename)
		{
			var f = filename.As();
			OrderedDictionary kvdkt = null;
			var inidkt = new OrderedDictionary(StringComparer.CurrentCultureIgnoreCase);

			foreach (var line in File.ReadLines(f))
			{
				var ln = line.Trim(TrimLine);

				if (ln.Length > 0)
				{
					var split = ln.Split('=').Select(l => l.Trim(TrimLine)).ToArray();

					if (ln[0] == '#')
					{
						if (kvdkt == null)
							_ = inidkt.GetOrAdd<string, OrderedDictionary, IEqualityComparer>(ln, StringComparer.CurrentCultureIgnoreCase);
						else
							kvdkt[ln] = "";
					}
					else
					{
						if (split.Length == 1)
							kvdkt = inidkt.GetOrAdd<string, OrderedDictionary, IEqualityComparer>(split[0], StringComparer.CurrentCultureIgnoreCase);
						else if (split.Length == 2 && kvdkt != null)
							kvdkt[split[0]] = split[1];
					}
				}
			}

			return inidkt;
		}
	}
}