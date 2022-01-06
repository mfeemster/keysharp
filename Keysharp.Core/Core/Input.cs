using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Keysharp.Core.Common;
using Keysharp.Core.Common.Input;
using Keysharp.Core.Common.Threading;

namespace Keysharp.Core
{
	public static class Input
	{
		public static InputObject InputHook(params object[] obj)
		{
			var (options, endKeys, matchList) = obj.L().S3();
			var io = new InputObject(options, endKeys, matchList);
			//it.Setup(options, endKeys, matchList);
			//var io = new InputObject();
			//if (options.IndexOf('b', StringComparison.OrdinalIgnoreCase) != -1)
			//  io.BackspaceIsUndo = false;
			//if (options.IndexOf('c', StringComparison.OrdinalIgnoreCase) != -1)
			//  io.CaseSensitive = true;
			//if (options.IndexOf('m', StringComparison.OrdinalIgnoreCase) != -1)
			//  io.EndMods
			return io;
		}
	}
}
