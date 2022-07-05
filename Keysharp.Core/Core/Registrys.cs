using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace Keysharp.Core
{
	public static class Registrys
	{
		/// <summary>
		/// Deletes a value from the registry.
		/// </summary>
		/// <param name="KeyName">The full name of the registry key</param>
		/// <param name="ValueName">The name of the value to delete. If blank or omitted, the key's default value will be deleted.</param>
		public static void RegDelete(object obj0 = null, object obj1 = null)
		{
			var keyname = obj0.As();
			var valname = obj1.As();

			try
			{
				if (keyname?.Length == 0)
					if (Accessors.A_LoopRegKey is string k)
						keyname = k;

				if (valname?.Length == 0)
				{
					if (Accessors.A_LoopRegType is string t)
					{
						if (t == "KEY")
						{
						}
						else if (t != "" && valname?.Length == 0)//Wasn't overriden with passed in parameter.
							valname = Accessors.A_LoopRegName;
					}
				}

				var val = valname.ToLowerInvariant();

				if (val == "(default)" || val == "ahk_default")
					val = string.Empty;

				Conversions.ToRegKey(keyname, true).Item1.DeleteValue(val, true);
			}
			catch (Exception ex)
			{
				throw new OSError(ex);
			}
		}

		/// <summary>
		/// Deletes a key from the registry.
		/// </summary>
		/// <param name="KeyName">They full path of the key to delete</param>
		public static void RegDeleteKey(object obj0 = null)
		{
			var keyname = obj0.As();

			try
			{
				if (keyname?.Length == 0)
					if (Accessors.A_LoopRegKey is string k)
						keyname = k;

				var (reg, comp, key) = Conversions.ToRegRootKey(keyname);
				reg.DeleteSubKeyTree(key, true);
			}
			catch (Exception ex)
			{
				throw new OSError(ex);
			}
		}

		/// <summary>
		/// Reads a value from the registry.
		/// </summary>
		/// <param name="KeyName">The name of the key</param>
		/// <param name="ValueName">The name of the value to retrieve. If omitted, Subkey's default value will be retrieved, which is the value displayed as "(Default)" by RegEdit. If there is no default value (that is, if RegEdit displays "value not set"), OutputVar is made blank and Accessors.A_ErrorLevel is set to 1.</param>
		/// <returns>The value retrieved. If the value cannot be retrieved, the variable is made blank and Accessors.A_ErrorLevel is set to 1.</returns>
		public static object RegRead(object obj0 = null, object obj1 = null, object obj2 = null)
		{
			var keyname = obj0.As();
			var valname = obj1.As();
			var def = obj2.As();

			try
			{
				if (keyname.Length == 0)
					if (Accessors.A_LoopRegKey is string k)
						keyname = k;

				if (valname.Length == 0)
				{
					if (Accessors.A_LoopRegType is string t)
					{
						if (t == "KEY")
						{
						}
						else if (t != "" && valname?.Length == 0)//Wasn't overriden with passed in parameter.
							valname = Accessors.A_LoopRegName;
					}
				}

				valname = valname.ToLowerInvariant();

				if (valname == "(default)" || valname == "ahk_default")
					valname = string.Empty;

				var reg = Conversions.ToRegKey(keyname, false).Item1.GetValue(valname);

				if (reg is int i)//All integer numbers need to be longs.
					reg = (long)i;
				else if (reg is uint ui)
					reg = (long)ui;
				else if (reg is string[] sa)
					reg = new Array(sa);
				else if (reg is byte[] ba)
					reg = new Array(ba);
				else if (reg is null)
				{
					if (!string.IsNullOrEmpty(def))
						return def;

					throw new OSError("", $"Registry key {keyname} and value {valname} was not found and no default was specified.");
				}

				return reg;
			}
			catch (Exception ex)
			{
				throw new OSError(ex);
			}
		}

		/// <summary>
		/// Writes a value to the registry.
		/// </summary>
		/// <param name="Value">The value to be written. If omitted, it will default to an empty (blank) string, or 0, depending on ValueType.</param>
		/// <param name="ValueType">Must be either REG_SZ, REG_EXPAND_SZ, REG_MULTI_SZ, REG_DWORD, or REG_BINARY.</param>
		/// <param name="KeyName">Full path of the registry key, including the remote computer if needed.</param>
		/// <param name="ValueName">The name of the value that will be written to. If blank or omitted, Subkey's default value will be used, which is the value displayed as "(Default)" by RegEdit.</param>
		public static void RegWrite(object obj0, object obj1 = null, object obj2 = null, object obj3 = null)
		{
			var val = obj0;
			var valtype = obj1.As();
			var keyname = obj2.As();
			var valname = obj3.As();

			try
			{
				var sub = keyname;

				if (keyname?.Length == 0)
				{
					if (valtype?.Length == 0)
					{
						if (Accessors.A_LoopRegType is string t)
						{
							if (t == "KEY")
							{
							}
							else if (t != "")
							{
								valtype = t;//In this case, value type should be gotten from the current loop.

								if (valname?.Length == 0)//Wasn't overriden with passed in parameter.
									valname = Accessors.A_LoopRegName;
							}
						}
					}

					if (Accessors.A_LoopRegKey is string k)
						if (Accessors.A_LoopRegType is string t)
							if (t == "KEY")
								keyname = k;
				}

				valname = valname.ToLowerInvariant();

				if (valname == "(default)" || valname == "ahk_default")
					valname = string.Empty;

				var (reg, comp, key) = Conversions.ToRegKey(keyname, true);

				if (reg != null)
				{
					var regtype = Conversions.GetRegistryType(valtype);

					if (val is string vs)
					{
						if (regtype == Microsoft.Win32.RegistryValueKind.Binary)
							val = Conversions.StringToByteArray(vs);
						else if (regtype == Microsoft.Win32.RegistryValueKind.MultiString)
							val = vs.Split('\n');
					}

					reg.SetValue(valname, val, regtype);
				}
			}
			catch (Exception ex)
			{
				throw new OSError(ex);
			}
		}

		public static void SetRegView(object obj) => Accessors.A_RegView = obj;

		internal static RegistryView GetRegView() => (long)Accessors.A_RegView == 32L ? RegistryView.Registry32 : RegistryView.Registry64;
	}
}