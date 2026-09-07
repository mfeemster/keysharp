namespace Keysharp.Internals.Audio
{
	/// <summary>
	/// The headerless PCM vocabulary and the one conversion every source funnels through. Sample data enters the
	/// engine in one of five documented layouts and leaves as interleaved float32, which is the only shape the
	/// mixer and every backend agree on.
	/// </summary>
	internal static class AudioFormats
	{
		internal const int MinSampleRate = 8_000;
		internal const int MaxSampleRate = 192_000;
		internal const int MinChannels = 1;
		internal const int MaxChannels = 2;

		/// <summary>512 MiB of canonical prepared data per clip, checked before any allocation.</summary>
		internal const long MaxClipBytes = 512L * 1024 * 1024;

		internal const string Unsigned8 = "Unsigned8";
		internal const string Signed16 = "Signed16";
		internal const string Signed24 = "Signed24";
		internal const string Signed32 = "Signed32";
		internal const string Float32 = "Float32";

		/// <summary>
		/// Maps a script token to its canonical spelling and width. Comparison ignores case, as every documented
		/// token does; the returned spelling is the one every getter reports back.
		/// </summary>
		internal static bool TryResolve(string token, out string canonical, out int bytesPerSample)
		{
			if (string.Equals(token, Unsigned8, StringComparison.OrdinalIgnoreCase)) { canonical = Unsigned8; bytesPerSample = 1; return true; }

			if (string.Equals(token, Signed16, StringComparison.OrdinalIgnoreCase)) { canonical = Signed16; bytesPerSample = 2; return true; }

			if (string.Equals(token, Signed24, StringComparison.OrdinalIgnoreCase)) { canonical = Signed24; bytesPerSample = 3; return true; }

			if (string.Equals(token, Signed32, StringComparison.OrdinalIgnoreCase)) { canonical = Signed32; bytesPerSample = 4; return true; }

			if (string.Equals(token, Float32, StringComparison.OrdinalIgnoreCase)) { canonical = Float32; bytesPerSample = 4; return true; }

			canonical = null;
			bytesPerSample = 0;
			return false;
		}

		internal static bool IsValidSampleRate(long rate) => rate >= MinSampleRate && rate <= MaxSampleRate;

		internal static bool IsValidChannels(long channels) => channels >= MinChannels && channels <= MaxChannels;

		/// <summary>
		/// Converts interleaved PCM into interleaved float32. Signed formats normalize asymmetrically, dividing a
		/// negative value by the full negative extent and a positive value by the smaller positive one, so that
		/// full-scale silence-to-peak maps onto exactly [-1, 1] without clipping either end.
		/// </summary>
		/// <param name="source">Exactly <paramref name="dst"/>.Length samples worth of bytes.</param>
		/// <param name="canonicalFormat">A spelling already resolved by <see cref="TryResolve"/>.</param>
		/// <param name="dst">Receives one float per source sample.</param>
		/// <param name="error">Set when a Float32 source carries a value the mixer cannot accept.</param>
		internal static bool TryConvert(ReadOnlySpan<byte> source, string canonicalFormat, Span<float> dst, out string error)
		{
			error = null;

			switch (canonicalFormat)
			{
				case Unsigned8:
					for (var i = 0; i < dst.Length; i++)
						dst[i] = (source[i] - 128) / 128f;

					return true;

				case Signed16:
					for (var i = 0; i < dst.Length; i++)
					{
						var v = (short)(source[i * 2] | (source[(i * 2) + 1] << 8));
						dst[i] = v < 0 ? v / 32768f : v / 32767f;
					}

					return true;

				case Signed24:
					for (var i = 0; i < dst.Length; i++)
					{
						var o = i * 3;
						var raw = source[o] | (source[o + 1] << 8) | (source[o + 2] << 16);

						if ((raw & 0x800000) != 0)
							raw |= unchecked((int)0xFF000000);   // sign-extend the packed three-byte value

						dst[i] = raw < 0 ? raw / 8388608f : raw / 8388607f;
					}

					return true;

				case Signed32:
					for (var i = 0; i < dst.Length; i++)
					{
						var o = i * 4;
						var raw = source[o] | (source[o + 1] << 8) | (source[o + 2] << 16) | (source[o + 3] << 24);
						dst[i] = raw < 0 ? raw / 2147483648f : raw / 2147483647f;
					}

					return true;

				case Float32:
					for (var i = 0; i < dst.Length; i++)
					{
						var v = BitConverter.Int32BitsToSingle(source[i * 4] | (source[(i * 4) + 1] << 8) | (source[(i * 4) + 2] << 16) | (source[(i * 4) + 3] << 24));

						// A NaN or an out-of-range sample would propagate through every later gain stage, so it is
						// refused at admission rather than silently saturated deep inside the mixer.
						if (!float.IsFinite(v) || v < -1f || v > 1f)
						{
							error = $"Float32 sample {i} is {v}; every sample must be finite and within -1 through 1.";
							return false;
						}

						dst[i] = v;
					}

					return true;

				default:
					error = $"Unknown sample format {canonicalFormat}.";
					return false;
			}
		}
	}
}
