using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Order;

namespace Keysharp.Benchmark
{
	/// <summary>
	/// Some benchmarking from https://www.meziantou.net/comparing-implementations-with-benchmarkdotnet.htm
	/// and https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-9/#strings-arrays-spans
	/// </summary>
	[MemoryDiagnoser]
	//[OrderProvider(SummaryOrderPolicy.FastestToSlowest)] // Order the result
	public class HexBench
	{
		// Initialize the byte array for each run
		private byte[] _array;

		//[Params(10, 1000, 10000)]
		[Params(100000)]

		public int Size { get; set; }

		[GlobalSetup]
		public void Setup()
		{
			_array = Enumerable.Range(0, Size).Select(i => (byte)i).ToArray();
		}

		public string ToHexWithStringBuilder(byte[] bytes)
		{
			var hex = new StringBuilder(bytes.Length * 2);

			foreach (byte b in bytes)
				hex.Append(b.ToString("X2"));

			return hex.ToString();
		}

		public string ToHexWithBitConverter(byte[] bytes)
		{
			var hex = BitConverter.ToString(bytes);
			return hex.Replace("-", "");
		}

		public string ToHexWithLookupAndShift(byte[] bytes)
		{
			const string hexAlphabet = "0123456789ABCDEF";
			var result = new StringBuilder(bytes.Length * 2);

			foreach (byte b in bytes)
			{
				result.Append(hexAlphabet[b >> 4]);
				result.Append(hexAlphabet[b & 0xF]);
			}

			return result.ToString();
		}

		public string ToHexWithByteManipulation(byte[] bytes)
		{
			var c = new char[bytes.Length * 2];
			int b;

			for (int i = 0; i < bytes.Length; i++)
			{
				b = bytes[i] >> 4;
				c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
				b = bytes[i] & 0xF;
				c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
			}

			return new string(c);
		}

		public string ConvertToHexString(byte[] bytes) => Convert.ToHexString(bytes);


		[Benchmark(Baseline = true)]
		public string ToHexWithStringBuilder() => ToHexWithStringBuilder(_array);

		[Benchmark]
		public string ToHexWithBitConverter() => ToHexWithBitConverter(_array);

		[Benchmark]
		public string ToHexWithLookupAndShift() => ToHexWithLookupAndShift(_array);

		[Benchmark]
		public string ToHexWithByteManipulation() => ToHexWithByteManipulation(_array);

		[Benchmark]
		public string ConvertToHexString() => ConvertToHexString(_array);

	}
}
