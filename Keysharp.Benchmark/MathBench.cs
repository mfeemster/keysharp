using static Keysharp.Core.Accessors;
using static Keysharp.Core.Core;
using static Keysharp.Core.Common.Keyboard.HotstringDefinition;
using static Keysharp.Core.Dialogs;
using static Keysharp.Core.Dir;
using static Keysharp.Core.Drive;
using static Keysharp.Core.DllHelper;
using static Keysharp.Core.Env;
using static Keysharp.Core.File;
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
	public class MathBench
	{
		private Dictionary<object, object> dkt;
		private Map map, mapScript;
		private double totalCos;
		private List<double> vals;
		private List<object> objvals;
		private FuncObj fo;

		[Params(100000)]
		public int Size { get; set; }

		[Benchmark(Baseline = true)]
		public void CosNative()
		{
			var total = 0.0;

			for (var i = 0; i < Size; i++)
				total += Math.Cos(vals[i]);

			if (!total.IsAlmostEqual(totalCos))
				throw new Exception($"{total} was not equal to {totalCos}.");
		}

		[Benchmark]
		public void KeysharpCos()
		{
			var total = 0.0;

			for (var i = 0; i < Size; i++)
				total += Maths.Cos(vals[i]);

			if (!total.IsAlmostEqual(totalCos))
				throw new Exception($"{total} was not equal to {totalCos}.");
		}

		[Benchmark]
		public void KeysharpCosObj()
		{
			var total = 0.0;

			for (var i = 0; i < Size; i++)
				total += Maths.Cos(objvals[i]);

			if (!total.IsAlmostEqual(totalCos))
				throw new Exception($"{total} was not equal to {totalCos}.");
		}

		[Benchmark]
		public void KeysharpCosFunc()
		{
			var total = 0.0;

			for (var i = 0; i < Size; i++)
				total += (double)fo.Call(vals[i]);

			if (!total.IsAlmostEqual(totalCos))
				throw new Exception($"{total} was not equal to {totalCos}.");
		}

		/*
		            [Benchmark]
		            public void KeysharpCosDirectPass()
		            {
		            var total = 0.0;

		            for (var i = 0; i < Size; i++)
		                total += Maths.CosDirectPass(vals[i]);

		            if (!total.IsAlmostEqual(totalCos))
		                throw new Exception($"{total} was not equal to {totalCos}.");
		            }

		            [Benchmark]
		            public void KeysharpCosDirectPassObject()
		            {
		            var total = 0.0;

		            for (var i = 0; i < Size; i++)
		                total += Maths.CosDirectPassObject(vals[i]);

		            if (!total.IsAlmostEqual(totalCos))
		                throw new Exception($"{total} was not equal to {totalCos}.");
		            }
		*/
		[GlobalSetup]
		public void Setup()
		{
			Keysharp.Scripting.Script.Variables.InitGlobalVars();
			totalCos = 0;
			vals = new List<double>(Size);
			objvals = new List<object>(Size);
			fo = Func("Cos");

			for (var i = 0; i < Size; i++)
			{
				var val = Maths.Random();
				var d = val.Ad();
				vals.Add(d);
				objvals.Add(val);
				totalCos += Math.Cos(d);
			}
		}
	}
}