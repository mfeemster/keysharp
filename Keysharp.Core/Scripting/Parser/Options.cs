namespace Keysharp.Scripting
{
	public partial class Parser
	{
		private const string Legacy = "LEGACY";

		private const bool LaxExpressions =
#if LEGACY
			true
#else
			false
#endif
			;

		private const bool LegacyIf = LaxExpressions;

		private const bool LegacyLoop = LaxExpressions;

		private bool DynamicVars = LaxExpressions;
	}
}