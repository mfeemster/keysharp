using System.Collections.Generic;

namespace Keysharp.Core.Common
{
	internal class MsgMonitor
	{
		internal static int MAX_INSTANCES = Keysharp.Scripting.Script.maxThreadsLimit;
		internal int instanceCount;
		internal int maxInstances = 1;
		internal List<IFuncObj> funcs = new List<IFuncObj>();
	}
}