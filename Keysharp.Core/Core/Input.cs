using System.Collections.Generic;
using Keysharp.Core.Common.Input;

namespace Keysharp.Core
{
	public static class Input
	{
		public static InputObject InputHook(object obj0 = null, object obj1 = null, object obj2 = null)//Need to make this work.//TODO
		{
			var options = obj0.As();
			var endKeys = obj1.As();
			var matchList = obj2.As();
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