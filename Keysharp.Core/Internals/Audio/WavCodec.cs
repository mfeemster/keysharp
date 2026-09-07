namespace Keysharp.Internals.Audio
{
	/// <summary>
	/// The managed RIFF/WAVE reader. It is deliberately narrow: little-endian RIFF carrying integer or IEEE-float
	/// PCM, mono or stereo, at a rate the engine accepts. Everything else is refused by name rather than guessed
	/// at, because a wrong guess reaches the user as noise rather than as an error.
	/// </summary>
	internal static class WavCodec
	{
		// KSDATAFORMAT_SUBTYPE_* share this 12-byte tail; only the leading tag distinguishes PCM from float.
		private static ReadOnlySpan<byte> SubFormatTail => [0x00, 0x00, 0x10, 0x00, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71];

		/// <summary>
		/// Decodes a whole WAV image into interleaved float32.
		/// </summary>
		/// <param name="bytes">The complete file image.</param>
		/// <param name="samples">Interleaved float32, one entry per source sample.</param>
		/// <param name="sampleRate">The declared rate, already range-checked.</param>
		/// <param name="channels">1 or 2.</param>
		/// <param name="error">Why the image was refused, when this returns false.</param>
		internal static bool TryDecode(ReadOnlySpan<byte> bytes, out float[] samples, out int sampleRate, out int channels, out string error)
		{
			samples = null;
			sampleRate = 0;
			channels = 0;
			error = null;

			if (bytes.Length < 12)
			{
				error = "The file is too short to be a WAV image.";
				return false;
			}

			var riff = Tag(bytes, 0);

			if (riff == "RIFX")
			{
				error = "Big-endian RIFX WAV files are not supported.";
				return false;
			}

			if (riff == "RF64")
			{
				error = "RF64 WAV files are not supported.";
				return false;
			}

			if (riff != "RIFF" || Tag(bytes, 8) != "WAVE")
			{
				error = "The file is not a RIFF/WAVE image.";
				return false;
			}

			// The declared RIFF size is advisory: a real file may carry trailing bytes. Chunk walking is bounded by
			// the smaller of the declaration and the actual image, and every chunk is bounds-checked besides.
			var limit = Math.Min((long)bytes.Length, 8L + U32(bytes, 4));
			var haveFormat = false;
			int formatTag = 0, bits = 0, blockAlign = 0;
			var dataRanges = new List<(int Offset, int Length)>();
			var position = 12L;

			while (position + 8 <= limit)
			{
				var id = Tag(bytes, (int)position);
				var size = U32(bytes, (int)position + 4);
				var body = position + 8;

				if (body + size > bytes.Length)
				{
					error = $"The {id} chunk declares {size} bytes but the file ends before them.";
					return false;
				}

				if (id == "fmt ")
				{
					if (haveFormat)
					{
						error = "The file carries more than one format chunk.";
						return false;
					}

					if (!TryReadFormat(bytes.Slice((int)body, (int)size), out formatTag, out channels, out sampleRate, out blockAlign, out bits, out error))
						return false;

					haveFormat = true;
				}
				else if (id == "data")
				{
					dataRanges.Add(((int)body, (int)size));
				}

				// Chunks are word-aligned: an odd size is followed by one pad byte that is not part of any chunk.
				position = body + size + (size & 1);
			}

			if (!haveFormat)
			{
				error = "The file carries no format chunk.";
				return false;
			}

			if (dataRanges.Count == 0)
			{
				error = "The file carries no data chunk.";
				return false;
			}

			var format = Canonical(formatTag, bits);

			if (format == null)
			{
				error = $"Unsupported WAV sample layout: format tag {formatTag} at {bits} bits.";
				return false;
			}

			_ = AudioFormats.TryResolve(format, out _, out var bytesPerSample);
			long total = 0;

			foreach (var (_, length) in dataRanges)
			{
				if (length % bytesPerSample != 0)
				{
					error = "A data chunk does not hold a whole number of samples.";
					return false;
				}

				total += length;
			}

			if (total == 0 || total % blockAlign != 0)
			{
				error = "The data chunks do not hold a whole number of audio frames.";
				return false;
			}

			var sampleCount = total / bytesPerSample;

			if (sampleCount * 4L > AudioFormats.MaxClipBytes)
			{
				error = $"The decoded audio would exceed the {AudioFormats.MaxClipBytes / (1024 * 1024)} MiB per-clip limit.";
				return false;
			}

			var result = new float[sampleCount];
			var written = 0;

			foreach (var (offset, length) in dataRanges)
			{
				var count = length / bytesPerSample;

				if (!AudioFormats.TryConvert(bytes.Slice(offset, length), format, result.AsSpan(written, count), out error))
					return false;

				written += count;
			}

			samples = result;
			return true;
		}

		/// <summary>
		/// The 44-byte canonical RIFF/WAVE header. Written before any frames so a recording that
		/// is interrupted still has a parseable file, and rewritten at the end once the true length is known.
		/// </summary>
		internal static byte[] Header(int sampleRate, int channels, int bitsPerSample, bool isFloat, long dataBytes)
		{
			var blockAlign = channels * (bitsPerSample / 8);
			var header = new byte[44];
			"RIFF"u8.CopyTo(header);
			WriteU32(header, 4, (uint)Math.Min(uint.MaxValue, 36 + dataBytes));
			"WAVE"u8.CopyTo(header.AsSpan(8));
			"fmt "u8.CopyTo(header.AsSpan(12));
			WriteU32(header, 16, 16);
			WriteU16(header, 20, isFloat ? (ushort)3 : (ushort)1);   // WAVE_FORMAT_IEEE_FLOAT or WAVE_FORMAT_PCM
			WriteU16(header, 22, (ushort)channels);
			WriteU32(header, 24, (uint)sampleRate);
			WriteU32(header, 28, (uint)(sampleRate * blockAlign));
			WriteU16(header, 32, (ushort)blockAlign);
			WriteU16(header, 34, (ushort)bitsPerSample);
			"data"u8.CopyTo(header.AsSpan(36));
			WriteU32(header, 40, (uint)Math.Min(uint.MaxValue, dataBytes));
			return header;
		}

		/// <summary>
		/// Converts interleaved float32 into the integer or float layout a recording is being written in. The
		/// format is resolved once and each layout gets its own tight loop, because this runs on every captured
		/// chunk and a per-sample string comparison would dominate it.
		/// </summary>
		internal static void FromFloat(ReadOnlySpan<float> source, string canonicalFormat, Span<byte> destination)
		{
			switch (canonicalFormat)
			{
				case AudioFormats.Unsigned8:
					for (var i = 0; i < source.Length; i++)
						destination[i] = (byte)Math.Clamp((int)MathF.Round((Clamp(source[i]) * 128f) + 128f), 0, 255);

					break;

				case AudioFormats.Signed24:
					for (var i = 0; i < source.Length; i++)
					{
						var v = Clamp(source[i]);
						var s24 = (int)MathF.Round(v < 0 ? v * 8388608f : v * 8388607f);
						var o = i * 3;
						destination[o] = (byte)s24;
						destination[o + 1] = (byte)(s24 >> 8);
						destination[o + 2] = (byte)(s24 >> 16);
					}

					break;

				case AudioFormats.Signed32:
					for (var i = 0; i < source.Length; i++)
					{
						var v = Clamp(source[i]);
						WriteU32(destination, i * 4, (uint)(int)Math.Round(v < 0 ? v * 2147483648.0 : v * 2147483647.0));
					}

					break;

				case AudioFormats.Float32:
					for (var i = 0; i < source.Length; i++)
						WriteU32(destination, i * 4, (uint)BitConverter.SingleToInt32Bits(Clamp(source[i])));

					break;

				default:   // Signed16 is the default recording layout
					for (var i = 0; i < source.Length; i++)
					{
						var v = Clamp(source[i]);
						var s16 = (short)MathF.Round(v < 0 ? v * 32768f : v * 32767f);
						destination[i * 2] = (byte)s16;
						destination[(i * 2) + 1] = (byte)(s16 >> 8);
					}

					break;
			}
		}

		/// <summary>A sample outside full scale would wrap when it is narrowed, so it is bounded first.</summary>
		private static float Clamp(float v) => v < -1f ? -1f : v > 1f ? 1f : v;

		private static void WriteU32(Span<byte> b, int o, uint v)
		{
			b[o] = (byte)v;
			b[o + 1] = (byte)(v >> 8);
			b[o + 2] = (byte)(v >> 16);
			b[o + 3] = (byte)(v >> 24);
		}

		private static void WriteU16(Span<byte> b, int o, ushort v)
		{
			b[o] = (byte)v;
			b[o + 1] = (byte)(v >> 8);
		}

		/// <summary>Reads and validates one format chunk, resolving WAVE_FORMAT_EXTENSIBLE to the tag it stands for.</summary>
		private static bool TryReadFormat(ReadOnlySpan<byte> fmt, out int formatTag, out int channels, out int sampleRate,
										  out int blockAlign, out int bits, out string error)
		{
			formatTag = channels = sampleRate = blockAlign = bits = 0;

			if (fmt.Length < 16)
			{
				error = "The format chunk is too short.";
				return false;
			}

			formatTag = U16(fmt, 0);
			channels = U16(fmt, 2);
			sampleRate = (int)U32(fmt, 4);
			blockAlign = U16(fmt, 12);
			bits = U16(fmt, 14);

			if (formatTag == 0xFFFE)
			{
				if (fmt.Length < 40 || U16(fmt, 16) < 22)
				{
					error = "The extensible format chunk is too short.";
					return false;
				}

				var validBits = U16(fmt, 18);

				if (validBits != bits)
				{
					error = $"The extensible format declares {validBits} valid bits inside a {bits}-bit container, which this reader does not repack.";
					return false;
				}

				var mask = U32(fmt, 20);

				// Absent is fine; present must describe exactly as many speakers as there are channels.
				if (mask != 0 && System.Numerics.BitOperations.PopCount(mask) != channels)
				{
					error = "The extensible channel mask does not match the channel count.";
					return false;
				}

				var sub = fmt.Slice(24, 16);

				if (!sub.Slice(4, 12).SequenceEqual(SubFormatTail))
				{
					error = "The extensible subformat is not a PCM or IEEE-float subtype.";
					return false;
				}

				formatTag = (int)U32(sub, 0);
			}

			if (formatTag != 1 && formatTag != 3)
			{
				error = $"WAV format tag {formatTag} is compressed; only PCM and IEEE float are supported.";
				return false;
			}

			if (!AudioFormats.IsValidChannels(channels))
			{
				error = $"WAV channel count {channels} is outside the supported mono or stereo range.";
				return false;
			}

			if (!AudioFormats.IsValidSampleRate(sampleRate))
			{
				error = $"WAV sample rate {sampleRate} is outside {AudioFormats.MinSampleRate} through {AudioFormats.MaxSampleRate} Hz.";
				return false;
			}

			if (Canonical(formatTag, bits) == null)
			{
				error = $"Unsupported WAV sample layout: format tag {formatTag} at {bits} bits.";
				return false;
			}

			// A block align that disagrees with the declared layout means the file is describing two different
			// things, and every offset computed from it would be wrong.
			if (blockAlign != channels * (bits / 8))
			{
				error = "The WAV block alignment disagrees with its channel count and sample width.";
				return false;
			}

			error = null;
			return true;
		}

		/// <summary>The headerless format name a WAV tag and width correspond to, or null when unsupported.</summary>
		private static string Canonical(int formatTag, int bits) => formatTag switch
		{
			1 => bits switch
			{
				8 => AudioFormats.Unsigned8,
				16 => AudioFormats.Signed16,
				24 => AudioFormats.Signed24,
				32 => AudioFormats.Signed32,
				_ => null,
			},
			3 => bits == 32 ? AudioFormats.Float32 : null,
			_ => null,
		};

		private static string Tag(ReadOnlySpan<byte> b, int offset)
			=> offset + 4 <= b.Length ? Encoding.ASCII.GetString(b.Slice(offset, 4)) : "";

		private static uint U32(ReadOnlySpan<byte> b, int o) => (uint)(b[o] | (b[o + 1] << 8) | (b[o + 2] << 16) | (b[o + 3] << 24));

		private static int U16(ReadOnlySpan<byte> b, int o) => b[o] | (b[o + 1] << 8);
	}
}
