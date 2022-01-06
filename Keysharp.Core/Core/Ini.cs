using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using static Keysharp.Core.Core;

namespace Keysharp.Core
{
	public static class Ini
	{
		/// <summary>
		/// Deletes a value from a standard format .ini file.
		/// </summary>
		/// <param name="file">The name of the file.</param>
		/// <param name="section">The section name.</param>
		/// <param name="key">The key name. If omitted, the entire <paramref name="section"/> will be deleted.</param>
		public static void IniDelete(params object[] obj)
		{
			var (file, section, key) = obj.L().S3();

			if (!System.IO.File.Exists(file))
				return;

			if (section != "")
				section = string.Format(Core.Keyword_IniSectionOpen + "{0}]", section.ToLower());

			try
			{
				var haskey = !string.IsNullOrEmpty(key);
				var hassec = !string.IsNullOrEmpty(section);
				var sb = new StringBuilder(1024);
				var writer = new StringWriter();
				var inidkt = IniLoad(file);

				if (hassec && haskey)
				{
					if (inidkt.Contains(section))
					{
						var secdkt = inidkt[section] as OrderedDictionary;
						secdkt.Remove(key);
					}
				}
				else if (hassec)
					inidkt.Remove(section);

				foreach (DictionaryEntry kv in inidkt)
				{
					writer.WriteLine(kv.Key);

					foreach (DictionaryEntry kv2 in ((OrderedDictionary)kv.Value))
						if (((string)kv2.Key)[0] != '#')
							writer.WriteLine($"{kv2.Key}={kv2.Value}");
						else
							writer.WriteLine($"{kv2.Key}");

					writer.WriteLine();
				}

				writer.Flush();
				var text = writer.ToString();

				if (System.IO.File.Exists(file))
					System.IO.File.Delete(file);

				Disk.FileAppend("", file, "unicode");
				System.IO.File.WriteAllText(file, text);
				Accessors.A_ErrorLevel = 0;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Accessors.A_ErrorLevel = 1;
			}
		}

		/// <summary>
		/// Reads a value from a standard format .ini file.
		/// </summary>
		/// <param name="result">The variable to store the result.</param>
		/// <param name="file">The name of the file.</param>
		/// <param name="section">The section name.</param>
		/// <param name="key">The key name.</param>
		/// <param name="error">The value to store in <paramref name="result"/> if the specified <paramref name="key"/> is not found.
		/// By default this is "ERROR".</param>
		public static string IniRead(params object[] obj)
		{
			var (file, section, key, error) = obj.L().S4("", "", "", "ERROR");
			var result = "";

			if (!System.IO.File.Exists(file))
				return "";

			if (section != "")
				section = string.Format(Core.Keyword_IniSectionOpen + "{0}]", section.ToLower());

			try
			{
				var haskey = !string.IsNullOrEmpty(key);
				var hassec = !string.IsNullOrEmpty(section);
				var sb = new StringBuilder(1024);
				var inidkt = IniLoad(file);

				if (!haskey && !hassec)
				{
					foreach (DictionaryEntry kv in inidkt)
					{
						_ = sb.AppendLine(((string)kv.Key).Trim(trimsec));
					}
				}
				else if (haskey && hassec)
				{
					var secdkt = inidkt.GetOrAdd<string, OrderedDictionary>(section);
					var val = secdkt.GetOrAdd<string, string, string>(key, "");
					_ = sb.Append(val);
				}
				else if (hassec)
				{
					var secdkt = inidkt.GetOrAdd<string, OrderedDictionary>(section);

					foreach (DictionaryEntry kv in secdkt)
						if (((string)kv.Key)[0] != '#')
							_ = sb.AppendLine($"{kv.Key}={kv.Value}");
				}

				result = sb.ToString();
				Accessors.A_ErrorLevel = 0;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Accessors.A_ErrorLevel = 1;
			}

			return result;
		}

		/// <summary>
		/// Writes a value to a standard format .ini file.
		/// </summary>
		/// <param name="value">The string or number that will be written to the right of <paramref name="key"/>'s equal sign (=).</param>
		/// <param name="file">The name of the file.</param>
		/// <param name="section">The section name.</param>
		/// <param name="key">The key name.</param>
		/// <remarks><see cref="Accessors.A_ErrorLevel"/> is set to <c>1</c> if there was a problem or <c>0</c> otherwise.</remarks>
		public static void IniWrite(params object[] obj)
		{
			var (value, file, section, key) = obj.L().S4();
			var within = string.IsNullOrEmpty(section);
			section = string.Format(Core.Keyword_IniSectionOpen + "{0}]", section ?? string.Empty);
			var haskey = !string.IsNullOrEmpty(key);
			var writer = new StringWriter();

			try
			{
				if (!System.IO.File.Exists(file))
				{
					writer.WriteLine(section);

					if (haskey)
						writer.WriteLine($"{key}={value}");
					else
						writer.WriteLine(value);
				}
				else
				{
					var inidkt = IniLoad(file);

					if (section != "")
					{
						var kvdkt = inidkt.GetOrAdd<string, OrderedDictionary>(section);

						if (haskey)
						{
							kvdkt[key] = value;
						}
						else
						{
							kvdkt.Clear();//Documentation seems to suggest it should overwrite all in the specified section.
							var pairsplits = value.Split(trimline, StringSplitOptions.RemoveEmptyEntries);

							foreach (var pair in pairsplits)
							{
								var split = pair.Split('=').Select(l => l.Trim(trimline)).ToArray();

								if (split.Length == 2 && kvdkt != null)
									kvdkt[split[0]] = split[1];
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

				if (System.IO.File.Exists(file))
					System.IO.File.Delete(file);

				Disk.FileAppend("", file, "unicode");
				System.IO.File.WriteAllText(file, text);
				Accessors.A_ErrorLevel = 0;
			}
			catch (IOException ioe)
			{
				Console.WriteLine(ioe.Message);
				Accessors.A_ErrorLevel = 1;
			}
		}

		private static OrderedDictionary IniLoad(params object[] obj)
		{
			var filename = obj.L().S1();
			OrderedDictionary kvdkt = null;
			var inidkt = new OrderedDictionary();

			foreach (var ln in System.IO.File.ReadAllLines(filename).Select(l => l.Trim(trimline)).Where(x => x != ""))
			{
				var split = ln.Split('=').Select(l => l.Trim(trimline)).ToArray();

				if (ln[0] == '#')
				{
					if (kvdkt == null)
						_ = inidkt.GetOrAdd<string, OrderedDictionary>(ln);
					else
						kvdkt[ln] = "";
				}
				else
				{
					if (split.Length == 1)
						kvdkt = inidkt.GetOrAdd<string, OrderedDictionary>(split[0]);
					else if (split.Length == 2 && kvdkt != null)
						kvdkt[split[0]] = split[1];
				}
			}

			return inidkt;
		}
	}
}