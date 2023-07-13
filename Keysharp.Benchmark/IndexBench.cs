using static Keysharp.Core.Accessors;
using static Keysharp.Core.Core;
using static Keysharp.Core.Common.Keyboard.HotstringDefinition;
using static Keysharp.Core.Dialogs;
using static Keysharp.Core.Dir;
using static Keysharp.Core.Drive;
using static Keysharp.Core.DllHelper;
using static Keysharp.Core.Env;
using static Keysharp.Core.KeysharpFile;
using static Keysharp.Core.Flow;
using static Keysharp.Core.Function;
using static Keysharp.Core.GuiHelper;
using static Keysharp.Core.Images;
using static Keysharp.Core.ImageLists;
using static Keysharp.Core.Ini;
using static Keysharp.Core.Keyboard;
using static Keysharp.Core.KeysharpObject;
using static Keysharp.Core.Loops;
using static Keysharp.Core.Maths;
using static Keysharp.Core.Menu;
using static Keysharp.Core.Misc;
using static Keysharp.Core.Monitor;
using static Keysharp.Core.Mouse;
using static Keysharp.Core.Network;
using static Keysharp.Core.Options;
using static Keysharp.Core.Processes;
using static Keysharp.Core.Registrys;
using static Keysharp.Core.Screen;
using static Keysharp.Core.Security;
using static Keysharp.Core.SimpleJson;
using static Keysharp.Core.Sound;
using static Keysharp.Core.Strings;
using static Keysharp.Core.ToolTips;
using static Keysharp.Core.Window;
using static Keysharp.Core.Windows.WindowsAPI;
using static Keysharp.Scripting.Script;
using static Keysharp.Scripting.Script.Operator;

namespace Keysharp.Benchmark
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Data;
	using System.IO;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Windows.Forms;
	using BenchmarkDotNet.Analysers;
	using BenchmarkDotNet.Attributes;
	using BenchmarkDotNet.Configs;
	using BenchmarkDotNet.Exporters;
	using BenchmarkDotNet.Loggers;
	using BenchmarkDotNet.Running;
	using Keysharp.Core;
	using Keysharp.Scripting;
	using Array = Keysharp.Core.Array;
	using Buffer = Keysharp.Core.Buffer;

	[MemoryDiagnoser]
	public class IndexBench
	{
		private object[] nativearray = System.Array.Empty<object>();
		private double[] nativedoublearray = System.Array.Empty<double>();
		private Array keysharparray = Keysharp.Core.Misc.Array();
		private double totalSum;

		[Params(1000000)]
		public int Size { get; set; }

		[Benchmark(Baseline = true)]
		public void NativeObjectArray()
		{
			var total = 0.0;

			for (var i = 0; i < Size; i++)
				total += (double)nativearray[i];

			if (!total.IsAlmostEqual(totalSum))
				throw new Exception($"{total} was not equal to {totalSum}.");
		}

		[Benchmark]
		public void NativeDoubleArray()
		{
			var total = 0.0;

			for (var i = 0; i < Size; i++)
				total += nativedoublearray[i];

			if (!total.IsAlmostEqual(totalSum))
				throw new Exception($"{total} was not equal to {totalSum}.");
		}

		[Benchmark]
		public unsafe void NativeUnsafeArray()
		{
			var total = 0.0;

			fixed (double* ptr = nativedoublearray)
			{
				for (var i = 0; i < Size; i++)
					total += ptr[i];
			}

			if (!total.IsAlmostEqual(totalSum))
				throw new Exception($"{total} was not equal to {totalSum}.");
		}

		[Benchmark]
		public void KeysharpArray()
		{
			var total = 0.0;

			for (var i = 1; i <= Size; i++)
				total += (double)keysharparray[i];

			if (!total.IsAlmostEqual(totalSum))
				throw new Exception($"{total} was not equal to {totalSum}.");
		}

		[Benchmark]
		public void KeysharpArrayIndexMethod()
		{
			var total = 0.0;

			for (long i = 1; i <= Size; i++)
				total += (double)Index(keysharparray, i);

			if (!total.IsAlmostEqual(totalSum))
				throw new Exception($"{total} was not equal to {totalSum}.");
		}

		[Benchmark]
		public void KeysharpArrayIndexEnumerator()
		{
			var total = 0.0;
			var e0 = keysharparray;
			var e2 = MakeEnumerator(e0);
			_ = Push();

			for (
				; e2.MoveNext();
			)
			{
				_ = Inc();
				var (_, val) = e2.Current;
				total += (double)val;
				//e3:
				//;
			}

			//e4:
			_ = Pop();

			if (!total.IsAlmostEqual(totalSum))
				throw new Exception($"{total} was not equal to {totalSum}.");
		}

		[GlobalSetup]
		public void Setup()
		{
			Keysharp.Scripting.Script.Variables.InitGlobalVars();
			totalSum = 0;
			Size = 1000000;
			nativearray = new object[Size];
			nativedoublearray = new double[Size];
			keysharparray = new Array(Size);

			for (var i = 0; i < Size; i++)
			{
				var val = Maths.Random();
				var d = val.Ad();
				nativearray[i] = d;
				nativedoublearray[i] = d;
				_ = keysharparray.Add(d);
				totalSum += d;
			}
		}
	}
}
