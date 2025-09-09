﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Keysharp.Scripting;
using static Keysharp.Scripting.Script;

namespace Keysharp.Benchmark
{
	public class ParserBench : BaseTest
	{
		private Keysharp.Scripting.Script _ks_s;
		private CompilerHelper _ch;
		private string _filePath;
		private string _outputDir;

		[Params(1000)]
		public int Size { get; set; }

		[Benchmark]
		public void CreateTreeFromFile()
		{
			var ch = new CompilerHelper();
			var (st, errs) = ch.CreateCompilationUnitFromFile("./Keysharp.ks");
		}

		[GlobalSetup]
		public void Setup()
		{
			_ks_s = new();
			_ks_s.Vars.InitClasses();
		}
	}
}
