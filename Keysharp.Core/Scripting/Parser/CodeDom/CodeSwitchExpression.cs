namespace Keysharp.Scripting
{
	internal class CodeSwitchStatement : CodeStatement
	{
		internal CodeStatementCollection AllStatements { get; } = new CodeStatementCollection();
		internal Dictionary<string, CodeStatementCollection> CaseBodyStatements { get; } = new Dictionary<string, CodeStatementCollection>();
		internal Dictionary<string, object> CaseExpressions { get; } = new Dictionary<string, object>();
		internal StringComparison? CaseSense { get; } = null;
		internal CodeConditionStatement Condition { get; } = new CodeConditionStatement();
		internal CodeLabeledStatement DefaultLabelStatement { get; } = new CodeLabeledStatement();
		internal CodeStatementCollection DefaultStatements { get; } = new CodeStatementCollection();
		internal CodeLabeledStatement FinalLabelStatement { get; } = new CodeLabeledStatement();
		internal string SwitchVar { get; }

		internal string SwitchVarTempName { get; }

		internal CodeSwitchStatement(string switchVar, object caseSense, uint switchCount)
		{
			SwitchVar = switchVar;

			if (caseSense != null)
			{
				CaseSense = Keysharp.Core.Conversions.ParseComparisonOption(caseSense);

				if (CaseSense == System.StringComparison.CurrentCulture)
					CaseSense = System.StringComparison.CurrentCultureIgnoreCase;
			}

			SwitchVarTempName = $"ks_switchvar{switchCount}";
			FinalLabelStatement = new CodeLabeledStatement($"ks_finallabel{switchCount}");
			DefaultLabelStatement = new CodeLabeledStatement($"ks_defaultlabel{switchCount}");
		}
	}
}