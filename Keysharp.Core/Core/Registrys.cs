using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace Keysharp.Core
{
	public static class Registrys
	{
		/// <summary>
		/// Deletes a key from the registry.
		/// </summary>
		/// <param name="KeyName">They full path of the key to delete</param>
		public static void RegDeleteKey(params object[] obj)
		{
			var keyname = obj.L().S1();

			try
			{
				if (keyname?.Length == 0)
					if (Accessors.A_LoopRegKey is string k)
						keyname = k;

				var (reg, comp, key) = Conversions.ToRegRootKey(keyname);
				reg.DeleteSubKeyTree(key, true);
				Accessors.A_ErrorLevel = 0;
			}
			catch (Exception e)
			{
				Keysharp.Scripting.Script.OutputDebug(e.Message);
				Accessors.A_ErrorLevel = 1;
			}
		}

		/// <summary>
		/// Deletes a value from the registry.
		/// </summary>
		/// <param name="KeyName">The full name of the registry key</param>
		/// <param name="ValueName">The name of the value to delete. If blank or omitted, the key's default value will be deleted.</param>
		public static void RegDelete(params object[] obj)
		{
			var (keyname, valname) = obj.L().S2();

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
				Accessors.A_ErrorLevel = 0;
			}
			catch (Exception e)
			{
				Keysharp.Scripting.Script.OutputDebug(e.Message);
				Accessors.A_ErrorLevel = 1;
			}
		}

		/// <summary>
		/// Reads a value from the registry.
		/// </summary>
		/// <param name="KeyName">The name of the key</param>
		/// <param name="ValueName">The name of the value to retrieve. If omitted, Subkey's default value will be retrieved, which is the value displayed as "(Default)" by RegEdit. If there is no default value (that is, if RegEdit displays "value not set"), OutputVar is made blank and Accessors.A_ErrorLevel is set to 1.</param>
		/// <returns>The value retrieved. If the value cannot be retrieved, the variable is made blank and Accessors.A_ErrorLevel is set to 1.</returns>
		public static object RegRead(params object[] obj)
		{
			var (keyname, valname) = obj.L().S2();

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

				Accessors.A_ErrorLevel = 0;
				return reg;
			}
			catch (Exception e)
			{
				Keysharp.Scripting.Script.OutputDebug(e.Message);
				Accessors.A_ErrorLevel = 1;
			}

			return "";
		}

		/// <summary>
		/// Writes a value to the registry.
		/// </summary>
		/// <param name="Value">The value to be written. If omitted, it will default to an empty (blank) string, or 0, depending on ValueType.</param>
		/// <param name="ValueType">Must be either REG_SZ, REG_EXPAND_SZ, REG_MULTI_SZ, REG_DWORD, or REG_BINARY.</param>
		/// <param name="KeyName">Full path of the registry key, including the remote computer if needed.</param>
		/// <param name="ValueName">The name of the value that will be written to. If blank or omitted, Subkey's default value will be used, which is the value displayed as "(Default)" by RegEdit.</param>
		public static void RegWrite(params object[] obj)
		{
			var (val, valtype, keyname, valname) = obj.L().Os3();

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
					Accessors.A_ErrorLevel = 0;
				}
			}
			catch (Exception e)
			{
				Keysharp.Scripting.Script.OutputDebug(e.Message);
				Accessors.A_ErrorLevel = 1;
			}
		}

		public static void SetRegView(params object[] obj)
		{
			var val = obj.L().L1(64L);

			if (val == 32L || val == 64L)
				Accessors.A_RegView = val;
		}

		internal static RegistryView GetRegView() => Accessors.A_RegView == 32L ? RegistryView.Registry32 : RegistryView.Registry64;
	}
}