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
	using System.Drawing;
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
	public class FuncBench
	{
		private myclass cl;
		private long totalSum;

		[Params(1000000)]
		public static long Size { get; set; }

		public static object x { get; set; }

		public static object IncFunc()
		{
			x = Operate(Add, x, 1L);
			return "";
		}

		[Benchmark]
		public void KeysharpClassFuncLoopIncrement()
		{
			cl.x = 0;
			cl.ClassIncTestFunc();

			if ((long)cl.x != totalSum)
				throw new Exception($"{x} was not equal to {totalSum}.");
		}

		[Benchmark]
		public void KeysharpNativeObjectLoopIncrement()
		{
			x = 0L;

			for (System.Collections.IEnumerator e3 = Loop(Size).GetEnumerator(); IsTrueAndRunning(e3.MoveNext());
				)
			{
				x = (long)x + 1L;
				e4:
				;
			}

			e5:
			Pop();

			if ((long)x != totalSum)
				throw new Exception($"{x} was not equal to {totalSum}.");
		}

		[Benchmark]
		public void KeysharpNativeLongLoopIncrement()
		{
			var total = 0L;

			for (System.Collections.IEnumerator e3 = Loop(Size).GetEnumerator(); IsTrueAndRunning(e3.MoveNext());
				)
			{
				total++;
				e4:
				;
			}

			e5:
			Pop();

			if (total != totalSum)
				throw new Exception($"{x} was not equal to {totalSum}.");
		}

		[Benchmark]
		public void KeysharpFuncLoopIncrement()
		{
			x = 0L;

			for (System.Collections.IEnumerator e3 = Loop(Size).GetEnumerator(); IsTrueAndRunning(e3.MoveNext());
				)
			{
				IncFunc();
				e4:
				;
			}

			e5:
			Pop();

			if ((long)x != totalSum)
				throw new Exception($"{x} was not equal to {totalSum}.");
		}

		[Benchmark]
		public void KeysharpLoopIncrement()
		{
			x = 0L;

			for (System.Collections.IEnumerator e0 = Loop(Size).GetEnumerator(); IsTrueAndRunning(e0.MoveNext());
				)
			{
				x = Operate(Add, x, 1L);
				e1:
				;
			}

			e2:
			Pop();

			if ((long)x != totalSum)
				throw new Exception($"{x} was not equal to {totalSum}.");
		}

		[Benchmark(Baseline = true)]
		public void NativeLoopIncrement()
		{
			var total = 0L;

			for (var i = 0; i < Size; i++)
				total++;

			if (total != totalSum)
				throw new Exception($"{total} was not equal to {totalSum}.");
		}

		[GlobalSetup]
		public void Setup()
		{
			Keysharp.Scripting.Script.Variables.InitGlobalVars();
			Size = 1000000;
			totalSum = Size;
			cl = new myclass();
		}
	}

	[MemoryDiagnoser]
	public class myclass : KeysharpObject
	{
		public static string @__Class
		{
			get
			{
				return "myclass";
			}
		}

		public object x { get; set; }

		public myclass()
		{
			__Init();
		}

		public static myclass Call()
		{
			return new myclass();
		}

		public object ClassInc()
		{
			x = Operate(Add, x, 1L);
			return "";
		}

		public object ClassIncTestFunc()
		{
			x = 0L;

			for (System.Collections.IEnumerator e8 = Loop(FuncBench.Size).GetEnumerator(); IsTrueAndRunning(e8.MoveNext());
				)
			{
				Invoke(GetMethodOrProperty(this, "ClassInc", 0));
				e9:
				;
			}

			e10:
			Pop();
			return "";
		}

		private void @__Init()
		{
			x = 0L;
		}
	}
}