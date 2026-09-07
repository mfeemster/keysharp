namespace Keysharp.Internals.Audio
{
	/// <summary>
	/// One decoded sound, immutable once constructed. Every source route funnels here: a WAV image, a headerless
	/// PCM copy taken from a script <c>Buffer</c>, or later a codec worker. The samples are interleaved float32 at
	/// the rate and channel count they were authored in; converting them to an output's negotiated format is
	/// <see cref="PrepareFor"/>, which happens during setup rather than on a play call.
	/// </summary>
	internal sealed class AudioClipData
	{
		/// <summary>Interleaved float32 at <see cref="SampleRate"/> and <see cref="Channels"/>. Never mutated.</summary>
		internal readonly float[] Samples;
		internal readonly int SampleRate;
		internal readonly int Channels;

		/// <summary>The canonical token this clip's samples arrived as, reported back by the script property.</summary>
		internal readonly string SampleFormat;

		internal AudioClipData(float[] samples, int sampleRate, int channels, string sampleFormat)
		{
			Samples = samples;
			SampleRate = sampleRate;
			Channels = channels;
			SampleFormat = sampleFormat;
		}

		internal long FrameCount => Channels == 0 ? 0 : Samples.LongLength / Channels;

		internal double DurationMilliseconds => SampleRate == 0 ? 0 : FrameCount * 1000.0 / SampleRate;

		/// <summary>
		/// Converts this clip once into an output's negotiated rate and channel count. The result is cached by the
		/// output for as long as that format generation lives, so a play call never resamples.
		/// </summary>
		internal float[] PrepareFor(int targetRate, int targetChannels)
		{
			var mapped = MapChannels(Samples, Channels, targetChannels);
			return targetRate == SampleRate ? mapped : Resample(mapped, targetChannels, SampleRate, targetRate);
		}

		/// <summary>
		/// Mono to stereo duplicates into both sides; stereo to mono averages, which is the only mapping that
		/// preserves apparent loudness without clipping a correlated pair.
		/// </summary>
		private static float[] MapChannels(float[] source, int from, int to)
		{
			if (from == to)
				return source;

			var frames = source.LongLength / from;
			var result = new float[frames * to];

			if (from == 1 && to == 2)
			{
				for (long f = 0; f < frames; f++)
				{
					var v = source[f];
					result[f * 2] = v;
					result[(f * 2) + 1] = v;
				}
			}
			else   // 2 -> 1
			{
				for (long f = 0; f < frames; f++)
					result[f] = (source[f * 2] + source[(f * 2) + 1]) * 0.5f;
			}

			return result;
		}

		/// <summary>
		/// Rate conversion by linear interpolation, preceded by a box average when downsampling by more than half
		/// an octave. The box is not a brick wall, but without it a large downshift folds everything above the new
		/// Nyquist frequency back into the audible band, which is far more audible than the gentle high-frequency
		/// loss the average costs.
		/// </summary>
		private static float[] Resample(float[] source, int channels, int fromRate, int toRate)
		{
			var srcFrames = source.LongLength / channels;

			if (srcFrames == 0)
				return [];

			var ratio = (double)fromRate / toRate;
			var filtered = ratio > 1.5 ? BoxAverage(source, channels, srcFrames, (int)Math.Min(ratio, 64)) : source;
			var dstFrames = Math.Max(1L, (long)Math.Round(srcFrames / ratio));
			var result = new float[dstFrames * channels];

			for (long d = 0; d < dstFrames; d++)
			{
				var pos = d * ratio;
				var i0 = (long)pos;
				var frac = (float)(pos - i0);
				var i1 = Math.Min(i0 + 1, srcFrames - 1);

				for (var c = 0; c < channels; c++)
				{
					var a = filtered[(i0 * channels) + c];
					var b = filtered[(i1 * channels) + c];
					result[(d * channels) + c] = a + ((b - a) * frac);
				}
			}

			return result;
		}

		/// <summary>A moving average `width` frames wide, applied per channel, used only as the anti-alias step above.</summary>
		private static float[] BoxAverage(float[] source, int channels, long frames, int width)
		{
			if (width < 2)
				return source;

			var result = new float[source.LongLength];
			var inv = 1f / width;

			for (long f = 0; f < frames; f++)
			{
				for (var c = 0; c < channels; c++)
				{
					var sum = 0f;

					for (var k = 0; k < width; k++)
					{
						var s = Math.Min(f + k, frames - 1);
						sum += source[(s * channels) + c];
					}

					result[(f * channels) + c] = sum * inv;
				}
			}

			return result;
		}
	}
}
