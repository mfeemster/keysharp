using System.Collections.Generic;
using Keysharp.Core.Common.Input;

namespace Keysharp.Core
{
	public static class Input
	{
		public static InputObject InputHook(object obj0 = null, object obj1 = null, object obj2 = null)
		{
			var options = obj0.As();
			var endKeys = obj1.As();
			var matchList = obj2.As();
			var io = new InputObject(options, endKeys, matchList);
			return io;
		}
	}
}