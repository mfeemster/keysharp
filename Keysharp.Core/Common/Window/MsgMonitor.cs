namespace Keysharp.Core.Common.Window
{
	internal class MsgMonitor
	{
		internal static int MAX_INSTANCES = Script.maxThreadsLimit;
		internal int instanceCount;
		internal int maxInstances = 1;
		internal List<IFuncObj> funcs = [];
	}
}