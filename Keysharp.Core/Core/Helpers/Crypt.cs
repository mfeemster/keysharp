using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Keysharp.Core
{
	public static class Crypt
	{
		internal static byte[] Encrypt(object value, object key, bool decrypt, SymmetricAlgorithm alg)
		{
			var size = 0;

			foreach (var legal in alg.LegalKeySizes)
				size = Math.Max(size, legal.MaxSize);

			var k = new byte[size / 8];
			var keyBytes = ToByteArray(key);

			if (keyBytes.Length < k.Length)
			{
				var padded = new byte[k.Length];
				keyBytes.CopyTo(padded, 0);
				keyBytes = padded;
			}

			for (var i = 0; i < k.Length; i++)
				k[i] = keyBytes[i];

			try
			{
				alg.Key = k;
			}
			catch (CryptographicException)
			{
				Accessors.A_ErrorLevel = 2;
				return new byte[] { };
			}

			var iv = new byte[alg.IV.Length];
			var hash = new SHA1Managed().ComputeHash(keyBytes, 0, iv.Length);

			for (var i = 0; i < Math.Min(iv.Length, hash.Length); i++)
				iv[i] = hash[i];

			alg.IV = iv;
			var trans = decrypt ? alg.CreateDecryptor() : alg.CreateEncryptor();
			var buffer = ToByteArray(value);
			var result = trans.TransformFinalBlock(buffer, 0, buffer.Length);
			return result;
		}

		internal static string Hash(object value, HashAlgorithm alg)
		{
			var raw = ToByteArray(value);
			var result = alg.ComputeHash(raw);
			return Strings.BytesToHexString(result);
		}

		internal static byte[] ToByteArray(object value)
		{
			if (value is string s)
				return Encoding.Unicode.GetBytes(s);

			if (value is byte[] b)
				return b;

			if (value == null)
				return new byte[] { };

			var formatter = new BinaryFormatter();

			var writer = new MemoryStream();

			//For some reason this is obsolete, but no replacement has been provided, so we still use it.
#pragma warning disable SYSLIB0011
			formatter.Serialize(writer, value);

#pragma warning restore SYSLIB0001
			return writer.ToArray();
		}
	}
}