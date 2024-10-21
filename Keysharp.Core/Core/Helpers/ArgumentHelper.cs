namespace Keysharp.Core
{
	internal class ArgumentHelper
	{
		internal const string cdeclstr = "cdecl";
		protected bool cdecl = false;
		protected HashSet<GCHandle> gcHandles = [];
		protected ScopeHelper gcHandlesScope;
		protected bool hasreturn = false;
		protected string returnName = "";
		protected Type returnType = typeof(int);
		internal bool CDecl => cdecl;
		internal string ReturnName => returnName;
		internal Type ReturnType => returnType;

		internal ArgumentHelper(object[] parameters)
		{
			gcHandlesScope = new ScopeHelper(gcHandles);
			gcHandlesScope.eh += (sender, o) =>
			{
				if (o is HashSet<GCHandle> hs)
					foreach (var gch in hs)
						gch.Free();
			};
			ConvertParameters(parameters);
		}

		protected virtual void ConvertParameters(object[] parameters)
		{
		}
	}
}