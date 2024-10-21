namespace Keysharp.Core
{
	public static class Security
	{
		/// <summary>
		/// Encrypt or decrypt data with the AES algorithm.
		/// </summary>
		/// <param name="value">The data to encrypt or decrypt.</param>
		/// <param name="key">The secret key.</param>
		/// <param name="decrypt"><code>true</code> to decrypt the given <paramref name="value"/>, otherwise encrypt.</param>
		/// <returns>The corresponding encrypted or decrypted data.</returns>
		public static Array AES(object value, object key, bool decrypt = false) => new Array(Crypt.Encrypt(value, key, decrypt, Aes.Create()));

		/// <summary>
		/// Calculates the CRC32 polynomial of an object.
		/// </summary>
		/// <param name="value">The object to check.</param>
		/// <returns>A checksum of <paramref name="value"/> as an integer.</returns>
		public static long CRC32(object value)
		{
			var raw = Crypt.ToByteArray(value);
			var alg = new CRC32();
			_ = alg.ComputeHash(raw);
			return alg.Value;
		}

		/// <summary>
		/// Calculates the MD5 hash of an object.
		/// </summary>
		/// <param name="value">The object to hash.</param>
		/// <returns>A 32-character hexadecimal number.</returns>
		public static string MD5(object value) => Crypt.Hash(value, System.Security.Cryptography.MD5.Create());

		/// <summary>
		/// Generates a secure (cryptographic) random number.
		/// </summary>
		/// <param name="min">The lower bound.</param>
		/// <param name="max">The upper bound.</param>
		/// <returns>A random number between the specified range. Leave both parameters blank to give any 128-bit numeric result.
		/// If <paramref name="min"/> and <paramref name="max"/> are both pure integers, the result would also be an integer without a remainder.</returns>
		/// <remarks>A cryptographic random number generator produces an output that is computationally infeasible to predict with a probability that is better than one half.
		/// <see cref="Random"/> uses a simpler algorithm which is much faster but less secure.</remarks>
		public static decimal SecureRandom(decimal min = decimal.MinValue, decimal max = decimal.MaxValue)
		{
			var diff = Math.Abs(min - max);

			if (diff == 0 && !(min == 0 && max == 0))
				return min;

			var csp = RandomNumberGenerator.Create();
			var rnd = new byte[4 * 3 + 1];
			csp.GetBytes(rnd);
			var s = new int[3];

			for (var i = 0; i < s.Length; i++)
				s[i] = BitConverter.ToInt32(rnd, i * 4);

			var rem = decimal.Remainder(min, 1) != 0 || decimal.Remainder(max, 1) != 0;
			byte scale = 0;

			if (rem)
				scale = (byte)(rnd[12] % 28);

			var val = new decimal(s[0], s[1], s[2], false, scale);
			return diff == 0 ? val : min + val % diff;
		}

		/// <summary>
		/// Calculates the SHA1 hash of an object.
		/// </summary>
		/// <param name="value">The object to hash.</param>
		/// <returns>A 40-character hexadecimal number.</returns>
		public static string SHA1(object value) => Crypt.Hash(value, System.Security.Cryptography.SHA1.Create());

		/// <summary>
		/// Calculates the SHA256 hash of an object.
		/// </summary>
		/// <param name="value">The object to hash.</param>
		/// <returns>A 64-character hexadecimal number.</returns>
		public static string SHA256(object value) => Crypt.Hash(value, System.Security.Cryptography.SHA256.Create());

		/// <summary>
		/// Calculates the SHA384 hash of an object.
		/// </summary>
		/// <param name="value">The object to hash.</param>
		/// <returns>A 96-character hexadecimal number.</returns>
		public static string SHA384(object value) => Crypt.Hash(value, System.Security.Cryptography.SHA384.Create());

		/// <summary>
		/// Calculates the SHA512 hash of an object.
		/// </summary>
		/// <param name="value">The object to hash.</param>
		/// <returns>A 128-character hexadecimal number.</returns>
		public static string SHA512(object value) => Crypt.Hash(value, System.Security.Cryptography.SHA512.Create());
	}
}