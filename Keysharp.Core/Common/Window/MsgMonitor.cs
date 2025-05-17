namespace Keysharp.Core.Common.Window
{
	internal class MsgMonitor
	{
		internal int instanceCount;
		internal int maxInstances = 1;
		internal List<IFuncObj> funcs = [];
	}
}